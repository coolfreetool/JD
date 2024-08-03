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
    /// JDModel testing class.
    /// </summary>
    [TestFixture]
    public class JDModelTests
    {
        [SetUp]
        public void Init()
        {
            JDTester.ResetSolver();
        }

        /// <summary>
        /// Join two JDModels (with ComposedConstants).
        /// </summary>
        [Test]
        public void CloneModelTest()
        {
            JDModel mdl = new JDModel();
            JDVar x = mdl.AddVar(xSize: 3, lb: 0);
            mdl += 7 + x <= 17;
            mdl.SetObjective(x.Sum(), JD.MAXIMIZE);
            JDModel mdlClone = mdl.Clone() as JDModel;
            JDTester._solver.Solve(mdl);
            JDTester.ResetSolver();
            JDTester._solver.Solve(mdlClone);
            // Test - results are equal
            Assert.AreEqual(10.0, mdl.ConVars[0].Value, 1e-10);
            Assert.AreEqual(10.0, mdlClone.ConVars[0].Value, 1e-10);
            // models are distinct
            Assert.AreEqual(false, mdl.Equals(mdlClone));
            // variables are distinct
            Assert.AreEqual(false, mdl.ConVars[0].Equals(mdlClone.ConVars[0]));
        }

        /// <summary>
        /// Join two JDModels.
        /// </summary>
        [Test]
        public void JoinTwoModelsTest()
        {
            JDModel mdl = new JDModel();
            JDVar x = mdl.AddVar(xSize: 3, lb: 0);
            mdl += 7 + x <= 17;
            mdl.SetObjective(x.Sum(), JD.MAXIMIZE);
            JDModel mdl2Add = new JDModel(firstVarId: mdl.Vars.Count + 1,
                firstConstrId: mdl.Constrs.Count + 1);
            JDVar y = mdl2Add.AddVar(xSize: 4, lb: 0, type: JD.INTEGER);
            mdl2Add += 3 * y <= 21.3;
            mdl2Add.SetObjective(y.Sum(), JD.MAXIMIZE);
            mdl.Add(mdl2Add);
            JDTester._solver.Solve(mdl);
            double[,] xRefer = JD.ConstantMatrix(10.0, x.XSize, x.YSize);
            double[,] yRefer = JD.ConstantMatrix(7.0, y.XSize, y.YSize);
            AssertExtensions.AreEqual(xRefer, x.ToDoubleMat(), 1e-10);
            AssertExtensions.AreEqual(yRefer, y.ToDoubleMat(), 1e-10);
        } 

        /// <summary>
        /// Join two JDModels (with ComposedConstants).
        /// </summary>
        [Test]
        public void JoinTwoComposedModelsTest1()
        {
            JDModel mdl = new JDModel(dataLoadable: true);
            JDVar x = mdl.AddVar(xSize: 3, lb: 0);
            ComposedConstant c1 = new NamedConst(7);
            mdl += c1 + x <= 17;
            mdl.SetObjective(x.Sum(), JD.MAXIMIZE);
            JDModel mdl2Add = new JDModel(firstVarId: mdl.Vars.Count + 1,
                firstConstrId: mdl.Constrs.Count + 1, dataLoadable: true);
            JDVar y = mdl2Add.AddVar(xSize: 4, lb: 0, type: JD.INTEGER);
            ComposedConstant c2 = new NamedConst(3);
            mdl2Add += c2 * y <= 21.3;
            mdl2Add.SetObjective(y.Sum(), JD.MAXIMIZE);
            mdl.Add(mdl2Add);
            JDTester._solver.Solve(mdl);
            double[,] xRefer = JD.ConstantMatrix(10.0, x.XSize, x.YSize);
            double[,] yRefer = JD.ConstantMatrix(7.0, y.XSize, y.YSize);
            AssertExtensions.AreEqual(xRefer, x.ToDoubleMat(), 1e-10);
            AssertExtensions.AreEqual(yRefer, y.ToDoubleMat(), 1e-10);
        }

        /// <summary>
        /// Join two JDModels (with ComposedConstants).
        /// </summary>
        [Test]
        public void JoinTwoComposedModelsTest2()
        {
            JDModel mdl = new JDModel(dataLoadable: true);
            JDVar x = mdl.AddVar(xSize: 1, lb: 0);
            ComposedConstant c1 = new NamedConst("c1", 7);
            mdl += c1 + x <= 17;
            mdl.SetObjective(x.Sum(), JD.MAXIMIZE);
            JDModel mdl2Add = new JDModel(firstVarId: mdl.Vars.Count + 1,
                firstConstrId: mdl.Constrs.Count + 1, dataLoadable: true);
            JDVar y = mdl2Add.AddVar(xSize: 4, lb: 0, type: JD.INTEGER);
            ComposedConstant c2 = new NamedConst("c2", 3);
            mdl2Add += c2 * y <= 21.3;
            mdl2Add.SetObjective(y.Sum(), JD.MAXIMIZE);
            mdl.Add(mdl2Add);
            JDTester._solver.Solve(mdl);
            double[,] xRefer1 = JD.ConstantMatrix(10.0, x.XSize, x.YSize);
            double[,] yRefer1 = JD.ConstantMatrix(7.0, y.XSize, y.YSize);
            AssertExtensions.AreEqual(xRefer1, x.ToDoubleMat(), 1e-10);
            AssertExtensions.AreEqual(yRefer1, y.ToDoubleMat(), 1e-10);

            Dictionary<string, double> dic = new Dictionary<string, double>()
            {
                {"c1", 10},
                {"c2", 7}
            };
            mdl.LoadDataValues(dic);
            JDTester.ResetSolver();
            JDTester._solver.Solve(mdl);
            // check result
            double[,] xRefer2 = JD.ConstantMatrix(7.0, x.XSize, x.YSize);
            double[,] yRefer2 = JD.ConstantMatrix(3.0, y.XSize, y.YSize);
            AssertExtensions.AreEqual(xRefer2, x.ToDoubleMat(), 1e-10);
            AssertExtensions.AreEqual(yRefer2, y.ToDoubleMat(), 1e-10);

        } 
    }
}
