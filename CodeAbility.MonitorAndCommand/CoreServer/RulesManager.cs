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
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CodeAbility.MonitorAndCommand.Models;

namespace CodeAbility.MonitorAndCommand.Server
{
    internal class RulesManager
    {
        const string ALL = "*";

        List<Rule> rules = new List<Rule>();

        public RulesManager() { }

        public void AddRule(string originator, string fromDevice, string toDevice, string dataSourceOrCommandTarget, string dataOrCommandName)
        {
            if (!Exists(originator, fromDevice, toDevice, dataSourceOrCommandTarget, dataOrCommandName))
                rules.Add(new Rule(originator, fromDevice, toDevice, dataSourceOrCommandTarget, dataOrCommandName));
        }

        public void RemoveRule(string originator, string fromDevice, string toDevice, string dataSourceOrCommandTarget, string dataOrCommandName)
        {
            rules.RemoveAll(x => x.OriginatorDevice.Equals(originator) && 
                                 x.FromDevice.Equals(fromDevice) && 
                                 x.ToDevice.Equals(toDevice) && 
                                 x.DataSourceOrCommandTarget.Equals(dataSourceOrCommandTarget) && 
                                 x.DataSourceOrCommandTarget.Equals(dataOrCommandName));
        }

        public void RemoveAllRules(string deviceName)
        {
            rules.RemoveAll(x => x.FromDevice.Equals(deviceName) || x.ToDevice.Equals(deviceName));
        }

        public void RemoveAllRules()
        {
            rules.Clear();
        }

        public IEnumerable<string> GetAuthorizedDeviceNames(ContentTypes contentType, string fromDevice, string toDevice, string parameter, string content)
        {
			IEnumerable<string> deviceNames = null; 

			if (contentType == ContentTypes.COMMAND)
				deviceNames = GetAuthorizedDeviceNamesForCommand (fromDevice, toDevice, content, parameter);
			else if (contentType == ContentTypes.DATA)
				deviceNames = GetAuthorizedDeviceNamesForData (fromDevice, toDevice, parameter, content);
			else
				throw new NotSupportedException ();

            return deviceNames.Distinct(); 
        }

		protected IEnumerable<string> GetAuthorizedDeviceNamesForCommand(string fromDevice, string toDevice, string parameter, string content)
		{
			IEnumerable<string> deviceNames = null; 

			IEnumerable<Rule> subscribeRules = GetSubscribeRulesForCommand (toDevice, parameter, content);
			IEnumerable<Rule> publishRules = GetPublishRulesForCommand (fromDevice, parameter, content);

			if (ContainsFromAllDevices (subscribeRules)) 
			{
				deviceNames = from rule in subscribeRules
                             where !rule.OriginatorDevice.Equals(fromDevice)
				            select rule.OriginatorDevice; 
			} 
			else 
			{
                deviceNames = from rule in subscribeRules
                              select rule.OriginatorDevice; 
			}
				
			return deviceNames.Distinct();
		}

		protected IEnumerable<string> GetAuthorizedDeviceNamesForData(string fromDevice, string toDevice, string parameter, string content)
		{
			IEnumerable<string> deviceNames = null; 

			IEnumerable<Rule> subscribeRules = GetSubscribeRulesForData (fromDevice, parameter, content);
			IEnumerable<Rule> publishRules = GetPublishRulesForData (fromDevice, parameter, content);

			if (ContainsToAllDevices (publishRules)) 
			{
				deviceNames = from rule in subscribeRules
                             where !rule.OriginatorDevice.Equals(fromDevice)
							select rule.OriginatorDevice; 
			} 
			else 
			{ 
				deviceNames = from rule in subscribeRules
                              select rule.OriginatorDevice;  
			}

			return deviceNames;
		}

		#region Publish/Subscribe rules for Command/Data 

        protected IEnumerable<Rule> GetSubscribeRulesForData(string fromDevice, string commandTarget, string commandName)
        {
			return rules.Where(x => (x.FromDevice.Equals(fromDevice) || x.FromDevice.Equals(ALL)) &&
                                    (x.DataSourceOrCommandTarget.Equals(commandTarget) || x.DataSourceOrCommandTarget.Equals(ALL)) &&
                                    (x.DataOrCommandName.Equals(commandName) || x.DataOrCommandName.Equals(ALL)));
        }
			
        protected IEnumerable<Rule> GetSubscribeRulesForCommand(string toDevice, string commandTarget, string commandName)
        {
            return rules.Where(x => (x.ToDevice.Equals(toDevice) || x.ToDevice.Equals(ALL)) &&
	                                (x.DataSourceOrCommandTarget.Equals(commandTarget) || x.DataSourceOrCommandTarget.Equals(ALL)) &&
	                                (x.DataOrCommandName.Equals(commandName) || x.DataOrCommandName.Equals(ALL)));
        }

		protected IEnumerable<Rule> GetPublishRulesForData(string fromDevice, string commandTarget, string commandName)
        {
            return rules.Where(x => (x.FromDevice.Equals(fromDevice) || x.FromDevice.Equals(ALL)) &&
	                                  (x.DataSourceOrCommandTarget.Equals(commandTarget) || x.DataSourceOrCommandTarget.Equals(ALL)) &&
	                                  (x.DataOrCommandName.Equals(commandName) || x.DataOrCommandName.Equals(ALL)));
        }

        protected IEnumerable<Rule> GetPublishRulesForCommand(string fromDevice, string commandTarget, string commandName)
        {
            return rules.Where(x => (x.FromDevice.Equals(fromDevice) || x.FromDevice.Equals(ALL)) &&
							          (x.DataSourceOrCommandTarget.Equals(commandTarget) || x.DataSourceOrCommandTarget.Equals(ALL)) &&
							          (x.DataOrCommandName.Equals(commandName) || x.DataOrCommandName.Equals(ALL)));
    	}

		#endregion

		#region Helpers

		protected bool ContainsToAllDevices(IEnumerable<Rule> rules)
		{
			return rules.Count (x => x.ToDevice.Equals (ALL)) > 0;
		}

		protected bool ContainsFromAllDevices(IEnumerable<Rule> rules)
		{
			return rules.Count (x => x.FromDevice.Equals (ALL)) > 0;
		}

        protected bool Exists(string originator, string fromDevice, string toDevice, string dataSourceOrCommandTarget, string dataOrCommandName)
        {
            Rule route = rules.FirstOrDefault(x => x.OriginatorDevice.Equals(originator) &&
                                                   x.FromDevice.Equals(fromDevice) && 
                                                   x.ToDevice.Equals(toDevice) &&
                                                   x.DataSourceOrCommandTarget.Equals(dataSourceOrCommandTarget) &&
                                                   x.DataOrCommandName.Equals(dataOrCommandName));

            return (route != null);
        }

		#endregion 
    }
}
