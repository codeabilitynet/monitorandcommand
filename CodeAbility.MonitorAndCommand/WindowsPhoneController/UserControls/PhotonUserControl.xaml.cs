/*
 * Copyright (c) 2015, Paul Gaunard (www.codeability.net)
 * All rights reserved.

 * Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
 * - Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
 * - Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the 
 *  documentation and/or other materials provided with the distribution.

 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED 
 * TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR 
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, 
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF 
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS 
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

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

namespace CodeAbility.MonitorAndCommand.WindowsPhoneController.UserControls
{
    public partial class PhotonUserControl : UserControl
    {
        PhotonsViewModel ParentViewModel 
        {
            get 
            { 
                //HACK 
                return ((Grid)this.Parent).DataContext as PhotonsViewModel; 
            }
        }

        Models.Photon Model
        {
            get { return this.DataContext as Models.Photon; }
        }

        public PhotonUserControl()
        {
            InitializeComponent();
        }

        private void GreenEllipse_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            ParentViewModel.ToggleGreenLed(Model.Name); 
        }

        private void RedEllipse_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            ParentViewModel.ToggleRedLed(Model.Name); 
        }
    }
}
