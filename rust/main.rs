use std::env;
use std::path;

mod stargrid;
mod solver;


fn main()
{
    let args: Vec<String> = env::args().collect();
    let mut file: String = String::from("input.txt");

    if args.len() > 1
    {
        file = args[1].to_owned();
    }

    match stargrid::StarGrid::read_grid(path::Path::new(&file))
    {
        Ok(mut v) =>
        {
            println!("Input Grid:");
            println!("{0:}", v.to_string());
            println!("Starting Solve:");
            v.star_pos = solver::solve_battle_grid(&v);
            if v.star_pos.is_none()
            {
                println!("No Solution Found");
            }
            println!("{0}", v.to_string());
        },
        Err(e) => println!("Error: {:?}", e)
    };
}
