using System;
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

using CodeAbility.MonitorAndCommand.MFClient;
using CodeAbility.MonitorAndCommand.Models;
using CodeAbility.MonitorAndCommand.Environment;

namespace CodeAbility.MonitorAndCommand.Netduino3Wifi
{
    public class Process
    {
        const int STARTUP_TIME = 5000;
        const int PERIOD = 10000;

        const int BUTTON_PRESSED_DURATION = 500;
        const int RECONNECTION_TIMER_DURATION = 60000;

        MessageClient messageClient = null;

        OutputPort boardLed = new OutputPort(Pins.ONBOARD_LED, false);
        InputPort temperatureSensor = new InputPort(Pins.GPIO_PIN_D0, true, Port.ResistorMode.Disabled);

        InterruptPort button = new InterruptPort(Pins.ONBOARD_SW1, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeBoth);

        AutoResetEvent autoEvent = new AutoResetEvent(false);

        //ManualResetEvent boardLedEvent = new ManualResetEvent(false);

        public bool ledState = false;

        public void Start(string ipAddress, int port)
        {
            while (true)
            {
                try
                {
                    autoEvent.Reset();

                    messageClient = new MessageClient(Environment.Devices.NETDUINO_3, false);

                    if (messageClient != null)
                    {
                        messageClient.CommandReceived += socketClient_CommandReceived;

                        messageClient.Start(ipAddress, port);

                        messageClient.PublishData(Environment.Devices.ALL, Environment.Netduino3.OBJECT_BOARD_LED, Environment.Netduino3.DATA_LED_STATUS);
                        messageClient.PublishData(Environment.Devices.ALL, Environment.Netduino3.OBJECT_BUTTON, Environment.Netduino3.DATA_BUTTON_STATUS);
                        messageClient.PublishData(Environment.Devices.ALL, Environment.Netduino3.OBJECT_TEMPERATURE_SENSOR, Environment.Netduino3.DATA_SENSOR_TEMPERATURE);

                        messageClient.SubscribeToCommand(Environment.Devices.ALL, Environment.Netduino3.OBJECT_BOARD_LED, Environment.Netduino3.COMMAND_TOGGLE_LED);
                        messageClient.SubscribeToCommand(Environment.Devices.ALL, Environment.Netduino3.OBJECT_BUTTON, Environment.Netduino3.COMMAND_BUTTON_PRESSED);
                    }

                    button.OnInterrupt += new NativeEventHandler(button_OnInterrupt);

                    TimerCallback workTimerCallBack = DoWork;
                    Timer workTimer = new Timer(workTimerCallBack, messageClient, STARTUP_TIME, PERIOD);

                    autoEvent.WaitOne();
                }
                catch (Exception)
                {
                    if (messageClient != null)
                        messageClient.CommandReceived -= socketClient_CommandReceived;

                    button.OnInterrupt -= new NativeEventHandler(button_OnInterrupt);

                    AutoResetEvent autoResetEvent = new AutoResetEvent(false);
                    autoResetEvent.WaitOne(RECONNECTION_TIMER_DURATION, false);

                    autoEvent.Set();
                }
            }
        }

        void socketClient_CommandReceived(object sender, MessageEventArgs e)
        {
            //Only consider the messages addressed to me
            if (!e.ToDevice.Equals(Environment.Devices.NETDUINO_3))
                return;

            string objectName = e.Parameter.ToString();
            string commandValue = (e.Content != null) ? e.Content.ToString() : String.Empty;

            if (objectName.Equals(Environment.Netduino3.OBJECT_BUTTON))
            {
                //boardLedEvent.Set();
            }
        }

        private void DoWork(object state)
        {
            try
            {
                //Board LED On
                boardLed.Write(true);
                if (messageClient != null)
                    messageClient.SendData(Environment.Devices.ALL, Environment.Netduino3.OBJECT_BOARD_LED, Environment.Netduino3.DATA_LED_STATUS, Environment.Netduino3.CONTENT_LED_STATUS_ON);

                //Sensor data
                //string sensorDataString = new Random().NextDouble().ToString();
                string sensorDataString = new TemperatureSensor().ReadTemperature().ToString();
                if (messageClient != null)
                    messageClient.SendData(Environment.Devices.ALL, Environment.Netduino3.OBJECT_TEMPERATURE_SENSOR, Environment.Netduino3.DATA_SENSOR_TEMPERATURE, sensorDataString);

                //Board LED Off
                boardLed.Write(false);
                if (messageClient != null)
                    messageClient.SendData(Environment.Devices.ALL, Environment.Netduino3.OBJECT_BOARD_LED, Environment.Netduino3.DATA_LED_STATUS, Environment.Netduino3.CONTENT_LED_STATUS_OFF);
            }
            catch (Exception exception)
            {
                Logger.Instance.Write(exception.ToString());
            }
        }

        #region Interruptions

        void button_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            if (messageClient != null)
                messageClient.SendData(Environment.Devices.ALL, Environment.Netduino3.OBJECT_BUTTON, Environment.Netduino3.DATA_BUTTON_STATUS, Environment.Netduino3.CONTENT_BUTTON_PRESSED);
        }

        #endregion 
    }
}
