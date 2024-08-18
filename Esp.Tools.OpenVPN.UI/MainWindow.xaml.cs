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
using System.Windows;
using System.Windows.Forms;
using Esp.Tools.OpenVPN.UI.Model;
using Application = System.Windows.Application;


namespace Esp.Tools.OpenVPN.UI
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public enum Orientation
        {
            UpDown,
            DownUp,
            LeftRight,
            RightLeft
        }

        private const int DispayTimeOut = 250;
        private readonly ConnectionsViewModel _context;
        private readonly NotifyIcon _notify;
        private bool _aboutShowing;
        private Point _basePosition;
        private readonly Timer _timer;

        public MainWindow()
        {
            InitializeComponent();

            _timer = new Timer();
            _timer.Interval = DispayTimeOut;
            _timer.Tick += (s, o) =>
            {
                FadeOut();
                _timer.Enabled = false;
            };
            _context = new ConnectionsViewModel();
            FormFadeOut.Completed += (s, o) => Hide();

            _notify = new NotifyIcon {Text = "VPN Client", Icon = Properties.Resources.disconnected, Visible = true};
            _notify.MouseClick += (pS, pArg) =>

            {
                if (!_aboutShowing && pArg.Button == MouseButtons.Left)
                {
                    _timer.Enabled = false;
                    if (!IsVisible)
                    {
                        Show();
                        Activate();
                        FormFade.Begin();
                    }
                    else
                    {
                        FadeOut();
                    }
                }
            };

            _notify.ContextMenu = new ContextMenu(new[]
            {
                new MenuItem("&About",
                    (pS, pE) => ShowAboutDialog()),
                new MenuItem("-"),
                new MenuItem("&Exit",
                    (pS, pE) => Application.Current.Shutdown())
            });

            _context.NewConnection += pCon => UpdateNotifyIcon();
            _context.Connecting += pCon => UpdateNotifyIcon();
            DataContext = _context;
            _context.Connected +=
                pCon =>
                    Dispatcher.BeginInvoke(
                        new Action(() =>
                        {
                            if (!IsActive)
                                _notify.ShowBalloonTip(2000, "OpenVPN", "You are connected to " + pCon.Name,
                                    ToolTipIcon.Info);
                            UpdateNotifyIcon();
                        }));
            _context.Disconnected += pCon => Dispatcher.BeginInvoke(
                new Action(
                    () =>
                    {
                        if (!IsActive)
                            _notify.ShowBalloonTip(2000, "OpenVPN", "You are disconnected from " + pCon.Name,
                                ToolTipIcon.Info);
                        UpdateNotifyIcon();
                    }));
        }

        private void ShowAboutDialog()
        {
            //    _notify.Visible = false;
            _aboutShowing = true;
            var aboutDialog = new AboutDialog();
            aboutDialog.ShowDialog();
            _aboutShowing = false;
            //  _notify.Visible = true;
        }

        private void FadeOut()
        {
            if (!_context.AnyAuthenticating)
                FormFadeOut.Begin();
        }

        private void UpdateNotifyIcon()
        {
            if (_context.AnyConnecting)
                _notify.Icon = Properties.Resources.connecting;
            else if (_context.AnyConnected)
                _notify.Icon = Properties.Resources.connected;
            else
                _notify.Icon = Properties.Resources.disconnected;
        }


        private void Window_Loaded(object pSender, RoutedEventArgs pE)
        {
            var source = PresentationSource.FromVisual(this);
            var transformToDevice = source.CompositionTarget.TransformFromDevice;
            var p = TrayInfo.GetTrayLocation();
            foreach (var screen in Screen.AllScreens)
                if (!screen.Bounds.Equals(screen.WorkingArea))
                {
                }

            _basePosition = transformToDevice.Transform(new Point(p.X, p.Y));
            _basePosition.Y -= 4;
            //_basePosition.X -= 11;

            Height = _basePosition.Y < 0 ? 0 : _basePosition.Y;
            Top = 5;

            Left = _basePosition.X - Width;
        }

        private void Window_Deactivated(object pSender, EventArgs pE)
        {
            _timer.Enabled = true;
        }

        private void Window_SizeChanged(object pSender, SizeChangedEventArgs e)
        {
            // var top = _border.Margin.Top;

            // var delta = e.NewSize.Height - e.PreviousSize.Height;

            var top = _basePosition.Y - items.DesiredSize.Height; // _basePosition.Y - _content.ActualHeight;
            _border.Margin = new Thickness(0, top, 0, 0);
        }
    }
}