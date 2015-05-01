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

namespace CodeAbility.MonitorAndCommand.Models
{

#if !MF_FRAMEWORK_VERSION_V4_2

    public enum ContentTypes
    {
        CONTROL = 0,
        COMMAND = 1,
        DATA = 2,
        HEALTH = 3,
        HEARTBEAT = 4,
        RESPONSE = 5
    }

#endif

#if MF_FRAMEWORK_VERSION_V4_2

    //.NET MF FRAMEWORK v4.2 does not support enums

    public class ContentTypes
    {
        public const int CONTROL = 0;
        public const int COMMAND = 1;
        public const int DATA = 2;
        public const int HEALTH = 3; 
        public const int HEARTBEAT = 4;
        public const int RESPONSE = 5;
    }

#endif
}
