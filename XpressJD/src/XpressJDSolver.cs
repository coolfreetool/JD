using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BCL;
using JDSpace;
using JDUtils;
using Optimizer;

namespace XpressJD
{

    public class XpressJDSolver : IJDSolver, IDisposable
    {
        private int _nBinVars = 0;

        private bool _infeasible;

        private MessageCallback messageCallback;

        public XPRBprob XprbProb { get; private set; }

        public XPRSprob XprsProb { get; private set; }
        public Dictionary<int, XPRBvar> XVars { get; private set; }

        //
        // Summary:
        //     Returns message about infeasible constraints.
        public Action<string> AddConstrsInfeasibleDelegate { get; set; }

        public bool SupportsSOS1 => true;

        public bool SupportsSOS2 => true;

        public XpressJDSolver()
        {
            init();
        }

        private void init()
        {
            XPRB.init();
            XprbProb = new XPRBprob("XpressModel");
            XprsProb = XprbProb.getXPRSprob();
            XVars = new Dictionary<int, XPRBvar>();
            messageCallback = OptimizerMsg;
            XprsProb.MessageCallbacks += messageCallback;
        }

        private void reset()
        {
            XprbProb.Dispose();
            XVars.Clear();
            XPRB.finish();
            XPRB.free();
            _infeasible = false;
            _nBinVars = 0;
        }

        void IJDSolver.AddScVars(List<ScVar> scVars)
        {
            foreach (ScVar scVar in scVars)
            {
                int type;
                switch (scVar.Type)
                {
                    case 'b':
                        type = 1;
                        _nBinVars++;
                        break;
                    case 'i':
                        type = 2;
                        break;
                    default:
                        type = 0;
                        break;
                }

                double lob = ((scVar.Lb != double.MinValue) ? scVar.Lb : (-1E+20));
                double upb = ((scVar.Ub != double.MaxValue) ? scVar.Ub : 1E+20);
                if (scVar.Use)
                {
                    XPRBvar value = XprbProb.newVar(scVar.Name, type, lob, upb);
                    XVars.Add(scVar.Id, value);
                }
            }
        }

        public void SetLogger(Logger logger)
        {
        }

        public Logger GetLogger()
        {
            return null;
        }

        void IJDSolver.Update()
        {
        }

        void IJDSolver.Reset()
        {
            reset();
            init();
        }

        void IJDSolver.AddConstrs(List<ScConstr> constrs)
        {
            foreach (ScConstr constr in constrs)
            {
                if (constr.Lhs.Terms.Count == 0 && constr.Lhs.Constant == 0.0)
                {
                    continue;
                }

                XPRBexpr xPRBexpr = ScLinExpr2XPRBexpr(constr.Lhs);
                if (object.ReferenceEquals(xPRBexpr, null))
                {
                    string text = null;
                    switch (constr.Sense)
                    {
                        case '>':
                            if (constr.Lhs.Constant < 0.0)
                            {
                                text = "Problem is infeasible due to constraint " + constr.Name + ": 0 < " + constr.Lhs.Constant;
                                _infeasible = true;
                            }

                            break;
                        case '<':
                            if (constr.Lhs.Constant > 0.0)
                            {
                                text = "Problem is infeasible due to constraint " + constr.Name + ": 0 > " + constr.Lhs.Constant;
                                _infeasible = true;
                            }

                            break;
                        case '=':
                            if (constr.Lhs.Constant != 0.0)
                            {
                                text = "Problem is infeasible due to constraint " + constr.Name + ": 0 == " + constr.Lhs.Constant;
                                _infeasible = true;
                            }

                            break;
                    }

                    if (text != null)
                    {
                        Console.WriteLine(text);
                        if (AddConstrsInfeasibleDelegate != null)
                        {
                            AddConstrsInfeasibleDelegate(text);
                        }
                    }

                    break;
                }

                switch (constr.Sense)
                {
                    case '>':
                        if (0.0 - constr.Lhs.Constant > -1E+20)
                        {
                            XprbProb.newCtr(constr.Name, xPRBexpr >= 0.0 - constr.Lhs.Constant);
                        }

                        break;
                    case '<':
                        if (0.0 - constr.Lhs.Constant < 1E+20)
                        {
                            XprbProb.newCtr(constr.Name, xPRBexpr <= 0.0 - constr.Lhs.Constant);
                        }

                        break;
                    case '=':
                        XprbProb.newCtr(constr.Name, xPRBexpr == 0.0 - constr.Lhs.Constant);
                        break;
                }
            }
        }

        void IJDSolver.AddSOSConstr(SOSConstr sosCon)
        {
            if (sosCon.Vars.Count == 0 || sosCon.Vars.TrueForAll((ScVar x) => !x.Use))
            {
                return;
            }

            if (!sosCon.Vars.TrueForAll((ScVar x) => x.Use))
            {
                throw new JDException("The definition of SOS is inconsistent - some of the variables are marked as used in the optimization while some are not.All the variables should be either used or not used.");
            }

            XPRBvar[] array = new XPRBvar[sosCon.Vars.Count];
            XPRBsos xPRBsos;
            if (sosCon.Type == 1)
            {
                xPRBsos = XprbProb.newSos(0);
            }
            else
            {
                if (sosCon.Type != 2)
                {
                    throw new JDException("Invalid type of SOS cotraints {0}", sosCon.Type);
                }

                if (sosCon.Weights.Distinct().Count() != sosCon.Weights.Length)
                {
                    throw new JDException("Coefficients defining the order of elements of special ordered set must all have distrinct values!");
                }

                xPRBsos = XprbProb.newSos(1);
            }

            for (int i = 0; i < array.Length; i++)
            {
                xPRBsos.addElement(XVars[sosCon.Vars[i].Id], sosCon.Weights[i]);
            }

            if (xPRBsos.isValid())
            {
                return;
            }

            xPRBsos.print();
            throw new JDException("Creating of a SOS constraint failed.");
        }

        public XPRBexpr ScLinExpr2XPRBexpr(ScLinExpr scLinExpr)
        {
            bool flag = false;
            XPRBexpr xPRBexpr = new XPRBexpr();
            foreach (ScTerm term in scLinExpr.Terms)
            {
                if (term.Coeff != 0.0)
                {
                    flag = true;
                }

                xPRBexpr.addTerm(term.Coeff, XVars[term.Var.Id]);
            }

            if (!flag)
            {
                return null;
            }

            return xPRBexpr;
        }

        void IJDSolver.SetObjective(ScLinExpr scObj, int sense)
        {
            XPRBexpr xPRBexpr = ScLinExpr2XPRBexpr(scObj);
            if (!object.ReferenceEquals(xPRBexpr, null))
            {
                XprbProb.setObj(xPRBexpr);
            }
            else
            {
                XprbProb.setObj(new XPRBexpr(XprbProb.newVar("objVar", 0, 0.0, 0.0)));
            }

            if (sense == -1)
            {
                XprbProb.setSense(0);
            }
            else
            {
                XprbProb.setSense(1);
            }
        }

        void IJDSolver.Optimize(JDParams pars)
        {
            ConfigureXpress(pars);
            if (pars.IsSet("WRITE_TO_FILE"))
            {
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            if (!_infeasible)
            {
                XprbProb.mipOptimize();
            }

            stopwatch.Stop();
            pars.Set("SOLVER_NAME", "XPRESS");
            MIPStatus mIPStatus = XprsProb.MIPStatus;
            if (_infeasible)
            {
                mIPStatus = MIPStatus.Infeasible;
            }

            int value = 0;
            if (mIPStatus == MIPStatus.Solution || mIPStatus == MIPStatus.Optimal)
            {
                value = 1;
            }

            if (mIPStatus == MIPStatus.Infeasible)
            {
                _infeasible = true;
            }

            pars.Set("RESULT_STATUS", value);
            pars.Set("STATUS", mIPStatus.ToString());
            pars.Set("SOLVER_TIME", stopwatch.Elapsed.TotalSeconds);
            if (pars.Get<int>("RESULT_STATUS") > 0)
            {
                pars.Set("MIP_GAP_REACHED", _calcReachedGap());
                pars.Set("MIP_GAP_ABS_REACHED", _calcReachedAbsGap());
                pars.Set("OBJ_VALUE", XprsProb.MIPObjVal);
            }
        }

        private double _calcReachedGap()
        {
            if (_nBinVars != 0)
            {
                double num = Math.Pow(10.0, -10.0);
                double bestBound = XprsProb.BestBound;
                double mIPObjVal = XprsProb.MIPObjVal;
                return Math.Abs(bestBound - mIPObjVal) / (num + Math.Abs(mIPObjVal));
            }

            return 0.0;
        }

        private double _calcReachedAbsGap()
        {
            if (_nBinVars != 0)
            {
                double bestBound = XprsProb.BestBound;
                double mIPObjVal = XprsProb.MIPObjVal;
                return Math.Abs(bestBound - mIPObjVal);
            }

            return 0.0;
        }

        double? IJDSolver.GetVarValue(int id)
        {
            if (_infeasible)
            {
                return null;
            }

            return XVars[id].getSol();
        }

        public void ConfigureXpress(JDParams pars)
        {
            if (pars.IsSet("TIME_LIMIT"))
            {
                XprsProb.SetIntControl(8020, (int)pars.Get<double>("TIME_LIMIT"));
            }

            if (pars.IsSet("MIP_GAP"))
            {
                XprsProb.SetDblControl(7020, pars.Get<double>("MIP_GAP"));
            }

            if (pars.IsSet("HEURISTICS"))
            {
                XprsProb.SetDblControl(8154, pars.Get<double>("HEURISTICS"));
            }

            if (pars.IsSet("OUT_FLAG"))
            {
                XprsProb.SetIntControl(8035, pars.Get<int>("OUT_FLAG"));
            }

            if (pars.IsSet("LOG_FILE"))
            {
                XprsProb.SetStrControl(8035, pars.Get<string>("LOG_FILE"));
            }

            if (pars.IsSet("MIP_GAP_ABS"))
            {
                XprsProb.SetDblControl(7019, pars.Get<double>("MIP_GAP_ABS"));
            }

            if (pars.IsSet("THREADS"))
            {
                XprsProb.SetIntControl(8079, pars.Get<int>("THREADS"));
                XprsProb.SetIntControl(8274, pars.Get<int>("THREADS"));
            }

            if (pars.IsSet("WRITE_TO_FILE"))
            {
                XprsProb.WriteSol(pars.Get<string>("WRITE_TO_FILE"));
            }
        }

        private void OptimizerMsg(XPRSprob prob, object data, string message, int len, int msglvl)
        {
            Console.WriteLine("{0}" + message, data);
        }

        /// <summary>Export model to file</summary>
        /// <param name="filenameWithoutExtension">File name without extension</param>
        /// <param name="fileType">File type (mps, lp)</param>
        /// <returns>Returns if export was successful</returns>
        public bool Export(string filenameWithoutExtension, string fileType)
        {
            string filename = $"{filenameWithoutExtension}.{fileType}";
            int num;
            if (fileType == "lp")
            {
                num = XprbProb.exportProb(1, filename);
            }
            else
            {
                if (!(fileType == "mps"))
                {
                    throw new JDException("Unknown file type {0}", fileType);
                }

                num = XprbProb.exportProb(2, filename);
            }

            return num == 0;
        }

        public void Dispose()
        {
            reset();
        }
    }
}