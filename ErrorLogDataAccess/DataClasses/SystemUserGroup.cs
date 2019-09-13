using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErrorLogDataAccess.DataClasses
{
    public partial class SystemUserGroup
    {

        public ObjectId _id { get; set; }

        public int UserGroupId { get; set; }

        public string UserGroupName { get; set; }
        public string UserGroupDescription { get; set; }

        public bool IsActive { get; set; }

        public string CreatedDate { get; set; }

        public string LastUpdatedDate { get; set; }

        public int CreatedUserId { get; set; }


        public DateTime lastModified { get; set; }


    }
}
