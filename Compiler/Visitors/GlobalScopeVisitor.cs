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

    public override void CaseAGrammar(AGrammar node)
    {
        InAGrammar(node);
        {
            Object[] temp = new Object[node.GetGlobal().Count];
            node.GetGlobal().CopyTo(temp, 0);
            for(int i = 0; i < temp.Length; i++)
            {
                if (temp[i] is ADeclstmtGlobal)
                {
                    ((PGlobal) temp[i]).Apply(this);
                }
            }
        }
        OutAGrammar(node);
    }
    public override void OutAGrammar(AGrammar node)
    {
        string symbols = "";
        bool grammarIsOk = true;
        List<PGlobal> globals = node.GetGlobal().OfType<PGlobal>().ToList();
        foreach (PGlobal global in globals)
        {
            symbols += symbolTable.GetSymbol(global).ToString();
            if (symbolTable.GetSymbol(global) == Symbol.NotOk)
                grammarIsOk = false;
        }
        foreach (string error in errorResults)
        {
            Console.WriteLine(error);
        }

        symbolTable = symbolTable.ResetScope();
       // symbolTable.AddNode(node, grammarIsOk ? Symbol.Ok : Symbol.NotOk);
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
    
    
    public override void InADeclassStmt(ADeclassStmt node)
    {
        locations.Push(IndentedString($"in DeclarationAssignment {node}\n"));
        indent++;
    }
    public override void OutADeclassStmt(ADeclassStmt node)
    {
        if (!symbolTable.IsInCurrentScope(node.GetId()))
        {
            Node expression = node.GetExp();
            typeChecker.symbolTable = symbolTable;
            expression.Apply(typeChecker);
            symbolTable = typeChecker.symbolTable;
            Symbol? exprType = symbolTable.GetSymbol(expression);
            PType unit = node.GetType();
            switch (unit)
                {
                    case AIntType a:
                        symbolTable.AddId(node.GetId().ToString(), node, exprType == Symbol.Int ? Symbol.Int : Symbol.NotOk);
                        tempResult += exprType == Symbol.Int ? "" : IndentedString("expression is not an Int\n");
                        break;
                    case ADecimalType b:
                        symbolTable.AddId(node.GetId().ToString(), node, exprType == Symbol.Decimal ? Symbol.Decimal : Symbol.NotOk);
                        tempResult += exprType == Symbol.Decimal ? "" : IndentedString("expression is not an Decimal\n");
                        break;
                    case ABoolType c:
                        symbolTable.AddId(node.GetId().ToString(), node, exprType == Symbol.Bool ? Symbol.Bool : Symbol.NotOk);
                        tempResult += exprType == Symbol.Bool ? "" : IndentedString("expression is not an Bool\n");
                        break;
                    case ACharType d:
                        symbolTable.AddId(node.GetId().ToString(), node, exprType == Symbol.Char ? Symbol.Char : Symbol.NotOk);
                        tempResult += exprType == Symbol.Char ? "" : IndentedString("expression is not an Char\n");
                        break;
                    case AStringType e:
                        symbolTable.AddId(node.GetId().ToString(), node, exprType is Symbol.String or Symbol.Char ? Symbol.String : Symbol.NotOk);
                        tempResult += exprType == Symbol.String ? "" : IndentedString("expression is not a string or char\n");
                        break;
                    case APinType:
                        symbolTable.AddId(node.GetId().ToString(), node, exprType is Symbol.Int or Symbol.Pin ? Symbol.Int : Symbol.NotOk);
                        tempResult += exprType is Symbol.Int or Symbol.Pin ? "" : IndentedString("expression is not an an pin or integer\n");
                        break;
                    case AUnitType customType:
                        if (symbolTable.GetUnit(customType, out var unitType) && 
                            symbolTable.GetUnit(expression, out var expType))
                        {
                            if (symbolTable.CompareUnitTypes(unitType, expType))
                            {
                                symbolTable.AddIdToNode(node.GetId().Text, node);
                                symbolTable.AddNodeToUnit(node, unitType);
                                symbolTable.AddNode(node, Symbol.Ok); 
                            }
                            else
                            {
                                symbolTable.AddIdToNode(node.GetId().ToString(), node);
                                symbolTable.AddNode(node, Symbol.NotOk);
                                tempResult += IndentedString("expression is not correct unitType\n");
                            }
                        }
                        else
                        {
                            symbolTable.AddIdToNode(node.GetId().ToString(), node);
                            symbolTable.AddNode(node, Symbol.NotOk);
                            tempResult += IndentedString("expression have no unitType associated\n");
                        }
                        break; 
                    default:
                        symbolTable.AddIdToNode(node.GetId().ToString(), node);
                        symbolTable.AddNode(node, Symbol.NotOk);
                        tempResult += IndentedString("Wrong declaretype\n");
                        break;
                }
        }
        else
        {
            symbolTable.AddNode(node, Symbol.NotOk);
            tempResult += IndentedString("id is allready declared in this scope\n");
        }
        PrintError();
        indent--;
    }
    
    
    public override void InADeclStmt(ADeclStmt node)
    {
        locations.Push( IndentedString($"in Declaration {node}\n"));
        indent++;
    }

    public override void OutADeclStmt(ADeclStmt node)
    {
        if (!symbolTable.IsInCurrentScope(node.GetId()))
        {
            switch (node.GetType())
            {
                case AIntType:
                    symbolTable.AddId(node.GetId().ToString(), node, Symbol.Int);
                    break;
                case ADecimalType:
                    symbolTable.AddId(node.GetId().ToString(), node, Symbol.Decimal);
                    break;
                case ABoolType:
                    symbolTable.AddId(node.GetId().ToString(), node, Symbol.Bool);
                    break;
                case ACharType:
                    symbolTable.AddId(node.GetId().ToString(), node, Symbol.Char);
                    break;
                case AStringType:
                    symbolTable.AddId(node.GetId().ToString(), node, Symbol.String);
                    break;
                case APinType:
                    symbolTable.AddId(node.GetId().ToString(), node, Symbol.Pin);
                    break;
                case AUnitType customType:
                    symbolTable.GetUnit(customType, out var unit);
                    symbolTable.AddIdToNode(node.GetId().ToString(), node);
                    symbolTable.AddNodeToUnit(node, unit);
                    symbolTable.AddNode(node, Symbol.Ok);
                    break; 
                default:
                    symbolTable.AddNode(node, Symbol.NotOk);
                    tempResult += IndentedString("Type not recognized\n");
                    break;
            }
        }
        else
        {
            symbolTable.AddNode(node, Symbol.NotOk);
            tempResult += IndentedString("id is allready declared in this scope\n");

        }
        PrintError();
        indent--;
    }
    
    
    /*
    public override void CaseAUnitdeclGlobal(AUnitdeclGlobal node)
    {
        symbolTable = symbolTable.EnterScope().ExitScope();
        symbolTable = symbolTable.ResetScope();
    }

    public override void CaseAUntypedGlobal(AUntypedGlobal node)
    {
        symbolTable = symbolTable.EnterScope().ExitScope();
        symbolTable = symbolTable.ResetScope();
    }

    public override void CaseATypedGlobal(ATypedGlobal node)
    {
        symbolTable = symbolTable.EnterScope().ExitScope();
        symbolTable = symbolTable.ResetScope();
    }

    public override void CaseALoopGlobal(ALoopGlobal node)
    {
        symbolTable = symbolTable.EnterScope().ExitScope();
        symbolTable = symbolTable.ResetScope();
    }
    public override void CaseAProgGlobal(AProgGlobal node)
    {
        symbolTable = symbolTable.EnterScope().ExitScope();
        symbolTable = symbolTable.ResetScope();
    } */
}