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
            LinearAxis temperatureAxis = TemperatureChart.Axes[1] as LinearAxis;
            SetLinearAxisProperties(temperatureAxis, ViewModel.MinTemperature - TEMPERATURE_MARGIN, ViewModel.MaxTemperature + TEMPERATURE_MARGIN, 1d, true);

            LinearAxis humidityAxis = HumidityChart.Axes[1] as LinearAxis;
            SetLinearAxisProperties(humidityAxis, ViewModel.MinHumidity - HUMIDITY_MARGIN, ViewModel.MaxHumidity + HUMIDITY_MARGIN, 2.5d, true);

            LinearAxis photoVoltageAxis = PhotoVoltageChart.Axes[1] as LinearAxis;
            SetLinearAxisProperties(photoVoltageAxis, 0d, 1d, 0.5d, true); 
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
