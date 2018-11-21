using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Linq.Attributes
{
    class ForeignKeyAttribute : Attribute
    {
        //ToDo: Use this!!!
        public string Name { get; private set; }

        public string ForeignKexClassName { get; private set; }

        public string ForeignKexMemberName { get; private set; }

        public ForeignKeyAttribute(string foreignKeyClassName, string  foreignKeyMemberName, [CallerMemberName]string name = "")
        {
            this.Name = name;
            this.ForeignKexClassName = foreignKeyClassName;
            this.ForeignKexMemberName = foreignKeyMemberName;
        }
    }
}
