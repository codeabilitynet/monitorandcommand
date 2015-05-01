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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CodeAbility.MonitorAndCommand.Models;

namespace CodeAbility.MonitorAndCommand.Server
{
    internal class Rule
    {
        public string OriginatorDevice { get; protected set; }
        public string FromDevice { get; protected set; }
        public string ToDevice { get; protected set; }
        public string DataSourceOrCommandTarget { get; protected set; }
        public string DataOrCommandName { get; protected set; }
       
        public Rule(string originatorDevice, string fromDevice, string toDevice, string dataSourceOrCommandTarget, string dataOrCommandName)
        {
            OriginatorDevice = originatorDevice;
            FromDevice = fromDevice;
            ToDevice = toDevice;
            DataSourceOrCommandTarget = dataSourceOrCommandTarget;
            DataOrCommandName = dataOrCommandName; 
        }

        public override string ToString()
        {
            return String.Format("Originator: {0}, From: {1}, To: {2}, Parameter: {3}, Content: {4}", OriginatorDevice, FromDevice, ToDevice, DataSourceOrCommandTarget, DataOrCommandName);
        }
    }
}
