namespace SqlSentence
{
    public class SqlFromPart : SqlPart
    {
        public SqlFromPart(string value, string name, SqlFromPartOperator @operator)
            : base(value, name)
        {
            Operator = @operator;
        }

        public bool NoLock { get; set; }

        public SqlFromPartOperator Operator { get; set; }

        public override object Clone()
        {
            return new SqlFromPart(Value, Name, Operator) { NoLock = NoLock };
        }
    }
}