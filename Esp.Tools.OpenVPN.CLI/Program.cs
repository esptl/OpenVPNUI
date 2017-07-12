//  This file is part of OpenVPN UI.
//  Copyright 2011 ESP Technologies Ltd.
//
//  Author: James Martin - September 2011
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
using System.IO;
using Esp.Tools.OpenVPN.Client;
using Esp.Tools.OpenVPN.ConnectionFile;
using Esp.Tools.OpenVPN.IPCProtocol.Contracts;

namespace Esp.Tools.OpenVPN.CLI
{
    class Program
    {
        static void Main(string[] pArgs)
        {          
            if(pArgs.Length==0 || pArgs[0] == "--help" || pArgs[0]=="-h")
            {
                PrintHelp();
                return;
            }

            switch (pArgs[0])
            {
                case "--makefile":
                    var output = pArgs[1];
                    var config = pArgs[2];
                    var ca = pArgs[3];
                    var name = pArgs[4];
                    var connectionFile = new ConnectionDefinitionFile
                                             {
                                                 ConfigurationData = File.ReadAllText(config),
                                                 AuthorityCertData = File.ReadAllBytes(ca),
                                                 ConnectionName = name
                                             };
                    connectionFile.SaveFile(output+".connection");

                    return;
            }

            ControllerPipeClient pipeClient;
            try
            {
                pipeClient = new ControllerPipeClient();
            } catch(Exception exception)
            {
                Console.WriteLine("An error has occured connecting to the OpenVPN UI Host service.");
                Console.WriteLine("Exception: "+exception.Message);
                return;
            }

            pipeClient.Initialized +=
                pMessage =>
                    {
                        switch (pArgs[0])
                        {                            
                            case "--list":
                                ListConnections(pipeClient);
                                return;
                            //case "--start":
                            //    var con = 0;
                            //    if(pArgs.Length!=2 || !Int32.TryParse(pArgs[1],out con))
                            //    {
                            //        Console.WriteLine("SYNTAX: --start {connection number}");
                            //        return;
                            //    }

                            //    if(con < pipeClient.ConnectionCount)
                            //    {
                            //        if(pipeClient[con].ConnectionStatus==ConnectionStatus.Disconnected)
                            //        {
                                        
                            //        } else
                            //        {
                            //            Console.WriteLine("{0} is already {1}.", pipeClient[con].Name, pipeClient[i].ConnectionStatus.ToString().ToLower());
                            //        }
                            //    }
                            //    else
                            //        Console.WriteLine("ERROR: Invalid connection number.");
                            //    break;
                            case "--stop":
                                var con = 0;
                                if(pArgs.Length!=2 || !Int32.TryParse(pArgs[1],out con))
                                {
                                    Console.WriteLine("SYNTAX: --start {connection number}");
                                    return;
                                }

                                if(con < pipeClient.ConnectionCount)
                                {
                                    if(pipeClient[con].ConnectionStatus==ConnectionStatus.Disconnected)
                                    {
                                        
                                    } else
                                    {
                                        Console.WriteLine("{0} is already {1}.", pipeClient[con].Name, pipeClient[con].ConnectionStatus.ToString().ToLower());
                                    }
                                }
                                else
                                    Console.WriteLine("ERROR: Invalid connection number.");
                                break;

                        }
                    };


        }

        private static void ListConnections(ControllerPipeClient pPipeClient)
        {
            throw new NotImplementedException();
        }

        private static void PrintHelp()
        {
            throw new NotImplementedException();
        }
    }
}
