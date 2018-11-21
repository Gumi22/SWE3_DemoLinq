using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Linq.Attributes
{
    class PrimaryKeyAttribute : Attribute
    {
        public string Name { get; set; }

        public PrimaryKeyAttribute([CallerMemberName]string name = "")
        {
            this.Name = name;
        }
    }
}
