using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ProductOrderSystem.WebUI.Areas.Fibre.Models
{
    public class OrderEditInstallModel
    {
        [Required]
        public int ID { get; set; }

        [Required]
        public int StatusInstall { get; set; }

        [MaxLength(200)]
        public string ReasonRejectInstall { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime? InstallDate { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public DateTime? InstallTime { get; set; }

        [MaxLength(30)]
        public string BTUID { get; set; }

        [MaxLength(30)]
        public string BTUInstalled { get; set; }

        [MaxLength(30)]
        public string SIPPort { get; set; }
    }
}