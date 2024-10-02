namespace OTJD
{
    public class ScipJDSolver : OTJDSolver
    {
        internal override string name
        {
            get { return "SCIP"; }
        }
        public ScipJDSolver()
            : base("SCIP") { }
    }
}