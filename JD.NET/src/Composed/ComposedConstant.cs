using System;
using System.Collections.Generic;

namespace JDSpace
{
    /// <summary>
    /// Class encapsulates composed constant (non-variable member of model), which enables constant
    /// values reloading reloading.
    /// </summary>
    [Serializable]
    public abstract class ComposedConstant
    {
        /// <summary>
        /// Get constant double representation (required for putting model to solver).
        /// </summary>
        public abstract double DoubleValue { get; }

        /// <summary>
        /// Register all NamedConst (base ComposedConstant implementation) subconstants.
        /// </summary>
        public abstract void RegisterNamedMembers(ref IDictionary<string, NamedConst> namedMembersList);

        /// <summary>
        /// Clean duplicities in inserted named constants list.
        /// </summary>
        /// <param name="namedMembersList">Named constants list to clean duplicities.</param>
        internal abstract void ClearDupl(ref IDictionary<string, NamedConst> namedMembersList);

        #region << OPERATOR OVERLOADS >>

        /// <summary>
        /// Add two composed constants
        /// </summary>
        /// <param name="r">First composed constant</param>
        /// <param name="l">Second composed constant</param>
        /// <returns>New composed constants</returns>
        public static ComposedConstant operator +(ComposedConstant r, ComposedConstant l)
        {
            ComposedConstant ret = new ComposedSum(r, l);
            return ret;
        }

        /// <summary>
        /// Substract two composed constants
        /// </summary>
        /// <param name="r">First composed constant</param>
        /// <param name="l">Second composed constant</param>
        /// <returns>New composed constants</returns>v
        public static ComposedConstant operator -(ComposedConstant r, ComposedConstant l)
        {
            ComposedConstant ret = new ComposedDec(r, l);
            return ret;
        }

        /// <summary>
        /// Multiply two composed constants
        /// </summary>
        /// <param name="r">First composed constant</param>
        /// <param name="l">Second composed constant</param>
        /// <returns>New composed constants</returns>
        public static ComposedConstant operator *(ComposedConstant r, ComposedConstant l)
        {
            ComposedConstant ret = new ComposedProduct(r, l);
            return ret;
        }

        /// <summary>
        /// Divide two composed constants
        /// </summary>
        /// <param name="r">First composed constant</param>
        /// <param name="l">Second composed constant</param>
        /// <returns>New composed constants</returns>
        public static ComposedConstant operator /(ComposedConstant r, ComposedConstant l)
        {
            ComposedConstant ret = new ComposedDiv(r, l);
            return ret;
        }

        /// <summary>
        /// Composed power
        /// </summary>
        /// <param name="r">First composed constant</param>
        /// <param name="l">Second composed constant</param>
        /// <returns>New composed constants</returns>
        public static ComposedConstant operator ^(ComposedConstant r, ComposedConstant l)
        {
            ComposedConstant ret = new ComposedPower(r, l);
            return ret;
        }

        #endregion
    }
}