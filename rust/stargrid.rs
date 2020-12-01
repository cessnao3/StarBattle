use std::fs;
use std::path;

#[derive(Clone)]
pub struct SolvedState
{
    pub star_pos: Vec<bool>,
    pub valid_cell: Vec<bool>
}

#[derive(Clone)]
pub struct StarGrid
{
    pub grid: Vec<u8>,
    pub dim_size: usize,
    pub shape_indices: Vec<Vec<usize>>,
    pub target_star_count: u8,
    pub star_pos: Option<SolvedState>
}

impl StarGrid
{
    #[inline]
    pub fn rowcol_to_ind(&self, row: usize, col: usize) -> usize
    {
        return row * self.dim_size + col;
    }

    #[inline]
    pub fn ind_to_row(&self, ind: usize) -> usize
    {
        return ind / self.dim_size;
    }

    #[inline]
    pub fn ind_to_col(&self, ind: usize) -> usize
    {
        return ind % self.dim_size;
    }

    #[inline]
    pub fn get_shape_id(&self, row: usize, col: usize) -> u8
    {
        return self.grid[self.rowcol_to_ind(row, col)];
    }

    pub fn read_grid(filepath: &path::Path) -> std::result::Result<StarGrid, &str>
    {
        let d = fs::read_to_string(filepath);

        let mut file_str = match d
        {
            Ok(v) => v,
            Err(_) => String::from("")
        };

        let mut shape_indices: Vec<Vec<usize>> = Vec::new();

        file_str = file_str.replace('\n', "").replace('\r', "");

        let grid_size_large: usize = 14;
        let grid_size_norm: usize = 10;

        let dim_size: usize;
        let target_star_count: u8;

        if file_str.len() == grid_size_norm.pow(2)
        {
            dim_size = grid_size_norm;
            target_star_count = 2;
        }
        else if file_str.len() == grid_size_large.pow(2)
        {
            dim_size = grid_size_large;
            target_star_count = 3;
        }
        else
        {
            return Err("Invalid string length provided");
        }

        for _ in 0..dim_size
        {
            shape_indices.push(Vec::new());
        }

        let mut grid: Vec<u8> = Vec::new();

        for (i,  c) in file_str.chars().enumerate()
        {
            let char_index: u8 = match c.to_digit(16)
            {
                Some(v) => v as u8,
                _ => 32
            };

            grid.push(char_index);
            shape_indices[char_index as usize].push(i);
        }

        return Ok(StarGrid
        {
            grid: grid,
            dim_size: dim_size,
            shape_indices: shape_indices,
            target_star_count: target_star_count,
            star_pos: None
        });
    }
}

impl ToString for StarGrid
{
    fn to_string(&self) -> String
    {
        // Define an initial empty string
        let mut grid_str: String = String::from("");

        // Define characters to use
        let char_bound = 'O';
        let char_invalid = 'o';
        let char_star = '*';
        let char_empty = '_';

        // Build the Header
        grid_str.push(char_bound);
        for i in 0..self.dim_size
        {
            grid_str.push_str("---");
            if i == self.dim_size - 1
            {
                grid_str.push(char_bound);
            }
            else
            {
                grid_str.push('-');
            }
        }
        grid_str.push('\n');

        // Start on the Character Parameters
        for i in 0..self.dim_size as usize
        {
            // Define the provided lines
            let mut curr_str: String = String::from("");
            let mut next_str: String = String::from("");

            // Check if this is the last row
            let last_row = i + 1 == self.dim_size;

            // Append starting values to each
            curr_str.push('|');
            next_str.push(if last_row { char_bound } else { '|' });

            // Iterate for each column
            for j in 0..self.dim_size
            {
                // Extract the shape ID
                let sid = self.get_shape_id(i, j);

                // Add the current character value
                curr_str.push(' ');
                curr_str.push(match &self.star_pos
                {
                    Some(v) =>
                    {
                        let ind = self.rowcol_to_ind(i, j);
                        if v.star_pos[ind]
                        {
                            char_star
                        }
                        else if !v.valid_cell[ind]
                        {
                            char_invalid
                        }
                        else
                        {
                            char_empty
                        }
                    },
                    None => char_empty
                });
                curr_str.push(' ');

                // Get the shape ID of the next column
                if i + 1 == self.dim_size || sid != self.get_shape_id(i + 1, j)
                {
                    next_str.push_str("---");
                }
                else
                {
                    next_str.push_str("   ");
                }

                // Check the next column value
                if j + 1 == self.dim_size || sid != self.get_shape_id(i, j + 1)
                {
                    curr_str.push('|');
                }
                else
                {
                    curr_str.push(' ');
                }

                // Check the final value
                if j + 1 == self.dim_size
                {
                    next_str.push(if last_row { char_bound } else { '|' });
                }
                else
                {
                    next_str.push('-');
                }
            }

            // Append all values
            grid_str.push_str(&curr_str);
            grid_str.push('\n');
            grid_str.push_str(&next_str);
            grid_str.push('\n');
        }

        // Return the string result
        return grid_str;
    }
}
