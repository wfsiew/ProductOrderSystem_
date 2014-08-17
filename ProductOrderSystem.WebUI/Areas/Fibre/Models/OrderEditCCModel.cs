using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ProductOrderSystem.WebUI.Areas.Fibre.Models
{
    public class OrderEditCCModel
    {
        [Required]
        public int ID { get; set; }

        [Required]
        public int StatusCC { get; set; }

        [MaxLength(200)]
        public string ReasonRejectCC { get; set; }

        [MaxLength(200)]
        public string RemarksCC { get; set; }
    }
}