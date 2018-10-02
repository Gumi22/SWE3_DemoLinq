using System;
using System.Collections.Generic;
using System.Text;

namespace Linq
{
    class MyTable
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }

        public override string ToString()
        {
            return $"{FirstName} {LastName} {Age}";
        }
    }
}
