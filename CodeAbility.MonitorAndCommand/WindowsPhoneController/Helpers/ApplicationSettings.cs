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
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAbility.MonitorAndCommand.WindowsPhoneController.Helpers
{
    public static class ApplicationSettings
    {

        private static string IP_ADDRESS = "IpAddress";
        public static string IpAddress 
        {
            get { return ReadSetting(IP_ADDRESS) != null ? ReadSetting(IP_ADDRESS).ToString() : String.Empty; }
            set { SaveSetting(IP_ADDRESS, value); }
        }

        private static string PORT_NUMBER = "PortNumber";
        public static int? PortNumber
        {
            get { return ReadSetting(PORT_NUMBER) != null ? Int32.Parse(ReadSetting(PORT_NUMBER).ToString()) : (int?)null; }
            set { SaveSetting(PORT_NUMBER, value); }
        }

        private static object ReadSetting(string name)
        {
            return IsolatedStorageSettings.ApplicationSettings.Contains(name) ? IsolatedStorageSettings.ApplicationSettings[name] : null;
        }

        private static void SaveSetting(string name, object value)
        {
            IsolatedStorageSettings.ApplicationSettings[name] = value;
        }
    }
}
