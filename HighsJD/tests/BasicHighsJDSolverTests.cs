using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using JDSpace;
using HighsJD;
using System.Collections;

namespace Tests
{
    [TestFixture]
    internal class SolverTests
    {
        [Test]
        public void CreateSolverTest()
        {
            HighsJDSolver solver = new HighsJDSolver();
        }

        [Test]
        public void ScalarVar1()
        {        
            JDModel mdl = new JDModel();

            // var init.
            JDVar x = mdl.AddVar(1, 1);

            // const.
            mdl += x * 5 == 15;

            // solve
            mdl.SetObjective(x, JD.MINIMIZE);

            IJDSolver solver = new HighsJDSolver();
            solver.Solve(mdl);

            // check result
            double[,] refer = { { 3 } };
            AssertExtensions.AreEqual(refer, x.ToDoubleMat(), 0);
        }
    }
}
