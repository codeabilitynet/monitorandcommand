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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

using WinRTXamlToolkit.Controls.DataVisualization.Charting;

using CodeAbility.MonitorAndCommand.Windows8Monitor.ViewModels;
using System.Collections.ObjectModel;

namespace CodeAbility.MonitorAndCommand.Windows8Monitor
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        const float TEMPERATURE_MARGIN = 1.5f;
        const float HUMIDITY_MARGIN = 3f; 

        public MainPageViewModel ViewModel
        {
            get;
            set;
        }

        public MainPage()
        {
            this.InitializeComponent();

            ViewModel = new MainPageViewModel(this);
            this.DataContext = ViewModel;

            this.Loaded += MainPage_Loaded;
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.LoadData();
            ViewModel.Start();
        }

        public void SetChartsAxes()
        {
            LinearAxis voltageAxis = VoltageChart.Axes[1] as LinearAxis;
            SetLinearAxisProperties(voltageAxis, 0d, 3.5d, 0.5d, true); 
        }        

        private DateTimeAxis BuildDateTimeAxis(AxisOrientation orientation, double interval, bool showGridLines)
        {
            DateTimeAxis axis = new DateTimeAxis();
            axis.Orientation = orientation;
            //axis.Interval = 
            axis.ShowGridLines = showGridLines;

            return axis;
        }

        private LinearAxis BuildLinearAxis(AxisOrientation orientation, double minimum, double maximum, double interval, bool showGridLines)
        {
            LinearAxis axis = new LinearAxis();
            axis.Orientation = orientation;
            axis.Minimum = Math.Round(minimum);
            axis.Maximum = Math.Round(maximum);
            axis.Interval = interval; 
            axis.ShowGridLines = showGridLines; 

            return axis; 
        }

        private void SetLinearAxisProperties(LinearAxis axis, double minimum, double maximum, double interval, bool showGridLines)
        {
            axis.Minimum = Math.Round(minimum);
            axis.Maximum = Math.Round(maximum);
            axis.Interval = interval;
            axis.ShowGridLines = showGridLines;
        }
    }
}
