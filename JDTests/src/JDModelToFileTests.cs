using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JDSpace;
using NUnit.Framework;
using JDUtils;

namespace JDSpace
{
    [TestFixture]
    public class JDModelToFileTests
    {
        private static string __file = "model";

        [SetUp]
        public void Init()
        {
            JDTester.ResetSolver();
        }

        [Test]
        public void SimpleVar()
        {
            string _file = "model";
            JDModel mdl = new JDModel();
            JDVar a = mdl.AddVar(1, 1);
            
            
            mdl += a <= 5;

            mdl.SetObjective(a.Sum(), JD.MAXIMIZE);
            mdl.SaveToFile(_file);
            LoadSolveSave(_file);
            JDModel mdl2 = JDModel.BuildFromFile(__file);
            Assert.AreEqual(5, mdl2.ConVars[0].Value, 0);
        }

        private void LoadSolveSave(string _file)
        {
            JDModel mdl = JDModel.BuildFromFile(_file);
            JDTester._solver.Solve(mdl);
            mdl.SaveToFile(_file);
        }
    }
}
