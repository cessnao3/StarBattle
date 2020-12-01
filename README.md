# StarBattle Solvers

This repository provides several different implementations of solvers for StarBattle, in C++, C#, and Rust. The algorithms within each are very similar, but each have some differences in the algorithms based on the languages being used. The C# variant also has the option to make use of threading, but the processing speed of the algorithm is so fast that threading is unnecessary and may cost more than it provides.

## Input Files

For each program, the input file should be formatted the same way.

For a 10x10, each shape is given an index from 0 through 9 and entered line-by-line, one character per cell, and 10 characters per line for a total of 10 lines.

For a 14x14, each shape is given an index from 0 through d, in hexadecimal, also entered line-by-line, one character per cell, and 14 characters per line for a total of 14 lines.
