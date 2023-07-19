using JDSpace;
using OTJD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JDUtils;
using NUnit.Framework;

namespace JDTests
{
    class Program
    {
        static void Main(string[] args)
        {
            //JDTester.SOS2Test();
            performTests();
            // SetDoubleParamWithInteger();
            //SaveComposedModelTest();
            //ReuseJDModelTest();
            //CopyJDModelTest();
            //AddJDModelNonComposedConstantsTest();
            //TimeLimitReports();
            // Console.ReadKey();
        }

        static void TimeLimitReports()
        {
            IJDSolver solver = new CbcJDSolver();
            JDModel mdl = new JDModel();
            JDVar x = mdl.AddVar(xSize: 1, ySize: 1, lb: 0);
            mdl += x <= 100;
            mdl.SetObjective(x.Sum(), JD.MAXIMIZE);
            mdl.Params.Set(JD.DoubleParam.TIME_LIMIT, 0.00000001);
            solver.SetLogger(new JDUtils.Logger());
            solver.GetLogger().Register(new JDUtils.ConsolLogClient(), JDUtils.Logger.AllFlags);
            solver.Solve(mdl);
            if (mdl.Params.Get<int>(JD.IntParam.RESULT_STATUS) == 1)
            {
                // check result
                double[,] referX = { { 100 } };
                AssertExtensions.AreEqual(referX, x.ToDoubleMat(), 1e-10);
            }
            else
            {
                Console.WriteLine("Solving fail");
                if (mdl.Params.IsSet(JD.StringParam.STATUS))
                {
                    Console.WriteLine("Solver status string: {0}", 
                        mdl.Params.Get<string>(JD.StringParam.STATUS));
                }
            }
        }

        static void ReuseJDModelTest()
        {
            IJDSolver solver = new CbcJDSolver();
            JDModel mdl = new JDModel();
            JDVar x = mdl.AddVar(xSize: 5, ySize: 5, lb: 0);
            mdl += x <= 17;
            mdl.SetObjective(x.Sum(), JD.MAXIMIZE);
            solver.SetLogger(new JDUtils.Logger());
            solver.GetLogger().Register(new JDUtils.ConsolLogClient(), JDUtils.Logger.AllFlags);
            solver.Solve(mdl);
            x.Print();
            JDVar y = mdl.AddVar(xSize: 4, ySize: 7);
            mdl += y <= 10;
            mdl.SetObjective(y.Sum(), JD.MAXIMIZE);
            solver.Solve(mdl);
            y.Print();
        }

        static void CopyJDModelTest()
        {
            IJDSolver solver = new CbcJDSolver();
            JDModel mdl = new JDModel();
            JDVar x = mdl.AddVar(xSize: 5, ySize: 5, lb: 0);
            mdl += x <= 17;
            mdl.SetObjective(x.Sum(), JD.MAXIMIZE);
            solver.SetLogger(new JDUtils.Logger());
            solver.GetLogger().Register(new JDUtils.ConsolLogClient(), JDUtils.Logger.AllFlags);
            solver.Solve(mdl);
            JDModel mdlCp = mdl.Clone() as JDModel;
            Console.WriteLine("Model - Vars count: {0}.", mdl.ConVars.Count + mdl.IntVars.Count + mdl.BinVars.Count);
            Console.WriteLine("Copy of model - Vars count: {0}.", mdlCp.ConVars.Count + mdlCp.IntVars.Count + mdlCp.BinVars.Count);
        }

        static void AddJDModelNonComposedConstantsTest()
        {
            IJDSolver solver = new CbcJDSolver();
            JDModel mdl = new JDModel(dataLoadable: true);
            JDVar x = mdl.AddVar(xSize: 5, lb: 0);
            ComposedConstant c1 = new NamedConst(7);
            mdl += c1 + x <= 17;
            mdl.SetObjective(x.Sum(), JD.MAXIMIZE);
            solver.Solve(mdl);
            x.Print();
            JDModel mdl2Add = new JDModel(firstVarId: mdl.Vars.Count + 1,
                firstConstrId: mdl.Constrs.Count + 1, dataLoadable: true);
            JDVar y = mdl2Add.AddVar(xSize: 5, lb: 0);
            ComposedConstant c2 = new NamedConst(3);
            mdl2Add += c2 * y <= 21;
            mdl2Add.SetObjective(y.Sum(), JD.MAXIMIZE);
            mdl.Add(mdl2Add);
            solver.Solve(mdl);
            y.Print();    
        }

        static void SetDoubleParamWithInteger()
        {
            JDTester._solver = new CbcJDSolver();
            JDModel mdl = new JDModel();
            JDVar x = mdl.AddVar(xSize: 1, ySize: 1, lb: 0);
            mdl += x <= 10;
            mdl.SetObjective(x.Sum(), JD.MAXIMIZE);
            mdl.Params.Set(JD.DoubleParam.TIME_LIMIT, 1);

            JDTester._solver.Solve(mdl);

            // check result
            double[,] referX = { { 10 } };
            AssertExtensions.AreEqual(referX, x.ToDoubleMat(), 1e-10);
        }

        static void SaveComposedModelTest()
        {
            JDTester._solver = new CbcJDSolver();

            JDModel mdl = new JDModel(dataLoadable: true);

            // var init.
            JDVar x = mdl.AddVar(xSize: 1, ySize: 1, lb: 0);
            double d = 5;
            object NameD = d.Name("c");
            mdl += x == NameD;
            //mdl += x == d;
            // solve
            mdl.SetObjective(x.Sum(), JD.MAXIMIZE);

            JDTester._solver.Solve(mdl);

            // check result
            double[,] referX = { { 5 } };
            AssertExtensions.AreEqual(referX, x.ToDoubleMat(), 1e-10);

            //JDTester.ResetSolver();
            JDTester._solver = new CbcJDSolver();

            string mdlfile = "mdlfile";
            mdl.SaveToFile(mdlfile);
            mdl = JDModel.BuildFromFile(mdlfile);
            Dictionary<string, double> dic = new Dictionary<string, double>()
            {
                { "c", 22 }
            };
            mdl.LoadDataValues(dic);
            JDTester._solver.Solve(mdl);
            // check result
            double referX2 = 22;
            ScVar scX = mdl.Vars[0];
            Assert.AreEqual(referX2, scX.Value, 1e-10);
        }

        static void performTests()
        {
            
            // create solver instances
            List<IJDSolver> solvers = new List<IJDSolver>()
            { 
                new CbcJDSolver()
            };
            List<IJDSolver> solversLoaded = JD.GetAvailableSolvers();
            solvers.AddRange(solversLoaded);
            foreach(IJDSolver solver in solvers)
            {
                JDTester.TestSolver(solver);
            }
        }
    }
}
