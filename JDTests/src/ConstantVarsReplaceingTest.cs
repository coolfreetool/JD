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
    /// Constant variables (equal Lb, Ub) with constant replaceing tests.
    /// </summary>
    [TestFixture]
    internal class ConstantVarsReplaceingTest
    {
        [SetUp]
        public void Init()
        {
            JDTester.ResetSolver();
        }

        /// <summary>
        /// Simple matrix equation (var[] == c) test.
        /// </summary>
        [Test]
        public void ReplaceingTest1()
        {
            //JDModel mdl = new GurobiJDModel();
            JDModel mdl = new JDModel();

            // var init.
            JDVar x = mdl.AddVar(1, 2, 0, 1000);
            JDVar y = mdl.AddVar(1, 2, 20, 20);

            mdl += x + y <= 1000;

            // solve
            mdl.SetObjective(x.Sum() + y.Sum(), JD.MAXIMIZE);
            //mdl.ReplaceConstantVars();
            JDTester._solver.Solve(mdl);

            // check result
            double[,] refer = JD.ConstantMatrix(980.0, x.XSize, x.YSize);
            AssertExtensions.AreEqual(refer, x.ToDoubleMat(), 1e-10);
        }
    }
}
