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
    public class SqlMessageRepository : IMessageRepository
    {
        public string ConnectionString { get; set; }

        public SqlMessageRepository(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public void Insert(Message message)
        {
            const string CommandName = "SP_Message_Insert";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = CommandName;

                    command.Parameters.AddWithValue("sendingDevice", message.SendingDevice);
                    command.Parameters.AddWithValue("receivingDevice", message.ReceivingDevice);
                    command.Parameters.AddWithValue("fromDevice", message.FromDevice);
                    command.Parameters.AddWithValue("toDevice", message.ToDevice);
                    command.Parameters.AddWithValue("contentType", message.ContentType.ToString());
                    command.Parameters.AddWithValue("name", message.Name);
                    command.Parameters.AddWithValue("parameter", message.Parameter != null ? message.Parameter.ToString() : String.Empty);
                    command.Parameters.AddWithValue("content", message.Content != null ? message.Content.ToString() : String.Empty);
                    command.Parameters.AddWithValue("timestamp", message.Timestamp);

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

        public IEnumerable<Message> ListLastMessages(int numberOfMessages)
        {
            const string CommandName = "SP_Message_List";

            List<Message> messages = new List<Message>();

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("intNumberOfMessages", numberOfMessages);
                    command.CommandText = CommandName;

                    try
                    {
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            Message message = InstanciateMessage(reader);
                            messages.Add(message);
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

            return messages;
        }

        public IEnumerable<Average> ListHourlyAverages(int numberOfMessages, string deviceName, string objectName, string parameterName)
        {
            const string CommandName = "SP_Message_HourlyAveragesList";

            List<Average> averages = new List<Average>();

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("intNumberOfMessages", numberOfMessages);
                    command.Parameters.AddWithValue("strDeviceName", deviceName);
                    command.Parameters.AddWithValue("strObjectName", objectName);
                    command.Parameters.AddWithValue("strParameterName", parameterName);
                    command.CommandText = CommandName;

                    try
                    {
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            Average average = InstanciateAverage(reader);
                            averages.Add(average);
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

            return averages;
        }

        public IEnumerable<Average> List15MinutesAverages(int numberOfMessages, string deviceName, string objectName, string parameterName)
        {
            const string CommandName = "SP_Message_15MinutesAveragesList";

            List<Average> averages = new List<Average>();

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("intNumberOfMessages", numberOfMessages);
                    command.Parameters.AddWithValue("strDeviceName", deviceName);
                    command.Parameters.AddWithValue("strObjectName", objectName);
                    command.Parameters.AddWithValue("strParameterName", parameterName);
                    command.CommandText = CommandName;

                    try
                    {
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            Average average = InstanciateAverage(reader);
                            averages.Add(average);
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

            return averages;
        }

        public void Purge()
        {
            const string CommandName = "SP_Message_Purge";

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

        private Message InstanciateMessage(SqlDataReader reader)
        {
            Message message = new Message()
            {
                SendingDevice = (string)reader["SendingDevice"],
                ReceivingDevice = (string)reader["ReceivingDevice"],
                FromDevice = (string)reader["FromDevice"],
                ToDevice = (string)reader["ToDevice"],
                ContentType = (ContentTypes)Enum.Parse(typeof(ContentTypes), reader["ContentType"].ToString()),
                Name = (string)reader["Name"],
                Parameter = (string)reader["Parameter"],
                Content = (string)reader["Content"],
                Timestamp = (DateTime)reader["Timestamp"]
            };

            return message;
        }

        private Average InstanciateAverage(SqlDataReader reader)
        {
            Average average = new Average()
            {
                Year = Int32.Parse(reader["Year"].ToString()),
                Month = Int32.Parse(reader["Month"].ToString()),
                Day = Int32.Parse(reader["Day"].ToString()),
                Hour = Int32.Parse(reader["Hour"].ToString()),
                Minute = Int32.Parse(reader["Minute"].ToString()),
                Value = Double.Parse(reader["Average"].ToString())
            };

            return average;
        }
    }
}
