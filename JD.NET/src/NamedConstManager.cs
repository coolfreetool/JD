using System.Collections.Generic;

namespace JDSpace
{
    /// <summary>
    /// JDModel named constants manager. It ensures named
    /// constants registration and values changing (over unique name).
    /// </summary>
    internal class NamedConstManager
    {
        /// <summary>
        /// Dictionary with named constants
        /// </summary>
        public IDictionary<string, NamedConst> NamedConsts;

        /// <summary>
        /// Named constants manager Default constructor
        /// </summary>
        public NamedConstManager()
        {
            NamedConsts = new Dictionary<string, NamedConst>();
        }

        /// <summary>
        /// Register constraint named constants to future value reloading.
        /// </summary>
        public void Register(ScConstr constr)
        {
            Register(constr.Lhs);
        }

        /// <summary>
        /// Clear duplicit values
        /// </summary>
        /// <param name="constr">Scalar constraint</param>
        public void ClearDupl(ScConstr constr)
        {
            ClearDupl(constr.Lhs);
        }

        /// <summary>
        /// Register constraints named constants to future value reloading.
        /// </summary>
        public void Register(IList<ScConstr> constrs)
        {
            foreach (ScConstr con in constrs) Register(con);
        }

        /// <summary>
        /// Clear duplicit values
        /// </summary>
        /// <param name="constrs">List of scalar constraints</param>
        public void ClearDupl(IList<ScConstr> constrs)
        {
            foreach (ScConstr con in constrs) ClearDupl(con);
        }

        /// <summary>
        /// Register named constant.
        /// </summary>
        internal void Register(NamedConst namCon)
        {
            namCon.RegisterNamedMembers(ref NamedConsts);
        }

        /// <summary>
        /// Get named constant of specific name or return null (if not provided).
        /// </summary>
        /// <param name="name">Name of constant to return.</param>
        /// <returns>Result named constant or null (if not provided).</returns>
        internal NamedConst GetNamedConst(string name)
        {
            if (NamedConsts.ContainsKey(name)) return NamedConsts[name];
            return null;
        }

        /// <summary>
        /// Register named constants of inserted scalar linear expression.
        /// </summary>
        /// <param name="expr">Scalar linear expression to register named constants in.</param>
        internal void Register(ScLinExpr expr)
        {
            if (expr is ComposedScLinExpr)
            {
                ComposedScLinExpr cExpr = expr as ComposedScLinExpr;
                cExpr.ConstantObj.RegisterNamedMembers(ref NamedConsts);
                foreach (ScTerm term in expr.Terms)
                {
                    (term as ComposedScTerm).CoeffObj2.RegisterNamedMembers(ref NamedConsts);
                }
            }
        }

        /// <summary>
        /// Clear named constant duplicities in inserted scalar linear expression.
        /// </summary>
        /// <param name="expr">Scalare linear expression to clear duplicities in.</param>
        internal void ClearDupl(ScLinExpr expr)
        {
            if (expr is ComposedScLinExpr)
            {
                ComposedScLinExpr cExpr = expr as ComposedScLinExpr;
                cExpr.ConstantObj.ClearDupl(ref NamedConsts);
                foreach (ScTerm term in expr.Terms)
                {
                    ClearDupl(term);
                }
            }
        }

        /// <summary>
        /// Clear named constant duplicities in inserted scalar term.
        /// </summary>
        /// <param name="term">Scalar term to clean duplicities in.</param>
        internal void ClearDupl(ScTerm term)
        {
            ComposedScTerm cTerm = term as ComposedScTerm;
            ComposedExtenders.ClearDupls(() => cTerm.CoeffObj2, (x) => cTerm.CoeffObj2 = x, NamedConsts);
        }

        /// <summary>
        /// Try change value of specific model named constant.
        /// </summary>
        /// <param name="name">Named constant name</param>
        /// <param name="newValue">New constant value to set</param>
        /// <returns></returns>
        public void ChangeValue(string name, double newValue)
        {
            if (NamedConsts.ContainsKey(name))
            {
                NamedConsts[name].Value = newValue;
            }
        }

        /// <summary>
        /// Add set of NamedConsts (for JDModels joining f.e.)
        /// </summary>
        /// <param name="namedConsts"></param>
        public void Join(IDictionary<string, NamedConst> namedConsts)
        {
            foreach (KeyValuePair<string, NamedConst> pair in namedConsts)
            {
                NamedConsts.Add(pair.Key, pair.Value);
            }
        }
    }
}