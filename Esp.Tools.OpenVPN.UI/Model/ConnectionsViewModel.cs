﻿//  This file is part of OpenVPN UI.
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
        private bool _initialized;
        private bool _isPipeConnected;

        public ConnectionsViewModel()
        {
            _controllerPipeClient = new ControllerPipeClient();
            _controllerPipeClient.Connected += OnPipeConnected;
            _controllerPipeClient.Disconnected += OnPipeDisconnected;
            _controllerPipeClient.ConnectionInfo += OnConnectInfo;
            _controllerPipeClient.RequestPassword += OnRequestPassword;
            _controllerPipeClient.Message += OnMessage;
            _controllerPipeClient.Initialized += OnInitialized;
        }


        public List<ConnectionViewModel> Connections { get; private set; } = new List<ConnectionViewModel>();

        public bool AnyConnected
        {
            get { return Connections.Any(con => con.Status == ConnectionStatus.Connected); }
        }

        public bool AnyAuthenticating
        {
            get { return Connections.Any(con => con.Status == ConnectionStatus.Authenticating); }
        }

        public bool AnyConnecting
        {
            get { return Connections.Any(con => con.Status == ConnectionStatus.Connecting); }
        }

        public bool AnyShowLog
        {
            get { return Connections.Any(con => con.ShowLog); }
        }

        public bool IsPipeConnected
        {
            get => _isPipeConnected;
            set
            {
                if (value == _isPipeConnected) return;
                _isPipeConnected = value;
                OnPropertyChanged(nameof(IsPipeConnected));
                OnPropertyChanged(nameof(IsPipeDisconnected));
            }
        }


        public bool IsPipeDisconnected => !IsPipeConnected;

        private void OnInitialized(BaseMessage<InitializedInfo> pObj)
        {
            _initialized = true;
            OnPropertyChanged("Connections");
        }

        private void OnRequestPassword(BaseMessage<RequestPasswordInfo> pInfo)
        {
            Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Normal,
                new Action(() =>
                {
                    var item =
                        Connections.FirstOrDefault(pX => pX.Index == pInfo.Connection);
                    if (item != null)
                        item.OnRequestPassword(pInfo.Data);
                }));
        }

        private void OnPipeConnected()
        {
            Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Normal,
                new Action(() =>
                {
                    IsPipeConnected = true;
                }));
        }


        private void OnPipeDisconnected()
        {
            Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Normal,
                new Action(() =>
                {
                    IsPipeConnected = false;
                }));
        }


        private void OnMessage(BaseMessage<OutputLine> pInfo)
        {
            Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Normal,
                new Action(() =>
                {
                    var item =
                        Connections.FirstOrDefault(pX => pX.Index == pInfo.Connection);
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
                    var item =
                        Connections.FirstOrDefault(pX => pX.Index == pInfo.Connection);
                    if (item != null)
                    {
                        item.OnConnectInfo(pInfo)
                            ;
                    }
                    else
                    {
                        item = new ConnectionViewModel(this, _controllerPipeClient, pInfo);
                        item.LogShown += () =>
                        {
                            foreach (var con in Connections)
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
                        Connections.Add(item);
                        Connections = new List<ConnectionViewModel>(Connections);
                        if (_initialized)
                            OnPropertyChanged("Connections");

                        if (NewConnection != null)
                            NewConnection(item);
                    }
                }));
        }
    }
}