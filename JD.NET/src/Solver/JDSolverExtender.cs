using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using JDUtils;

namespace JDSpace
{
    /// <summary>
    /// Common IJDSolver methods extender.
    /// </summary>
    public static class JDSolverExtender
    {
        private static LogFlags _logFlag = LogFlags.JD;

        /// <summary>
        /// Log string message with parameters and specific flag
        /// </summary>
        /// <param name="t">JD solver</param>
        /// <param name="logFlags">Log flag</param>
        /// <param name="msg">Log message</param>
        /// <param name="pars">Log parameters</param>
        public static void Log(this IJDSolver t, LogFlags logFlags, string msg, params Param[] pars)
        {
            Logger logger = t.GetLogger();
            if (logger != null)
            {
                logger.Log(logFlags, msg, pars);
            }
        }

        /// <summary>
        /// Log string message with specific flag
        /// </summary>
        /// <param name="t">JD solver</param>
        /// <param name="logFlags">Log flag</param>
        /// <param name="msg">Log message</param>
        public static void Log(this IJDSolver t, LogFlags logFlags, string msg)
        {
            Logger logger = t.GetLogger();
            if (logger != null)
            {
                logger.Log(logFlags, msg);
            }
        }

        /// <summary>
        /// Add SOS constraints to the solver
        /// </summary>
        /// <param name="t">JD solver</param>
        /// <param name="sosCons">List of SOS constraints</param>
        public static void AddSOSConstrs(this IJDSolver t, List<SOSConstr> sosCons)
        {
            foreach (SOSConstr sosCon in sosCons)
            {
                t.AddSOSConstr(sosCon);
            }
        }

        /// <summary>
        /// Test whether is possible to get results from solver
        /// </summary>
        /// <param name="t">Solver that have solved model</param>
        /// <param name="jdMdl">Solved model</param>
        /// <returns>Test result</returns>
        private static bool _tryGetResult(this IJDSolver t, JDModel jdMdl)
        {
            if (jdMdl.ConVars.Count > 0)
            {
                try
                {
                    t.GetVarValue(jdMdl.ConVars[0].Id);
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            if (jdMdl.BinVars.Count > 0)
            {
                try
                {
                    t.GetVarValue(jdMdl.BinVars[0].Id);
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            if (jdMdl.IntVars.Count > 0)
            {
                try
                {
                    t.GetVarValue(jdMdl.IntVars[0].Id);
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Base IJDSolver usage method. Solve inserted JDModel.
        /// Send JDModel variables, constraints and objective function into solver,
        /// optimize problem and put variables results.
        /// </summary>
        /// <param name="t">JDSolver</param>
        /// <param name="jdMdl">JDModel </param>
        public static void Solve(this IJDSolver t, JDModel jdMdl)
        {
            Stopwatch sw = new Stopwatch();
            t.Log(_logFlag, JD.MSG_MODEL_SOLVING, jdMdl.ToParams().ToArray());
            DateTime tModelSolving1 = DateTime.Now;
            t.Log(_logFlag, JD.MSG_PUTTING_MODEL);
            t.Log(_logFlag, JD.MSG_PUTTING_VARS);
            sw.Start();
            if (jdMdl.Params.IsSet(JD.IntParam.RELAX_BIN_VARIABLES))
                if (jdMdl.Params.intParams[JD.IntParam.RELAX_BIN_VARIABLES] == 1)
                {
                    foreach (ScVar scvar in jdMdl.BinVars)
                    {
                        scvar.Type = JD.CONTINUOUS;
                        scvar.Ub = 1;
                        scvar.Lb = 0;
                    }
                    jdMdl.ConVars.AddRange(jdMdl.BinVars);
                    jdMdl.BinVars.Clear();
                }
            t.AddScVars(jdMdl.Vars);
            sw.Stop();
            t.Log(_logFlag, JD.MSG_PUTTING_VARS,
                new Param(JD.PARAM_TIME, sw.Elapsed.TotalSeconds), new Param(JD.PARAM_MODEL, jdMdl.Name));
            t.Log(_logFlag, JD.MSG_MODEL_UPDATING);
            sw.Reset();
            t.Update();
            sw.Stop();
            t.Log(_logFlag, JD.MSG_MODEL_UPDATING,
                new Param(JD.PARAM_TIME, sw.Elapsed.TotalSeconds), new Param(JD.PARAM_MODEL, jdMdl.Name));
            t.Log(_logFlag, JD.MSG_PUTTING_CONSTRS);
            sw.Restart();
            t.AddConstrs(jdMdl.Constrs);
            t.AddSOSConstrs(jdMdl.SOSConstraints);
            sw.Stop();
            t.Log(_logFlag, JD.MSG_PUTTING_CONSTRS,
                new Param(JD.PARAM_TIME, sw.Elapsed.TotalSeconds), new Param(JD.PARAM_MODEL, jdMdl.Name));
            t.Log(_logFlag, JD.MSG_PUTTING_OBJ_FUN);
            sw.Restart();
            t.SetObjective(jdMdl.Obj, jdMdl.ObjSense);
            sw.Stop();
            DateTime tPuttingModelFinsihed = DateTime.Now;
            t.Log(_logFlag, JD.MSG_PUTTING_OBJ_FUN,
                new Param(JD.PARAM_TIME, sw.Elapsed.TotalSeconds), new Param(JD.PARAM_MODEL, jdMdl.Name));
            t.Log(_logFlag, JD.MSG_PUTTING_MODEL,
                new Param(JD.PARAM_TIME, (tPuttingModelFinsihed - tModelSolving1).TotalSeconds), new Param(JD.PARAM_MODEL, jdMdl.Name));
            t.Log(_logFlag, JD.MSG_OPTIMIZING);
            sw.Restart();
            t.Optimize(jdMdl.Params);
            sw.Stop();

            t.Log(_logFlag, JD.MSG_OPTIMIZING,
                new Param(JD.PARAM_TIME, sw.Elapsed.TotalSeconds), new Param(JD.PARAM_MODEL, jdMdl.Name));
            t.Log(_logFlag, JD.MSG_OPTIMIZING, jdMdl.ToParams().ToArray());
            if (jdMdl.Params.Get<int>(JD.IntParam.RESULT_STATUS) > 0)
            {
                if (t._tryGetResult(jdMdl))
                {
                    t.Log(_logFlag, JD.MSG_RESULTS_PARSING);
                    sw.Restart();
                    Parallel.ForEach(jdMdl.ConVars, var => // vyuziti multithreadingu pri nacitani vysledku
                    {
                        var.Value = t.GetVarValue(var.Id);
                    });
                    Parallel.ForEach(jdMdl.BinVars, var => // vyuziti multithreadingu pri nacitani vysledku
                    {
                        var.Value = t.GetVarValue(var.Id);
                    });
                    Parallel.ForEach(jdMdl.IntVars, var => // vyuziti multithreadingu pri nacitani vysledku
                    {
                        var.Value = t.GetVarValue(var.Id);
                    });
                    sw.Stop();
                    t.Log(_logFlag, JD.MSG_RESULTS_PARSING,
                        new Param(JD.PARAM_TIME, sw.Elapsed.TotalSeconds), new Param(JD.PARAM_MODEL, jdMdl.Name));
                }
                else
                {
                    t.Log(_logFlag, "Can not get results from solver, probably no results are available.");
                }
            }
            DateTime tModelSolving2 = DateTime.Now;
            t.Log(_logFlag, JD.MSG_MODEL_SOLVING,
                new Param(JD.PARAM_TIME,
                    (tModelSolving2 - tModelSolving1).TotalSeconds));
        }
        
        /// <summary>
        /// Export inserted JDModel as *.lp or *.mps file.
        /// </summary>
        /// <param name="solver">Solver used for export</param>
        /// <param name="filenameWithoutExtension">File name without extension</param>
        /// <param name="fileType">File type (mps, lp)</param>  
        public static bool Export(this IJDSolver solver, JDModel model, string filenameWithoutExtension, string fileType)
        {
            Stopwatch stopwatch = new Stopwatch();
            DateTime now = DateTime.Now;
            solver.Log(_logFlag, "Model exporting", model.ToParams().ToArray());
            AddModelToSolver(solver, model);
            bool result = WriteToFile(solver, model, filenameWithoutExtension, fileType);
            solver.Reset();
            DateTime now2 = DateTime.Now;
            solver.Log(_logFlag, "Model exporting", new Param("time[s]", (now2 - now).TotalSeconds));
            return result;
        }

        private static void AddModelToSolver(IJDSolver solver, JDModel model)
        {
            Stopwatch stopwatch = new Stopwatch();
            DateTime now = DateTime.Now;
            solver.Log(_logFlag, "Putting model to solver");
            AddVariablesToSolver(solver, model);
            UpdateSolver(solver, model);
            AddConstraintsToSolver(solver, model);
            AddObjectiveToSolver(solver, model);
            DateTime now2 = DateTime.Now;
            solver.Log(_logFlag, "Putting model to solver", new Param("time[s]", (now2 - now).TotalSeconds), new Param("model", model.Name));
        }

        private static void AddVariablesToSolver(IJDSolver solver, JDModel model)
        {
            Stopwatch stopwatch = new Stopwatch();
            solver.Log(_logFlag, "Putting variables to solver");
            stopwatch.Start();
            solver.AddScVars(model.Vars);
            stopwatch.Stop();
            solver.Log(_logFlag, "Putting variables to solver", new Param("time[s]", stopwatch.Elapsed.TotalSeconds), new Param("model", model.Name));
        }

        private static void UpdateSolver(IJDSolver solver, JDModel model)
        {
            Stopwatch stopwatch = new Stopwatch();
            solver.Log(_logFlag, "Model updating");
            stopwatch.Reset();
            solver.Update();
            stopwatch.Stop();
            solver.Log(_logFlag, "Model updating", new Param("time[s]", stopwatch.Elapsed.TotalSeconds), new Param("model", model.Name));
        }

        private static void AddConstraintsToSolver(IJDSolver solver, JDModel model)
        {
            Stopwatch stopwatch = new Stopwatch();
            solver.Log(_logFlag, "Putting constraints to solver");
            stopwatch.Restart();
            solver.AddConstrs(model.Constrs);
            solver.AddSOSConstrs(model.SOSConstraints);
            stopwatch.Stop();
            solver.Log(_logFlag, "Putting constraints to solver", new Param("time[s]", stopwatch.Elapsed.TotalSeconds), new Param("model", model.Name));
        }

        private static void AddObjectiveToSolver(IJDSolver solver, JDModel model)
        {
            Stopwatch stopwatch = new Stopwatch();
            solver.Log(_logFlag, "Putting obj. fun. to solver");
            stopwatch.Restart();
            solver.SetObjective(model.Obj, model.ObjSense);
            stopwatch.Stop();
            solver.Log(_logFlag, "Putting obj. fun. to solver", new Param("time[s]", stopwatch.Elapsed.TotalSeconds), new Param("model", model.Name));
        }

        private static bool WriteToFile(IJDSolver solver, JDModel model, string filename, string fileType)
        {
            Stopwatch stopwatch = new Stopwatch();
            solver.Log(_logFlag, "Writing to file");
            stopwatch.Start();
            bool result = solver.Export(filename, fileType);
            solver.Log(_logFlag, "Writing to file", new Param("time[s]", stopwatch.Elapsed.TotalSeconds), new Param("model", model.Name));
            return result;
        }
    }
}