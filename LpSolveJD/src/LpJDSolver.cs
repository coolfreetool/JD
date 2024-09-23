using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JDSpace;
using System.Diagnostics;
using JDUtils;
using LpSolveDotNet;
using System.IO;

namespace LpSolveJD
{
    public class LpJDSolver : IJDSolver
    {
        private int _nextColId = 1;
        public LpSolve Lp { get; private set; }
        public Dictionary<int, int> ColMap;
        public double[] Values;
        private LogFlags _logFlag = LogFlags.OPTIMIZER;
        private Logger _logger = null;
        public bool SupportsSOS1 => true;

        public bool SupportsSOS2 => true;
        public LpJDSolver()
        {
            LpSolve.Init();
            Lp = LpSolve.make_lp(0, 0);
            ColMap = new Dictionary<int, int>();
        }

        void AddScVar(ScVar scVar)
        {
            ColMap.Add(scVar.Id, _nextColId);
            Lp.add_column(new double[Lp.get_Ncolumns() + 1]);
            Lp.set_lowbo(_nextColId, scVar.Lb);
            Lp.set_upbo(_nextColId, scVar.Ub);
            if (scVar.Name != null)
            {
                Lp.set_col_name(_nextColId, scVar.Name);
            }
            if (scVar.Type == JD.BINARY)
            {
                Lp.set_binary(_nextColId, true); // variable must be binary
            }
            else if (scVar.Type == JD.INTEGER)
            {
                Lp.set_int(_nextColId, true); // variable must be integer
            }
            _nextColId++;
        }

        void IJDSolver.Reset()
        {
            Lp.delete_lp();
            Lp = LpSolve.make_lp(0, 0);
            _nextColId = 1;
            ColMap.Clear();
        }

        void IJDSolver.Update()
        {
            // nothing to do;
        }

        void AddConstr(ScConstr con)
        {
            int nVars = Lp.get_Ncolumns();
            double[] row = new double[nVars + 1];
            foreach (ScTerm term in con.Lhs.Terms)
            {
                row[ColMap[term.Var.Id]] += term.Coeff;
            }
            double rhs = -con.Lhs.Constant;
            switch (con.Sense)
            {
                case JD.EQUAL:
                    Lp.add_constraint(row, lpsolve_constr_types.EQ, rhs);
                    break;
                case JD.LESS_EQUAL:
                    Lp.add_constraint(row, lpsolve_constr_types.LE, rhs);
                    break;
                case JD.GREATER_EQUAL:
                    Lp.add_constraint(row, lpsolve_constr_types.GE, rhs);
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
            Lp.add_SOS("", sosCon.Type, prior, sosCon.Weights.Length, varsIds, sosCon.Weights);
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
            int nVars = Lp.get_Ncolumns();
            double[] objArr = new double[nVars + 1];
            foreach (ScTerm term in obj.Terms)
            {
                objArr[ColMap[term.Var.Id]] = term.Coeff;
            }
            Lp.set_obj_fn(objArr); // set obj fun

            if (sense == JD.MAXIMIZE) // set minimize or maximize
            {
                Lp.set_maxim();
            }
            else if (sense == JD.MINIMIZE)
            {
                Lp.set_minim();
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
            lpsolve_return result = Lp.solve();
            sw.Stop();
            _log(_logFlag, String.Format("Solving finished: {0}, in {1} seconds.", result, sw.Elapsed.TotalSeconds));
            pars.Set(JD.StringParam.STATUS, result.ToString());
            pars.Set(JD.StringParam.SOLVER_NAME, "LP SOLVE");
            int jdResult = 0;
            if ((result == lpsolve_return.OPTIMAL) ||
                (result == lpsolve_return.SUBOPTIMAL) ||
                (result == lpsolve_return.PRESOLVED))
            {
                jdResult = 1;
            }
            pars.Set(JD.IntParam.RESULT_STATUS, jdResult);
            pars.Set(JD.DoubleParam.SOLVER_TIME, sw.Elapsed.TotalSeconds);
            pars.Set(JD.DoubleParam.OBJ_VALUE, Lp.get_objective());
            Values = new double[Lp.get_Ncolumns()];
            Lp.get_variables(Values);
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
            if (pars.IsSet(JD.DoubleParam.TIME_LIMIT)) Lp.set_timeout((int)pars.Get<double>(JD.DoubleParam.TIME_LIMIT));
            if (pars.IsSet(JD.DoubleParam.MIP_GAP)) Lp.set_mip_gap(false, pars.Get<double>(JD.DoubleParam.MIP_GAP));
            if (pars.IsSet(JD.IntParam.OUT_FLAG))
            {
                if (pars.Get<int>(JD.IntParam.OUT_FLAG) > 0)
                {
                    Lp.set_verbose(lpsolve_verbosity.FULL);
                }
                else
                {
                    Lp.set_verbose(lpsolve_verbosity.NEUTRAL);
                }
            }
        }

        /// <summary>
        /// Export model to file
        /// </summary>
        /// <param name="filenameWithoutExtension">File name without extension</param>
        /// <param name="fileType">File type (mps, lp)</param>
        /// <returns>true if succeeded, false otherwise.</returns>    
        public bool Export(string filenameWithoutExtension, string fileType)
        {
            FileInfo fileInfo = new FileInfo($"{filenameWithoutExtension}.{fileType}");
            if (fileInfo.Exists && fileInfo.IsReadOnly)
            {
                return false;
            }
            string text = null;
            if (fileType == JD.LP)
            {
                return Lp.write_lp(fileInfo.FullName);
            }
            else
            {
                if (!(fileType == JD.MPS))
                {
                    throw new JDException("Unknown file type {0}", fileType);
                }

                return Lp.write_mps(fileInfo.FullName);
            }
        }
    }
}
