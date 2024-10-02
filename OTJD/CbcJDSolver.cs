namespace OTJD
{
    public class CbcJDSolver : OTJDSolver
    {
        internal override string name
        {
            get { return "CBC"; }
        }
        public CbcJDSolver()
            : base("CBC_MIXED_INTEGER_PROGRAMMING") { }
    }
}