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
using System.Resources;
using System.Text;
using System.Threading;

namespace CodeAbility.MonitorAndCommand.Netduino
{
    public class Program
    {
        const string IP_ADDRESS = "192.168.178.26";
        const int PORT = 11000;

        public static void Main()
        {
            Process process = new Process();
            process.Start(IP_ADDRESS, PORT);
        }
    }
}
