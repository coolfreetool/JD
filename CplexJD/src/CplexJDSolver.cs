using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ILOG.CPLEX;
using ILOG.Concert;
using System.Diagnostics;
using JDUtils;
using JDSpace;

namespace CplexJD
{
    public class CplexJDSolver : IJDSolver
    {
        public Cplex Cplx { get; private set; }
        public Dictionary<int, INumVar> CPVars;

        public CplexJDSolver()
        {
            Cplx = new Cplex();
            CPVars = new Dictionary<int, INumVar>();
        }

        void AddScVar(ScVar scVar)
        {
            NumVarType numVarType;
            switch (scVar.Type)
            {
                case JD.BINARY:
                    numVarType = NumVarType.Bool;
                    break;
                case JD.INTEGER:
                    numVarType = NumVarType.Int;
                    break;
                default:
                    numVarType = NumVarType.Float;
                    break;
            }
            INumVar iNumVar = Cplx.NumVar(scVar.Lb, scVar.Ub, numVarType, scVar.Name);

            CPVars.Add(scVar.Id, iNumVar);
        }

        void IJDSolver.Update()
        {
            // nothing to do.
        }

        void IJDSolver.Reset()
        {
            Cplx.ClearModel();
            CPVars.Clear();
        }

        void AddConstr(ScConstr con)
        {
            switch (con.Sense)
            {
                case JD.GREATER_EQUAL:
                    Cplx.AddLe(0, ScLinExpr2CPLinExpr(con.Lhs), con.Name);
                    break;
                case JD.LESS_EQUAL:
                    Cplx.AddGe(0, ScLinExpr2CPLinExpr(con.Lhs), con.Name);
                    break;
                default:
                    Cplx.AddEq(0, ScLinExpr2CPLinExpr(con.Lhs), con.Name);
                    break;
            }
        }

        public ILinearNumExpr ScLinExpr2CPLinExpr(ScLinExpr linExpr)
        {
            ILinearNumExpr cpLinExpr = Cplx.LinearNumExpr(linExpr.Constant);
            foreach (ScTerm term in linExpr.Terms)
            {
                cpLinExpr.AddTerm(term.Coeff, CPVars[term.Var.Id]);
            }
            return cpLinExpr;
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
            INumVar[] cpVarArr = new INumVar[sosCon.Vars.Count];
            for (int i = 0; i < cpVarArr.Length; i++)
            {
                cpVarArr[i] = CPVars[sosCon.Vars[i].Id];
            }
            if (sosCon.Type == 1)
            {
                Cplx.AddSOS1(cpVarArr, sosCon.Weights);
            }
            else if (sosCon.Type == 2)
            {
                Cplx.AddSOS2(cpVarArr, sosCon.Weights);
            }
            else
            {
                throw new JDException("Unknown SOS type: {0}!", sosCon.Type);
            }
        }

        void IJDSolver.SetObjective(ScLinExpr obj, int sense)
        {
            if (sense == JD.MINIMIZE)
            {
                Cplx.AddMinimize(ScLinExpr2CPLinExpr(obj));
            }
            else if (sense == JD.MAXIMIZE)
            {
                Cplx.AddMaximize(ScLinExpr2CPLinExpr(obj));
            }
            else
            {
                throw new JDException("Unknown objective sense type: {0}!", sense);
            }
        }

        void IJDSolver.Optimize(JDParams pars)
        {
            ConfigureCplex(pars);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            bool solved = Cplx.Solve();
            sw.Stop();
            pars.Set(JD.StringParam.SOLVER_NAME, "CPLEX");
            pars.Set(JD.DoubleParam.SOLVER_TIME, sw.Elapsed.TotalSeconds);
            Cplex.Status cpStatus = Cplx.GetStatus();
            int jdResult = 0;
            if ((cpStatus == Cplex.Status.Optimal) ||
                (cpStatus == Cplex.Status.Bounded))
            {
                jdResult = 1;
            }
            pars.Set(JD.IntParam.RESULT_STATUS, jdResult);
            pars.Set(JD.StringParam.STATUS, cpStatus.ToString());
            pars.Set(JD.DoubleParam.OBJ_VALUE, Cplx.ObjValue);
        }

        /// <summary>
        /// Configure Cplex solver.
        /// Supported parameters: JD.DoubleParam.TIME_LIMIT, JD.DoubleParam.MIP_GAP.
        /// </summary>
        /// <param name="pars"></param>
        public void ConfigureCplex(JDParams pars)
        {
            if (pars.IsSet(JD.DoubleParam.TIME_LIMIT)) Cplx.SetParam(Cplex.DoubleParam.TiLim, pars.Get<double>(JD.DoubleParam.TIME_LIMIT));
            if (pars.IsSet(JD.DoubleParam.MIP_GAP)) Cplx.SetParam(Cplex.DoubleParam.EpGap, pars.Get<double>(JD.DoubleParam.MIP_GAP));
        }

        double? IJDSolver.GetVarValue(int id)
        {
            //pokud je vytvořena proměnná, která není v žádné omezující podmínce, nedostane se do modelu a při získávání hodnoty vyskočí výjimka.
            try
            {
                return Cplx.GetValue(CPVars[id]);
            }
            catch (CpxException ex)
            {
                if (!ex.Message.Equals("CPLEX Error: object is unknown to IloCplex"))
                    throw ex;
                return null;
            }
            
        }

        Logger IJDSolver.GetLogger()
        {
            return null;
        }

        void IJDSolver.SetLogger(Logger logger)
        {
            throw new NotImplementedException();
        }
    }
}
