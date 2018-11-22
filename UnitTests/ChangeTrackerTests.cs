using System;
using System.Collections.Generic;
using System.Text;
using Linq;
using Linq.Tables;
using Xunit;

namespace UnitTests
{
    public class ChangeTrackerTests
    {
        [Fact]
        public void Insert_DetectsChangesAsAdded()
        {
            //Arrange
            ChangeTracker ct = new ChangeTracker();
            var phot = GetPhotographer();

            //Act
            ct.Insert(phot);
            var changes = ct.DetectChanges();

            //Assert
            Assert.Single(changes);
            Assert.Equal(ChangeTrackerEntry.States.Added, changes[0].State);

            //Cleanup
            ct.Clear();
        }

        [Fact]
        public void Update_DetectsChangesAsModified()
        {
            //Arrange
            ChangeTracker ct = new ChangeTracker();
            var phot = GetPhotographer();
            ct.Insert(phot);
            ct.DetectChanges();
            ct.ComputeChanges();

            //Act
            phot.Name = "OtherName";
            var changes = ct.DetectChanges();

            //Assert
            Assert.Single(changes);
            Assert.Equal(ChangeTrackerEntry.States.Modified, changes[0].State);

            //Cleanup
            ct.Clear();
        }

        [Fact]
        public void Delete_DetectsChangesAsDeleted()
        {
            //Arrange
            ChangeTracker ct = new ChangeTracker();
            var phot = GetPhotographer();
            ct.Insert(phot);
            ct.DetectChanges();
            ct.ComputeChanges();

            //Act
            ct.Delete(phot);
            var changes = ct.DetectChanges();

            //Assert
            Assert.Single(changes);
            Assert.Equal(ChangeTrackerEntry.States.Deleted, changes[0].State);

            //Cleanup
            ct.Clear();
        }

        [Fact]
        public void Track_DoNotDetectChanges()
        {
            //Arrange
            ChangeTracker ct = new ChangeTracker();
            var phot = GetPhotographer();

            //Act
            ct.Track(phot);
            var changes = ct.DetectChanges();

            //Assert
            Assert.Empty(changes);

            //Cleanup
            ct.Clear();
        }

        [Fact]
        public void ComputeChanges_SetsAllEntriesToUnmodified()
        {
            //Arrange
            ChangeTracker ct = new ChangeTracker();
            var phot = GetPhotographer();
            var phot2 = GetPhotographer();
            var phot3 = GetPhotographer();
            var phot4 = GetPhotographer();
            ct.Insert(phot);
            ct.Track(phot2);
            ct.Track(phot3);
            ct.Insert(phot4);
            ct.Delete(phot2);
            phot3.Name = "OtherName";

            //Act
            var changes = ct.DetectChanges();
            ct.ComputeChanges();
            var changes2 = ct.DetectChanges();

            //Assert
            Assert.Equal(4, changes.Count);
            Assert.Empty(changes2);

            //Cleanup
            ct.Clear();
        }

        private static Photographer GetPhotographer()
        {
            Photographer p = new Photographer();
            p.Name = "name";
            p.SurName = "surname";
            p.BirthDate = new DateTime(1995, 12, 22);
            p.Notes = "lel";
            return p;
        }
    }
}
