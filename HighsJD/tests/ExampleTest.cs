using System;
using NUnit.Framework;
using Highs;

namespace Tests
{
    [TestFixture]
    internal class ExampleTest
    {
        [Test]
        public void PassMip1Test()
        {      
            double[] cc = {1, 1}; // optim function koefs.
            double[] cl = {2.1, 2.1}; // vars lower bounds
            double[] cu = {8.5, 10.1}; // vars upper bounds
            int[] varTypes = {1, 0}; // 0 - continuous, 1 - integer
            int[] astart = {0, 2};
            int[] aindex = {0, 1, 0};
            double[] avalue = {1, 1, 1};
            double[] rl = {5.2, 2};
            double[] ru = {5.2, 4.5};
            HighsObjectiveSense sense = HighsObjectiveSense.kMaximize;
            double offset = 0;
            HighsMatrixFormat a_format = HighsMatrixFormat.kColwise;

            HighsModel model = new HighsModel(cc, cl, cu, rl, ru, astart, aindex, avalue, varTypes, offset, a_format, sense);

            HighsLpSolver solver = new HighsLpSolver();

            solver.passMip(model);
            HighsStatus status = solver.run();
            HighsSolution sol = solver.getSolution();
            HighsBasis bas = solver.getBasis();
            HighsModelStatus modelStatus = solver.GetModelStatus();

            Console.WriteLine("Status: " + status);
            Console.WriteLine("Modelstatus: " + modelStatus);
            Console.WriteLine("Optimization value: " + solver.getObjectiveValue());

            for (int i=0; i<sol.rowvalue.Length; i++) {
                Console.WriteLine("Value x" + i + " = " + sol.rowvalue[i]);
            }
            for (int i=0; i<sol.coldual.Length; i++) {
                Console.WriteLine("Reduced cost x[" + i + "] = " + sol.coldual[i]);
            }
            for (int i=0; i<sol.rowdual.Length; i++) {
                Console.WriteLine("Dual value for row " + i + " = " + sol.rowdual[i]);
            }
            for (int i=0; i<sol.colvalue.Length; i++) {
                Console.WriteLine("x" + i + " = " + sol.colvalue[i] + " is " + bas.colbasisstatus[i]);
            }
            Assert.AreEqual(3, sol.colvalue[0], "x0 value");
            Assert.AreEqual(2.2, sol.colvalue[1], "x1 value");
        }


        [Test]
        public void PassMip2Test()
        {
            HighsLpSolver solver = new HighsLpSolver();

            // Define x0 var
            solver.addCol(1, 0, 100, new int[]{}, new double[]{});

            // Define x1 var
            solver.addCol(1, 0, 200, new int[]{}, new double[]{});
            solver.changeColsIntegralityByRange(1, 1, new HighsIntegrality[]{HighsIntegrality.kInteger});

            // Define x2 var
            solver.addCol(1, 0, 300, new int[]{}, new double[]{});
            solver.changeObjectiveSense(HighsObjectiveSense.kMaximize);

            solver.addRow(double.NegativeInfinity, 115.9, new int[]{1}, new double[]{1});
            solver.addRow(double.NegativeInfinity, 250.5, new int[]{2}, new double[]{1});
            
            HighsStatus status = solver.run();
            HighsSolution sol = solver.getSolution();
            HighsBasis bas = solver.getBasis();
            HighsModelStatus modelStatus = solver.GetModelStatus();
            
            Console.WriteLine("Status: " + status);
            Console.WriteLine("Modelstatus: " + modelStatus);
            Console.WriteLine("Optimization value: " + solver.getObjectiveValue());
        
            for (int i=0; i<sol.colvalue.Length; i++) {
                Console.WriteLine("x" + i + " = " + sol.colvalue[i] + " is " + bas.colbasisstatus[i]);
            }
            Assert.AreEqual(100, sol.colvalue[0], "x0 value");
            Assert.AreEqual(115, sol.colvalue[1], "x1 value");
            Assert.AreEqual(250.5, sol.colvalue[2], "x2 value");
        }
    }
}
