using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErrorLogDataAccess.DataClasses
{
    public partial class ErrorMaster
    {
        public ObjectId _id { get; set; }

        public int ErrorId { get; set; }

        public string ErrorCode { get; set; }
        public Nullable<int> DomainId { get; set; }
        public Nullable<int> FieldId { get; set; }

        public Nullable<int> ClientId { get; set; }
        public string ErrorCaption { get; set; }
        public string ErrorDescription { get; set; }
        public Nullable<int> UserId { get; set; }
        public string ErrorLogDate { get; set; }

        public string LastUpdatedDate { get; set; }


        public DateTime lastModified { get; set; }

        public List<ErrorAttachment> ErrorAttachments { get; set; }
        public List<ErrorSearchLog> ErrorSearchLogs { get; set; }
        public List<SolutionMaster> SolutionMasters { get; set; }


    }
}
