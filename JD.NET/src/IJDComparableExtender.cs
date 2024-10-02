namespace JDSpace
{
    internal static class IJDComparableExtender
    {
        internal static ScLinExprFactory GetScFactory(this IJDComparable t)
        {
            if (t is JdConstant) return (t as JdConstant).ScLinExprFactory;
            return (t as JDElement).ScLinExprFactory;
        }
    }
}