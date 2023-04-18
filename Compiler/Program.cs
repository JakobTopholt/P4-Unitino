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

// Codegen Visitor
{
    using FileStream stream = File.Create(Directory.GetCurrentDirectory() + "/../../../output.ino");
    using StreamWriter writer = new(stream);
    using CodeGen codegen = new (writer);
    start.Apply(codegen);
}

// TODO Visitor 1: scope-check/symbol table
// create symbolTable
SymbolTable symbolTable = new SymbolTable();

// GlobalScopeCollector
start.Apply(new GlobalScopeCollector(symbolTable));

// LocalScopeCollector
// start.Apply(new LocalScopeCollector());

// TODO Visitor 2: type checking
// TypeChecker
// start.Apply(new TypeChecker());

// TODO Visitor 3: optional compiler optimization (lecture 20)



