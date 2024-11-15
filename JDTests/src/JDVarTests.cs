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
    internal class JDVarTests
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
        public void ScalarVar1()
        {
            //JDModel mdl = new GurobiJDModel();            
            JDModel mdl = new JDModel();
            
            // var init.
            JDVar x = mdl.AddVar(1, 1);
            

            // const.
            mdl += x * 5 == 15;

            // solve
            mdl.SetObjective(x, JD.MINIMIZE);
            JDTester._solver.Solve(mdl);

            // check result
            double[,] refer = { { 3 } };
            AssertExtensions.AreEqual(refer, x.ToDoubleMat(), 0);
        }
        [Test]
        public void ScalarVar2()
        {
            //JDModel mdl = new GurobiJDModel();            
            JDModel mdl = new JDModel();

            // var init.
            JDVar x = mdl.AddVar(1, 1);
            JDVar y = mdl.AddVar(1, 1);

            // const.
            mdl += x == 10;
            mdl += x + y * 3 == 16;

            // solve
            mdl.SetObjective(x, JD.MINIMIZE);
            JDTester._solver.Solve(mdl);

            // check result
            double[,] xRefer = { { 10 } };
            double[,] yRefer = { { 2 } };
            AssertExtensions.AreEqual(xRefer, x.ToDoubleMat(), 0);
            AssertExtensions.AreEqual(yRefer, y.ToDoubleMat(), 0);
        }

        /// <summary>
        /// Simple matrix equation (var[] == c) test.
        /// </summary>
        [Test]
        public void MatrixVarXScalar()
        {
            //JDModel mdl = new GurobiJDModel();
            JDModel mdl = new JDModel();
            
            // var init.
            JDVar x = mdl.AddVar(4, 5);
            

            // constr.
            mdl += x == 7;

            // solve
            mdl.SetObjective(x.Sum(), JD.MAXIMIZE);
            JDTester._solver.Solve(mdl);

            // check result
            double[,] refer = JD.ConstantMatrix(7.0, x.XSize, x.YSize);
            AssertExtensions.AreEqual(refer, x.ToDoubleMat(), 1e-10);
        }

        
        [Test]
        public void MatrixVar2()
        {            
            JDModel mdl = new JDModel();

            // var init.
            JDVar x = mdl.AddVar(4, 5, ub: 100);


            // constr.
            mdl += x >= 7;

            // solve
            mdl.SetObjective(x.Sum(), JD.MAXIMIZE);
            JDTester._solver.Solve(mdl);

            // check result
            double[,] refer = JD.ConstantMatrix(100.0, x.XSize, x.YSize);
            AssertExtensions.AreEqual(refer, x.ToDoubleMat(), 1e-10);
        }

        /// <summary>
        /// Matrix difference (var[] - const) equation test.
        /// </summary>
        [Test]
        public void MatrixVarMinus()
        {
            //JDModel mdl = new GurobiJDModel();
            JDModel mdl = new JDModel();
            
            // var init.
            JDVar x = mdl.AddVar(4, 5);
            

            // constr.
            mdl += x - 5 <= 7;

            // solve
            mdl.SetObjective(x.Sum(), JD.MAXIMIZE);
            JDTester._solver.Solve(mdl);

            // check result
            double[,] refer = JD.ConstantMatrix(12.0, x.XSize, x.YSize);
            AssertExtensions.AreEqual(refer, x.ToDoubleMat(), 1e-10);
        }

        /// <summary>
        /// Matrix sum (var[] + const) equation test.
        /// </summary>
        [Test]
        public void MatrixVarPlus()
        {
            //JDModel mdl = new GurobiJDModel();
            JDModel mdl = new JDModel();
            
            // var init.
            JDVar x = mdl.AddVar(4, 5);
            

            // constr.
            mdl += x + 5 <= 7;

            // solve
            mdl.SetObjective(x.Sum(), JD.MAXIMIZE);
            JDTester._solver.Solve(mdl);

            // check result
            double[,] refer = JD.ConstantMatrix(2.0, x.XSize, x.YSize);
            AssertExtensions.AreEqual(refer, x.ToDoubleMat(), 1e-10);
        }

        /// <summary>
        /// Matrix var with scalar constant multiplier (var[] * c) test.
        /// </summary>
        [Test]
        public void MatrixVarMultip()
        {
            JDModel mdl = new JDModel();
            
            // var init.
            JDVar x = mdl.AddVar(4, 5);
            

            // constr.
            mdl += x * 3 <= 15;

            // solve
            mdl.SetObjective(x.Sum(), JD.MAXIMIZE);
            JDTester._solver.Solve(mdl);

            // check result
            double[,] refer = JD.ConstantMatrix(5.0, x.XSize, x.YSize);
            AssertExtensions.AreEqual(refer, x.ToDoubleMat(), 1e-10);
        }

        /// <summary>
        /// Matrix indexing (selecting submatrices) test.
        /// </summary>
        [Test]
        public void MatrixIndexing()
        {           
            // Vytvor model - optimalizacni ulohu
            JDModel mdl = new JDModel();
            // Vytvor matici opt. promennych rozmeru 4x5
            JDVar x = mdl.AddVar(4, 5, -500, 500, JD.CONTINUOUS);            
            // Vytvor konstantni matici
            double[,] C = {{ 1, 2, 3 },
                          { 4, 5, 6 }};
            // Pridej omezeni
            mdl += (1 * x[0, 1, 1, 3] <= C); 
            mdl += (1 * x[2, 4] == 24); 
            mdl += (x[2, 3, 1, 2] <= 2); 
            mdl += (x[1, 0] + x[2, 0] == 300); 
            mdl += (2 * x[1, 0] == x[2, 0]); 
            // Definuj kriterium pro maximalizaci - sumu prvku x
            mdl.SetObjective(x.Sum(), JD.MAXIMIZE);
            // Optimalizuj
            JDTester._solver.Solve(mdl);

            // check result
            double[,] refer = { { 500,  1,  2,  3,   500 },
                                { 100,  4,  5,  6,   500 },
                                { 200,  2,  2,  500,  24 },
                                { 500,  2,  2,  500, 500 } };

            AssertExtensions.AreEqual(refer, x.ToDoubleMat(), 1e-10);
        }

        
        [Test]
        public void MultipleConstrs1()
        {
            //JDModel mdl = new GurobiJDModel();
            JDModel mdl = new JDModel();


            // var init.
            JDVar x = mdl.AddVar(xSize: 2, ySize: 2);

            mdl += -20 <= x <= 50;

            // solve
            mdl.SetObjective(x[0,0,0,1].Sum() - x[1,1,0,1].Sum(), JD.MAXIMIZE);
            JDTester._solver.Solve(mdl);

            // check result
            double[,] refer = { { 50,  50 },
                                { -20,  -20}};

            AssertExtensions.AreEqual(refer, x.ToDoubleMat(), 1e-10);
        }
        
        [Test]
        public void MultipleConstrs2()
        {
            //JDModel mdl = new GurobiJDModel();
            JDModel mdl = new JDModel();

            // var init.
            JDVar x = mdl.AddVar(xSize: 2, ySize: 2);

            mdl += 50 >= x >= -20;

            // solve
            mdl.SetObjective(x[0, 0, 0, 1].Sum() - x[1, 1, 0, 1].Sum(), JD.MAXIMIZE);
            JDTester._solver.Solve(mdl);

            // check result
            double[,] refer = { { 50,  50 },
                                { -20,  -20}};
            AssertExtensions.AreEqual(refer, x.ToDoubleMat(), 1e-10);
        }

        
        [Test]
        public void MultipleConstrs3()
        {
            //JDModel mdl = new GurobiJDModel();
            JDModel mdl = new JDModel();

            // var init.
            JDVar x = mdl.AddVar(xSize: 2, ySize: 2);
            JDVar y = mdl.AddVar();
            JDVar z = mdl.AddVar(xSize: 3);

            mdl += y <= 50 >= x >= -20 == z;

            // solve
            mdl.SetObjective(x[0, 0, 0, 1].Sum() - x[1, 1, 0, 1].Sum() + y + z.Sum() , JD.MAXIMIZE);
            JDTester._solver.Solve(mdl);

            // check result
            double[,] referX = { { 50,  50 },
                                { -20,  -20}};
            AssertExtensions.AreEqual(referX, x.ToDoubleMat(), 1e-10);
            double[,] referY = {{ 50 }};
            AssertExtensions.AreEqual(referY, y.ToDoubleMat(), 1e-10);

            double[,] referZ = { { -20 }, { -20 }, { -20 } };
            AssertExtensions.AreEqual(referZ, z.ToDoubleMat(), 1e-10);
        }

        [Test]
        public void MatrixMulitplyingTest1()
        {
            //JDModel mdl = new GurobiJDModel();
            JDModel mdl = new JDModel();

            // var init.
            JDVar x = mdl.AddVar(xSize: 2, ySize: 1, lb: 0);
            
            double[,] A = { {1, 2} };
            double[,] C = { { 11 } };

            mdl += A * x == C;
            mdl += x[1, 0] == 4;

            // solve
            mdl.SetObjective(x.Sum(), JD.MAXIMIZE);
            JDTester._solver.Solve(mdl);

            // check result
            double[,] referX = { { 3 },
                                { 4,}};
            AssertExtensions.AreEqual(referX, x.ToDoubleMat(), 1e-10);
        }

        [Test]
        public void MatrixMulitplyingTest2()
        {
            //JDModel mdl = new GurobiJDModel();
            JDModel mdl = new JDModel();

            // var init.
            JDVar x = mdl.AddVar(xSize: 2, ySize: 1, lb: 0);

            double[][] A = new double[][]{ new double[]{ 1, 2 } };
            double[,] C = { { 11 } };

            mdl += A * x == C;
            mdl += x[1, 0] == 4;

            // solve
            mdl.SetObjective(x.Sum(), JD.MAXIMIZE);
            JDTester._solver.Solve(mdl);

            // check result
            double[,] referX = { { 3 },
                                { 4,}};
            AssertExtensions.AreEqual(referX, x.ToDoubleMat(), 1e-10);
        }

        [Test]
        public void MatrixMulitplyingTest3()
        {
            //JDModel mdl = new GurobiJDModel();
            JDModel mdl = new JDModel();

            // var init.
            JDVar x = mdl.AddVar(xSize: 1, ySize: 2, lb: 0);

            double[,] A =  { { 3 },
                             { 4,}};
            double[,] C = { { 11 } };

            mdl += x * A == C;
            mdl += x[0, 1] == 2;

            // solve
            mdl.SetObjective(x.Sum(), JD.MAXIMIZE);
            JDTester._solver.Solve(mdl);

            // check result
            double[,] referX = { { 1, 2 } };
            AssertExtensions.AreEqual(referX, x.ToDoubleMat(), 1e-10);
        }

        [Test]
        public void MatrixMulitplyingTest4()
        {
            //JDModel mdl = new GurobiJDModel();
            JDModel mdl = new JDModel();

            // var init.
            JDVar x = mdl.AddVar(xSize: 1, ySize: 2, lb: 0);

            double[][] A = new double[][] {new double[] { 3 }, new double[]{ 4,}};
            double[][] C = new double[][]{new double[] { 11 } };

            mdl += x * A == C;
            mdl += x[0, 1] == 2;

            // solve
            mdl.SetObjective(x.Sum(), JD.MAXIMIZE);
            JDTester._solver.Solve(mdl);

            // check result
            double[,] referX = { { 1, 2 } };
            AssertExtensions.AreEqual(referX, x.ToDoubleMat(), 1e-10);
        }

        [Test]
        public void MatrixMulitplyingTest5()
        {
            //JDModel mdl = new GurobiJDModel();
            JDModel mdl = new JDModel();

            // var init.
            JDVar x = mdl.AddVar(xSize: 2, ySize: 1);
            double[,] A = { { 1,  2 },
                            { 3,  4},};

            double[,] C = { { 17 },
                            { 39}};
            mdl += A * x == C;
            //mdl += x[1, 1] == 5;
            //mdl += x[1, 2] == 6;
            //mdl += 

            // solve
            mdl.SetObjective(x.Sum(), JD.MAXIMIZE);
            JDTester._solver.Solve(mdl);

            // check result
            double[,] referX = { { 5},
                                { 6 }};
            AssertExtensions.AreEqual(referX, x.ToDoubleMat(), 1e-10);
        }

        [Test]
        public void MatrixMulitplyingTest6()
        {
            //JDModel mdl = new GurobiJDModel();
            JDModel mdl = new JDModel();

            // var init.
            JDVar x = mdl.AddVar(xSize: 2, ySize: 2, lb: 0);
            double[,] A = { { 5},
                                { 6 }};

            double[,] C = { { 17 },
                            { 39}};
            mdl += x * A == C;
            mdl += x[0, 1] == 2;
            mdl += x[1, 0] == 3;
            //mdl += 

            // solve
            mdl.SetObjective(x.Sum(), JD.MAXIMIZE);
            JDTester._solver.Solve(mdl);

            // check result
            double[,] referX = { { 1,  2 },
                            { 3,  4},};
            
            AssertExtensions.AreEqual(referX, x.ToDoubleMat(), 1e-10);
        }

        [Test]
        public void MatrixMulitplyingTest7()
        {
            //JDModel mdl = new GurobiJDModel();
            JDModel mdl = new JDModel();

            // var init.
            JDVar x = mdl.AddVar(xSize: 2, ySize: 3, lb: 0);
            double[,] A = { { 7,  8 },
                            { 9,  10},
                            { 11,  12},};
            double[,] C = { { 58,  64 },
                            { 139,  154}};
            mdl += x * A == C;
            mdl += x[1, 1] == 5;
            mdl += x[1, 2] == 6;
            mdl += x[0, 0] == 1;
            mdl += x[0, 2] == 3;
            //mdl += 

            // solve
            mdl.SetObjective(x.Sum(), JD.MAXIMIZE);
            JDTester._solver.Solve(mdl);

            // check result
            double[,] referX = { { 1,  2, 3 },
                                { 4, 5, 6 }};
            AssertExtensions.AreEqual(referX, x.ToDoubleMat(), 1e-10);
        }

        [Test]
        public void MatrixMulitplyingTest8()
        {
            //JDModel mdl = new GurobiJDModel();
            JDModel mdl = new JDModel();

            // var init.
            JDVar x = mdl.AddVar(xSize: 2, ySize: 3, lb: 0, type: JD.INTEGER);
            double[,] A = { { 7,  8 },
                            { 9,  10},
                            { 11,  12},};
            double[,] C = { { 58,  64 },
                            { 139,  154}};
            mdl += x * A == C;
            mdl += x[1, 1] == 5;
            mdl += x[1, 2] == 6;
            mdl += x[0, 0] == 1;
            //mdl += x[0, 2] == 3;
            //mdl += 

            // solve
            mdl.SetObjective(x.Sum(), JD.MAXIMIZE);
            JDTester._solver.Solve(mdl);

            // check result
            double[,] referX = { { 1,  2, 3 },
                                { 4, 5, 6 }};
            AssertExtensions.AreEqual(referX, x.ToDoubleMat(), 1e-10);
        }

        [Test]
        public void MatrixMulitplyingTest10()
        {
            //JDModel mdl = new GurobiJDModel();
            JDModel mdl = new JDModel();

            // var init.
            JDVar x = mdl.AddVar(xSize: 3, ySize: 2, lb: 0);
            double[,] A = { { 1,  2, 3 },
                                { 4, 5, 6 }};
            double[,] C = { { 58,  64 },
                            { 139,  154}};
            mdl += A * x == C;
            mdl += x[1, 1] == 10;
            //mdl += x[1, 2] == 6;
            mdl += x[0, 0] == 7;
            //mdl += x[0, 2] == 3;
            //mdl += 

            // solve
            mdl.SetObjective(x.Sum(), JD.MAXIMIZE);
            JDTester._solver.Solve(mdl);

            // check result
            double[,] referX = { { 7,  8 },
                            { 9,  10},
                            { 11,  12},};
            
            AssertExtensions.AreEqual(referX, x.ToDoubleMat(), 1e-10);
        }

        [Test]
        public void MatrixMulitplyingTest11()
        {
            //JDModel mdl = new GurobiJDModel();
            JDModel mdl = new JDModel();

            // var init.
            JDVar x = mdl.AddVar(xSize: 1, ySize: 3, lb: 0);
            //double[] A = {  3,  2,  1 };
            double[,] A = { {3}, {2}, {1} };

            double[,] C = { { 3, 6, 9 },
                            { 2, 4, 6},
                            { 1, 2, 3}};
            mdl += A * x == C;

            // solve
            mdl.SetObjective(x.Sum(), JD.MAXIMIZE);
            JDTester._solver.Solve(mdl);

            // check result
            double[,] referX = { {1, 2,  3 } };

            AssertExtensions.AreEqual(referX, x.ToDoubleMat(), 1e-10);
        }

        [Test]
        public void MatrixMulitplyingTest12()
        {
            //JDModel mdl = new GurobiJDModel();
            JDModel mdl = new JDModel();

            // var init.
            JDVar x = mdl.AddVar(xSize: 1, ySize: 3, lb: 0);
            double[] A = { 3 , 2, 1 };

            double[,] C = { { 3, 6, 9 },
                            { 2, 4, 6},
                            { 1, 2, 3}};
            mdl += A * x == C;

            // solve
            mdl.SetObjective(x.Sum(), JD.MAXIMIZE);
            JDTester._solver.Solve(mdl);

            // check result
            double[,] referX = { { 1, 2, 3 } };
            AssertExtensions.AreEqual(referX, x.ToDoubleMat(), 1e-10);
        }

        [Test]
        public void MatrixMulitplyingTest13()
        {
            //JDModel mdl = new GurobiJDModel();
            JDModel mdl = new JDModel();

            // var init.
            JDVar x = mdl.AddVar(xSize: 3, ySize: 1, lb: 0);
            double[] A = { 1, 2, 3 };

            double[,] C = { { 3, 6, 9 },
                            { 2, 4, 6},
                            { 1, 2, 3}};
            mdl += x * A == C;

            // solve
            mdl.SetObjective(x.Sum(), JD.MAXIMIZE);
            JDTester._solver.Solve(mdl);

            // check result
            double[,] referX = { { 3 }, { 2 }, { 1 } };
            AssertExtensions.AreEqual(referX, x.ToDoubleMat(), 1e-10);
        }

        [Test]
        public void ArrayTest()
        {
            //JDModel mdl = new GurobiJDModel();
            JDModel mdl = new JDModel();

            // var init.
            JDVar x = mdl.AddVar(xSize: 1, ySize: 1, lb: 0);
            double[] A = { 5 };
            mdl += x == A;
            // solve
            mdl.SetObjective(x.Sum(), JD.MAXIMIZE);
            JDTester._solver.Solve(mdl);

            // check result
            double[,] referX = { { 5 } };
            AssertExtensions.AreEqual(referX, x.ToDoubleMat(), 1e-10);
        }


        [Test]
        public void ScalarAndVectorConstr()
        {
            if (Environment.GetEnvironmentVariable("SOLVER") == "Highs") {
                Assert.Ignore("Failing in Highs - TODO fix");
            }
            //JDModel mdl = new GurobiJDModel();
            JDModel mdl = new JDModel();

            // var init.
            JDVar x = mdl.AddVar(2, 1);
            JDVar y = mdl.AddVar(1, 1);

            mdl += x == 20;
            mdl += y + y == 10 * x;
            mdl.SetObjective(y + x.Sum(), JD.MAXIMIZE);
            JDTester._solver.Solve(mdl);
            double expY = 100;
            double actY = y.ToDouble(0, 0);
            Assert.AreEqual(expY, actY, 1e-10);
        }
    }
}
