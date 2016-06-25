using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

using CodeAbility.MonitorAndCommand.Models;
using CodeAbility.MonitorAndCommand.Repository;
using CodeAbility.MonitorAndCommand.SqlStorage;
using CodeAbility.MonitorAndCommand.AzureStorage;
using CodeAbility.MonitorAndCommand.Environment;

namespace MvcApplication.ViewModels
{
    public class ChartsViewModel
    {
        static IMessageRepository messageRepository = new SqlMessageRepository(ConfigurationManager.ConnectionStrings["MonitorAndCommand"].ConnectionString);
        //static IMessageRepository messageRepository = new AzureMessageRepository(ConfigurationManager.AppSettings["StorageConnectionString"].ToString());

        public ChartViewModel PhotonATemperatureViewModel { get; set; }
        public ChartViewModel PhotonBTemperatureViewModel { get; set; }
        public ChartViewModel PhotonCTemperatureViewModel { get; set; }

        public ChartViewModel PhotonAHumidityViewModel { get; set; }
        public ChartViewModel PhotonBHumidityViewModel { get; set; }
        public ChartViewModel PhotonCHumidityViewModel { get; set; }

        public ChartsViewModel() { }

        public void Load(int numberOfMessages, int rowInterval)
        {
            IEnumerable<Message> photonATemperatureMessages = messageRepository.ListLastMessages(numberOfMessages, Devices.PHOTON_A, Photon.OBJECT_SENSOR, Photon.DATA_SENSOR_TEMPERATURE, rowInterval);
            IEnumerable<Message> photonBTemperatureMessages = messageRepository.ListLastMessages(numberOfMessages, Devices.PHOTON_B, Photon.OBJECT_SENSOR, Photon.DATA_SENSOR_TEMPERATURE, rowInterval);
            IEnumerable<Message> photonCTemperatureMessages = messageRepository.ListLastMessages(numberOfMessages, Devices.PHOTON_C, Photon.OBJECT_SENSOR, Photon.DATA_SENSOR_TEMPERATURE, rowInterval);

            IEnumerable<Message> photonAHumidityMessages = messageRepository.ListLastMessages(numberOfMessages, Devices.PHOTON_A, Photon.OBJECT_SENSOR, Photon.DATA_SENSOR_HUMIDITY, rowInterval);
            IEnumerable<Message> photonBHumidityMessages = messageRepository.ListLastMessages(numberOfMessages, Devices.PHOTON_B, Photon.OBJECT_SENSOR, Photon.DATA_SENSOR_HUMIDITY, rowInterval);
            IEnumerable<Message> photonCHumidityMessages = messageRepository.ListLastMessages(numberOfMessages, Devices.PHOTON_C, Photon.OBJECT_SENSOR, Photon.DATA_SENSOR_HUMIDITY, rowInterval);

            PhotonATemperatureViewModel = new ChartViewModel(Devices.PHOTON_A, Photon.DATA_SENSOR_TEMPERATURE, String.Empty, photonATemperatureMessages);
            PhotonBTemperatureViewModel = new ChartViewModel(Devices.PHOTON_B, Photon.DATA_SENSOR_TEMPERATURE, String.Empty, photonBTemperatureMessages);
            PhotonCTemperatureViewModel = new ChartViewModel(Devices.PHOTON_C, Photon.DATA_SENSOR_TEMPERATURE, String.Empty, photonCTemperatureMessages);

            PhotonAHumidityViewModel = new ChartViewModel(Devices.PHOTON_A, Photon.DATA_SENSOR_HUMIDITY, String.Empty, photonAHumidityMessages);
            PhotonBHumidityViewModel = new ChartViewModel(Devices.PHOTON_B, Photon.DATA_SENSOR_HUMIDITY, String.Empty, photonBHumidityMessages);
            PhotonCHumidityViewModel = new ChartViewModel(Devices.PHOTON_C, Photon.DATA_SENSOR_HUMIDITY, String.Empty, photonCHumidityMessages);
        }
    }
}