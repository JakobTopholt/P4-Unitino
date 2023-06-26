using System.Collections;
using Compiler.Visitors.TypeCheckerDir;
using Moduino.analysis;
using Moduino.node;

namespace Compiler.Visitors;

public class GlobalScopeVisitor : DepthFirstAdapter
{
    private SymbolTable symbolTable;
    private TypeChecker typeChecker;
    public GlobalScopeVisitor(SymbolTable symbolTable, TypeChecker typeChecker)
    {
        this.symbolTable = symbolTable;
        this.typeChecker = typeChecker;
    }
    public Stack<string> locations = new ();
    public string tempResult = "";
    public List<string?> errorResults = new ();
    public int indent = 0;
    public bool reported = false;

    protected string IndentedString(string s)
    {
        return new string(' ', indent * 3) + s;
    }

    protected void PrintError()
    {
        if (tempResult != "" && !reported)
        {
            string error = "";
            error += tempResult;
            foreach(string location in locations)
            {
                error += location;
            }
            errorResults.Add(error);
            locations.Pop();
            reported = true;
        }
        else
        {
            locations.Pop();
        }
    }
    // implement globalscope declarations

    public override void OutAGrammar(AGrammar node)
    {
        base.OutAGrammar(node);
    }
    public override void InADeclstmtGlobal(ADeclstmtGlobal node)
    {
        locations.Push(IndentedString($"In a global declaration: {node.GetStmt()}\n"));
        indent++;
        PStmt globalStmt = node.GetStmt();

        if (globalStmt is ADeclStmt decl)
        {
            if (symbolTable.IsInCurrentScope(decl.GetId()))
            {
                symbolTable.AddNode(node, Symbol.NotOk);
                tempResult += IndentedString($"The id: {decl.GetId()} has already been declared before");
            }
        } else if (globalStmt is ADeclassStmt declass)
        {
            if (symbolTable.IsInCurrentScope(declass.GetId()))
            {
                symbolTable.AddNode(node, Symbol.NotOk);
                tempResult += IndentedString($"The id: {declass.GetId()} has already been declared before");
            }
        }
    }
    public override void OutADeclstmtGlobal(ADeclstmtGlobal node)
    {
        PStmt globalStmt = node.GetStmt();
        if (symbolTable.GetSymbol(node) == null)
        {
            if (globalStmt is ADeclStmt decl)
            {
                symbolTable.AddNode(node, symbolTable.GetSymbol(decl) != Symbol.NotOk ? Symbol.Ok : Symbol.NotOk);

            }
            else if (globalStmt is ADeclassStmt declass)
            {
                symbolTable.AddNode(node, symbolTable.GetSymbol(declass) != Symbol.NotOk ? Symbol.Ok : Symbol.NotOk);
            }
        }
        PrintError();
        indent--;
        locations.Clear();
    }
}