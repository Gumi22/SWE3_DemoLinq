using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Linq;
using Linq.Tables;
using Xunit;

namespace UnitTests
{
    public class ChangeTrackerEntryTests
    {
        [Fact]
        public void Constructor_ThrowsOnNullObject()
        {
            //Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ChangeTrackerEntry(null));
        }

        [Fact]
        public void Constructor_ThrowsOnObjectWithoutProperties()
        {
            //Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ChangeTrackerEntry(new ChangeTrackerEntryTests()));
        }

        [Fact]
        public void Constructor_SetsValues()
        {
            //Arrange
            Photographer p = GetPhotographer();

            //Act
            ChangeTrackerEntry cte = new ChangeTrackerEntry(p);

            //Assert
            Assert.NotNull(cte.Item);
            Assert.Equal(p, cte.Item);
            Assert.NotNull(cte.Originals);
            Assert.Equal(ChangeTrackerEntry.States.Unmodified, cte.State);
            Assert.Equal(p.GetType().GetProperties().Length, cte.Originals.Count);
            foreach (var pair in cte.Originals)
            {
                Assert.Equal(pair.Item1.GetValue(p), pair.Item2);
            }
        }

        [Fact]
        public void ComputeState_NoChange_StateAdded()
        {
            //Arrange
            Photographer p = GetPhotographer();
            ChangeTrackerEntry cte = new ChangeTrackerEntry(p, ChangeTrackerEntry.States.Added);

            //Act
            p.Name = "othername";
            cte.ComputeState();

            //Assert
            Assert.NotNull(cte.Item);
            Assert.Equal(p, cte.Item);
            Assert.NotNull(cte.Originals);
            Assert.Equal(ChangeTrackerEntry.States.Added, cte.State);
            Assert.Equal(p.GetType().GetProperties().Length, cte.Originals.Count);
        }

        [Fact]
        public void ComputeState_NoChange_StateDeleted()
        {
            //Arrange
            Photographer p = GetPhotographer();
            ChangeTrackerEntry cte = new ChangeTrackerEntry(p, ChangeTrackerEntry.States.Deleted);

            //Act
            p.Name = "othername";
            cte.ComputeState();

            //Assert
            Assert.NotNull(cte.Item);
            Assert.Equal(p, cte.Item);
            Assert.NotNull(cte.Originals);
            Assert.Equal(ChangeTrackerEntry.States.Deleted, cte.State);
            Assert.Equal(p.GetType().GetProperties().Length, cte.Originals.Count);
        }

        [Fact]
        public void ComputeState_SetState_StateModified()
        {
            //Arrange
            Photographer p = GetPhotographer();
            ChangeTrackerEntry cte = new ChangeTrackerEntry(p);

            //Act
            p.BirthDate = p.BirthDate.AddDays(2);
            cte.ComputeState();

            //Assert
            Assert.NotNull(cte.Item);
            Assert.Equal(p, cte.Item);
            Assert.NotNull(cte.Originals);
            Assert.Equal(ChangeTrackerEntry.States.Modified, cte.State);
            Assert.Equal(p.GetType().GetProperties().Length, cte.Originals.Count);
        }

        [Fact]
        public void SetUnmodified_SetsStateToUnmodified_FromModified()
        {
            //Arrange
            Photographer p = GetPhotographer();
            ChangeTrackerEntry cte = new ChangeTrackerEntry(p);
            p.BirthDate = p.BirthDate.AddDays(2);
            cte.ComputeState();

            //Act
            cte.SetUnmodified();

            //Assert
            Assert.NotNull(cte.Item);
            Assert.Equal(p, cte.Item);
            Assert.NotNull(cte.Originals);
            Assert.Equal(ChangeTrackerEntry.States.Unmodified, cte.State);
            Assert.Equal(p.GetType().GetProperties().Length, cte.Originals.Count);
            foreach (var pair in cte.Originals)
            {
                Assert.Equal(pair.Item1.GetValue(p), pair.Item2);
            }
        }




        private static Photographer GetPhotographer()
        {
            Photographer p = new Photographer();
            p.Id = 1;
            p.Name = "name";
            p.SurName = "surname";
            p.BirthDate = new DateTime(1995, 12, 22);
            p.Notes = "lel";
            return p;
        }
    }
}
