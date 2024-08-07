using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JDSpace;
using BCL;
using Optimizer;
using System.Diagnostics;

namespace XpressJD
{
    public class XpressJDSolver : IJDSolver
    {
        public XPRBprob XPRBModel { get; private set; }
        public XPRSprob XPRSProb { get; private set; }
        public Dictionary<int, XPRBvar> XVars { get; private set; }

        public XpressJDSolver()
        {
            XPRB.init();
            XPRBModel = new XPRBprob("XpressModel");
            XPRSProb = XPRBModel.getXPRSprob();
            XVars = new Dictionary<int, XPRBvar>();

            //connect optimizer output with standard console output
            XPRSProb.MessageCallbacks += new MessageCallback(this.OptimizerMsg);
        }

        void IJDSolver.AddScVar(ScVar scVar)
        {
            int xType;
            double xLb;
            double xUb;
            switch (scVar.Type)
            {
                case JD.BINARY:
                    xType = BCLconstant.XPRB_BV;
                    break;
                case JD.INTEGER:
                    xType = BCLconstant.XPRB_UI;
                    break;
                default:
                    xType = BCLconstant.XPRB_PL;
                    break;
            }
            if (scVar.Lb == JD.INFINITY)
            {
                xLb = BCLconstant.XPRB_INFINITY;
            }
            else
            {
                xLb = scVar.Lb;
            }
            if (scVar.Ub == -JD.INFINITY)
            {
                xUb = -BCLconstant.XPRB_INFINITY;
            }
            else
            {
                xUb = scVar.Ub;
            }
            XPRBvar xVar = XPRBModel.newVar("XpressVar" + scVar.Id, xType, xLb, xUb);
            XVars.Add(scVar.Id, xVar);
        }

        void IJDSolver.Update()
        {
            // nothing to do.
        }

        void IJDSolver.Reset()
        {
            XPRBModel.reset();
            XVars.Clear();
        }

        void IJDSolver.AddConstr(ScConstr constr)
        {
            XPRBexpr xLhs = ScLinExpr2XPRBexpr(constr.Lhs);

            switch (constr.Sense)
            {
                case JD.GREATER_EQUAL:
                    XPRBModel.newCtr(constr.Name, xLhs >= constr.Rhs);
                    break;
                case JD.LESS_EQUAL:
                    XPRBModel.newCtr(constr.Name, xLhs <= constr.Rhs);
                    break;
                default:
                    XPRBModel.newCtr(constr.Name, xLhs == constr.Rhs);
                    break;
            }
        }

        //TODO need to be checked
        void IJDSolver.AddSOSConstr(SOSConstr sosCon)
        {
            XPRBvar[] xVarArr = new XPRBvar[sosCon.Vars.Count];
            XPRBexpr xLinExpr = new XPRBexpr();

            for(int i = 0; i < xVarArr.Length; i++)
            {
                xVarArr[i] = XVars[sosCon.Vars[i].Id];
                xLinExpr.addTerm(sosCon.Weights[i], xVarArr[i]);
            }
            if (sosCon.Type == 1)
                XPRBModel.newSos(BCLconstant.XPRB_S1, xLinExpr);
            if (sosCon.Type == 2)
                XPRBModel.newSos(BCLconstant.XPRB_S2, xLinExpr);
        }

        public XPRBexpr ScLinExpr2XPRBexpr(ScLinExpr scLinExpr)
        {
            XPRBexpr xLinExpr = new XPRBexpr(scLinExpr.Constant);
            foreach(ScTerm term in scLinExpr.Terms.Values)
            {
                xLinExpr.addTerm(term.Coeff, XVars[term.Var.Id]);
            }
            return xLinExpr;
        }

        void IJDSolver.SetObjective(ScLinExpr scObj, int sense)
        {
            XPRBexpr xLinExpr = ScLinExpr2XPRBexpr(scObj);
            XPRBModel.setObj(xLinExpr);
            if(sense == JD.MINIMIZE)
            {
                XPRBModel.setSense(BCLconstant.XPRB_MINIM);
            }
            else
            {
                XPRBModel.setSense(BCLconstant.XPRB_MAXIM);
            }
        }

        double? IJDSolver.Optimize(Dictionary<string, Param> pars)
        {   
            try
            {
                ConfigureXpress(pars);
                Stopwatch sw = new Stopwatch();
                sw.Start();
                XPRBModel.mipOptimize();
                sw.Stop();
                int status = (int)XPRSProb.MIPStatus;
                pars.SetParam("SolverName", "Gurobi");
                pars.SetParam("SolverStatus", status);
                pars.SetParam("Optimal", (status == (int)MIPStatus.Optimal));
                pars.SetParam("SolutionTime", sw.Elapsed.TotalSeconds);
                pars.SetParam("SolverTime", sw.Elapsed.TotalSeconds);
                pars.SetParam("Objective", XPRBModel.getObjVal());
                return XPRBModel.getObjVal();
            }
            catch (XPRSException e)
            {
                Console.WriteLine("XPRS exception: " + e.Message + ". Helplink: " + e.HelpLink);
                return null;
            }
        }

        double? IJDSolver.GetVarValue(int id)
        {
            try
            {
                return XVars[id].getSol();
            }
            catch (Exception)
            {
                Console.WriteLine("No result value for variable {0}.", id);
                return null;
            }
        }

        public void ConfigureXpress(Dictionary<string, Param> pars)
        {
            int tOutFlag = 1;
            if (pars.ContainsKey("OutFlag"))
            {
                tOutFlag = (int)pars["OutFlag"].Value;
            }
            try
            {
                double timeLimit = (double)pars["TimeLimit"].Value;
                XPRSProb.SetIntControl((int)XPRScontrol.MaxTime, (int)timeLimit);
            }
            catch { 
               if(tOutFlag > 0) Console.WriteLine("Time limit default"); 
            }
            try
            {
                double mipGap = (double)pars["MIPGap"].Value;
                XPRSProb.SetDblControl((int)XPRScontrol.MIPRelStop, mipGap);        
            }
            catch { if (tOutFlag > 0) Console.WriteLine("MIP gap default"); }
            try
            {
                double heur = (double)pars["Heuristics"].Value;
                XPRSProb.SetDblControl((int)XPRScontrol.HeurStrategy, heur);
            }
            catch { if (tOutFlag > 0) Console.WriteLine("Heuristics: default"); }
            try
            {
                int outFlag = (int)pars["OutFlag"].Value;
                XPRSProb.SetIntControl((int)XPRScontrol.OutputLog, outFlag); //TODO check if it works correctly
            }
            catch { if (tOutFlag > 0) Console.WriteLine("Output flag : default"); }
        }

        private void OptimizerMsg(XPRSprob prob, object data, string message, int len, int msglvl)
        {
            Console.WriteLine("{0}" + message, data);
        }
    }
}
