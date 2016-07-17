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

        public Average.ChartSpans ChartSpan { get; set; }

        public String Title { get; set; }

        public ChartViewModel PhotonATemperatureViewModel { get; set; }
        public ChartViewModel PhotonBTemperatureViewModel { get; set; }
        public ChartViewModel PhotonCTemperatureViewModel { get; set; }

        public ChartViewModel PhotonAHumidityViewModel { get; set; }
        public ChartViewModel PhotonBHumidityViewModel { get; set; }
        public ChartViewModel PhotonCHumidityViewModel { get; set; }

        public ChartsViewModel(Average.ChartSpans chartSpan)
        {
            ChartSpan = chartSpan;

            SetTitle();
        }

        public void Load()
        {
            IEnumerable<Average> photonATemperatureAverages = messageRepository.ListAverages(ChartSpan, Devices.PHOTON_A, Photon.OBJECT_SENSOR, Photon.DATA_SENSOR_TEMPERATURE);
            IEnumerable<Average> photonBTemperatureAverages = messageRepository.ListAverages(ChartSpan, Devices.PHOTON_B, Photon.OBJECT_SENSOR, Photon.DATA_SENSOR_TEMPERATURE);
            IEnumerable<Average> photonCTemperatureAverages = messageRepository.ListAverages(ChartSpan, Devices.PHOTON_C, Photon.OBJECT_SENSOR, Photon.DATA_SENSOR_TEMPERATURE);

            IEnumerable<Average> photonAHumidityAverages = messageRepository.ListAverages(ChartSpan, Devices.PHOTON_A, Photon.OBJECT_SENSOR, Photon.DATA_SENSOR_HUMIDITY);
            IEnumerable<Average> photonBHumidityAverages = messageRepository.ListAverages(ChartSpan, Devices.PHOTON_B, Photon.OBJECT_SENSOR, Photon.DATA_SENSOR_HUMIDITY);
            IEnumerable<Average> photonCHumidityAverages = messageRepository.ListAverages(ChartSpan, Devices.PHOTON_C, Photon.OBJECT_SENSOR, Photon.DATA_SENSOR_HUMIDITY);

            PhotonATemperatureViewModel = new ChartViewModel(Devices.PHOTON_A, Photon.DATA_SENSOR_TEMPERATURE, String.Empty, photonATemperatureAverages);
            PhotonBTemperatureViewModel = new ChartViewModel(Devices.PHOTON_B, Photon.DATA_SENSOR_TEMPERATURE, String.Empty, photonBTemperatureAverages);
            PhotonCTemperatureViewModel = new ChartViewModel(Devices.PHOTON_C, Photon.DATA_SENSOR_TEMPERATURE, String.Empty, photonCTemperatureAverages);

            PhotonAHumidityViewModel = new ChartViewModel(Devices.PHOTON_A, Photon.DATA_SENSOR_HUMIDITY, String.Empty, photonAHumidityAverages);
            PhotonBHumidityViewModel = new ChartViewModel(Devices.PHOTON_B, Photon.DATA_SENSOR_HUMIDITY, String.Empty, photonBHumidityAverages);
            PhotonCHumidityViewModel = new ChartViewModel(Devices.PHOTON_C, Photon.DATA_SENSOR_HUMIDITY, String.Empty, photonCHumidityAverages);
        }

        private void SetTitle()
        {
            switch (ChartSpan)
            {
                case CodeAbility.MonitorAndCommand.Repository.Average.ChartSpans.Last48Hours:
                    Title = "Last 48 Hours";
                    break;
                case CodeAbility.MonitorAndCommand.Repository.Average.ChartSpans.Last7Days:
                    Title = "Last 7 Days";
                    break;
                case CodeAbility.MonitorAndCommand.Repository.Average.ChartSpans.Last30Days:
                    Title = "Last 30 Days";
                    break;
                case CodeAbility.MonitorAndCommand.Repository.Average.ChartSpans.Last3Monthes:
                    Title = "Last 3 Monthes";
                    break;
                case CodeAbility.MonitorAndCommand.Repository.Average.ChartSpans.LastYear:
                    Title = "Last Year";
                    break;
            }
        }
    }
}