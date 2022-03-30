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
using System.Management;
using Esp.Tools.OpenVPN.Certificates;
using Esp.Tools.OpenVPN.SharedUI;

namespace Esp.Tools.OpenVPN.Configuration.UI.ViewModel
{
    public class CreateEnrollRequestViewModel : ViewModelBase
    {
        private string _city;

        private string _companyName;

        private EnrollCountry _country;

        private string _county;

        private string _department;

        private string _emailAddress;

        private string _name = $"{Environment.MachineName}{Environment.UserName.Replace(" ","").Replace(".","")}";

        public CreateEnrollRequestViewModel()
        {
            OkCommand = new BasicCommand(OnOk,
                () => IsValid);
            CancelCommand = new BasicCommand(OnClose);
            Country = EnrollCountry.IE;
            var mo = new ManagementClass("Win32_OperatingSystem");
            foreach (var instance in mo.GetInstances())
            {
                var props = new Dictionary<string, object>();
                foreach (var prop in instance.Properties)
                    props.Add(prop.Name, prop.Value);
                //    var principal = System.DirectoryServices.AccountManagement.UserPrincipal.Current;
                //      EmailAddress = principal.EmailAddress;


                CompanyName = (instance.Properties["Organization"].Value ?? "none").ToString();
                //     var c = (instance.Properties["Country"].Value ?? "none").ToString();
            }
        }

        protected bool IsValid => !(string.IsNullOrEmpty(Name) ||
                                    string.IsNullOrEmpty(CompanyName) ||
                                    string.IsNullOrEmpty(Department) ||
                                    string.IsNullOrEmpty(City) ||
                                    string.IsNullOrEmpty(County) ||
                                    string.IsNullOrEmpty(EmailAddress));

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged("Name");
                OkCommand.TriggerChanged();
            }
        }

        public string CompanyName
        {
            get => _companyName;
            set
            {
                _companyName = value;
                OkCommand.TriggerChanged();
            }
        }

        public string Department
        {
            get => _department;
            set
            {
                _department = value;
                OkCommand.TriggerChanged();
            }
        }

        public string City
        {
            get => _city;
            set
            {
                _city = value;
                OkCommand.TriggerChanged();
            }
        }

        public string County
        {
            get => _county;
            set
            {
                _county = value;
                OkCommand.TriggerChanged();
            }
        }

        public EnrollCountry Country
        {
            get => _country;
            set
            {
                _country = value;
                OkCommand.TriggerChanged();
            }
        }

        public string EmailAddress
        {
            get => _emailAddress;
            set
            {
                _emailAddress = value;
                OkCommand.TriggerChanged();
            }
        }

        public BasicCommand OkCommand { get; set; }
        public BasicCommand CancelCommand { get; set; }


        public event Action Close;
        public event Action Ok;

        protected void OnOk()
        {
            var handler = Ok;
            if (handler != null) handler();
        }

        protected void OnClose()
        {
            var handler = Close;
            if (handler != null) handler();
        }
    }
}