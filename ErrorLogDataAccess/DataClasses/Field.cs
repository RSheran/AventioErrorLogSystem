using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErrorLogDataAccess.DataClasses
{
    public partial class Field
    {

        public ObjectId _id { get; set; }
        public int FieldId { get; set; }
        public string FieldCode { get; set; }
        public string FieldName { get; set; }
        public string FieldDescription { get; set; }
        public string EntryDate { get; set; }
        public int UserId { get; set; }

        public string LastUpdatedDate { get; set; }

        public bool IsActive { get; set; }
        public DateTime lastModified { get; set; }

    }
}
