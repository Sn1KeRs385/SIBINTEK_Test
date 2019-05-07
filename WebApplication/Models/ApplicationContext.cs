using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace WebApplication.Models
{
    [DbConfigurationType(typeof(MySql.Data.Entity.MySqlEFConfiguration))]
    public class ApplicationContext : DbContext
    {
        public DbSet<Data> Data { get; set; }
        public DbSet<Lot> Lot { get; set; }
    }
}