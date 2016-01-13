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
            var request = new GetFilasRequest();
            request.AddDimension(GetFilasRequest.DimensionType.Tipologia, 1);
            request.AddDimension(GetFilasRequest.DimensionType.NivelClasificacion, 2, 1);
            request.AddDimension(GetFilasRequest.DimensionType.NivelClasificacion, 3, 2);
            request.AddDimension(GetFilasRequest.DimensionType.NivelClasificacion, 4, 4);
            var sql = GetFilas(4, request);
            Debug.Print(sql);
        }

        private string GetFilas(int versionId, GetFilasRequest request)
        {
            var sql = new SqlBuilder();
            sql.AddSelect("Canal");
            foreach (var dimension in request.Dimensions.OrderBy(d => d.Position))
            {
                sql.AddSelect(dimension.FieldName);
            }
            sql.AddSelect(@"SUM(Facturacion)
	,SUM(Cantidad)
	,SUM(DesignsTotales)
	,SUM(DesignsLanzados)
	,SUM(PrecioMedio)
	,SUM(CosteMedio)
	,SUM(MargenBruto)
	,SUM(Markup)
	,SUM(CosteTotal)
	,SUM(Otb)
	,SUM(CantidadPorDesignsTotales)
	,SUM(CantidadTotalLanzamiento)
	,SUM(CantidadPorDesignLanzado)
	,SUM(CostePorDesignsTotales)
	,SUM(CosteTotalLanzamiento)
	,SUM(CostePorDesignLanzado)
	,SUM(FacturacionReal)
	,SUM(CantidadReal)
	,SUM(DesignsTotalesReal)
	,SUM(DesignsLanzadosReal)
	,SUM(PrecioMedioReal)
	,SUM(CosteMedioReal)
	,SUM(MargenBrutoReal)
	,SUM(MarkupReal)
	,SUM(CosteTotalReal)
	,SUM(FacturacionAnterior)
	,SUM(CantidadAnterior)
	,SUM(DesignsTotalesAnterior)
	,SUM(DesignsLanzadosAnterior)
	,SUM(PrecioMedioAnterior)
	,SUM(CosteMedioAnterior)
	,SUM(MargenBrutoAnterior)
	,SUM(MarkupAnterior)
	,SUM(CosteTotalAnterior)
	,SUM(PorcentajeFacturacion)
	,SUM(PorcentajeFacturacionReal)
	,SUM(PorcentajeFacturacionAnterior)");
            sql.AddFrom("MerchandisePlanningFilas");
            sql.AddWhere(string.Format("VersionId = {0}", versionId));
            sql.AddGroupBy("Canal");
            foreach (var dimension in request.Dimensions.OrderBy(d => d.Position))
            {
                sql.AddGroupBy(dimension.FieldName);
                sql.AddOrderBy(dimension.FieldName);
            }
            sql.Paging.Enabled = true;
            sql.Paging.PageIndex = 0;
            sql.Paging.PageSize = 10;
            return sql.Create();
        }

        public class GetFilasRequest
        {
            private readonly IList<Dimension> _dimensions;

            public GetFilasRequest()
            {
                _dimensions = new List<Dimension>();
            }

            public IEnumerable<Dimension> Dimensions
            {
                get { return _dimensions; }
            }

            public void AddDimension(DimensionType type, int position)
            {
                AddDimension(type, position, null);
            }

            public void AddDimension(DimensionType type, int position, int? levelIndex)
            {
                var dimension = new Dimension(type, position, levelIndex);
                _dimensions.Add(dimension);
            }

            public class Dimension
            {


                public Dimension(DimensionType type, int position)
                    : this(type, position, null)
                {

                }

                public Dimension(DimensionType type, int position, int? levelIndex)
                {
                    if (type == DimensionType.NivelClasificacion && levelIndex == null)
                    {
                        throw new ArgumentException("levelIndex can not be null if type is NivelClasificacion");
                    }
                    Type = type;
                    Position = position;
                    LevelIndex = levelIndex;
                }

                public int Position { get; set; }
                public DimensionType Type { get; set; }
                public int? LevelIndex { get; set; }

                public string FieldName
                {
                    get
                    {
                        if (Type == DimensionType.Tipologia)
                        {
                            return "Tipologia";
                        }
                        return string.Format("NivelClasificacion{0}", LevelIndex);
                    }
                }

            }

            public enum DimensionType
            {
                Tipologia,
                NivelClasificacion
            }
        }
    }
}
