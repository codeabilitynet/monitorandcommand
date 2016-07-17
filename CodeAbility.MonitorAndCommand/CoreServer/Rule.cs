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
            return String.Format("Originator: {0}, From: {1}, To: {2}, DataSourceOrCommandTarget: {3}, DataOrCommandName: {4}", OriginatorDevice, FromDevice, ToDevice, DataSourceOrCommandTarget, DataOrCommandName);
        }
    }
}
