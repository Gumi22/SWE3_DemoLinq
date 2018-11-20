using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Linq
{
    class ChangeTrackerEntry
    {

        public States State { get; set; }

        public object Item { get; set; }

        public List<Pair<PropertyInfo, object>> Originals { get; set; }

        public ChangeTrackerEntry(){}

        public ChangeTrackerEntry(object obj) : this(obj, States.Unmodified){}

        public ChangeTrackerEntry(object obj, States state)
        {
            Item = obj;
            Originals = new List<Pair<PropertyInfo, object>>();
            foreach (var x in obj.GetType().GetProperties())
            {
                Originals.Add(new Pair<PropertyInfo, object>(x, x.GetValue(obj)));
            }

            State = state;
        }

        public void ComputeState()
        {
            if (State == States.Deleted || State == States.Added) //if deleted or added, don't change state
            {
                return;
            }
            if (Item != null)
            {
                if (Originals.Count > 0)
                {
                    foreach (var originalValue in Originals)
                    {
                        if (!originalValue.Item2.Equals(originalValue.Item1.GetValue(Item)))
                        {
                            State = States.Modified;
                            return;
                        }
                    }
                    return;
                }
                else
                {
                    State = States.Added;
                    return;
                }
            }
            State = States.Deleted;
            return;
        }

        public enum States
        {
            Unmodified = 0,
            Modified,
            Added,
            Deleted
        }
        public class Pair<T1, T2>
        {
            public Pair() { }

            public Pair(T1 item1, T2 item2)
            {
                Item1 = item1;
                Item2 = item2;
            }

            public T1 Item1 { get; set; }
            public T2 Item2 { get; set; }
        }
    }
}
