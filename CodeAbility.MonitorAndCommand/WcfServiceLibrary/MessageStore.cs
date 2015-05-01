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
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CodeAbility.MonitorAndCommand.Models;


namespace CodeAbility.MonitorAndCommand.WcfServiceLibrary
{
    public class MessageStore
    {
        public static void Insert(Message message)
        {
            const string CommandName = "SP_Message_Insert";

            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["eTools"].ConnectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = CommandName;

                    command.Parameters.AddWithValue("datetimeTimeStamp", message.Timestamp);
                    command.Parameters.AddWithValue("strContent", message.Parameter);

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
    }
}
