using BCL;
using JDSpace;
using JDUtils;
using Optimizer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

#nullable disable
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

    public XpressJDSolver() => this.init();

    private void init()
    {
      XPRB.init();
      this.XprbProb = new XPRBprob("XpressModel");
      this.XprsProb = this.XprbProb.getXPRSprob();
      this.XVars = new Dictionary<int, XPRBvar>();
      this.messageCallback = new MessageCallback(this.OptimizerMsg);
      this.XprsProb.MessageCallbacks += this.messageCallback;
    }

    private void reset()
    {
      this.XprbProb.Dispose();
      this.XVars.Clear();
      XPRB.finish();
      XPRB.free();
      this._infeasible = false;
      this._nBinVars = 0;
    }

    void IJDSolver.AddScVars(List<ScVar> scVars)
    {
      foreach (ScVar scVar in scVars)
      {
        int num1;
        switch (scVar.Type)
        {
          case 'b':
            num1 = 1;
            ++this._nBinVars;
            break;
          case 'i':
            num1 = 2;
            break;
          default:
            num1 = 0;
            break;
        }
        double num2 = scVar.Lb != double.MinValue ? scVar.Lb : -1E+20;
        double num3 = scVar.Ub != double.MaxValue ? scVar.Ub : 1E+20;
        if (scVar.Use)
        {
          XPRBvar xprBvar = this.XprbProb.newVar(scVar.Name, num1, num2, num3);
          this.XVars.Add(scVar.Id, xprBvar);
        }
      }
    }

    public void SetLogger(Logger logger)
    {
    }

    public Logger GetLogger() => (Logger) null;

    void IJDSolver.Update()
    {
    }

    void IJDSolver.Reset()
    {
      this.reset();
      this.init();
    }

    /// <summary>Returns message about infeasible constraints.</summary>
    public Action<string> AddConstrsInfeasibleDelegate { get; set; }

    void IJDSolver.AddConstrs(List<ScConstr> constrs)
    {
      foreach (ScConstr constr in constrs)
      {
        if (constr.Lhs.Terms.Count != 0 || constr.Lhs.Constant != 0.0)
        {
          XPRBexpr objA = this.ScLinExpr2XPRBexpr(constr.Lhs);
          if (object.ReferenceEquals((object) objA, (object) null))
          {
            string str = (string) null;
            switch (constr.Sense)
            {
              case '<':
                if (constr.Lhs.Constant > 0.0)
                {
                  str = "Problem is infeasible due to constraint " + constr.Name + ": 0 > " + (object) constr.Lhs.Constant;
                  this._infeasible = true;
                  break;
                }
                break;
              case '=':
                if (constr.Lhs.Constant != 0.0)
                {
                  str = "Problem is infeasible due to constraint " + constr.Name + ": 0 == " + (object) constr.Lhs.Constant;
                  this._infeasible = true;
                  break;
                }
                break;
              case '>':
                if (constr.Lhs.Constant < 0.0)
                {
                  str = "Problem is infeasible due to constraint " + constr.Name + ": 0 < " + (object) constr.Lhs.Constant;
                  this._infeasible = true;
                  break;
                }
                break;
            }
            if (str == null)
              break;
            Console.WriteLine(str);
            if (this.AddConstrsInfeasibleDelegate != null)
              this.AddConstrsInfeasibleDelegate(str);
            break;
          }
          switch (constr.Sense)
          {
            case '<':
              if (-constr.Lhs.Constant < 1E+20)
              {
                this.XprbProb.newCtr(constr.Name, XPRBexpr.op_LessThanOrEqual(objA, -constr.Lhs.Constant));
                break;
              }
              break;
            case '=':
              this.XprbProb.newCtr(constr.Name, XPRBexpr.op_Equality(objA, -constr.Lhs.Constant));
              break;
            case '>':
              if (-constr.Lhs.Constant > -1E+20)
              {
                this.XprbProb.newCtr(constr.Name, XPRBexpr.op_GreaterThanOrEqual(objA, -constr.Lhs.Constant));
                break;
              }
              break;
          }
        }
      }
    }

    void IJDSolver.AddSOSConstr(SOSConstr sosCon)
    {
      if (sosCon.Vars.Count == 0 || sosCon.Vars.TrueForAll((Predicate<ScVar>) (x => !x.Use)))
        return;
      if (!sosCon.Vars.TrueForAll((Predicate<ScVar>) (x => x.Use)))
        throw new JDException("The definition of SOS is inconsistent - some of the variables are marked as used in the optimization while some are not.All the variables should be either used or not used.", new object[0]);
      XPRBvar[] xprBvarArray = new XPRBvar[sosCon.Vars.Count];
      XPRBsos xprBsos;
      if (sosCon.Type == 1)
        xprBsos = this.XprbProb.newSos(0);
      else if (sosCon.Type == 2)
      {
        if (((IEnumerable<double>) sosCon.Weights).Distinct<double>().Count<double>() != sosCon.Weights.Length)
          throw new JDException("Coefficients defining the order of elements of special ordered set must all have distrinct values!", new object[0]);
        xprBsos = this.XprbProb.newSos(1);
      }
      else
        throw new JDException("Invalid type of SOS cotraints {0}", new object[1]
        {
          (object) sosCon.Type
        });
      for (int index = 0; index < xprBvarArray.Length; ++index)
        xprBsos.addElement(this.XVars[sosCon.Vars[index].Id], sosCon.Weights[index]);
      if (!xprBsos.isValid())
      {
        xprBsos.print();
        throw new JDException("Creating of a SOS constraint failed.", new object[0]);
      }
    }

    public XPRBexpr ScLinExpr2XPRBexpr(ScLinExpr scLinExpr)
    {
      bool flag = false;
      XPRBexpr xprBexpr = new XPRBexpr();
      foreach (ScTerm term in (IEnumerable<ScTerm>) scLinExpr.Terms)
      {
        if (term.Coeff != 0.0)
          flag = true;
        xprBexpr.addTerm(term.Coeff, this.XVars[term.Var.Id]);
      }
      return !flag ? (XPRBexpr) null : xprBexpr;
    }

    void IJDSolver.SetObjective(ScLinExpr scObj, int sense)
    {
      XPRBexpr objA = this.ScLinExpr2XPRBexpr(scObj);
      if (!object.ReferenceEquals((object) objA, (object) null))
        this.XprbProb.setObj(objA);
      else
        this.XprbProb.setObj(new XPRBexpr(this.XprbProb.newVar("objVar", 0, 0.0, 0.0)));
      if (sense == -1)
        this.XprbProb.setSense(0);
      else
        this.XprbProb.setSense(1);
    }

    void IJDSolver.Optimize(JDParams pars)
    {
      this.ConfigureXpress(pars);
      if (!pars.IsSet("WRITE_TO_FILE"))
        ;
      Stopwatch stopwatch = new Stopwatch();
      stopwatch.Start();
      if (!this._infeasible)
        this.XprbProb.mipOptimize();
      stopwatch.Stop();
      pars.Set("SOLVER_NAME", "XPRESS");
      MIPStatus mipStatus = this.XprsProb.MIPStatus;
      if (this._infeasible)
        mipStatus = (MIPStatus) 5;
      int num = 0;
      if (mipStatus == 4 || mipStatus == 6)
        num = 1;
      if (mipStatus == 5)
        this._infeasible = true;
      pars.Set("RESULT_STATUS", num);
      pars.Set("STATUS", mipStatus.ToString());
      pars.Set("SOLVER_TIME", stopwatch.Elapsed.TotalSeconds);
      if (pars.Get<int>("RESULT_STATUS") <= 0)
        return;
      pars.Set("MIP_GAP_REACHED", this._calcReachedGap());
      pars.Set("MIP_GAP_ABS_REACHED", this._calcReachedAbsGap());
      pars.Set("OBJ_VALUE", this.XprsProb.MIPObjVal);
    }

    private double _calcReachedGap()
    {
      if (this._nBinVars == 0)
        return 0.0;
      double num = Math.Pow(10.0, -10.0);
      double bestBound = this.XprsProb.BestBound;
      double mipObjVal = this.XprsProb.MIPObjVal;
      return Math.Abs(bestBound - mipObjVal) / (num + Math.Abs(mipObjVal));
    }

    private double _calcReachedAbsGap()
    {
      return this._nBinVars != 0 ? Math.Abs(this.XprsProb.BestBound - this.XprsProb.MIPObjVal) : 0.0;
    }

    double? IJDSolver.GetVarValue(int id)
    {
      return this._infeasible ? new double?() : new double?(this.XVars[id].getSol());
    }

    public void ConfigureXpress(JDParams pars)
    {
      if (pars.IsSet("TIME_LIMIT"))
        this.XprsProb.SetIntControl(8020, (int) pars.Get<double>("TIME_LIMIT"));
      if (pars.IsSet("MIP_GAP"))
        this.XprsProb.SetDblControl(7020, pars.Get<double>("MIP_GAP"));
      if (pars.IsSet("HEURISTICS"))
        this.XprsProb.SetDblControl(8154, pars.Get<double>("HEURISTICS"));
      if (pars.IsSet("OUT_FLAG"))
        this.XprsProb.SetIntControl(8035, pars.Get<int>("OUT_FLAG"));
      if (pars.IsSet("LOG_FILE"))
        this.XprsProb.SetStrControl(8035, pars.Get<string>("LOG_FILE"));
      if (pars.IsSet("MIP_GAP_ABS"))
        this.XprsProb.SetDblControl(7019, pars.Get<double>("MIP_GAP_ABS"));
      if (pars.IsSet("THREADS"))
      {
        this.XprsProb.SetIntControl(8079, pars.Get<int>("THREADS"));
        this.XprsProb.SetIntControl(8274, pars.Get<int>("THREADS"));
      }
      if (!pars.IsSet("WRITE_TO_FILE"))
        return;
      this.XprsProb.WriteSol(pars.Get<string>("WRITE_TO_FILE"));
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
      string str = string.Format("{0}.{1}", (object) filenameWithoutExtension, (object) fileType);
      int num;
      switch (fileType)
      {
        case "lp":
          num = this.XprbProb.exportProb(1, str);
          break;
        case "mps":
          num = this.XprbProb.exportProb(2, str);
          break;
        default:
          throw new JDException("Unknown file type {0}", new object[1]
          {
            (object) fileType
          });
      }
      return num == 0;
    }

    public void Dispose() => this.reset();

    public bool SupportsSOS1 => true;

    public bool SupportsSOS2 => true;
  }
}
