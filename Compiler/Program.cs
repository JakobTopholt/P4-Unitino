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


List<SymbolTable> AllTables = new() { };

SymbolTable symbolTable = new(null, AllTables);
// UnitVisitor
start.Apply(new UnitVisitor(symbolTable));

// FunctionVisitor
start.Apply(new FunctionVisitor(symbolTable));

// Typechecker
start.Apply(new TypeChecker(symbolTable));

// Codegen Visitor
{
    using FileStream stream = File.Create(Directory.GetCurrentDirectory() + "/../../../output.ino");
    using StreamWriter writer = new(stream);
    using CodeGen codegen = new (writer, symbolTable);
    start.Apply(codegen);
}


// TODO Visitor 3: optional compiler optimization (lecture 20)



