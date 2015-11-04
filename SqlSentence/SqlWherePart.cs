namespace SqlSentence
{
    public class SqlWherePart : SqlPart
    {
        public SqlWherePart()
        {
        }

        public SqlWherePart(SqlWherePartOperator @operator, string text)
            : this(@operator, text, string.Empty)
        {
        }

        public SqlWherePart(SqlWherePartOperator @operator, string text, string name)
            : base(text, name)
        {
            Operator = @operator;
        }

        public SqlWherePartOperator Operator { get; set; }

        public override object Clone()
        {
            return new SqlWherePart(Operator, Text, Name);
        }
    }
}