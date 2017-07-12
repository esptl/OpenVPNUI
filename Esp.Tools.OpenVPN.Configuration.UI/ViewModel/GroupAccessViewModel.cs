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

using System.Collections.Generic;
using Esp.Tools.OpenVPN.SharedUI;

namespace Esp.Tools.OpenVPN.Configuration.UI.ViewModel
{
    public class GroupAccessViewModel : ViewModelBase
    {
        private readonly IViewModelDialogs _dialogs;
        private readonly BaseAccessConfiguration _configuration;

        public GroupAccessViewModel(IViewModelDialogs pDialogs, BaseAccessConfiguration pConfiguration)
        {
            _dialogs = pDialogs;
            _configuration = pConfiguration;
            SelectCommand = new BasicCommand(pValue=>
                                                 {
                                                     if(pValue!=null)
                                                        Select(pValue.ToString());
                                                 });
            UnselectCommand = new BasicCommand(pValue =>
                                                   {
                                                       if(pValue!=null)                                                            
                                                          Unselect(pValue.ToString());
                                                   });
            RestartServiceCommand = new BasicCommand(RestartService);
        }

        private void RestartService()
        {
            var m = new RestartServiceViewModel(_dialogs);
        }

        private void Unselect(string pGroup)
        {
            var groups = SelectedGroups;
            groups.Remove(pGroup);
            SelectedGroups = groups;
        }

        private void Select(string pGroup)
        {
            var groups = SelectedGroups;
            groups.Add(pGroup);
            SelectedGroups = groups;
        }

        public List<string> UnselectedGroups
        {
            get { return _configuration.UnselectedGroups; }
        }

        public List<string> SelectedGroups
        {
            get { return _configuration.SelectedGroups; }
            set
            {
                _configuration.SelectedGroups = value;
                OnPropertyChanged("SelectedGroups");
                OnPropertyChanged("UnselectedGroups");
            }
        }

        public BasicCommand SelectCommand { get; private set; }
        public BasicCommand UnselectCommand { get; private set; }
        public BasicCommand RestartServiceCommand { get; private set; }
    }
}
