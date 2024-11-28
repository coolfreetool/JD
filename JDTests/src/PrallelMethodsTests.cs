using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using JDSpace;
using System.Diagnostics;
using JDUtils;

namespace JDSpace
{
    /// <summary>
    /// Testing parallel methods improving of JD framework.
    /// </summary>
    [TestFixture]
    public class PrallelMethodsTests
    {
        [SetUp]
        public void Init()
        {
            JDTester.ResetSolver();
        }

        /// <summary>
        /// Big var matrix creating.
        /// </summary>
        [Test]
        public void CreateVars()
        {
            Stopwatch sw = new Stopwatch();
            JDModel mdl = new JDModel();

            int size = 800;
            sw.Start();
            JDVar x = mdl.AddVar(size, size, 0, 100, JD.CONTINUOUS);
            sw.Stop();
            Console.WriteLine("Vars creating {0} s", sw.Elapsed.TotalSeconds);

            sw.Restart();
            mdl += x <= 5;
            sw.Stop();
            Console.WriteLine("Constrs creating {0} s", sw.Elapsed.TotalSeconds);

            sw.Restart();
            JDLinExpr obj = x.Sum();
            sw.Stop();
            Console.WriteLine("Obj creating {0} s", sw.Elapsed.TotalSeconds);

            mdl.SetObjective(obj, JD.MAXIMIZE);
            Logger logger = new Logger();
            logger.Register(new ConsolLogClient(), Logger.AllFlags);
            JDTester._solver.SetLogger(logger);
            JDTester._solver.Solve(mdl);
        }
    }
}
