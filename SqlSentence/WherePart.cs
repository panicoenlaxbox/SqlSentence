namespace SqlSentence
{
    public class WherePart : Part
    {
        public WherePart(string value, string name, WhereOperator @operator)
            : base(value, name)
        {
            Operator = @operator;
        }

        public WhereOperator Operator { get; set; }

        public override object Clone()
        {
            return MemberwiseClone();
        }
    }
}