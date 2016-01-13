namespace SqlSentence
{
    public class SqlWherePart : SqlPart
    {
        public SqlWherePart(string value, string name, SqlWherePartOperator @operator)
            : base(value, name)
        {
            Operator = @operator;
        }

        public SqlWherePartOperator Operator { get; set; }

        public override object Clone()
        {
            return new SqlWherePart(Value, Name, Operator);
        }
    }
}