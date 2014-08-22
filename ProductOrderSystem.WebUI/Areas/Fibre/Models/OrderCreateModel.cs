using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ProductOrderSystem.WebUI.Areas.Fibre.Models
{
    public class OrderCreateModel
    {
        [Required]
        public int SalesPersonID { get; set; }

        [Required]
        public int OrderTypeID { get; set; }

        [Required]
        public int StatusSC { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime ReceivedDate { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public DateTime ReceivedTime { get; set; }

        [MaxLength(10)]
        public string CustID { get; set; }

        [Required]
        [MaxLength(100)]
        public string CustName { get; set; }

        [Required]
        [MaxLength(200)]
        public string CustAddr { get; set; }

        [Required]
        [MaxLength(100)]
        public string ContactPerson { get; set; }

        [Required]
        [MaxLength(50)]
        public string ContactPersonNo { get; set; }

        [Required]
        public bool IsCoverageAvailable { get; set; }

        [Required]
        public bool IsDemandList { get; set; }

        [Required]
        public bool IsReqFixedLine { get; set; }

        [Required]
        public bool IsCeoApproved { get; set; }
        public bool IsWithdrawFixedLineReq { get; set; }
        public bool IsServiceUpgrade { get; set; }

        [MaxLength(200)]
        public string ReasonWithdraw { get; set; }

        [MaxLength(200)]
        public string Comments { get; set; }

        [DataType(DataType.Date)]
        public DateTime? BookedInstallDate { get; set; }

        [DataType(DataType.Time)]
        public DateTime? BookedInstallTime { get; set; }

        [Required]
        public bool IsKIV { get; set; }

        [Required]
        public bool IsBTUInstalled { get; set; }
    }
}