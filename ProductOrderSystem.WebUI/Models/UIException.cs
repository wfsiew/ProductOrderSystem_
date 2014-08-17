using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProductOrderSystem.WebUI.Models
{
    public class UIException : Exception
    {
        public UIException(string m) : base(m)
        {
        }
    }
}