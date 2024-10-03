using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using JDSpace;
using Highs;
using System.Collections;

namespace Tests
{
    [TestFixture]
    internal class ExampleTest
    {
        [Test]
        public void PassLp1Test()
        {      
            double[] cc = {1, -2};
            double[] cl = {0, 0};
            double[] cu = {10, 10};
            double[] rl = {0, 0};
            double[] ru = {2, 1};
            int[] astart = {0, 2};
            int[] aindex = {0, 1, 0, 1};
            double[] avalue = {1, 2, 1, 3};
            HighsObjectiveSense sense = HighsObjectiveSense.kMinimize;
            double offset = 0;
            HighsMatrixFormat a_format = HighsMatrixFormat.kColwise;

            HighsModel model = new HighsModel(cc, cl, cu, rl, ru, astart, aindex, avalue, null, offset, a_format, sense);

            HighsLpSolver solver = new HighsLpSolver();

            HighsStatus status = solver.passLp(model);
            status = solver.run();
            HighsSolution sol = solver.getSolution();
            HighsBasis bas = solver.getBasis();
            HighsModelStatus modelStatus = solver.GetModelStatus();
            
            Console.WriteLine("Status: " + status);
            Console.WriteLine("Modelstatus: " + modelStatus);
        
            for (int i=0; i<sol.rowvalue.Length; i++) {
                Console.WriteLine("Activity for row " + i + " = " + sol.rowvalue[i]);
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
        }

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
                Console.WriteLine("Activity for row " + i + " = " + sol.rowvalue[i]);
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
        }
    }
}
