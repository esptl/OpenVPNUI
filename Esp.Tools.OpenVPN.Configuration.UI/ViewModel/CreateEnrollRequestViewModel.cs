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
        public CreateEnrollRequestViewModel()
        {
            
         
           
            OkCommand = new BasicCommand(OnOk, 
                ()=> IsValid);
            CancelCommand=new BasicCommand(OnClose);
            Country = EnrollCountry.IE;
            var mo = new ManagementClass("Win32_OperatingSystem");
            foreach(var instance in mo.GetInstances())
            {
                var props = new Dictionary<string, object>();
                foreach(var prop in instance.Properties)
                {
                    props.Add(prop.Name,prop.Value);
                }
            //    var principal = System.DirectoryServices.AccountManagement.UserPrincipal.Current;
          //      EmailAddress = principal.EmailAddress;
               
               
                 CompanyName = (instance.Properties["Organization"].Value ?? "none").ToString();
            //     var c = (instance.Properties["Country"].Value ?? "none").ToString();
            }
           
           
        }

        protected bool IsValid
        {
            get
            {
                return !(String.IsNullOrEmpty(Name) ||
                         String.IsNullOrEmpty(CompanyName) ||
                         String.IsNullOrEmpty(Department) ||
                         String.IsNullOrEmpty(City) ||
                         String.IsNullOrEmpty(County) ||
                         String.IsNullOrEmpty(EmailAddress));

            }
        }


        public event Action Close;
        public event Action Ok;

        protected void OnOk()
        {
            var handler = Ok;
            if (handler != null) handler();
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
                OkCommand.TriggerChanged();
            }
        }

        private string _companyName;
        public string CompanyName
        {
            get { return _companyName; }
            set
            {
                _companyName = value;
                OkCommand.TriggerChanged();
            }
        }

        private string _department;
        public string Department
        {
            get { return _department; }
            set
            {
                _department = value;
                OkCommand.TriggerChanged();
            }
        }

        private string _city;
        public string City
        {
            get { return _city; }
            set
            {
                _city = value;
                OkCommand.TriggerChanged();
            }
        }

        private string _county;
        public string County
        {
            get { return _county; }
            set
            {
                _county = value;
                OkCommand.TriggerChanged();
            }
        }

        private EnrollCountry _country;
        public EnrollCountry Country
        {
            get { return _country; }
            set
            {
                _country = value;
                OkCommand.TriggerChanged();
            }
        }

        private string _emailAddress;
        public string EmailAddress
        {
            get { return _emailAddress; }
            set
            {
                _emailAddress = value;
                OkCommand.TriggerChanged();
            }
        }

        public BasicCommand OkCommand { get; set; }
        public BasicCommand CancelCommand { get; set; }

        protected void OnClose()
        {
            var handler = Close;
            if (handler != null) handler();
        }
    }
}
