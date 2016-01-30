using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Windows.UI.Core; 
using Windows.UI.Xaml;

using CodeAbility.MonitorAndCommand.Windows8Monitor.Models;
using CodeAbility.MonitorAndCommand.W8Client;
using CodeAbility.MonitorAndCommand.Environment;
using System.Collections.ObjectModel;

namespace CodeAbility.MonitorAndCommand.Windows8Monitor.ViewModels
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        const int LOAD_DATA_PERIOD_IN_SECONDS = 60;

        IEnumerable<SerieItem> temperatureSerie; 
        public IEnumerable<SerieItem> TemperatureSerie
        {
            get { return temperatureSerie; }
            set 
            { 
                temperatureSerie = value;
                OnPropertyChanged("TemperatureSerie");
            }
        }

        string firstTemperatureData;
        public string FirstTemperatureData
        {
            get { return firstTemperatureData; }
            set
            {
                firstTemperatureData = value;

                OnPropertyChanged("FirstTemperatureData");
            }
        }

        IEnumerable<SerieItem> humiditySerie;
        public IEnumerable<SerieItem> HumiditySerie
        {
            get { return humiditySerie; }
            set
            {
                humiditySerie = value;
                OnPropertyChanged("HumiditySerie");
            }
        }

        string firstHumidityData;
        public string FirstHumidityData
        {
            get { return firstHumidityData; }
            set
            {
                firstHumidityData = value;
                
                OnPropertyChanged("FirstHumidityData");
            }
        }

        IEnumerable<SerieItem> voltageSerie;
        public IEnumerable<SerieItem> VoltageSerie
        {
            get { return voltageSerie; }
            set
            {
                voltageSerie = value;
                OnPropertyChanged("VoltageSerie");
            }
        }

        string firstVoltageData;
        public string FirstVoltageData
        {
            get { return firstVoltageData; }
            set
            {
                firstVoltageData = value;

                OnPropertyChanged("FirstVoltageData");
            }
        }

        string receivedData;
        public string ReceivedData
        {
            get { return receivedData; }
            set
            {
                receivedData = value;

                OnPropertyChanged("ReceivedData");
            }
        }

        Model model;

        public double MinTemperature { get; set; }
        public double MaxTemperature { get; set;}

        public double MinHumidity { get; set; }
        public double MaxHumidity { get; set;}

        MainPage mainPage;

        DispatcherTimer dispatcherTimer;

        const string DEFAULT_IP_ADDRESS = "192.168.178.26"; 

        public string IpAddress { get; set; }
        //{
        //    get { return ApplicationSettings.IpAddress; }
        //    set { ApplicationSettings.IpAddress = value; }
        //}

        public int PortNumber { get; set; }
        //{
        //    get { return ApplicationSettings.PortNumber.Value; }
        //    set { ApplicationSettings.PortNumber = value; }
        //}

        public MainPageViewModel(MainPage page)
        {
            mainPage = page;

            model = new Model();

            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, LOAD_DATA_PERIOD_IN_SECONDS);

            dispatcherTimer.Start();

            IpAddress = DEFAULT_IP_ADDRESS;
            PortNumber = 11000;
        }

        public async void Start()
        {        
            MessageClient messageClient = App.Current.Resources["MessageClient"] as MessageClient;

            messageClient.DataReceived += messageClient_DataReceived;

            if (await messageClient.Start(IpAddress, PortNumber))
            {
                messageClient.SubscribeToData(Devices.PIBRELLA, Pibrella.OBJECT_GREEN_LED, Pibrella.DATA_LED_STATUS);
                messageClient.SubscribeToData(Devices.PIBRELLA, Pibrella.OBJECT_YELLOW_LED, Pibrella.DATA_LED_STATUS);
                messageClient.SubscribeToData(Devices.PIBRELLA, Pibrella.OBJECT_RED_LED, Pibrella.DATA_LED_STATUS);
                messageClient.SubscribeToData(Devices.PIBRELLA, Pibrella.OBJECT_BUTTON, Pibrella.DATA_BUTTON_STATUS);

                messageClient.PublishCommand(Devices.PIBRELLA, Pibrella.OBJECT_GREEN_LED, Pibrella.COMMAND_TOGGLE_LED);
                messageClient.PublishCommand(Devices.PIBRELLA, Pibrella.OBJECT_YELLOW_LED, Pibrella.COMMAND_TOGGLE_LED);
                messageClient.PublishCommand(Devices.PIBRELLA, Pibrella.OBJECT_RED_LED, Pibrella.COMMAND_TOGGLE_LED);
                messageClient.PublishCommand(Devices.PIBRELLA, Pibrella.OBJECT_BUTTON, Pibrella.COMMAND_BUTTON_PRESSED);
            }
        }

        void messageClient_DataReceived(object sender, MonitorAndCommand.Models.MessageEventArgs e)
        {
            ReceivedData = e.Parameter.ToString(); 
        }

        void dispatcherTimer_Tick(object sender, object e)
        {
            LoadData(); 
        }

        public void StartDispatcherTimer()
        {

        }

        public void LoadData()
        {
            temperatureSerie = model.LoadTemperatureSerie();
            humiditySerie = model.LoadHumiditySerie();
            voltageSerie = model.LoadVoltageSerie();

            if (TemperatureSerie.Count() > 0)
            {
                MinTemperature = temperatureSerie.Min(x => x.Value);
                MaxTemperature = temperatureSerie.Max(x => x.Value);
            }
            else
            {
                MinTemperature = 15;
                MaxTemperature = 25;
            }

            if (HumiditySerie.Count() > 0)
            {
                MinHumidity = HumiditySerie.Min(x => x.Value);
                MaxHumidity = HumiditySerie.Max(x => x.Value);
            }
            else
            {
                MinHumidity = 40;
                MaxHumidity = 60;
            }

            mainPage.SetChartsAxes();

            TemperatureSerie = temperatureSerie;
            HumiditySerie = humiditySerie;
            VoltageSerie = voltageSerie;

            if (TemperatureSerie.Count() > 0)
                FirstTemperatureData = String.Format("{0}c°", Math.Round(TemperatureSerie.First().Value, 1).ToString());

            if (HumiditySerie.Count() > 0)
                FirstHumidityData = String.Format("{0}%", Math.Round(HumiditySerie.First().Value, 1).ToString());

            if (VoltageSerie.Count() > 0)
                FirstVoltageData = String.Format("{0}V", Math.Round(VoltageSerie.First().Value, 2).ToString());
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));                
            }
        }
    }
}
