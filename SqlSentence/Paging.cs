using System;

namespace SqlSentence
{
    public class Paging : ICloneable
    {
        public Paging()
        {
            PageIndex = 1;
            PageSize = 10;
            CteTotalCountFieldName = "TotalCount";
        }

        public bool Enabled { get; set; }

        public int PageIndex { get; set; }

        public int PageSize { get; set; }

        public bool UseCte { get; set; }

        public string CteTotalCountFieldName { get; set; }

        public int Offset
        {
            get
            {
                return (PageIndex - 1) * PageSize;
            }
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}