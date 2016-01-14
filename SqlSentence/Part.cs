using System;

namespace SqlSentence
{
    public class Part : ICloneable
    {
        public Part(string value, string name)
        {
            Value = value;
            Name = name;
        }

        public string Name { get; set; }

        public string Value { get; set; }

        public virtual object Clone()
        {
            return MemberwiseClone();
        }
    }
}