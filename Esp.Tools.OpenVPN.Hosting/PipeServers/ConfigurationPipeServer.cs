using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Threading.Tasks;
using Esp.Tools.OpenVPN.Certificates;
using Esp.Tools.OpenVPN.ConnectionFile;
using Esp.Tools.OpenVPN.EventLog;
using Esp.Tools.OpenVPN.Hosting.Config;
using Esp.Tools.OpenVPN.IPCProtocol;
using Esp.Tools.OpenVPN.IPCProtocol.Configuration.Commands;
using Esp.Tools.OpenVPN.IPCProtocol.Configuration.Commands.Contracts;
using Esp.Tools.OpenVPN.IPCProtocol.Configuration.Messages;
using Esp.Tools.OpenVPN.IPCProtocol.Configuration.Messages.Contracts;

namespace Esp.Tools.OpenVPN.Hosting.PipeServers
{
    public class ConfigurationPipeServer : BasePipeServer
    {
        private readonly OpenVPNConfigurations _configurations;

        public ConfigurationPipeServer(OpenVPNConfigurations pConfigurations)
            : base(Configuration.Configuration.Current.ConfigPipe, 1)
        {
            _configurations = pConfigurations;
        }

        protected override IEnumerable<PipeAccessRule> PipeAccessRules => new[]
        {
            new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null),
                PipeAccessRights.ReadWrite |
                PipeAccessRights.CreateNewInstance, AccessControlType.Allow),
            new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.CreatorOwnerSid, null),
                PipeAccessRights.FullControl,
                AccessControlType.Allow)
        };

        protected override IEnumerable<IMessageReader> MessageReaders => new IMessageReader[]
        {
            new MessageReader<EnrollRequestInfo>(EnrollRequestCommand.MessageKey)
                {MessageRecieved = OnEnrollRequest},
            new MessageReader<EnrollInfo>(EnrollCommand.MessageKey)
                {MessageRecieved = OnEnroll},
            new MessageReader<ImportPfxInfo>(ImportPfxCommand.MessageKey)
                {MessageRecieved = OnImportPfx},
            new MessageReader<GetCertificatesInfo>(GetCertificatesCommand.MessageKey)
                {MessageRecieved = OnGetCertificates},
            new MessageReader<InstallConfigurationInfo>(InstallConfigurationCommand.MessageKey)
                {MessageRecieved = OnInstallConfiguration},
            new MessageReader<DeleteConfigurationInfo>(DeleteConfigurationCommand.MessageKey)
                {MessageRecieved = OnDeleteConfiguration},
            new MessageReader<SetConfigurationCertificateInfo>(SetConfigurationCertificateCommand.MessageKey)
                {MessageRecieved = OnSetConfigurationCertificate},
            new MessageReader<DeleteCertificateInfo>(DeleteCertificateCommand.MessageKey)
                {MessageRecieved = OnDeleteCertificate}
        };

        private async Task OnImportPfx(BaseMessage<ImportPfxInfo> pRequest)
        {
            CertificateManager.Current.ImportPfx(pRequest.Data.PfxData, pRequest.Data.Password);
            await SendCertificates();
            await SendConfigurations();
        }

        private async Task OnDeleteCertificate(BaseMessage<DeleteCertificateInfo> pRequest)
        {
            CertificateManager.Current.DeleteCertificate(pRequest.Data.ThumbPrint);
            await SendCertificates();
        }

        private async Task OnSetConfigurationCertificate(BaseMessage<SetConfigurationCertificateInfo> pRequest)
        {
            var con = _configurations.FirstOrDefault(pX => pX.ConfigurationName == pRequest.Data.Name);
            if (con != null)
            {
                con.SetCertificate(pRequest);
                await SendConfigurations();
            }
            else
            {
                EventLogHelper.LogEvent("Error setting configuration cert as it does not exist: " + pRequest.Data.Name);
            }
        }

        private async Task OnDeleteConfiguration(BaseMessage<DeleteConfigurationInfo> pRequest)
        {
            var con = _configurations.FirstOrDefault(pX => pX.ConfigurationName == pRequest.Data.Name);
            if (con != null)
            {
                con.Delete();
                await SendConfigurations();
            }
            else
            {
                EventLogHelper.LogEvent("Error deleting configuration as it does not exist: " + pRequest.Data.Name);
            }
        }

        private async Task OnInstallConfiguration(BaseMessage<InstallConfigurationInfo> pRequest)
        {
            var ms = new MemoryStream(pRequest.Data.ConfigurationData);
            var configFile = ConnectionDefinitionFile.LoadFromStream(ms);
            var con = _configurations.FirstOrDefault(pX => pX.ConfigurationName == configFile.ConnectionName);
            if (con != null)
                con.Delete();
            _configurations.InstallConfiguration(configFile);

            await SendCertificates();
            await SendConfigurations();
        }

        private async Task OnEnroll(BaseMessage<EnrollInfo> pRequest)
        {
            var certificate = CertificateManager.Current.ImportResponse(pRequest.Data.Certificate);
            if (certificate != null)
                await SendEnrollResponse(certificate);
        }


        private async Task OnGetCertificates(BaseMessage<GetCertificatesInfo> pRequest)
        {
            await SendCertificates();
            await SendConfigurations();
        }

        private async Task OnEnrollRequest(BaseMessage<EnrollRequestInfo> pRequest)
        {
            var response = CertificateManager.Current.GenerateRequest(pRequest.Data.Request);
            if (response != null)
                await SendEnrollRequestResponse(response);
        }

        protected override async Task OnConnection()
        {
            await SendCertificates();
            await SendConfigurations();
        }

        private async Task SendCertificates()
        {
            var certificates =
                new CertificatesInfo
                {
                    Certificates = CertificateManager.Current.GetCertificates(null)
                };
            await SendMessageAsync(new AvailableCertificatesMessage(0) {Data = certificates});
        }

        private async Task SendEnrollResponse(X509Certificate2 pCertificate)
        {
            await SendCertificates();
            await SendConfigurations();
            await SendMessageAsync(new EnrollResponseMessage(0)
            {
                Data = new EnrollResponseInfo {ThumbPrint = pCertificate.Thumbprint}
            });
        }

        private async Task SendEnrollRequestResponse(string pResponse)
        {
            await SendMessageAsync(
                new EnrollRequestResponseMessage(0) {Data = new EnrollRequestResponseInfo {Request = pResponse}});
        }

        private async Task SendConfigurations()
        {
            var configurations = _configurations.Select(pX => pX.ConfigurationInfo).ToArray();
            await SendMessageAsync(new ConfigurationsMessage {Data = new ConfigurationsInfo {Configurations = configurations}});
        }
    }
}