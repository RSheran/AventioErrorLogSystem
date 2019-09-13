using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErrorLogDataAccess.DataClasses
{
    public class UserMessage
    {
        public ObjectId _id { get; set; }

        public int UserMessageId { get; set; }

        public int ParentUserMessageId { get; set; }

        public string MessageSubject { get; set; }

        public string MessageBody { get; set; }

        public bool IsMessageRead { get; set; }

        public Nullable<int> SentUserId { get; set; }

        public Nullable<int> ReceivedUserId { get; set; }

        public Boolean IsDeleted { get; set; }
        public string LastUpdatedDate { get; set; }
        public DateTime MessageSentDate { get; set; }

        public string MessageDeliveryDate { get; set; }
        
        public DateTime lastModified { get; set; }

        
    }
}
