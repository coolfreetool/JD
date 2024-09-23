using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using JDUtils;
using System.Reflection;
using System.IO;
using System.Collections;

namespace JDSpace
{
    /// <summary>
    /// Class for various JD constants.
    /// </summary>
    public class JD
    {
        #region << JD constants >>
        /// <summary>
        /// JD less equal char.
        /// </summary>
        public const char LESS_EQUAL = '<';

        /// <summary>
        /// JD greater equal char.
        /// </summary>
        public const char GREATER_EQUAL = '>';

        /// <summary>
        /// JD equality char.
        /// </summary>
        public const char EQUAL = '=';

        /// <summary>
        /// JD continuous char.
        /// </summary>
        public const char CONTINUOUS = 'c';

        /// <summary>
        /// JD binary char.
        /// </summary>
        public const char BINARY = 'b';

        /// <summary>
        /// JD integer char.
        /// </summary>
        public const char INTEGER = 'i';

        /// <summary>
        /// JD infinity double.
        /// </summary>
        public const double INFINITY = double.MaxValue;

        /// <summary>
        /// JD maximize int.
        /// </summary>
        public const int MAXIMIZE = 1;

        /// <summary>
        /// JD minimize int.
        /// </summary>
        public const int MINIMIZE = -1;

        /// <summary>
        /// JD *.mps file type string.
        /// </summary>
        public const string MPS = "mps";


        /// <summary>
        /// JD *.lp file type string.
        /// </summary>
        public const string LP = "lp";
        #endregion

        #region << LOG MESSAGES >>
        /// <summary>
        /// JD log message - Putting model to solver
        /// </summary>
        public const string MSG_PUTTING_MODEL = "Putting model to solver";
        /// <summary>
        /// JD log message - Putting variables to solver
        /// </summary>
        public const string MSG_PUTTING_VARS = "Putting variables to solver";
        /// <summary>
        /// JD log message - Model updating
        /// </summary>
        public const string MSG_MODEL_UPDATING = "Model updating";
        /// <summary>
        /// JD log message - Model solving
        /// </summary>
        public const string MSG_MODEL_SOLVING = "Model solving";
        /// <summary>
        /// JD log message - Putting constraints to solver
        /// </summary>
        public const string MSG_PUTTING_CONSTRS = "Putting constraints to solver";
        /// <summary>
        /// JD log message - Putting objective function to solver
        /// </summary>
        public const string MSG_PUTTING_OBJ_FUN = "Putting obj. fun. to solver";
        /// <summary>
        /// JD log message - Optimizing
        /// </summary>
        public const string MSG_OPTIMIZING = "Optimizing";
        /// <summary>
        /// JD log message - Parsing results
        /// </summary>
        public const string MSG_RESULTS_PARSING = "Results parsing";
        /// <summary>
        /// JD log message - Time in seconds
        /// </summary>
        public const string PARAM_TIME = "time[s]";
        /// <summary>
        /// JD log message - Model
        /// </summary>
        public const string PARAM_MODEL = "model";
        #endregion

        #region << JD params >>

        #region << PARAMS DEFINITIONS >>
        /// <summary>
        /// JDModel double parameters.
        /// </summary>
        public class DoubleParam
        {

            /// <summary>
            /// Optimality gap (input).
            /// Value (0 - 1) is allowed.
            /// </summary>
            public const string MIP_GAP = "MIP_GAP";

            /// <summary>
            /// Max. solver time in seconds (input).
            /// </summary>
            public const string TIME_LIMIT = "TIME_LIMIT";

            /// <summary>
            /// Current solution obj. fun. value (output).
            /// </summary>
            public const string OBJ_VALUE = "OBJ_VALUE";

            /// <summary>
            /// Sover heuristics (input).
            /// </summary>
            public const string HEURISTICS = "HEURISTICS";

            /// <summary>
            /// Current solution solver time (output).
            /// </summary>
            public const string SOLVER_TIME = "SOLVER_TIME";

            /// <summary>
            /// Current solution presolve time (output).
            /// </summary>
            public const string PRESOLVE_TIME = "PRESOLVE_TIME";

            /// <summary>
            /// Estimated relative proximity of the solution to the optmimal solution.
            /// </summary>
            public const string MIP_GAP_REACHED = "MIP_GAP_REACHED";

            /// <summary>
            /// Required absolute proximity to the optmimal solution. The optimization process is terminated when this values is reached.
            /// </summary>
            public const string MIP_GAP_ABS = "MIP_GAP_ABS";

            /// <summary>
            /// Estimated absolute proximity of the solution to the optmimal solution.
            /// </summary>
            public const string MIP_GAP_ABS_REACHED = "MIP_GAP_ABS_REACHED";

            /// <summary>
            /// After this time is the MIPfocus strategy switched to bounds tightening.
            /// </summary>
            public const string FOCUS_TO_BOUND_TIME = "FOCUS_TO_BOUND_TIME";
        }

        /// <summary>
        /// JDModel double parameters.
        /// </summary>
        public class StringParam
        {
            /// <summary>
            /// Current solution solver name  (output).
            /// </summary>
            public const string SOLVER_NAME = "SOLVER_NAME";

            /// <summary>
            /// Solver log file path (input).
            /// </summary>
            public const string LOG_FILE = "LOG_FILE";

            /// <summary>
            /// Solver dependent status label (output).
            /// </summary>
            public const string STATUS = "STATUS";

            /// <summary>
            /// Path for solver exported model file (input).
            /// </summary>
            public const string WRITE_TO_FILE = "WRITE_TO_FILE";
        }

        /// <summary>
        /// JDModel integer parameters.
        /// </summary>
        public class IntParam
        {
            /// <summary>
            /// Current solution result status (output).
            /// 0: unsolved (unfeasible, unbounded).
            /// 1: solved successfully.
            /// </summary>
            public const string RESULT_STATUS = "RESULT_STATUS";

            /// <summary>
            /// Next solution solver output flag (input).
            /// </summary>
            public const string OUT_FLAG = "OUT_FLAG";

            /// <summary>
            /// Branching priority (input).
            /// </summary>
            public const string BRANCH_PRIORITY = "BRANCH_PRIORITY";

            /// <summary>
            /// The number of logical processors used by solver (input).
            /// </summary>
            public const string THREADS = "THREADS";

            /// <summary>
            /// Choose simplex pricing norm. 
            /// -1 - automatic choose
            /// 0,1,2,3 : different norms
            /// </summary>
            public const string NORM_ADJUST = "NORM_ADJUST";

            /// <summary>
            /// Reduce the number of nonzero values in the presolved model.
            /// 1: on
            /// 2: off
            /// </summary>
            public const string PRESPARSIFY = "PRESPARSIFY";

            /// <summary>
            /// Mmdify high-level solution strategy of GUROBI
            /// 0: Default = no focus
            /// 1: Focus on finding feasible solutions
            /// 2: Focus on proving optimality
            /// 3: Focus on bound tightening
            /// </summary>
            public const string MIPFOCUS = "MIPFOCUS";

            /// <summary>
            /// Allow adding lazy constraints during gurobi grbCallback (0-Disable, 1-Allow)
            /// </summary>
            public const string LAZY_CONSTRAINTS = "LAZY_CONSTRAINTS";

            /// <summary>
            /// Change binary variables to continuous with bounds [0,1]
            /// 0: binary variables are binary.
            /// 1: binary variables are continuous.
            /// </summary>
            public const string RELAX_BIN_VARIABLES = "RELAX_BIN_VARIABLES";

            /// <summary>
            /// Algorithm used to solve continuous models [-1,4]
            /// -1: automatic,
            ///  0: primal simplex
            ///  1: dual simplex
            ///  2: barrier
            ///  3: concurrent
            ///  4: deterministic concurrent
            /// </summary>
            public const string LP_METHOD = "LP_METHOD"; 
        }
        #endregion

        #region << DEFAULT PARAMS VALUES
        internal static Dictionary<string, int> GetDefModelIntParams()
        {
            // add parameter with default value
            return new Dictionary<string, int>()
            {
                {JD.IntParam.OUT_FLAG, 1} // output flag is by default on
            };
        }

        internal static Dictionary<string, double> GetDefModelDoubleParams()
        {
            // add parameter with default value
            return new Dictionary<string, double>()
            {
            };
        }

        internal static Dictionary<string, string> GetDefModelStringParams()
        {
            // add parameter with default value
            return new Dictionary<string, string>()
            {
            };
        }
        #endregion
        #endregion

        /// <summary>
        /// Search available IJDSolver implementations - dll files in ".\JDSolverPlugins".
        /// </summary>
        public static List<IJDSolver> GetAvailableSolvers(string solversFolderPath = @".\JDSolverPlugins")
        {
            List<IJDSolver> solvers = new List<IJDSolver>();
            if (!Directory.Exists(solversFolderPath))
            {
                Console.WriteLine("Can not find folder '{0}'!", solversFolderPath);
                return solvers;
            }
            //Go through all the files in the plugin directory
            foreach (string fileOn in Directory.GetFiles(solversFolderPath))
            {
                FileInfo file = new FileInfo(fileOn);
                //Preliminary check, must be .dll
                string fileSufix = file.Name.Substring(file.Name.Length - 6);

                //if (fileSufix.Equals("JD.dll"))
                if (file.Extension.Equals(".dll"))
                {
                    //Add the 'plugin'
                    List<IJDSolver> assemblySolvers = _getAssemblyJDSolvers(fileOn);
                    if (assemblySolvers.Count > 0)
                    {
                        foreach (IJDSolver solver in assemblySolvers)
                        {
                            solvers.Add(solver);
                            Console.WriteLine("{0} pluged-in", solver.GetType().Name);
                        }
                    }
                }
            }
            return solvers;
        }

        /// <summary>
        /// Create IJDSolver instances from "FileName" assembly.
        /// </summary>
        private static List<IJDSolver> _getAssemblyJDSolvers(string FileName)
        {
            //Create a new assembly from the plugin file we're adding..
            List<IJDSolver> solvers = new List<IJDSolver>();
            Assembly pluginAssembly = null;
            try
            {
                pluginAssembly = Assembly.LoadFrom(FileName);
            }
            catch (Exception)
            {
                return solvers;
            }

            //Next we'll loop through all the Types found in the assembly
            foreach (Type pluginType in pluginAssembly.GetTypes())
            {
                if (pluginType.IsPublic) //Only look at public types
                {
                    if (!pluginType.IsAbstract)  //Only look at non-abstract types
                    {
                        //Gets a type object of the interface we need the plugins to match
                        Type typeInterface = pluginType.GetInterface("JDSpace.IJDSolver", true);

                        //Make sure the interface we want to use actually exists
                        if (typeInterface != null)
                        {
                            //Create a new instance and store the instance in the collection for later use
                            //We could change this later on to not load an instance.. we have 2 options
                            //1- Make one instance, and use it whenever we need it.. it's always there
                            //2- Don't make an instance, and instead make an instance whenever we use it, then close it
                            //For now we'll just make an instance of all the plugins
                            IJDSolver solver;
                            try
                            {
                                solver = (IJDSolver)Activator.CreateInstance(pluginAssembly.GetType(pluginType.ToString()));
                                if (solver != null) solvers.Add(solver);
                            }
                            catch (Exception ex)
                            {
                                if (!ex.InnerException.Message.Contains("License") && !ex.InnerException.Message.Contains("license"))
                                {
                                    throw;
                                }
                            }
                        }
                        typeInterface = null; //Mr. Clean			
                    }
                }
            }
            pluginAssembly = null; //more cleanup
            return solvers;
        }


        /// <summary>
        /// Create two-dimensional double array wiht zero elements.
        /// </summary>
        /// <param name="xSize">First dimension size.</param>
        /// <param name="ySize">Second dimension size.</param>
        /// <returns>New double array.</returns>
        public static double[,] Zeros(int xSize, int ySize)
        {
            return ConstantMatrix(0.0, xSize, ySize);
        }

        /// <summary>
        /// Create two-dimensional double array wiht "number" elements.
        /// </summary>
        /// <param name="xSize">First dimension size.</param>
        /// <param name="ySize">Second dimension size.</param>
        /// <param name="value">Matrix members value.</param>
        /// <returns>New double array.</returns>
        public static T[,] ConstantMatrix<T>(T value, int xSize, int ySize)
        {
            T[,] arr = new T[xSize, ySize];
            for (int i = 0; i < arr.GetLength(0); i++)
            {
                for (int y = 0; y < arr.GetLength(1); y++)
                {
                    arr[i, y] = value;
                }
            }
            return arr;
        }

        /// <summary>
        /// Create two-dimensional double array wiht "number" elements.
        /// </summary>
        /// <param name="xSize">First dimension size.</param>
        /// <param name="ySize">Second dimension size.</param>
        /// <param name="number">Matrix members value.</param>
        /// <returns>New double array.</returns>
        public static double[,] ConstantMatrixOld<T>(double number, int xSize, int ySize)
        {
            return ConstantMatrix(number, xSize, ySize); // removed /unsafe code usage, kept for backw. compatibility
        }

        /// <summary>
        /// Create two-dimensional double array wiht "number" elements.
        /// </summary>
        /// <param name="xSize">First dimension size.</param>
        /// <param name="ySize">Second dimension size.</param>
        /// <param name="number">Matrix members value.</param>
        /// <returns>New double array.</returns>
        public static double[][] ConstantArrs(double number, int xSize, int ySize)
        {
            double[][] arrs = new double[xSize][];
            for (int x = 0; x < xSize; x++)
            {
                arrs[x] = new double[ySize];
                for (int y = 0; y < ySize; y++)
                {
                    arrs[x][y] = number;
                }
            }
            return arrs;
        }

        /// <summary>
        /// Return list with predefined number of variables with the same value 
        /// </summary>
        /// <param name="count">List size</param>
        /// <param name="value">Variable value</param>
        /// <returns></returns>
        public static List<double> ConstantList(int count, double value)
        {
            List<double> list = new List<double>(count);
            for (int i = 0; i < count; i++)
            {
                list.Add(value);
            }
            return list;
        }

        /// <summary>
        /// Create matrix (two-dim. double array) transposition.
        /// </summary>
        /// <param name="mat">Original matrix.</param>
        /// <returns>New matrix (two-dim. double array).</returns>
        public static double[,] Transpose(double[,] mat)
        {
            double[,] tMat = new double[mat.GetLength(1), mat.GetLength(0)];
            for (int x = 0; x < mat.GetLength(0); x++)
            {
                for (int y = 0; y < mat.GetLength(1); y++)
                {
                    tMat[y, x] = mat[x, y];
                }
            }
            return tMat;
        }

        /// <summary>
        /// Create matrix (two-dim. double array) transposition.
        /// </summary>
        /// <param name="mat">Original matrix.</param>
        /// <returns>New matrix (two-dim. double array).</returns>
        public static double[][] Transpose(double[][] mat)
        {
            double[][] tMat = new double[mat[0].Length][];
            for (int y = 0; y < mat[0].Length; y++)
            {
                tMat[y] = new double[mat.Length];
                for (int x = 0; x < mat.Length; x++)
                {
                    tMat[y][x] = mat[x][y];
                }
            }
            return tMat;
        }

        #region TESTS

        ///// <summary>
        ///// Performs JDModel implementation tests.
        ///// </summary>
        ///// <param name="model">Tested JDModel instance.</param>
        //public static void JDTestsRun(JDModel model)
        //{
        //    TestTwoDimVarsIndexing(model);
        //    model.Reset();
        //    TestSum(model);
        //    model.Reset();
        //    TestSOS1(model);
        //    model.Reset();
        //    TestSOS2(model);
        //    model.Reset();
        //    TestTranspose(model);
        //    model.Reset();
        //    TestTerms(model);
        //}

        ///// <summary>
        ///// Test of basic model properties.
        ///// </summary>
        ///// <param name="model">Tested JDModel instance.</param>
        //public static void TestSimple(JDModel model)
        //{
        //    JDVar x = model.AddVar(2, 2, -500, 500, JD.CONTINUOUS);
        //    model.Update();
        //    model += (2*x[0,1,0, 0] <= 210);

        //    model.SetObjective(x.Sum(), JD.MAXIMIZE);
        //    Console.WriteLine("Solver: " + model.SolverName);
        //    if (model.Optimize())
        //    {
        //        Console.WriteLine("Simple test");
        //        Console.WriteLine("x:");
        //        x.Print();
        //    }
        //}

        ///// <summary>
        ///// Test matrix variables indexing.
        ///// </summary>
        ///// <param name="model">Tested JDModel instance.</param>
        //public static void TestTwoDimVarsIndexing(JDModel model)
        //{
        //    JDVar x = model.AddVar(4, 5, -500, 500, JD.CONTINUOUS);
        //    model.Update();
        //    double[,] c = {{ 1, 2, 3 },
        //                       { 4, 5, 6 }};
        //    model += (1 * x[0, 1, 1, 3] <= c); // -> prvky 0 - 1 radku, 1 - 3 sloupce se jsou mensi nebo rovny matici c
        //    model += (1 * x[2, 4] == 24); // -> prvek na pozici 2,4 je roven 24
        //    model += (x[2, 3, 1, 2] <= 2); // -> prvky 2 - 3 radku, 1 - 2 sloupce jsou mensi nebo rovny 2
        //    model += (x[1, 0] + x[2, 0] == 0); // -> soucet prvku na pozici [1,0] a [2,0] je roven 0
        //    model += (2 * x[1, 0] == x[2, 0]); // -> prvek na pozici [2, 0] je dvojnasobkem prvku na pozici [1, 0]

        //    model.SetObjective(x.Sum(), JD.MAXIMIZE);
        //    Console.WriteLine("Solver: " + model.SolverName);
        //    if (model.Optimize())
        //    {
        //        Console.WriteLine("Two-dim. indexing test");
        //        Console.WriteLine("x:");
        //        x.Print();
        //    }            
        //}

        ///// <summary>
        ///// Test optimization variable matrix sum usage.
        ///// </summary>
        ///// <param name="model">Tested JDModel instance.</param>
        //public static void TestSum(JDModel model)
        //{
        //    JDVar x = model.AddVar(2, 3, -50, 50, JD.CONTINUOUS);
        //    model.Update();
        //    double[,] c1 = { { 30, 30, 30 } };
        //    double[,] c2 = { { 1, 2, 3 } };

        //    JDLinExpr colSum = x.Sum(0);
        //    model += (colSum == c1);
        //    model += (x[1, 1, 0, 2] == c2);

        //    model.SetObjective(x.Sum(), JD.MAXIMIZE);
        //    Console.WriteLine("Solver: " + model.SolverName);
        //    if (model.Optimize())
        //    {
        //        Console.WriteLine("x:");
        //        x.Print();
        //    }
        //}

        ///// <summary>
        ///// Test of SOS 1.
        ///// </summary>
        ///// <param name="model">Tested JDModel instance.</param>
        //public static void TestSOS1(JDModel model)
        //{
        //    JDVar x = model.AddVar(4, 1, -500, 500, JD.CONTINUOUS);            
        //    model.Update();
        //    double[] weights = { 1, 2, 3, 4 };
        //    model.AddSOS1(x, weights);
        //    model.SetObjective(x.Sum(), JD.MAXIMIZE);
        //    Console.WriteLine("Solver: " + model.SolverName);
        //    if (model.Optimize())
        //    {
        //        Console.WriteLine("SOS 1 test");
        //        Console.WriteLine("x:");
        //        x.Print();
        //    }
        //}

        ///// <summary>
        ///// Test of SOS 2.
        ///// </summary>
        ///// <param name="model">Tested JDModel instance.</param>
        //public static void TestSOS2(JDModel model)
        //{
        //    JDVar x = model.AddVar(4, 1, -500, 500, JD.CONTINUOUS);
        //    model.Update();
        //    double[] weights = { 1, 2, 3, 4 };
        //    model.AddSOS2(x, weights);
        //    model.SetObjective(x.Sum(), JD.MAXIMIZE);
        //    Console.WriteLine("Solver: " + model.SolverName);
        //    if (model.Optimize())
        //    {
        //        Console.WriteLine("SOS 2 test");
        //        Console.WriteLine("x:");
        //        x.Print();
        //    }
        //}

        ///// <summary>
        ///// Test optim. variable matrix transposition.
        ///// </summary>
        ///// <param name="model"></param>
        //public static void TestTranspose(JDModel model)
        //{
        //    JDVar x = model.AddVar(3, 1, 0, 10, JD.CONTINUOUS);
        //    double[,] vec = new double[1, 3] { { 1, 2, 3 } };
        //    model.Update();
        //    model += (x >= 3);
        //    model += (vec * x <= 100);
        //    model.SetObjective(x.Sum(), JD.MAXIMIZE);
        //    if (model.Optimize())
        //    {
        //        Console.WriteLine("x = ");
        //        x.Transpose().Print();
        //        Console.WriteLine("vec * x = ");
        //        (x.Transpose() * JD.Transpose(vec)).Print();
        //    }
        //}

        ///// <summary>
        ///// Test by members matrix multiplying.
        ///// </summary>
        ///// <param name="model">Tested JDModel instance.</param>
        //public static void TestTerms(JDModel model)
        //{
        //    JDVar x = model.AddVar(2, 3, 0, 100, JD.CONTINUOUS);
        //    double[,] mat = new double[2, 3] { { 1, 2, 3 }, { 4, 5, 6 } };
        //    model.Update();
        //    model += x.Term(mat) == 60;
        //    model.SetObjective(x.Sum(), JD.MAXIMIZE);
        //    if (model.Optimize())
        //    {
        //        Console.WriteLine("x = ");
        //        x.Print();
        //        Console.WriteLine("x.*mat = ");
        //        (x.Term(mat)).Print();
        //    }
        //}

        /// <summary>
        /// Show size of available memory.
        /// </summary>
        public static void ShowAvailableRAM()
        {
            PerformanceCounter ramCounter;
            ramCounter = new PerformanceCounter("Memory", "Available MBytes");
            float avaiMem = ramCounter.NextValue();
            Console.WriteLine("Available RAM (MB): " + avaiMem);
        }

        #endregion TESTS
    }

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

    /// <summary>
    /// JD extenders
    /// </summary>
    public static class JDExtenders
    {
        /// <summary>
        /// To string method that returns "null" when object is null
        /// </summary>
        /// <param name="t">Object</param>
        /// <returns>String</returns>
        public static string ToString2(this double? t)
        {
            if (t == null)
            {
                return "null";
            }
            return t.ToString();
        }

        /// <summary>
        /// Creates JDConstant from given object using SsLinExprFactory 
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="scFactory">Scalar linear expression factory</param>
        /// <returns>JD constant</returns>
        internal static JDConstant ToJDConstant(this object obj, ScLinExprFactory scFactory)
        {
            int xSize, ySize;
            object unwr = null;
            if (obj.IsScalar(out unwr, out xSize, out ySize))
            {
                return new ObjectJDConstant(unwr, scFactory); ;
            }
            else if (obj is Array)
            {
                Array arr = obj as Array;
                int rank = arr.Rank;
                if (rank == 2) return new DoubleArr2dJDConstant(arr, scFactory);
            }
            if (obj is IList)
            {
                IList list = obj as IList;
                if (list[0] is IList) return new DoubleArrJagRhs(list, scFactory);
                return new DoubleArrJDConstant(list, scFactory);
            }
            throw new JDException("JDConstant conversion unsupported for {0}!", obj.GetType().Name);
        }

        /// <summary>
        /// Convert object to ComposedConstant.
        /// </summary>
        internal static ComposedConstant ToComposedConstant(this object obj)
        {
            if (obj is ComposedConstant) return obj as ComposedConstant;
            return new NamedConst(obj);
        }

        internal static object GetJaggXYMember(this IList jagList, int x, int y)
        {
            return (jagList[x] as IList)[y];
        }

        internal static object Get2DArrXYMember(this IList List2DArr, int x, int y)
        {
            return (List2DArr as Array).GetValue(x, y);
        }

        internal static object GetSimpleListColumnVectorXYMember(this IList List2DArr, int x, int y)
        {
            if (y == 0)
            {
                return List2DArr[x];
            }
            throw new JDException("Second coordinate must be zero for simple list two-dimensional indexing as column vector! Inserted indices ({0},{1})!", x, y);
        }

        internal static object GetSimpleListRowVectorXYMember(this IList List2DArr, int x, int y)
        {
            if (x == 0)
            {
                return List2DArr[y];
            }
            throw new JDException("First coordinate must be zero for simple list two-dimensional indexing as row vector! Inserted indices ({0},{1})!", x, y);
        }

        internal static double ToDouble(this object obj)
        {
            double d;
            if (obj is ComposedConstant) return (obj as ComposedConstant).DoubleValue;
            return d = Convert.ToDouble(obj);
        }

        internal static Func<int, int, object> InitXYGetter(this IList list)
        {
            if (list.IsJagged())
                return list.GetJaggXYMember;
            return list.Get2DArrXYMember;
        }

        internal static void GetSize(this object obj, out int xSize, out int ySize)
        {
            if (obj is IList)
            {
                (obj as IList)._getSize(out xSize, out ySize);
                return;
            }
            xSize = 1;
            ySize = 1;
        }

        internal static bool Is2D(this IList list)
        {
            if (list is Array)
            {
                Array arr = list as Array;
                if (arr.Rank == 2) return true;
            }
            if (list[0] is IList) return true;
            return false;
        }

        internal static bool IsJagged(this IList list)
        {
            if (list is Array)
            {
                Array arr = list as Array;
                if (arr.Rank == 2) return false;
            }
            if (list[0] is IList) return true;
            return false;
        }

        private static void _getSize(this IList list, out int xSize, out int ySize)
        {
            if (list is Array)
            {
                Array arr = list as Array;
                if (arr.Rank == 2)
                {
                    xSize = arr.GetLength(0);
                    ySize = arr.GetLength(1);
                    return;
                }
            }
            xSize = list.Count;
            ySize = 1;
            if (list[0] is IList)
            {
                ySize = (list[0] as IList).Count;
            }
        }

        internal static IList TransposeSquare(this IList toTranspose)
        {
            int xSize, ySize;
            toTranspose.GetSize(out xSize, out ySize);
            Func<int, int, object> xyGetter = toTranspose.InitXYGetter();
            object[,] tList = new object[ySize, xSize];
            for (int ix = 0; ix < xSize; ix++)
            {
                for (int iy = 0; iy < ySize; iy++)
                {
                    tList[iy, ix] = xyGetter(ix, iy);
                }
            }
            return tList;
        }

        internal static bool IsScalar(this object obj, out object unWrapedScalar, out int xSize, out int ySize)
        {
            obj.GetSize(out xSize, out ySize);
            if ((xSize == 1) && (ySize == 1))
            {
                if (obj is IList)
                {
                    IList list = obj as IList;
                    if (list.Is2D())
                    {
                        if (list.IsJagged()) { unWrapedScalar = list.GetJaggXYMember(0, 0); }
                        else { unWrapedScalar = list.Get2DArrXYMember(0, 0); }
                        return true;
                    }
                    else
                    {
                        unWrapedScalar = list[0];
                        return true;
                    }
                }
                unWrapedScalar = obj;
                return true;
            }
            unWrapedScalar = null;
            return false;
        }

        internal static IList<T> Transpose<T>(this IList<T> toTranspose, int xSize, int ySize)
        {
            T[] tList = new T[toTranspose.Count];
            int x, y, i1, i2;
            for (int i = 0; i < toTranspose.Count; i++)
            {
                y = i % ySize;
                x = i / ySize;
                i1 = x * ySize + y;
                i2 = y * xSize + x;
                tList[i2] = toTranspose[i1];
            }
            return tList;
        }
    }
}
