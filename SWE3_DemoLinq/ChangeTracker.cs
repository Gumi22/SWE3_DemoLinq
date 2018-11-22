using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Linq
{
    public class ChangeTracker : IChangeTracker
    {
        private static readonly List<ChangeTrackerEntry> _objects = new List<ChangeTrackerEntry>();

        /// <summary>
        /// Detects Objects that have been modified, added, or deleted
        /// and compiles them into a new List
        /// ComputeChanges should be called after this
        /// </summary>
        /// <returns>A list of ChangeTrackerEntries that were inserted, modified, or deleted</returns>
        public List<ChangeTrackerEntry> DetectChanges()
        {
            List<ChangeTrackerEntry> changed = new List<ChangeTrackerEntry>();

            foreach (var ctEntry in _objects)
            {
                ctEntry.ComputeState();
                if (ctEntry.State != ChangeTrackerEntry.States.Unmodified)
                {
                    changed.Add(ctEntry);
                }
            }

            return changed;
        }

        /// <summary>
        /// Inserts an object to be tracked
        /// </summary>
        /// <param name="obj">the objects that should be inserted (must be Object with Table attribute)</param>
        public void Insert(object obj)
        {
            if (obj != null)
            {
                ChangeTrackerEntry cte = new ChangeTrackerEntry(obj, ChangeTrackerEntry.States.Added);
                _objects.Add(cte);
            }
        }

        /// <summary>
        /// Marks an object as deleted, but doesn't delete the object itself
        /// </summary>
        /// <param name="obj">object that should be marked as deleted</param>
        public void Delete(object obj)
        { 
            //ToDo: if references to this Object are set, delete them also? evtl. new Parameter("cascade")
            if (obj != null)
            {
                _objects.Find(x => x.Item.Equals(obj)).State = ChangeTrackerEntry.States.Deleted;
            }
        }

        /// <summary>
        /// Inserts an object into the List of tracked objects and marks it as unmodified
        /// </summary>
        /// <param name="obj">object that should be tracked</param>
        public void Track(object obj)
        {
            if (obj != null)
            {
                ChangeTrackerEntry cte = new ChangeTrackerEntry(obj);
                _objects.Add(cte);
            }
        }

        /// <summary>
        /// resolves the status to delete deleted objects, and update the other ones.
        /// </summary>
        public void ComputeChanges()
        {
            var toBeDeleted = new List<ChangeTrackerEntry>();
            foreach (var obj in _objects)
            {
                switch (obj.State)
                {
                    case ChangeTrackerEntry.States.Modified:
                        obj.SetUnmodified();
                        break;
                    case ChangeTrackerEntry.States.Added:
                        obj.SetUnmodified();
                        break;
                    case ChangeTrackerEntry.States.Deleted:
                        toBeDeleted.Add(obj);
                        break;
                }
            }

            for (int i = toBeDeleted.Count - 1; i >= 0; i--)
            {
                _objects.Remove(toBeDeleted[i]);
            }
        }

        public void Clear()
        {
            _objects.Clear();
        }
    }
}