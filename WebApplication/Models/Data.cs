using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication.Models
{
    [Table("Data")]
    public class Data
    {
        public int id { get; set; }
        public int notificationNumber { get; set; }
        public string fio { get; set; }
        public string notificationType { get; set; }
        public string organizationName { get; set; }
        public string description { get; set; }
        public string inn { get; set; }
        public string kpp { get; set; }
        public string bankName { get; set; }
        public string bik { get; set; }
        public string rs { get; set; }
        public string ks { get; set; }
    }
}