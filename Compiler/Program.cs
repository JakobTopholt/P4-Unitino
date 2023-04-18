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

// SymbolTable
SymbolTable symbolTable = new SymbolTable();

// GlobalScopeCollector - globale variabler gemmes i table (skal funktioner?)
// start.Apply(new GlobalScopeCollector(symbolTable));

// LocalScopeCollector - lokale variabler + funktioner og deres retur gemmes i table
// start.Apply(new LocalScopeCollector(symbolTable));

// TODO Visitor 2: type checking
// Tredje scan -  type checking - ok/not ok

// TypeChecker - bruger symbolTable til at typechecke
// start.Apply(new TypeChecker(SymbolTable));

// TODO Visitor 3: optional compiler optimization (lecture 20)



