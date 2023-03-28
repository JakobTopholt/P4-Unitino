using System.Text;
using System.Text.RegularExpressions;
using org.sablecc.sablecc.analysis; 
using org.sablecc.sablecc.lexer;
using org.sablecc.sablecc.node;
using org.sablecc.sablecc.parser;
// line 11 in grammer.sablecc3 which says "Package org.sablecc.sablecc;" defines the namespace

Console.WriteLine("Hello, World!");

using FileStream fileStream = 
    File.Open("somepathwhatever.tothebestprogram.writteninthebestlanguage.withbestsuffix.aavild", FileMode.Open);
using TextReader textReader = new StreamReader(fileStream);

Lexer lexer = new (textReader);  // <-- redundant to assign the lexer as a variable when used in the parser
                                 // as it can only be used to traverse all tokens once
Parser parser = new (lexer);
Start start = parser.Parse();
Analysis analysis = parser.IgnoredTokens; // using analysis.GetIn(SomeNode) can get all ignored tokens(usually blanks)
                                          // before the node. Otherwise redudant to assign the parser to a variable,
                                          // because parser.Parse() will produce the same output each time the method is called

// So cleanest code without unnecessary vars would be: (opinionated)
Start start2;
{
    using FileStream fileStream2 = File.Open("somepathwhatever", FileMode.Open);
    using TextReader textReader2 = new StreamReader(fileStream);
    start2 = new Parser(new Lexer(textReader2)).Parse();
}

// Now the AST can be traversed using our own visitors which inherits from analysis

SomeVisitor someVisitor = new();
start2.Apply(someVisitor);

// TODO visitors:
// 1: 3 visitors: scope-check/symbol table, type checking and types (lecture 10-12)
// 2: optional compiler optimization (lecture 20)
// 3: code generator (depending on target language lecture 15-18)


// try use the context action "implement missing members on this SomeVisitor class. Good luck ;)
// You probably want to rather use DepthFirstAdapter and ReverserDepthFirstAdapter

class SomeVisitor : Analysis 
{
    
}
