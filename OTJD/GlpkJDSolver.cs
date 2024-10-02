namespace OTJD
{
    public class GlpkJDSolver : OTJDSolver
    {
        internal override string name
        {
            get { return "GLPK"; }
        }
        public GlpkJDSolver()
            : base("GLPK_MIXED_INTEGER_PROGRAMMING") { }
    }
}