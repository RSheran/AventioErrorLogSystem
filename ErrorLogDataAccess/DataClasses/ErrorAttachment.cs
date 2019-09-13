using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErrorLogDataAccess.DataClasses
{
    public  partial class ErrorAttachment
    {
        public ObjectId _id { get; set; }

        public int AttachmentId { get; set; }
              
        public Nullable<int> ErrorId { get; set; }

        public Nullable<int> ClientId { get; set; }
        public Nullable<int> SolutionId { get; set; }

        public DateTime lastModified { get; set; }

        //To identify whether it is the attachment or error or solution
        public bool IsErrorAttachment { get; set; }
        public string MediaType { get; set; }
        public string MediaName { get; set; }
        public string MediaURL { get; set; }

        public string MediaMIMEType { get; set; }

        public string MediaPath { get; set; }

    }
}
