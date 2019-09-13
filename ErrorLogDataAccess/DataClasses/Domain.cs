using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErrorLogDataAccess.DataClasses
{
  
        public partial class Domain
        {           
            public ObjectId _id { get; set; }
            public int DomainId { get; set; }
            public string DomainCode { get; set; }
            public string DomainName { get; set; }
            public string DomainDescription { get; set; }

             public string EntryDate { get; set; }
             public int UserId { get; set; }

             public string LastUpdatedDate { get; set; }
        
             public bool IsActive { get; set; }
             public DateTime lastModified { get; set; }

      
        }
   
}
