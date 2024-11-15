using HighsJD;
using JDSpace;
using JDUtils;
using OTJD;
using System;

namespace JDCli
{
    class Program
    {
        static void Main(string[] args)
        {
            switch (args[0])
            {
                case "show":
                    show(args[1]);
                    break;
                case "solve":
                    solve(args[1]);
                    break;
                case "example1":
                    example1();
                    break;
                default:
                    printHelp();
                    break;
            }
        }

        static void printHelp() {
            Console.WriteLine("Use two args: <command> <model>:");
            Console.WriteLine("1. show <model>");
            Console.WriteLine("2. solve <model>");
            Console.WriteLine("3. example1 <model>");
        }

        static void show(string model) {
            JDModel mdl = JDModel.BuildFromFile(model);
            mdl.PrintParams();
        }

        static void solve(string model) {
            Console.WriteLine("Reading model {0}", model);
            JDModel mdl = JDModel.BuildFromFile(model);
            mdl.PrintParams();
            Console.WriteLine("Setting up solver");
            IJDSolver solver = getSolverFromEnv();
            setSolverLogging(solver);
            solver.Solve(mdl);
        }

        static void example1() {
            Console.WriteLine("Setting up solver");
            IJDSolver solver = getSolverFromEnv();
            setSolverLogging(solver);
            JDModel mdl = new JDModel();
            Console.WriteLine("Print model params:");
            mdl.PrintParams();
            // init var
            JDVar x = mdl.AddVar(2, 3, -50, 50, JD.CONTINUOUS);
            // constr.
            double[,] C1 = { { 30, 30, 30 } };
            double[,] C2 = { { 1, 2, 3 } };
            JDLinExpr colSum = x.Sum(0);
            mdl += (colSum == C1);
            mdl += (x[1, 1, 0, 2] == C2);
            // solve
            mdl.SetObjective(x.Sum(), JD.MAXIMIZE);
            solver.Solve(mdl);
            Console.WriteLine("Evaluated matrix 'x':");
            x.Print();
        }

        static void setSolverLogging(IJDSolver solver) {
            Logger logger = new Logger();
            logger.Register(new ConsolLogClient(), Logger.AllFlags);
            solver.SetLogger(logger);
        }

        static IJDSolver getSolverFromEnv() {
            string solverVar = Environment.GetEnvironmentVariable("SOLVER");
            switch (solverVar)
            {
                case "CBC":
                    return new CbcJDSolver();
                case "SCIP":
                    return new ScipJDSolver();
                case "Glpk":
                    return new GlpkJDSolver();
                case "SAT":
                    return new SatJDSolver();
                case "Highs":
                    return new HighsJDSolver();
                default:
                    Console.WriteLine("No solver specified (CBC, SCIP, Glpk, SAT), using default SCIP");
                    return new ScipJDSolver();
            }
        }
    }
}
