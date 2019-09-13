using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErrorLogDataAccess.DataClasses
{
    public partial class PasswordChangeRequest
    {
            public ObjectId _id { get; set; }

            public int UserId { get; set; }
            public string TempPassword { get; set; }
            public string PasswordRequestedTime { get; set; }
            public DateTime lastModified { get; set; }
            

    }
}
