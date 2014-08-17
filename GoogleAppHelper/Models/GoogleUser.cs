using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GoogleAppHelper.Models
{
    public class GoogleUser
    {
        public string Name { get; set; }
        public string UserName { get; set; }

        public string Domain { get; set; }
        //Google API seem cannot get user email, hard code first.
        public string UserEmail
        {
            get
            {
                return string.Format("{0}@{1}", UserName, Domain);
            }
        }
    }
}
