using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErrorLogDataAccess.DataClasses
{
    public partial class UserSuggestion
    {
        public int suggestionId { get; set; }
        public string reason { get; set; }
        public string suggestionComment { get; set; }
        public Nullable<int> userId { get; set; }
        public Nullable<System.DateTime> suggestionDate { get; set; }

        public virtual SystemUser SystemUser { get; set; }
    }
}
