namespace LingYan.Extension.Comparers
{
    public class StringSortComparer : IComparer<string>
    {
        public bool MatchCase { get; }
        public StringSortComparer(bool matchCase)
        {
            MatchCase = matchCase;
        }
        private int CharCompare(char a, char b, bool matchCase)
        {
            char _a = char.MinValue, _b = char.MinValue;
            if (matchCase) { _a = a; _b = b; }
            else { _a = char.ToUpper(a); _b = char.ToUpper(b); }
            if (_a > _b) return 1;
            if (_a < _b) return -1;
            return 0;
        }
        public int Compare(string x, string y)
        {
            int len;
            if (x.Length > y.Length) len = x.Length;
            else len = y.Length;
            string numericx = "";
            string numericy = "";
            for (int i = 0; i < len; i++)
            {
                char cx = char.MinValue;
                char cy = char.MinValue;
                if (i < x.Length) cx = x[i];
                if (i < y.Length) cy = y[i];
                if (cx >= 48 && cx <= 57) numericx += cx;
                if (cy >= 48 && cy <= 57) numericy += cy;
                if (i == len - 1)
                {
                    if (numericx.Length > 0 && numericy.Length > 0)
                    {
                        if (decimal.Parse(numericx) < decimal.Parse(numericy)) return -1;
                        if (decimal.Parse(numericx) > decimal.Parse(numericy)) return 1;
                    }
                    return CharCompare(cy, cy, MatchCase);
                }
                if ((cx >= 48 && cx <= 57) && (cy >= 48 && cy <= 57)) continue;
                if (numericx.Length > 0 && numericy.Length > 0)
                {
                    if (decimal.Parse(numericx) < decimal.Parse(numericy)) return -1;
                    if (decimal.Parse(numericx) > decimal.Parse(numericy)) return 1;
                }
                if (CharCompare(cx, cy, MatchCase) == 0) continue;
                return CharCompare(cx, cy, MatchCase);
            }
            return 0;
        }
    }
}
