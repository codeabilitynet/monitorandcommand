// .NET/Mono Monitor and Command Middleware for embedded projects.
// Copyright (C) 2015 Paul Gaunard (codeability.net)

// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

using CodeAbility.MonitorAndCommand.WindowsPhoneController.ViewModels;

namespace CodeAbility.MonitorAndCommand.WindowsPhoneController.Pages
{
    public partial class Pibrella : PhoneApplicationPage
    {
        PibrellaViewModel ViewModel { get; set; }

        public Pibrella()
        {
            InitializeComponent();

            ViewModel = new PibrellaViewModel();
            DataContext = ViewModel; 
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            ViewModel.Subscribe();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            ViewModel.Unsubscribe();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ButtonPushed();
        }
    }
}