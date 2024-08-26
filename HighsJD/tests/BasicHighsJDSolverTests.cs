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
    }
}
