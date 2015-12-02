using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CodeAbility.MonitorAndCommand.Models;

namespace CodeAbility.MonitorAndCommand.Windows8Monitor.Models
{
    public class Model
    {
        const int NUMBER_OF_MESSAGES = 100;
        const int ROW_INTERVAL = 60;

        MessageServiceReference.MessageServiceClient client;

        public Model() 
        {
            client = new MessageServiceReference.MessageServiceClient();
        }

        public IEnumerable<SerieItem> LoadTemperatureSerie()
        {
            IEnumerable<Message> messages = client.ListDeviceLastMessagesAsync(NUMBER_OF_MESSAGES, "Netduino3Wifi", "DS18B20", "SensorTemperature", ROW_INTERVAL).Result;

            return BuildSerie(messages);
        }

        public IEnumerable<SerieItem> LoadHumiditySerie()
        {
            IEnumerable<Message> messages = client.ListDeviceLastMessagesAsync(NUMBER_OF_MESSAGES, "Netduino3Wifi", "HIH4000", "SensorHumidity", ROW_INTERVAL).Result;

            return BuildSerie(messages);
        }

        public IEnumerable<SerieItem> LoadVoltageSerie()
        {
            IEnumerable<Message> messages = client.ListDeviceLastMessagesAsync(NUMBER_OF_MESSAGES, "Netduino3Wifi", "SimpleVoltageSensor", "SensorVoltage", ROW_INTERVAL).Result;

            return BuildSerie(messages);
        }

        private IEnumerable<SerieItem> BuildSerie(IEnumerable<Message> messages)
        {
            List<SerieItem> serieItems = new List<SerieItem>();
            foreach (Message message in messages)
            {
                serieItems.Add(new SerieItem() { Timestamp = message.Timestamp, Value = double.Parse(message.Content.ToString()) });
            }

            return serieItems;
        }
    }
}
