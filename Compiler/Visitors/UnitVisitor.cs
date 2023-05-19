using System.Collections;
using Compiler.Visitors.TypeCheckerDir;
using Moduino.analysis;
using Moduino.node;

namespace Compiler.Visitors;

// First pass of the typechecker

public class UnitVisitor : DepthFirstAdapter
{
    private SymbolTable symbolTable;
    private TypeChecker typeChecker;
    public UnitVisitor(SymbolTable symbolTable, TypeChecker typeChecker)
    {
        this.symbolTable = symbolTable;
        this.typeChecker = typeChecker;

    }
    public static bool StateUnit;
    public string tempLocation = "";
    public string tempResult = "";
    public List<string?> errorResults = new List<string>();
    public int indent = 0;

    public override void OutAGrammar(AGrammar node)
    {
        foreach (string error in errorResults)
        {
            Console.WriteLine(error);
        }
        symbolTable = symbolTable.ResetScope();
    }
    
    protected string IndentedString(string s)
    {
        return new string(' ', indent * 3) + s;
    }

    protected void PrintError()
    {
        if (tempResult != "")
        {
            errorResults.Add(tempLocation  + tempResult);
            tempLocation = "";
        }
        else
        {
            tempLocation = "";
        }
    }
    
    public override void InAUnitdeclGlobal(AUnitdeclGlobal node)
    {
        if (symbolTable.IsInExtendedScope(node.GetId().ToString()))
        {
            symbolTable.AddNode(node, Symbol.NotOk);
            tempResult += IndentedString($"UnitId: {node.GetId()} has allready been declared before");
            PrintError();
        }
        tempLocation += IndentedString($"In unitDeclaration {node.GetId()}:\n");
        indent++;
        StateUnit = true;
    }
    public override void OutAUnitdeclGlobal(AUnitdeclGlobal node)
    {
        symbolTable.AddIdToNode(node.GetId().ToString(), node);
        bool subunitsIsOk = true;
        List<ASubunit> subunits = node.GetSubunit().OfType<ASubunit>().ToList();
        foreach (ASubunit subunit in subunits)
        {
            if (symbolTable.GetSymbol(subunit) == Symbol.NotOk)
            {
                subunitsIsOk = false;
            }
        }
        symbolTable.AddNode(node, subunitsIsOk ? Symbol.Ok : Symbol.NotOk);
        symbolTable.AddIdToUnitdecl(node.GetId().ToString(), node);

        StateUnit = false;
        PrintError();
        indent--;
    }

    public override void CaseASubunit(ASubunit node)
    {
        tempLocation += IndentedString($"In subunitDeclaration {node.GetId()}:");
        indent++;
        StateUnit = false;
        if (!symbolTable.IsInExtendedScope(node.GetId().ToString()))
        {
            PExp expression = node.GetExp();
            expression.Apply(typeChecker);
            
            if (symbolTable.GetSymbol(expression) != Symbol.Decimal)
            {
                symbolTable.AddNode(node, Symbol.NotOk);
                tempResult += IndentedString($"expression did not evaluate to a float or num value");
                return;
            }
            symbolTable.AddNode(node, Symbol.Decimal);
            symbolTable.AddIdToUnitdecl(node.GetId().ToString().Trim(), (AUnitdeclGlobal) node.Parent());
        }
        else
        {
            // Subunit's Id already declared
            symbolTable.AddNode(node, Symbol.NotOk);
            tempResult += IndentedString($"{node.GetId()} has allready been declared");
        }
        PrintError();
        indent--;
    }
}