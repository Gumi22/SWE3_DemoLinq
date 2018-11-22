using System;
using System.Collections.Generic;
using System.Text;

namespace Linq
{
    public interface IChangeTracker
    {
        /// <summary>
        /// Detects Objects that have been modified, added, or deleted
        /// and compiles them into a new List
        /// ComputeChanges should be called after this
        /// </summary>
        /// <returns>A list of ChangeTrackerEntries that were inserted, modified, or deleted</returns>
        List<ChangeTrackerEntry> DetectChanges();

        /// <summary>
        /// Inserts an object to be tracked
        /// </summary>
        /// <param name="obj">the objects that should be inserted (must be Object with Table attribute)</param>
        void Insert(object obj);

        /// <summary>
        /// Marks an object as deleted, but doesn't delete the object itself
        /// </summary>
        /// <param name="obj">object that should be marked as deleted</param>
        void Delete(object obj);

        /// <summary>
        /// Inserts an object into the List of tracked objects and marks it as unmodified
        /// </summary>
        /// <param name="obj">object that should be tracked</param>
        void Track(object obj);

        /// <summary>
        /// resolves the status to delete deleted objects, and update the other ones.
        /// </summary>
        void ComputeChanges();
    }
}
