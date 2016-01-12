using System;

namespace SqlSentence
{
    public class SqlPart : ICloneable
    {
        public SqlPart()
        {
        }

        public SqlPart(string text)
            : this(text, null)
        {
        }

        public SqlPart(string text, string name)
        {
            Text = text;
            Name = name;
        }

        public string Name { get; set; }

        public string Text { get; set; }

        public virtual object Clone()
        {
            return new SqlPart(Text, Name);
        }
    }
}