﻿//  This file is part of OpenVPN UI.
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

using System.Drawing.Printing;
using System.Windows.Input;
using Esp.Tools.OpenVPN.Certificates;
using Esp.Tools.OpenVPN.Configuration.UI.ViewModel;

namespace Esp.Tools.OpenVPN.Configuration.UI
{
    /// <summary>
    ///     Interaction logic for CreateSelfSignedView.xaml
    /// </summary>
    public partial class CreateEnrollRequestView
    {
        private readonly ViewModelDialogs _viewModelDialogs;

        public CreateEnrollRequestView(ViewModelDialogs pViewModelDialogs)
        {
            _viewModelDialogs = pViewModelDialogs;
            InitializeComponent();
            GlassMargin = new Margins(0, 0, 30, 0);
        }

        public EnrollRequestDetails GetEnrollmentDetails()
        {
            var viewmodel = new CreateEnrollRequestViewModel();

            DataContext = viewmodel;
            viewmodel.Close += () =>
            {
                DialogResult = false;
                Close();
            };
            viewmodel.Ok += () =>
            {
                DialogResult = true;
                Close();
            };


            ShowDialog();

            if (DialogResult.Value)
                return new EnrollRequestDetails
                {
                    CommonName = viewmodel.Name,
                    City = viewmodel.City,
                    CompanyName = viewmodel.CompanyName,
                    Country = viewmodel.Country,
                    County = viewmodel.County,
                    Department = viewmodel.Department,
                    EmailAddress = viewmodel.EmailAddress,
                    Algorithm = viewmodel.Algorithm
                };
            return null;
        }


        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}