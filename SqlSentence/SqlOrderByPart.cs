namespace SqlSentence
{
    public class SqlOrderByPart : SqlPart
    {       
        public SqlOrderByPart(string value, string name, SqlOrderByDirection direction)
            : base(value, name)
        {
            Direction = direction;
        }

        public SqlOrderByDirection Direction { get; set; }

        public override object Clone()
        {
            return new SqlOrderByPart(Value, Name, Direction);
        }
    }
}