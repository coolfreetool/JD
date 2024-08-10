using System;
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
    public class JDLinExprTests
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
        public void VectorSumColumnTest()
        {
            //JDModel mdl = new GurobiJDModel();
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
            
            JDTester._solver.Solve(mdl);

            double[,] refer = {{  29, 28, 27},
                                {  1,  2,  3}};
            AssertExtensions.AreEqual(refer, x.ToDoubleMat(), 1e-10);
        }

        /// <summary>
        /// Constraint in form (expr == expr) test.
        /// </summary>
        [Test]
        public void BothSitesTest()
        {
            //JDModel mdl = new GurobiJDModel();
            JDModel mdl = new JDModel();
            
            // init var
            JDVar x = mdl.AddVar(2, 3, -50, 50, JD.CONTINUOUS);
            JDVar y = mdl.AddVar(2, 3, -50, 50, JD.CONTINUOUS);
            

            mdl += 3 * y == 10 * (x + 3);

            // solve
            mdl.SetObjective(x.Sum() + y.Sum(), JD.MAXIMIZE);
            JDTester._solver.Solve(mdl);

            double[,] xRefer = {{  12, 12, 12},
                                {  12,  12,  12}};
            double[,] yRefer = {{  50, 50, 50},
                                {  50,  50,  50}};
            AssertExtensions.AreEqual(xRefer, x.ToDoubleMat(), 1e-10);
            AssertExtensions.AreEqual(yRefer, y.ToDoubleMat(), 1e-10);
        }

        /// <summary>
        /// Expr minus constant test.
        /// </summary>
        [Test]
        public void MinusConstantTest()
        {
            JDModel mdl = new JDModel();
            
            // init var
            JDVar x = mdl.AddVar(1, 3, -50, 50, JD.CONTINUOUS);
            

            // constr.
            double[] C1 = { 29, 28, 27 };
            double[] C2 = { 1, 2, 3 };
            mdl += C1 == x - C2;

            // solve
            mdl.SetObjective(x.Sum(), JD.MAXIMIZE);
            JDTester._solver.Solve(mdl);

            double[,] xRef = { { 30, 30, 30 } };
            AssertExtensions.AreEqual(xRef, x.ToDoubleMat(), 1e-10);
        }

        /// <summary>
        /// Custom lazy level setting test.
        /// </summary>
        [Test]
        public void LazyLevelTest()
        {
            JDModel mdl = new JDModel();

            // init var
            JDVar x = mdl.AddVar(1, 3, -50, 50, JD.CONTINUOUS);

            byte myLazyLevel = 2;

            // constr.
            double[] C1 = { 29, 28, 27 };
            double[] C2 = { 1, 2, 3 };
            mdl += (C1 == x - C2) / myLazyLevel;

            foreach (ScConstr con in mdl.Constrs)
            {
                Assert.AreEqual(myLazyLevel, con.LazyLevel);
            }
        }

        /// <summary>
        /// Custom lazy level setting test.
        /// </summary>
        [Test]
        public void RedundantExprsTest1()
        {
            JDModel mdl = new JDModel();

            // init var
            JDVar x = mdl.AddVar(1, 3, -50, 50, JD.CONTINUOUS);

            byte myLazyLevel = 2;

            // constr.
            double[] C1 = { 29, 28, 27 };
            double[] C2 = { 1, 2, 3 };
            JDLinExpr exp = mdl.AddLinExpr(3, 1);
            exp += C2;
            mdl += (exp <= C1);

            Assert.AreEqual(mdl.Constrs.Count, 0);
        }

        /// <summary>
        /// Custom lazy level setting test.
        /// </summary>
        [Test]
        public void RedundantExprsTest2()
        {
            JDModel mdl = new JDModel();

            // init var
            JDVar x = mdl.AddVar(1, 3, -50, 50, JD.CONTINUOUS);

            byte myLazyLevel = 2;

            // constr.
            double[] C1 = { 29, 28, 27 };
            double[] C2 = { 1, 2, 3 };
            JDLinExpr exp = mdl.AddLinExpr(3, 1);
            exp += C2;
            try
            {
                mdl += (exp >= C1);
            }
            catch (JDException ex)
            {
                // Infeasible constant constraint catched.
                return;
            }
            throw new Exception("Infeasible constant constraint uncatched!");
        }

        /// <summary>
        /// Get linear expr sub region.
        /// </summary>
        [Test]
        public void SubLinExprTest1()
        {
            JDModel mdl = new JDModel();
            JDVar a = mdl.AddVar(3, 5, 0, 100, JD.CONTINUOUS);
            double[,] C = { { 1, 2, 3, 4,  5 },
                            { 6, 7, 8, 9, 10 },
                            { 11, 12, 13, 14, 15 } };
            JDLinExpr e1 = a + C;

            JDLinExpr e2 = e1[1, 2, 1, 3];
            Assert.AreEqual(2, e2.XSize);
            Assert.AreEqual(3, e2.YSize);

            mdl.SetObjective(a.Sum(), JD.MAXIMIZE);
            JDTester._solver.Solve(mdl);

            double?[,] expE2 = { { 107, 108, 109 },
                                { 112, 113, 114 } };
            AssertExtensions.AreEqual(expE2, e2.ToDouble());
        }
    }
}
