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

namespace CodeAbility.MonitorAndCommand.AndroidPhoneController
{
    [Activity(Label = "Android Phone Controller", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        string ipAddress = "192.168.178.22";
        int portNumber = 11000;

        MessageClient Client { get; set; }

        Button RedLEDButton { get; set; }
        Button GreenLEDButton { get; set; }

        TextView TemperatureTextView { get; set; }
        TextView HumidityTextView { get; set; }

        ConcurrentQueue<Models.Message> receivedMessages = new ConcurrentQueue<Models.Message>();

        Timer uiUpdatesTimer = new Timer(100);

        int RGBRed { get; set; }
        int RGBGreen { get; set; }
        int RGBBlue { get; set; }

        int RedSeekBar { get; set; }
        int GreenSeekBar { get; set; }
        int BlueSeekBar { get; set; }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            RedLEDButton = FindViewById<Button>(Resource.Id.RedLEDButton);
            GreenLEDButton = FindViewById<Button>(Resource.Id.GreenLEDButton);

            Client = new MessageClient(Devices.ANDROID_PHONE);

            if (Client != null)
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
            }

            uiUpdatesTimer.Elapsed += UiUpdatesTimer_Elapsed;
            uiUpdatesTimer.Start();

            Client.DataReceived += Client_DataReceived;

            RedLEDButton.Click += RedLEDButton_Click;
            GreenLEDButton.Click += GreenLEDButton_Click;
        }

        private void RedLEDButton_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;

            ButtonClick(Devices.PHOTON_B, Photon.OBJECT_RED_LED);
        }

        private void GreenLEDButton_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;

            ButtonClick(Devices.PHOTON_B, Photon.OBJECT_GREEN_LED);           
        }

        private void ButtonClick(string deviceName, string objectName)
        {
            Client.SendCommand(deviceName, objectName, Photon.COMMAND_TOGGLE_LED, String.Empty);
        }

        public void Client_DataReceived(object sender, MessageEventArgs e)
        {
            //Button.Text = String.Format("{0} : {1}", e.Parameter.ToString(), e.Content.ToString());

            Models.Message message = new Models.Message(e.SendingDevice, e.FromDevice, e.ToDevice, e.ContentType, e.Name, e.Parameter, e.Content);
            receivedMessages.Enqueue(message);
        }

        bool checkSeekBars = false;
        private void UiUpdatesTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (receivedMessages.Count > 0)
                {
                    Models.Message message = null;
                    if (receivedMessages.TryDequeue(out message))
                    {
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
                            if (message.Parameter.Equals(Photon.DATA_RGB_RED))
                                RGBRed = Int32.Parse(message.Content.ToString());
                            else if (message.Parameter.Equals(Photon.DATA_RGB_GREEN))
                                RGBGreen = Int32.Parse(message.Content.ToString());
                            else if (message.Parameter.Equals(Photon.DATA_RGB_BLUE))
                                RGBBlue = Int32.Parse(message.Content.ToString());
                        }
                    }
                }

                checkSeekBars = !checkSeekBars;
                if (checkSeekBars)
                {
                    HandleSeekBarsChanges(Devices.PHOTON_B);
                }
            }
            catch(Exception)
            {

            }
        }

        protected void UpdateTextView(string deviceName, string dataName, string value)
        {
            int textViewId = 0;
            switch (dataName)
            {
                case Photon.DATA_SENSOR_TEMPERATURE:
                    textViewId = Resource.Id.temperatureTextView;
                    break;
                case Photon.DATA_SENSOR_HUMIDITY:
                    textViewId = Resource.Id.humidityTextView;
                    break;
            }

            TextView textView = FindViewById<TextView>(textViewId);

            textView.Text = value;
        }

        protected void ToggleLEDButton(string deviceName, string objectName, string state)
        {
            int ledButtonId = Resource.Id.BoardLEDButton;
            switch(objectName)
            {
                case Photon.OBJECT_BOARD_LED:
                    ledButtonId = Resource.Id.BoardLEDButton;
                    break;
                case Photon.OBJECT_RED_LED:
                    ledButtonId = Resource.Id.RedLEDButton;
                    break;
                case Photon.OBJECT_GREEN_LED:
                    ledButtonId = Resource.Id.GreenLEDButton;
                    break;
            }

            try
            {
                Button ledButton = FindViewById<Button>(ledButtonId);

                Color PaleColor = new Color(50, 0, 0);
                Color IntenseColor = new Color(50, 0, 0);

                ledButton.SetBackgroundColor(state.Equals("On") ? IntenseColor : PaleColor);
            }
            catch (Exception exception)
            {

            }
        }

        void HandleSeekBarsChanges(string deviceName)
        {
            SeekBar seekBarRed = FindViewById<SeekBar>(Resource.Id.seekBarRed);
            SeekBar seekBarGreen = FindViewById<SeekBar>(Resource.Id.seekBarGreen);
            SeekBar seekBarBlue = FindViewById<SeekBar>(Resource.Id.seekBarBlue);

            if (seekBarRed.Progress != RedSeekBar)
            {
                RedSeekBar = seekBarRed.Progress;
                int setRGBRed = (int)(RedSeekBar * 2.555);
                Client.SendCommand(deviceName, Photon.OBJECT_RGB_LED, Photon.COMMAND_SET_RGB_RED, setRGBRed.ToString());
            }

            if (seekBarGreen.Progress != GreenSeekBar)
            {
                GreenSeekBar = seekBarGreen.Progress;
                int setRGBGreen = (int)(GreenSeekBar * 2.555);
                Client.SendCommand(deviceName, Photon.OBJECT_RGB_LED, Photon.COMMAND_SET_RGB_GREEN, setRGBGreen.ToString());
            }

            if (seekBarBlue.Progress != BlueSeekBar)
            {
                BlueSeekBar = seekBarBlue.Progress;
                int setRGBBlue = (int)(BlueSeekBar * 2.555);
                Client.SendCommand(deviceName, Photon.OBJECT_RGB_LED, Photon.COMMAND_SET_RGB_BLUE, setRGBBlue.ToString());
            }
        }
    }
}