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
    public static class DS18B20
    {
        //Object
        public const string OBJECT_BOARD_LED = "LED";
        public const string OBJECT_BUTTON = "Button";
        public const string OBJECT_TEMPERATURE_SENSOR = "TemperatureSensor";

        //Command
        public const string COMMAND_BUTTON_PRESSED = "ButtonPressed";
        public const string COMMAND_TOGGLE_LED = "ToggleLed";

        //Data 
        public const string DATA_LED_STATUS = "LEDStatus";
        public const string DATA_BUTTON_STATUS = "ButtonStatus";
        public const string DATA_SENSOR_TEMPERATURE = "SensorTemperature";

        //Content
        //public const string CONTENT_BUTTON_ON = "On";
        //public const string CONTENT_BUTTON_OFF = "Off";
        public const string CONTENT_BUTTON_PRESSED = "Pressed";

        public const string CONTENT_LED_STATUS_ON = "On";
        public const string CONTENT_LED_STATUS_OFF = "Off";
    }
}
