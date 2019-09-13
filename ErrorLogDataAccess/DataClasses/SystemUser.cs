using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErrorLogDataAccess.DataClasses
{
    public partial class SystemUser
    {

        public ObjectId _id { get; set; }

        public int UserId { get; set; }
        public Nullable<int> UserGroupId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public string TempPassword { get; set; }

        public string CallingName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string ProfilePicURL { get; set; }
        public bool IsActive { get; set; }

        public string UserGroupName { get; set; }

        public string CreatedDate { get; set; }

        public string LastUpdatedDate { get; set; }

        public int LastUpdatedUserId { get; set; }

        public bool IsLoggedIn{ get; set; }

        public DateTime lastModified { get; set; }




    }

    public partial class SystemUserDTO
    {
             
        public int UserId { get; set; }
        public Nullable<int> UserGroupId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public string TempPassword { get; set; }

        public string CallingName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string ProfilePicURL { get; set; }
        public bool IsActive { get; set; }

        public string UserGroupName { get; set; }

        public string CreatedDate { get; set; }

        public string LastUpdatedDate { get; set; }

        public int LastUpdatedUserId { get; set; }

        public bool IsLoggedIn { get; set; }

        public string LastLogInTime { get; set; }

        public string LastLogOffTime { get; set; }

     




    }
}
