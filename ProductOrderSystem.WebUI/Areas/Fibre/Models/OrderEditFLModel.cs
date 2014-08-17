using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ProductOrderSystem.WebUI.Areas.Fibre.Models
{
    public class OrderEditFLModel
    {
        [Required]
        public int ID { get; set; }

        [Required]
        public int StatusFL { get; set; }

        [MaxLength(200)]
        public string ReasonRejectFL { get; set; }

        [MaxLength(30)]
        public string AllocatedFixedLineNo { get; set; }

        [MaxLength(200)]
        public string RemarksFL { get; set; }
    }
}