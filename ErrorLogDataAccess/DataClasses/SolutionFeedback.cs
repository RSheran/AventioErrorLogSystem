using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErrorLogDataAccess.DataClasses
{
    public class SolutionFeedback
    {
        public ObjectId _id { get; set; }

        public int SolutionFeedbackId { get; set; }

        public int UserId { get; set; }

        public int SolutionId { get; set; }

        public bool IsSolutionCorrect { get; set; }

        public string FeedBackComment { get; set; }

        public string FeedBackDate { get; set; }

        public DateTime lastModified { get; set; }

    }
}
