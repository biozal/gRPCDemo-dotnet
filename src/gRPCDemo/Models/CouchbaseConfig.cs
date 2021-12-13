using System;
namespace gRPCDemo.Models
{
    public class CouchbaseConfig
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? ConnectionString { get; set; }
        public string? BucketName { get; set; }
        public string? ScopeName { get; set; }
        public string? CollectionName { get; set; }
    }
}

