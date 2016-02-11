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
using Windows.UI.Xaml;

namespace CodeAbility.MonitorAndCommand.Windows8Monitor.Models
{
    public class VoltageModel
    {
        const int NUMBER_OF_MESSAGES = 50;
        const int TIME_INTERVAL_IN_MILLISECONDS = 3000;

        //MessageServiceReference.MessageServiceClient client;

        Queue<SerieItem> serieItems = new Queue<SerieItem>(NUMBER_OF_MESSAGES);

        DispatcherTimer dispatcherTimer;

        double lastReceivedVoltage = 0;
        DateTime lastReceivedTimestamp = DateTime.Now;

        bool GenerateNullValue { get; set; }

        public VoltageModel() 
        {
            //client = new MessageServiceReference.MessageServiceClient();       
            dispatcherTimer = new DispatcherTimer();  
            dispatcherTimer.Tick += dispatcherTimer_Tick;   
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, TIME_INTERVAL_IN_MILLISECONDS);

            InitializeQueue();

            GenerateNullValue = true;

            dispatcherTimer.Start();  
        }

        void  dispatcherTimer_Tick( object  sender,   object  e) 
        {   
            if (GenerateNullValue)
            { 
                lastReceivedTimestamp  =  lastReceivedTimestamp. AddMilliseconds( TIME_INTERVAL_IN_MILLISECONDS) ; 
                serieItems. Enqueue( new  SerieItem( )   {   Timestamp  =  lastReceivedTimestamp,   Value  =  null  } ) ;
            }
            
            GenerateNullValue = true;
        }  
 
        public void EnqueueVoltage(double value, DateTime timestamp)
        {
            GenerateNullValue = false;

            if (serieItems.Count == 0)
                InitializeQueue();

            serieItems.Enqueue(new SerieItem() { Timestamp = timestamp, Value = value });
            
            lastReceivedVoltage = value;
            lastReceivedTimestamp = timestamp;

            if (serieItems.Count >= NUMBER_OF_MESSAGES)
                serieItems.Dequeue();
        }

        public IEnumerable<SerieItem> LoadVoltageSerie()
        {
            return serieItems;
        }

        private void InitializeQueue()
        {
            DateTime now = DateTime.Now;

            for (int index = 0; index < NUMBER_OF_MESSAGES; index++)
            {
                SerieItem serieItem = new SerieItem() { Timestamp = now.AddMilliseconds(-TIME_INTERVAL_IN_MILLISECONDS * index), Value = null };

                if (index == NUMBER_OF_MESSAGES - 1)
                    lastReceivedTimestamp = serieItem.Timestamp;

                serieItems.Enqueue(serieItem);
            }
        }
    }
}
