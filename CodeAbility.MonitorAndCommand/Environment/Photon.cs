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

namespace CodeAbility.MonitorAndCommand.Environment
{
    public static class Photon
    {
        //Object
        public const string OBJECT_SENSOR = "Sensor";
        public const string OBJECT_RGB_LED = "RGBLED";
        public const string OBJECT_BOARD_LED = "BoardLED";
        public const string OBJECT_GREEN_LED = "GreenLED";
        public const string OBJECT_RED_LED = "RedLED";
        public const string OBJECT_BUTTON = "Button";

        //Command
        public const string COMMAND_TOGGLE_LED = "ToggleLED";
        public const string COMMAND_SET_RGB_RED = "SetRGBRed";
        public const string COMMAND_SET_RGB_GREEN = "SetRGBGreen";
        public const string COMMAND_SET_RGB_BLUE = "SetRGBBlue";
        public const string COMMAND_BUTTON_PRESSED = "ButtonPressed";

        //Data
        public const string DATA_LED_STATUS = "LEDStatus";
        public const string DATA_RGB_RED = "RGBRed";
        public const string DATA_RGB_GREEN = "RGBGreen";
        public const string DATA_RGB_BLUE = "RGBBlue";
        public const string DATA_SENSOR_TEMPERATURE = "Temperature";
        public const string DATA_SENSOR_HUMIDITY = "Humidity";

        //Content
        public const string CONTENT_LED_STATUS_ON = "On";
        public const string CONTENT_LED_STATUS_OFF = "Off";
        public const string CONTENT_BUTTON_PRESSED = "Pressed";
    }
}
