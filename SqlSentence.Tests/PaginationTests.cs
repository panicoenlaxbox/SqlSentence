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
            var builder = new Builder();
            builder.AddSelect(@"Canal
		,Tipologia
		,NivelClasificacion1
		,NivelClasificacion2
		,SUM(MerchandisePlanningFilas.Facturacion) AS Facturacion");
            builder.AddFrom("MerchandisePlanningFilas");
            builder.AddWhere("VersionId = 4");
            builder.AddOrderBy("MerchandisePlanningFilas.Tipologia ASC, NivelClasificacion1 ASC, [MerchandisePlanningFilas].NivelClasificacion2 ASC");
            builder.AddGroupBy("Canal");
            builder.AddGroupBy("Tipologia");
            builder.AddGroupBy("NivelClasificacion1, NivelClasificacion2");
            builder.EnablePaginationWithCte(1, 10);
            Debug.Print(builder.Build());
        }
    }
}
