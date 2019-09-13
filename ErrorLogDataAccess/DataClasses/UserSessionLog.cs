using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErrorLogDataAccess.DataClasses
{
    public class UserSessionLog
    {
        public ObjectId _id { get; set; }

        public int UserSessionLogId { get; set; }
        public int UserId { get; set; }

        public string IPAddress { get; set; }

        public string CountryCode { get; set; }

        public string Country { get; set; }


        public string City { get; set; }

        public string Region { get; set; }

        public string LoggedInTimestamp { get; set; }

        public string LoggedOffTimestamp { get; set; }

        public DateTime lastModified { get; set; }


    }

    public class UserSessionLogDTO
    {
       
        public int UserSessionLogId { get; set; }
        public int UserId { get; set; }

        public string ProfilePicURL { get; set; }

        public string Username { get; set; }

        public string UserCallingName { get; set; }

        public string UserFullName { get; set; }

        public string UserGroupName { get; set; }

        public string IPAddress { get; set; }

        public string CountryCode { get; set; }

        public string Country { get; set; }


        public string City { get; set; }

        public string  Region { get; set; }


        public string LoggedInTimestamp { get; set; }

        public string LoggedOffTimestamp { get; set; }

        

    }
}
