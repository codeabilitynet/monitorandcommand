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
