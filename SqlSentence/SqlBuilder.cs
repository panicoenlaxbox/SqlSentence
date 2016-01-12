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
            OrderBy = new List<SqlPart>();
            Paging = new SqlPaging();
        }

        public bool Distinct { get; set; }

        public bool Formatted { get; set; }

        public List<SqlFromPart> From { get; set; }

        public List<SqlPart> GroupBy { get; set; }

        public List<SqlPart> Having { get; set; }

        public List<SqlPart> OrderBy { get; set; }

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
                OrderBy = new List<SqlPart>()
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
            var part = new SqlFromPart(@operator, value, name);
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

        public SqlPart AddOrderBy(string value)
        {
            return AddOrderBy(value, null);
        }

        public SqlPart AddOrderBy(string value, string name)
        {
            var part = new SqlPart(value, name);
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

        public SqlWherePart AddWhere(string value, string name)
        {
            return AddWhere(SqlWherePartOperator.And, value, name);
        }

        public SqlWherePart AddWhere(SqlWherePartOperator @operator, string value, string name)
        {
            var part = new SqlWherePart(@operator, value, name);
            Where.Add(part);
            return part;
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

            AppendGroupBy(sql);

            AppendHaving(sql);

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
                sql.Append(string.Format(" {0},", part.Text));
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
                sql.Append(string.Format(" {0} {1}", @operator, part.Text));
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

        private void AppendOrderBy(StringBuilder sql)
        {
            if (OrderBy.Any())
            {
                sql.Append(" ORDER BY");
                foreach (var part in OrderBy)
                {
                    sql.Append(string.Format(" {0}", part.Text));
                }
                if (Formatted)
                {
                    sql.Append(Environment.NewLine);
                }
            }
        }

        private void AppendHaving(StringBuilder sql)
        {
            if (Having.Any())
            {
                sql.Append(" HAVING");
                foreach (var part in Having)
                {
                    sql.Append(string.Format(" {0}", part.Text));
                }
                if (Formatted)
                {
                    sql.Append(Environment.NewLine);
                }
            }
        }

        private void AppendGroupBy(StringBuilder sql)
        {
            if (GroupBy.Any())
            {
                sql.Append(" GROUP BY");
                foreach (var part in GroupBy)
                {
                    sql.Append(string.Format(" {0}", part.Text));
                }
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
                    sql.Append(string.Format(" {0} ({1})", first ? null : @operator, part.Text));
                    first = false;
                }
                if (Formatted)
                {
                    sql.Append(Environment.NewLine);
                }
            }
        }
    }
}