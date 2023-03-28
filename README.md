# Template project for using the C# version of SableCC
by Aavild

## Index

* [Prerequisites](#prerequisites)
* * [Rider (recommended)](#rider-recommended)
* * [Visual Studio 2022](#visual-studio-2022)
* [Building the project](#building-the-project)
* [Docs for SableCC](#docs-for-sablecc)
* [Credits](#credits)
* [ANTLR?](#antlr)

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
* The `SableCC + Run` configuration is a compound of the 2 other configurations

(should be clickable if you open the readme in Rider)

## Docs for SableCC

[SableCC docs](https://sablecc.sourceforge.net/documentation.html)

[SableCC Thesis(also a good doc)](https://sablecc.sourceforge.net/thesis/thesis.html)

[SableCC local docs:](sablecc-3-beta.3.altgen.20041114/doc/) `/sablecc-3-beta.3.altgen.20041114/doc`


## Credits

[SableCC](sablecc.org) for the original compiler compiler

[Indrek's Tools](http://www.mare.ee/indrek/sablecc/) for adjusting SableCC to generate a C# parser(this is the one in the project)

## ANTLR?
Maybe test out and create a project ready to use [ANTLR](https://theantlrguy.atlassian.net/wiki/spaces/ANTLR3/pages/2687262/Antlr+3+CSharp+Target#Antlr3CSharpTarget-GettingStarted)?