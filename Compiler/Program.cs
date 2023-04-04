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
start.Apply(new PrettyPrint());
using CodeGen codegen = new (Directory.GetCurrentDirectory() + "/../../../output.ino");
start.Apply(codegen);

// TODO visitors:
// 1: 3 visitors: scope-check/symbol table, type checking and types (lecture 10-12)
// 2: optional compiler optimization (lecture 20)
// 3: code generator (depending on target language lecture 15-18)
