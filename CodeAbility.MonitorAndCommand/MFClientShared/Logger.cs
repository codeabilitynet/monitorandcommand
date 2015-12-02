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
using System.IO;
using Microsoft.SPOT;

namespace CodeAbility.MonitorAndCommand.MFClient
{
    public class Logger
    {
        protected static Logger instance = null;
        public static Logger Instance
        {
            get
            {
                if (instance == null)
                    instance = new Logger();

                return instance;
            }
        }

        const string rootPath = "\\SD";
        public string RootPath
        {
            get { return rootPath; }
        }

        string fileName = "\\mfclient.txt";
        public string FileName 
        {
            get { return fileName; }
            set { fileName = value; }
        }

        protected Logger() {}

        public void Write(string message)
        {
            try
            { 
                using (var filestream = new FileStream(BuildFilePath(), FileMode.Append, FileAccess.Write))
                {
                    StreamWriter streamWriter = new StreamWriter(filestream);
                    streamWriter.WriteLine(message);
                    streamWriter.Close(); 
                }
            }
            catch(Exception exception)
            {
                //There is not much we can do except preventing the whole process to crash if something goes wrong with logging
            }
        }

        private string BuildFilePath()
        {
            return RootPath + "\\" + FileName;
        }
    }
}
