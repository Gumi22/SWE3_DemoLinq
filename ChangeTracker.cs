using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Linq
{
    class ChangeTracker
    {
        private static readonly List<ChangeTrackerEntry> _objects = new List<ChangeTrackerEntry>();

        public ChangeTracker()
        {
        }

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

        public void Insert(object obj)
        {
            if (obj != null)
            {
                ChangeTrackerEntry cte = new ChangeTrackerEntry(obj, ChangeTrackerEntry.States.Added);
                _objects.Add(cte);
            }
        }

        public void Delete(object obj)
        {
            if (obj != null)
            {
                _objects.Find(x => x.Item.Equals(obj)).State = ChangeTrackerEntry.States.Deleted;
            }
        }

        public void Track(object obj)
        {
            if (obj != null)
            {
                ChangeTrackerEntry cte = new ChangeTrackerEntry(obj);
                _objects.Add(cte);
            }
        }

        public void ComputeChanges()
        {
            var toBeDeleted = new List<ChangeTrackerEntry>();
            foreach (var obj in _objects)
            {
                switch (obj.State)
                {
                    case ChangeTrackerEntry.States.Modified:
                        ComputeUpdate(obj);
                        break;
                    case ChangeTrackerEntry.States.Added:
                        ComputeInsert(obj);
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

        private void ComputeUpdate(ChangeTrackerEntry entry)
        {
            foreach (var originalValue in entry.Originals)
            {
                originalValue.Item2 = originalValue.Item1.GetValue(entry.Item);
            }
            entry.State = ChangeTrackerEntry.States.Unmodified;
        }

        private void ComputeInsert(ChangeTrackerEntry entry)
        {
            ComputeUpdate(entry);
        }
    }
}