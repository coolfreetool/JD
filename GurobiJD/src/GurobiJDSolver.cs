using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gurobi;
using System.Diagnostics;
using JDUtils;
using JDSpace;
using System.Threading.Tasks;

namespace GurobiJD
{
    public class GurobiJDSolver : IJDSolver
    {
        /// <summary>
        /// Gurobi enviroment
        /// </summary>
        public GRBEnv Env { get; private set; }
        /// <summary>
        /// Gurobi model
        /// </summary>
        public GRBModel Model { get; private set; }

        private Dictionary<int, int> _branchPriors;
        private JDGRBCallback _jdGRBCallback;
        private int _nBinVars = 0;

        /// <summary>
        /// Gurobi variables array
        /// </summary>
        public GRBVar[] GVars2;
        /// <summary>
        /// List with lazy ScConstraints
        /// </summary>
        public IList<ScConstr> LazyConstrs;
        /// <summary>
        /// Lazy constraints in GRBLinExpr array
        /// </summary>
        public GRBLinExpr[] LazyConstrsExp;
        /// <summary>
        /// ScVar to GurobiVar map
        /// </summary>
        public Dictionary<int, int> ScVarId_GVarIdx;

        /// <summary>
        /// Create Gurobi solver instance
        /// </summary>
        public GurobiJDSolver()
        {
            Env = new GRBEnv();
            Model = new GRBModel(Env);
            _jdGRBCallback = new JDGRBCallback(this);
            Model.SetCallback(_jdGRBCallback);
            _branchPriors = new Dictionary<int, int>();
            LazyConstrs = new List<ScConstr>();
        }

        void IJDSolver.SetLogger(Logger logger)
        {
            if (_jdGRBCallback != null)
            {
                _jdGRBCallback.SetLogger(logger);
            }
        }

        Logger IJDSolver.GetLogger()
        {
            if (_jdGRBCallback != null)
            {
                return _jdGRBCallback._logger;
            }
            else
            {
                return null;
            }
        }

        #region << ADD ALL VARS AND CONSTRAINTS >>
        /// <summary>
        /// Add list of ScVars to Gurobi
        /// </summary>
        /// <param name="vars">List with ScVars</param>
        public void AddScVars(List<ScVar> vars)
        {
            ScVarId_GVarIdx = new Dictionary<int, int>(vars.Count);
            double[] lbs = new double[vars.Count];
            double[] ubs = new double[vars.Count];
            char[] gTypes = new char[vars.Count];
            string[] names = new string[vars.Count];
            Parallel.For(0, vars.Count, (idx) =>
            {
                ScVar var = vars[idx];
                switch (var.Type)
                {
                    case JD.BINARY:
                        gTypes[idx] = GRB.BINARY;
                        break;
                    case JD.INTEGER:
                        gTypes[idx] = GRB.INTEGER;
                        break;
                    default:
                        gTypes[idx] = GRB.CONTINUOUS;
                        break;
                }
                if (var.Lb == JD.INFINITY)
                {
                    lbs[idx] = GRB.INFINITY;
                }
                else
                {
                    lbs[idx] = var.Lb;
                }
                if (var.Ub == -JD.INFINITY)
                {
                    ubs[idx] = -GRB.INFINITY;
                }
                else
                {
                    ubs[idx] = var.Ub;
                }
                lock (ScVarId_GVarIdx) ScVarId_GVarIdx.Add(var.Id, idx);
                names[idx] = var.Name;
                // save nonzero optional variable branch priority
                if (var.BranchPriority > 0)
                {
                    lock (_branchPriors) _branchPriors.Add(var.Id, var.BranchPriority);
                }
                if (var.Type == JD.BINARY)
                {
                    _nBinVars++; // pocitani poctu binarnich promennych
                }
            });
            GVars2 = Model.AddVars(lbs, ubs, new double[vars.Count], gTypes, names);
        }

        /// <summary>
        /// Add List of ScConstraints to Gurobi
        /// </summary>
        /// <param name="constrs">List with ScConstraints</param>
        public void AddConstrs(List<ScConstr> constrs)
        {
            // Count the number of lazy constraints
            int LazyCount = 0;
            Parallel.For(0, constrs.Count, (idx) =>
            {
                if (constrs[idx].LazyLevel > 0)
                    LazyCount++;
            });

            // Divide to lazy and ordinary constraints
            IList<ScConstr> nonLazyConstrs = new List<ScConstr>(constrs.Count - LazyCount);
            LazyConstrs = new List<ScConstr>(LazyCount);
            if (LazyCount > 0)
            {
                Parallel.For(0, constrs.Count, (idx) =>
                {
                    if (constrs[idx].LazyLevel > 0)
                    { lock (LazyConstrs) LazyConstrs.Add(constrs[idx]); }
                    else
                    { lock (nonLazyConstrs) nonLazyConstrs.Add(constrs[idx]); }
                });
            }
            else
            {
                nonLazyConstrs = constrs; // Direct assignment
            }

            GRBLinExpr[] lhs = new GRBLinExpr[nonLazyConstrs.Count];
            char[] sense = new char[nonLazyConstrs.Count];
            double[] rhs = new double[nonLazyConstrs.Count];
            string[] names = new string[nonLazyConstrs.Count];


            Parallel.For(0, nonLazyConstrs.Count, (idx, loopState) =>
            {
                switch (nonLazyConstrs[idx].Sense)
                {
                    case JD.GREATER_EQUAL:
                        sense[idx] = GRB.GREATER_EQUAL;
                        break;
                    case JD.LESS_EQUAL:
                        sense[idx] = GRB.LESS_EQUAL;
                        break;
                    default:
                        sense[idx] = GRB.EQUAL;
                        break;
                }
                lhs[idx] = scLinExpr2GRBLinExpr2(nonLazyConstrs[idx].Lhs);
            });
            Model.AddConstrs(lhs, sense, rhs, names);
        }

        /// <summary>
        /// Converts ScLinExpr to GRBLinExpr
        /// </summary>
        /// <param name="scLinExpr">Scalar linear expression</param>
        /// <returns>Gurobi linear expression</returns>
        internal GRBLinExpr scLinExpr2GRBLinExpr2(ScLinExpr scLinExpr)
        {
            if (scLinExpr == null)
                throw new JDException("Scalar linear expression is null.");
            GRBLinExpr gLinExpr = new GRBLinExpr(scLinExpr.Constant);
            foreach (ScTerm term in scLinExpr.Terms)
            {
                gLinExpr.AddTerm(term.Coeff, GVars2[ScVarId_GVarIdx[term.Var.Id]]);
            }
            return gLinExpr;
        }
        #endregion

        /// <summary>
        /// Update model - Branch priorities are set to the model variables
        /// </summary>
        void IJDSolver.Update()
        {
            Model.Update();
            // Set additional variables options
            // branch priorities
            foreach (KeyValuePair<int, int> varPriority in _branchPriors)
            {
                //Console.WriteLine("nastavuji prioritu {0} promenne {1}.", varPriority.Value, varPriority.Key);
                GVars2[ScVarId_GVarIdx[varPriority.Key]].Set(GRB.IntAttr.BranchPriority, varPriority.Value);
            }
            //Model.Update();
        }

        /// <summary>
        /// Reset of the JD solver. Enviroment and model with all parameters are cleared.
        /// </summary>
        void IJDSolver.Reset()
        {
            Model.Dispose();
            Env.Dispose();
            Env = new GRBEnv();
            Model = new GRBModel(Env);
            GVars2 = null;
            if (ScVarId_GVarIdx != null) ScVarId_GVarIdx.Clear();

            _branchPriors.Clear();
            LazyConstrs.Clear();
            Model.SetCallback(_jdGRBCallback);
            _nBinVars = 0;
        }

        //void IJDSolver.Reset()
        //{
        //    Model.Dispose();
        //    Env.Dispose();
        //    Env = new GRBEnv();
        //    Model = new GRBModel(Env);
        //    GVars.Clear();
        //    _branchPriors.Clear();
        //    LazyConstrs.Clear();
        //    Model.SetCallback(_jdGRBCallback);
        //    _nBinVars = 0;
        //}

        //private void _addConstrToGurobi(ScConstr constr)
        //{
        //    char grbSense;
        //    switch (constr.Sense)
        //    {
        //        case JD.GREATER_EQUAL:
        //            grbSense = GRB.GREATER_EQUAL;
        //            break;
        //        case JD.LESS_EQUAL:
        //            grbSense = GRB.LESS_EQUAL;
        //            break;
        //        default:
        //            grbSense = GRB.EQUAL;
        //            break;
        //    }
        //    GRBLinExpr gLhs = ScLinExpr2GRBLinExpr(constr.Lhs);
        //    lock (Model)
        //    {
        //        Model.AddConstr(gLhs, grbSense, 0, constr.Name);
        //    }
        //}

        //void IJDSolver.AddConstr(ScConstr constr)
        //{
        //    if (constr.LazyLevel == 0)
        //    {
        //        _addConstrToGurobi(constr);
        //    }
        //    else
        //    {
        //        LazyConstrs.Add(constr);
        //    }
        //}

        /// <summary>
        /// Add SOS constraints to the model.
        /// </summary>
        /// <param name="sosCon">SOS constraint</param>
        void IJDSolver.AddSOSConstr(SOSConstr sosCon)
        {
            GRBVar[] gVarArr = new GRBVar[sosCon.Vars.Count];
            for (int i = 0; i < gVarArr.Length; i++)
            {
                //gVarArr[i] = GVars[sosCon.Vars[i].Id];
                gVarArr[i] = GVars2[ScVarId_GVarIdx[sosCon.Vars[i].Id]];
            }
            Model.AddSOS(gVarArr, sosCon.Weights, sosCon.Type);
        }

        //public GRBLinExpr ScLinExpr2GRBLinExpr(ScLinExpr scLinExpr)
        //{
        //    GRBLinExpr gLinExpr = new GRBLinExpr(scLinExpr.Constant);
        //    foreach (ScTerm term in scLinExpr.Terms.Values)
        //    {
        //        gLinExpr.AddTerm(term.Coeff, GVars[term.Var.Id]);
        //    }
        //    return gLinExpr;
        //}

        //public GRBLinExpr ScLinExpr2GRBLinExpr(ScLinExpr scLinExpr)
        //{
        //    GRBLinExpr gLinExpr = new GRBLinExpr(scLinExpr.Constant);
        //    foreach (ScTerm term in scLinExpr.Terms)
        //    {
        //        gLinExpr.AddTerm(term.Coeff, GVars[term.Var.Id]);
        //    }
        //    return gLinExpr;
        //}

        /// <summary>
        /// Set objective to the model
        /// </summary>
        /// <param name="scObj">Objective function in form of a linear scalar expression</param>
        /// <param name="sense">Sense. Possible values: JD.MINIMIZE, JD.MAXIMIZE</param>
        void IJDSolver.SetObjective(ScLinExpr scObj, int sense)
        {
            //GRBLinExpr gLinExpr = ScLinExpr2GRBLinExpr(scObj);
            if (scObj == null)
                throw new JDException("GurobiJD - Objective is null.");
            GRBLinExpr gLinExpr = null;
            try { gLinExpr = scLinExpr2GRBLinExpr2(scObj); }
            catch (Exception ex)
            { throw new JDException("GurobiJD - Objective creation failed.", ex); }
            if (sense == JD.MINIMIZE)
            {
                Model.SetObjective(gLinExpr, GRB.MINIMIZE);
            }
            else
            {
                Model.SetObjective(gLinExpr, GRB.MAXIMIZE);
            }
        }

        /// <summary>
        /// Start optimization of already initialized model
        /// </summary>
        /// <param name="pars">Optimization parameters (Gurobi parameters)</param>
        void IJDSolver.Optimize(JDParams pars)
        {
            ConfigureGurobi(pars);
            if (pars.IsSet(JD.StringParam.WRITE_TO_FILE))
            {
                try
                {
                    Model.Update();
                    //Model.Write(pars.Get<string>(JD.StringParam.WRITE_TO_FILE));
                }
                catch (Exception ex)
                {
                    _jdGRBCallback._logger.Log(LogFlags.OPTIMIZER, "Model updating error: " + ex.Message);
                    throw new JDException("Model updating error.", ex);
                }
            }
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Model.Optimize();
            sw.Stop();
            pars.Set(JD.StringParam.SOLVER_NAME, "GUROBI");
            #region << GRB.IntAttr.Status description >>
            //  LOADED	        1	Model is loaded, but no solution information is available.
            //  OPTIMAL	        2	Model was solved to optimality (subject to tolerances), 
            //                      and an optimal solution is available.
            //  INFEASIBLE	    3	Model was proven to be infeasible.
            //  INF_OR_UNBD	    4	Model was proven to be either infeasible or unbounded.
            //  UNBOUNDED 	    5	Model was proven to be unbounded. Important note: an unbounded status indicates the presence of an unbounded ray that allows the objective to improve without limit. It says nothing about whether the model has a feasible solution. If you require information on feasibility, you should set the objective to zero and reoptimize.
            //  CUTOFF	        6	Optimal objective for model was proven to be worse than the value specified in the Cutoff parameter. No solution information is available.
            //  ITERATION_LIMIT	7	Optimization terminated because the total number of simplex iterations performed exceeded the value specified in the IterationLimit parameter, or because the total number of barrier iterations exceeded the value specified in the BarIterLimit parameter.
            //  NODE_LIMIT	    8	Optimization terminated because the total number of branch-and-cut nodes explored exceeded the value specified in the NodeLimit parameter.
            //  TIME_LIMIT	    9	Optimization terminated because the time expended exceeded the value specified in the TimeLimit parameter.
            //  SOLUTION_LIMIT	10	Optimization terminated because the number of solutions found reached the value specified in the SolutionLimit parameter.
            //  INTERRUPTED	    11	Optimization was terminated by the user.
            //  NUMERIC	        12	Optimization was terminated due to unrecoverable numerical difficulties.
            //  SUBOPTIMAL	    13	Unable to satisfy optimality tolerances; a sub-optimal solution is available.
            #endregion
            int gStatus = Model.Get(GRB.IntAttr.Status);
            int jdStatus = 0;
            if ((gStatus == 2) || (gStatus == 13) || (gStatus == 9)) // optimal or suboptimal or interrupted or timeLimit
            {
                jdStatus = 1;
            }
            pars.Set(JD.IntParam.RESULT_STATUS, jdStatus);
            pars.Set(JD.StringParam.STATUS, _intToGRB_Status(gStatus));
            pars.Set(JD.DoubleParam.SOLVER_TIME, Model.Get(GRB.DoubleAttr.Runtime));
            if (pars.Get<int>(JD.IntParam.RESULT_STATUS) > 0)
            {
                pars.Set(JD.DoubleParam.MIP_GAP_REACHED, _calcReachedGap());
                pars.Set(JD.DoubleParam.MIP_GAP_ABS_REACHED, _calcReachedAbsGap());
                pars.Set(JD.DoubleParam.OBJ_VALUE, Model.Get(GRB.DoubleAttr.ObjVal));
            }
        }

        /// <summary>
        /// Calculate reached Gap value
        /// </summary>
        /// <returns>Reached Gap value</returns>
        private double _calcReachedGap()
        {
            if (_nBinVars != 0)
            {
                double LB = Model.Get(GRB.DoubleAttr.ObjBound);
                double UB = Model.Get(GRB.DoubleAttr.ObjVal);
                if (Model.Get(GRB.IntAttr.ModelSense) == GRB.MINIMIZE)
                {
                    double temp = LB;
                    LB = UB;
                    UB = temp;
                }
                return (LB - UB) / UB;
            }
            else
            { return 0; }
        }

        /// <summary>
        /// Calculate reached absolute Gap value
        /// </summary>
        /// <returns>Reached absolute Gap value</returns>
        private double _calcReachedAbsGap()
        {
            if (_nBinVars != 0)
            {
                double LB = Model.Get(GRB.DoubleAttr.ObjBound);
                double UB = Model.Get(GRB.DoubleAttr.ObjVal);
                return Math.Abs(LB - UB);
            }
            else
            { return 0; }
        }

        /// <summary>
        /// Converts integer index (value of GRB.IntAttr.Status) to GRB.Status constant name.
        /// </summary>
        /// <param name="intStatus">GRB.Status value</param>
        /// <returns>Status string</returns>
        private string _intToGRB_Status(int intStatus)
        {
            switch (intStatus)
            {
                case 1:
                    return "LOADED";
                case 2:
                    return "OPTIMAL";
                case 3:
                    return "INFEASIBLE";
                case 4:
                    return "INF_OR_UNBD";
                case 5:
                    return "UNBOUNDED";
                case 6:
                    return "CUTOFF";
                case 7:
                    return "ITERATION_LIMIT";
                case 8:
                    return "NODE_LIMIT";
                case 9:
                    return "TIME_LIMIT";
                case 10:
                    return "SOLUTION_LIMIT";
                case 11:
                    return "INTERRUPTED";
                case 12:
                    return "NUMERIC";
                case 13:
                    return "SUBOPTIMAL";
                default:
                    throw new JDException("Unknown 'GRB.Status' int index {0}! Only 1 - 13 are allowed!", intStatus);
            }
        }

        /// <summary>
        /// Get double value of a variable with specific id
        /// </summary>
        /// <param name="id">Variable id</param>
        /// <returns>Variable value</returns>
        double? IJDSolver.GetVarValue(int id)
        {
            return GVars2[ScVarId_GVarIdx[id]].Get(GRB.DoubleAttr.X);
        }

        /// <summary>
        /// Configure Gurobi solver with JDparameters
        /// </summary>
        /// <param name="pars">JDparameters</param>
        public void ConfigureGurobi(JDParams pars)
        {
            Model.Update();
            Model.Write("model.lp");

            if (pars.IsSet(JD.DoubleParam.TIME_LIMIT))
            {
                Model.GetEnv().Set(GRB.DoubleParam.TimeLimit, (double)pars.Get(JD.DoubleParam.TIME_LIMIT)); // to enable put timelimit also as int.
                _jdGRBCallback.timeLimit = pars.Get<double>(JD.DoubleParam.TIME_LIMIT);
            }
            if (pars.IsSet(JD.DoubleParam.MIP_GAP)) Model.GetEnv().Set(GRB.DoubleParam.MIPGap, pars.Get<double>(JD.DoubleParam.MIP_GAP));
            if (pars.IsSet(JD.DoubleParam.HEURISTICS)) Model.GetEnv().Set(GRB.DoubleParam.Heuristics, pars.Get<double>(JD.DoubleParam.HEURISTICS));
            if (pars.IsSet(JD.IntParam.OUT_FLAG)) Model.GetEnv().Set(GRB.IntParam.OutputFlag, pars.Get<int>(JD.IntParam.OUT_FLAG));
            if (pars.IsSet(JD.StringParam.LOG_FILE)) Model.GetEnv().Set(GRB.StringParam.LogFile, pars.Get<string>(JD.StringParam.LOG_FILE));
            if (pars.IsSet(JD.DoubleParam.MIP_GAP_ABS)) Model.GetEnv().Set(GRB.DoubleParam.MIPGapAbs, pars.Get<double>(JD.DoubleParam.MIP_GAP_ABS));
            if (pars.IsSet(JD.IntParam.THREADS)) Model.GetEnv().Set(GRB.IntParam.Threads, pars.Get<int>(JD.IntParam.THREADS));
            if (pars.IsSet(JD.IntParam.NORM_ADJUST)) Model.GetEnv().Set(GRB.IntParam.NormAdjust, pars.Get<int>(JD.IntParam.NORM_ADJUST));
            if (pars.IsSet(JD.StringParam.WRITE_TO_FILE)) Model.GetEnv().Set(GRB.StringParam.ResultFile, pars.Get<string>(JD.StringParam.WRITE_TO_FILE));
            if (pars.IsSet(JD.DoubleParam.FOCUS_TO_BOUND_TIME)) _jdGRBCallback.focusTimeLimit = pars.Get<double>(JD.DoubleParam.FOCUS_TO_BOUND_TIME);
            _jdGRBCallback.focusChanged = false;
            _jdGRBCallback.lazyConAdded = false;
            if (pars.IsSet(JD.IntParam.PRESPARSIFY)) Model.GetEnv().Set(GRB.IntParam.PreSparsify, pars.Get<int>(JD.IntParam.PRESPARSIFY));
            //if (pars.IsSet(JD.IntParam.LAZY_CONSTRAINTS)) Model.GetEnv().Set(GRB.IntParam.LazyConstraints, pars.Get<int>(JD.IntParam.LAZY_CONSTRAINTS));
            if (pars.IsSet(JD.IntParam.LP_METHOD)) Model.GetEnv().Set(GRB.IntParam.Method, pars.Get<int>(JD.IntParam.LP_METHOD));
        }
    }

    internal class JDGRBCallback : GRBCallback
    {
        private int lastmsg; // callback field
        internal Logger _logger = null;
        private LogFlags _logFlag = LogFlags.OPTIMIZER;
        private GurobiJDSolver _solver;

        internal double timeLimit = -1;
        internal double focusTimeLimit = -1;
        internal bool focusChanged = false;
        internal bool lazyConAdded = false;

        /// <summary>
        /// Create gurobi callback
        /// </summary>
        /// <param name="solver">GurobiJDSolver</param>
        public JDGRBCallback(GurobiJDSolver solver)
        {
            lastmsg = -100;
            _solver = solver;
        }

        /// <summary>
        /// Set logger to log mesasges
        /// </summary>
        /// <param name="logger">Logger</param>
        public void SetLogger(Logger logger)
        {
            _logger = logger;
        }

        private void _log(string msg)
        {
            _logger.Log(_logFlag, msg);
        }

        // callback implementation
        protected override void Callback()
        {
            if (_logger != null)
            {
                try
                {
                    if (where == GRB.Callback.MESSAGE)
                    {
                        string st = GetStringInfo(GRB.Callback.MSG_STRING);
                        if (st != null) _log(st.Replace("\n", ""));
                    }
                    else if (where == GRB.Callback.PRESOLVE)
                    {
                        return;
                        //int cdels = GetIntInfo(GRB.Callback.PRE_COLDEL);
                        //int rdels = GetIntInfo(GRB.Callback.PRE_ROWDEL);
                        //_log("PRESOLVE: " + cdels + " columns and " + rdels + " rows are removed");
                    }
                    else if (where == GRB.Callback.SIMPLEX)
                    {
                        int itcnt = (int)GetDoubleInfo(GRB.Callback.SPX_ITRCNT);
                        return;
                        //if (itcnt % 100 == 0)
                        //{
                        //    double obj = GetDoubleInfo(GRB.Callback.SPX_OBJVAL);
                        //    double pinf = GetDoubleInfo(GRB.Callback.SPX_PRIMINF);
                        //    double dinf = GetDoubleInfo(GRB.Callback.SPX_DUALINF);
                        //    int ispert = GetIntInfo(GRB.Callback.SPX_ISPERT);
                        //    char ch;
                        //    if (ispert == 0) ch = ' ';
                        //    else if (ispert == 1) ch = 'S';
                        //    else ch = 'P';
                        //    _log("SIMPLEX: " + itcnt + "  " + obj + ch + "  " + pinf + "  " + dinf);
                        //}
                    }
                    else if (where == GRB.Callback.MIP)
                    {
                        if (focusTimeLimit > 0 && timeLimit > 0 && !focusChanged && GetDoubleInfo(GRB.Callback.RUNTIME) >= focusTimeLimit)
                        {
                            _solver.Model.GetEnv().Set(GRB.IntParam.MIPFocus, 3);
                            _log(" Solving focus changed to Bound tightening");
                            focusChanged = true;
                        }
                        //int nodecnt = (int)GetDoubleInfo(GRB.Callback.MIP_NODCNT);
                        //if (nodecnt - lastmsg >= 100)
                        //{
                        //    lastmsg = nodecnt;
                        //    double objbst = GetDoubleInfo(GRB.Callback.MIP_OBJBST);
                        //    double objbnd = GetDoubleInfo(GRB.Callback.MIP_OBJBND);
                        //    if (Math.Abs(objbst - objbnd) < 0.1 * (1.0 + Math.Abs(objbst)))
                        //    {
                        //        //Abort();
                        //        //_log("GRB.Callback.MIP - Aborted: (|objbst - objnd| < 0.1*(1 + |objbst|))");
                        //        _log("(|objbst - objnd| < 0.1*(1 + |objbst|))");
                        //    }
                        //    int actnodes = (int)GetDoubleInfo(GRB.Callback.MIP_NODLFT);
                        //    int itcnt = (int)GetDoubleInfo(GRB.Callback.MIP_ITRCNT);
                        //    int solcnt = GetIntInfo(GRB.Callback.MIP_SOLCNT);
                        //    int cutcnt = GetIntInfo(GRB.Callback.MIP_CUTCNT);

                        //    _log("MIP: " + nodecnt + " " + actnodes + " " + itcnt + " "
                        //      + objbst + " " + objbnd + " " + solcnt + " " + cutcnt);
                        //}
                    }
                    else if (where == GRB.Callback.MIPSOL)
                    {
                        double MIPSOL_OBJBST;
                        double MIPSOL_SOLCNT;
                        try
                        {
                            MIPSOL_OBJBST = GetDoubleInfo(GRB.Callback.MIPSOL_OBJBST);
                            MIPSOL_SOLCNT = GetIntInfo(GRB.Callback.MIPSOL_SOLCNT);
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                        //if (lazyConAdded == false && MIPSOL_OBJBST > 0 && _solver.LazyConstrs != null && _solver.LazyConstrs.Count > 0)
                        //{
                        //_log(" Adding lazy constraints.");
                        //foreach (ScConstr con in _solver.LazyConstrs)
                        //{
                        //    char grbSense;
                        //    switch (con.Sense)
                        //    {
                        //        case JD.GREATER_EQUAL:
                        //            grbSense = GRB.GREATER_EQUAL;
                        //            break;
                        //        case JD.LESS_EQUAL:
                        //            grbSense = GRB.LESS_EQUAL;
                        //            break;
                        //        default:
                        //            grbSense = GRB.EQUAL;
                        //            break;
                        //    }
                        //    this.AddLazy(_solver.scLinExpr2GRBLinExpr2(con.Lhs), grbSense, 0);
                        //}
                        //}

                        double obj = GetDoubleInfo(GRB.Callback.MIPSOL_OBJ);
                        int nodecnt = (int)GetDoubleInfo(GRB.Callback.MIPSOL_NODCNT);
                        //double[] x = GetSolution(vars);
                        //Console.WriteLine("**** New solution at node " + nodecnt + ", obj "
                        //                   + obj + ", x[0] = " + x[0] + "****");
                    }
                }
                catch (GRBException e)
                {
                    _log("Error code: " + e.ErrorCode + ". " + e.Message);
                    _log(e.StackTrace);
                }
            }
        }
    }
}
