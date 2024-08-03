using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using JDSpace;

namespace JDSpace
{
    /// <summary>
    /// Data - model separating features tests.
    /// </summary>
    [TestFixture]
    internal class NamedConstantsTest
    {
        [SetUp]
        public void Init()
        {
            JDTester.ResetSolver();
        }

        /// <summary>
        /// Simple scalar equation test.
        /// </summary>
        [Test]
        public void NamedDoubleTest1()
        {
                    
            JDModel mdl = new JDModel(dataLoadable: true);

            // var init.
            JDVar x = mdl.AddVar(1, 1);

            // const.
            NamedConst coeff = new NamedConst("c1", 5);
            mdl += x * coeff == 15;

            // solve
            mdl.SetObjective(x, JD.MINIMIZE);
            
            JDTester._solver.Solve(mdl);

            // check result
            double[,] refer = { { 3 } };
            AssertExtensions.AreEqual(refer, x.ToDoubleMat(), 0);

            // reload model data
            Dictionary<string, double> modelData = new Dictionary<string, double>(){
                { "c1", 3} };
            mdl.LoadDataValues(modelData);
            JDTester.ResetSolver();
            JDTester._solver.Solve(mdl);

            // check result
            double[,] refer2 = { { 5 } };
            AssertExtensions.AreEqual(refer2, x.ToDoubleMat(), 0);
        }

        [Test]
        public void NamedDoubleTest2()
        {
                    
            JDModel mdl = new JDModel(dataLoadable: true);

            // var init.
            JDVar x = mdl.AddVar(1, 1, 0);

            // const.
            NamedConst coeff = new NamedConst("c1", 5);
            mdl += x + coeff <= 15;
            //mdl += x + 5 <= 15;

            // solve
            mdl.SetObjective(x, JD.MAXIMIZE);
            
            JDTester._solver.Solve(mdl);

            // check result
            double[,] refer = { { 10 } };
            AssertExtensions.AreEqual(refer, x.ToDoubleMat(), 0);

            // reload model data
            Dictionary<string, double> modelData = new Dictionary<string, double>(){
                { "c1", 3} };
            mdl.LoadDataValues(modelData);
            JDTester.ResetSolver();
            JDTester._solver.Solve(mdl);

            // check result
            double[,] refer2 = { { 12 } };
            AssertExtensions.AreEqual(refer2, x.ToDoubleMat(), 0);
        }

        [Test]
        public void NamedDoubleTest3()
        {
                    
            JDModel mdl = new JDModel(dataLoadable: true);

            // var init.
            JDVar x = mdl.AddVar(1, 1, 0);

            // const.
            NamedConst c1 = new NamedConst("c1", 2);
            NamedConst c2 = new NamedConst("c2", 5);
            NamedConst c3 = new NamedConst("c3", 15);
            mdl += c1 * x + c2 <= c3;
            //mdl += x + 5 <= 15;

            // solve
            mdl.SetObjective(x, JD.MAXIMIZE);
            
            JDTester._solver.Solve(mdl);

            // check result
            double[,] refer = { { 5 } };
            AssertExtensions.AreEqual(refer, x.ToDoubleMat(), 0);

            // reload model data
            Dictionary<string, double> modelData = new Dictionary<string, double>(){
                { "c1", 3}, 
                { "c2", 10},
                { "c3", 40}
            };
            mdl.LoadDataValues(modelData);
            JDTester.ResetSolver();
            JDTester._solver.Solve(mdl);

            // check result
            double[,] refer2 = { { 10 } };
            AssertExtensions.AreEqual(refer2, x.ToDoubleMat(), 0);
        }

        [Test]
        public void NamedArrayTest1()
        {
            //JDModel mdl = new GurobiJDModel();
            JDModel mdl = new JDModel();

            // var init.
            JDVar x = mdl.AddVar(xSize: 2, ySize: 1, lb: 0);
            NamedConst[] A = { new NamedConst(value: 5), new NamedConst(value: 3) };
            mdl += x == A;
            // solve
            mdl.SetObjective(x.Sum(), JD.MAXIMIZE);
            JDTester._solver.Solve(mdl);

            // check result
            double[,] referX = { { 5 }, { 3 } };
            AssertExtensions.AreEqual(referX, x.ToDoubleMat(), 1e-10);
        }

        [Test]
        public void NamedArrayTest2()
        {
            //JDModel mdl = new GurobiJDModel();
            JDModel mdl = new JDModel();

            // var init.
            JDVar x = mdl.AddVar(xSize: 2, ySize: 2, lb: 0);
            object[,] A = { { 1, new NamedConst(value: 2) }, { new NamedConst(value:3), 4.0 } };
            mdl += x == A;
            // solve
            mdl.SetObjective(x.Sum(), JD.MAXIMIZE);
            JDTester._solver.Solve(mdl);

            // check result
            double[,] referX = { { 1, 2 }, { 3, 4 } };
            AssertExtensions.AreEqual(referX, x.ToDoubleMat(), 1e-10);
        }

        [Test]
        public void NamedArrayReloadTest1()
        {
            //JDModel mdl = new GurobiJDModel();
            JDModel mdl = new JDModel(dataLoadable: true);

            // var init.
            JDVar x = mdl.AddVar(xSize: 2, ySize: 2, lb: 0);
            object[,] A = { { 1, new NamedConst("c1", 2) }, { new NamedConst("c2", 3), 4.0 } };
            mdl += x == A;
            // solve
            mdl.SetObjective(x.Sum(), JD.MAXIMIZE);
            
            JDTester._solver.Solve(mdl);

            // check result
            double[,] referX = { { 1, 2 }, { 3, 4 } };
            AssertExtensions.AreEqual(referX, x.ToDoubleMat(), 1e-10);

            JDTester.ResetSolver();
            Dictionary<string, double> dic = new Dictionary<string, double>()
            {
                {"c1", 22},{"c2",33}
            };
            mdl.LoadDataValues(dic);
            JDTester._solver.Solve(mdl);
            // check result
            double[,] referX2 = { { 1, 22 }, { 33, 4 } };
            AssertExtensions.AreEqual(referX2, x.ToDoubleMat(), 1e-10);
        }

        [Test]
        public void NamedArrayReloadTest2()
        {
            //JDModel mdl = new GurobiJDModel();
            JDModel mdl = new JDModel(dataLoadable: false);

            // var init.
            JDVar x = mdl.AddVar(xSize: 2, ySize: 2, lb: 0);
            object[,] A = { { 1, new NamedConst("c1", 2) }, { new NamedConst("c2", 3), 4.0 } };
            mdl += x + A == 5;
            // solve
            mdl.SetObjective(x.Sum(), JD.MAXIMIZE);
            
            JDTester._solver.Solve(mdl);

            // check result
            double[,] referX = { { 4, 3 }, { 2, 1 } };
            AssertExtensions.AreEqual(referX, x.ToDoubleMat(), 1e-10);
        }

        [Test]
        public void NamedArrayReloadTest3()
        {
                    
            JDModel mdl = new JDModel(dataLoadable: true);

            // var init.
            JDVar x = mdl.AddVar(1, 1, 0);

            // const.
            NamedConst c1 = new NamedConst("c1", 2);
            NamedConst c2 = new NamedConst("c2", 5);
            NamedConst c3 = new NamedConst("c3", 15);
            NamedConst c4 = new NamedConst("c4", 2);
            mdl += c4 * (c1 * x) + c2 <= c3; 
            //mdl += 2 * x + 5 <= 15;

            // solve
            mdl.SetObjective(x, JD.MAXIMIZE);
            
            JDTester._solver.Solve(mdl);

            // check result
            double[,] refer = { { 2.5 } };
            AssertExtensions.AreEqual(refer, x.ToDoubleMat(), 0);

            // reload model data
            Dictionary<string, double> modelData = new Dictionary<string, double>(){
                { "c1", 3}, 
                { "c2", -8},
                { "c3", 40},
                { "c4", 2}
            };
            mdl.LoadDataValues(modelData);
            JDTester.ResetSolver();
            JDTester._solver.Solve(mdl);

            // check result
            double[,] refer2 = { { 8 } };
            AssertExtensions.AreEqual(refer2, x.ToDoubleMat(), 0);
        }

        [Test]
        public void NamedExtensionTest1()
        {
            JDModel mdl = new JDModel(dataLoadable: true);

            // var init.
            JDVar x = mdl.AddVar(xSize: 2, ySize: 2, lb: 0);
            int[,] A = { { 1, 2 }, { 3, 4 } };
            object NamedA = A.Name("c");
            mdl += x == NamedA;
            // solve
            mdl.SetObjective(x.Sum(), JD.MAXIMIZE);

            JDTester._solver.Solve(mdl);

            // check result
            double[,] referX = { { 1, 2 }, { 3, 4 } };
            AssertExtensions.AreEqual(referX, x.ToDoubleMat(), 1e-10);

            JDTester.ResetSolver();
            Dictionary<string, double> dic = new Dictionary<string, double>()
            {
                {"c1", 22},{"c2",33}
            };
            mdl.LoadDataValues(dic);
            JDTester._solver.Solve(mdl);
            // check result
            double[,] referX2 = { { 1, 22 }, { 33, 4 } };
            AssertExtensions.AreEqual(referX2, x.ToDoubleMat(), 1e-10);
        }

        [Test]
        public void NamedExtensionTest2()
        {
            JDModel mdl = new JDModel(dataLoadable: true);

            // var init.
            JDVar x = mdl.AddVar(xSize: 1, ySize: 5, lb: 0);
            object[] A = { 1, 2, 3, 4, 5 };
            object NamedA = A.Name("c");
            mdl += x == NamedA;
            // solve
            mdl.SetObjective(x.Sum(), JD.MAXIMIZE);

            JDTester._solver.Solve(mdl);

            // check result
            double[,] referX =  { { 1, 2 ,  3, 4 , 5} };
            AssertExtensions.AreEqual(referX, x.ToDoubleMat(), 1e-10);

            JDTester.ResetSolver();
            Dictionary<string, double> dic = new Dictionary<string, double>()
            {
                {"c1", 22},{"c2",33}
            };
            mdl.LoadDataValues(dic);
            JDTester._solver.Solve(mdl);
            // check result
            double[,] referX2 ={ { 1, 22, 33, 4, 5 }};
            AssertExtensions.AreEqual(referX2, x.ToDoubleMat(), 1e-10);
        }

        [Test]
        public void NamedExtensionTest3()
        {
            JDModel mdl = new JDModel(dataLoadable: true);

            // var init.
            JDVar x = mdl.AddVar(xSize: 1, ySize: 1, lb: 0);
            double d = 5;
            object NameD = d.Name("c");
            mdl += x == NameD;
            // solve
            mdl.SetObjective(x.Sum(), JD.MAXIMIZE);

            JDTester._solver.Solve(mdl);

            // check result
            double[,] referX = { { 5 } };
            AssertExtensions.AreEqual(referX, x.ToDoubleMat(), 1e-10);

            JDTester.ResetSolver();
            Dictionary<string, double> dic = new Dictionary<string, double>()
            {
                {"c", 22}
            };
            mdl.LoadDataValues(dic);
            JDTester._solver.Solve(mdl);
            // check result
            double[,] referX2 = { { 22 } };
            AssertExtensions.AreEqual(referX2, x.ToDoubleMat(), 1e-10);
        }

        [Test]
        public void NamedExtensionTest4()
        {
            JDModel mdl = new JDModel(dataLoadable: true);

            // var init.
            JDVar x = mdl.AddVar(xSize: 1, ySize: 1, lb: 0);
            JDVar y = mdl.AddVar(xSize: 1, ySize: 1, lb: 0);
            double d = 5;
            object NameD = d.Name("c");
            object nc = new NamedConst(10);
            mdl += x == NameD;
            mdl += nc * (x + y) == 80;
            // solve
            mdl.SetObjective(x + y, JD.MAXIMIZE);

            JDTester._solver.Solve(mdl);

            // check result
            double[,] referX = { { 5 } };
            double[,] referY = { { 3 } };
            AssertExtensions.AreEqual(referX, x.ToDoubleMat(), 1e-10);
            AssertExtensions.AreEqual(referY, y.ToDoubleMat(), 1e-10);

            JDTester.ResetSolver();
            Dictionary<string, double> dic = new Dictionary<string, double>()
            {
                {"c", 4}
            };
            mdl.LoadDataValues(dic);
            JDTester._solver.Solve(mdl);
            // check result
            double[,] referX2 = { { 4 } };
            double[,] referY2 = { { 4 } }; 
            AssertExtensions.AreEqual(referX2, x.ToDoubleMat(), 1e-10);
            AssertExtensions.AreEqual(referY2, y.ToDoubleMat(), 1e-10);
        }

        [Test]
        public void NamedExtensionTest5()
        {
            JDModel mdl = new JDModel(dataLoadable: true);

            // var init.
            JDVar x = mdl.AddVar(xSize: 1, ySize: 1, lb: 0);
            JDVar y = mdl.AddVar(xSize: 1, ySize: 1, lb: 0);
            JDVar z = mdl.AddVar(xSize: 1, ySize: 1, lb: 0);
            ComposedConstant A = new NamedConst(10);
            //object B = new NamedConst(5);
            mdl += z == (x - y) * A;
            mdl += x == 5;
            mdl += z == 20;
            // solve
            mdl.SetObjective(x + y + z, JD.MAXIMIZE);

            JDTester._solver.Solve(mdl);

            // check result
            double[,] referY = { { 3 } };
            AssertExtensions.AreEqual(referY, y.ToDoubleMat(), 1e-10);
        }

        [Test]
        public void NamedExtensionTest6()
        {
            JDModel mdl = new JDModel(dataLoadable: true);

            // var init.
            JDVar x = mdl.AddVar(xSize: 1, ySize: 1, lb: 0);
            JDVar y = mdl.AddVar(xSize: 1, ySize: 1, lb: 0);
            JDVar z = mdl.AddVar(xSize: 1, ySize: 1, lb: 0);
            ComposedConstant A = new NamedConst(2);
            ComposedConstant B = new NamedConst(5);
            mdl += z == (x - y) * A * B | "Add constr name";
            mdl += x == 5;
            mdl += z == 20;
            // solve
            mdl.SetObjective(x + y + z, JD.MAXIMIZE);
            JDTester._solver.Solve(mdl);
            // check result
            double[,] referY = { { 3 } };
            AssertExtensions.AreEqual(referY, y.ToDoubleMat(), 1e-10);
        }

        // TODO 

        //[Test]
        //public void NamedArrayReloadTest2()
        //{
        //    //JDModel mdl = new GurobiJDModel();
        //    JDModel mdl = new JDModel(dataLoadable: true);

        //    // var init.
        //    JDVar x = mdl.AddVar(xSize: 2, ySize: 2, lb: 0);
        //    object[,] A = { { 1, new NamedConstant(2, "c1") }, { new NamedConstant(3, "c2"), 4.0 } };
        //    mdl += x + A == 5;
        //    // solve
        //    mdl.SetObjective(x.Sum(), JD.MAXIMIZE);
        //    mdl.RegisterNamedConstants();
        //    JDTester._solver.Solve(mdl);

        //    // check result
        //    double[,] referX = { { 4, 3 }, { 2, 1 } };
        //    AssertExtensions.AreEqual(referX, x.ToDoubleMat(), 1e-10);

        //    JDTester.ResetSolver();
        //    Dictionary<string, double> dic = new Dictionary<string, double>()
        //    {
        //        {"c1", 0},{"c2",5}
        //    };
        //    mdl.LoadDataValues(dic);
        //    JDTester._solver.Solve(mdl);
        //    // check result
        //    double[,] referX2 = { { 1, 5 }, { 0, 4 } };
        //    AssertExtensions.AreEqual(referX2, x.ToDoubleMat(), 1e-10);
        //}
    }
}
