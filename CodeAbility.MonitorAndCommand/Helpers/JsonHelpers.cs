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

namespace CodeAbility.MonitorAndCommand.Helpers
{
    public static class JsonHelpers
    {
        public static string PadSerializedMessage(string serializedMessage, int bufferSize)
        {
#if !MF_FRAMEWORK_VERSION_V4_2
            string paddedSerializedData = serializedMessage.PadRight(bufferSize, '.');
#else
            //Padding serializedMessage with dots to have a BUFFER_SIZE bytes message
            int serializedMessageLength = serializedMessage.Length;
            int paddingStringSize = bufferSize - serializedMessageLength;
            string paddingString = new string('.', paddingStringSize);
            string paddedSerializedData = serializedMessage + paddingString;
#endif

            return paddedSerializedData;
        }

        public static string CleanUpSerializedData(string paddedSerializedData)
        {
            int firstBraceIndex = paddedSerializedData.IndexOf('{');
            int lastBraceIndex = paddedSerializedData.LastIndexOf('}');
            string serializedMessage = paddedSerializedData.Substring(firstBraceIndex, lastBraceIndex - firstBraceIndex + 1);

            return serializedMessage;
        }
    }
}
