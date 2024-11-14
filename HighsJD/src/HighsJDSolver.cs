using Highs;
using JDSpace;
using JDUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace HighsJD {

    public class HighsJDSolver : IJDSolver
    {
        public bool SupportsSOS1 => false;
        public bool SupportsSOS2 => false;

        // ScVar Id -> Highs column index
        private Dictionary<int, int> _varsMap;
        private Logger _logger;

        HighsLpSolver _highsSolver;

        public HighsJDSolver() {
            _highsSolver = new HighsLpSolver();
            _varsMap = new Dictionary<int, int>();
        }

        public void AddScVar(ScVar scVar) {
            double cost = 0;
            var status = _highsSolver.addCol(cost, scVar.Lb, scVar.Ub, new int[]{}, new double[]{});
            if (status == HighsStatus.kError) {
                throw new JDException("Highs variable (column) creation error {0} for ScVar {1}", status, scVar);
            }
            //if (status == HighsStatus.kWarning) {
            //  send warning to logger
            //}

            int iCol = _highsSolver.getNumCol() - 1;
            _varsMap.Add(scVar.Id, iCol);
            switch (scVar.Type)
            {
                case JD.CONTINUOUS:
                    _highsSolver.changeColsIntegralityByRange(iCol, iCol, new HighsIntegrality[]{HighsIntegrality.kContinuous});
                    break;
                case JD.BINARY:
                    _highsSolver.changeColsIntegralityByRange(iCol, iCol, new HighsIntegrality[]{HighsIntegrality.kInteger});
                    _highsSolver.changeColBounds(iCol, 0, 1);
                    break;
                case JD.INTEGER:
                    _highsSolver.changeColsIntegralityByRange(iCol, iCol, new HighsIntegrality[]{HighsIntegrality.kInteger});
                    break;
                default:
                    throw new JDException("Unknown var type: {0}", scVar.Type);
            }
        }

        public void SetLogger(JDUtils.Logger logger)
        {
            _logger = logger;
        }

        public JDUtils.Logger GetLogger()
        {
            return _logger;
        }

        public void Update()
        {
            // nothing to do
        }

        public void Reset()
        {
            //_solver.Reset();
            // _solver.Clear();
            // _otVars.Clear();
            throw new NotImplementedException();
        }

        public void AddConstr(ScConstr con)
        {
            double highsConstant = -con.Lhs.Constant;
            int[] indices = new int[con.Lhs.Terms.Count];
            double[] coeffs = new double[con.Lhs.Terms.Count];
            for (int i = 0; i < con.Lhs.Terms.Count; i++) {
                ScTerm term = con.Lhs.Terms[i];
                indices[i] = _varsMap[term.Var.Id];
                coeffs[i] = term.Coeff;
            }
            HighsStatus rowCreateStatus;
            switch(con.Sense)
            {
                case JD.LESS_EQUAL:
                    // JD: lhs <= 0
                    // Highs: lhsTerms <= -lhsConstant
                    rowCreateStatus = _highsSolver.addRow(double.NegativeInfinity, highsConstant, indices, coeffs);
                    break;
                case JD.GREATER_EQUAL:
                    // JD:  lhs => 0
                    // Highs: -lhsConstant <= lhsTerms
                    rowCreateStatus = _highsSolver.addRow(highsConstant, double.NegativeInfinity, indices, coeffs);
                    break;
                case JD.EQUAL:
                    // JD:  0 <= lhs <= 0
                    // Highs: -lhsConstant <= lhsTerms <= -lhsConstant
                    rowCreateStatus = _highsSolver.addRow(highsConstant, highsConstant, indices, coeffs);
                    break;
                default:
                    throw new JDException("Unknown sense: {0}", con.Sense);
            }
            if (rowCreateStatus == HighsStatus.kError) {
                throw new JDException("Highs constraint (row) creation error for ScConstr {0}", con);
            }
        }

        void IJDSolver.AddScVars(List<ScVar> vars)
        {
            foreach (ScVar var in vars) AddScVar(var);
        }

        void IJDSolver.AddConstrs(List<ScConstr> cons)
        {
            foreach (ScConstr con in cons) AddConstr(con);
        }


        public void AddSOSConstr(SOSConstr sosCon)
        {
            throw new JDException("SOS constraints unsupported in OTSolvers");
        }

        public void SetObjective(ScLinExpr obj, int sense)
        {
            foreach (ScTerm term in obj.Terms)
            {
                _highsSolver.changeColCost(_varsMap[term.Var.Id], term.Coeff);
            }
            // _objective.SetOffset(obj.Constant);

            switch (sense)
            {
                case JD.MAXIMIZE:
                    _highsSolver.changeObjectiveSense(HighsObjectiveSense.kMaximize);
                    break;
                case JD.MINIMIZE:
                    _highsSolver.changeObjectiveSense(HighsObjectiveSense.kMinimize);
                    break;
                default:
                    throw new JDException("Unknown optimization sense {0}", sense);
            }
        }

        public void Optimize(JDParams pars)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            HighsStatus status = _highsSolver.run();
            sw.Stop();
            HighsSolution sol = _highsSolver.getSolution();
            HighsBasis bas = _highsSolver.getBasis();
            HighsModelStatus modelStatus = _highsSolver.GetModelStatus();

            Console.WriteLine("Status: " + status);
            Console.WriteLine("Model status: " + modelStatus);
            Console.WriteLine("Optimization value: " + _highsSolver.getObjectiveValue());
            pars.Set(JD.StringParam.SOLVER_NAME, "Highs");
            int jdStatus = 0;
            if (modelStatus == HighsModelStatus.kOptimal) // optimal or suboptimal
            {
                jdStatus = 1;
            }
            pars.Set(JD.IntParam.RESULT_STATUS, jdStatus);
            pars.Set(JD.StringParam.STATUS, modelStatus.ToString());
            pars.Set(JD.DoubleParam.SOLVER_TIME, sw.Elapsed.TotalSeconds);
            if (pars.Get<int>(JD.IntParam.RESULT_STATUS) > 0)
            {
                pars.Set(JD.DoubleParam.OBJ_VALUE, _highsSolver.getObjectiveValue());
            }
        }

        public double? GetVarValue(int id)
        {
            return _highsSolver.getSolution().colvalue[_varsMap[id]];
        }

        public bool Export(string filenameWithoutExtension, string fileType) {
            throw new NotImplementedException();
        }
    }
}
