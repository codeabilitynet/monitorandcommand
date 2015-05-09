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
