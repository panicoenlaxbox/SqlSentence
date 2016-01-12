using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace SqlSentence.Tests
{
    [TestFixture]
    public class PaginationTests
    {
        [Test]
        public void Pagination()
        {
            //Arrange
            var sql = new SqlBuilder();
            sql.AddSelect("EmpresaId");
            sql.AddSelect("Nombre");
            sql.AddFrom("Empresas");
            sql.AddOrderBy("Nombre ASC");
            sql.Paging.Enabled = false;
            //Assert
            Debug.Print(sql.Create());
            //Act
        }
    }
}
