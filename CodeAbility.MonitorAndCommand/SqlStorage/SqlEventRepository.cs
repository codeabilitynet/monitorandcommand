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
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CodeAbility.MonitorAndCommand.Repository;
using CodeAbility.MonitorAndCommand.Models;

namespace CodeAbility.MonitorAndCommand.SqlStorage
{
    public class SqlEventRepository : IEventRepository
    {
        public string ConnectionString { get; set; }

        public SqlEventRepository(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public void Insert(Event _event)
        {
            const string CommandName = "SP_Event_Insert";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = CommandName;

                    command.Parameters.AddWithValue("fromDevice", _event.FromDevice);
                    command.Parameters.AddWithValue("content", _event.Content);

                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }

                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }

        public IEnumerable<Event> ListLastEvents(int numberOfLogEntries)
        {
            const string CommandName = "SP_Event_List";

            List<Event> logEntries = new List<Event>();

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("intNumberOfEvents", numberOfLogEntries);
                    command.CommandText = CommandName;

                    try
                    {
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            Event logEntry = InstanciateLogEntry(reader);
                            logEntries.Add(logEntry);
                        }
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }

                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }

            return logEntries;
        }

   
        public void Purge()
        {
            const string CommandName = "SP_Event_Purge";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = CommandName;

                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }

                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }

        private Event InstanciateLogEntry(SqlDataReader reader)
        {
            Event logEntry = new Event()
            {
                FromDevice = (string)reader["FromDevice"],
                Content = (string)reader["Content"],
                Timestamp = (DateTime)reader["TimeStamp"]
            };

            return logEntry;
        }
    }
}
