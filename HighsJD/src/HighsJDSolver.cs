using JDSpace;
using System;
using System.Collections.Generic;

namespace HighsJD {

    public class HighsJDSolver : IJDSolver
    {
        public bool SupportsSOS1 => false;
        public bool SupportsSOS2 => false;

        public HighsJDSolver() {
            
        }

        public void AddScVar(ScVar scVar) {
            throw new NotImplementedException();
        }

        public void SetLogger(JDUtils.Logger logger)
        {
            // _logger = logger;
            throw new NotImplementedException();
        }

        public JDUtils.Logger GetLogger()
        {
            // return _logger;
            throw new NotImplementedException();
        }

        public void Update()
        {
            // nothing to do
        }

        public void Reset()
        {
            //_solver.Reset();
            // _solver.Clear();
            // _otVars.Clear();
            throw new NotImplementedException();
        }

        public void AddConstr(ScConstr con)
        {
            // Constraint otCon;
            // double otRhs = -con.Lhs.Constant;
            // switch(con.Sense)
            // {
            //     case JD.LESS_EQUAL:
            //         otCon = _solver.MakeConstraint(double.NegativeInfinity, otRhs);
            //         break;
            //     case JD.GREATER_EQUAL:
            //         otCon = _solver.MakeConstraint(otRhs, double.PositiveInfinity);
            //         break;
            //     case JD.EQUAL:
            //         otCon = _solver.MakeConstraint(otRhs, otRhs);
            //         break;
            //     default:
            //         throw new JDException("Unknown sense: {0}", con.Sense);
            // }
            // // precalculate expression (add coeffs of the same variables)
            // Dictionary<int, double> simTerms = _simplify(con.Lhs.Terms);
            // foreach (KeyValuePair<int, double> pair in simTerms)
            // {
            //     otCon.SetCoefficient(_otVars[pair.Key], pair.Value);
            // }
            throw new NotImplementedException();
        }

        void IJDSolver.AddScVars(List<ScVar> vars)
        {
            // foreach (ScVar var in vars) AddScVar(var);
            throw new NotImplementedException();
        }

        void IJDSolver.AddConstrs(List<ScConstr> cons)
        {
            // foreach (ScConstr con in cons) AddConstr(con);
            throw new NotImplementedException();
        }


        public void AddSOSConstr(SOSConstr sosCon)
        {
            // throw new JDException("SOS constraints unsupported in OTSolvers");
            throw new NotImplementedException();
        }

        public void SetObjective(ScLinExpr obj, int sense)
        {
            throw new NotImplementedException();
            // foreach (ScTerm term in obj.Terms)
            // {
            //     _objective.SetCoefficient(_otVars[term.Var.Id], term.Coeff);
            // }
            // _objective.SetOffset(obj.Constant);

            // switch (sense)
            // {
            //     case JD.MAXIMIZE:
            //         _objective.SetMaximization();
            //         break;
            //     case JD.MINIMIZE:
            //         _objective.SetMinimization();
            //         break;
            //     default:
            //         throw new JDException("Unknown optimization sense {0}", sense);
            // }
        }

        public void Optimize(JDParams pars)
        {
            throw new NotImplementedException();
            // Stopwatch sw = new Stopwatch();
            // sw.Start();
            // Solver.ResultStatus gStatus = _solver.Solve();
            // sw.Stop();
            // pars.Set(JD.StringParam.SOLVER_NAME, name);
            // int jdStatus = 0;
            // if (gStatus == Solver.ResultStatus.OPTIMAL) // optimal or suboptimal
            // {
            //     jdStatus = 1;
            // }
            // pars.Set(JD.IntParam.RESULT_STATUS, jdStatus);
            // pars.Set(JD.StringParam.STATUS, gStatus.ToString());
            // pars.Set(JD.DoubleParam.SOLVER_TIME, sw.Elapsed.TotalSeconds);
            // if (pars.Get<int>(JD.IntParam.RESULT_STATUS) > 0)
            // {
            //     pars.Set(JD.DoubleParam.OBJ_VALUE, _objective.Value());
            // }
        }

        public double? GetVarValue(int id)
        {
            // return _otVars[id].SolutionValue();
            throw new NotImplementedException();
        }

        public bool Export(string filenameWithoutExtension, string fileType) {
            throw new NotImplementedException();
        }
    }
}