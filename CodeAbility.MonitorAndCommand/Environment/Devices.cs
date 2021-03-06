﻿/*
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
    public class Devices
    {
        public const string ALL = "*";
        public const string SERVER = "Server"; 

        public const string WINDOWS_PHONE = "Nokia Lumia 520";
        public const string WINDOWS_SURFACE = "Surface RT";
        public const string WINDOWS_CONSOLE = "Windows Console";
        public const string WPF_MONITOR = "WPF Monitor";

        //NETMF devices 
        public const string NETDUINO_PLUS = "Netduino Plus";
        public const string NETDUINO_3_WIFI = "Netduino 3 Wifi";

        //.NET/Mono devices 
        public const string RASPBERRY_PI_B = "Raspberry Pi B";
        public const string RASPBERRY_PI_2 = "Raspberry Pi 2";
        public const string ANDROID_PHONE = "Acer Liquid Zest";

        //C++/Wiring devices 
        public const string PHOTON_A = "Photon A";
        public const string PHOTON_B = "Photon B";
        public const string PHOTON_C = "Photon C"; 

    }
}
