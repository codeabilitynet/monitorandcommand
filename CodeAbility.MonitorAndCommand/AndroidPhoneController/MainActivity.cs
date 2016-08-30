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
using System.Collections.Concurrent;
using System.Timers;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Graphics.Drawables;
using Android.Graphics;

using CodeAbility.MonitorAndCommand.Client;
using CodeAbility.MonitorAndCommand.Environment;
using CodeAbility.MonitorAndCommand.Models;
using static Android.Graphics.PorterDuff;

namespace CodeAbility.MonitorAndCommand.AndroidPhoneController
{
    /// <summary>
    /// 
    /// </summary>

    [Activity(Label = "Android Phone Controller", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        string ipAddress = "192.168.178.22";
        int portNumber = 11000;

        MessageClient Client { get; set; }

        ToggleButton ConnectButton { get; set; }

        Button PhotonA_RedLEDButton { get; set; }
        Button PhotonA_GreenLEDButton { get; set; }
        TextView PhotonA_TemperatureTextView { get; set; }
        TextView PhotonA_HumidityTextView { get; set; }

        Button PhotonB_RedLEDButton { get; set; }
        Button PhotonB_GreenLEDButton { get; set; }
        TextView PhotonB_TemperatureTextView { get; set; }
        TextView PhotonB_HumidityTextView { get; set; }

        Button PhotonC_RedLEDButton { get; set; }
        Button PhotonC_GreenLEDButton { get; set; }
        TextView PhotonC_TemperatureTextView { get; set; }
        TextView PhotonC_HumidityTextView { get; set; }

        public class PhotonStates
        {
            public int RedSeekBar { get; set; }
            public int GreenSeekBar { get; set; }
            public int BlueSeekBar { get; set; }

            public int RGBRed { get; set; }
            public int RGBGreen { get; set; }
            public int RGBBlue { get; set; }

            public PhotonStates()
            {
                RedSeekBar = 0;
                GreenSeekBar = 0;
                BlueSeekBar = 0;

                RGBRed = 0;
                RGBGreen = 0;
                RGBBlue = 0;
            }
        }

        PhotonStates PhotonA_States = new PhotonStates();
        PhotonStates PhotonB_States = new PhotonStates();
        PhotonStates PhotonC_States = new PhotonStates();

        ConcurrentQueue<Models.Message> receivedMessages = new ConcurrentQueue<Models.Message>();

        Timer uiUpdatesTimer = new Timer(100);

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            ConnectButton = FindViewById<ToggleButton>(Resource.Id.ConnectButton);

            PhotonA_RedLEDButton = FindViewById<Button>(GetResourceId(Devices.PHOTON_A, "redLEDButton"));
            PhotonA_GreenLEDButton = FindViewById<Button>(GetResourceId(Devices.PHOTON_A, "greenLEDButton"));
            PhotonB_RedLEDButton = FindViewById<Button>(GetResourceId(Devices.PHOTON_B, "redLEDButton"));
            PhotonB_GreenLEDButton = FindViewById<Button>(GetResourceId(Devices.PHOTON_B, "greenLEDButton"));
            PhotonC_RedLEDButton = FindViewById<Button>(GetResourceId(Devices.PHOTON_C, "redLEDButton"));
            PhotonC_GreenLEDButton = FindViewById<Button>(GetResourceId(Devices.PHOTON_C, "greenLEDButton"));

            ConnectButton.Text = "Connect";
            Client = new MessageClient(Devices.ANDROID_PHONE);
            Client.DataReceived += Client_DataReceived;

            ConnectButton.Click += ConnectButton_Click;

            PhotonA_RedLEDButton.Click += PhotonA_RedLEDButton_Click;
            PhotonA_GreenLEDButton.Click += PhotonA_GreenLEDButton_Click;
            PhotonB_RedLEDButton.Click += PhotonB_RedLEDButton_Click;
            PhotonB_GreenLEDButton.Click += PhotonB_GreenLEDButton_Click;
            PhotonC_RedLEDButton.Click += PhotonC_RedLEDButton_Click;
            PhotonC_GreenLEDButton.Click += PhotonC_GreenLEDButton_Click;

            ToggleLEDButton(Devices.PHOTON_A, Photon.OBJECT_BOARD_LED, Photon.CONTENT_LED_STATUS_OFF);
            ToggleLEDButton(Devices.PHOTON_A, Photon.OBJECT_RED_LED, Photon.CONTENT_LED_STATUS_OFF);
            ToggleLEDButton(Devices.PHOTON_A, Photon.OBJECT_GREEN_LED, Photon.CONTENT_LED_STATUS_OFF);

            ToggleLEDButton(Devices.PHOTON_B, Photon.OBJECT_BOARD_LED, Photon.CONTENT_LED_STATUS_OFF);
            ToggleLEDButton(Devices.PHOTON_B, Photon.OBJECT_RED_LED, Photon.CONTENT_LED_STATUS_OFF);
            ToggleLEDButton(Devices.PHOTON_B, Photon.OBJECT_GREEN_LED, Photon.CONTENT_LED_STATUS_OFF);

            ToggleLEDButton(Devices.PHOTON_C, Photon.OBJECT_BOARD_LED, Photon.CONTENT_LED_STATUS_OFF);
            ToggleLEDButton(Devices.PHOTON_C, Photon.OBJECT_RED_LED, Photon.CONTENT_LED_STATUS_OFF);
            ToggleLEDButton(Devices.PHOTON_C, Photon.OBJECT_GREEN_LED, Photon.CONTENT_LED_STATUS_OFF);

            uiUpdatesTimer.Elapsed += UiUpdatesTimer_Elapsed;
            uiUpdatesTimer.Start();
        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            if (Client != null && !Client.IsConnected)
            {
                Client.Start(ipAddress, portNumber);

                Client.SubscribeToData(Devices.PHOTON_A, Photon.OBJECT_SENSOR, Photon.DATA_SENSOR_HUMIDITY);
                Client.SubscribeToData(Devices.PHOTON_A, Photon.OBJECT_SENSOR, Photon.DATA_SENSOR_TEMPERATURE);
                Client.SubscribeToData(Devices.PHOTON_A, Photon.OBJECT_BOARD_LED, Photon.DATA_LED_STATUS);
                Client.SubscribeToData(Devices.PHOTON_A, Photon.OBJECT_GREEN_LED, Photon.DATA_LED_STATUS);
                Client.SubscribeToData(Devices.PHOTON_A, Photon.OBJECT_RED_LED, Photon.DATA_LED_STATUS);
                Client.SubscribeToData(Devices.PHOTON_A, Photon.OBJECT_RGB_LED, Photon.DATA_RGB_RED);
                Client.SubscribeToData(Devices.PHOTON_A, Photon.OBJECT_RGB_LED, Photon.DATA_RGB_GREEN);
                Client.SubscribeToData(Devices.PHOTON_A, Photon.OBJECT_RGB_LED, Photon.DATA_RGB_BLUE);

                Client.SubscribeToData(Devices.PHOTON_B, Photon.OBJECT_SENSOR, Photon.DATA_SENSOR_HUMIDITY);
                Client.SubscribeToData(Devices.PHOTON_B, Photon.OBJECT_SENSOR, Photon.DATA_SENSOR_TEMPERATURE);
                Client.SubscribeToData(Devices.PHOTON_B, Photon.OBJECT_BOARD_LED, Photon.DATA_LED_STATUS);
                Client.SubscribeToData(Devices.PHOTON_B, Photon.OBJECT_GREEN_LED, Photon.DATA_LED_STATUS);
                Client.SubscribeToData(Devices.PHOTON_B, Photon.OBJECT_RED_LED, Photon.DATA_LED_STATUS);
                Client.SubscribeToData(Devices.PHOTON_B, Photon.OBJECT_RGB_LED, Photon.DATA_RGB_RED);
                Client.SubscribeToData(Devices.PHOTON_B, Photon.OBJECT_RGB_LED, Photon.DATA_RGB_GREEN);
                Client.SubscribeToData(Devices.PHOTON_B, Photon.OBJECT_RGB_LED, Photon.DATA_RGB_BLUE);

                Client.SubscribeToData(Devices.PHOTON_C, Photon.OBJECT_SENSOR, Photon.DATA_SENSOR_HUMIDITY);
                Client.SubscribeToData(Devices.PHOTON_C, Photon.OBJECT_SENSOR, Photon.DATA_SENSOR_TEMPERATURE);
                Client.SubscribeToData(Devices.PHOTON_C, Photon.OBJECT_BOARD_LED, Photon.DATA_LED_STATUS);
                Client.SubscribeToData(Devices.PHOTON_C, Photon.OBJECT_GREEN_LED, Photon.DATA_LED_STATUS);
                Client.SubscribeToData(Devices.PHOTON_C, Photon.OBJECT_RED_LED, Photon.DATA_LED_STATUS);
                Client.SubscribeToData(Devices.PHOTON_C, Photon.OBJECT_RGB_LED, Photon.DATA_RGB_RED);
                Client.SubscribeToData(Devices.PHOTON_C, Photon.OBJECT_RGB_LED, Photon.DATA_RGB_GREEN);
                Client.SubscribeToData(Devices.PHOTON_C, Photon.OBJECT_RGB_LED, Photon.DATA_RGB_BLUE);

                Client.PublishCommand(Devices.PHOTON_A, Photon.OBJECT_GREEN_LED, Photon.COMMAND_TOGGLE_LED);
                Client.PublishCommand(Devices.PHOTON_A, Photon.OBJECT_RED_LED, Photon.COMMAND_TOGGLE_LED);
                Client.PublishCommand(Devices.PHOTON_A, Photon.OBJECT_RGB_LED, Photon.COMMAND_SET_RGB_RED);
                Client.PublishCommand(Devices.PHOTON_A, Photon.OBJECT_RGB_LED, Photon.COMMAND_SET_RGB_GREEN);
                Client.PublishCommand(Devices.PHOTON_A, Photon.OBJECT_RGB_LED, Photon.COMMAND_SET_RGB_BLUE);

                Client.PublishCommand(Devices.PHOTON_B, Photon.OBJECT_GREEN_LED, Photon.COMMAND_TOGGLE_LED);
                Client.PublishCommand(Devices.PHOTON_B, Photon.OBJECT_RED_LED, Photon.COMMAND_TOGGLE_LED);
                Client.PublishCommand(Devices.PHOTON_B, Photon.OBJECT_RGB_LED, Photon.COMMAND_SET_RGB_RED);
                Client.PublishCommand(Devices.PHOTON_B, Photon.OBJECT_RGB_LED, Photon.COMMAND_SET_RGB_GREEN);
                Client.PublishCommand(Devices.PHOTON_B, Photon.OBJECT_RGB_LED, Photon.COMMAND_SET_RGB_BLUE);

                Client.PublishCommand(Devices.PHOTON_C, Photon.OBJECT_GREEN_LED, Photon.COMMAND_TOGGLE_LED);
                Client.PublishCommand(Devices.PHOTON_C, Photon.OBJECT_RED_LED, Photon.COMMAND_TOGGLE_LED);
                Client.PublishCommand(Devices.PHOTON_C, Photon.OBJECT_RGB_LED, Photon.COMMAND_SET_RGB_RED);
                Client.PublishCommand(Devices.PHOTON_C, Photon.OBJECT_RGB_LED, Photon.COMMAND_SET_RGB_GREEN);
                Client.PublishCommand(Devices.PHOTON_C, Photon.OBJECT_RGB_LED, Photon.COMMAND_SET_RGB_BLUE);

                ConnectButton = FindViewById<ToggleButton>(Resource.Id.ConnectButton);
                ConnectButton.Text = "Disconnect";
            }
            else if (Client != null && Client.IsConnected)
            {
                OnStop();
            }
        }

        protected override void OnStop()
        {
            Client.Stop();

            base.OnStop();
        }

        protected override void OnDestroy()
        {
            if (Client != null && Client.IsConnected)
            {
                Client.Stop();
            }

            base.OnDestroy();
        }

        private void PhotonA_RedLEDButton_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            ButtonClick(Devices.PHOTON_A, Photon.OBJECT_RED_LED);
        }

        private void PhotonA_GreenLEDButton_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            ButtonClick(Devices.PHOTON_A, Photon.OBJECT_GREEN_LED);           
        }

        private void PhotonB_RedLEDButton_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            ButtonClick(Devices.PHOTON_B, Photon.OBJECT_RED_LED);
        }

        private void PhotonB_GreenLEDButton_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            ButtonClick(Devices.PHOTON_B, Photon.OBJECT_GREEN_LED);
        }

        private void PhotonC_RedLEDButton_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            ButtonClick(Devices.PHOTON_C, Photon.OBJECT_RED_LED);
        }

        private void PhotonC_GreenLEDButton_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            ButtonClick(Devices.PHOTON_C, Photon.OBJECT_GREEN_LED);
        }

        private void ButtonClick(string deviceName, string objectName)
        {
            if (Client != null && Client.IsConnected)
            {
                Client.SendCommand(deviceName, objectName, Photon.COMMAND_TOGGLE_LED, String.Empty);
            }
        }

        protected void Client_DataReceived(object sender, MessageEventArgs e)
        {
            Models.Message message = new Models.Message(e.SendingDevice, e.FromDevice, e.ToDevice, e.ContentType, e.Name, e.Parameter, e.Content);
            receivedMessages.Enqueue(message);
        }

        private void UiUpdatesTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (receivedMessages.Count > 0)
                {
                    Models.Message message = null;
                    if (receivedMessages.TryDequeue(out message))
                    {
                        PhotonStates photonStates = GetPhotonStates(message.FromDevice);

                        if (message.Name.Equals(Photon.OBJECT_SENSOR))
                        {
                            if (message.Parameter.Equals(Photon.DATA_SENSOR_TEMPERATURE))
                            {
                                this.RunOnUiThread(() => { UpdateTextView(message.FromDevice, Photon.DATA_SENSOR_TEMPERATURE, message.Content.ToString()); });
                            }
                            else if (message.Parameter.Equals(Photon.DATA_SENSOR_HUMIDITY))
                            {
                                this.RunOnUiThread(() => { UpdateTextView(message.FromDevice, Photon.DATA_SENSOR_HUMIDITY, message.Content.ToString()); });
                            }
                        }
                        else if (message.Name.Equals(Photon.OBJECT_BOARD_LED))
                        {
                            if (message.Parameter.Equals(Photon.DATA_LED_STATUS))
                            {
                                this.RunOnUiThread(() => { ToggleLEDButton(message.FromDevice, Photon.OBJECT_BOARD_LED, message.Content.ToString()); });
                            }
                        }
                        else if (message.Name.Equals(Photon.OBJECT_RED_LED))
                        {
                            if (message.Parameter.Equals(Photon.DATA_LED_STATUS))
                            {
                                this.RunOnUiThread(() => { ToggleLEDButton(message.FromDevice, Photon.OBJECT_RED_LED, message.Content.ToString()); });
                            }
                        }
                        else if (message.Name.Equals(Photon.OBJECT_GREEN_LED))
                        {
                            if (message.Parameter.Equals(Photon.DATA_LED_STATUS))
                            {
                                this.RunOnUiThread(() => { ToggleLEDButton(message.FromDevice, Photon.OBJECT_GREEN_LED, message.Content.ToString()); });
                            }
                        }
                        else if (message.Name.Equals(Photon.OBJECT_RGB_LED))
                        {
                            switch(message.FromDevice)
                            {
                                case Environment.Devices.PHOTON_A:
                                    photonStates = PhotonA_States;
                                    break;
                                case Environment.Devices.PHOTON_B:
                                    photonStates = PhotonB_States;
                                    break;
                                case Environment.Devices.PHOTON_C:
                                    photonStates = PhotonC_States;
                                    break;
                            }

                            if (message.Parameter.Equals(Photon.DATA_RGB_RED))
                                photonStates.RGBRed = Int32.Parse(message.Content.ToString());
                            else if (message.Parameter.Equals(Photon.DATA_RGB_GREEN))
                                photonStates.RGBGreen = Int32.Parse(message.Content.ToString());
                            else if (message.Parameter.Equals(Photon.DATA_RGB_BLUE))
                                photonStates.RGBBlue = Int32.Parse(message.Content.ToString());

                            this.RunOnUiThread(() => { SetRGBButton(message.FromDevice, photonStates.RGBRed, photonStates.RGBGreen, photonStates.RGBBlue); });
                        }
                    }
                }

                HandleSeekBarsChanges(Devices.PHOTON_A);
                HandleSeekBarsChanges(Devices.PHOTON_B);
                HandleSeekBarsChanges(Devices.PHOTON_C);
                
            }
            catch(Exception)
            {
                //Add logging for Mono.Android
            }
        }

        protected void UpdateTextView(string deviceName, string dataName, string value)
        {
            try
            {
                int textViewId = 0;
                switch (dataName)
                {
                    case Photon.DATA_SENSOR_TEMPERATURE:
                        textViewId = GetResourceId(deviceName, "temperatureTextView");
                        value = value + "°c";
                        break;
                    case Photon.DATA_SENSOR_HUMIDITY:
                        textViewId = GetResourceId(deviceName, "humidityTextView");
                        value = value + "%";
                        break;
                }

                TextView textView = FindViewById<TextView>(textViewId);

                textView.Text = value;
            }
            catch(Exception exception)
            {
                //Add logging for Mono.Android
            }
        }

        protected void ToggleLEDButton(string deviceName, string objectName, string state)
        {
         
            try
            {
                int ledButtonId = GetLEDButtonId(deviceName, objectName);
                Button ledButton = FindViewById<Button>(ledButtonId);
                Drawable background = ledButton.Background;
                background.SetAlpha(state.Equals(Photon.CONTENT_LED_STATUS_ON) ? 255 : 50);

            }
            catch (Exception exception)
            {
                //Add logging for Mono.Android
            }
        }

        protected void SetRGBButton(string deviceName, int redLED, int greenLED, int blueLED)
        {

            try
            {
                Button ledButton = FindViewById<Button>(GetResourceId(deviceName, "RGBLEDButton"));
                Color color = new Color(redLED, greenLED, blueLED);
                Drawable background = ledButton.Background;
                background.SetColorFilter(color, Mode.Screen);
            }
            catch (Exception exception)
            {
                //Add logging for Mono.Android
            }
        }

        protected PhotonStates GetPhotonStates(string deviceName)
        {
            switch (deviceName)
            {
                case Environment.Devices.PHOTON_A:
                    return PhotonA_States;
                case Environment.Devices.PHOTON_B:
                    return PhotonB_States;
                case Environment.Devices.PHOTON_C:
                    return PhotonC_States;
                default:
                    return null;
            }
        }

        void HandleSeekBarsChanges(string deviceName)
        {
            try
            {
                SeekBar seekBarRed = FindViewById<SeekBar>(GetResourceId(deviceName, "seekBarRed"));
                SeekBar seekBarGreen = FindViewById<SeekBar>(GetResourceId(deviceName, "seekBarGreen"));
                SeekBar seekBarBlue = FindViewById<SeekBar>(GetResourceId(deviceName, "seekBarBlue"));

                PhotonStates photonStates = GetPhotonStates(deviceName);

                if (seekBarRed.Progress != photonStates.RedSeekBar)
                {
                    photonStates.RedSeekBar = seekBarRed.Progress;
                    int setRGBRed = (int)(photonStates.RedSeekBar * 2.555);
                    SendCommand(deviceName, Photon.OBJECT_RGB_LED, Photon.COMMAND_SET_RGB_RED, setRGBRed.ToString());
                }

                if (seekBarGreen.Progress != photonStates.GreenSeekBar)
                {
                    photonStates.GreenSeekBar = seekBarGreen.Progress;
                    int setRGBGreen = (int)(photonStates.GreenSeekBar * 2.555);
                    SendCommand(deviceName, Photon.OBJECT_RGB_LED, Photon.COMMAND_SET_RGB_GREEN, setRGBGreen.ToString());
                }

                if (seekBarBlue.Progress != photonStates.BlueSeekBar)
                {
                    photonStates.BlueSeekBar = seekBarBlue.Progress;
                    int setRGBBlue = (int)(photonStates.BlueSeekBar * 2.555);
                    SendCommand(deviceName, Photon.OBJECT_RGB_LED, Photon.COMMAND_SET_RGB_BLUE, setRGBBlue.ToString());
                }
            }
            catch(Exception exception)
            {
                //Add logging for Mono.Android
            }
        }

        void SendCommand(string deviceName, string targetName, string commandName, string content)
        {
            if (Client != null && Client.IsConnected)
            {
                Client.SendCommand(deviceName, targetName, commandName, content);
            }
        }

        protected int GetLEDButtonId(string deviceName, string objectName)
        {
            switch (objectName)
            {
                case Photon.OBJECT_BOARD_LED:
                    return GetResourceId(deviceName, "boardLEDButton");
                case Photon.OBJECT_RED_LED:
                    return GetResourceId(deviceName, "redLEDButton");
                case Photon.OBJECT_GREEN_LED:
                    return GetResourceId(deviceName, "greenLEDButton");
                default:
                    return 0;
            }
        }

        protected Color GetLEDColor(string objectSource, bool isOn)
        {
            switch (objectSource)
            {
                case Photon.OBJECT_RED_LED:
                    return isOn ? new Color(255, 0, 0, 255) : new Color(255, 0, 0, 50);
                case Photon.OBJECT_GREEN_LED:
                    return isOn ? new Color(0, 255, 0, 255) : new Color(0, 255, 0, 50);
                case Photon.OBJECT_BOARD_LED:
                    return isOn ? new Color(0, 0, 255, 255) : new Color(0, 0, 255, 50);
            }

            return new Color();
        }

        protected int GetResourceId(string deviceName, string resourceBaseName)
        {
            const string formatString = "{0}_{1}";

            string resourceName = string.Format(formatString, deviceName.Replace(' ', '_'), resourceBaseName);
            return this.Resources.GetIdentifier(resourceName, "id", this.ApplicationContext.PackageName);
        }
    }
}