using System;

namespace StarBattleSharp
{
    /// <summary>
    /// The thread solver state for use in running through grid parameters
    /// </summary>
    class ThreadSolverState
    {
        /// <summary>
        /// The underlying BattleGrid to use in solving
        /// </summary>
        public BattleGrid BattleGrid { get; init; }

        /// <summary>
        /// The thread index (0, 1, 2, etc.)
        /// </summary>
        public int ThreadIndex { get; init; }

        /// <summary>
        /// The total number of threads
        /// </summary>
        public int ThreadCount { get; init; }

        /// <summary>
        /// A boolean to determine if the thread should be aborted
        /// </summary>
        public volatile bool abort = false;

        /// <summary>
        /// Defines the initial thread solver state
        /// </summary>
        public ThreadSolverState(
            BattleGrid battleGrid,
            int threadIndex,
            int threadCount)
        {
            BattleGrid = battleGrid;
            ThreadIndex = threadIndex;
            ThreadCount = threadCount;
        }
    }
}
