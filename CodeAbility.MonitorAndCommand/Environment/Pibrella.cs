// .NET/Mono Monitor and Command Middleware for embedded projects.
// Copyright (C) 2015 Paul Gaunard (codeability.net)

// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;

namespace CodeAbility.MonitorAndCommand.Environment
{
    public static class Pibrella
    {
        //Object
        public const string OBJECT_RED_LED = "RedLED";
        public const string OBJECT_YELLOW_LED = "YellowLED";
        public const string OBJECT_GREEN_LED = "GreenLED";
        public const string OBJECT_BUTTON = "Button";

        //Command
        public const string COMMAND_TOGGLE_LED = "ToggleLED";
        public const string COMMAND_BUTTON_PRESSED = "ButtonPressed";

        //Data
        public const string DATA_LED_STATUS = "LEDStatus";
        public const string DATA_BUTTON_STATUS = "ButtonStatus";

        //Content
        public const string CONTENT_LED_STATUS_ON = "On";
        public const string CONTENT_LED_STATUS_OFF = "Off";

        public const string CONTENT_BUTTON_ON = "True";
        public const string CONTENT_BUTTON_OFF = "False";
    }
}
