namespace Compiler.Visitors.TypeChecker.Utils;

public class DuplicateKeyComparer<TKey> : IComparer<TKey> where TKey : IComparable
{
    #region IComparer<TKey> Members

    public int Compare(TKey x, TKey y)
    {
        int result = x.CompareTo(y);

        if (result == 0)
            return 1;         // Handle equality as being greater. Note: this will break Remove(key) or
        else                  // IndexOfKey(key) since the comparer never returns 0 to signal key equality
            return result;
    }

    #endregion
}