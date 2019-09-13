using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErrorLogDataAccess.DataClasses
{
    public class Client
    {

        public ObjectId _id { get; set; }
        public int ClientId { get; set; }
        public string ClientCode { get; set; }
        public string ClientName { get; set; }
        public string ClientDescription { get; set; }

        public bool IsActive { get; set; }
        public string EntryDate { get; set; }
        public string LastUpdatedDate { get; set; }
        public DateTime lastModified { get; set; }
    }
}
