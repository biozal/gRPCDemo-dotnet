using System;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.Query;
using Grpc.Core;
using gRPCDemo.Models;
using gRPCDemo.Providers;

namespace gRPCDemo.Services
{
    public class QuestsService :
        QuestService.QuestServiceBase 
    {
        private readonly CouchbaseConfig _config;
        private readonly IDemoGameBucketProvider _bucketProvider;
        private readonly IClusterProvider _clusterProvider;

        public QuestsService(
            CouchbaseConfigService configService,
            IClusterProvider clusterProvider,
	        IDemoGameBucketProvider bucketProvider)
        {
            _config = configService.Config;
            _clusterProvider = clusterProvider;
            _bucketProvider = bucketProvider;
        }

        public override async Task<Quest> GetQuest(
	        DocumentRequest request, 
	        ServerCallContext context)
        {
            Couchbase.KeyValue.ICouchbaseCollection collection = await GetCollection();
            var document = await collection.GetAsync(request.DocumentId);
            var quest = document.ContentAs<Quest>();
            return (quest is not null) ? quest : new Quest();
        }

        public override async Task<Quests> GetQuests(
            QuestsRequest request,
            ServerCallContext context)
        {
	        var quests = new Quests();
            //assume index is created on the server or this will go very poor
            //if running slow, check this index to make sure it's created
            //CREATE INDEX document_type_idx on `demogame` (documentType, isActive)

            //validate we can see the proper config
            if (_clusterProvider is not null
                && _config is not null
                && _config.BucketName is not null)
            {
                //query the database and get items back
                var cluster = await _clusterProvider.GetClusterAsync();
                var query = GetQuestQuery(request.ActiveOnly); 
		        var results = await cluster
		            .QueryAsync<Quest>(query, GetQueryOptions())
		            .ConfigureAwait(false);

		        if (results is not null 
		            && results?.MetaData?.Status == QueryStatus.Success)
		        {
                    if (results.MetaData.Metrics is not null && results.MetaData.Metrics.ExecutionTime is not null)
                    {
                        quests.ElapsedTime = results.MetaData.Metrics.ElapsedTime;
                        quests.ExecutionTime = results.MetaData.Metrics.ExecutionTime;
                    }
		            var questList = await results.Rows.ToListAsync<Quest>();
		            if (questList is not null && questList.Count > 0)
			        {
			            quests.Quests_.AddRange(questList);
			        }
		        }
            }
		    return quests; 
        }

        public override async Task<SetResponse> SetQuest( 
	        Quest request, 
	        ServerCallContext context)
        {
            try 
	        {
                var collection = await GetCollection(); 
                await collection.InsertAsync<Quest>(request.DocumentId, request);
            }
            catch (Exception ex) 
	        {
                return new SetResponse
                {
                    IsError = true,
                    Message = $"{ex.Message} {ex.StackTrace}"
                };
	        }
            return new SetResponse
            {
                IsError = false,
                Message = String.Empty
            };
        }

        private QueryOptions GetQueryOptions()
        {
            var queryOptions = new QueryOptions().Metrics(true);
            queryOptions.Readonly(true);

            return queryOptions;
        }

        private string GetQuestQuery(bool isActive) 
	    {
            var sb = new System.Text.StringBuilder("SELECT a.* FROM ");
            sb.Append($"{_config.BucketName} a ");
            sb.Append("WHERE a.documentType = 'quest' ");
            sb.Append("AND ");
            sb.Append($"a.isActive = {isActive} ");
            return sb.ToString();
        }

        private async Task<Couchbase.KeyValue.ICouchbaseCollection> GetCollection()
        {
            var bucket = await _bucketProvider.GetBucketAsync();
            var collectionName = (_config.CollectionName is not null) ? _config.CollectionName : "_default";
            var collection = await bucket.CollectionAsync(collectionName);

            return collection;
        }
    }
}

