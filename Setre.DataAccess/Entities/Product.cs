using Setre.DataAccess.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Setre.DataAccess.Entities
{
    public class Product
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductID { get; set; }

        [CustomSearchPropertyAttribute]
        public string ProductName { get; set; }

        public string QuantityPerUnit { get; set; }

        public decimal UnitPrice { get; set; }

        public byte UnitsInStock { get; set; }

        public byte UnitsOnOrder { get; set; }

        public byte ReorderLevel { get; set; }

        public bool Discontinued { get; set; }

        //Relation(Lazy Loading)

        public int SupplierID { get; set; }

        public virtual  Supplier Supplier { get; set; }

        public int CategoryID { get; set; }

        public virtual  Category Category { get; set; }

    }
}
