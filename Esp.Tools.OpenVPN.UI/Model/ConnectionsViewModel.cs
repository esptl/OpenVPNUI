//  This file is part of OpenVPN UI.
//  Copyright 2011 ESP Technologies Ltd.
//
//  Author: James Martin - August 2011
//
//
//  Foobar is free software: you can redistribute it and/or modify
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
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Esp.Tools.OpenVPN.Client;
using Esp.Tools.OpenVPN.IPCProtocol;
using Esp.Tools.OpenVPN.IPCProtocol.Contracts;
using Esp.Tools.OpenVPN.IPCProtocol.Controller.Messages.Contracts;

namespace Esp.Tools.OpenVPN.UI.Model
{
    public class ConnectionsViewModel : ViewModelBase
    {
        private readonly ControllerPipeClient _controllerPipeClient;
        private List<ConnectionViewModel> _connections = new List<ConnectionViewModel>();
        private bool _initialized = false;

        public ConnectionsViewModel()
        {
            _controllerPipeClient = new ControllerPipeClient();
            _controllerPipeClient.ConnectionInfo += OnConnectInfo;
            _controllerPipeClient.RequestPassword += OnRequestPassword;
            _controllerPipeClient.Message += OnMessage;
            _controllerPipeClient.Initialized += OnInitialized;
        }

        private void OnInitialized(BaseMessage<InitializedInfo> pObj)
        {
            _initialized = true;
            OnPropertyChanged("Connections");
                                           
        }


        public List<ConnectionViewModel> Connections
        {
            get { return _connections; }
        }

        public bool AnyConnected
        {
            get { return _connections.Any(con => con.Status == ConnectionStatus.Connected); }
        }
        public bool AnyAuthenticating
        {
            get { return _connections.Any(con => con.Status == ConnectionStatus.Authenticating); }
        }

        public bool AnyConnecting
        {
            get { return _connections.Any(con => con.Status == ConnectionStatus.Connecting); }
        }

        public bool AnyShowLog
        {
            get { return _connections.Any(con => con.ShowLog); }
        }

        private void OnRequestPassword(BaseMessage<RequestPasswordInfo> pInfo)
        {
            Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Normal,
                new Action(() =>
                {
                    ConnectionViewModel item =
                        _connections.FirstOrDefault(pX => pX.Index == pInfo.Connection);
                    if (item != null)
                        item.OnRequestPassword(pInfo.Data);
                }));
        }


        private void OnMessage(BaseMessage<OutputLine> pInfo)
        {
            Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Normal,
                new Action(() =>
                               {
                                   ConnectionViewModel item =
                                       _connections.FirstOrDefault(pX => pX.Index == pInfo.Connection);
                                   if (item != null)
                                       item.OnMessage(pInfo)
                                           ;
                               }));
        }

        public event Action<ConnectionViewModel> Connected;
        public event Action<ConnectionViewModel> Connecting;
        public event Action<ConnectionViewModel> Disconnected;
        public event Action<ConnectionViewModel> NewConnection;

        private void OnConnectInfo(BaseMessage<ConfigurationInfo> pInfo)
        {
            Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Normal,
                new Action(() =>
                               {
                                   ConnectionViewModel item =
                                       _connections.FirstOrDefault(pX => pX.Index == pInfo.Connection);
                                   if (item != null)
                                       item.OnConnectInfo(pInfo)
                                           ;
                                   else
                                   {
                                       item = new ConnectionViewModel(this,_controllerPipeClient, pInfo);
                                       item.LogShown += () =>
                                                            {
                                                                foreach (var con in _connections)
                                                                    if (con.ShowLog)
                                                                       con.ShowLog = false;
                                                            };
                                       item.Connected += () =>
                                                             {
                                                                 if (Connected != null)
                                                                     Connected(item);
                                                             };
                                       item.Disconnected += () =>
                                                                {
                                                                    if (Disconnected != null)
                                                                        Disconnected(item);
                                                                };
                                       item.Connecting += () =>
                                                              {
                                                                  if (Connecting != null)
                                                                      Connecting(item);
                                                              };
                                       _connections.Add(item);
                                       _connections = new List<ConnectionViewModel>(_connections);
                                       if(_initialized)
                                           OnPropertyChanged("Connections");
                                           
                                       if (NewConnection != null)
                                           NewConnection(item);
                                   }
                               }));
        }
    }
}