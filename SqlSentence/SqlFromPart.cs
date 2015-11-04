namespace SqlSentence
{
    public class SqlFromPart : SqlPart
    {
        public SqlFromPart()
        {
        }

        public SqlFromPart(SqlFromPartOperator @operator, string text)
            : this(@operator, text, string.Empty)
        {
        }

        public SqlFromPart(SqlFromPartOperator @operator, string text, string name)
            : this(@operator, text, name, false)
        {
            Operator = @operator;
        }

        public SqlFromPart(SqlFromPartOperator @operator, string text, string name, bool noLock)
            : base(text, name)
        {
            Operator = @operator;
            NoLock = noLock;
        }

        public bool NoLock { get; set; }

        public SqlFromPartOperator Operator { get; set; }

        public override object Clone()
        {
            return new SqlFromPart(Operator, Text, Name, NoLock);
        }
    }
}