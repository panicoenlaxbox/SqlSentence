using System;

namespace SqlSentence
{
    public class SqlPart : ICloneable
    {
        public SqlPart(string value)
            : this(value, null)
        {
        }

        public SqlPart(string value, string name)
        {
            Value = value;
            Name = name;
        }

        public string Name { get; set; }

        public string Value { get; set; }

        public virtual object Clone()
        {
            return new SqlPart(Value, Name);
        }
    }
}