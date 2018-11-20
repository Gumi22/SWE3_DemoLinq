using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Linq.Attributes
{
    class ColumnAttribute : Attribute
    {

        public string Name { get; set; }

        public ColumnAttribute([CallerMemberName]string name = "")
        {
            this.Name = name;
        }
    }
}
