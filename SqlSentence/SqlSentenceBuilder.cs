using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SqlSentence
{
    public class SqlSentenceBuilder : ICloneable
    {
        public SqlSentenceBuilder()
        {
            Select = new List<Part>();
            From = new List<FromPart>();
            Where = new List<WherePart>();
            GroupBy = new List<Part>();
            Having = new List<Part>();
            OrderBy = new List<Part>();
            Paging = new Paging();
        }

        public bool Distinct { get; set; }

        public IList<FromPart> From { get; set; }

        public IList<Part> GroupBy { get; set; }

        public IList<Part> Having { get; set; }

        public IList<Part> OrderBy { get; set; }

        public Paging Paging { get; set; }

        public IList<Part> Select { get; set; }

        public IList<WherePart> Where { get; set; }

        public object Clone()
        {
            var o = new SqlSentenceBuilder { Distinct = Distinct };
            foreach (var part in Select)
            {
                o.Select.Add((Part)part.Clone());
            }
            foreach (var part in From)
            {
                o.From.Add((FromPart)part.Clone());
            }
            foreach (var part in Where)
            {
                o.Where.Add((WherePart)part.Clone());
            }
            foreach (var part in GroupBy)
            {
                o.GroupBy.Add((Part)part.Clone());
            }
            foreach (var part in Having)
            {
                o.Having.Add((Part)part.Clone());
            }
            foreach (var part in OrderBy)
            {
                o.OrderBy.Add((Part)part.Clone());
            }
            o.Paging = (Paging)Paging.Clone();
            return o;
        }

        public FromPart AddFrom(string value)
        {
            return AddFrom(value, FromOperator.None);
        }

        public FromPart AddFrom(string value, FromOperator @operator)
        {
            return AddFrom(value, null, @operator);
        }

        public FromPart AddFrom(string value, string name, FromOperator @operator)
        {
            var part = new FromPart(value, name, @operator);
            From.Add(part);
            return part;
        }

        public Part AddGroupBy(string value)
        {
            return AddGroupBy(value, null);
        }

        public Part AddGroupBy(string value, string name)
        {
            var part = new Part(value, name);
            GroupBy.Add(part);
            return part;
        }

        public Part AddHaving(string value)
        {
            return AddHaving(value, null);
        }

        public Part AddHaving(string value, string name)
        {
            var part = new Part(value, name);
            Having.Add(part);
            return part;
        }

        public Part AddOrderBy(string value)
        {
            return AddOrderBy(value, null);
        }

        public Part AddOrderBy(string value, string name)
        {
            var part = new Part(value, name);
            OrderBy.Add(part);
            return part;
        }

        public Part AddSelect(string value)
        {
            return AddSelect(value, null);
        }

        public Part AddSelect(string value, string name)
        {
            var part = new Part(value, name);
            Select.Add(part);
            return part;
        }

        public WherePart AddWhere(string value)
        {
            return AddWhere(value, WhereOperator.And);
        }

        public WherePart AddWhere(string value, WhereOperator @operator)
        {
            return AddWhere(value, null, @operator);
        }

        public WherePart AddWhere(string value, string name)
        {
            return AddWhere(value, name, WhereOperator.And);
        }

        public WherePart AddWhere(string value, string name, WhereOperator @operator)
        {
            var part = new WherePart(value, name != null ? name.Trim() : null, @operator);
            Where.Add(part);
            return part;
        }

        public void RemoveWhere(string name)
        {
            var part = Where.SingleOrDefault(p => string.Equals(p.Name, name.Trim(), StringComparison.CurrentCultureIgnoreCase));
            if (part != null)
            {
                Where.Remove(part);
            }
        }

        public void RemoveWhere(IEnumerable<string> names)
        {
            foreach (var name in names)
            {
                RemoveWhere(name);
            }
        }

        public void EnablePagination(int pageIndex, int pageSize)
        {
            Paging.Enabled = true;
            Paging.PageIndex = pageIndex;
            Paging.PageSize = pageSize;
        }

        public void EnablePaginationWithCte(int pageIndex, int pageSize)
        {
            EnablePaginationWithCte(pageIndex, pageSize, Paging.CteTotalCountFieldName);
        }

        public void EnablePaginationWithCte(int pageIndex, int pageSize, string cteTotalCountFieldName)
        {
            Paging.Enabled = true;
            Paging.PageIndex = pageIndex;
            Paging.PageSize = pageSize;
            Paging.UseCte = true;
            Paging.CteTotalCountFieldName = cteTotalCountFieldName;
        }

        private static string Prettify(string sql)
        {
            const RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.ExplicitCapture;
            var regex = new Regex("\\s{2,}", options);
            sql = sql.Trim();
            sql = regex.Replace(sql, " ");
            regex = new Regex(",\\s*,", options);
            sql = regex.Replace(sql, ",");
            return sql;
        }

        public string BuildWithCount()
        {
            var copy = new List<Part>();
            foreach (var part in Select)
            {
                copy.Add((Part)part.Clone());
            }
            Select.Clear();
            AddSelect("COUNT(*)");
            var sql = Build();
            Select = copy;
            return sql;
        }

        public string Build()
        {
            var sql = new StringBuilder();

            AppendSelect(sql);
            sql.NewLine();

            AppendFrom(sql);
            sql.NewLine();

            AppendWhere(sql);
            sql.NewLineIf(Where.Any());

            AppendSqlPartList(sql, GroupBy, "GROUP BY");
            sql.NewLineIf(GroupBy.Any());

            AppendSqlPartList(sql, Having, "HAVING");
            sql.NewLineIf(Having.Any());

            var paging = Paging.Enabled;
            var pagingWithCte = paging && Paging.UseCte;
            if (pagingWithCte)
            {
                var sqlCte = new StringBuilder();
                sqlCte.Append("WITH CTE_MAIN AS (");
                sqlCte.NewLine();
                sqlCte.Append(sql);
                sqlCte.NewLine();
                sqlCte.Append(string.Format("), CTE_COUNT AS (SELECT COUNT(*) AS [{0}] FROM CTE_MAIN)", Paging.CteTotalCountFieldName));
                sqlCte.NewLine();
                sqlCte.Append("SELECT * FROM CTE_MAIN, CTE_COUNT");
                sqlCte.NewLine();
                AppendSqlPartList(sqlCte, OrderBy, "ORDER BY", removeTable: true);
                sqlCte.NewLine();
                AppendPagination(sqlCte);
                sql = sqlCte;
            }
            else if (paging)
            {
                AppendSqlPartList(sql, OrderBy, "ORDER BY");
                sql.NewLine();

                AppendPagination(sql);
                sql.NewLine();
            }
            else
            {
                AppendSqlPartList(sql, OrderBy, "ORDER BY");
            }

            sql.Append(";");

            return Prettify(sql.ToString());
        }

        private void AppendSelect(StringBuilder sql)
        {
            sql.Append("SELECT");
            if (Distinct)
            {
                sql.Append(" DISTINCT ");
            }
            foreach (var part in Select)
            {
                sql.Append(string.Format(" {0},", part.Value));
            }
            sql.Remove(sql.Length - 1, 1);
        }

        private void AppendFrom(StringBuilder sql)
        {
            sql.Append(" FROM");
            foreach (var part in From)
            {
                string @operator = null;
                switch (part.Operator)
                {
                    case FromOperator.InnerJoin:
                        @operator = "INNER JOIN";
                        break;
                    case FromOperator.LeftJoin:
                        @operator = "LEFT JOIN";
                        break;
                    case FromOperator.RightJoin:
                        @operator = "RIGHT JOIN";
                        break;
                }
                sql.Append(string.Format(" {0} {1}", @operator, part.Value));
                if (part.NoLock)
                {
                    sql.Append(" WITH (NOLOCK)");
                }
            }
        }

        private void AppendPagination(StringBuilder sql)
        {
            sql.Append(string.Format(" OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", Paging.Offset, Paging.PageSize));
        }

        private void AppendSqlPartList(StringBuilder sql, IEnumerable<Part> list, string clausule, bool removeTable = false)
        {
            if (list.Any())
            {
                sql.Append(string.Format(" {0}", clausule));
                foreach (var part in list)
                {
                    var value = part.Value;
                    if (removeTable)
                    {
                        value = RemoveTable(value);
                    }
                    sql.Append(string.Format(" {0},", value));
                }
                sql.Remove(sql.Length - 1, 1);
            }
        }

        private void AppendWhere(StringBuilder sql)
        {
            if (Where.Any())
            {
                sql.Append(" WHERE");
                var first = true;
                foreach (var part in Where)
                {
                    string @operator = null;
                    switch (part.Operator)
                    {
                        case WhereOperator.And:
                            @operator = "AND";
                            break;
                        case WhereOperator.Or:
                            @operator = "OR";
                            break;
                        case WhereOperator.AndNot:
                            @operator = "AND NOT";
                            break;
                        case WhereOperator.OrNot:
                            @operator = "OR NOT";
                            break;
                    }
                    sql.Append(string.Format(" {0} ({1})", first ? null : @operator, part.Value));
                    first = false;
                }
            }
        }

        private string RemoveTable(string value)
        {
            var regex = new Regex(@"[^,\s]+?\.", RegexOptions.Multiline);
            return regex.Replace(value, string.Empty);
        }
    }
}