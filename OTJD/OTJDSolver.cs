using Google.OrTools.LinearSolver;
using JDSpace;
using JDUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OTJD
{
    public abstract class OTJDSolver : IJDSolver
    {
        internal abstract string name { get; }
        private Solver _solver;
        private Objective _objective;
        private Dictionary<int, Variable> _otVars;
        private Logger _logger;
        private string _solverType;

        internal OTJDSolver(string solverType)
        {
            Console.WriteLine("SOLVER CREATED");
            CustomLogger cl = new CustomLogger();
            Console.SetError(cl);
            _solver = Solver.CreateSolver(solverType);
            if (_solver == null)
            {
                throw new JDException("Can not create {0}", solverType);
            }
            Console.ReadKey(); // ... test logs print order, "SOLVER CREATED" should be written, but it's not
            _solverType = solverType;
            _objective = _solver.Objective();
            _otVars = new Dictionary<int, Variable>();
        }

        public void AddScVar(ScVar scVar)
        {
            Variable otVar;
            string varName = scVar.Name;
            if (varName == null) varName = String.Format("v{0}", scVar.Id);
            switch (scVar.Type)
            {
                case JD.CONTINUOUS:
                    otVar = _solver.MakeNumVar(scVar.Lb, scVar.Ub, varName);
                    break;
                case JD.BINARY:
                    otVar = _solver.MakeIntVar(0, 1, varName);
                    break;
                case JD.INTEGER:
                    otVar = _solver.MakeIntVar(scVar.Lb, scVar.Ub, varName);
                    break;
                default:
                    throw new JDException("Unknown var type: {0}", scVar.Type);
            }
            _otVars.Add(scVar.Id, otVar);
        }

        public void SetLogger(JDUtils.Logger logger)
        {
            _logger = logger;
        }

        public JDUtils.Logger GetLogger()
        {
            return _logger;
        }

        public void Update()
        {
            // nothing to do
        }

        public void Reset()
        {
            //_solver.Reset();
            _solver.Clear();
            _otVars.Clear();
        }

        public void AddConstr(ScConstr con)
        {
            Constraint otCon;
            double otRhs = -con.Lhs.Constant;
            switch(con.Sense)
            {
                case JD.LESS_EQUAL:
                    otCon = _solver.MakeConstraint(double.NegativeInfinity, otRhs);
                    break;
                case JD.GREATER_EQUAL:
                    otCon = _solver.MakeConstraint(otRhs, double.PositiveInfinity);
                    break;
                case JD.EQUAL:
                    otCon = _solver.MakeConstraint(otRhs, otRhs);
                    break;
                default:
                    throw new JDException("Unknown sense: {0}", con.Sense);
            }
            // precalculate expression (add coeffs of the same variables)
            Dictionary<int, double> simTerms = _simplify(con.Lhs.Terms);
            foreach (KeyValuePair<int, double> pair in simTerms)
            {
                otCon.SetCoefficient(_otVars[pair.Key], pair.Value);
            }
        }

        void IJDSolver.AddScVars(List<ScVar> vars)
        {
            foreach (ScVar var in vars) AddScVar(var);
        }

        void IJDSolver.AddConstrs(List<ScConstr> cons)
        {
            foreach (ScConstr con in cons) AddConstr(con);
        }

        /// <summary>
        /// Sum coeffs of the same variables.
        /// </summary>
        private Dictionary<int, double> _simplify(IList<ScTerm> terms)
        {
            Dictionary<int, double> simTerms = new Dictionary<int, double>();
            foreach (ScTerm term in terms)
            {
                if (simTerms.ContainsKey(term.Var.Id))
                {
                    simTerms[term.Var.Id] += term.Coeff;
                }
                else
                {
                    simTerms.Add(term.Var.Id, term.Coeff);
                }
            }
            return simTerms;
        }

        public void AddSOSConstr(SOSConstr sosCon)
        {
            throw new JDException("SOS constraints unsupported in OTSolvers");
        }

        public void SetObjective(ScLinExpr obj, int sense)
        {
            foreach (ScTerm term in obj.Terms)
            {
                _objective.SetCoefficient(_otVars[term.Var.Id], term.Coeff);
            }
            _objective.SetOffset(obj.Constant);

            switch (sense)
            {
                case JD.MAXIMIZE:
                    _objective.SetMaximization();
                    break;
                case JD.MINIMIZE:
                    _objective.SetMinimization();
                    break;
                default:
                    throw new JDException("Unknown optimization sense {0}", sense);
            }
        }

        public void Optimize(JDParams pars)
        {
            if (pars.IsSet(JD.IntParam.OUT_FLAG))
            {
                if (pars.Get<int>(JD.IntParam.OUT_FLAG) > 0)
                {
                    _solver.SetSolverSpecificParametersAsString("display/verblevel = 5");
                    _solver.SetSolverSpecificParametersAsString("display/lpinfo = TRUE");
                    _solver.EnableOutput();
                }
                else
                {
                    _solver.SuppressOutput();
                }
            }
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Solver.ResultStatus gStatus = _solver.Solve();
            sw.Stop();
            pars.Set(JD.StringParam.SOLVER_NAME, name);
            int jdStatus = 0;
            if (gStatus == Solver.ResultStatus.OPTIMAL) // optimal or suboptimal
            {
                jdStatus = 1;
            }
            pars.Set(JD.IntParam.RESULT_STATUS, jdStatus);
            pars.Set(JD.StringParam.STATUS, gStatus.ToString());
            pars.Set(JD.DoubleParam.SOLVER_TIME, sw.Elapsed.TotalSeconds);
            if (pars.Get<int>(JD.IntParam.RESULT_STATUS) > 0)
            {
                pars.Set(JD.DoubleParam.OBJ_VALUE, _objective.Value());
            }
        }

        public double? GetVarValue(int id)
        {
            return _otVars[id].SolutionValue();
        }
    }

    public class GlpkJDSolver : OTJDSolver 
    {
        internal override string name
        {
            get { return "GLPK"; }
        }
        public GlpkJDSolver()
            : base("GLPK_MIXED_INTEGER_PROGRAMMING"){}
    }

    public class CbcJDSolver : OTJDSolver
    {
        internal override string name
        {
            get { return "CBC"; }
        }
        public CbcJDSolver()
            : base("CBC_MIXED_INTEGER_PROGRAMMING"){}
    }

    public class ScipJDSolver : OTJDSolver
    {
        internal override string name
        {
            get { return "SCIP"; }
        }
        public ScipJDSolver()
            : base("SCIP"){}
    }

    public class SatJDSolver : OTJDSolver
    {
        internal override string name
        {
            get { return "SAT"; }
        }
        public SatJDSolver()
            : base("SAT"){}
    }
}

