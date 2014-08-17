using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ProductOrderSystem.WebUI.Areas.Fibre.Models
{
    public class OrderEditACModel
    {
        [Required]
        public int ID { get; set; }

        [Required]
        public int StatusAC { get; set; }

        [MaxLength(200)]
        public string ReasonRejectAC { get; set; }

        [MaxLength(200)]
        public string RemarksAC { get; set; }

        [Required]
        public bool IsFormReceived { get; set; }
    }
}