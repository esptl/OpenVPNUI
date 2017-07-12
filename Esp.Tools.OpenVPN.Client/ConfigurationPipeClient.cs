#region

using System;
using System.Collections.Generic;
using System.IO;
using Esp.Tools.OpenVPN.Certificates;
using Esp.Tools.OpenVPN.ConnectionFile;
using Esp.Tools.OpenVPN.IPCProtocol;
using Esp.Tools.OpenVPN.IPCProtocol.Configuration.Commands;
using Esp.Tools.OpenVPN.IPCProtocol.Configuration.Commands.Contracts;
using Esp.Tools.OpenVPN.IPCProtocol.Configuration.Messages;
using Esp.Tools.OpenVPN.IPCProtocol.Configuration.Messages.Contracts;
using Esp.Tools.OpenVPN.IPCProtocol.Contracts;

#endregion

namespace Esp.Tools.OpenVPN.Client
{
    public class ConfigurationPipeClient : BasePipeClient
    {
        private CertificateDetails[] _certificates;
        private ConfigurationInfo[] _configurations;

        public ConfigurationPipeClient() : base(Configuration.Configuration.Current.ConfigPipe)
        {
        }

        protected override IEnumerable<IMessageReader> MessageReaders
        {
            get
            {
                return new IMessageReader[]
                           {
                               new MessageReader<CertificatesInfo>(AvailableCertificatesMessage.MessageKey)
                                   {MessageRecieved = OnAvailableCertificates},
                               new MessageReader<ConfigurationsInfo>(ConfigurationsMessage.MessageKey)
                                   {MessageRecieved = OnConfigurations},
                               new MessageReader<EnrollRequestResponseInfo>(EnrollRequestResponseMessage.MessageKey)
                                   {MessageRecieved = OnEnrollRequestResponse},
                               new MessageReader<EnrollResponseInfo>(EnrollResponseMessage.MessageKey)
                                   {MessageRecieved = OnEnrollResponse}
                           };
            }
        }

        public ConfigurationInfo[] Configurations
        {
            get { return _configurations; }
            set
            {
                _configurations = value;
                if (ConfigurationsChanged != null)
                    ConfigurationsChanged();
            }
        }

        public CertificateDetails[] Certificates
        {
            get { return _certificates; }
            set
            {
                _certificates = value;
                if (CertificatesChanged != null)
                    CertificatesChanged();
            }
        }

        public bool IsConnected
        {
            get { return _pipe.IsConnected; }
        }

        private void OnEnrollResponse(BaseMessage<EnrollResponseInfo> pMessage)
        {
            if (Enrolled != null)
                Enrolled(pMessage.Data);
        }

        private void OnEnrollRequestResponse(BaseMessage<EnrollRequestResponseInfo> pMessage)
        {
            if (EnrollRequestResponse != null)
                EnrollRequestResponse(pMessage.Data);
        }

        private void OnConfigurations(BaseMessage<ConfigurationsInfo> pMessage)
        {
            Configurations = pMessage.Data.Configurations;
        }

        private void OnAvailableCertificates(BaseMessage<CertificatesInfo> pMessage)
        {
            Certificates = pMessage.Data.Certificates;
        }

        public event Action ConfigurationsChanged;
        public event Action CertificatesChanged;
        public event Action<EnrollRequestResponseInfo> EnrollRequestResponse;
        public event Action<EnrollResponseInfo> Enrolled;


        public void SendRefreshCertificatesCommand()
        {
            SendCommand(new GetCertificatesCommand {Data = new GetCertificatesInfo()});
        }

        public void SendDeleteCertificateCommand(string pThumbPrint)
        {
            SendCommand(new DeleteCertificateCommand {Data = new DeleteCertificateInfo {ThumbPrint = pThumbPrint}});
        }

        public void SendEnrollCommand(string pCertificate)
        {
            SendCommand(new EnrollCommand {Data = new EnrollInfo {Certificate = pCertificate}});
        }

        public void SendRequestEnrollCommand(EnrollRequestDetails pDetails)
        {
            SendCommand(new EnrollRequestCommand {Data = new EnrollRequestInfo {Request = pDetails}});
        }

        public void SendDeleteConfigurationCommand(string pConfigurationName)
        {
            SendCommand(new DeleteConfigurationCommand {Data = new DeleteConfigurationInfo {Name = pConfigurationName}});
        }

        public void SendInstallConfigurationCommand(ConnectionDefinitionFile pConnectionFile)
        {
            var ms = new MemoryStream();
            pConnectionFile.Save(ms);
            SendCommand(new InstallConfigurationCommand
                            {Data = new InstallConfigurationInfo {ConfigurationData = ms.ToArray()}});
        }

        public void SendSetConfigurationCertificateCommand(string pConfigurationName, CertificateDetails pCertificate)
        {
            SendCommand(new SetConfigurationCertificateCommand
                            {
                                Data =
                                    new SetConfigurationCertificateInfo
                                        {ThumbPrint = pCertificate.ThumbPrint, Name = pConfigurationName}
                            });
        }

        public void SendImportPfxCommand(byte[] pPfxData, string pPassword)
        {
            SendCommand(new ImportPfxCommand {Data = new ImportPfxInfo {PfxData = pPfxData, Password = pPassword}});
        }
    }
}