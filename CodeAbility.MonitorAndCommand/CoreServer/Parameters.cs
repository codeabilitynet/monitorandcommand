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
using System.Configuration;

namespace CodeAbility.MonitorAndCommand.Server
{
    internal static class Parameters
    {
        //TODO : check if usefull
        public static readonly int SOCKET_READ_TIMEOUT = Int32.Parse(ConfigurationManager.AppSettings["SOCKET_READ_TIMEOUT"]);
        public static readonly int MAX_CONCURRENT_CLIENTS = Int32.Parse(ConfigurationManager.AppSettings["MAX_CONCURRENT_CLIENTS"]);
        public static readonly int DEFAULT_BUFFER_SIZE = Int32.Parse(ConfigurationManager.AppSettings["DEFAULT_BUFFER_SIZE"]);
        public static readonly int DEFAULT_PORT = Int32.Parse(ConfigurationManager.AppSettings["DEFAULT_PORT"]);
        public static readonly string DEFAULT_SERVICE_URI = ConfigurationManager.AppSettings["DEFAULT_SERVICE_URI"];
    }
}
