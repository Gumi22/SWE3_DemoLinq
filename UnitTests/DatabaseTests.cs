using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Linq;
using Linq.Tables;
using Moq;
using Xunit;

namespace UnitTests
{
    public class DatabaseTests
    {
        [Fact]
        public void Select_ReturnsListGreaterOne()
        {
            //Arrange
            var qry = new OrMapper().GetQuery<Picture>();
            var filtered = qry
                .Where(i => true);

            //Act
            var lst = filtered.ToList();

            //Assert
            Assert.NotNull(lst);
            Assert.IsAssignableFrom<List<Picture>>(lst);
            Assert.True(1 < lst.Count);
        }

        [Fact]
        public void Insert_InsertsObject_ReturnsID()
        {
            //Arrange
            var orm = new OrMapper();
            PostGreSqlDatabase db = new PostGreSqlDatabase();
            Photographer p = GetPhotographer();

            //Act
            int id = db.Insert(p);

            //Assert
            Assert.NotNull(p);
            Assert.True(0 < id);
            var lst = orm.GetQuery<Photographer>().Where(i => i.Id == id).ToList();
            Assert.NotNull(lst);
            Assert.IsAssignableFrom<List<Photographer>>(lst);
            Assert.Single(lst);

            //Cleanup
            p.Id = id;
            db.Delete(p);

        }

        [Fact]
        public void Delete_DeletesObject()
        {
            //Arrange
            var orm = new OrMapper();
            PostGreSqlDatabase db = new PostGreSqlDatabase();
            Photographer p = GetPhotographer();
            p.Id = db.Insert(p);
            int id = p.Id; //only because .where(... == p.Id cant be resolved by visitor and it writes photographer.id instead of 5 or 7 or whatever id is

            //Act
            db.Delete(p);

            //Assert
            Assert.NotNull(p);
            var lst = orm.GetQuery<Photographer>().Where(i => i.Id == id).ToList();
            Assert.NotNull(lst);
            Assert.IsAssignableFrom<List<Photographer>>(lst);
            Assert.Empty(lst);
        }

        [Fact]
        public void Update_UpdatesObject()
        {
            //Arrange
            var orm = new OrMapper();
            PostGreSqlDatabase db = new PostGreSqlDatabase();
            Photographer p = GetPhotographer();
            int id = db.Insert(p);
            p.Id = id;
            p.Name = "OtherName";
            p.SurName = "OtherSurName";

            //Act
            db.Update(p);

            //Assert
            var lst = orm.GetQuery<Photographer>().Where(i => i.Id == id).ToList();
            Assert.NotNull(lst);
            Assert.IsAssignableFrom<List<Photographer>>(lst);
            Assert.Single(lst);
            Assert.Equal(p.Name, lst[0].Name);
            Assert.Equal(p.SurName, lst[0].SurName);

            //Cleanup
            p.Id = id;
            db.Delete(p);
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
