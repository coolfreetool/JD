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
            if (args.Length != 2) {
                printHelp();
                return;
            }
            switch (args[0])
            {
                case "show":
                    show(args[1]);
                    break;
                case "solve":
                    solve(args[1]);
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
                default:
                    Console.WriteLine("No solver specified (CBC, SCIP, Glpk, SAT), using default SCIP");
                    return new ScipJDSolver();
            }
        }
    }
}
