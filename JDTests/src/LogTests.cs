﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JDSpace;
using NUnit.Framework;
using JDUtils;

namespace JDSpace
{
    /// <summary>
    /// JDLinExpr testing class.
    /// </summary>
    [TestFixture]
    public class LogTests
    {
        [SetUp]
        public void Init()
        {
            JDTester.ResetSolver();
        }

        /// <summary>
        /// Vector sum over columns test.
        /// </summary>
        [Test]
        public void LogTest()
        {
            JDModel mdl = new JDModel();
            
            // init var
            JDVar x = mdl.AddVar(2, 3, -50, 50, JD.CONTINUOUS);
            
            // constr.
            double[,] C1 = { { 30, 30, 30 } };
            double[,] C2 = { { 1, 2, 3 } };
            JDLinExpr colSum = x.Sum(0);
            mdl += (colSum == C1); // soucet po sloupcich (radkovy vektor) promenne matice == colSum
            mdl += (x[1, 1, 0, 2] == C2); // dolni radek promenne matice je roven C2
            // solve
            mdl.SetObjective(x.Sum(), JD.MAXIMIZE);
            Logger logger = new Logger();
            logger.Register(new ConsolLogClient(), Logger.AllFlags);
            JDTester._solver.SetLogger(logger);
            
            JDTester._solver.Solve(mdl);
            
            double[,] refer = {{  29, 28, 27},
                                {  1,  2,  3}};
            AssertExtensions.AreEqual(refer, x.ToDoubleMat(), 1e-10);
        }
    }
}
