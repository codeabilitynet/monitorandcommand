using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

using CodeAbility.MonitorAndCommand.Models;
using CodeAbility.MonitorAndCommand.Repository;
using CodeAbility.MonitorAndCommand.SqlStorage;
using CodeAbility.MonitorAndCommand.AzureStorage;

using CodeAbility.MonitorAndCommand.Models; 

namespace MvcApplication.ViewModels
{
    public class ChartsViewModel
    {
        static IMessageRepository messageRepository = new SqlMessageRepository(ConfigurationManager.ConnectionStrings["MonitorAndCommand"].ConnectionString);
        //static IMessageRepository messageRepository = new AzureMessageRepository(ConfigurationManager.AppSettings["StorageConnectionString"].ToString());

        public ChartViewModel TemperatureViewModel { get; set; }
        public ChartViewModel HumidityViewModel { get; set; }
        public ChartViewModel VoltageViewModel { get; set; }
        
        public ChartsViewModel() { }

        const int NUMBER_OF_MESSAGES = 100;

        public void Load(int rowInterval)
        {
            IEnumerable<Message> lastTemperatureMessages = messageRepository.ListLastMessages(NUMBER_OF_MESSAGES, "Netduino3Wifi", "DS18B20", "SensorTemperature", rowInterval);
            IEnumerable<Message> lastHumidityMessages = messageRepository.ListLastMessages(NUMBER_OF_MESSAGES, "Netduino3Wifi", "HIH4000", "SensorHumidity", rowInterval);
            IEnumerable<Message> lastVoltageMessages = messageRepository.ListLastMessages(NUMBER_OF_MESSAGES, "Netduino3Wifi", "SimpleVoltageSensor", "SensorVoltage", rowInterval);

            TemperatureViewModel = new ChartViewModel("Temperature", "Temperature", String.Empty, lastTemperatureMessages);
            HumidityViewModel = new ChartViewModel("Humidity", "Humidity", String.Empty, lastHumidityMessages);
            VoltageViewModel = new ChartViewModel("Voltage", "Voltage", String.Empty, lastVoltageMessages); 
        }
    }
}