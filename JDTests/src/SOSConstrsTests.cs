using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using JDSpace;

namespace JDSpace
{
    /// <summary>
    /// JDVar testing class.
    /// </summary>
    [TestFixture]
    public class SOSConstrsTests
    {
        public const double ALLOWED_DELTA = 0.0001;

        [SetUp]
        public void Init()
        {
            JDTester.ResetSolver();
        }
                
        [Test]
        public void SimpleSOS1()
        {
            JDModel mdl = new JDModel();
            
            // var init.
            JDVar x = mdl.AddVar(1, 3);

            // const.
            mdl += x <= 10;
            mdl.AddSOS1(x, new double[] { 0, 1, 2 });

            // solve
            mdl.SetObjective(x, JD.MAXIMIZE);
            JDTester._solver.Solve(mdl);

            // check result
            double[,] refer = { { 10, 0, 0 } };
            AssertExtensions.AreEqual(refer, x.ToDoubleMat(), ALLOWED_DELTA);
        }

        [Test]
        public void SimpleSOS2()
        {
            JDModel mdl = new JDModel();

            // var init.
            JDVar x = mdl.AddVar(1, 4);

            // const.
            mdl += x <= 5;
            mdl.AddSOS2(x, new double[] { 0, 1, 2, 3 });

            // solve
            mdl.SetObjective(x, JD.MAXIMIZE);
            JDTester._solver.Solve(mdl);

            // check result
            double[,] refer = { { 5, 5, 0, 0 } };
            AssertExtensions.AreEqual(refer, x.ToDoubleMat(), ALLOWED_DELTA);
        }

        [Test]
        public void ExplicitSOS1Test1()
        {
            var mdl = new JDModel(1,useExplicitSOS: true);

            var x = mdl.AddVar(1, 4, lb: 0, ub: 10);
            
            mdl.AddSOS1(x, new double[] { 0, 0, 0, 0 });

            mdl.SetObjective(x * new double[] { 1, 2, 4, 3 },JD.MAXIMIZE);
            JDTester._solver.Solve(mdl);

            double[,] expected = { { 0, 0, 10, 0 } };
            AssertExtensions.AreEqual(expected, x.ToDoubleMat(), ALLOWED_DELTA);
        }

        [Test]
        public void ExplicitSOS1Test2()
        {
            var mdl = new JDModel(1, useExplicitSOS: true);

            var x = mdl.AddVar(1, 4, lb: 0, ub: 10);
            mdl.AddSOS1(x, new double[] { 0, 0, 0, 0 });

            mdl.SetObjective(x * new double[] { 1, 2, 4, 3 }, JD.MINIMIZE);
            JDTester._solver.Solve(mdl);

            double[,] expected = { { 0, 0, 0, 0 } };
            AssertExtensions.AreEqual(expected, x.ToDoubleMat(), ALLOWED_DELTA);
        }

        [Test]
        public void ExplicitSOS2Test1()
        {
            var mdl = new JDModel(1, useExplicitSOS: true);

            var x = mdl.AddVar(1, 5, lb: 0, ub: 10);

            mdl.AddSOS2(x, new double[] { 0, 0, 0, 0 ,0});

            var objexpr = x * new double[] { 10, -6, 9, 2, 1};
            mdl.SetObjective(objexpr, JD.MAXIMIZE);
            JDTester._solver.Solve(mdl);

            double[,] expected = { { 0, 0, 10, 10, 0} };
            AssertExtensions.AreEqual(expected, x.ToDoubleMat(), ALLOWED_DELTA);

        }

        [Test]
        public void ExplicitSOS2Test2()
        {
            var mdl = new JDModel(1, useExplicitSOS: true);

            var x = mdl.AddVar(1, 5, lb: 0, ub: 10);
            mdl.AddSOS2(x, new double[] { 0, 0, 0, 0, 0 });


            mdl.SetObjective(x * new double[] { 10, 1, 2, 9, 1 }, JD.MINIMIZE);
            JDTester._solver.Solve(mdl);

            double[,] expected = { { 0, 0, 0, 0, 0 } };
            AssertExtensions.AreEqual(expected, x.ToDoubleMat(), ALLOWED_DELTA);

        }

        [Test]
        public void ExplicitSOS2Test3()
        {
            var mdl = new JDModel(1, useExplicitSOS: true);

            var x = mdl.AddVar(1, 5, lb: 0, ub: 10);
            mdl.AddSOS2(x, new double[] { 0, 0, 0, 0, 0 });

            mdl.SetObjective(x * new double[] { 9, 2, -2, 12, -1 }, JD.MAXIMIZE);
            JDTester._solver.Solve(mdl);

            double[,] expected = { { 0, 0, 0, 10, 0 } };
            AssertExtensions.AreEqual(expected, x.ToDoubleMat(), ALLOWED_DELTA);

        }

        [Test]
        public void ExplicitSOS1and2Test1()
        {
            var mdl = new JDModel(1, useExplicitSOS: true);

            var x = mdl.AddVar(1, 10, lb: 0, ub: 10);

            mdl.AddSOS1(x[0, 0, 0, 6], new double[] { 0, 0, 0, 0, 0, 0, 0 });
            mdl.AddSOS2(x[0, 0, 3, 9], new double[] { 0, 0, 0, 0, 0, 0, 0 });

            mdl.SetObjective(x * new double[] { 1, 1, 10, 1, 1, 1, 10, 10, 10, 1 }, JD.MAXIMIZE);
            JDTester._solver.Solve(mdl);

            double[,] expected = { { 0, 0, 10, 0, 0, 0, 0, 10, 10, 0 } };
            AssertExtensions.AreEqual(expected, x.ToDoubleMat(), ALLOWED_DELTA);
        }

        [Test]
        public void ExplicitSOS1and2Test2()
        {
            var mdl = new JDModel(1, useExplicitSOS: true);

            var x = mdl.AddVar(1, 10, lb: 0, ub: 10);

            mdl.AddSOS1(x[0, 0, 0, 6], new double[] { 0, 0, 0, 0, 0, 0, 0 });
            mdl.AddSOS2(x[0, 0, 3, 9], new double[] { 0, 0, 0, 0, 0, 0, 0 });

            mdl.SetObjective(x * new double[] { 1, 1, 1, 1, 100, 1, 70, 50, 1, 1 }, JD.MAXIMIZE);
            JDTester._solver.Solve(mdl);

            double[,] expected = { { 0, 0, 0, 0, 0, 0, 10, 10, 0, 0 } };
            AssertExtensions.AreEqual(expected, x.ToDoubleMat(), ALLOWED_DELTA);
        }

        [Test]
        public void ExplicitSOS1and2Test3()
        {
            var mdl = new JDModel(1, useExplicitSOS: true);

            var x = mdl.AddVar(1, 10, lb: 0, ub: 10);

            mdl.AddSOS1(x[0, 0, 0, 6], new double[] { 0, 0, 0, 0, 0, 0, 0 });
            mdl.AddSOS2(x[0, 0, 3, 9], new double[] { 0, 0, 0, 0, 0, 0, 0 });

            mdl.SetObjective(x * new double[] { 1, 1, 100, 70, 1, 1, 90, 20, 10, 1 }, JD.MAXIMIZE);
            JDTester._solver.Solve(mdl);

            double[,] expected = { { 0, 0, 10, 0, 0, 0, 0, 10, 10, 0 } };
            AssertExtensions.AreEqual(expected, x.ToDoubleMat(), ALLOWED_DELTA);
        }
    }
}
