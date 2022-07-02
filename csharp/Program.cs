using System;
using System.IO;
using System.Threading;

namespace StarBattleSharp
{
    /// <summary>
    /// Program is the main program's class
    /// </summary>
    class Program
    {
        /// <summary>
        /// Runs the main program
        /// </summary>
        /// <param name="argv">input arguments</param>
        /// <returns>return code - 0 for success; otherwise fail</returns>
        static int Main(string[] argv)
        {
            // Define the input file parameter
            string inputFile;

            // Determine if we take the input from the arguments or as the default file based on the current directory
            if (argv.Length < 1)
            {
                inputFile = Path.Combine(Directory.GetCurrentDirectory(), "input.txt");
            }
            else
            {
                inputFile = argv[0];
            }
            
            // Ensure that the file exists
            if (!File.Exists(inputFile))
            {
                Console.WriteLine($"Cannot find input file {inputFile}");
                return -1;
            }

            // Define the number of threads to use and the respective variable parameters
            int numThreads = 1;
            Thread[] threads = new Thread[numThreads];
            SolverManager[] results = new SolverManager[numThreads];

            // Define the battle grid and print
            BattleGrid battleGrid = BattleGrid.FromFile(inputFile);
            Console.WriteLine("Input Grid:");
            Console.WriteLine(battleGrid.ToString());

            // Start the solve parameters
            Console.WriteLine();
            Console.WriteLine($"Solving on {numThreads} threads...");

            // Save the start time to be able to check the total elapsed time
            DateTime startTime = DateTime.UtcNow;

            // Initialize and start each of the threads
            for (int i = 0; i < numThreads; ++i)
            {
                ThreadSolverState state = new(
                    battleGrid: battleGrid,
                    threadIndex: i,
                    threadCount: numThreads);

                results[i] = new SolverManager(state);
                threads[i] = new Thread(new ThreadStart(results[i].Run));
                threads[i].Start();
            }

            // Define the resulting solved grid and continue conditions
            GridInstance? solvedGrid = null;
            bool anyRunning = true;

            // Loop while the problem hasn't been solved
            while (anyRunning && solvedGrid == null)
            {
                // Check if any thread is still running
                anyRunning = false;
                for (int i = 0; i < numThreads; ++i)
                {
                    if (threads[i].IsAlive)
                    {
                        anyRunning = true;
                        break;
                    }
                }

                // Check if any thread has found the solution
                for (int i = 0; i < numThreads; ++i)
                {
                    results[i].mutex.WaitOne();
                    if (results[i].SolvedGridValid)
                    {
                        solvedGrid = results[i].SolvedGrid;
                    }
                    results[i].mutex.ReleaseMutex();
                }

                // Sleep to wait until the next loop check
                Thread.Sleep(100);
            }

            // Set each of the remaining threads to abort
            for (int i = 0; i < numThreads; ++i)
            {
                if (threads[i].IsAlive)
                {
                    results[i].InputState.abort = true;
                }
            }

            // Join the remaining threads with the main thread
            for (int i = 0; i < numThreads; ++i)
            {
                threads[i].Join();
            }

            // Print an empty line
            Console.WriteLine();

            // Print the solution if found; otherwise print to solution foudn
            if (solvedGrid != null)
            {
                Console.WriteLine("Solution:");
                Console.WriteLine(solvedGrid.ToString());

                TimeSpan dT = DateTime.UtcNow - startTime;

                Console.Write("Solved in ");
                if (dT.TotalSeconds <= 90)
                {
                    Console.WriteLine($"{dT.TotalSeconds} seconds");
                }
                else
                {
                    Console.WriteLine($"{dT.TotalMinutes} minutes");
                }

                return 0;
            }
            else
            {
                Console.WriteLine("No solution found");
                return 1;
            }
        }
    }
}
