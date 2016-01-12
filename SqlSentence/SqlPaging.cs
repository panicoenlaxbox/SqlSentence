using System;

namespace SqlSentence
{
    public class SqlPaging : ICloneable
    {
        public SqlPaging()
            : this(1, 10, false)
        {
        }

        public SqlPaging(int pageIndex, int pageSize, bool enabled)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            Enabled = enabled;
        }

        public bool Enabled { get; set; }

        public int PageIndex { get; set; }

        public int PageSize { get; set; }

        public int Offset
        {
            get
            {
                if (PageIndex == 1)
                {
                    return 0;
                }
                return PageIndex * PageSize;
            }
        }

        public string Select { get; set; }

        public object Clone()
        {
            return new SqlPaging(PageIndex, PageSize, Enabled);
        }
    }
}