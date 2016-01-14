namespace SqlSentence
{
    public class FromPart : Part
    {
        public FromPart(string value, string name, FromOperator @operator)
            : base(value, name)
        {
            Operator = @operator;
        }

        public bool NoLock { get; set; }

        public FromOperator Operator { get; set; }

        public override object Clone()
        {
            return MemberwiseClone();
        }
    }
}