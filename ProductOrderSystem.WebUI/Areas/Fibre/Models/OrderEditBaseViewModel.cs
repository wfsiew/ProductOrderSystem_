using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProductOrderSystem.WebUI.Areas.Fibre.Models
{
    public class OrderEditBaseViewModel
    {
        public int ID { get; set; }
        public string SalesPerson { get; set; }
        public string OrderType { get; set; }
        public int OrderTypeID { get; set; }
        public string StatusSC { get; set; }
        public DateTime ReceivedDatetime { get; set; }
        public string CustID { get; set; }
        public string CustName { get; set; }
        public string CustAddr { get; set; }
        public string ContactPerson { get; set; }
        public string ContactPersonNo { get; set; }
        public bool IsCoverageAvailable { get; set; }
        public bool IsDemandList { get; set; }
        public bool IsReqFixedLine { get; set; }
        public bool IsCeoApproved { get; set; }
        public bool IsWithdrawFixedLineReq { get; set; }
        public bool IsServiceUpgrade { get; set; }
        public string ReasonWithdraw { get; set; }
        public string Comments { get; set; }
        public DateTime? BookedInstallDatetime { get; set; }
    }
}