using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErrorLogDataAccess.DataClasses
{
    public class ErrorFrequency
    {
        public ObjectId _id { get; set; }
        public int ErrorFrequencyId { get; set; }
               
        public string ErrorCode { get; set; }
        public int SearchFrequency { get; set; }
       
        public string LastUpdatedDate { get; set; }
        public DateTime lastModified { get; set; }
    }
}
