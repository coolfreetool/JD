using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JDSpace;
using System.Diagnostics;
using JDUtils;
using NUnitLite;
using OTJD;

namespace JDSpace
{
    /// <summary>
    /// JD unit tests "root" class.
    /// </summary>
    public static class JDTester
    {
        // currently tested solver
        internal static IJDSolver _solver;

        public static void ResetSolver() {
            string solverVar = Environment.GetEnvironmentVariable("SOLVER");
            switch (solverVar)
            {
                case "CBC":
                    _solver = new CbcJDSolver(); // tested - it works
                    break;
                case "SCIP":
                    _solver = new ScipJDSolver(); // tested - it works
                    break;
                case "Glpk":
                    _solver = new GlpkJDSolver(); // doesn't work
                    break;
                case "SAT":
                    _solver = new SatJDSolver(); // tested - it works partially
                    break;
                default:
                    throw new JDException("Unknown solver {0}", solverVar);
            }
        }

        /// <summary>
        /// Perform unit tests proofing solver usability.
        /// </summary>
        /// <param name="solver">Tested solver</param>
        public static void TestSolver(IJDSolver solver)
        {
            _solver = solver;
            #region << EDITABLE PART - INSERT TEST CLASSES >>
            new AutoRun().Execute(new string[]{});
            #endregion << EDITABLE PART - INSERT TEST CLASSES >>
        }

        /// <summary>
        /// Tests inserted solvers using JDTester and return zero-faild solvers list only.
        /// </summary>
        /// <param name="solvers">Tested solvers.</param>
        /// <returns>Solvers verified with JDTester.</returns>
        public static List<IJDSolver> GetVerifiedSolvers(List<IJDSolver> solvers)
        {
            List<IJDSolver> verSolvers = new List<IJDSolver>();
            // foreach (IJDSolver solver in solvers)
            // {
            //     JDTester.TestSolver(solver);
            //     if (results.NumberOfFails == 0)
            //     {
            //         solver.Reset();
            //         verSolvers.Add(solver);
            //     }
            //     verSolvers.Add(solver);
            // }
            return verSolvers;
        }

        /// <summary>
        /// Let user choose solver over console interface (prints available solvers list).
        /// </summary>
        // public static IJDSolver ManuallyChooseSolver(List<IJDSolver> solvers, bool verifyAtFirst = false)
        // {
        //     // test results list - not used, when verifyAtFirst == false
        //     List<TestResults> testResults = new List<TestResults>();
        //     if (verifyAtFirst)
        //     {
        //         // preform solvers testing
        //         foreach (IJDSolver solver in solvers)
        //         {
        //             TestResults result = JDTester.TestSolver(solver);
        //             solver.Reset();
        //             testResults.Add(result);
        //         }
        //     }
        //     // If solver choosing fails, repeat solver choosing routine.
        //     while (true)
        //     {
        //         int idx = 1;
        //         if (verifyAtFirst) Console.WriteLine("{0,3} {1,-25}{2,10}{3,10}", "ID", "SOLVER", "FAILS", "PASSES");
        //         for (int i = 0; i < solvers.Count; i++)
        //         {
        //             // Print available solvers list (with test results if performed).
        //             Console.Write("[{0,1}] {1,-25}", idx, solvers[i].GetType().Name);
        //             if (verifyAtFirst)
        //             {
        //                 // mark verified solvers with star
        //                 string mark = "";
        //                 if (testResults[i].NumberOfFails == 0) mark = "*";
        //                 Console.WriteLine("{0,10}{1,10} {2}", testResults[i].NumberOfFails, testResults[i].NumberOfPasses, mark);
        //             }
        //             else
        //             {
        //                 Console.Write("\n");
        //             }
        //             idx++;
        //         }
        //         Console.WriteLine("Choose solver (type 1 - {0})", solvers.Count);
        //         // get user choice
        //         string userNum = Console.ReadLine();
        //         try
        //         {
        //             int iSolver = Int32.Parse(userNum);
        //             if ((iSolver > 0) || (iSolver <= solvers.Count))
        //             {
        //                 return solvers[iSolver - 1];
        //             }
        //         }
        //         catch (Exception) { }
        //     }
        // }

        private static void SeriTest()
        {
            JDModel mdl = new JDModel();
            JDVar a = mdl.AddVar(xSize: 2, name: "a");
            JDVar b = mdl.AddVar(xSize: 2, type: JD.BINARY, name: "b");

            mdl += a <= 20;
            mdl += -3 + a + b == 20;

            mdl.SetObjective(a.Sum() + b.Sum(), JD.MAXIMIZE);

            mdl.SaveToFile("mdl");
            JDModel mdl2 = JDModel.BuildFromFile("mdl");
            Console.Write(mdl2);
            //mdl.SetObjective(sumA, JD.MAXIMIZE);
        }

        private static void DelenoTest()
        {
            JDModel mdl = new JDModel();
            JDVar a = mdl.AddVar(2, 2, -10, 10, JD.CONTINUOUS);

            mdl += a * (1.0 / 20) <= 1;
            
            Stopwatch sw = new Stopwatch();
            sw.Start();
            JDLinExpr sumA = a.Sum(1);
            sw.Stop();
            Console.WriteLine("Sum creating time: {0} s", sw.Elapsed.TotalSeconds);
            //mdl.SetObjective(sumA, JD.MAXIMIZE);
        }


        private static void JDSumTest()
        {
            JDModel mdl = new JDModel();
            JDVar a = mdl.AddVar(1000, 1000, -10, 10, JD.CONTINUOUS);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            JDLinExpr sumA = a.Sum(1);
            sw.Stop();
            Console.WriteLine("Sum creating time: {0} s", sw.Elapsed.TotalSeconds);
            //mdl.SetObjective(sumA, JD.MAXIMIZE);
        }

        public static void JDModelTest()
        {
            JDModel mdl = new JDModel();
            JDVar x = mdl.AddVar(ySize: 5, name: "x");
            JDVar y = mdl.AddVar(name: "y");
            double[] arr = { 10, 20, 30, 40, 50 };
            //// const.
            mdl += (x.Sum() == 1) / "sum";
            mdl += (x <= 1) / 1;
            mdl += x >= 0;
            mdl += (y == x * arr) / "con" / 5;
            mdl += y == 50;
            mdl.AddSOS2(x, new double[] { 0, 1, 2, 3, 4 });

            mdl.SetObjective(x, JD.MAXIMIZE);
            _solver.Solve(mdl);
            x.Print();
            Console.WriteLine(mdl.Params);
        }

        public static void SOS2Test()
        {
            string file = "model";
            JDModel mdl = new JDModel(1, useExplicitSOS: false);
            JDVar a = mdl.AddVar(5, 1, 0, 1);
            mdl.AddSOS2(a, new double[5]);
            //mdl += a <= 1;
            mdl += a.Sum() == 1;
            mdl += a[2, 0] == 0.3;

            mdl.SetObjective(a.Sum(), JD.MAXIMIZE);
            IJDSolver gSolver = JD.GetAvailableSolvers()[0];

            
            gSolver.Solve(mdl);
            double[,] refer = { { 10 }, { 10 }, { 0 } };
            a.Print();
        }
    }
}
