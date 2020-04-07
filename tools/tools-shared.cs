using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;


namespace RageStringsDatabase
{
    public enum HashType
    {
        Default,
        AWC
    };

    public class ToolsConstants
    {
        public const string APP_ID_OPENIV = "OPENIV";
    }

    public class HashManager
    {
        public static IEqualityComparer<StringLine> GetStringLineComparer(HashType hash)
        {
            switch (hash)
            {
                case HashType.Default: return new StringLineCaseInsensitiveComparer();
                case HashType.AWC: return new StringLineCaseInsensitiveComparer();
            }

            return null;
        }

        public static IEqualityComparer<string> GetStringComparer(HashType hash)
        {
            switch (hash)
            {
                case HashType.Default: return new StringCaseInsensitiveComparer();
                case HashType.AWC: return new StringCaseInsensitiveComparer();
            }

            return null;
        }

        public static Func<string, UInt32> GetHashFunction(HashType hash)
        {
            switch (hash)
            {
                case HashType.Default: return HashDefault;
                case HashType.AWC: return HashAWC;
            }

            return null;
        }

        public static UInt32 HashDefault(string stringLine)
        {
            stringLine = stringLine.ToLower();
            UInt32 tempHash = 0;

            for (int i = 0; i < stringLine.Length; i++)
            {
                tempHash += stringLine[i];
                tempHash += tempHash << 10;
                tempHash ^= tempHash >> 6;
            }

            tempHash += tempHash << 3;
            tempHash ^= tempHash >> 11;
            tempHash += tempHash << 15;

            return tempHash;
        }

        public static UInt32 HashAWC(string stringLine)
        {
            return HashDefault(stringLine) & 0x1FFFFFFF;
        }
    }

    public class StringLine
    {
        public string Value { get; set; }

        public string Container { get; set; }
    }

    public class StringLineCaseInsensitiveComparer : IEqualityComparer<StringLine>
    {
        public bool Equals(StringLine x, StringLine y)
        {
            return x.Value.Equals(y.Value, StringComparison.InvariantCultureIgnoreCase);
        }

        public int GetHashCode(StringLine obj)
        {
            return obj.Value.ToLower().GetHashCode();
        }
    }

    public class StringLineCaseSensitiveComparer : IEqualityComparer<StringLine>
    {
        public bool Equals(StringLine x, StringLine y)
        {
            return x.Value.Equals(y.Value, StringComparison.InvariantCulture);
        }

        public int GetHashCode(StringLine obj)
        {
            return obj.Value.GetHashCode();
        }
    }

    public class StringCaseInsensitiveComparer : IEqualityComparer<string>
    {
        public bool Equals(string x, string y)
        {
            return x.Equals(y, StringComparison.InvariantCultureIgnoreCase);
        }

        public int GetHashCode(string obj)
        {
            return obj.ToLower().GetHashCode();
        }
    }

    public class StringCaseSensitiveComparer : IEqualityComparer<string>
    {
        public bool Equals(string x, string y)
        {
            return x.Equals(y, StringComparison.InvariantCulture);
        }

        public int GetHashCode(string obj)
        {
            return obj.GetHashCode();
        }
    }
}
