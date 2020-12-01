#include "solver.h"

#include <vector>
#include <iostream>
#include <cstring>

struct SolveGrid
{
    SolveGrid(const StarGrid& grid) :
        star_pos_len(grid.grid.size()),
        star_count(0),
        star_pos(new uint8_t[star_pos_len])
    {
        for (size_t i = 0; i < grid.grid.size(); ++i)
        {
            star_pos[i] = false;
        }
    }

    const size_t star_pos_len;
    size_t star_count;
    uint8_t* star_pos;

    ~SolveGrid()
    {
        if (star_pos != nullptr)
        {
            delete[] star_pos;
            star_pos = nullptr;
        }
    }
};

struct SolveGridState
{
    SolveGridState(const StarGrid& grid) :
        count_len(grid.dim_size),
        valid_len(grid.grid.size())
    {
        create_pointers();

        for (size_t i = 0; i < count_len; ++i)
        {
            star_shape_count[i] = 0;
            star_row_count[i] = 0;
            star_col_count[i] = 0;
            free_shape_count[i] = static_cast<uint8_t>(grid.shape_indices[i].size());
            free_row_count[i] = grid.dim_size;
            free_col_count[i] = grid.dim_size;
        }

        for (size_t i = 0; i < valid_len; ++i)
        {
            valid_cells[i] = true;
        }
    }

    SolveGridState(const SolveGridState& other) :
        count_len(other.count_len),
        valid_len(other.valid_len)
    {
        create_pointers();
        update_vals(other);
    }

    SolveGridState& operator=(const SolveGridState& other)
    {
        if (this == &other)
        {
            return *this;
        }

        if (count_len != other.count_len || valid_len != other.valid_len)
        {
            delete_pointers();
            create_pointers();
        }

        update_vals(other);
        return *this;
    }

    const size_t count_len;
    const size_t valid_len;

    uint8_t* star_shape_count;
    uint8_t* star_row_count;
    uint8_t* star_col_count;
    uint8_t* free_shape_count;
    uint8_t* free_row_count;
    uint8_t* free_col_count;
    uint8_t* valid_cells;

    void update_vals(const SolveGridState& other)
    {
        memcpy(star_shape_count, other.star_shape_count, sizeof(uint8_t) * count_len);
        memcpy(star_row_count, other.star_row_count, sizeof(uint8_t) * count_len);
        memcpy(star_col_count, other.star_col_count, sizeof(uint8_t) * count_len);
        memcpy(free_shape_count, other.free_shape_count, sizeof(uint8_t) * count_len);
        memcpy(free_row_count, other.free_row_count, sizeof(uint8_t) * count_len);
        memcpy(free_col_count, other.free_col_count, sizeof(uint8_t) * count_len);
        memcpy(valid_cells, other.valid_cells, sizeof(uint8_t) * valid_len);
    }

    ~SolveGridState()
    {
        delete_pointers();
    }

private:
    void create_pointers()
    {
        star_shape_count = new uint8_t[count_len];
        star_row_count = new uint8_t[count_len];
        star_col_count = new uint8_t[count_len];
        free_shape_count = new uint8_t[count_len];
        free_row_count = new uint8_t[count_len];
        free_col_count = new uint8_t[count_len];
        valid_cells = new uint8_t[valid_len];
    }

    void delete_pointers()
    {
        if (star_shape_count != nullptr) delete[] star_shape_count;
        if (star_row_count != nullptr) delete[] star_row_count;
        if (star_col_count != nullptr) delete[] star_col_count;
        if (free_shape_count != nullptr) delete[] free_shape_count;
        if (free_row_count != nullptr) delete[] free_row_count;
        if (free_col_count != nullptr) delete[] free_col_count;
        if (valid_cells != nullptr) delete[] valid_cells;
    }
};

bool add_star(const StarGrid& grid, SolveGrid& base, std::vector<SolveGridState>& base_states, size_t ind, SolvedState& state_out)
{
    // Create a copy of the state
    SolveGridState& state = base_states[base.star_count + 1];
    state.update_vals(base_states[base.star_count]);

    // Extract the current shape ID
    const size_t sid = grid.grid[ind];

    // Convert ind to row/col
    const size_t row = grid.ind_to_row(ind);
    const size_t col = grid.ind_to_col(ind);

    // Mark the grid star as added
    state.star_shape_count[sid] += 1;
    state.star_row_count[row] += 1;
    state.star_col_count[col] += 1;

    // Loop over boundary cells around the added cell value
    {
        const size_t row_min = (row > 0) ? row - 1 : 0;
        const size_t row_max = std::min(row + 2, grid.dim_size);

        const size_t col_min = (col > 0) ? col - 1 : 0;
        const size_t col_max = std::min(col + 2, grid.dim_size);

        for (size_t row_i = row_min; row_i < row_max; ++row_i)
        {
            for (size_t col_i = col_min; col_i < col_max; ++col_i)
            {
                const size_t ind_rc = grid.rc_to_ind(row_i, col_i);
                if (state.valid_cells[ind_rc])
                {
                    state.valid_cells[ind_rc] = false;
                    state.free_shape_count[grid.grid[ind_rc]] -= 1;
                    state.free_row_count[row_i] -= 1;
                    state.free_col_count[col_i] -= 1;
                }
            }
        }
    }

    // Check if the row is full
    if (state.star_row_count[row] == grid.stars_per_object)
    {
        for (size_t k = 0; k < grid.dim_size; ++k)
        {
            const size_t indi = grid.rc_to_ind(row, k);
            if (state.valid_cells[indi])
            {
                state.valid_cells[indi] = false;
                state.free_shape_count[grid.grid[indi]] -= 1;
                state.free_row_count[row] -= 1;
                state.free_col_count[k] -= 1;
            }
        }
    }

    // Check if the column is full
    if (state.star_col_count[col] == grid.stars_per_object)
    {
        for (size_t k = 0; k < grid.dim_size; ++k)
        {
            const size_t indi = grid.rc_to_ind(k, col);
            if (state.valid_cells[indi])
            {
                state.valid_cells[indi] = false;
                state.free_shape_count[grid.grid[indi]] -= 1;
                state.free_row_count[k] -= 1;
                state.free_col_count[col] -= 1;
            }
        }
    }

    // Check if the shape is now full
    if (state.star_shape_count[sid] == grid.stars_per_object)
    {
        const std::vector<size_t>& shape_is = grid.shape_indices[sid];
        const size_t gs = shape_is.size();
        for (size_t i = 0; i < gs; ++i)
        {
            const size_t indi = shape_is[i];
            if (state.valid_cells[indi])
            {
                state.valid_cells[indi] = false;
                state.free_shape_count[sid] -= 1;
                state.free_row_count[grid.ind_to_row(indi)] -= 1;
                state.free_col_count[grid.ind_to_col(indi)] -= 1;
            }
        }
    }

    // Check for invalid grid solution to trim constraints
    for (size_t i = 0; i < grid.dim_size; ++i)
    {
        if (state.free_col_count[i] + state.star_col_count[i] < grid.stars_per_object)
        {
            return false;
        }
        else if (state.free_row_count[i] + state.star_row_count[i] < grid.stars_per_object)
        {
            return false;
        }
        else if (state.free_shape_count[i] + state.star_shape_count[i] < grid.stars_per_object)
        {
            return false;
        }
    }

    // Increment and add the star count value
    base.star_count += 1;
    base.star_pos[ind] = true;

    // Check for a solution
    if (base.star_count == grid.target_stars)
    {
        state_out.star_pos = base.star_pos;
        base.star_pos = nullptr;
        state_out.valid_pos = state.valid_cells;
        state.valid_cells = nullptr;
        return true;
    }

    // Loop through future indices to add potential cells

    for (size_t i = ind; i < grid.grid.size(); ++i)
    {
        if (state.valid_cells[i])
        {
            const size_t ri = grid.ind_to_row(i);
            if (ri > 0 && state.star_row_count[ri - 1] < grid.stars_per_object)
            {
                break;
            }

            if (add_star(grid, base, base_states, i, state_out))
            {
                return true;
            }
        }
    }

    base.star_count -= 1;
    base.star_pos[ind] = false;

    if (state.star_row_count[row] == 0)
    {
        SolveGridState& prev_state = base_states[base.star_count];
        prev_state.valid_cells[ind] = false;
        prev_state.free_row_count[row] -= 1;
        prev_state.free_col_count[col] -= 1;
        prev_state.free_shape_count[sid] -= 1;
    }

    return false;
}

bool solve_battle_grid(const StarGrid& grid, SolvedState& state_out)
{
    // Define the starting base grid and states
    SolveGrid base(grid);
    std::vector<SolveGridState> base_states(grid.dim_size * grid.stars_per_object + 1, SolveGridState(grid));

    // Define the basic loop to check the first row
    for (size_t i = 0; i < grid.dim_size; ++i)
    {
        // Add the star and check for solution
        if (add_star(grid, base, base_states, i, state_out))
        {
            return true;
        }
    }

    // Return false on no solution
    return false;
}
