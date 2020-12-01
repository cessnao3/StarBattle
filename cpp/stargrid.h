#ifndef __H_STAR_GRID__
#define __H_STAR_GRID__

#include <string>
#include <vector>
#include <cstdint>
#include <stdexcept>

struct SolvedState
{
    uint8_t* star_pos;
    uint8_t* valid_pos;

    SolvedState() :
        star_pos(nullptr),
        valid_pos(nullptr)
    {
        // Empty Constructor
    }

    ~SolvedState()
    {
        if (star_pos != nullptr)
        {
            delete[] star_pos;
            star_pos = nullptr;
        }

        if (valid_pos != nullptr)
        {
            delete[] valid_pos;
            valid_pos = nullptr;
        }
    }
};

struct StarGrid
{
    StarGrid(const size_t dim_size, const size_t stars_per_object) :
        dim_size(dim_size),
        stars_per_object(stars_per_object),
        target_stars(dim_size * stars_per_object)
    {
        // Empty Constructor
    }

    std::vector<size_t> grid;
    std::vector<std::vector<size_t>> shape_indices;

public:
    const size_t dim_size;
    const size_t stars_per_object;
    const size_t target_stars;

    inline size_t rc_to_ind(const size_t row, const size_t col) const
    {
        return row * dim_size + col;
    }

    inline size_t ind_to_row(const size_t ind) const
    {
        return ind / dim_size;
    }

    inline size_t ind_to_col(const size_t ind) const
    {
        return ind % dim_size;
    }

    static StarGrid from_grid(const std::string& input)
    {
        std::vector<size_t> grid;
        std::vector<std::vector<size_t>> shape_indices;

        for (size_t i = 0; i < input.size(); ++i)
        {
            bool valid = false;

            if (input[i] >= '0' && input[i] <= '9')
            {
                grid.push_back(input[i] - '0');
                valid = true;
            }
            else if (input[i] >= 'a' && input[i] <= 'f')
            {
                grid.push_back(input[i] - 'a' + 10);
                valid = true;
            }

            if (valid)
            {
                while (shape_indices.size() <= grid.back())
                {
                    shape_indices.push_back(std::vector<size_t>());
                }

                shape_indices[grid.back()].push_back(grid.size() - 1);
            }
        }

        size_t grid_size;
        size_t target_star_count;

        const size_t grid_size_norm = 10;
        const size_t grid_size_large = 14;

        if (grid.size() == grid_size_norm * grid_size_norm)
        {
            grid_size = grid_size_norm;
            target_star_count = 2;
        }
        else if (grid.size() == grid_size_large * grid_size_large)
        {
            grid_size = grid_size_large;
            target_star_count = 3;
        }
        else
        {
            throw std::invalid_argument("invalid input size provided");
        }

        if (shape_indices.size() != grid_size)
        {
            throw std::invalid_argument("unexpected shape count provided");
        }

        StarGrid sg(grid_size, target_star_count);
        sg.grid = std::move(grid);
        sg.shape_indices = std::move(shape_indices);
        return sg;
    }

    std::string to_string(const SolvedState* solved = nullptr) const
    {
        // Define initial parameters
        std::string output;

        // Define characters to use
        const char char_corner = 'O';
        const char char_invalid = 'o';
        const char char_star = '*';
        const char char_empty = '_';

        // Build the Header
        output += char_corner;
        for (size_t i = 0; i < dim_size; ++i)
        {
            output += "---";
            if (i + 1 == dim_size)
            {
                output += char_corner;
            }
            else
            {
                output += '-';
            }
        }
        output += '\n';

        // Start on the Character Parameters
        for (size_t i = 0; i< dim_size; ++i)
        {
            // Define the provided lines
            std::string curr_str;
            std::string next_str;

            // Check if this is the last row
            const bool is_last_row = i + 1 == dim_size;

            // Append starting values to each
            curr_str += '|';
            next_str += is_last_row ? char_corner : '|';

            // Iterate for each column
            for (size_t j = 0; j < dim_size; ++j)
            {
                // Extract the shape ID
                const size_t ind = rc_to_ind(i, j);
                const size_t sid = grid[ind];

                // Add the current character value
                curr_str += ' ';
                if (solved != nullptr)
                {
                    if (solved->star_pos[ind])
                    {
                        curr_str += char_star;
                    }
                    else if (!solved->valid_pos[ind])
                    {
                        curr_str += char_invalid;
                    }
                    else
                    {
                        curr_str += char_empty;
                    }
                }
                else
                {
                    curr_str += char_empty;
                }
                curr_str += ' ';

                // Get the shape ID of the next column
                if (i + 1 == dim_size || sid != grid[rc_to_ind(i + 1, j)])
                {
                    next_str += "---";
                }
                else
                {
                    next_str += "   ";
                }

                // Check the next column value
                if (j + 1 == dim_size || sid != grid[rc_to_ind(i, j + 1)])
                {
                    curr_str += '|';
                }
                else
                {
                    curr_str += ' ';
                }

                // Check the final value
                if (j + 1 == dim_size)
                {
                    next_str += is_last_row ? char_corner : '|';
                }
                else
                {
                    next_str += '-';
                }
            }

            // Append all values
            output += curr_str + '\n';
            output += next_str + '\n';
        }

        // Return the string result
        return output;
    }
};

#endif
