using System;
using System.Collections.Generic;
using System.Text;

namespace StarBattleSharp
{
    /// <summary>
    /// Provides an underlying BattleGrid to determine basic validity of a given StarBattle grid
    /// and to associate cells with shapes
    /// </summary>
    class BattleGrid
    {
        /// <summary>
        /// Defines the types of grid sizes that may be used
        /// </summary>
        public enum GridType
        {
            Unknown = 0,
            Normal = 1,
            Large = 2
        };

        /// <summary>
        /// Defines the grid isntance type to use
        /// </summary>
        public readonly GridType gridInstanceType;

        /// <summary>
        /// Defines the minimum cells allowed per shape
        /// </summary>
        protected int MinShapeCells { get; init; }

        /// <summary>
        /// Defines the grid size for a normal grid
        /// </summary>
        const int gridSizeNormal = 10;

        /// <summary>
        /// Defines the grid size for a large grid
        /// </summary>
        const int gridSizeLarge = 14;

        /// <summary>
        /// Provides the resulting size interface of the battle grid
        /// </summary>
        public int Size { get; init; }

        /// <summary>
        /// Defines the actual grid with the shape ID's for each of the cells
        /// </summary>
        public int[] Grid { get; init; }

        /// <summary>
        /// Provides the cell indices associated with each shape
        /// </summary>
        public int[][] ShapeIndices { get; init; }

        /// <summary>
        /// Initializes a BattleGrid from another grid
        /// </summary>
        /// <param name="other"></param>
        public BattleGrid(BattleGrid other)
        {
            // Do a soft-copy of the grid parameters for the grid and the shape indices
            Grid = other.Grid;
            ShapeIndices = other.ShapeIndices;
            gridInstanceType = other.gridInstanceType;
            Size = other.Size;
            MinShapeCells = other.MinShapeCells;
        }

        /// <summary>
        /// Initializes the grid from an input array of shape indices
        /// </summary>
        /// <param name="gridInput">Initialzies the grid from an input array of row-major shape indices, starting at 0 to Size</param>
        /// <param name="gridType">Defines the grid type to use for the given grid parameters</param>
        public BattleGrid(int[] gridInput)
        {
            // Save the resulting grid type parameter
            gridInstanceType = gridInput.Length switch
            {
                gridSizeNormal * gridSizeNormal => GridType.Normal,
                gridSizeLarge * gridSizeLarge => GridType.Large,
                _ => throw new ArgumentException($"unknown grid type for input length of {gridInput.Length}")
            };

            // Init the minimum shape cell count
            MinShapeCells = gridInstanceType switch
            {
                GridType.Normal => 2,
                GridType.Large => 3,
                _ => throw new Exception("Unknown GridType")
            };

            // Init the GridSize
            Size = gridInstanceType switch
            {
                GridType.Normal => gridSizeNormal,
                GridType.Large => gridSizeLarge,
                _ => throw new Exception("Unknown GridType")
            };

            // Save the iniput grid to the current grid
            Grid = gridInput ?? throw new ArgumentException("input grid must not be null");

            // Define a dictionary for shape ID parameters
            Dictionary<int, int> shape_id_count = new();

            // Check for validity of the grid
            for (int i = 0; i < Size; ++i)
            {
                for (int j = 0; j < Size; ++j)
                {
                    // Extract the current shape ID
                    int sid = GetShapeID(i, j);

                    // Add the shape to the dictionary if it doesn't already exist
                    if (!shape_id_count.ContainsKey(sid))
                    {
                        shape_id_count.Add(sid, 0);
                    }

                    // Increment the shape ID count
                    shape_id_count[sid] += 1;

                    // Check for a matching neighboring cell, required for validity
                    bool any_match = false;
                    if (i > 0)
                    {
                        any_match |= GetShapeID(i - 1, j) == sid;
                    }
                    if (i < Size - 1)
                    {
                        any_match |= GetShapeID(i + 1, j) == sid;
                    }
                    if (j > 0)
                    {
                        any_match |= GetShapeID(i, j - 1) == sid;
                    }
                    if (j < Size - 1)
                    {
                        any_match |= GetShapeID(i, j + 1) == sid;
                    }

                    // Raise error for invalid parameters
                    if (!any_match)
                    {
                        throw new ArgumentException($"input grid shape ID {sid} has isolated cell");
                    }
                }
            }

            // Check that the correct number of shapes were found with each having at least three cells
            if (shape_id_count.Count != Size)
            {
                throw new ArgumentException($"input grid contains unexpected {shape_id_count.Count} number of cells");
            }

            foreach (KeyValuePair<int, int> pair in shape_id_count)
            {
                if (pair.Value < MinShapeCells)
                {
                    throw new ArgumentException($"shape {pair.Key} has {pair.Value} cells < {MinShapeCells} minimum");
                }
            }

            // Save the shape IDs
            List<int> shapeIDs = new(shape_id_count.Keys);
            shapeIDs.Sort();
            for (int i = 0; i < Size; ++i)
            {
                if (shapeIDs[i] != i)
                {
                    throw new ArgumentException("invalid shape ID {shapeIDs[i]} is not contiguous");
                }
            }

            // Update the shape index parameters
            List<List<int>> mShapeIndices = new();
            for (int i = 0; i < Size; ++i)
            {
                mShapeIndices.Add(new List<int>());
            }

            for (int i = 0; i < Grid.Length; ++i)
            {
                mShapeIndices[Grid[i]].Add(i);
            }

            // Save the shape indices to the main BattleGrid class parameter
            ShapeIndices = new int[Size][];
            for (int i = 0; i < Size; ++i)
            {
                ShapeIndices[i] = mShapeIndices[i].ToArray();
            }
        }

        /// <summary>
        /// Converts from a row-col index to an inline index
        /// </summary>
        /// <param name="row">The 0-indexed row</param>
        /// <param name="col">The 0-indexed column</param>
        /// <returns>The resulting index in the BattleGrid arrays</returns>
        protected int RowColToIndex(int row, int col)
        {
            return row * Size + col;
        }

        /// <summary>
        /// Converts an inline index to a row index
        /// </summary>
        /// <param name="i">index</param>
        /// <returns>row index</returns>
        protected int IndexToRow(int i)
        {
            return i / Size;
        }

        /// <summary>
        /// Converts an inline index to a column index
        /// </summary>
        /// <param name="i">index</param>
        /// <returns>column index</returns>
        protected int IndexToCol(int i)
        {
            return i % Size;
        }

        /// <summary>
        /// Provides the shape ID for the given index
        /// </summary>
        /// <param name="i">index</param>
        /// <returns>shape ID associated with the given cell index</returns>
        protected int GetShapeID(int i)
        {
            return Grid[i];
        }

        /// <summary>
        /// Provides the shape ID for the given row/column
        /// </summary>
        /// <param name="row">row index</param>
        /// <param name="col">column index</param>
        /// <returns>shape ID associated with the given cell row/col</returns>
        protected int GetShapeID(int row, int col)
        {
            return Grid[RowColToIndex(row, col)];
        }

        /// <summary>
        /// Provides the character to use in printing the output cell
        /// </summary>
        /// <param name="row">row index</param>
        /// <param name="col">column index</param>
        /// <returns>cell character</returns>
        protected virtual char IndexChar(int row, int col)
        {
            return '_';
        }

        /// <summary>
        /// Prints a string representation of the current BattleGrid
        /// </summary>
        /// <returns>a string-formatted grid complete with cell characters</returns>
        public override string ToString()
        {
            // Define the overall builder
            StringBuilder total_builder = new();

            // Define the header
            StringBuilder header_builder = new();
            header_builder.Append('|');
            for (int i = 0; i < Size; ++i)
            {
                header_builder.Append("---");
                if (i == Size - 1)
                {
                    header_builder.Append('|');
                }
                else
                {
                    header_builder.Append('-');
                }
            }
            total_builder.AppendLine(header_builder.ToString());

            // Iterate over each line
            for (int i = 0; i < Size; ++i)
            {
                // Define inner string builder values
                StringBuilder current_line = new();
                StringBuilder next_line = new();

                // Append the starting values to each
                current_line.Append('|');
                next_line.Append('|');

                // Iterate over each column
                for (int j = 0; j < Size; ++j)
                {
                    // Add the current character count
                    current_line.Append(' ');
                    current_line.Append(IndexChar(i, j));
                    current_line.Append(' ');

                    // Check the shape ID values for the bottom line
                    if (i == Size - 1 || GetShapeID(i, j) != GetShapeID(i + 1, j))
                    {
                        next_line.Append("---");
                    }
                    else
                    {
                        next_line.Append("   ");
                    }

                    // Check the next column value
                    if (j == Size - 1 || GetShapeID(i, j) != GetShapeID(i, j + 1))
                    {
                        current_line.Append('|');
                    }
                    else
                    {
                        current_line.Append(' ');
                    }

                    // Check the final value for the bottom line
                    if (j == Size - 1)
                    {
                        next_line.Append('|');
                    }
                    else
                    {
                        next_line.Append('-');
                    }
                }

                // Append results to the overall string builder
                total_builder.AppendLine(current_line.ToString());
                total_builder.AppendLine(next_line.ToString());
            }

            // Return the string builder output
            return total_builder.ToString();
        }

        public static BattleGrid FromFile(string filePath)
        {
            // Read all the lines
            string text = System.IO.File.ReadAllText(filePath).ToLower();
            text = text.Replace("\n", "").Replace("\r", "").Replace(" ", "").Trim();

            // Convert to ID parameters
            List<int> gridArray = new();
            for (int i = 0; i < text.Length; ++i)
            {
                // Extract the current character
                char c = text[i];

                // Parse the resulting parameters
                int sid = c switch
                {
                    '0' => 0,
                    '1' => 1,
                    '2' => 2,
                    '3' => 3,
                    '4' => 4,
                    '5' => 5,
                    '6' => 6,
                    '7' => 7,
                    '8' => 8,
                    '9' => 9,
                    'a' => 10,
                    'b' => 11,
                    'c' => 12,
                    'd' => 13,
                    'e' => 14,
                    'f' => 15,
                    _ => throw new Exception($"Invalid character {c} provided")
                };

                // Add the character to the grid array
                gridArray.Add(sid);
            }

            // Create the BattleGrid
            return new BattleGrid(gridInput: gridArray.ToArray());
        }
    }
}
