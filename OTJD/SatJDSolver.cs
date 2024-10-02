namespace OTJD
{
    public class SatJDSolver : OTJDSolver
    {
        internal override string name
        {
            get { return "SAT"; }
        }
        public SatJDSolver()
            : base("SAT") { }
    }
}