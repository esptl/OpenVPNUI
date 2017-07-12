using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Esp.Tools.OpenVPN.ConnectionFile;

namespace MakeOpenVPNConfig
{
    class Program
    {
        static void Main(string[] pArgs)
        {
            if (pArgs.Length < 4)
            {
                Console.WriteLine("SYNTAX: MakeOpenVPNConfig [ovpn file] [ca cert] [display name] [output file]");
                Console.WriteLine();
                Console.WriteLine("For example:-");
                Console.WriteLine(@"MakeOpenVPNConfig c:\test\myvpn.ovpn c:\test\ca.crt ""My VPN"" myvpn.openvpn");
                return;
            }
            var configFileName = pArgs[0];
            var caFileName = pArgs[1];
            var displayName = pArgs[2];
            var outputName = pArgs[3];

            if (!File.Exists(configFileName))
                Console.WriteLine("ERROR: Configuration file does not exist");
            if (!File.Exists(caFileName))
                Console.WriteLine("ERROR: CA cert file does not exist");
            if (!File.Exists(configFileName) || !File.Exists(caFileName))
                return;

            string configXml;
            byte[] caBytes;
            try
            {
                configXml = File.ReadAllText(configFileName);
            }
            catch (IOException)
            {
                Console.WriteLine("ERROR: Reading Config File");
                return;
            }

            try
            {
                caBytes = File.ReadAllBytes(caFileName);
            }
            catch (IOException)
            {
                Console.WriteLine("ERROR: Reading CA Cert File");
                return;
            }

            var file = new ConnectionDefinitionFile();
            file.AuthorityCertData = caBytes;
            file.ConfigurationData = configXml;
            file.ConnectionName = displayName;
            try
            {
                file.SaveFile(outputName);
            }
            catch (IOException)
            {
                Console.WriteLine("ERROR: Error writing output file.");
                return;
            }
        }
    }
}
