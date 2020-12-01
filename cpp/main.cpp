#include <iostream>

#include <string>
#include <fstream>

#include "stargrid.h"
#include "solver.h"

int main(int argc, char* argv[])
{
    // Check for an input file
    std::string input_name;

    if (argc > 1)
    {
        input_name = argv[1];
    }
    else
    {
        input_name = "input.txt";
    }

    // Read all input file data
    std::string data;
    {
        std::ifstream input_file(input_name);

        if (!input_file.is_open())
        {
            std::cerr << "Unable to open file " << input_name << std::endl;
            return 1;
        }

        {
            std::string line;

            while (std::getline(input_file, line))
            {
                data += line;
            }
        }

        input_file.close();
    }

    // Attempt to solve the d
    try
    {
        // Create the grid
        StarGrid grid = StarGrid::from_grid(data);

        // Print input grid
        std::cout << "Input Grid:" << '\n';
        std::cout << grid.to_string() << '\n';

        // Define the solve structure
        SolvedState state;

        // Work to solve the grid
        std::cout << "Starting Solve:" << std::endl;

        // Print grid output if a solution is found
        if (solve_battle_grid(grid, state))
        {
            std::cout << grid.to_string(&state);
        }
        else
        {
            std::cout << "No solution found" << '\n';
        }
    }
    catch (const std::invalid_argument& e)
    {
        // Catch an input error exception
        std::cerr << "Error reading input: " << e.what() << '\n';
        return 1;
    }

    // Return success by default
    return 0;
}
