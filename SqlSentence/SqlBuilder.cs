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
                o.Select.Add((SqlPart) part.Clone());
            }
            foreach (var part in From)
            {
                o.From.Add((SqlFromPart) part.Clone());
            }
            foreach (var part in Where)
            {
                o.Where.Add((SqlWherePart) part.Clone());
            }
            foreach (var part in GroupBy)
            {
                o.GroupBy.Add((SqlPart) part.Clone());
            }
            foreach (var part in Having)
            {
                o.Having.Add((SqlPart) part.Clone());
            }
            o.Paging = (SqlPaging) Paging.Clone();
            o.Distinct = Distinct;
            return o;
        }

        public SqlFromPart AddFrom(string text)
        {
            return AddFrom(SqlFromPartOperator.None, text);
        }

        public SqlFromPart AddFrom(SqlFromPartOperator @operator, string text)
        {
            return AddFrom(@operator, text, string.Empty);
        }

        public SqlFromPart AddFrom(SqlFromPartOperator @operator, string text, string name)
        {
            var part = new SqlFromPart(@operator, text, name);
            From.Add(part);
            return part;
        }

        public SqlPart AddGroupBy(string text)
        {
            return AddGroupBy(text, string.Empty);
        }

        public SqlPart AddGroupBy(string text, string name)
        {
            var part = new SqlPart(text, name);
            GroupBy.Add(part);
            return part;
        }

        public SqlPart AddHaving(string text)
        {
            return AddHaving(text, string.Empty);
        }

        public SqlPart AddHaving(string text, string name)
        {
            var part = new SqlPart(text, name);
            Having.Add(part);
            return part;
        }

        public SqlPart AddOrderBy(string text)
        {
            return AddOrderBy(text, string.Empty);
        }

        public SqlPart AddOrderBy(string text, string name)
        {
            var part = new SqlPart(text, name);
            OrderBy.Add(part);
            return part;
        }

        public SqlPart AddSelect(string text)
        {
            return AddSelect(text, string.Empty);
        }

        public SqlPart AddSelect(string text, string name)
        {
            var part = new SqlPart(text, name);
            Select.Add(part);
            return part;
        }

        public SqlWherePart AddWhere(string text)
        {
            return AddWhere(SqlWherePartOperator.And, text);
        }

        public SqlWherePart AddWhere(SqlWherePartOperator @operator, string text)
        {
            return AddWhere(@operator, text, string.Empty);
        }

        public SqlWherePart AddWhere(string text, string name)
        {
            return AddWhere(SqlWherePartOperator.And, text, name);
        }

        public SqlWherePart AddWhere(SqlWherePartOperator @operator, string text, string name)
        {
            var part = new SqlWherePart(@operator, text, name);
            Where.Add(part);
            return part;
        }

        public string ExtractFieldNamesFromSelect()
        {
            var content = new StringBuilder();
            foreach (var part in Select)
            {
                content.Append(string.Format(" {0},", part.Text));
            }
            content.Remove(content.Length - 1, 1);
            const RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.ExplicitCapture;
            var regEx = new Regex(@"AS\s+\[[^\]]+\]", options);
            var matches = regEx.Matches(content.ToString());
            var regEx2 = new Regex(@"\[.+\]", options);
            var value = new StringBuilder();
            foreach (Match match in matches)
            {
                value.Append(regEx2.Match(match.Value).Value);
                value.Append(",");
            }
            value.Remove(value.Length - 1, 1);
            return value.ToString();
        }

        public void RemoveWhere(IEnumerable<string> names)
        {
            foreach (var name in names)
            {
                RemoveWhere(name);
            }
        }

        public void RemoveWhere(string name)
        {
            var sqlWherePart = Where.SingleOrDefault(p => p.Name.Trim().ToLower() == name.Trim().ToLower());
            if (sqlWherePart != null)
            {
                Where.Remove(sqlWherePart);
            }
        }

        public override string ToString()
        {
            return ToString(false);
        }

        public string ToString(bool count)
        {
            return CreateSql(count, "*");
        }

        public string ToString(bool count, string countClause)
        {
            return CreateSql(count, countClause);
        }

        private static string Adjust(string sql)
        {
            const RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.ExplicitCapture;
            var regEx = new Regex("\\s{2,}", options);
            sql = sql.Trim();
            sql = regEx.Replace(sql, " ");
            regEx = new Regex(",\\s*,", options);
            sql = regEx.Replace(sql, ",");
            return sql;
        }

        private string CreateSql(bool count, string countClause)
        {
            var content = new StringBuilder();
            content.Append("SELECT");
            if (Distinct)
            {
                content.Append(" DISTINCT ");
            }
            if (!count)
            {
                foreach (var part in Select)
                {
                    content.Append(string.Format(" {0},", part.Text));
                }
                content.Remove(content.Length - 1, 1);
            }
            else
            {
                content.Append(string.Format(" COUNT({0})", countClause));
            }
            content.Append(" FROM");
            foreach (var part in From)
            {
                var @operator = string.Empty;
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
                content.Append(string.Format(" {0} {1}", @operator, part.Text));
                if (part.NoLock)
                {
                    content.Append(" WITH (NOLOCK)");
                }
            }
            if (Where.Any())
            {
                content.Append(" WHERE");
                var first = true;
                foreach (var part in Where)
                {
                    var @operator = string.Empty;
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
                    content.Append(string.Format(" {0} ({1})", first ? string.Empty : @operator, part.Text));
                    first = false;
                }
            }
            if (GroupBy.Any())
            {
                content.Append(" GROUP BY");
                foreach (var part in GroupBy)
                {
                    content.Append(string.Format(" {0}", part.Text));
                }
            }
            if (Having.Any())
            {
                content.Append(" HAVING");
                foreach (var part in Having)
                {
                    content.Append(string.Format(" {0}", part.Text));
                }
            }
            var orderBy = string.Empty;
            if (OrderBy.Any() && !count)
            {
                foreach (var part in OrderBy)
                {
                    orderBy += string.Format(" {0}", part.Text);
                }
                if (!Paging.Enabled)
                {
                    content.Append(string.Format(" ORDER BY {0}", orderBy));
                }
            }
            var sql = content.ToString();

            if (Paging.Enabled && !count)
            {
                const string format = @"
                    SELECT 
                        {0}
                    FROM 
                        (SELECT ROW_NUMBER() OVER(ORDER BY {1}) AS RowNumber, {2}) AS Paging
                    WHERE 
                        RowNumber BETWEEN {3} AND {4}";
                var select = Paging.Select;
                if (string.IsNullOrWhiteSpace(select))
                {
                    select = ExtractFieldNamesFromSelect();
                }
                var from = Paging.PageIndex <= 1 ? 1 : ((Paging.PageIndex - 1)*Paging.PageSize) + 1;
                var to = from + Paging.PageSize - 1;
                sql = string.Format(
                    format,
                    select,
                    orderBy,
                    sql.Substring("SELECT ".Length),
                    from,
                    to);
            }

            sql = Adjust(sql);

            return sql;
        }
    }
}