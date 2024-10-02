using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JDUtils;
using System.Collections;

namespace JDSpace
{
    /// <summary>
    /// Right hand side - double array.
    /// </summary>
    internal class DoubleArrJDConstant : JdConstant
    {
        public override int Numel
        {
            get { return _val.Count; }
        }
        public override object this[int i]
        {
            get { return _val[i]; }
        }
        private IList _val;
        internal DoubleArrJDConstant(IList val, ScLinExprFactory scFactory)
            : base(scFactory)
        {
            _val = val;
        }
    }

    /// <summary>
    /// Right hand side - 2D double array.
    /// </summary>
    internal class DoubleArr2dJDConstant : JdConstant
    {
        public override int Numel
        {
            get { return _numel; }
        }
        private int _numel;
        public override object this[int i]
        {
            get
            {
                int x = i / _arr2d.GetLength(1);
                int y = i % _arr2d.GetLength(1);
                return _arr2d.GetValue(x, y);
            }
        }
        private Array _arr2d;
        internal DoubleArr2dJDConstant(Array arr2d, ScLinExprFactory scFactory)
            : base(scFactory)
        {
            _arr2d = arr2d;
            _numel = _arr2d.GetLength(0) * _arr2d.GetLength(1);
        }
    }

    /// <summary>
    /// Right hand side - jagged array.
    /// </summary>
    internal class DoubleArrJagRhs : JdConstant
    {
        public override int Numel
        {
            get { return _numel; }
        }
        private int _xSize;
        public override object this[int i]
        {
            get
            {
                int x = i / _xSize;
                int y = i % _xSize;
                return (_val[x] as IList)[y];
            }
        }
        private int _numel;
        private IList _val;
        internal DoubleArrJagRhs(IList val, ScLinExprFactory scFactory)
            : base(scFactory)
        {
            _xSize = (_val[0] as IList).Count;
            _val = val;
            _numel = _val.Count * _xSize; // suppose square jagged list.
        }
    }

    /// <summary>
    /// Right hand side - double.
    /// </summary>
    internal class NamedConstantJDConstant : JdConstant
    {
        public override int Numel
        {
            get { return 1; }
        }
        public override object this[int i]
        {
            get { return _val; }
        }
        private ComposedConstant _val;
        internal NamedConstantJDConstant(ComposedConstant val, ScLinExprFactory scFactory)
            : base(scFactory)
        {
            _val = val;
        }
    }

    /// <summary>
    /// JD temporary constraint object.
    /// </summary>
    public class JDTempConstraint
    {
        /// <summary>
        /// Constraint left-hand side member.
        /// </summary>
        public IJDComparable Lhs { get; set; }

        /// <summary>
        /// Constraint sense (JD.LESS_EQUAL, JD.EQUAL, or JD.GREATER_EQUAL).
        /// </summary>
        public char Sense { get; set; }

        /// <summary>
        /// Constraint right-hand side member.
        /// </summary>
        public IJDComparable Rhs { get; set; }

        /// <summary>
        /// Constraint name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Lazy property (0 or 1 are possible values, 0 - by default).
        /// </summary>
        public byte LazyLevel { get; set; }

        /// <summary>
        /// Create JDTempConstraint object
        /// </summary>
        /// <param name="lhs">Left hand side - IJDComparable</param>
        /// <param name="sense">Constraint sense</param>
        /// <param name="rhs">Right hand side - IJDComparable</param>
        /// <param name="name">Constraint name (null default)</param>
        /// <param name="lazyLevel">Lazy level (0 default)</param>
        public JDTempConstraint(IJDComparable lhs, char sense, IJDComparable rhs, string name = null, byte lazyLevel = 0)
        {
            Lhs = lhs;
            Sense = sense;
            Rhs = rhs;
            Name = name;
            LazyLevel = lazyLevel;
        }

        /// <summary>
        /// Set name of constraint using / operator.
        /// </summary>
        /// <param name="constr">Constraint to be edited.</param>
        /// <param name="name">Name for constraint.</param>
        /// <returns>Constraint updated with name.</returns>
        public static JDTempConstraint operator |(JDTempConstraint constr, string name)
        {
            constr.Name = name;
            return constr;
        }

        /// <summary>
        /// Set name of constraint using / operator.
        /// </summary>
        /// <param name="constr">Constraint to be edited.</param>
        /// <param name="name">Name for constraint.</param>
        /// <returns>Constraint updated with name.</returns>
        public static JDTempConstraint operator /(JDTempConstraint constr, string name)
        {            
            return constr | name;
        }        

        /// <summary>
        /// Set lazy level of constraint using / operator.
        /// </summary>
        /// <param name="constr">Constraint to be edited.</param>
        /// <param name="lazyLevel">Lazy level for constraint.</param>
        /// <returns>Constraint updated with lazy level.</returns>
        public static JDTempConstraint operator /(JDTempConstraint constr, byte lazyLevel)
        {
            constr.LazyLevel = lazyLevel;
            return constr;
        }

        #region << MULTIPLE <=, >=, == IN ONE LINE USAGE >>

        /// <summary>
        /// JDTempConstraint greater or equal operator
        /// </summary>
        /// <param name="con">JD temp constraint</param>
        /// <param name="rhs">Right hand side JDElement</param>
        /// <returns>JDTempConstraints list</returns>
        public static JDTempConstraintsList operator >=(JDTempConstraint con, JDElement rhs)
        {
            JDTempConstraintsList cons = new JDTempConstraintsList(2);
            cons.Add(con);
            JDTempConstraint con2 = new JDTempConstraint(con.Rhs, JD.GREATER_EQUAL, rhs);
            cons.Add(con2);
            return cons;
        }

        /// <summary>
        /// JDTempConstraint greater or equal operator
        /// </summary>
        /// <param name="con">JD temp constraint</param>
        /// <param name="rhs">Right hand side object</param>
        /// <returns>JDTempConstraints list</returns>
        public static JDTempConstraintsList operator >=(JDTempConstraint con, object rhs)
        {
            JDTempConstraintsList cons = new JDTempConstraintsList(2);
            cons.Add(con);
            JDTempConstraint con2 = new JDTempConstraint(con.Rhs, JD.GREATER_EQUAL, rhs.ToJDConstant(con.Lhs.GetScFactory()));
            cons.Add(con2);
            return cons;
        }

        /// <summary>
        /// JDTempConstraint lower or equal operator
        /// </summary>
        /// <param name="con">JD temp constraint</param>
        /// <param name="rhs">Right hand side JDElement</param>
        /// <returns>JDTempConstraints list</returns>
        public static JDTempConstraintsList operator <=(JDTempConstraint con, JDElement rhs)
        {
            JDTempConstraintsList cons = new JDTempConstraintsList(2);
            cons.Add(con);
            JDTempConstraint con2 = new JDTempConstraint(con.Rhs, JD.LESS_EQUAL, rhs);
            cons.Add(con2);
            return cons;
        }

        /// <summary>
        /// JDTempConstraint lower or equal operator
        /// </summary>
        /// <param name="con">JD temp constraint</param>
        /// <param name="rhs">Right hand side object</param>
        /// <returns>JDTempConstraints list</returns>
        public static JDTempConstraintsList operator <=(JDTempConstraint con, object rhs)
        {
            JDTempConstraintsList cons = new JDTempConstraintsList(2);
            cons.Add(con);
            JDTempConstraint con2 = new JDTempConstraint(con.Rhs, JD.LESS_EQUAL, rhs.ToJDConstant(con.Lhs.GetScFactory()));
            cons.Add(con2);
            return cons;
        }


        /// <summary>
        /// JDTempConstraint equality operator
        /// </summary>
        /// <param name="con">JD temp constraint</param>
        /// <param name="rhs">Right hand side JDElement</param>
        /// <returns>JDTempConstraints list</returns>
        public static JDTempConstraintsList operator ==(JDTempConstraint con, JDElement rhs)
        {
            JDTempConstraintsList cons = new JDTempConstraintsList(2);
            cons.Add(con);
            JDTempConstraint con2 = new JDTempConstraint(con.Rhs, JD.EQUAL, rhs);
            cons.Add(con2);
            return cons;
        }

        /// <summary>
        /// JDTempConstraint equality operator
        /// </summary>
        /// <param name="con">JD temp constraint</param>
        /// <param name="rhs">Right hand side object</param>
        /// <returns>JDTempConstraints list</returns>
        public static JDTempConstraintsList operator ==(JDTempConstraint con, object rhs)
        {
            JDTempConstraintsList cons = new JDTempConstraintsList(2);
            cons.Add(con);
            JDTempConstraint con2 = new JDTempConstraint(con.Rhs, JD.EQUAL, rhs.ToJDConstant(con.Lhs.GetScFactory()));
            cons.Add(con2);
            return cons;
        }

        /// <summary>
        /// JDTempConstraint inequality operator. (Deprecated, use <=, >= or == instead) 
        /// </summary>
        /// <param name="con">JD temp constraint</param>
        /// <param name="rhs">Right hand side JDElement</param>
        /// <returns>JDTempConstraints list</returns>
        public static JDTempConstraintsList operator !=(JDTempConstraint con, JDElement rhs)
        {
            throw new JDException("Bad operator '!=' used in constraint. Use <=, >= or ==.");
        }

        /// <summary>
        /// JDTempConstraint inequality operator. (Deprecated, use <=, >= or == instead) 
        /// </summary>
        /// <param name="con">JD temp constraint</param>
        /// <param name="rhs">Right hand side object</param>
        /// <returns>JDTempConstraints list</returns>
        public static JDTempConstraintsList operator !=(JDTempConstraint con, object rhs)
        {
            throw new JDException("Bad operator '!=' used in constraint. Use <=, >= or ==.");
        }

        #endregion << MULTIPLE <=, >=, == IN ONE LINE USAGE >>
    }

    /// <summary>
    /// JD temp constraints list class
    /// </summary>
    public class JDTempConstraintsList
    {
        /// <summary>
        /// JDTempConstraint list
        /// </summary>
        public List<JDTempConstraint> Cons;
        /// <summary>
        /// Last element of JDTempConstraintsList
        /// </summary>
        public JDTempConstraint Last { get { return Cons[Cons.Count - 1]; } }
        /// <summary>
        /// Initialize JDTempConstraintList with default capacity
        /// </summary>
        /// <param name="cap"></param>
        public JDTempConstraintsList(int cap)
        {
            Cons = new List<JDTempConstraint>(cap);
        }
        /// <summary>
        /// Add JDTempConstraint to the JDTempConstraintsList
        /// </summary>
        /// <param name="con"></param>
        public void Add(JDTempConstraint con)
        {
            Cons.Add(con);
        }

        /// <summary>
        /// JDTempConstraintsList greater or equal operator
        /// </summary>
        /// <param name="cons">JD temp constraints list</param>
        /// <param name="rhs">Right hand side JDElement</param>
        /// <returns>JDTempConstraints list</returns>
        public static JDTempConstraintsList operator >=(JDTempConstraintsList cons, JDElement rhs)
        {
            JDTempConstraint con = cons.Last;
            JDTempConstraint con2 = new JDTempConstraint(con.Rhs, JD.GREATER_EQUAL, rhs);
            cons.Add(con2);
            return cons;
        }

        /// <summary>
        /// JDTempConstraintsList greater or equal operator
        /// </summary>
        /// <param name="cons">JD temp constraints list</param>
        /// <param name="rhs">Right hand side object</param>
        /// <returns>JDTempConstraints list</returns>
        public static JDTempConstraintsList operator >=(JDTempConstraintsList cons, object rhs)
        {
            JDTempConstraint con = cons.Last;
            JDTempConstraint con2 = new JDTempConstraint(con.Rhs, JD.GREATER_EQUAL, rhs.ToJDConstant(con.Lhs.GetScFactory()));
            cons.Add(con2);
            return cons;
        }

        /// <summary>
        /// JDTempConstraintsList lower or equal operator
        /// </summary>
        /// <param name="cons">JD temp constraints list</param>
        /// <param name="rhs">Right hand side JDElement</param>
        /// <returns>JDTempConstraints list</returns>
        public static JDTempConstraintsList operator <=(JDTempConstraintsList cons, JDElement rhs)
        {
            JDTempConstraint con = cons.Last;
            JDTempConstraint con2 = new JDTempConstraint(con.Rhs, JD.LESS_EQUAL, rhs);
            cons.Add(con2);
            return cons;
        }

        /// <summary>
        /// JDTempConstraintsList lower or equal operator
        /// </summary>
        /// <param name="cons">JD temp constraints list</param>
        /// <param name="rhs">Right hand side object</param>
        /// <returns>JDTempConstraints list</returns>
        public static JDTempConstraintsList operator <=(JDTempConstraintsList cons, object rhs)
        {
            JDTempConstraint con = cons.Last;
            JDTempConstraint con2 = new JDTempConstraint(con.Rhs, JD.LESS_EQUAL, rhs.ToJDConstant(con.Lhs.GetScFactory()));
            cons.Add(con2);
            return cons;
        }

        /// <summary>
        /// JDTempConstraintsList equality operator
        /// </summary>
        /// <param name="cons">JD temp constraints list</param>
        /// <param name="rhs">Right hand side JDElement</param>
        /// <returns>JDTempConstraints list</returns>
        public static JDTempConstraintsList operator ==(JDTempConstraintsList cons, JDElement rhs)
        {
            JDTempConstraint con = cons.Last;
            JDTempConstraint con2 = new JDTempConstraint(con.Rhs, JD.EQUAL, rhs);
            cons.Add(con2);
            return cons;
        }

        /// <summary>
        /// JDTempConstraintsList equality operator
        /// </summary>
        /// <param name="cons">JD temp constraints list</param>
        /// <param name="rhs">Right hand side object</param>
        /// <returns>JDTempConstraints list</returns>
        public static JDTempConstraintsList operator ==(JDTempConstraintsList cons, object rhs)
        {
            JDTempConstraint con = cons.Last;
            JDTempConstraint con2 = new JDTempConstraint(con.Rhs, JD.EQUAL, rhs.ToJDConstant(con.Lhs.GetScFactory()));
            cons.Add(con2);
            return cons;
        }

        /// <summary>
        /// JDTempConstraintsList inequality operator. (Deprecated, use <=, >= or == instead) 
        /// </summary>
        /// <param name="cons">JD temp constraints list</param>
        /// <param name="rhs">Right hand side JDElement</param>
        /// <returns>JDTempConstraints list</returns>
        public static JDTempConstraintsList operator !=(JDTempConstraintsList cons, JDElement rhs)
        {
            throw new JDException("Bad operator '!=' used in constraint. Use <=, >= or ==.");
        }

        /// <summary>
        /// JDTempConstraintsList inequality operator. (Deprecated, use <=, >= or == instead) 
        /// </summary>
        /// <param name="cons">JD temp constraints list</param>
        /// <param name="rhs">Right hand side object</param>
        /// <returns>JDTempConstraints list</returns>
        public static JDTempConstraintsList operator !=(JDTempConstraintsList cons, object rhs)
        {
            throw new JDException("Bad operator '!=' used in constraint. Use <=, >= or ==.");
        }

    }
}
