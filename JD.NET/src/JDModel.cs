using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Diagnostics;
using JDUtils;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Threading;
using System.Runtime.Serialization;
namespace JDSpace
{
    /// <summary>
    /// JD model object. A model captures all the constraints and variables associated
    /// with one optimization problem. 
    /// </summary>
    [Serializable]
    public class JDModel : ISerializable
    {
        #region << PROPERTIES >>
        /// <summary>
        /// Model Id. Default id: 1.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Model name. Default is "Model{Id}".
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// If set to True, definitions of SOS1 and SOS2 are translated into an explicit formulation using additional binary variables and constraints.
        /// This functionality is handy as SOS are not supported by some solvers and can not be used when decompositions are defined.
        /// </summary>
        public bool UseExplicitSOS { get; private set; }

        /// <summary>
        /// Next opt. variable id.
        /// </summary>
        private int _nextVarId;

        /// <summary>
        /// Next opt. variable id.
        /// </summary>
        private int _nextConstrId;

        /// <summary>
        /// All model opt. scalar continuous variables list.
        /// </summary>
        public List<ScVar> ConVars { get; private set; }

        /// <summary>
        /// All model opt. scalar binary variables list.
        /// </summary>
        public List<ScVar> BinVars { get; private set; }

        /// <summary>
        /// All model opt. scalar integer variables list.
        /// </summary>
        public List<ScVar> IntVars { get; private set; }

        /// <summary>
        /// Get all model opt. variables.
        /// </summary>
        public List<ScVar> Vars
        {
            get
            {
                List<ScVar> vars = new List<ScVar>(ConVars.Count + BinVars.Count + IntVars.Count);
                vars.AddRange(ConVars);
                vars.AddRange(BinVars);
                vars.AddRange(IntVars);
                return vars;
            }
        }

        /// <summary>
        /// Get all named model opt. scalar variables map over names.
        /// </summary>
        public Dictionary<string, ScVar> DicVars
        {
            get
            {
                Dictionary<string, ScVar> dic = new Dictionary<string, ScVar>(ConVars.Count + BinVars.Count + IntVars.Count);
                foreach (ScVar var in ConVars)
                {
                    if (var.Name != null)
                    {
                        dic.Add(var.Name, var);
                    }
                }
                foreach (ScVar var in BinVars)
                {
                    if (var.Name != null)
                    {
                        dic.Add(var.Name, var);
                    }
                }
                foreach (ScVar var in IntVars)
                {
                    if (var.Name != null)
                    {
                        dic.Add(var.Name, var);
                    }
                }
                return dic;
            }
        }

        /// <summary>
        /// Get all named model opt. scalar variables values (if not null) map over names.
        /// </summary>
        public Dictionary<string, double> DicVarsValues
        {
            get
            {
                Dictionary<string, double> dic = new Dictionary<string, double>(ConVars.Count + BinVars.Count + IntVars.Count);
                foreach (ScVar var in ConVars)
                {
                    if ((var.Name != null) && (var.Value != null))
                    {
                        dic.Add(var.Name, (double)var.Value);
                    }
                }
                foreach (ScVar var in BinVars)
                {
                    if ((var.Name != null) && (var.Value != null))
                    {
                        dic.Add(var.Name, (double)var.Value);
                    }
                }
                foreach (ScVar var in IntVars)
                {
                    if ((var.Name != null) && (var.Value != null))
                    {
                        dic.Add(var.Name, (double)var.Value);
                    }
                }
                return dic;
            }
        }

        /// <summary>
        /// Equality constraints
        /// </summary>
        public List<ScConstr> EqConstrs { get; private set; }
        /// <summary>
        /// Lower or equal constraints
        /// </summary>
        public List<ScConstr> LEqConstrs { get; private set; }

        internal ScLinExprFactory ScLinExprFactory { get; private set; }

        /// <summary>
        /// Get all model constraints list.
        /// </summary>
        public List<ScConstr> Constrs
        {
            get
            {
                List<ScConstr> cons = new List<ScConstr>(LEqConstrs.Count + EqConstrs.Count);
                cons.AddRange(LEqConstrs);
                cons.AddRange(EqConstrs);
                return cons;
            }
        }

        /// <summary>
        /// All model SOS (1 and 2 type) constraints.
        /// </summary>
        public List<SOSConstr> SOSConstraints { get; private set; }

        /// <summary>
        /// Model objective function sense.
        /// </summary>
        public int ObjSense { get; private set; }

        /// <summary>
        /// Model objective function.
        /// </summary>
        public ScLinExpr Obj { get; private set; }

        /// <summary>
        /// Model parameters (integer, double, string)
        /// </summary>
        public JDParams Params { get; private set; }

        /// <summary>
        /// Get total optimization variables count.
        /// </summary>
        public int nVars { get { return ConVars.Count + BinVars.Count + IntVars.Count; } }

        /// <summary>
        /// Get total constraints count.
        /// </summary>
        public int nConstraints { get { return Constrs.Count; } }

        /// <summary>
        /// Get JDModel data loadable property.
        /// </summary>
        public bool IsDataLoadable { get; private set; }
        private NamedConstManager _namedConstManager { get; set; }

        /// <summary>
        /// Get JDModel named constants (for data loadable models only).
        /// </summary>
        public IDictionary<string, NamedConst> NamedConstants
        {
            get
            {
                if (IsDataLoadable)
                {
                    return _namedConstManager.NamedConsts;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Add specific constr (first param) to constr set (second param).
        /// </summary>
        private Action<ScConstr, IList<ScConstr>> _addConstr { get; set; }
        /// <summary>
        /// Add specific constrs set (first param) to constr set (second param).
        /// </summary>
        private Action<IList<ScConstr>, List<ScConstr>> _addConstrSet { get; set; }
        #endregion

        /// <summary>
        /// Create new JDModel instance with specific id.
        /// </summary>
        /// <param name="id">Identifier of the model</param>
        /// <param name="firstVarId">Seed for variable ids</param>
        /// <param name="firstConstrId">Seed for constraint ids</param>
        /// <param name="useExplicitSOS">Indication whether explicit (formulated by constraints) SOS to formulations should be used instead of implicit (just let the solver handle SOS its way)</param>
        /// <param name="name">Model name</param>
        /// <param name="dataLoadable">Create model object as data reloadable (enables named constants (NamedConst) value reloading (over name)).</param>
        public JDModel(int id = 1, int firstVarId = 1, int firstConstrId = 1, bool useExplicitSOS = false, string name = null, bool dataLoadable = false)
        {
            ConVars = new List<ScVar>();
            BinVars = new List<ScVar>();
            IntVars = new List<ScVar>();
            LEqConstrs = new List<ScConstr>();
            EqConstrs = new List<ScConstr>();
            SOSConstraints = new List<SOSConstr>();

            Params = new JDParams(JD.GetDefModelIntParams(), JD.GetDefModelDoubleParams(), JD.GetDefModelStringParams());
            Id = id;
            UseExplicitSOS = useExplicitSOS;
            _nextVarId = firstVarId;
            _nextConstrId = firstConstrId;
            if (name == null) Name = id.ToString();
            IsDataLoadable = dataLoadable;
            ScLinExprFactory = new ScLinExprFactory(IsDataLoadable);
            _initConstrAddingActions(IsDataLoadable);
            if (IsDataLoadable) _namedConstManager = new NamedConstManager();
        }

        /// <summary>
        /// Create constr/constrs adding actions.
        /// </summary>
        private void _initConstrAddingActions(bool dataLoadable)
        {
            if (dataLoadable)
            {
                // add constr/constrs to list and register named
                // values with named constants manager
                _addConstr = (con, cons) =>
                {
                    _namedConstManager.Register(con);
                    cons.Add(con);
                };
                _addConstrSet = (consToAdd, consSet) =>
                {
                    _namedConstManager.Register(consToAdd);
                    consSet.AddRange(consToAdd);
                };
            }
            else
            {
                // only add const/constrs to list
                _addConstr = (con, cons) =>
                {
                    cons.Add(con);
                };
                _addConstrSet = (consToAdd, consSet) =>
                {
                    consSet.AddRange(consToAdd);
                };
            }
        }

        /// <summary>
        /// Add JD model to the current model
        /// </summary>
        /// <param name="mdl">JD model</param>
        public void Add(JDModel mdl)
        {
            ConVars.AddRange(mdl.ConVars);
            BinVars.AddRange(mdl.BinVars);
            IntVars.AddRange(mdl.IntVars);
            LEqConstrs.AddRange(mdl.LEqConstrs);
            EqConstrs.AddRange(mdl.EqConstrs);
            SOSConstraints.AddRange(mdl.SOSConstraints);
            if (IsDataLoadable) _namedConstManager.Join(mdl.NamedConstants);
            Obj.Add(mdl.Obj);
        }

        #region << SERIALIZATION AND DESERIALIZATION >>
        /// <summary>
        /// Save JDModel object to file.
        /// </summary>
        public void SaveToFile(string filename)
        {
            using (Stream stream = File.Open(filename, FileMode.Create))
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(stream, this);
                stream.Close();
            }
        }

        /// <summary>
        /// Create JDModel object with source file.
        /// </summary>
        public static JDModel BuildFromFile(string filename)
        {
            JDModel data;
            using (Stream stream = File.Open(filename, FileMode.Open))
            {
                BinaryFormatter bf = new BinaryFormatter();
                data = (JDModel)bf.Deserialize(stream);
                stream.Close();
            }
            return data;
        }
        #endregion

        /// <summary>
        /// Get important model parameters as Param objects set.
        /// </summary>
        public List<Param> ToParams()
        {
            List<Param> pars = new List<Param>();
            pars.AddRange(Params.ToParams());
            int conCount = ConVars.Count;
            int binCount = BinVars.Count;
            int intCount = IntVars.Count;
            pars.Add(new Param("Model_name", Name));
            pars.Add(new Param("Vars_count", conCount + binCount + intCount));
            if (conCount > 0)
                pars.Add(new Param("Con_vars_count", conCount));
            if (binCount > 0)
                pars.Add(new Param("Bin_vars_count", binCount));
            if (intCount > 0)
                pars.Add(new Param("Int_vars_count", intCount));
            pars.Add(new Param("Constr_count", EqConstrs.Count + LEqConstrs.Count));
            if (LEqConstrs.Count > 0)
                pars.Add(new Param("Leq_constr_count", LEqConstrs.Count));
            if (EqConstrs.Count > 0)
                pars.Add(new Param("Eq_constr_count", EqConstrs.Count));
            if (SOSConstraints.Count > 0)
                pars.Add(new Param("SOS_constrs_count", SOSConstraints.Count));
            return pars;
        }

        #region << MODEL COMPONENTS ADDING >>
        /// <summary>
        /// Adds a new variable to the model.
        /// </summary>
        /// <param name="xSize">First dimension size for new variable.</param>
        /// <param name="ySize">Second dimension size for new variable.</param>
        /// <param name="lb">Lower bound for new variable.</param>
        /// <param name="ub">Upper bound for new variable.</param>
        /// <param name="type">Variable type for new variable (JD.CONTINUOUS, JD.BINARY, JD.INTEGER).</param>
        /// <param name="name">Variables name-prefix.</param>
        /// <param name="braPri">Variables branch priority.</param>
        /// <returns>New JDVar object.</returns>
        public JDVar AddVar(int xSize = 1, int ySize = 1, double lb = -JD.INFINITY, double ub = JD.INFINITY, char type = JD.CONTINUOUS, string name = null, int braPri = 0)
        {
            int count = xSize * ySize;
            if (type == JD.BINARY)
            {
                if (lb == -JD.INFINITY)
                    lb = 0;
                if (ub == JD.INFINITY)
                    ub = 1;
            }
            List<ScVar> varList = AddScVars(count, lb, ub, type, name, braPri);
            return new JDVar(varList, xSize, ySize, ScLinExprFactory);
        }

        /// <summary>
        /// Create new instance of linear expression.
        /// </summary>
        /// <param name="linExprs">Scalar linear expressions array.</param>
        /// <param name="xSize">First dimension size.</param>
        /// <param name="ySize">Second dimension size.</param>
        public JDLinExpr AddLinExpr(List<ScLinExpr> linExprs, int xSize, int ySize)
        {
            JDLinExpr expr = new JDLinExpr(linExprs, xSize, ySize, ScLinExprFactory);
            return expr;
        }

        /// <summary>
        /// Put named constant to model.
        /// </summary>
        public void AddNamedConstant(NamedConst namCon)
        {
            _namedConstManager.Register(namCon);
        }
        #endregion

        /// <summary>
        /// Get specific named const.
        /// </summary>
        public NamedConst GetNameConstant(string name)
        {
            NamedConst nc = _namedConstManager.GetNamedConst(name);
            return nc;
        }

        /// <summary>
        /// Create new instance of scalar linear expression from existing
        /// scalar linear expression.
        /// </summary>
        /// <param name="scLinExpr">Existing scalar linear expression.</param>
        public JDLinExpr AddLinExpr(ScLinExpr scLinExpr)
        {
            JDLinExpr expr = new JDLinExpr(scLinExpr, ScLinExprFactory);
            return expr;
        }

        /// <summary>
        /// Class constructor. Create instance of two-dimensional linear expression.
        /// </summary>
        /// <param name="xSize">First dimension size.</param>
        /// <param name="ySize">Second dimension size.</param>
        public JDLinExpr AddLinExpr(int xSize, int ySize)
        {
            JDLinExpr expr = new JDLinExpr(xSize, ySize, ScLinExprFactory);
            return expr;
        }

        /// <summary>
        /// Creates two-dimensional new JDLinExpr instance using existing
        /// JDLinExpr object.
        /// </summary>
        /// <param name="subLinExpr">Existing JDLinExpr object.</param>
        /// <param name="x1">First line index of choosen line range.</param>
        /// <param name="x2">End line index of choosen line range.</param>
        /// <param name="y1">First column index of choosen column range.</param>
        /// <param name="y2">End column index of choosen column range.</param>
        public JDLinExpr AddLinExpr(JDLinExpr subLinExpr, int x1, int x2, int y1, int y2)
        {
            JDLinExpr expr = new JDLinExpr(subLinExpr, x1, x2, y1, y2, ScLinExprFactory);
            return expr;
        }

        /// <summary>
        /// Adds a new scalar opt. variable to the model.
        /// </summary>
        /// <param name="lb">Lower bound for new variable.</param>
        /// <param name="ub">Upper bound for new variable.</param>
        /// <param name="type">Variable type for new variable (JD.CONTINUOUS, JD.BINARY, JD.INTEGER).</param>
        /// <param name="name">Name of new variable.</param>
        /// <param name="braPri">Branch priority (0 default)</param>
        /// <returns></returns>
        public ScVar AddScVar(double lb, double ub, char type, string name = null, int braPri = 0)
        {
            return AddScVars(1, lb, ub, type, name, braPri)[0];
        }

        /// <summary>
        /// Add new variables.
        /// </summary>
        /// <param name="count">New variables count.</param>
        /// <param name="lb">Lower bound for new variable.</param>
        /// <param name="ub">Upper bound for new variable.</param>
        /// <param name="type">Variable type for new variable (JD.CONTINUOUS, JD.BINARY, JD.INTEGER)</param>
        /// <param name="name">Name of new variable.</param>
        /// <param name="braPri">Branch priority (0 default)</param>
        /// <returns>New variables list.</returns>
        public List<ScVar> AddScVars(int count, double lb, double ub, char type, string name = null, int braPri = 0)
        {
            int firstId = _nextVarId;
            _nextVarId += count; // reserv [_nextId,  _nextId + count - 1] varIds
            List<ScVar> varsList = new List<ScVar>(count);
            int id = 0;
            if (name != null)
            {
                string varName;
                for (int i = 0; i < count; i++)
                {
                    id = firstId + i;
                    varName = String.Format("{0}{1}", name, id);
                    varsList.Add(new ScVar(id, varName, type, lb, ub, braPri));
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    id = firstId + i;
                    varsList.Add(new ScVar(id, name, type, lb, ub, braPri));
                }
            }
            switch (type)
            {
                case JD.CONTINUOUS:
                    ConVars.AddRange(varsList);
                    break;
                case JD.BINARY:
                    BinVars.AddRange(varsList);
                    break;
                case JD.INTEGER:
                    IntVars.AddRange(varsList);
                    break;
                default:
                    throw new JDException("Unknown variable type {0}", type);
            }
            return varsList;
        }

        /// <summary>
        /// Adds a new variable to the model. Upper bound is INFINITY, lower bound is MINUS INFINITY.
        /// Type is CONTINUOUS.
        /// </summary>
        /// <param name="xSize">First dimension size for new variable.</param>
        /// <param name="ySize">Second dimension size for new variable.</param>
        /// <returns>New variable object.</returns>
        public JDVar AddVar(int xSize, int ySize)
        {
            JDVar jdVar = AddVar(xSize, ySize, -JD.INFINITY, JD.INFINITY, JD.CONTINUOUS);
            return jdVar;
        }

        /// <summary>
        /// Adds a new constraint to the model.
        /// </summary>
        /// <param name="constr">JD constraint temporary object.</param>
        internal void AddConstraint(JDTempConstraint constr)
        {
            int numel = Math.Max(constr.Rhs.Numel, constr.Lhs.Numel);
            List<ScConstr> scCons = new List<ScConstr>(numel);
            ScLinExpr scExpr;
            if (constr.Sense == JD.GREATER_EQUAL)
            {
                for (int i = 0; i < numel; i++)
                {
                    scExpr = ScLinExprFactory.CreateScLinExpr();
                    scExpr.Add(constr.Rhs.GetScLinExpr(i));
                    scExpr.Add(-1, constr.Lhs.GetScLinExpr(i));
                    ScConstr con = new ScConstr(_nextConstrId, scExpr, JD.LESS_EQUAL, 0, constr.Name, constr.LazyLevel);
                    if (con.IsValid())
                    {
                        scCons.Add(con);
                        _nextConstrId++;
                    }
                }
            }
            else
            {
                for (int i = 0; i < numel; i++)
                {
                    scExpr = ScLinExprFactory.CreateScLinExpr();
                    scExpr.Add(constr.Lhs.GetScLinExpr(i));
                    scExpr.Add(-1, constr.Rhs.GetScLinExpr(i));
                    ScConstr con = new ScConstr(_nextConstrId, scExpr, constr.Sense, 0, constr.Name, constr.LazyLevel);
                    if (con.IsValid())
                    {
                        scCons.Add(con);
                        _nextConstrId++;
                    }
                }
            }
            // sort constrs by sense type
            if (constr.Sense == JD.EQUAL)
            {
                _addConstrSet(scCons, EqConstrs);
            }
            else
            {
                _addConstrSet(scCons, LEqConstrs);
            }
        }

        /// <summary>
        /// Sets the model objective equal to the argument expression.
        /// </summary>
        /// <param name="obj">New model objective.</param>
        /// <param name="sense">New optimization sense (JD.MINIMIZE for minimization, JD.MAXIMIZE for maximization).</param>
        public void SetObjective(ScLinExpr obj, int sense)
        {
            Obj = obj;
            if (IsDataLoadable) _namedConstManager.Register(obj);
            ObjSense = sense;
        }

        /// <summary>
        /// Sets the model objective equal to the argument expression.
        /// </summary>
        /// <param name="obj">New model objective.</param>
        /// <param name="sense">New optimization sense (JD.MINIMIZE for minimization, JD.MAXIMIZE for maximization).</param>
        public void SetObjective(JDLinExpr obj, int sense)
        {            
            SetObjective(obj.Sum().LinExprs[0], sense);
        }

        /// <summary>
        /// Sets the model objective equal to the argument variable. If argument variable is not scalar, then objective is
        /// set as sum of its elements.
        /// </summary>
        /// <param name="jdVar">New model objective.</param>
        /// <param name="sense">New optimization sense (JD.MINIMIZE for minimization, JD.MAXIMIZE for maximization).</param>
        public void SetObjective(JDVar jdVar, int sense = JD.MINIMIZE)
        {
            SetObjective(jdVar.Sum(), sense);
        }

        /// <summary>
        /// Remove all model elements (variables, constraints).
        /// </summary>
        public void Reset()
        {
            ConVars.Clear();
            Constrs.Clear();
            SOSConstraints.Clear();
        }

        

        #region << CONSTANT VARS CHECKING AND REPLACEING >>

        /// <summary>
        /// Replace constant variables (equal Lb, Ub values) with Lb value.
        /// It simplifies optimization task.
        /// </summary>
        public void ReplaceConstantVars()
        {
            // get Lb, Ub equal variables
            List<ScVar> constantVars = GetConstantVars();
            // replace these vars with Ub constants in constraints
            _replaceConstantVars(constantVars, LEqConstrs);
            _replaceConstantVars(constantVars, EqConstrs);
            // put constant variables values with Ub
            foreach (ScVar var in constantVars)
                var.Value = var.Ub;
        }

        /// <summary>
        /// Replace constant vars in inserted constraints with the specific constant.
        /// </summary>
        private void _replaceConstantVars(List<ScVar> constantVars, List<ScConstr> constrs)
        {
            foreach (ScConstr constr in constrs) _replaceConstantVars(constantVars, constr);
        }

        /// <summary>
        /// Replace constant vars in inserted constraints with the specific constant.
        /// </summary>
        private void _replaceConstantVars(List<ScVar> constantVars, ScConstr constr)
        {
            ScLinExpr expr = constr.Lhs;
            List<ScTerm> toRemove = new List<ScTerm>();
            foreach (ScTerm term in expr.Terms)
            {
                // if var is constant var
                if (constantVars.Contains(term.Var))
                {
                    ComposedConstant constToAdd = new NamedConst(term.Var.Ub) * term.CoeffObj.ToComposedConstant();
                    expr.Add(constToAdd);
                    toRemove.Add(term);
                }
            }
            // remove constant var terms
            foreach (ScTerm term in toRemove) expr.Terms.Remove(term);
        }

        /// <summary>
        /// Remove redundant constraints of type: 0 leq BinVar leq 1
        /// </summary>
        public void RemoveRedundantConstraints()
        {
            List<ScConstr> toPreserve = new List<ScConstr>();
            foreach (ScConstr constr in LEqConstrs)
            {
                if (constr.Lhs.Terms.Count == 1)
                {
                    if (constr.Lhs.Terms[0].Var.Type == JD.BINARY)
                    {
                        if (constr.Sense == JD.LESS_EQUAL && constr.Lhs.Constant <= -1 && constr.Lhs.Terms[0].Coeff == 1)
                            continue;
                        if (constr.Sense == JD.LESS_EQUAL && constr.Lhs.Constant == 0 && constr.Lhs.Terms[0].Coeff == -1)
                            continue;
                        if (constr.Sense == JD.GREATER_EQUAL && constr.Lhs.Constant == 0 && constr.Lhs.Terms[0].Coeff == 1)
                            continue;
                    }
                }
                toPreserve.Add(constr);
            }
            int remCount = LEqConstrs.Count - toPreserve.Count;
            int count = LEqConstrs.Count;
            LEqConstrs = new List<ScConstr>(toPreserve);
            if (remCount > 0)
                Console.WriteLine(String.Format("Constraints \n Before remove : \t{0} \n After remove : \t{1} \n Removed :          \t{2}", count, LEqConstrs.Count, remCount));
        }

        /// <summary>
        /// Get variables with equal Ub,Lb values - the only possible variable value.
        /// </summary>
        public List<ScVar> GetConstantVars()
        {
            List<ScVar> constantVars = new List<ScVar>();
            _getConstantVars(ConVars, constantVars);
            _getConstantVars(IntVars, constantVars);
            _getConstantVars(BinVars, constantVars);
            return constantVars;
        }

        /// <summary>
        /// Get variables (from inserted set) with equal Ub,Lb values - the only possible variable value.
        /// </summary>
        private void _getConstantVars(List<ScVar> vars, List<ScVar> constantVarsToOut)
        {
            foreach (ScVar var in vars)
            {
                if (var.Lb == var.Ub) constantVarsToOut.Add(var);
            }
        }

        #endregion

        #region << SOS CONSTRAINTS >>
        /// <summary>
        /// Add SOS type 1 constraint to the model.
        /// </summary>
        /// <param name="var">Variable vector ([n x 1] or [1 x n] JDVar) that participate 
        /// in the SOS constraint.</param>
        /// <param name="weights">Weights for the variables in the SOS constraint.</param>
        public void AddSOS1(List<ScVar> var, double[] weights)
        {
            int type = 1;
            AddSOS(var, weights, type);
        }

        /// <summary>
        /// Add SOS type 2 constraint to the model.
        /// </summary>
        /// <param name="var">Variable vector ([n x 1] or [1 x n] JDVar) that participate 
        /// in the SOS constraint.</param>
        /// <param name="weights">Weights for the variables in the SOS constraint.</param>
        public void AddSOS2(List<ScVar> var, double[] weights)
        {
            int type = 2;
            AddSOS(var, weights, type);
        }

        private void AddSOS(List<ScVar> var, double[] weights, int type)
        {

            if (UseExplicitSOS)
            {
                addSOSExplicit(var, type);
            }
            else
            {
                SOSConstr sosC = new SOSConstr(var, weights, type);
                SOSConstraints.Add(sosC);
            }
        }

        /// <summary>
        /// Returns binary variable indicating whether the continuous variable v is nonzero.
        /// </summary>
        /// <param name="v">A continuous variable with finite LB and UB. Zero has to be valid value.</param>
        /// <returns></returns>
        private ScVar getNonZeroValueIndicator(ScVar v)
        {
            if (v.Type == JD.BINARY)
            {
                throw new JDException(String.Format("It does not make sense to define bigM semicontinuous formulation for binary variable {0}.", v.Name));
            }

            if (v.Ub == Double.MaxValue || v.Lb == Double.MinValue) //UB and LB have to be defined
            {
                throw new JDException(String.Format("Only variables with defined upper and lower bounds may be made semicontiuous. Variable {0} has either no upper or lower bound defined.", v.Name));
            }

            if (v.Lb > 0 || v.Ub < 0)
            {
                throw new JDException(String.Format("The variable {0} cannot be in SOS as its lower bound is greater than zero or upper bound less than zero, i.e. zero is not in the domain of this variable."), v.Id);
            }

            var b = AddScVar(0, 1, JD.BINARY); //an additional binary is defined

            //semicontinous behaviour of v is defined using bigM formulation: b*LB <= v <= b*UB
            if (v.Lb < 0)
            {
                Debug.Assert(v.Ub >= 0);
                //b*LB <= v
                var lbExpr = ScLinExprFactory.CreateScLinExpr();
                ScLinExprFactory.CreateScLinExpr();
                lbExpr.AddTerm(v.Lb, b);
                lbExpr.AddTerm(-1, v);
                var con = new ScConstr(_nextConstrId, lbExpr, JD.LESS_EQUAL, 0, "bigMLb_" + v.Name);
                _addConstr(con, LEqConstrs);
                _nextConstrId++;
            }

            if (v.Ub > 0) //and v.Lb <= 0
            {
                Debug.Assert(v.Lb <= 0);
                //v <= b*UB
                var ubExpr = ScLinExprFactory.CreateScLinExpr();
                ubExpr.AddTerm(1, v);
                ubExpr.AddTerm(-v.Ub, b);
                var con = new ScConstr(_nextConstrId, ubExpr, JD.LESS_EQUAL, 0, "bigMUb_" + v.Name);
                _addConstr(con, LEqConstrs);
                _nextConstrId++;
            }
            return b;
        }

        /// <summary>
        /// Explicit formulation of SOS1 is created using additional variables and constraints.
        /// </summary>
        /// <param name="vars">Variables to be included in the set. All the variables have to be binary.</param>
        private void addSOS1Explicit(List<ScVar> vars)
        {
            var expr = ScLinExprFactory.CreateScLinExpr();
            foreach (var v in vars)
            {
                if (v.Type != JD.BINARY)
                {
                    throw new JDException(String.Format("Variable within SOS has to be binary while the variable {0} is not.", v.Id));
                }
                expr.AddTerm(1, v);
            }

            //The sum over the set has to be 1 (binary variables).
            var constr = new ScConstr(_nextConstrId, expr, JD.LESS_EQUAL, 1, "ExplicitSOS1_" + _nextConstrId.ToString());
            //LEqConstrs.Add(constr);
            _addConstr(constr, LEqConstrs);
            _nextConstrId++;
        }

        /// <summary>
        /// Explicit formulation of SOS2 is created using additional variables and constraints.
        /// </summary>
        /// <param name="vars">Variables to be included in the set. All the variables have to be binary.</param>
        private void addSOS2Explicit(List<ScVar> vars)
        {
            //Check variable type (binaries required)
            foreach (var v in vars)
            {
                if (v.Type != JD.BINARY)
                {
                    throw new JDException(String.Format("Variable within SOS has to be binary while the variable {0} is not.", v.Id));
                }
            }

            //Variables su expressing "startups" 0->1 are defined
            var su = AddVar(vars.Count, lb: 0, ub: 1).VarList;

            ScLinExpr expr1;
            ScLinExpr expr2;
            ScLinExpr startupNr = ScLinExprFactory.CreateScLinExpr(); //sum(su)
            ScLinExpr nonzerosNr = ScLinExprFactory.CreateScLinExpr(); //sum(vars)
            for (int k = 0; k < su.Count; k++)
            {
                expr1 = ScLinExprFactory.CreateScLinExpr(); //su[k] >= u[k] - u[k-1]
                expr2 = ScLinExprFactory.CreateScLinExpr(); //su[k] <= u[k]
                if (k > 0)
                {
                    expr1.AddTerm(1, vars[k]);
                    expr1.AddTerm(-1, vars[k - 1]);
                    expr1.AddTerm(-1, su[k]);
                }
                else
                { //if the first var in vars is nonzero then count it as a startup, su[k] >= u[k]
                    expr1.AddTerm(1, vars[k]);
                    expr1.AddTerm(-1, su[k]);
                }

                expr2.AddTerm(-1, vars[k]);
                expr2.AddTerm(1, su[k]);

                //LEqConstrs.Add(new ScConstr(_nextConstrId, expr1, JD.LESS_EQUAL, 0, "ExplicitSOS2_" + _nextConstrId.ToString()));
                _addConstr(new ScConstr(_nextConstrId, expr1, JD.LESS_EQUAL, 0, "ExplicitSOS2_" + _nextConstrId.ToString()), LEqConstrs);
                _nextConstrId++;
                //LEqConstrs.Add(new ScConstr(_nextConstrId, expr2, JD.LESS_EQUAL, 0, "ExplicitSOS2_" + _nextConstrId.ToString()));
                _addConstr(new ScConstr(_nextConstrId, expr2, JD.LESS_EQUAL, 0, "ExplicitSOS2_" + _nextConstrId.ToString()), LEqConstrs);
                _nextConstrId++;

                startupNr.AddTerm(1, su[k]);
                nonzerosNr.AddTerm(1, vars[k]);
            }

            Debug.Assert(su.Count == vars.Count);

            //at most two variables may be nonzero
            //LEqConstrs.Add(new ScConstr(_nextConstrId, nonzerosNr, JD.LESS_EQUAL, 2, "ExplicitSOS2NonzeroNr_" + _nextConstrId.ToString()));
            _addConstr(new ScConstr(_nextConstrId, nonzerosNr, JD.LESS_EQUAL, 2, "ExplicitSOS2NonzeroNr_" + _nextConstrId.ToString()), LEqConstrs);
            _nextConstrId++;

            //finally, only a single startup may occur, hence only adjacent variables may be nonzero
            //LEqConstrs.Add(new ScConstr(_nextConstrId, startupNr, JD.LESS_EQUAL, 1, "ExplicitSOS2StartupNr_" + _nextConstrId.ToString()));
            _addConstr(new ScConstr(_nextConstrId, startupNr, JD.LESS_EQUAL, 1, "ExplicitSOS2StartupNr_" + _nextConstrId.ToString()), LEqConstrs);
            _nextConstrId++;
        }

        /// <summary>
        /// Explicit formulation of SOS1 and SOS2 is created using additional variables (where required) and constraints.
        /// </summary>
        /// <param name="v">Variables of any type to be included into the set.</param>
        /// <param name="type">Type of SOS. Currently only types 1 and 2 are supported.</param>
        private void addSOSExplicit(List<ScVar> v, int type)
        {
            //First, indicator variables b are defined, indicating whether v are (non)zero
            //For the sake of numerical stability, finite bounds for variables are required
            List<ScVar> b = new List<ScVar>(v.Count);
            for (int k = 0; k < v.Count; k++)
            {
                if (v[k].Type == JD.BINARY)
                {
                    //if the variable is binary, then no additional variables and constraints have to be created
                    b.Add(v[k]);
                }
                else
                {
                    //otherwise the variable v is made semicontinous
                    var new_b = getNonZeroValueIndicator(v[k]);
                    b.Add(new_b); //instead of the variable v[k] new_b, defining whether v[k] is (non)zero, is included
                }
            }

            switch (type)
            {
                case 1:
                    addSOS1Explicit(b);
                    break;
                case 2:
                    addSOS2Explicit(b);
                    break;
                default:
                    throw new JDException(String.Format("Unknown SOS type {0}.", type));
            }
        }

        /// <summary>
        /// Add SOS type 1 constraint to the model.
        /// </summary>
        /// <param name="var">Variable vector ([n x 1] or [1 x n] JDVar) that participate 
        /// in the SOS constraint.</param>
        /// <param name="weights">Weights for the variables in the SOS constraint.</param>
        public void AddSOS1(JDVar var, double[] weights)
        {
            if (((var.XSize == weights.Length) & (var.YSize == 1)) |
                ((var.YSize == weights.Length) & (var.XSize == 1)))
            {
                AddSOS1(var.VarList, weights);
            }
            else
            {
                // kriterialni funkci nemuze byt viceprvkovy JDLinExpr
                throw new JDException("Error: Bad JDVar size [{0} x {1}] for SOS2 usage! Only [n x 1] or [1 x n] size is possible.", var.XSize, var.YSize);
            }
        }

        /// <summary>
        /// Add SOS type 2 constraint to the model.
        /// </summary>
        /// <param name="var">Variable vector ([n x 1] or [1 x n] JDVar) that participate 
        /// in the SOS constraint.</param>
        /// <param name="weights">Weights for the variables in the SOS constraint.</param>
        public void AddSOS2(JDVar var, double[] weights)
        {
            if (((var.XSize == weights.Length) & (var.YSize == 1)) |
                ((var.YSize == weights.Length) & (var.XSize == 1)))
            {
                AddSOS2(var.VarList, weights);
            }
            else
            {
                // kriterialni funkci nemuze byt viceprvkovy JDLinExpr
                throw new JDException("Error: Bad JDVar size [{0} x {1}] for SOS2 usage! Only [n x 1] or [1 x n] size is possible.", var.XSize, var.YSize);
            }
        }

        /// <summary>
        /// For adding constraint to model is possible tu use + operator.
        /// </summary>
        /// <param name="model">Instance of JDModel.</param>
        /// <param name="constr">JD temporary constraint object.</param>
        /// <returns>Model updated with constraint.</returns>
        public static JDModel operator +(JDModel model, JDTempConstraint constr)
        {
            model.AddConstraint(constr);
            return model;
        }

        /// <summary>
        /// For adding constraints to model is possible tu use + operator.
        /// </summary>
        /// <param name="model">Instance of JDModel.</param>
        /// <param name="constrs">JD temporary constraints List.</param>
        /// <returns>Model updated with constraint.</returns>
        public static JDModel operator +(JDModel model, JDTempConstraintsList constrs)
        {
            foreach (JDTempConstraint con in constrs.Cons)
            {
                model.AddConstraint(con);
            }
            return model;
        }
        #endregion

        #region << EXPLICIT SERIALIZATION >>

        /// <summary>
        /// JD model deserialization
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        public JDModel(SerializationInfo info, StreamingContext context)
        {
            SerializationReader sr = SerializationReader.GetReader(info);
            readObjectData(sr);
        }

        /// <summary>
        /// JD model serialization
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            SerializationWriter sw = SerializationWriter.GetWriter();
            writeObjectData(sw);
            sw.AddToInfo(info);
        }

        /// <summary>
        /// Vytvořeno kvůli dědičnosti. Odvozená třída provede override této metody.
        /// </summary>
        /// <param name="sr">Objekt, ze kterého jsou čtena deserializovaná data.</param>
        protected virtual void readObjectData(SerializationReader sr)
        {
            Id = sr.ReadInt32();
            Name = sr.ReadString();
            _nextVarId = sr.ReadInt32();
            _nextConstrId = sr.ReadInt32();
            IsDataLoadable = sr.ReadBoolean();
            ScLinExprFactory = new ScLinExprFactory(IsDataLoadable);
            _initConstrAddingActions(IsDataLoadable);
            if (IsDataLoadable) _namedConstManager = new NamedConstManager();
            JDModelSerializationHelper.ScLinExprFactory = ScLinExprFactory;

            int nConVars = sr.ReadInt32();
            int nBinVars = sr.ReadInt32();
            int nIntVars = sr.ReadInt32();
            IList<ScVar> vars = sr.ReadList<ScVar>();
            JDModelSerializationHelper.VarsMap = vars.ToDictionary(x => x.Id, x => x);
            ConVars = new List<ScVar>(nConVars);
            BinVars = new List<ScVar>(nBinVars);
            IntVars = new List<ScVar>(nIntVars);
            for (int i = 0; i < nConVars; i++) ConVars.Add(vars[i]);
            for (int i = nConVars; i < nConVars + nBinVars; i++) BinVars.Add(vars[i]);
            for (int i = nConVars + nBinVars; i < nConVars + nBinVars + nIntVars; i++) IntVars.Add(vars[i]);
            int nLEConstrs = sr.ReadInt32();
            int nEqConstrs = sr.ReadInt32();
            IList<ScConstr> constrs = sr.ReadList<ScConstr>();
            LEqConstrs = new List<ScConstr>(nLEConstrs);
            EqConstrs = new List<ScConstr>(nEqConstrs);
            for (int i = 0; i < nLEConstrs; i++) _addConstr(constrs[i], LEqConstrs); // LEqConstrs.Add(constrs[i]);
            for (int i = nLEConstrs; i < nLEConstrs + nEqConstrs; i++) _addConstr(constrs[i], EqConstrs);// EqConstrs.Add(constrs[i]);
            SOSConstraints = sr.ReadList<SOSConstr>().ToList();
            ObjSense = sr.ReadInt32();
            Obj = (ScLinExpr)sr.ReadObject();
            Params = (JDParams)sr.ReadObject();
            if (IsDataLoadable)
            {
                _namedConstManager.Register(Obj);
                IList<NamedConst> otherNamedConstants = sr.ReadList<NamedConst>();
                foreach (NamedConst nc in otherNamedConstants) _namedConstManager.NamedConsts.Add(nc.Name, nc);
                _namedConstManager.ClearDupl(LEqConstrs);
                _namedConstManager.ClearDupl(EqConstrs);
                _namedConstManager.ClearDupl(Obj);
            }
        }

        /// <summary>
        /// Vytvořeno kvůli dědičnosti. Odvozená třída provede override této metody.
        /// </summary>
        /// <param name="sw">Objekt, do kterého jsou zapisovaná serializovaná data.</param>
        protected virtual void writeObjectData(SerializationWriter sw)
        {
            sw.Write(Id);
            sw.Write(Name);
            sw.Write(_nextVarId);
            sw.Write(_nextConstrId);
            sw.Write(IsDataLoadable);
            if (IsDataLoadable)
            {
                JDModelSerializationHelper.NamedConstants = _namedConstManager.NamedConsts.Keys.ToList();
            }
            int nConVars = ConVars.Count;
            int nBinVars = BinVars.Count;
            int nIntVars = IntVars.Count;
            List<ScVar> vars = new List<ScVar>(nConVars + nBinVars + nIntVars);
            vars.AddRange(ConVars);
            vars.AddRange(BinVars);
            vars.AddRange(IntVars);
            int nLEConstrs = LEqConstrs.Count;
            int nEqConstrs = EqConstrs.Count;
            List<ScConstr> constrs = new List<ScConstr>(nLEConstrs + nEqConstrs);
            constrs.AddRange(LEqConstrs);
            constrs.AddRange(EqConstrs);

            sw.Write(nConVars);
            sw.Write(nBinVars);
            sw.Write(nIntVars);
            sw.Write<ScVar>(vars);
            sw.Write(nLEConstrs);
            sw.Write(nEqConstrs);
            sw.Write<ScConstr>(constrs);
            sw.Write<SOSConstr>(SOSConstraints);
            sw.Write(ObjSense);
            sw.WriteObject(Obj);
            sw.WriteObject(Params);
            if (IsDataLoadable)
            {
                List<NamedConst> otherNamedConstants = JDModelSerializationHelper.NamedConstants.Select(x => _namedConstManager.NamedConsts[x]).ToList();
                sw.Write<NamedConst>(otherNamedConstants);
            }
        }

        #endregion << EXPLICIT SERIALIZATION >>

        #region << DATA MODEL SEPARATION FEATURES >>
        /// <summary>
        /// Update values of named constants
        /// </summary>
        /// <param name="modelDataToChange">Dictionary with update values</param>
        public void LoadDataValues(IDictionary<string, double> modelDataToChange)
        {
            if (!IsDataLoadable) throw new JDException("Model is not build as data loadable! Create JDModel with dataLoadable true parameter");
            foreach (KeyValuePair<string, double> pair in modelDataToChange)
            {
                _namedConstManager.ChangeValue(pair.Key, pair.Value);
            }
        }

        #endregion
    }

    internal class JDModelSerializationHelper
    {
        /// <summary>
        /// Scalar variables map
        /// </summary>
        public static Dictionary<int, ScVar> VarsMap;
        /// <summary>
        /// Scalar linear expression factory
        /// </summary>
        public static ScLinExprFactory ScLinExprFactory;
        /// <summary>
        /// Named constants list
        /// </summary>
        public static List<string> NamedConstants;
    }

    /// <summary>
    /// Common external solver interface.
    /// </summary>
    public interface IJDSolver
    {

        /// <summary>
        /// Set logger object (callback).
        /// </summary>
        void SetLogger(Logger logger);

        /// <summary>
        /// Get logger object (callback).
        /// </summary>
        Logger GetLogger();

        /// <summary>
        /// Update optim. variables set in solver (call befor adding constraints).
        /// </summary>
        void Update();

        /// <summary>
        /// Clear solver state (remove all variables and constraints).
        /// </summary>
        void Reset();

        /// <summary>
        /// Add SOS constraint to solver.
        /// </summary>
        /// <param name="sos">SOS constraint</param>
        void AddSOSConstr(SOSConstr sos);

        /// <summary>
        /// Set inserted model objective function.
        /// </summary>
        void SetObjective(ScLinExpr obj, int sense);

        /// <summary>
        /// Solve inserted model with given parameters.
        /// </summary>
        void Optimize(JDParams pars);

        /// <summary>
        /// Get resolved variable value (or null if not solved).
        /// </summary>
        double? GetVarValue(int id);

        // testing methods
        /// <summary>
        /// Add list of scalar variables
        /// </summary>
        /// <param name="vars">List of scalar variables</param>
        void AddScVars(List<ScVar> vars);
        /// <summary>
        /// Add list of scalar constraints
        /// </summary>
        /// <param name="contstrs">List of scalar constraints</param>
        void AddConstrs(List<ScConstr> contstrs);
    }

    /// <summary>
    /// JDModel solving service interface. Encapsulation for local and
    /// remote IJDSolver objects.
    /// </summary>
    public interface IJDSolverWrapper
    {
        /// <summary>
        /// Solve inserted model. Return solved model
        /// with out parameter. Retrun false if not success.
        /// </summary>
        bool Solve(JDModel jdMdl, out JDModel solvedMdl);
        /// <summary>
        /// Set logging callback object.
        /// </summary>
        void SetLogger(Logger logger);
        /// <summary>
        /// Reset used solver.
        /// </summary>
        void Reset();
    }

    /// <summary>
    /// Local IJDSolver wrapper.
    /// </summary>
    public class CommonJDSolverWrapper : IJDSolverWrapper
    {
        /// <summary>
        /// Used solver.
        /// </summary>
        IJDSolver Solver;

        /// <summary>
        /// Standard constructor.
        /// </summary>
        /// <param name="solver">Used solver.</param>
        public CommonJDSolverWrapper(IJDSolver solver)
        {
            Solver = solver;
        }

        #region << IJDSolverWrapper IMPLEMENTATION >>
        /// <summary>
        /// Solve JD model
        /// </summary>
        /// <param name="jdMdl">JD model to solve</param>
        /// <param name="solvedMdl">Solved JD model</param>
        /// <returns>True</returns>
        public bool Solve(JDModel jdMdl, out JDModel solvedMdl)
        {
            Solver.Solve(jdMdl);
            solvedMdl = jdMdl; // the same instance in this case (fast)
            return true; // easy case - always retrun true.
        }

        /// <summary>
        /// Reset solver state
        /// </summary>
        public void Reset()
        {
            Solver.Reset();
        }

        /// <summary>
        /// Set logger to solver
        /// </summary>
        /// <param name="logger">Logger object</param>
        public void SetLogger(Logger logger)
        {
            Solver.SetLogger(logger);
        }

        #endregion << IJDSolverWrapper IMPLEMENTATION >>

        /// <summary>
        /// Solver label (local).
        /// </summary>
        public override string ToString()
        {
            return String.Format("{0} (local)", Solver.GetType().Name);
        }
    }

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
        private static bool _tryGetResult(this IJDSolver t, JDModel jdMdl){
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
    }

    /// <summary>
    /// Condition depending on task states.
    /// </summary>    
    public interface IComputeCondition
    {
        /// <summary>
        /// Returns decision whether solve or not.
        /// </summary>
        ECondResult CanSolve(Dictionary<int, ETaskState> tasks);
    }

    /// <summary>
    /// JD task cispatcher class
    /// </summary>
    [Serializable]
    public class JDTaskDispatcher : IComputeCondition
    {
        /// <summary>
        /// State table
        /// </summary>
        StateTable stateTable;

        /// <summary>
        /// Create JDTaskDispatcher with initial state table
        /// </summary>
        /// <param name="stateTable"></param>
        public JDTaskDispatcher(StateTable stateTable)
        {
            this.stateTable = stateTable;
        }

        /// <summary>
        /// Return possible IConditionResults
        /// </summary>
        /// <param name="tasks">Dictionary with tasks</param>
        /// <returns>State </returns>
        public ECondResult CanSolve(Dictionary<int, ETaskState> tasks)
        {
            if (tasks == null) throw new JDException("Dictionary with tasks cannot be null.");

            HashSet<Tuple<int, ETaskState>> state = new HashSet<Tuple<int, ETaskState>>();

            foreach (var task in tasks)
            {
                state.Add(new Tuple<int, ETaskState>(task.Key, task.Value));
            }

            if (stateTable.ContainsKey(state))
            {
                return stateTable[state];
            }
            else
            {
                //pripadne by se vracel akci default(ECondResult), coz je WAIT, ale takto je to bezpecnejsi
                throw new JDException("Invalid state encountered.");
            }

        }
    }

    /// <summary>
    /// StateTable class
    /// </summary>
    [Serializable]
    public class StateTable : Dictionary<HashSet<Tuple<int, ETaskState>>, ECondResult>
    {
        /// <summary>
        /// Default StateTable constructor
        /// </summary>
        public StateTable()
            : base(HashSet<Tuple<int, ETaskState>>.CreateSetComparer())
        { }

        /// <summary>
        /// StateTable serialization
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            SerializationWriter sw = SerializationWriter.GetWriter();
            sw.Write(this.Count);
            foreach (var pair in this)
            {
                sw.WriteObject(pair);
            }
            sw.AddToInfo(info);
        }

        /// <summary>
        /// StateTable deserialization
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        public StateTable(SerializationInfo info, StreamingContext context)
            : base(HashSet<Tuple<int, ETaskState>>.CreateSetComparer())
        {
            SerializationReader sr = SerializationReader.GetReader(info);
            int cnt = sr.ReadInt32();
            for (int k = 0; k < cnt; k++)
            {
                var kvp = (KeyValuePair<HashSet<Tuple<int, ETaskState>>, ECondResult>)sr.ReadObject();
                this.Add(kvp.Key, kvp.Value);
            }
        }

        /// <summary>
        /// Add table to current state table
        /// </summary>
        /// <param name="table">State table</param>
        public void Add(StateTable table)
        {
            foreach (var pair in table)
            {
                this.Add(pair.Key, pair.Value);
            }
        }
    }

    /// <summary>
    /// Possible IComputeCondition results.
    /// </summary>
    public enum ECondResult
    {
        /// <summary>
        /// Condition state - Wait
        /// </summary>
        WAIT = 0,
        /// <summary>
        /// Condition state - Solve
        /// </summary>
        SOLVE = 1,
        /// <summary>
        /// Condition state - Refuse
        /// </summary>
        REFUSE = 2,
    }

    /// <summary>
    /// JDModel and IComputeCondition couple.
    /// </summary>
    [Serializable]
    public class JDModelWithCondition
    {
        /// <summary>
        /// JD model
        /// </summary>
        public JDModel mdl;
        /// <summary>
        /// Compute condition
        /// </summary>
        public IComputeCondition cond;
    }

    /// <summary>
    /// JDModel and ETaskState couple.
    /// </summary>
    [Serializable]
    public class JDModelWithState
    {
        /// <summary>
        /// JD model
        /// </summary>
        public JDModel mdl;
        /// <summary>
        /// Compute state
        /// </summary>
        public ETaskState state;
    }
}