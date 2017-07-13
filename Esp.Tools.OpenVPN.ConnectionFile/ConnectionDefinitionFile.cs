//  This file is part of OpenVPN UI.
//  Copyright 2011 ESP Technologies Ltd.
//
//  Author: James Martin - October 2011
//
//
//  OpenVPN UI is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  OpenVPN UI is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with OpenVPN UI.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Esp.Tools.OpenVPN.EventLog;
using ICSharpCode.SharpZipLib.Zip;

namespace Esp.Tools.OpenVPN.ConnectionFile
{
    [DataContract]
    public class ConnectionDefinitionFile
    {
        private static readonly DataContractSerializer Serializer =
            new DataContractSerializer(typeof(ConnectionDefinitionFile));

        private List<KeyValuePair<string, string>> _configLines = new List<KeyValuePair<string, string>>();
        private string _fileName;

        public ConnectionDefinitionFile(ConnectionDefinitionFileLocks pLocks)
        {
            FileConfig = new ConnectionDefinitionFileConfig(pLocks);
        }

        public ConnectionDefinitionFile() : this(null)
        {
        }

        [DataMember]
        public ConnectionDefinitionFileConfig FileConfig { get; private set; }

        [DataMember]
        public string ConnectionName { get; set; }

        [DataMember]
        public string CertificateThumbPrint
        {
            get { return Certificate != null ? Certificate.Thumbprint : null; }
            set
            {
                if (value != null)
                {
                    var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                    store.Open(OpenFlags.ReadOnly);
                    try
                    {
                        foreach (var cert in store.Certificates.Find(X509FindType.FindByThumbprint, value, false))
                            Certificate = cert;
                    }
                    finally
                    {
                        store.Close();
                    }
                }
                else
                {
                    Certificate = null;
                }
            }
        }

        [DataMember]
        public string ConfigurationData
        {
            get => BuildConfigString(_configLines);
            set
            {
                var lines = value.Split('\n', '\r')
                    .Select(pX => pX.Trim('\n', '\r', '\t', ' '))
                    .Where(pX => !string.IsNullOrEmpty(pX));
                _configLines = new List<KeyValuePair<string, string>>();
                foreach (var line in lines)
                {
                    var pos = line.IndexOf(' ');
                    var key = "";
                    var val = "";
                    if (pos == -1)
                    {
                        key = line;
                        val = "";
                    }
                    else
                    {
                        key = line.Substring(0, pos);
                        val = line.Substring(pos + 1, line.Length - pos - 1);
                    }


                    _configLines.Add(new KeyValuePair<string, string>(key.Trim(), val));
                }
            }
        }

        [DataMember]
        public byte[] AuthorityCertData { get; set; }

        public string AuthorityCertText
        {
            get
            {
                if (AuthorityCertData == null) return null;
                return Encoding.ASCII.GetString(AuthorityCertData);
            }
        }

        public X509Certificate2 AuthorityCert
        {
            get
            {
                if (AuthorityCertData == null) return null;
                return new X509Certificate2(AuthorityCertData);
            }
        }

        public X509Certificate2 Certificate { get; set; }

        public string ComposedConfiguration
        {
            get
            {
                var ignoreKeys = new List<string> {"ca", "cert", "key"};
                var ignoredConfig = _configLines.Where(pX => !ignoreKeys.Contains(pX.Key));
                var sb = new StringBuilder(BuildConfigString(ignoredConfig));
                if (!string.IsNullOrEmpty(AuthorityCertText))
                {
                    sb.AppendLine("ca [inline]");
                    sb.AppendLine("<ca>");
                    sb.AppendLine(AuthorityCertText);
                    sb.AppendLine("</ca>");
                }

                if (Certificate != null)
                {
                    sb.AppendFormat("cryptoapicert \"THUMB:{0}\"", CertificateThumbPrint);
                    sb.AppendLine();
                }

                return sb.ToString();
            }
        }


        private string BuildConfigString(IEnumerable<KeyValuePair<string, string>> pConfigLines)
        {
            var sb = new StringBuilder();
            foreach (var line in pConfigLines)
            {
                sb.AppendFormat("{0} {1}", line.Key, line.Value);
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public bool HasOption(string pOption)
        {
            return _configLines.Exists(pX => pX.Key == pOption);
        }

        public static ConnectionDefinitionFile LoadFromStream(Stream pStream)
        {
            using (var zip = new ZipFile(pStream))
            {
                var entry = zip.GetEntry("config.xml");
                using (var stream = zip.GetInputStream(entry))
                {
                    return Serializer.ReadObject(stream) as ConnectionDefinitionFile;
                }
            }
        }

        public void Save(Stream pStream)
        {
            using (var zip = new ZipOutputStream(pStream))
            {
                zip.PutNextEntry(new ZipEntry("config.xml"));
                Serializer.WriteObject(zip, this);
                zip.Flush();
            }
        }

        public void SaveFile(string pFileName)
        {
            var filename = pFileName + ".tmp";
            if (File.Exists(filename))
                File.Delete(filename);

            using (var file = File.Create(filename))
            {
                try
                {
                    Save(file);
                    // file.Flush();
                }
                finally
                {
                    //  file.Close();
                }
            }
            if (File.Exists(pFileName))
            {
                var i = 0;
                for (; File.Exists(pFileName + "." + i); i++)
                {
                }
                File.Move(pFileName, pFileName + "." + i);
                File.Move(filename, pFileName);
            }
            else
            {
                File.Move(filename, pFileName);
            }
            _fileName = pFileName;
        }

        public static ConnectionDefinitionFile LoadFile(string pFile)
        {
            using (var file = File.OpenRead(pFile))
            {
                try
                {
                    var result = LoadFromStream(file);
                    result._fileName = pFile;
                    return result;
                }
                finally
                {
                    file.Close();
                }
            }
        }

        public void Save()
        {
            if (string.IsNullOrEmpty(_fileName))
                throw new InvalidOperationException(
                    "Attempted to save a configuration that hasn't been loaded directly from disk");
            try
            {
                SaveFile(_fileName);
            }
            catch (Exception ex)
            {
                EventLogHelper.LogEvent("Error Saving File:" + _fileName + "\n\r" + ex.Message + "\n\r" +
                                        ex.StackTrace);
            }
        }

        public void Delete()
        {
            if (string.IsNullOrEmpty(_fileName))
                throw new InvalidOperationException(
                    "Attempted to delete a configuration that hasn't been loaded directly from disk");
            try
            {
                File.Delete(_fileName);
            }
            catch (Exception ex)
            {
                EventLogHelper.LogEvent("Error Deleting File:" + _fileName + "\n\r" + ex.Message);
            }
        }
    }
}