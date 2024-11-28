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

        private HighsLpSolver _highsSolver;

        private double [] _varValues;

        private double _offset;

        public HighsJDSolver() {
            _highsSolver = new HighsLpSolver();
            _highsSolver.setStringOptionValue("log_file", "highs.log");
            _highsSolver.setBoolOptionValue("log_to_console", 0);
            _varsMap = new Dictionary<int, int>();
        }

        public void AddScVar(ScVar scVar) {
            double cost = 0;
            var status = _highsSolver.addCol(cost, scVar.Lb, scVar.Ub, Array.Empty<int>(), Array.Empty<double>());
            if (status == HighsStatus.kError) {
                throw new JDException("Highs variable (column) creation error {0} for ScVar {1}", status, scVar);
            }

            int iCol = _highsSolver.getNumCol() - 1;
            _varsMap.Add(scVar.Id, iCol);
            switch (scVar.Type)
            {
                case JD.CONTINUOUS:
                    _highsSolver.changeColsIntegralityByRange(iCol, iCol, new HighsIntegrality[]{HighsIntegrality.kContinuous});
                    break;
                case JD.BINARY:
                case JD.INTEGER:
                    _highsSolver.changeColsIntegralityByRange(iCol, iCol, new HighsIntegrality[]{HighsIntegrality.kInteger});
                    break;
                default:
                    throw new JDException("Unknown var type: {0}", scVar.Type);
            }
        }

        public void SetLogger(Logger logger)
        {
            _logger = logger;
        }

        public Logger GetLogger()
        {
            return _logger;
        }

        public void Update()
        {
            // nothing to do
        }

        public void Reset()
        {
            _highsSolver.clearModel();
            _highsSolver.clearSolver();
        }

        private void addRow(double lower, double upper, List<int> indices, List<double> values, ScConstr constr) {
            HighsStatus status = _highsSolver.addRow(lower, upper, indices.ToArray(), values.ToArray());
            if (status == HighsStatus.kError) {
                string msg = string.Format("Highs constraint (row) creation error for ScConstr {0}", constr);
                _logger?.Log(new LogItem(DateTime.Now, msg, LogFlags.MODELER));
            }
        }

        public void AddConstr(ScConstr con)
        {
            double highsConstant = -con.Lhs.Constant;
            Dictionary<int, int> varId2index = new Dictionary<int, int>();
            List<int> indices = new List<int>();
            List<double> coeffs = new List<double>();
            for (int iTerm = 0; iTerm < con.Lhs.Terms.Count; iTerm++) {
                ScTerm term = con.Lhs.Terms[iTerm];
                if (varId2index.ContainsKey(term.Var.Id)) {
                    coeffs[varId2index[term.Var.Id]] += term.Coeff;
                } else {
                    varId2index.Add(term.Var.Id, indices.Count);
                    indices.Add(_varsMap[term.Var.Id]);
                    coeffs.Add(term.Coeff);
                }
            }
            switch(con.Sense)
            {
                case JD.LESS_EQUAL:
                    // JD: lhs <= 0
                    // Highs: lhsTerms <= -lhsConstant
                    addRow(double.NegativeInfinity, highsConstant, indices, coeffs, con);
                    break;
                case JD.GREATER_EQUAL:
                    // JD:  lhs => 0
                    // Highs: -lhsConstant <= lhsTerms
                    addRow(highsConstant, double.NegativeInfinity, indices, coeffs, con);
                    break;
                case JD.EQUAL:
                    // JD:  0 <= lhs <= 0
                    // Highs: -lhsConstant <= lhsTerms <= -lhsConstant
                    addRow(highsConstant, highsConstant, indices, coeffs, con);
                    break;
                default:
                    throw new JDException("Unknown sense: {0}", con.Sense);
            }
        }

        void IJDSolver.AddScVars(List<ScVar> vars)
        {
            double[] lb = new double[vars.Count];
            double[] ub = new double[vars.Count];
            HighsIntegrality[] integrality = new HighsIntegrality[vars.Count];
            int iCol = _highsSolver.getNumCol();
            for(int i = 0; i < vars.Count; i++) {
                lb[i] = vars[i].Lb;
                ub[i] = vars[i].Ub;
                _varsMap.Add(vars[i].Id, iCol + i);
                switch (vars[i].Type)
                {
                    case JD.CONTINUOUS:
                        integrality[i] = HighsIntegrality.kContinuous;
                        break;
                    case JD.BINARY:
                    case JD.INTEGER:
                        integrality[i] = HighsIntegrality.kInteger;
                        break;
                    default:
                        throw new JDException("Unknown var type: {0}", vars[i].Type);
                }
            }
            double[] cost = new double[vars.Count];
            var status = _highsSolver.addCols(cost, lb, ub, Array.Empty<int>(), Array.Empty<int>(), Array.Empty<double>());
            if (status == HighsStatus.kError) {
                throw new JDException("Highs variables (columns) creation error {0} for ScVar {1}...{2}", status, vars[0], vars.Count);
            }
            status = _highsSolver.changeColsIntegralityByRange(iCol, iCol + vars.Count - 1, integrality);
            if (status == HighsStatus.kError) {
                throw new JDException("Highs variables (columns) integrality setting error {0} for ScVar {1}...{2}", status, vars[0], vars.Count);
            }
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
            this._offset = obj.Constant;
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
            HighsModelStatus modelStatus = _highsSolver.GetModelStatus();

            Console.WriteLine("Status: " + status);
            Console.WriteLine("Model status: " + modelStatus);
            pars.Set(JD.StringParam.SOLVER_NAME, "Highs");
            int jdStatus = 0;
            if (status == HighsStatus.kOk) // optimal or suboptimal
            {
                jdStatus = 1;
                _varValues = _highsSolver.getSolution().colvalue;
            }
            pars.Set(JD.IntParam.RESULT_STATUS, jdStatus);
            pars.Set(JD.StringParam.STATUS, modelStatus.ToString());
            pars.Set(JD.DoubleParam.SOLVER_TIME, sw.Elapsed.TotalSeconds);
            if (pars.Get<int>(JD.IntParam.RESULT_STATUS) > 0)
            {
                double objValue = _highsSolver.getObjectiveValue() + _offset;
                pars.Set(JD.DoubleParam.OBJ_VALUE, objValue);
            }
        }

        public double? GetVarValue(int id)
        {
            return _varValues[_varsMap[id]];
        }

        public bool Export(string filenameWithoutExtension, string fileType) {
            throw new NotImplementedException();
        }
    }
}
