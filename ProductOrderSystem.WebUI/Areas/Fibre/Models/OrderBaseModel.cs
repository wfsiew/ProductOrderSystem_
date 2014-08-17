using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using ProductOrderSystem.Domain.Models;

namespace ProductOrderSystem.WebUI.Areas.Fibre.Models
{
    public class OrderBaseModel
    {
        [Required]
        public int SalesPersonID { get; set; }

        [Required]
        public int OrderTypeID { get; set; }

        [Required]
        public byte StatusSC { get; set; }

        [Required]
        public DateTime ReceivedDatetime { get; set; }

        [MaxLength(10)]
        public string CustID { get; set; }

        [Required]
        [MaxLength(100)]
        public string CustName { get; set; }

        [Required]
        [MaxLength(100)]
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
        public string ReasonWithdraw { get; set; }
        public string Comments { get; set; }
        public DateTime BookedInstallDatetime { get; set; }
        public byte StatusCC { get; set; }
        public string ReasonRejectCC { get; set; }
        public string RemarksCC { get; set; }
        public byte StatusFL { get; set; }
        public string ReasonRejectFL { get; set; }
        public string AllocatedFixedLineNo { get; set; }
        public byte StatusAC { get; set; }
        public string ReasonRejectAC { get; set; }
        public string RemarksAC { get; set; }
        public bool IsFormReceived { get; set; }
        public byte StatusInstall { get; set; }
        public string ReasonRejectInstall { get; set; }
        public DateTime InstallDatetime { get; set; }
        public string BTUID { get; set; }
        public string BTUInstalled { get; set; }
        public string SIPPort { get; set; }

        [Required]
        public DateTime CreateDatetime { get; set; }

        [Required]
        public DateTime LastUpdateDatetime { get; set; }

        [Required]
        public int UserID { get; set; }
    }
}