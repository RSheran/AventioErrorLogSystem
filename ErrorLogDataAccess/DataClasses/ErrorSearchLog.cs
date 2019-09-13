using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErrorLogDataAccess.DataClasses
{
    public partial class ErrorSearchLog
    {
        public int errorSearchLogId { get; set; }
        public Nullable<int> errorId { get; set; }
        public Nullable<int> userId { get; set; }
        public Nullable<System.DateTime> logDate { get; set; }

        public virtual ErrorMaster ErrorMaster { get; set; }
        public virtual SystemUser SystemUser { get; set; }
    }
}
