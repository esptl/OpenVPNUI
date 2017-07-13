#region

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using CERTENROLLLib;
using Esp.Tools.OpenVPN.EventLog;
using X509KeyUsageFlags = CERTENROLLLib.X509KeyUsageFlags;

#endregion

namespace Esp.Tools.OpenVPN.Certificates
{
    public class CertificateManager
    {
        static CertificateManager()
        {
            Current = new CertificateManager();
        }

        private CertificateManager()
        {
        }

        public static CertificateManager Current { get; }

        public void DeleteCertificate(string pThumbPrint)
        {
            var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadWrite);
            try
            {
                var cert = store.Certificates.Find(X509FindType.FindByThumbprint, pThumbPrint, false);


                foreach (var ce in cert)
                    store.Remove(ce);
            }
            finally
            {
                store.Close();
            }
        }

        public byte[] CreatePfx(string pCertFile, string pKeyFile)
        {
            var workingPath = Configuration.Configuration.Current.WorkingPath + "openvpn\\bin\\";
            var process = new Process
            {
                StartInfo =
                {
                    WorkingDirectory = workingPath,
                    FileName = workingPath + "openssl.exe",
                    Arguments =
                        $@"pkcs12 -in ""{pCertFile}"" -inkey ""{pKeyFile}"" -export -password pass:password",
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    ErrorDialog = false
                }
            };
            process.Start();
            var ms = new MemoryStream();
            var strm = process.StandardOutput.BaseStream;
            while (strm.CanRead)
            {
                var buffer = new byte[4096];
                var read = strm.Read(buffer, 0, buffer.Length);
                if (read > 0)
                    ms.Write(buffer, 0, read);
                else
                    break;
            }

            var bytes = ms.ToArray();
            var cert = new X509Certificate2(bytes, "password");
            return bytes;
        }

        public CertificateDetails[] GetCertificates(X509Certificate2 pAuthority)
        {
            var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);

            store.Open(OpenFlags.ReadWrite);
            try
            {
                if (pAuthority != null)
                {
                    var authCert =
                        store.Certificates.Find(X509FindType.FindByThumbprint, pAuthority.Thumbprint, false).Count > 0;

                    if (!authCert)
                        store.Add(pAuthority);
                }
                var certs = store.Certificates.OfType<X509Certificate2>().Where(pX => pX.HasPrivateKey);
                if (pAuthority != null)
                    certs = certs.Where(pX =>
                    {
                        var chain = new X509Chain();
                        chain.Build(pX);
                        return
                            chain.ChainElements.Cast<X509ChainElement>()
                                .ToList()
                                .Exists(
                                    pY => pY.Certificate.Thumbprint == pAuthority.Thumbprint);
                    });
                return
                    certs.Select(
                            pX =>
                            {
                                var chain = new X509Chain();
                                chain.Build(pX);
                                return new CertificateDetails
                                {
                                    CommonName = pX.GetNameInfo(X509NameType.SimpleName, false),
                                    IssuerName = pX.GetNameInfo(X509NameType.SimpleName, true),
                                    ThumbPrint = pX.Thumbprint,
                                    ValidFrom = pX.NotBefore,
                                    ValidTo = pX.NotAfter,
                                    ChainThumbPrints =
                                        chain.ChainElements.Cast<X509ChainElement>()
                                            .Select(
                                                pY => pY.Certificate.Thumbprint)
                                            .ToArray()
                                };
                            })
                        .ToArray()
                    ;
            }
            finally
            {
                store.Close();
            }
        }

        public X509Certificate2 ImportResponse(string pResponse)
        {
            CX509Enrollment objEnroll = new CX509EnrollmentClass();

            try
            {
                var strCert = pResponse;

                // Install the certificate
                objEnroll.Initialize(X509CertificateEnrollmentContext.ContextUser);
                objEnroll.InstallResponse(
                    InstallResponseRestrictionFlags.AllowUntrustedRoot,
                    strCert,
                    EncodingType.XCN_CRYPT_STRING_BASE64HEADER,
                    null
                );
                var x509Cert = new X509Certificate2(Encoding.ASCII.GetBytes(pResponse));
                return x509Cert;
            }
            catch (Exception ex)
            {
                EventLogHelper.LogEvent(ex.Message + "\n\r" + ex.StackTrace);
                return null;
            }
        }

        public string GenerateRequest(EnrollRequestDetails pDetails)
        {
            var objPkcs10 =
                Activator.CreateInstance(Type.GetTypeFromProgID("X509Enrollment.CX509CertificateRequestPkcs10")) as
                    CX509CertificateRequestPkcs10;
            var objPrivateKey =
                Activator.CreateInstance(Type.GetTypeFromProgID("X509Enrollment.CX509PrivateKey")) as CX509PrivateKey;
            var objCSP = new CCspInformationClass();
            var objCSPs = new CCspInformationsClass();
            var objDN = new CX500DistinguishedNameClass();
            var objEnroll =
                Activator.CreateInstance(Type.GetTypeFromProgID("X509Enrollment.CX509Enrollment")) as CX509Enrollment;
            var objObjectIds = new CObjectIdsClass();
            var objObjectId = new CObjectIdClass();
            var objExtensionKeyUsage =
                Activator.CreateInstance(
                    Type.GetTypeFromProgID("X509Enrollment.CX509ExtensionKeyUsage")) as CX509ExtensionKeyUsage;

            var objX509ExtensionEnhancedKeyUsage =
                Activator.CreateInstance(
                        Type.GetTypeFromProgID("X509Enrollment.CX509ExtensionEnhancedKeyUsage")) as
                    CX509ExtensionEnhancedKeyUsage;


            try
            {
                //  Initialize the csp object using the desired Cryptograhic Service Provider (CSP)
                objCSP.InitializeFromName("Microsoft Enhanced Cryptographic Provider v1.0");

                //  Add this CSP object to the CSP collection object
                objCSPs.Add(
                    objCSP
                );

                //  Provide key container name, key length and key spec to the private key object
                //objPrivateKey.ContainerName = "AlejaCMa";
                objPrivateKey.Length = 4096;
                objPrivateKey.KeySpec = X509KeySpec.XCN_AT_SIGNATURE;
                objPrivateKey.ExportPolicy = X509PrivateKeyExportFlags.XCN_NCRYPT_ALLOW_EXPORT_FLAG;
                objPrivateKey.KeyUsage = X509PrivateKeyUsageFlags.XCN_NCRYPT_ALLOW_ALL_USAGES;
                objPrivateKey.MachineContext = false;

                //  Provide the CSP collection object (in this case containing only 1 CSP object)
                //  to the private key object
                objPrivateKey.CspInformations = objCSPs;

                //  Create the actual key pair
                objPrivateKey.Create();

                //  Initialize the PKCS#10 certificate request object based on the private key.
                //  Using the context, indicate that this is a user certificate request and don't
                //  provide a template name
                objPkcs10.InitializeFromPrivateKey(
                    X509CertificateEnrollmentContext.ContextUser,
                    objPrivateKey,
                    ""
                );

                // Key Usage Extension 
                objExtensionKeyUsage.InitializeEncode(
                    X509KeyUsageFlags.XCN_CERT_DIGITAL_SIGNATURE_KEY_USAGE |
                    X509KeyUsageFlags.XCN_CERT_NON_REPUDIATION_KEY_USAGE |
                    X509KeyUsageFlags.XCN_CERT_KEY_ENCIPHERMENT_KEY_USAGE |
                    X509KeyUsageFlags.XCN_CERT_DATA_ENCIPHERMENT_KEY_USAGE
                );
                objPkcs10.X509Extensions.Add((CX509Extension) objExtensionKeyUsage);

                // Enhanced Key Usage Extension
                objObjectId.InitializeFromValue("1.3.6.1.5.5.7.3.2"); // OID for Client Authentication usage
                objObjectIds.Add(objObjectId);
                objX509ExtensionEnhancedKeyUsage.InitializeEncode(objObjectIds);
                objPkcs10.X509Extensions.Add((CX509Extension) objX509ExtensionEnhancedKeyUsage);

                //  Encode the name in using the Distinguished Name object
                objDN.Encode(
                    string.Format("C={0}, ST={1}, L={2}, O={3}, CN={4}, email={5},OU={6}",
                        pDetails.Country,
                        pDetails.County,
                        pDetails.City,
                        pDetails.CompanyName,
                        pDetails.CommonName,
                        pDetails.EmailAddress,
                        pDetails.Department),
                    X500NameFlags.XCN_CERT_X500_NAME_STR
                );

                //  Assing the subject name by using the Distinguished Name object initialized above
                objPkcs10.Subject = objDN;

                // Create enrollment request
                objEnroll.InitializeFromRequest(objPkcs10);
                var strRequest = objEnroll.CreateRequest(
                    EncodingType.XCN_CRYPT_STRING_BASE64
                );
                var sb = new StringBuilder("-----BEGIN NEW CERTIFICATE REQUEST-----");
                sb.AppendLine();
                sb.Append(strRequest);
                sb.AppendLine("-----END NEW CERTIFICATE REQUEST-----");
                return sb.ToString();
            }
            catch (Exception ex)
            {
                EventLogHelper.LogEvent(ex.Message + "\n\r" + ex.StackTrace);
                return null;
            }
        }

        public void ImportPfx(byte[] pPfxData, string pPassword)
        {
            var cert = new X509Certificate2(pPfxData, pPassword,
                X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);
            var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadWrite);
            store.Add(cert);
            store.Close();
        }

        public void InstallCertificate(X509Certificate2 pCertificate)
        {
            var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadWrite);
            if (store.Certificates.Find(X509FindType.FindByThumbprint, pCertificate.Thumbprint, false).Count == 0)
                store.Add(pCertificate);
            store.Close();
        }
    }
}