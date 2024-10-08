﻿using Google.OrTools.LinearSolver;
using JDSpace;
using JDUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace OTJD
{
    public abstract class OTJDSolver : IJDSolver, IDisposable
    {
        internal abstract string name { get; }
        private Solver _solver;
        private Objective _objective;
        private Dictionary<int, Variable> _otVars;
        private Logger _logger;
        private string _solverType;
        public bool SupportsSOS1 => false;
        public bool SupportsSOS2 => false;
        internal OTJDSolver(string solverType)
        {
            _solverType = solverType;
            init();
        }

        private void init()
        {
            _solver = Solver.CreateSolver(_solverType);
            if (_solver == null)
            {
                throw new JDException("Can not create {0}", _solverType);
            }
            _objective = _solver.Objective();
            _otVars = new Dictionary<int, Variable>();
            _solver.EnableOutput();
        }
        private void reset()
        {
            _objective.Clear();
            _otVars.Clear();
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
            reset();
            init();
        }

        public void AddConstr(ScConstr con)
        {
            Constraint otCon;
            double otRhs = -con.Lhs.Constant;
            switch (con.Sense)
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
                    _solver.EnableOutput();
                }
                else
                {
                    _solver.SuppressOutput();
                }
            }
            Stopwatch sw = new Stopwatch();
            sw.Start();
            
            ConfigureSolver(pars);
            MPSolverParameters solverParams = new MPSolverParameters();
            if (pars.IsSet("MIP_GAP"))
            {
                solverParams.SetDoubleParam(MPSolverParameters.DoubleParam.RELATIVE_MIP_GAP, pars.Get<double>("MIP_GAP"));
            }
            Solver.ResultStatus gStatus = _solver.Solve(solverParams);
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

        public void ConfigureSolver(JDParams pars)
        {
            if (pars.IsSet("TIME_LIMIT"))
            {
                _solver.SetTimeLimit((int)pars.Get<double>("TIME_LIMIT") * 1000);
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
            string text = null;
            if (fileType == JD.LP)
            {
                text = _solver.ExportModelAsLpFormat(false);
            }
            else
            {
                if (!(fileType == JD.MPS))
                {
                    throw new JDException("Unknown file type {0}", fileType);
                }

                text = _solver.ExportModelAsMpsFormat(true, false);
            }

            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            FileInfo fileInfo = new FileInfo($"{filenameWithoutExtension}.{fileType}");
            if (fileInfo.Exists && fileInfo.IsReadOnly)
            {
                return false;
            }

            using (StreamWriter streamWriter = new StreamWriter(fileInfo.FullName))
            {
                streamWriter.Write(text);
            }

            return true;
        }

        public void Dispose()
        {
            reset();
        }

        public void Interrupt()
        {
            _solver.InterruptSolve();
        }
    }
}

