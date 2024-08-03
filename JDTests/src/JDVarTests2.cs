using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using JDSpace;
using System.Collections;

namespace JDSpace
{
    /// <summary>
    /// JDVar testing class.
    /// </summary>
    [TestFixture]
    internal class JDVarTests2
    {
        [SetUp]
        public void Init()
        {
            JDTester.ResetSolver();
        }

        [Test]
        public void IntArrayTest1()
        {
            //JDModel mdl = new GurobiJDModel();
            JDModel mdl = new JDModel();

            // var init.
            JDVar x = mdl.AddVar(xSize: 1, ySize: 1, lb: 0);
            int[] A = { 5 };
            mdl += x == A;
            // solve
            mdl.SetObjective(x.Sum(), JD.MAXIMIZE);
            JDTester._solver.Solve(mdl);

            // check result
            double[,] referX = { { 5 } };
            AssertExtensions.AreEqual(referX, x.ToDoubleMat(), 1e-10);
        }

        [Test]
        public void IntArrayTest2()
        {
            //JDModel mdl = new GurobiJDModel();
            JDModel mdl = new JDModel();

            // var init.
            JDVar x = mdl.AddVar(xSize: 2, ySize: 1, lb: 0);
            int[] A = { 5, 3 };
            mdl += x == A;
            // solve
            mdl.SetObjective(x.Sum(), JD.MAXIMIZE);
            JDTester._solver.Solve(mdl);

            // check result
            double[,] referX = { { 5 }, { 3 }};
            AssertExtensions.AreEqual(referX, x.ToDoubleMat(), 1e-10);
        }

        [Test]
        public void VarsArrayTest1()
        {
            //JDModel mdl = new GurobiJDModel();
            JDModel mdl = new JDModel();

            // var init.
            JDVar x = mdl.AddVar(xSize: 2, ySize: 1, lb: 0);
            JDVar y = mdl.AddVar(xSize: 2, ySize: 1, lb: 0);
            int[] A = { 5, 3 };
            JDVar[] varArr = new JDVar[] { x, y };
            mdl += x == A;
            mdl += varArr.ElementSum() == 11;
            // solve
            mdl.SetObjective(x.Sum() + y.Sum(), JD.MAXIMIZE);
            JDTester._solver.Solve(mdl);

            // check result            
            double[,] referY = { { 6 }, { 8 } };
            AssertExtensions.AreEqual(referY, y.ToDoubleMat(), 1e-10);
        }

        [Test]
        public void VarsArrayTest2()
        {
            //JDModel mdl = new GurobiJDModel();
            JDModel mdl = new JDModel();

            // var init.
            JDVar x = mdl.AddVar(xSize: 2, ySize: 1, lb: 0);
            JDVar y = mdl.AddVar(xSize: 2, ySize: 1, lb: 0);            
            JDVar[] varArr = new JDVar[] { x, y };
            int[] A = { 2, 3 };
            mdl += x == A;
            mdl += y[0, 0] == 1;
            mdl += varArr.Sum() == 10;
            // solve
            mdl.SetObjective(x.Sum() + y.Sum(), JD.MAXIMIZE);
            JDTester._solver.Solve(mdl);

            // check result            
            double[,] referY = { { 1 }, { 4 } };
            AssertExtensions.AreEqual(referY, y.ToDoubleMat(), 1e-10);
        }

        [Test]
        public void VarsArrayTest3()
        {
            //JDModel mdl = new GurobiJDModel();
            JDModel mdl = new JDModel();

            // var init.
            JDVar x = mdl.AddVar(xSize: 2, ySize: 1, lb: 0);
            JDVar y = mdl.AddVar(xSize: 2, ySize: 1, lb: 0);
            JDVar[] varArr = new JDVar[] { x, y };
            int[] A = { 2, 3 };
            mdl += x == A;
            mdl += y[0, 0] == 1;
            mdl += varArr.Sum(0) == 10;
            // solve
            mdl.SetObjective(x.Sum() + y.Sum(), JD.MAXIMIZE);
            JDTester._solver.Solve(mdl);

            // check result            
            double[,] referY = { { 1 }, { 4 } };
            AssertExtensions.AreEqual(referY, y.ToDoubleMat(), 1e-10);
        }
    }
}
