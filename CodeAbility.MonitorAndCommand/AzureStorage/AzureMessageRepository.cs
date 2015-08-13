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
using System.Net;
using System.Threading.Tasks;

using Microsoft.Azure;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

using CodeAbility.MonitorAndCommand.Repository;
using CodeAbility.MonitorAndCommand.AzureStorage.Model;

namespace CodeAbility.MonitorAndCommand.AzureStorage
{
    /// <summary>
    /// Azure Table Service Sample - Demonstrates how to perform common tasks using the Microsoft Azure Table storage 
    /// including creating a table, CRUD operations, batch operations and different querying techniques. 
    /// 
    /// Note: This sample uses the .NET 4.5 asynchronous programming model to demonstrate how to call the Storage Service using the 
    /// storage client libraries asynchronous API's. When used in real applications this approach enables you to improve the 
    /// responsiveness of your application. Calls to the storage service are prefixed by the await keyword. 
    /// 
    /// Documentation References: 
    /// - What is a Storage Account - http://azure.microsoft.com/en-us/documentation/articles/storage-whatis-account/
    /// - Getting Started with Tables - http://azure.microsoft.com/en-us/documentation/articles/storage-dotnet-how-to-use-tables/
    /// - Table Service Concepts - http://msdn.microsoft.com/en-us/library/dd179463.aspx
    /// - Table Service REST API - http://msdn.microsoft.com/en-us/library/dd179423.aspx
    /// - Table Service C# API - http://go.microsoft.com/fwlink/?LinkID=398944
    /// - Storage Emulator - http://msdn.microsoft.com/en-us/library/azure/hh403989.aspx
    /// - Asynchronous Programming with Async and Await  - http://msdn.microsoft.com/en-us/library/hh191443.aspx
    /// </summary>

    public class AzureMessageRepository : IMessageRepository
    {
        // *************************************************************************************************************************
        // Instructions: This sample can be run using either the Azure Storage Emulator that installs as part of this SDK - or by
        // updating the App.Config file with your AccountName and Key. 
        // 
        // To run the sample using the Storage Emulator (default option)
        //      1. Start the Azure Storage Emulator (once only) by pressing the Start button or the Windows key and searching for it
        //         by typing "Azure Storage Emulator". Select it from the list of applications to start it.
        //      2. Set breakpoints and run the project using F10. 
        // 
        // To run the sample using the Storage Service
        //      1. Open the app.config file and comment out the connection string for the emulator (UseDevelopmentStorage=True) and
        //         uncomment the connection string for the storage service (AccountName=[]...)
        //      2. Create a Storage Account through the Azure Portal and provide your [AccountName] and [AccountKey] in 
        //         the App.Config file. See http://go.microsoft.com/fwlink/?LinkId=325277 for more information
        //      3. Set breakpoints and run the project using F10. 
        // 
        // *************************************************************************************************************************
        
        
        CloudTable table;
        internal const string TableName = "Message";

        internal const string PartitionKey = "MonitorAndCommand";
        internal int RowKey = 0;

        internal int ReturnedMessageNumber = 100;

        public AzureMessageRepository(string connectionString)
        {
            table = CreateTableAsync().Result;
        }

        #region Public Methods

        public void Insert(Models.Message message)
        {
            if (RowKey > Int32.MaxValue)
            {
                Purge();
                RowKey = 0;
            }

            ++RowKey;
            InsertOrMergeEntityAsync(table, new MessageEntity(PartitionKey, RowKey.ToString(), message)).Wait();
        }

        public IEnumerable<Models.Message> ListMessages()
        {
            int startRowKey = RowKey - ReturnedMessageNumber;

            return null;// PartitionRangeQueryAsync(table, PartitionKey, startRowKey.ToString(), RowKey.ToString()).Result;
        }

        public void Purge()
        {
            DeleteTableAsync(table).Wait();
        }

        #endregion 

        #region Private methods

        /// <summary>
        /// Create a table for the sample application to process messages in. 
        /// </summary>
        /// <returns>A CloudTable object</returns>
        private static async Task<CloudTable> CreateTableAsync()
        {
            // Retrieve storage account information from connection string.
            CloudStorageAccount storageAccount = CreateStorageAccountFromConnectionString(CloudConfigurationManager.GetSetting("StorageConnectionString"));

            // Create a table client for interacting with the table service
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create a table client for interacting with the table service 
            CloudTable table = tableClient.GetTableReference(TableName);
            try
            {
                if (await table.CreateIfNotExistsAsync())
                {
                    Console.WriteLine("Created Table named: {0}", TableName);
                }
                else
                {
                    Console.WriteLine("Table {0} already exists", TableName);
                }
            }
            catch (StorageException)
            {
                Console.WriteLine("If you are running with the default configuration please make sure you have started the storage emulator. Press the Windows key and type Azure Storage to select and run it from the list of applications - then restart the sample.");
                Console.ReadLine();
                throw;
            }

            return table;
        }

        private static CloudStorageAccount CreateStorageAccountFromConnectionString(string storageConnectionString)
        {
            CloudStorageAccount storageAccount;
            try
            {
                storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            }
            catch (FormatException)
            {
                Console.WriteLine("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the application.");
                throw;
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the sample.");
                Console.ReadLine();
                throw;
            }

            return storageAccount;
        }

        private static async Task<MessageEntity> InsertOrMergeEntityAsync(CloudTable table, MessageEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            // Create the InsertOrReplace  TableOperation
            TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(entity);

            // Execute the operation.
            TableResult result = await table.ExecuteAsync(insertOrMergeOperation);
            MessageEntity insertedMessage = result.Result as MessageEntity;
            return insertedMessage;
        }

        private static async Task<IEnumerable<MessageEntity>> PartitionRangeQueryAsync(CloudTable table, string partitionKey, string startRowKey, string endRowKey)
        {
            List<MessageEntity> entities = new List<MessageEntity>();

            // Create the range query using the fluid API 
            TableQuery<MessageEntity> rangeQuery = new TableQuery<MessageEntity>().Where(
                TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey),
                        TableOperators.And,
                        TableQuery.CombineFilters(
                            TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, startRowKey),
                            TableOperators.And,
                            TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThanOrEqual, endRowKey))));

            // Page through the results - requesting 100 results at a time from the server. 
            TableContinuationToken token = null;
            rangeQuery.TakeCount = 100;
            do
            {
                TableQuerySegment<MessageEntity> segment = await table.ExecuteQuerySegmentedAsync(rangeQuery, token);
                token = segment.ContinuationToken;
                foreach (MessageEntity entity in segment)
                {
                    entities.Add(entity);
                }
            }
            while (token != null);

            return entities;
        }

        private static async Task DeleteTableAsync(CloudTable table)
        {
            await table.DeleteIfExistsAsync();
        }

        #endregion 
    }
}
