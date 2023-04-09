using Compiler.Visitors;
using Moduino.lexer;
using Moduino.node;
using Moduino.parser;

Start start;
{
    using FileStream fileStream = File.Open(Directory.GetCurrentDirectory() + "/../../../input.mino", FileMode.Open);
    using TextReader textReader = new StreamReader(fileStream);
    Lexer lexer = new (textReader);
    Parser parser = new (lexer);
    start = parser.Parse();
}
// PrettyPrint Visitor
start.Apply(new PrettyPrint());

// TODO Visitor 1: scope-check/symbol table
// TODO Visitor 2: type checking
// TODO Visitor 3: optional compiler optimization (lecture 20)

// CodeGen Visitor
using CodeGen codegen = new (Directory.GetCurrentDirectory() + "/../../../output.ino");
start.Apply(codegen);


