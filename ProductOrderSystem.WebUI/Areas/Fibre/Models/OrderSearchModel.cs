using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ProductOrderSystem.WebUI.Areas.Fibre.Models
{
    public class OrderSearchModel
    {
        public int? Page { get; set; }
        public string Sort { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = false)]
        public DateTime? DateFrom { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = false)]
        public DateTime? DateTo { get; set; }

        public string CustName { get; set; }
        public int? OrderID { get; set; }
        public string CustID { get; set; }
        public int? SalesPersonID { get; set; }
        public string CustAddr { get; set; }
        public int? Status { get; set; }
        public int? OrderTypeID { get; set; }
        public bool IsDemandList { get; set; }
    }
}