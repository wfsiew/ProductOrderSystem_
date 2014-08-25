using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using ProductOrderSystem.Domain.Models;

namespace ProductOrderSystem.Domain.Models.Fibre
{
    public class Order_Fibre
    {
        public Order_Fibre()
        {
            OrderFiles = new List<OrderFile_Fibre>();
        }

        public int ID { get; set; }

        [Required]
        public int SalesPersonID { get; set; }

        [Required]
        public int OrderTypeID { get; set; }

        [Required]
        public int StatusSC { get; set; }

        [Required]
        public DateTime ReceivedDatetime { get; set; }

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

        [Required]
        public bool IsWithdrawFixedLineReq { get; set; }

        [Required]
        public bool IsServiceUpgrade { get; set; }

        [MaxLength(200)]
        public string ReasonWithdraw { get; set; }

        [MaxLength(200)]
        public string Comments { get; set; }

        public int? StatusCC { get; set; }

        [MaxLength(200)]
        public string ReasonRejectCC { get; set; }

        [MaxLength(200)]
        public string RemarksCC { get; set; }
        public int? StatusFL { get; set; }

        [MaxLength(200)]
        public string ReasonRejectFL { get; set; }

        [MaxLength(30)]
        public string AllocatedFixedLineNo { get; set; }

        [MaxLength(200)]
        public string RemarksFL { get; set; }
        public int? StatusAC { get; set; }

        [MaxLength(200)]
        public string ReasonRejectAC { get; set; }

        [MaxLength(200)]
        public string RemarksAC { get; set; }

        [Required]
        public bool IsFormReceived { get; set; }

        public int? StatusInstall { get; set; }

        [MaxLength(200)]
        public string ReasonRejectInstall { get; set; }

        public DateTime? InstallDatetime { get; set; }

        [MaxLength(30)]
        public string BTUID { get; set; }

        [MaxLength(30)]
        public string BTUInstalled { get; set; }

        [MaxLength(30)]
        public string SIPPort { get; set; }

        [Required]
        public bool IsKIV { get; set; }

        [Required]
        public bool IsBTUInstalled { get; set; }

        [Required]
        public DateTime CreateDatetime { get; set; }

        [Required]
        public DateTime LastUpdateDatetime { get; set; }

        [Required]
        public int UserID { get; set; }

        public int? ActionTypeID { get; set; }

        public DateTime? LastUpdateDatetimeCC { get; set; }

        public int? CCUserID { get; set; }

        public DateTime? LastUpdateDatetimeFL { get; set; }

        public int? FLUserID { get; set; }

        public DateTime? LastUpdateDatetimeAC { get; set; }

        public int? ACUserID { get; set; }

        public DateTime? LastUpdateDatetimeInstall { get; set; }

        public int? InstallUserID { get; set; }

        [Required]
        public DateTime LastActionDatetime { get; set; }

        public int? KIVRoleID { get; set; }
        public int? KIVActionTypeID { get; set; }

        public virtual User SalesPerson { get; set; }
        public virtual OrderType OrderType { get; set; }
        public virtual User User { get; set; }
        public virtual User CCUser { get; set; }
        public virtual User FLUser { get; set; }
        public virtual User ACUser { get; set; }
        public virtual User InstallUser { get; set; }
        public virtual ICollection<OrderFile_Fibre> OrderFiles { get; set; }
    }

    public class OrderType
    {
        public int ID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
    }

    public class OrderFile_Fibre
    {
        public int ID { get; set; }

        [Required]
        public int OrderID { get; set; }

        [Required]
        public string FileName { get; set; }

        public int FileSize { get; set; }
        public string GoogleFileID { get; set; }
        public string GoogleFileUrl { get; set; }
        public string FileUniqueKey { get; set; }

        [Required]
        public DateTime UploadDatetime { get; set; }

        public virtual Order_Fibre Order { get; set; }
    }

    public class StatusType
    {
        public int ID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
    }

    public class ActionType_Fibre
    {
        public int ID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public int MaxDue { get; set; }
    }

    public class OrderAudit_Fibre
    {
        public Guid ID { get; set; }

        [Required]
        public int OrderID { get; set; }

        [Required]
        [MaxLength(100)]
        public string CustName { get; set; }

        [Required]
        public int OrderTypeID { get; set; }

        [Required]
        public DateTime ActionDatetime { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        public int SalesPersonID { get; set; }

        [Required]
        public int Status { get; set; }

        [Required]
        [MaxLength(100)]
        public string OverallStatus { get; set; }

        [Required]
        public bool IsInstallDateChange { get; set; }

        [Required]
        public bool IsInstallationPenalty { get; set; }

        public virtual User SalesPerson { get; set; }
        public virtual OrderType OrderType { get; set; }
        public virtual User User { get; set; }
    }
}
