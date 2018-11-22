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
    public class DemoLinqTests
    {
        [Fact]
        public void Select_ReturnsNotEmptyList()
        {
            //Arrange
            DemoLinq<Camera> linq = new DemoLinq<Camera>();

            //Act
            var list = linq.Where(c => c.Model != "");

            //Assert
            Assert.NotEmpty(list);
        }

        [Fact]
        public void Select_ReturnsFiltered()
        {
            //Arrange
            Camera x = GetCamera();
            DemoLinq<Camera> linq = new DemoLinq<Camera>();

            //Act
            var list = linq.Where(c => c.Producer != x.Producer);

            //Assert
            Assert.NotEmpty(list);
        }

        private static Camera GetCamera()
        {
            Camera p = new Camera();
            p.Model = "Model";
            p.Producer = "Kodac";
            p.PurchaseDate = new DateTime(1995, 12, 22);
            p.IsoLimitAcceptable = 1.2f;
            p.IsoLimitGood = 0.8f;
            p.Notes = "Notes";
            return p;
        }
    }
}
