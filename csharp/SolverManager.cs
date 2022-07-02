using System.Threading;

namespace StarBattleSharp
{
    /// <summary>
    /// SolveManager provides basic utilities to manage solving the grid with classes
    /// </summary>
    class SolverManager
    {
        /// <summary>
        /// The solved grid instance, or null if no solution
        /// </summary>
        public GridInstance? SolvedGrid { get; protected set; }

        /// <summary>
        /// Boolean state for the solved-state of the grid
        /// </summary>
        public bool SolvedGridValid
        {
            get
            {
                return SolvedGrid != null;
            }
        }

        /// <summary>
        /// The input state containing information pertinant to the setup of the thread
        /// </summary>
        public ThreadSolverState InputState { get; private set; }

        /// <summary>
        /// Mutex to provide read-access to thread parameters for setting/reading the solve grid state
        /// </summary>
        readonly public Mutex mutex = new();

        /// <summary>
        /// Defines a new SolverManager instance with a given state
        /// </summary>
        /// <param name="solverState">thread state to define the manager with</param>
        public SolverManager(ThreadSolverState solverState)
        {
            SolvedGrid = null;
            InputState = solverState;
        }

        /// <summary>
        /// Runs the thread solver instance
        /// </summary>
        public void Run()
        {
            // Define the solve instance with the input battle grid
            GridInstance solverInst = new(InputState.BattleGrid);

            // Attempt to solve the thread
            try
            {
                // Run the solver with the given thread state
                GridInstance? bgSolved = solverInst.Solve(threadState: InputState);

                // Check if the solution is valid
                if (bgSolved != null)
                {
                    // Aquire the mutex to set the SolveGrid state
                    mutex.WaitOne();
                    SolvedGrid = bgSolved;

                    // Release the mutex
                    mutex.ReleaseMutex();
                }
            }
            catch (ThreadInterruptedException)
            {
                SolvedGrid = null;
            }
        }
    }
}
