using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SqlSentence
{
    public class SqlBuilder : ICloneable
    {
        public SqlBuilder()
        {
            Select = new List<SqlPart>();
            From = new List<SqlFromPart>();
            Where = new List<SqlWherePart>();
            GroupBy = new List<SqlPart>();
            Having = new List<SqlPart>();
            OrderBy = new List<SqlOrderByPart>();
            Paging = new SqlPaging();
        }

        public bool Distinct { get; set; }

        public bool Formatted { get; set; }

        public List<SqlFromPart> From { get; set; }

        public List<SqlPart> GroupBy { get; set; }

        public List<SqlPart> Having { get; set; }

        public List<SqlOrderByPart> OrderBy { get; set; }

        public SqlPaging Paging { get; set; }

        public List<SqlPart> Select { get; set; }

        public List<SqlWherePart> Where { get; set; }

        public object Clone()
        {
            var o = new SqlBuilder
            {
                Select = new List<SqlPart>(),
                From = new List<SqlFromPart>(),
                Where = new List<SqlWherePart>(),
                GroupBy = new List<SqlPart>(),
                Having = new List<SqlPart>(),
                OrderBy = new List<SqlOrderByPart>()
            };
            foreach (var part in Select)
            {
                o.Select.Add((SqlPart)part.Clone());
            }
            foreach (var part in From)
            {
                o.From.Add((SqlFromPart)part.Clone());
            }
            foreach (var part in Where)
            {
                o.Where.Add((SqlWherePart)part.Clone());
            }
            foreach (var part in GroupBy)
            {
                o.GroupBy.Add((SqlPart)part.Clone());
            }
            foreach (var part in Having)
            {
                o.Having.Add((SqlPart)part.Clone());
            }
            foreach (var part in OrderBy)
            {
                o.OrderBy.Add((SqlOrderByPart)part.Clone());
            }
            o.Paging = (SqlPaging)Paging.Clone();
            o.Distinct = Distinct;
            return o;
        }

        public SqlFromPart AddFrom(string value)
        {
            return AddFrom(SqlFromPartOperator.None, value);
        }

        public SqlFromPart AddFrom(SqlFromPartOperator @operator, string value)
        {
            return AddFrom(@operator, value, null);
        }

        public SqlFromPart AddFrom(SqlFromPartOperator @operator, string value, string name)
        {
            var part = new SqlFromPart(value, name, @operator);
            From.Add(part);
            return part;
        }

        public SqlPart AddGroupBy(string value)
        {
            return AddGroupBy(value, null);
        }

        public SqlPart AddGroupBy(string value, string name)
        {
            var part = new SqlPart(value, name);
            GroupBy.Add(part);
            return part;
        }

        public SqlPart AddHaving(string value)
        {
            return AddHaving(value, null);
        }

        public SqlPart AddHaving(string value, string name)
        {
            var part = new SqlPart(value, name);
            Having.Add(part);
            return part;
        }

        public SqlOrderByPart AddOrderBy(string value)
        {
            return AddOrderBy(SqlOrderByDirection.Ascending, value);
        }

        public SqlOrderByPart AddOrderBy(SqlOrderByDirection direction, string value)
        {
            return AddOrderBy(SqlOrderByDirection.Ascending, value, null);
        }

        public SqlOrderByPart AddOrderBy(SqlOrderByDirection direction, string value, string name)
        {
            var part = new SqlOrderByPart(value, name, direction);
            OrderBy.Add(part);
            return part;
        }

        public SqlPart AddSelect(string value)
        {
            return AddSelect(value, null);
        }

        public SqlPart AddSelect(string value, string name)
        {
            var part = new SqlPart(value, name);
            Select.Add(part);
            return part;
        }

        public SqlWherePart AddWhere(string value)
        {
            return AddWhere(SqlWherePartOperator.And, value);
        }

        public SqlWherePart AddWhere(SqlWherePartOperator @operator, string value)
        {
            return AddWhere(@operator, value, null);
        }

        public SqlWherePart AddWhere(SqlWherePartOperator @operator, string value, string name)
        {
            var part = new SqlWherePart(value, name, @operator);
            Where.Add(part);
            return part;
        }

        public void EnablePagination(int pageIndex, int pageSize)
        {
            Paging.Enabled = true;
            Paging.PageIndex = pageIndex;
            Paging.PageSize = pageSize;
        }

        private static string Prettify(string sql)
        {
            const RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.ExplicitCapture;
            var regEx = new Regex("\\s{2,}", options);
            sql = sql.Trim();
            sql = regEx.Replace(sql, " ");
            regEx = new Regex(",\\s*,", options);
            sql = regEx.Replace(sql, ",");
            return sql;
        }

        public string Create()
        {
            var sql = new StringBuilder();

            AppendSelect(sql);

            AppendFrom(sql);

            AppendWhere(sql);

            AppendSqlPartList(sql, GroupBy, "GROUP BY");

            AppendSqlPartList(sql, Having, "HAVING");

            AppendOrderBy(sql);

            AppendPagination(sql);

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
            if (Formatted)
            {
                sql.Append(Environment.NewLine);
            }
        }

        private void AppendFrom(StringBuilder sql)
        {
            sql.Append(" FROM");
            foreach (var part in From)
            {
                string @operator = null;
                switch (part.Operator)
                {
                    case SqlFromPartOperator.InnerJoin:
                        @operator = "INNER JOIN";
                        break;
                    case SqlFromPartOperator.LeftJoin:
                        @operator = "LEFT JOIN";
                        break;
                    case SqlFromPartOperator.RightJoin:
                        @operator = "RIGHT JOIN";
                        break;
                }
                sql.Append(string.Format(" {0} {1}", @operator, part.Value));
                if (part.NoLock)
                {
                    sql.Append(" WITH (NOLOCK)");
                }
            }

            if (Formatted)
            {
                sql.Append(Environment.NewLine);
            }
        }

        private void AppendPagination(StringBuilder sql)
        {
            if (Paging.Enabled)
            {
                sql.Append(string.Format(" OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", Paging.Offset, Paging.PageSize));
            }
        }

        private void AppendSqlPartList(StringBuilder sql, IEnumerable<SqlPart> list, string clausule)
        {
            if (list.Any())
            {
                sql.Append(string.Format(" {0}", clausule));
                foreach (var part in list)
                {
                    sql.Append(string.Format(" {0},", part.Value));
                }
                sql.Remove(sql.Length - 1, 1);
                if (Formatted)
                {
                    sql.Append(Environment.NewLine);
                }
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
                        case SqlWherePartOperator.And:
                            @operator = "AND";
                            break;
                        case SqlWherePartOperator.Or:
                            @operator = "OR";
                            break;
                        case SqlWherePartOperator.AndNot:
                            @operator = "AND NOT";
                            break;
                        case SqlWherePartOperator.OrNot:
                            @operator = "OR NOT";
                            break;
                    }
                    sql.Append(string.Format(" {0} ({1})", first ? null : @operator, part.Value));
                    first = false;
                }
                if (Formatted)
                {
                    sql.Append(Environment.NewLine);
                }
            }
        }

        private void AppendOrderBy(StringBuilder sql)
        {
            if (OrderBy.Any())
            {
                sql.Append(" ORDER BY");
                foreach (var part in OrderBy)
                {
                    sql.Append(string.Format(" {0} {1},", part.Value, part.Direction == SqlOrderByDirection.Ascending ? "ASC" : "DESC"));
                }
                sql.Remove(sql.Length - 1, 1);
                if (Formatted)
                {
                    sql.Append(Environment.NewLine);
                }
            }
        }
    }
}