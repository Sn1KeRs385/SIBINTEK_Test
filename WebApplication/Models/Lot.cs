using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication.Models
{
    [Table("Lots")]
    public class Lot
    {
        public int id { get; set; }
        public int notificationNumber { get; set; }
        public int lotNumber { get; set; }
        public string location { get; set; }
        public string description { get; set; }
    }
}