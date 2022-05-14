using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Setre.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Setre.DataAccess.Context
{
    public class SetreDbContext: IdentityDbContext<User>
    {
        public SetreDbContext(DbContextOptions<SetreDbContext> options) :base(options)
        {

        }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Product> Products { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<OrderDetail> OrderDetails { get; set; }

        public DbSet<Supplier> Suppliers { get; set; }
    }
}
