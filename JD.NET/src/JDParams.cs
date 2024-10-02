using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JDUtils;

namespace JDSpace
{
    /// <summary>
    /// JD parameters class
    /// </summary>
    [Serializable]
    public class JDParams
    {
        #region << INPUT & OUTPUT PARAMETERS >>
        internal Dictionary<string, int> intParams = JD.GetDefModelIntParams();
        internal Dictionary<string, double> doubleParams = JD.GetDefModelDoubleParams();
        internal Dictionary<string, string> stringParams = JD.GetDefModelStringParams();

        /// <summary>
        /// Check if parameter is set
        /// </summary>
        /// <param name="par">Parameter name</param>
        /// <returns>True if parameter is set, false if not</returns>
        public bool IsSet(string par) { return (intParams.ContainsKey(par) || doubleParams.ContainsKey(par) || stringParams.ContainsKey(par)); }
        //public bool IsSet(JD.DoubleParam par) { return doubleParams.ContainsKey(par); }
        //public bool IsSet(JD.StringParam par) { return stringParams.ContainsKey(par); }

        /// <summary>
        /// JDParams default constructor
        /// </summary>
        public JDParams()
        {
            intParams = new Dictionary<string, int>();
            doubleParams = new Dictionary<string, double>();
            stringParams = new Dictionary<string, string>();
        }

        /// <summary>
        /// Create JDParams object with preset values
        /// </summary>
        /// <param name="intPars"></param>
        /// <param name="doublePars"></param>
        /// <param name="stringPars"></param>
        public JDParams(Dictionary<string, int> intPars,
            Dictionary<string, double> doublePars,
            Dictionary<string, string> stringPars)
        {
            intParams = intPars;
            doubleParams = doublePars;
            stringParams = stringPars;
        }

        /// <summary>
        /// Get specific name parameter. Return null, if this parameter is not set.
        /// </summary>
        public object Get(string par)
        {
            if (stringParams.ContainsKey(par)) return stringParams[par];
            if (intParams.ContainsKey(par)) return intParams[par];
            if (doubleParams.ContainsKey(par)) return doubleParams[par];
            return null;
        }

        /// <summary>
        /// Get specific name parameter.
        /// </summary>
        public T Get<T>(string par)
        {
            if (typeof(T).Equals(typeof(string)))
            {
                if (stringParams.ContainsKey(par))
                {
                    return (T)Convert.ChangeType(stringParams[par], typeof(T));
                }
                else
                {
                    throw new JDException("Parameter {0} is not set!", par);
                }
            }
            else if (typeof(T).Equals(typeof(double)))
            {
                if (doubleParams.ContainsKey(par))
                {
                    return (T)Convert.ChangeType(doubleParams[par], typeof(T));
                }
                else
                {
                    throw new JDException("Parameter {0} is not set!", par);
                }
            }
            else if (typeof(T).Equals(typeof(int)))
            {
                if (intParams.ContainsKey(par))
                {
                    return (T)Convert.ChangeType(intParams[par], typeof(T));
                }
                else
                {
                    throw new JDException("Parameter {0} is not set!", par);
                }
            }            
            else
            {
                throw new JDException("No parameters of type {0}!", typeof(T).Name);
            }
        }

        //public int Get<int>(string par)
        //{
        //    if (intParams.ContainsKey(par))
        //    {
        //        return intParams[par];
        //    }
        //    else
        //    {
        //        throw new JDException("Parameter {0} is not set!", par);
        //    }
        //}

        //public double Get<double>(string par)
        //{
        //    if (doubleParams.ContainsKey(par))
        //    {
        //        return doubleParams[par];
        //    }
        //    else
        //    {
        //        throw new JDException("Parameter {0} is not set!", par);
        //    }
        //}

        /// <summary>
        /// Set string value to parameter
        /// </summary>
        /// <param name="par">Parameter name</param>
        /// <param name="value">Parameter value</param>
        public void Set(string par, string value)
        {
            // if par parameter is already set...
            if (stringParams.ContainsKey(par))
            {
                // ... change its value
                stringParams[par] = value;
            }
            else
            {
                // ... add this parameter into parameters
                stringParams.Add(par, value);
            }
        }

        /// <summary>
        /// Set int value to parameter
        /// </summary>
        /// <param name="par">Parameter name</param>
        /// <param name="value">Parameter value</param>
        public void Set(string par, int value)
        {
            // if par parameter is already set...
            if (intParams.ContainsKey(par))
            {
                // ... change its value
                intParams[par] = value;
            }
            else
            {
                // ... add this parameter into parameters
                intParams.Add(par, value);
            }
        }

        /// <summary>
        /// Set double value to parameter
        /// </summary>
        /// <param name="par">Parameter name</param>
        /// <param name="value">Parameter value</param>
        public void Set(string par, double value)
        {
            // if par parameter is already set...
            if (doubleParams.ContainsKey(par))
            {
                // ... change its value
                doubleParams[par] = value;
            }
            else
            {
                // ... add this parameter into parameters
                doubleParams.Add(par, value);
            }
        }


        ///// <summary>
        ///// Get or set specific double JDModel parameter.
        ///// </summary>
        //public double this[string par]
        //{
        //    get
        //    {
        //        if (doubleParams.ContainsKey(par))
        //        {
        //            return doubleParams[par];
        //        }
        //        else
        //        {
        //            throw new JDException("Parameter {0} is not set!", par);
        //        }
        //    }
        //    set
        //    {
        //        // if par parameter is already set...
        //        if (doubleParams.ContainsKey(par))
        //        {
        //            // ... change its value
        //            doubleParams[par] = value;
        //        }
        //        else
        //        {
        //            // ... add this parameter into parameters
        //            doubleParams.Add(par, value);
        //        }
        //    }
        //}

        ///// <summary>
        ///// Get or set specific integer JDModel parameter.
        ///// </summary>
        //public int this[string par]
        //{
        //    get
        //    {
        //        if (intParams.ContainsKey(par))
        //        {
        //            return intParams[par];
        //        }
        //        else
        //        {
        //            throw new JDException("Parameter {0} is not set!", par);
        //        }
        //    }
        //    set
        //    {
        //        // if par parameter is already set...
        //        if (intParams.ContainsKey(par))
        //        {
        //            // ... change its value
        //            intParams[par] = value;
        //        }
        //        else
        //        {
        //            // ... add this parameter into parameters
        //            intParams.Add(par, value);
        //        }
        //    }
        //}

        ///// <summary>
        ///// Get or set specific string JDModel parameter.
        ///// </summary>
        //public string this[string par]
        //{
        //    get
        //    {
        //        if (stringParams.ContainsKey(par))
        //        {
        //            return stringParams[par];
        //        }
        //        else
        //        {
        //            throw new JDException("Parameter {0} is not set!", par);
        //        }
        //    }
        //    set
        //    {
        //        // if par parameter is already set...
        //        if (stringParams.ContainsKey(par))
        //        {
        //            // ... change its value
        //            stringParams[par] = value;
        //        }
        //        else
        //        {
        //            // ... add this parameter into parameters
        //            stringParams.Add(par, value);
        //        }
        //    }
        //}

        /// <summary>
        /// Print all parameter-value couples into one list.
        /// </summary>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("INTEGER PARAMETERS:");
            foreach (KeyValuePair<string, int> pair in intParams)
            {
                sb.AppendFormat("{0,20}: {1,20}\n", pair.Key, pair.Value);
            }
            sb.AppendLine("DOUBLE PARAMETERS:");
            foreach (KeyValuePair<string, double> pair in doubleParams)
            {
                sb.AppendFormat("{0,20}: {1,20:0.0000}\n", pair.Key, pair.Value);
            }
            sb.AppendLine("STRING PARAMETERS:");
            foreach (KeyValuePair<string, string> pair in stringParams)
            {
                sb.AppendFormat("{0,20}: {1,20}\n", pair.Key, pair.Value);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Fet list of all parameters
        /// </summary>
        /// <returns>List of all parameters</returns>
        public Param[] ToParams()
        {
            List<Param> pars = new List<Param>(intParams.Count + doubleParams.Count + stringParams.Count);
            pars.AddRange(intParams.Select(x => new Param(x.Key.ToString(), x.Value)).ToList());
            pars.AddRange(doubleParams.Select(x => new Param(x.Key.ToString(), x.Value)).ToList());
            pars.AddRange(stringParams.Select(x => new Param(x.Key.ToString(), x.Value)).ToList());
            return pars.ToArray();

        }
        #endregion
    }
}