using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Linq.Attributes
{
    class TableAttribute : Attribute
    {
        public string Name { get; set; }

        public TableAttribute(string name = "")
        {
            this.Name = name;
        }
    }
}
