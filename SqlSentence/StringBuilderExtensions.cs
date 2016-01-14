using System;
using System.Text;

namespace SqlSentence
{
    static class StringBuilderExtensions
    {
        public static void NewLine(this StringBuilder sb)
        {
            sb.Append(Environment.NewLine);
        }

        public static void NewLineIf(this StringBuilder sb, bool condition)
        {
            if (condition)
            {
                sb.Append(Environment.NewLine);
            }
        }
    }
}