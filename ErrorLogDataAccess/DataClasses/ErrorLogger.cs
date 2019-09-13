using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErrorLogDataAccess.DataClasses
{
    public class ErrorLogger
    {
        public ObjectId _id { get; set; }

        public int ErrorLoggerId { get; set; }
        public string TimeStamp { get; set; }
        public string Schema { get; set; }
        public string Username { get; set; }
        public string Exception_Error { get; set; }
    }
}
