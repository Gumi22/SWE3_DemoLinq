using System;
using System.Collections.Generic;
using System.Text;
using Linq.Attributes;

namespace Linq
{
    [Table("camera")]
    class Camera
    {
        [PrimaryKey, Column("id")]
        public int Id { get; set; }
        [Column("producer")]
        public string Producer { get; set; }
        [Column("model")]
        public string Model { get; set; }
        [Column("purchasedate")]
        public DateTime PurchaseDate { get; set; }
        [Column("notes")]
        public string Notes { get; set; }
        [Column("isolimitgood")]
        public double IsoLimitGood { get; set; }
        [Column("isolimitacceptable")]
        public double IsoLimitAcceptable { get; set; }

    }
}
