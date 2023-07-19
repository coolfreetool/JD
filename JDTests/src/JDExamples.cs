using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JDSpace;

namespace JDSpace
{
    /// <summary>
    /// Several examples class (testing, experimenting ...).
    /// </summary>
    public class JDExamples
    {
        public static void ex1()
        {
            JDModel mdl = new JDModel();
            JDVar c = mdl.AddVar(1, 2, 0, JD.INFINITY, JD.CONTINUOUS, "c");
            JDVar i = mdl.AddVar(1, 2, 0, JD.INFINITY, JD.CONTINUOUS, "i");
            JDVar b = mdl.AddVar(1, 2, 0, JD.INFINITY, JD.BINARY, "b");

            mdl += 2 * c <= new double[] { 10, 20 };
            mdl += 3 * i <= new double[] { 30, 40 };
            mdl += 4 * b <= new double[] { 40, 50 };

            JDLinExpr obj = (c + i + b).Sum();

            mdl.SetObjective(obj, JD.MAXIMIZE);
        }
    }
}
