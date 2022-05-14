using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Setre.DataAccess.Entities
{
    [Keyless]
    public  class OrderDetail
    {
  

        public decimal UnitPrice { get; set; }

        public int Quantity { get; set; }

        //Relation(Lazy Loading)

        public int ProductID { get; set; }

        public virtual Product Product { get; set; }

        public int OrderID { get; set; }

        public virtual Order Order { get; set; }
    }
}
