namespace JDSpace
{
    /// <summary>
    /// One JDTempConstraint site 
    /// </summary>
    public interface IJDComparable
    {
        int Numel { get; }
        ScLinExpr GetScLinExpr(int i);
    }
}