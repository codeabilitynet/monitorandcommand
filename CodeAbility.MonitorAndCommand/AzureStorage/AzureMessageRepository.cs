using System;
using System.Configuration;
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
    public class AzureMessageRepository : IMessageRepository
    {        
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

        public IEnumerable<Models.Message> ListLastMessages(int numberOfMessages)
        {
            int startRowKey = RowKey - numberOfMessages;

            List<Models.Message> messages = new List<Models.Message>();

            IEnumerable<MessageEntity> entities = PartitionRangeQueryAsync(table, PartitionKey, startRowKey.ToString(), RowKey.ToString()).Result;

            foreach (MessageEntity entity in entities)
            {
                messages.Add(new Models.Message(entity.SendingDevice, entity.FromDevice, entity.ToDevice, entity.ContentType, entity.Name, entity.Parameter, entity.Content));
            }

            return messages;
        }

        public IEnumerable<Models.Message> ListLastMessages(int numberOfMessages, string deviceName, string objectName, string parameterName, int rowInterval)
        {
            throw new NotImplementedException();
        }

        public void Purge()
        {
            DeleteTableAsync(table).Wait();
        }

        #endregion 

        #region Private methods

        private async Task<CloudTable> CreateTableAsync()
        {
            CloudStorageAccount storageAccount = CreateStorageAccountFromConnectionString(ConfigurationManager.AppSettings["StorageConnectionString"].ToString());

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            CloudTable table = tableClient.GetTableReference(TableName);
            try
            {
                //BUG : the following method hangs forever, even though it created the table...
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

        private CloudStorageAccount CreateStorageAccountFromConnectionString(string storageConnectionString)
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

        private async Task<MessageEntity> InsertOrMergeEntityAsync(CloudTable table, MessageEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(entity);

            TableResult result = await table.ExecuteAsync(insertOrMergeOperation);
            MessageEntity insertedMessage = result.Result as MessageEntity;
            return insertedMessage;
        }

        private async Task<IEnumerable<MessageEntity>> PartitionRangeQueryAsync(CloudTable table, string partitionKey, string startRowKey, string endRowKey)
        {
            List<MessageEntity> entities = new List<MessageEntity>();

            TableQuery<MessageEntity> rangeQuery = new TableQuery<MessageEntity>().Where(
                TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey),
                        TableOperators.And,
                        TableQuery.CombineFilters(
                            TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, startRowKey),
                            TableOperators.And,
                            TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThanOrEqual, endRowKey))));

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

        private async Task DeleteTableAsync(CloudTable table)
        {
            await table.DeleteIfExistsAsync();
        }

        #endregion 
    }
}
