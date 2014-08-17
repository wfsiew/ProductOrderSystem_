using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ProductOrderSystem.WebUI.Areas.Fibre.Models
{
    public class OrderEditSCModel : OrderCreateModel
    {
        [Required]
        public int ID { get; set; }
    }
}