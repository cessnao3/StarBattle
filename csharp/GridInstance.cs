using System;
using System.Linq;

namespace StarBattleSharp
{
    class GridInstance : BattleGrid
    {
        /// <summary>
        /// Constant to provide the number of stars per row/col/shape
        /// </summary>
        int StarsCountDesired { get; init; }


        /// <summary>
        /// Determines if a given cell contains a star or not
        /// </summary>
        readonly bool[] starPositions;

        /// <summary>
        /// Determines if a given cell can have a star in it or not
        /// </summary>
        readonly bool[] validCells;

        /// <summary>
        /// Contains the current star count in the total grid
        /// </summary>
        int starCount;

        /// <summary>
        /// Determines the star count in a given row
        /// </summary>
        readonly int[] starRowCount;

        /// <summary>
        /// Determines the star count in a given column
        /// </summary>
        readonly int[] starColCount;

        /// <summary>
        /// Determines the star count in a given shape
        /// </summary>
        readonly int[] starShapeCount;

        /// <summary>
        /// Determines the number of free cells in a given row
        /// </summary>
        readonly int[] freeRowCount;

        /// <summary>
        /// Determines the number of free cells in a given column
        /// </summary>
        readonly int[] freeColCount;

        /// <summary>
        /// Determines the number of free cells in a given shape
        /// </summary>
        readonly int[] freeShapeCount;

        /// <summary>
        /// Creates a new grid instance based on the underlying BattleGrid
        /// </summary>
        /// <param name="battleGrid">the BattleGrid to base the grid solver instance off of</param>
        public GridInstance(BattleGrid battleGrid) : base(battleGrid)
        {
            // Define the star count per parameter
            StarsCountDesired = gridInstanceType switch
            {
                GridType.Normal => 2,
                GridType.Large => 3,
                _ => throw new Exception("Unknown GridType")
            };

            // Define the star positions
            starPositions = new bool[Grid.Length];
            validCells = new bool[Grid.Length];
            for (int i = 0; i < starPositions.Length; ++i)
            {
                starPositions[i] = false;
                validCells[i] = true;
            }

            // Define the number of stars
            starCount = 0;

            // Define the number of row/column values
            starRowCount = new int[Size];
            starColCount = new int[Size];
            for (int i = 0; i < Size; ++i)
            {
                starRowCount[i] = 0;
                starColCount[i] = 0;
            }

            // Define the free parameters
            freeRowCount = new int[Size];
            freeColCount = new int[Size];

            // Setup the free and star count parameters
            starShapeCount = new int[Size];
            freeShapeCount = new int[Size];

            for (int i = 0; i < Size; ++i)
            {
                starShapeCount[i] = 0;
                freeShapeCount[i] = 0;
            }

            for (int i = 0; i < Grid.Length; ++i)
            {
                freeShapeCount[Grid[i]] += 1;
            }

            for (int i = 0; i < Size; ++i)
            {
                freeColCount[i] = Size;
                freeRowCount[i] = Size;
            }
        }

        /// <summary>
        /// Creates a GridInstance from another grid instance
        /// Copies all solver-specific instance parameters over, while using shallow references for the remaining items
        /// </summary>
        /// <param name="other">the other GridInstance to copy parameters from</param>
        protected GridInstance(GridInstance other) : base(other)
        {
            StarsCountDesired = other.StarsCountDesired;

            starPositions = (bool[])other.starPositions.Clone();
            validCells = (bool[])other.validCells.Clone();

            starCount = other.starCount;
            starRowCount = (int[])other.starRowCount.Clone();
            starColCount = (int[])other.starColCount.Clone();
            starShapeCount = (int[])other.starShapeCount.Clone();

            freeRowCount = (int[])other.freeRowCount.Clone();
            freeColCount = (int[])other.freeColCount.Clone();
            freeShapeCount = (int[])other.freeShapeCount.Clone();
        }

        /// <summary>
        /// Copies state properties from another GridInstance to the current instance to prevent
        /// unnecessary re-allocation of memory
        /// Note that the base BattleGrid for each must be the same for valid results
        /// </summary>
        /// <param name="other">the other GridInstance to copy parameters from</param>
        void UpdateValues(GridInstance other)
        {
            // Copy star and cell parmaeters
            Array.Copy(other.starPositions, starPositions, Grid.Length);
            Array.Copy(other.validCells, validCells, Grid.Length);

            // Copy the star count
            starCount = other.starCount;

            // Copy count parameters
            Array.Copy(other.starRowCount, starRowCount, Size);
            Array.Copy(other.starColCount, starColCount, Size);

            Array.Copy(other.freeRowCount, freeRowCount, Size);
            Array.Copy(other.freeColCount, freeColCount, Size);

            Array.Copy(other.starShapeCount, starShapeCount, Size);
            Array.Copy(other.freeShapeCount, freeShapeCount, Size);
        }

        void AddStar(int row, int col)
        {
            // Extract the row/col index
            int rowColIndex = RowColToIndex(row, col);

            // Return if already has a star
            if (starPositions[rowColIndex])
            {
                return;
            }

            // Determine the shape ID
            int shape_id = GetShapeID(rowColIndex);

            // Set the resulting parameters
            starPositions[rowColIndex] = true;
            starShapeCount[shape_id] += 1;
            starRowCount[row] += 1;
            starColCount[col] += 1;
            starCount += 1;

            // Loop over each direction value
            int row_end = Math.Min(Size, row + 2);
            int col_end = Math.Min(Size, col + 2);
            for (int row_i = Math.Max(0, row - 1); row_i < row_end; ++row_i)
            {
                for (int col_i = Math.Max(0, col - 1); col_i < col_end; ++col_i)
                {
                    int index = RowColToIndex(row_i, col_i);

                    if (validCells[index])
                    {
                        validCells[index] = false;
                        freeShapeCount[GetShapeID(index)] -= 1;
                        freeRowCount[row_i] -= 1;
                        freeColCount[col_i] -= 1;
                    }
                }
            }

            // Check if the current shape can be marked as full
            if (starShapeCount[shape_id] == StarsCountDesired)
            {
                int count = ShapeIndices[shape_id].Length;
                for (int i = 0; i < count; ++i)
                {
                    int index = ShapeIndices[shape_id][i];
                    if (validCells[index])
                    {
                        validCells[index] = false;
                        freeShapeCount[shape_id] -= 1;
                        freeRowCount[IndexToRow(index)] -= 1;
                        freeColCount[IndexToCol(index)] -= 1;
                    }
                }
            }

            // Check if the current row can be marked as full
            if (starRowCount[row] == StarsCountDesired)
            {
                for (int k = 0; k < Size; ++k)
                {
                    int index = RowColToIndex(row, k);

                    if (validCells[index])
                    {
                        validCells[index] = false;
                        freeShapeCount[GetShapeID(index)] -= 1;
                        freeRowCount[row] -= 1;
                        freeColCount[k] -= 1;
                    }
                }
            }

            // Check if the current column can be marked as full
            if (starColCount[col] == StarsCountDesired)
            {
                for (int k = 0; k < Size; ++k)
                {
                    int index = RowColToIndex(k, col);

                    if (validCells[index])
                    {
                        validCells[index] = false;
                        freeShapeCount[GetShapeID(index)] -= 1;
                        freeRowCount[k] -= 1;
                        freeColCount[col] -= 1;
                    }
                }
            }
        }

        /// <summary>
        /// Attempts to solve the Grid
        /// </summary>
        /// <param name="threadState">optional threadState parameters for solving with a thread state; otherwise null</param>
        /// <returns>solved GridInstance if a solution exists; otherwise null</returns>
        public GridInstance? Solve(ThreadSolverState? threadState = null)
        {
            // Preallocate grid parameters for each star
            GridInstance[] grids = Enumerable.Range(0, StarsCountDesired * Size + 1)
                .Select(_ => new GridInstance(this))
                .ToArray();

            // Determine the starting index based on the threadState parameter
            int startIndex = threadState?.ThreadIndex ?? 0;

            // Call the recursive SolveGrid function
            return grids[0].SolveGrid(
                startIndex: startIndex,
                grids: grids,
                threadState: threadState);
        }

        /// <summary>
        /// Recursive SolveGrid function used to find the Grid solution
        /// </summary>
        /// <param name="startIndex">The starting index to use to ignore prior solution parameters</param>
        /// <param name="grids">preallocated arrays for each star level</param>
        /// <param name="threadState">optional threadState parameters for solving with a thread state; otherwise null</param>
        /// <returns>solved GridInstance if a solution exists; otherwise null</returns>
        protected GridInstance? SolveGrid(int startIndex, GridInstance[] grids, ThreadSolverState? threadState = null)
        {
            // Check for completion
            if (starCount == StarsCountDesired * Size)
            {
                return this;
            }

            // Check for unable to complete parameters

            for (int i = 0; i < Size; ++i)
            {
                if (freeColCount[i] < StarsCountDesired - starColCount[i])
                {
                    return null;
                }
                else if (freeRowCount[i] < StarsCountDesired - starRowCount[i])
                {
                    return null;
                }
                else if (freeShapeCount[i] < StarsCountDesired - starShapeCount[i])
                {
                    return null;
                }
            }

            // Determine the preallocated GridInstance associated with the current star count
            GridInstance inst = grids[starCount + 1];

            // Determine the loop skip parameter for the 0-star case for threading based on the threadState
            // Otherwise, 1 will be used for no thread state or any other star count parameters
            int loopSkipI = (starCount == 0 && threadState != null) ? threadState.ThreadCount : 1;

            // Loop through the entire grid
            for (int i = startIndex; i < Grid.Length; i += loopSkipI)
            {
                int row = IndexToRow(i);
                if (row > 0 && starRowCount[row - 1] < StarsCountDesired)
                {
                    return null;
                }

                // Check if the current cell can have a star in it
                if (validCells[i])
                {
                    // Check if we need to abort the current thread
                    if (threadState != null && threadState.abort)
                    {
                        return null;
                    }

                    // Update the grid instance to the current grid parameters
                    inst.UpdateValues(this);

                    // Add the star
                    inst.AddStar(
                        row: IndexToRow(i),
                        col: IndexToCol(i));

                    // Attempt ot solve the grid
                    GridInstance? solve_check = inst.SolveGrid(
                        startIndex: i,
                        grids: grids,
                        threadState: threadState);

                    // Check if a solution was found
                    if (solve_check != null)
                    {
                        return solve_check;
                    }
                }
            }

            // Return null for no solution found
            return null;
        }

        /// <summary>
        /// Provides the character associated with the current row/column, based on the following rules:
        ///   '*' if there is a star
        ///   'o' if the cell is blocked and cannot contain a star
        /// Otherwise the base class character is used
        /// </summary>
        /// <param name="row">row index</param>
        /// <param name="col">column index</param>
        /// <returns>character associated with the current cell</returns>
        protected override char IndexChar(int row, int col)
        {
            if (starPositions[RowColToIndex(row, col)])
            {
                return '*';
            }
            else if (!validCells[RowColToIndex(row, col)])
            {
                return 'o';
            }
            else
            {
                return base.IndexChar(row, col);
            }
        }
    }
}
