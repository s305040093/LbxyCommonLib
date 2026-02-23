#pragma warning disable CS1591
#pragma warning disable SA1633
#pragma warning disable SA1600
#pragma warning disable SA1649
#pragma warning disable SA1101
#pragma warning disable SA1204
#pragma warning disable SA1518
#pragma warning disable SA1201
#pragma warning disable SA1602
#pragma warning disable SA1601

namespace LbxyCommonLib.ExcelImport
{
    using System.Collections.Generic;

    public sealed partial class ExcelImporter
    {
        private sealed class RowKeyComparer : IEqualityComparer<object[]>
        {
            public bool Equals(object[] x, object[] y)
            {
                if (ReferenceEquals(x, y))
                {
                    return true;
                }

                if (x == null || y == null || x.Length != y.Length)
                {
                    return false;
                }

                for (var i = 0; i < x.Length; i++)
                {
                    if (!Equals(x[i], y[i]))
                    {
                        return false;
                    }
                }

                return true;
            }

            public int GetHashCode(object[] obj)
            {
                if (obj == null)
                {
                    return 0;
                }

                var hash = 17;
                for (var i = 0; i < obj.Length; i++)
                {
                    var value = obj[i];
                    var valueHash = value == null ? 0 : value.GetHashCode();
                    unchecked
                    {
                        hash = (hash * 31) + valueHash;
                    }
                }

                return hash;
            }
        }
    }
}
