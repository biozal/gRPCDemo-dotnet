using System;
using gRPCDemo.Models;

namespace gRPCDemo.Services
{
    public class CouchbaseConfigService
    {
        private readonly IConfiguration _configuration;

        public CouchbaseConfig Config { get; private set; }

        public CouchbaseConfigService(
	        IConfiguration configuration)
        {
            _configuration = configuration;
            Config = new CouchbaseConfig();
        }

        public void InitConfig() 
	    {
            Config.BucketName = _configuration.GetValue<string>("CBBucketName");
            Config.CollectionName = _configuration.GetValue<string>("CBCollectionName");
            Config.ConnectionString = _configuration.GetValue<string>("CBConnectionString");
            Config.Password = _configuration.GetValue<string>("CBPassword");
            Config.ScopeName = _configuration.GetValue<string>("CBScopeName");
            Config.Username = _configuration.GetValue<string>("CBUsername");
        }
    }
}

