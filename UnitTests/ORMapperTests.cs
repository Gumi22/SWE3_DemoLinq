using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Linq;
using Linq.Attributes;
using Linq.Tables;
using Xunit;
using Moq;

namespace UnitTests
{
    public class ORMapperTests
    {
        [Fact]
        public void GetQuery_ReturnsIQueryable()
        {
            //Arrange
            OrMapper or = new OrMapper(new PostGreSqlDatabase(), new ChangeTracker());

            //Act
            var x = or.GetQuery<Camera>();

            //Assert
            Assert.NotNull(x);
            Assert.IsAssignableFrom<IQueryable>(x);
        }

        [Fact]
        public void GetQuery_ThrowsInvalidOperationException_OnNonTableType()
        {
            //Arrange
            OrMapper or = new OrMapper(new PostGreSqlDatabase(), new ChangeTracker());

            //Act & Assert
            Assert.Throws<InvalidOperationException>(() => or.GetQuery<OrMapper>());
        }

        [Fact]
        public void GetEnumerable_ReturnsIEnumerable()
        {
            //Arrange
            Mock<IDatabase> mock = new Mock<IDatabase>();
            mock.Setup(foo => foo.Select<Camera>(It.IsAny<Expression>()))
                .Returns(new List<Camera>() {new Camera() {Id = 5}});
            OrMapper or = new OrMapper(mock.Object, new ChangeTracker());
            var x = or.GetQuery<Camera>().Where(i => i.Id == 5);

            //Act
            var res = or.GetEnumerable<Camera>(x.Expression);

            //Assert
            Assert.NotNull(res);
            Assert.IsAssignableFrom<IEnumerable<Camera>>(res);
        }

        [Fact]
        public void GetEnumerable_ThrowsInvalidOperationException_OnNonTableType()
        {
            //Arrange
            Mock<IDatabase> mock = new Mock<IDatabase>();
            mock.Setup(foo => foo.Select<Camera>(It.IsAny<Expression>()))
                .Returns(new List<Camera>() { new Camera() { Id = 5 } });
            OrMapper or = new OrMapper(mock.Object, new ChangeTracker());
            var x = or.GetQuery<Camera>().Where(i => i.Id == 5);

            //Act & Assert
            Assert.Throws<InvalidOperationException>(() => or.GetEnumerable<OrMapper>(x.Expression));
        }

        [Fact]
        public void GetEnumerable_ReturnsIEnumerable_WithData()
        {
            //Arrange
            Mock<IDatabase> mock = new Mock<IDatabase>();
            mock.Setup(foo => foo.Select<Camera>(It.IsAny<Expression>()))
                .Returns(new List<Camera>() { new Camera() { Id = 5 }, new Camera() { Id = 5 }, new Camera() { Id = 5 } });
            OrMapper or = new OrMapper(mock.Object, new ChangeTracker());
            var x = or.GetQuery<Camera>().Where(i => i.Id == 5);

            //Act
            var res = or.GetEnumerable<Camera>(x.Expression);

            //Assert
            Assert.NotNull(res);
            Assert.IsAssignableFrom<IEnumerable<Camera>>(res);
            foreach (var cam in res)
            {
                Assert.Equal(5, cam.Id);
            }
        }

        [Fact]
        public void Insert_ThrowsInvalidOperationException_OnNonTableType()
        {
            //Arrange
            OrMapper or = new OrMapper(new PostGreSqlDatabase(), new ChangeTracker());

            //Act & Assert
            Assert.Throws<InvalidOperationException>(() => or.Insert(new OrMapper()));
        }

        [Fact]
        public void Insert_InsertsObjectIntoChangeTracker()
        {
            //Arrange
            var mock = new Mock<IChangeTracker>();
            mock.Setup(foo => foo.Insert(It.IsAny<Camera>())).Verifiable("Was not Called");
            OrMapper or = new OrMapper(new PostGreSqlDatabase(), mock.Object);

            //Act
            or.Insert(new Camera());

            //Assert
            mock.Verify(foo => foo.Insert(It.IsAny<Camera>()), Times.Once);
        }

        [Fact]
        public void Delete_ThrowsInvalidOperationException_OnNonTableType()
        {
            //Arrange
            OrMapper or = new OrMapper(new PostGreSqlDatabase(), new ChangeTracker());

            //Act & Assert
            Assert.Throws<InvalidOperationException>(() => or.Delete(new OrMapper()));
        }

        [Fact]
        public void Delete_InsertsObjectIntoChangeTracker()
        {
            //Arrange
            var mock = new Mock<IChangeTracker>();
            mock.Setup(foo => foo.Delete(It.IsAny<Camera>())).Verifiable("Was not Called");
            OrMapper or = new OrMapper(new PostGreSqlDatabase(), mock.Object);

            //Act
            or.Delete(new Camera());

            //Assert
            mock.Verify(foo => foo.Delete(It.IsAny<Camera>()), Times.Once);
        }

        [Fact]
        public void SubmitChanges_CallsRightDBAndCTMethods()
        {
            //Arrange
            var changes = new List<ChangeTrackerEntry>();
            changes.Add(new ChangeTrackerEntry(new Camera(), ChangeTrackerEntry.States.Added));
            changes.Add(new ChangeTrackerEntry(new Camera(), ChangeTrackerEntry.States.Deleted));
            changes.Add(new ChangeTrackerEntry(new Camera(), ChangeTrackerEntry.States.Modified));

            var ct = new Mock<IChangeTracker>();
            ct.Setup(foo => foo.DetectChanges()).Returns(changes).Verifiable("Was not Called");
            ct.Setup(foo => foo.ComputeChanges()).Verifiable("Was not Called");

            var db = new Mock<IDatabase>();
            db.Setup(foo => foo.Delete(It.IsAny<object>())).Verifiable("Was not Called");
            db.Setup(foo => foo.Update(It.IsAny<object>())).Verifiable("Was not Called");
            db.Setup(foo => foo.Insert(It.IsAny<object>())).Returns(1).Verifiable("Was not Called");

            OrMapper or = new OrMapper(db.Object, ct.Object);

            //Act
            or.SubmitChanges();

            //Assert
            ct.Verify(foo => foo.DetectChanges(), Times.Once);
            ct.Verify(foo => foo.ComputeChanges(), Times.Once);
            db.Verify(foo => foo.Delete(It.IsAny<object>()), Times.Once);
            db.Verify(foo => foo.Update(It.IsAny<object>()), Times.Once);
            db.Verify(foo => foo.Insert(It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public void IsTypeATable_True_OnTableType()
        {
            //Arrange
            OrMapper or = new OrMapper(new PostGreSqlDatabase(), new ChangeTracker());

            //Act & Assert
            Assert.True(or.IsTypeATable(new Camera().GetType()));
            Assert.True(or.IsTypeATable(new Photographer().GetType()));
            Assert.True(or.IsTypeATable(new Picture().GetType()));
        }

        [Fact]
        public void IsTypeATable_False_OnNonTableType()
        {
            //Arrange
            OrMapper or = new OrMapper(new PostGreSqlDatabase(), new ChangeTracker());

            //Act & Assert
            Assert.False(or.IsTypeATable(new OrMapper().GetType()));
        }
    }
}
