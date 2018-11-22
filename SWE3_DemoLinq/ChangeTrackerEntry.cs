using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Linq
{
    public class ChangeTrackerEntry
    {
        /// <summary>
        /// The current state ob this entry
        /// </summary>
        public States State { get; set; }

        /// <summary>
        /// the item for which this entry is tracking the changes
        /// </summary>
        public object Item { get; set; }

        /// <summary>
        /// A list of the Members of the object and the value that were stored
        /// </summary>
        public List<Pair<PropertyInfo, object>> Originals { get; set; }

        public ChangeTrackerEntry(object obj) : this(obj, States.Unmodified)
        {
        }

        /// <summary>
        /// creates new ChangeTrackerEntry with given values
        /// </summary>
        /// <param name="obj">The object to be tracked</param>
        /// <param name="state">the state in which it is initially in</param>
        public ChangeTrackerEntry(object obj, States state)
        {
            Item = obj ?? throw new ArgumentNullException(nameof(obj), "Cannot track changes of null object");
            Originals = new List<Pair<PropertyInfo, object>>();

            //ToDo: Use Helper class for these things
            foreach (var x in obj.GetType().GetProperties()) 
            {
                Originals.Add(new Pair<PropertyInfo, object>(x, x.GetValue(obj)));
            }

            if (Originals.Count == 0)
            {
                throw new ArgumentNullException(nameof(obj), "Cannot track changes of object without properties");
            }

            State = state;
        }

        /// <summary>
        /// checks values and sets the state of this Entry
        /// </summary>
        public void ComputeState()
        {
            if (State == States.Deleted || State == States.Added)
            {
                //if deleted or added, don't change state
                return;
            }

            foreach (var originalValue in Originals)
            {
                if (!originalValue.Item2?.Equals(originalValue.Item1.GetValue(Item)) ??
                    !originalValue.Item1?.GetValue(Item)?.Equals(originalValue.Item2) ??
                    !(originalValue.Item2 == null && originalValue.Item1?.GetValue(Item) == null))
                {
                    State = States.Modified;
                    return;
                }
            }
        }

        public void SetUnmodified()
        {
            foreach (var originalValue in Originals)
            {
                originalValue.Item2 = originalValue.Item1.GetValue(Item);
            }

            State = States.Unmodified;
        }

        /// <summary>
        /// States that an Table-Object can have
        /// </summary>
        public enum States
        {
            Unmodified = 0,
            Modified,
            Added,
            Deleted
        }

        /// <summary>
        /// Represents Pair of Values, that may be changed anytime
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        public class Pair<T1, T2>
        {
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