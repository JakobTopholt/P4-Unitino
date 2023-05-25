# Unitino - Arduino with units

## Index
* [Contribute](#contribute)
  * [Prerequisites](#prerequisites)
    * [Rider (recommended)](#rider-recommended)
    * [Visual Studio 2022](#visual-studio-2022)
  * [Building the project](#building-the-project)
* [Credits](#credits)
# Using the compiler
Navigate to [Release](Release) and download the Compiler.exe. Now just run the Compiler.exe in the terminal with `./Compiler.exe`

Note that the .pdb file is optional, however if you get a critical error, having this file next to the executeable allows better debugging.

# Contribute
If you implement a new feature in the grammar, then make sure that both prettyprint and codegen has a test that covers the feature to ensure that the language is stable and no grammar features are meaningless. Creating Typechecking and making a test for it is only required if things accepted in the grammar shouldn't be accepted by the compiler such as assign a string to an integer.
## Prerequisites

Hava Java installed and ensure it is in the PATH variable - check this by running `java --version` in any cmd


### Rider (recommended)
This solution is created in Rider IDE by Jetbrains and uses configurations for the IDE to have a quick workflow

### Visual Studio 2022

Runs poorly with the Resharper extension

Haven't yet created profiles in the [launchsettings file](Compiler/Properties/launchSettings.json), however it should be possible.

## Building the project

The `/.run` folder holds the run configurations

* The `SableCC` configuration compiles the grammar.sablecc3 in the Compiler project 
and places the created classes in the `/Compiler/SableCC` folder
* The `Run` configuration runs the program.cs class as a standard CSharp console app
* The `SableCC2` if the `SableCC` configuration doesn't work

# Credits

[SableCC](sablecc.org) for the original compiler compiler

[Indrek's Tools](http://www.mare.ee/indrek/sablecc/) for adjusting SableCC to generate a C# parser(this is the one in the project)
