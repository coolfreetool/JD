using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JDSpace;
using lpsolve55;
using System.Diagnostics;
using JDUtils;

namespace LpSolveJD
{
    public class LpJDSolver : IJDSolver
    {
        private int _nextColId = 1;
        public int LpId { get; private set; }
        public Dictionary<int, int> ColMap;
        public double[] Values;
        private LogFlags _logFlag = LogFlags.OPTIMIZER;
        private Logger _logger = null;

        public LpJDSolver()
        {
            LpId = lpsolve.make_lp(0, 0);
            ColMap = new Dictionary<int, int>();
        }

        public LpJDSolver(string outFile)
            : this()
        {
            lpsolve.set_outputfile(LpId, outFile);
        }

        void AddScVar(ScVar scVar)
        {
            ColMap.Add(scVar.Id, _nextColId);            
            lpsolve.add_column(LpId, new double[lpsolve.get_Nrows(LpId) + 1]);
            lpsolve.set_lowbo(LpId, _nextColId, scVar.Lb);
            lpsolve.set_upbo(LpId, _nextColId, scVar.Ub);
            if (scVar.Name != null)
            {
                lpsolve.set_col_name(LpId, _nextColId, scVar.Name);
            }
            if (scVar.Type == JD.BINARY)
            {
                lpsolve.set_binary(LpId, _nextColId, true); // variable must be binary
            }
            else if (scVar.Type == JD.INTEGER)
            {
                lpsolve.set_int(LpId, _nextColId, true); // variable must be integer
            }
            _nextColId++;
        }

        void IJDSolver.Reset()
        {            
            lpsolve.delete_lp(LpId);
            LpId = lpsolve.make_lp(0, 0);
            _nextColId = 1;
            ColMap.Clear();
        }

        void IJDSolver.Update()
        {
            // nothing to do;
        }

        void AddConstr(ScConstr con)
        {            
            int nVars = lpsolve.get_Ncolumns(LpId);
            double[] row = new double[nVars + 1];
            foreach (ScTerm term in con.Lhs.Terms)
            {
                row[ColMap[term.Var.Id]] += term.Coeff;
            }
            double rhs = -con.Lhs.Constant;
            switch (con.Sense)
            {
                case JD.EQUAL:
                    lpsolve.add_constraint(LpId, row, lpsolve.lpsolve_constr_types.EQ, rhs);
                    break;
                case JD.LESS_EQUAL:
                    lpsolve.add_constraint(LpId, row, lpsolve.lpsolve_constr_types.LE, rhs);
                    break;
                case JD.GREATER_EQUAL:
                    lpsolve.add_constraint(LpId, row, lpsolve.lpsolve_constr_types.GE, rhs);
                    break;
                default:
                    throw new Exception("Unknown constraint sense type: " + con.Sense);
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

        void IJDSolver.AddSOSConstr(SOSConstr sosCon)
        {
            int prior = 1;
            int[] varsIds = new int[sosCon.Weights.Length];
            for (int i = 0; i < varsIds.Length; i++)
            {
                varsIds[i] = ColMap[sosCon.Vars[i].Id];
            }
            lpsolve.add_SOS(LpId, "", sosCon.Type, prior, sosCon.Weights.Length, varsIds, sosCon.Weights); 
        }

        Logger IJDSolver.GetLogger()
        {
            return _logger;
        }

        public void SetLogger(Logger logger)
        {
            _logger = logger;
        }

        private void _log(LogFlags flags, string message, params Param[] parms)
        {
            if (_logger != null)
            {
                _logger.Log(flags, message, parms);
            }
        }
        
        private void _log(LogFlags flags, string message)
        {
            if (_logger != null)
            {
                _logger.Log(flags, message);
            }
        }

        void IJDSolver.SetObjective(ScLinExpr obj, int sense)
        {
            int nVars = lpsolve.get_Ncolumns(LpId);
            double[] objArr = new double[nVars + 1];
            foreach (ScTerm term in obj.Terms)
            {
                objArr[ColMap[term.Var.Id]] = term.Coeff;
            }
            lpsolve.set_obj_fn(LpId, objArr); // set obj fun

            if (sense == JD.MAXIMIZE) // set minimize or maximize
            {
                lpsolve.set_maxim(LpId);
            }
            else if (sense == JD.MINIMIZE)
            {
                lpsolve.set_minim(LpId);
            }
            else
            {
                throw new Exception("Unknown optim. func. sense: " + sense);
            }
        }

        void IJDSolver.Optimize(JDParams pars)
        {
            ConfigureLpSolve(pars);
            Stopwatch sw = new Stopwatch();
            _log(_logFlag, "Problem solving");
            sw.Start();            
            lpsolve.lpsolve_return result = lpsolve.solve(LpId);
            sw.Stop();
            _log(_logFlag, String.Format("Solving finished: {0}, in {1} seconds.", result, sw.Elapsed.TotalSeconds));
            pars.Set(JD.StringParam.STATUS, result.ToString());
            pars.Set(JD.StringParam.SOLVER_NAME, "LP SOLVE");
            int jdResult = 0;
            if((result ==  lpsolve.lpsolve_return.OPTIMAL) ||
                (result ==  lpsolve.lpsolve_return.SUBOPTIMAL) ||
                (result ==  lpsolve.lpsolve_return.PRESOLVED)){
                jdResult = 1;
            }
            pars.Set(JD.IntParam.RESULT_STATUS, jdResult);
            pars.Set(JD.DoubleParam.SOLVER_TIME, sw.Elapsed.TotalSeconds);
            pars.Set(JD.DoubleParam.OBJ_VALUE, lpsolve.get_objective(LpId));
            Values = new double[lpsolve.get_Ncolumns(LpId)];
            lpsolve.get_variables(LpId, Values);
        }

        double? IJDSolver.GetVarValue(int id)
        {
            try
            {
                double val = Values[ColMap[id] - 1];
                return val;
            }
            catch (Exception e)
            {
                Console.WriteLine("Can't take value of variable: {0}", id);
                return null;
            }
        }

        public void ConfigureLpSolve(JDParams pars)
        {
            _log(_logFlag, "Solver configuring");
            if (pars.IsSet(JD.DoubleParam.TIME_LIMIT)) lpsolve.set_timeout(LpId, (int)pars.Get<double>(JD.DoubleParam.TIME_LIMIT));
            if (pars.IsSet(JD.DoubleParam.MIP_GAP)) lpsolve.set_mip_gap(LpId, false, pars.Get<double>(JD.DoubleParam.MIP_GAP));
            if (pars.IsSet(JD.IntParam.OUT_FLAG))
            {
                if (pars.Get<int>(JD.IntParam.OUT_FLAG) > 0)
                {
                    lpsolve.set_verbose(LpId, 4);
                }
                else
                {
                    lpsolve.set_verbose(LpId, 0);
                }
            }
        }
    }
}
