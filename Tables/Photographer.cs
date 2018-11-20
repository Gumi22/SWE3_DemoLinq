using System;
using System.Collections.Generic;
using System.Text;
using Linq.Attributes;

namespace Linq
{
    [Table("photographer")]
    class Photographer
    {
        [PrimaryKey, Column("id")]
        public int Id { get; set; }
        [Column("name")]
        public string Name { get; set; }
        [Column("surname")]
        public string SurName { get; set; }
        [Column("birthdate")]
        public DateTime BirthDate { get; set; }
        [Column("notes")]
        public string Notes { get; set; }

    }
}
