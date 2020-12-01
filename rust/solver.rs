use crate::stargrid::StarGrid;
use crate::stargrid::SolvedState;

//#[derive(Clone)]
#[derive(Debug)]
struct SolveGrid
{
    star_count: u8,
    star_pos: Vec<bool>,
}

#[derive(Clone, Debug)]
struct SolveGridState
{
    star_shape_count: Vec<u8>,
    star_row_count: Vec<u8>,
    star_col_count: Vec<u8>,
    free_shape_count: Vec<u8>,
    free_row_count: Vec<u8>,
    free_col_count: Vec<u8>,
    valid_cells: Vec<bool>
}

fn create_main_solve(grid: &StarGrid) -> SolveGrid
{
    return SolveGrid
    {
        star_count: 0,
        star_pos: (0..grid.grid.len()).map(|_| false).collect::<Vec<bool>>()
    };
}

fn create_solve_state(grid: &StarGrid) -> SolveGridState
{
    let star_count: Vec<u8> = (0..grid.dim_size).map(|_| 0).collect::<Vec<u8>>();
    let free_count: Vec<u8> = (0..grid.dim_size).map(|_| grid.dim_size as u8).collect::<Vec<u8>>();

    let free_shape_count: Vec<u8> = grid.shape_indices.iter().map(|v| v.len() as u8).collect::<Vec<u8>>();
    let valid_cells: Vec<bool> = (0..grid.grid.len()).map(|_| true).collect::<Vec<bool>>();

    return SolveGridState
    {
        valid_cells: valid_cells,
        star_shape_count: star_count.clone(),
        star_row_count: star_count.clone(),
        star_col_count: star_count.clone(),
        free_shape_count: free_shape_count,
        free_row_count: free_count.clone(),
        free_col_count: free_count.clone()
    };
}

fn row_col_iter(grid: &StarGrid, u: usize) -> std::ops::Range<usize>
{
    return (if u > 0 { u - 1 } else { u })..std::cmp::min(grid.dim_size, u + 2);
}

fn add_star(grid: &StarGrid, solver: &mut SolveGrid, state_prev: &mut SolveGridState, ind: usize) -> std::option::Option<SolvedState>
{
    // Create a copy of the state
    let mut state: SolveGridState = (*state_prev).clone();

    // Check if there is already a star in the given position
    if solver.star_pos[ind] || !state.valid_cells[ind]
    {
        println!("This shouldn't ever happen!");
        return None;
    }

    // Extract the added star's grid shape ID
    let sid: u8 = grid.grid[ind];
    let sid_ind: usize = sid as usize;

    // Convert ind to row/col
    let row: usize = grid.ind_to_row(ind);
    let col: usize = grid.ind_to_col(ind);

    // Mark the grid star as added
    state.star_shape_count[sid as usize] += 1;
    state.star_row_count[row] += 1;
    state.star_col_count[col] += 1;

    // Loop over boundary cells around the added cell value
    for row_i in row_col_iter(grid, row)
    {
        for col_i in row_col_iter(grid, col)
        {
            let ind_rc = grid.rowcol_to_ind(row_i, col_i);
            if state.valid_cells[ind_rc]
            {
                state.valid_cells[ind_rc] = false;
                state.free_shape_count[grid.grid[ind_rc] as usize] -= 1;
                state.free_row_count[row_i] -= 1;
                state.free_col_count[col_i] -= 1;
            }
        }
    }

    // Check if the shape is now full
    if state.star_shape_count[sid_ind] == grid.target_star_count * grid.dim_size as u8
    {
        for i in &grid.shape_indices[sid_ind]
        {
            let ii: usize = *i;
            if state.valid_cells[ii]
            {
                state.valid_cells[ii] = false;
                state.free_shape_count[sid_ind] -= 1;
                state.free_row_count[grid.ind_to_row(ii)] -=1;
                state.free_col_count[grid.ind_to_col(ii)] -=1;
            }
        }
    }

    // Check if the row is full
    if state.star_row_count[row] == grid.target_star_count
    {
        for k in 0..grid.dim_size
        {
            let ii: usize = grid.rowcol_to_ind(row, k);

            if state.valid_cells[ii]
            {
                state.valid_cells[ii] = false;
                state.free_shape_count[grid.grid[ii] as usize] -= 1;
                state.free_row_count[row] -= 1;
                state.free_col_count[k] -= 1;
            }
        }
    }

    // Check if the column is full
    if state.star_col_count[col] == grid.target_star_count
    {
        for k in 0..grid.dim_size
        {
            let ii: usize = grid.rowcol_to_ind(k, col);

            if state.valid_cells[ii]
            {
                state.valid_cells[ii] = false;
                state.free_shape_count[grid.grid[ii] as usize] -= 1;
                state.free_row_count[k] -= 1;
                state.free_col_count[col] -= 1;
            }
        }
    }

    // Check for invalid grid solution to trim constraints
    for i in 0..grid.dim_size
    {
        if state.free_col_count[i] + state.star_col_count[i] < grid.target_star_count
        {
            return None;
        }
        else if state.free_row_count[i] + state.star_row_count[i] < grid.target_star_count
        {
            return None;
        }
        else if state.free_shape_count[i] + state.star_shape_count[i] < grid.target_star_count
        {
            return None;
        }
    }

    // Increment and add the star count value
    solver.star_count += 1;
    solver.star_pos[ind] = true;

    // Check for a solution
    if solver.star_count == grid.target_star_count * grid.dim_size as u8
    {
        return Some(SolvedState
        {
            star_pos: solver.star_pos.clone(),
            valid_cell: state.valid_cells.clone()
        });
    }

    // Loop through future indices to add potential cells
    for i in ind..grid.grid.len()
    {
        if state.valid_cells[i]
        {
            let ri = grid.ind_to_row(i);
            if ri > 0 && state.star_row_count[ri - 1] < grid.target_star_count
            {
                break;
            }

            let rval = add_star(grid, solver, &mut state, i);
            if rval.is_some()
            {
                return rval;
            }
        }
    }

    // Remove the star and uncheck the star position from the state grid
    solver.star_count -= 1;
    solver.star_pos[ind] = false;

    // Invalidate the current cell from the previous state if no possible solutions were found
    if state.star_row_count[row] == 0
    {
        state_prev.valid_cells[ind] = false;
        state_prev.free_row_count[row] -= 1;
        state_prev.free_col_count[col] -= 1;
        state_prev.free_shape_count[sid_ind] -= 1;
    }

    // Return no solution on failure
    return None;
}

pub fn solve_battle_grid(grid: &StarGrid) -> std::option::Option<SolvedState>
{
    // Create the base object and solve state
    let mut base = create_main_solve(grid);
    let mut base_state = create_solve_state(grid);

    // Iterate over the first row of values
    for i in 0..grid.dim_size
    {
        // Check the return value for the solution
        let ret = add_star(grid, &mut base, &mut base_state, i);

        // Detect if the return value is successful; if so, return
        if ret.is_some()
        {
            return ret;
        }
    }

    // Return no solution on failure
    return None;
}
