using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Linq
{
    class ChangeTracker
    {
       

        private readonly List<ChangeTrackerEntry> _objects;

        public ChangeTracker()
        {
            _objects = new List<ChangeTrackerEntry>();
            
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
            foreach (var obj in _objects)
            {
                switch (obj.State)
                {
                    case ChangeTrackerEntry.States.Modified:
                        ComputeUpdate(obj);
                        break;
                    case ChangeTrackerEntry.States.Added:
                        ComputeDelete(obj);
                        break;
                    case ChangeTrackerEntry.States.Deleted:
                        ComputeInsert(obj);
                        break;
                }
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

        private void ComputeDelete(ChangeTrackerEntry entry)
        {
            _objects.Remove(entry);
        }

        private void ComputeInsert(ChangeTrackerEntry entry)
        {
            ComputeUpdate(entry);
        }
    }
}