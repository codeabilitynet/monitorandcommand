/*
 * Copyright (c) 2015, Paul Gaunard (codeability.net)
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

namespace CodeAbility.MonitorAndCommand.Helpers
{
    public static class JsonHelpers
    {
        /// <summary>
        /// Pads a serialized Json message with '.' to ensure its size is equals to given buffer size
        /// </summary>
        /// <param name="serializedMessage"></param>
        /// <param name="bufferSize"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Removes '.' from a "padded" serialized message
        /// </summary>
        /// <param name="paddedSerializedData"></param>
        /// <returns></returns>
        public static string CleanUpPaddedSerializedData(string paddedSerializedData)
        {
            int firstBraceIndex = paddedSerializedData.IndexOf('{');
            int lastBraceIndex = paddedSerializedData.LastIndexOf('}');
            string serializedMessage = paddedSerializedData.Substring(firstBraceIndex, lastBraceIndex - firstBraceIndex + 1);

            return serializedMessage;
        }
    }
}
