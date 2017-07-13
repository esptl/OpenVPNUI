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
using System.Windows.Input;

namespace Esp.Tools.OpenVPN.UI.Model
{
    public class BasicCommand : ICommand
    {
        private readonly Func<object, bool> _canExecute;
        private readonly Action<object> _execute;

        public BasicCommand(Action<object> pExecute, Func<object, bool> pCanExecute)
        {
            _execute = pExecute;
            _canExecute = pCanExecute;
        }


        public BasicCommand(Action pExecute, Func<bool> pCanExecute)
        {
            _execute = pObj => pExecute();
            _canExecute = pObj => pCanExecute();
        }

        public BasicCommand(Action<object> pExecute)
        {
            _execute = pExecute;
            _canExecute = pObj => true;
        }


        public BasicCommand(Action pExecute)
        {
            _execute = pObj => pExecute();
            _canExecute = pObj => true;
        }

        public void TriggerChanged()
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, new EventArgs());
        }

        private event EventHandler CanExecuteChanged;

        #region ICommand Members

        void ICommand.Execute(object pParameter)
        {
            _execute(pParameter);
        }

        bool ICommand.CanExecute(object pParameter)
        {
            return _canExecute(pParameter);
        }

        event EventHandler ICommand.CanExecuteChanged
        {
            add => CanExecuteChanged += value;

            remove => CanExecuteChanged -= value;
        }

        #endregion
    }
}