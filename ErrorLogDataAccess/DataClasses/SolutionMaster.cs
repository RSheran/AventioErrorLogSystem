using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErrorLogDataAccess.DataClasses
{
    public partial class SolutionMaster
    {

        public ObjectId _id { get; set; }

        public int SolutionId { get; set; }

        public string SolutionCode { get; set; }
        public Nullable<int> ErrorId { get; set; }

        public Nullable<int> DomainId { get; set; }

        public Nullable<int> FieldId { get; set; }

        public Nullable<int> ClientId { get; set; }
        public Nullable<int> UserId { get; set; }
        public string SolutionComment { get; set; }
        public string SolutionLogDate { get; set; }

        public DateTime lastModified { get; set; }

        public  List<ErrorAttachment> ErrorAttachments { get; set; }
        public  List<SolutionFeedback> SolutionFeedbackList { get; set; }
    }
}
