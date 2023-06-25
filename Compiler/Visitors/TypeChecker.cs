using System.Collections;
using Compiler.Visitors.TypeCheckerDir;
using Moduino.analysis;
using Moduino.node;

namespace Compiler.Visitors;

// Dette er tredje og sidste pass af typecheckeren
// Den bruger symbolTablen vi har populated til at tjekke

public class TypeChecker : exprTypeChecker
{
    public TypeChecker(SymbolTable symbolTable) : base(symbolTable)
    {
    }
    SortedList<string, AUnitdeclGlobal> unitUseNums = new SortedList<string, AUnitdeclGlobal>(new DuplicateKeyComparer<string>());
    SortedList<string, AUnitdeclGlobal> unitUseDens = new SortedList<string, AUnitdeclGlobal>(new DuplicateKeyComparer<string>());
    
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
        symbolTable.AddNode(node, grammarIsOk ? Symbol.Ok : Symbol.NotOk);
    }
    
    
    public override void CaseAUnitdeclGlobal(AUnitdeclGlobal node)
    {
        
    }

    public override void InANumUnituse(ANumUnituse node)
    {
        locations.Push(IndentedString($"in A NumUnit: {node.GetId()}\n"));
        indent++;
    }

    public override void OutANumUnituse(ANumUnituse node)
    {
        string id = node.GetId().Text;
        AUnitdeclGlobal? unitDecl = symbolTable.GetUnitdeclFromId(id);
        
        symbolTable.AddNode(node, unitDecl != null ? Symbol.Ok : Symbol.NotOk);
        tempResult += unitDecl != null ? "" : IndentedString($"{id} is not a valid unitType\n");
        unitUseNums.Add(id, unitDecl);
        PrintError();
        indent--;
    }

    public override void InADenUnituse(ADenUnituse node)
    {
        locations.Push(IndentedString($"in A DenUnit: {node.GetId()}\n"));
        indent++;
    }

    public override void OutADenUnituse(ADenUnituse node)
    {
        string id = node.GetId().Text;
        AUnitdeclGlobal? unitDecl = symbolTable.GetUnitdeclFromId(id);
        
        symbolTable.AddNode(node, unitDecl != null ? Symbol.Ok : Symbol.NotOk);
        tempResult += unitDecl != null ? "" : IndentedString($"{id} is not a valid unitType\n");
        unitUseNums.Add(id, unitDecl);
        PrintError();
        indent--;
    }

    public override void InAUnitType(AUnitType node)
    {
        locations.Push( IndentedString($"in a Unittype: {node.GetUnituse()}\n"));
        indent++;
        unitUseNums = new SortedList<string, AUnitdeclGlobal>(new DuplicateKeyComparer<string>());
        unitUseDens = new SortedList<string, AUnitdeclGlobal>(new DuplicateKeyComparer<string>());
        
    }

    public override void OutAUnitType(AUnitType node)
    {
        // Still problems with scope checking
        // symbolTable = symbolTable.ExitScope();
        (SortedList<string, AUnitdeclGlobal>, SortedList<string, AUnitdeclGlobal>) unituse = (unitUseNums, unitUseDens);
        symbolTable.AddNodeToUnit(node, unituse);
        symbolTable.AddNode(node, Symbol.Ok);
        unitUseNums = new SortedList<string, AUnitdeclGlobal>(new DuplicateKeyComparer<string>());
        unitUseDens = new SortedList<string, AUnitdeclGlobal>(new DuplicateKeyComparer<string>());
        // symbolTable = symbolTable.EnterScope();
        PrintError();
        indent--;
    }
    public override void OutAIntType(AIntType node)
    {
        symbolTable.AddNode(node, Symbol.Int);
    }
    public override void OutADecimalType(ADecimalType node)
    {
        symbolTable.AddNode(node, Symbol.Decimal);
    }
    public override void OutABoolType(ABoolType node)
    {
        symbolTable.AddNode(node, Symbol.Bool);
    }
    public override void OutACharType(ACharType node)
    {
        symbolTable.AddNode(node, Symbol.Char);
    }
    public override void OutAStringType(AStringType node)
    {
        symbolTable.AddNode(node, Symbol.String);
    }
    public override void OutAVoidType(AVoidType node)
    {
        symbolTable.AddNode(node, Symbol.Func);
    }

    public override void OutAPinType(APinType node)
    {
        symbolTable.AddNode(node, Symbol.Pin);
    }

    public override void InAArg(AArg node)
    {
        locations.Push(IndentedString($"in argument: {node.GetType() + " " + node.GetId()}\n"));
        indent++;
    }

    public override void OutAArg(AArg node)
    {
        // Skal nok også ind i tredje pass ad typechecker (det lokale)
        switch (node.GetType())
        {
            case AIntType:
                symbolTable.AddNode(node, Symbol.Int);
                break;
            case ADecimalType:
                symbolTable.AddNode(node, Symbol.Decimal);
                break;
            case ABoolType:
                symbolTable.AddNode(node, Symbol.Bool);
                break;
            case ACharType:
                symbolTable.AddNode(node, Symbol.Char);
                break;
            case AStringType:
                symbolTable.AddNode(node, Symbol.String);
                break;
            case APinType:
                symbolTable.AddNode(node, Symbol.Pin);
                break;
            case AUnitType customType:
            {
                symbolTable.GetUnit(customType, out var unit);
                symbolTable.AddNodeToUnit(node, unit);
                symbolTable.AddNode(node, Symbol.Ok);
                break; 
            }
            default:
                symbolTable.AddNode(node, Symbol.NotOk);
                tempResult += IndentedString("Is not a valid type\n");
                break;
        }
        PrintError();
        indent--;
    }

    public void AddArgsToScope(Node node, IList args)
    {
        foreach (AArg arg in args)
        {
            string id = arg.GetId().ToString().Trim();
            PType type = arg.GetType();
            switch (type)
            {
                case AIntType:
                case ADecimalType:
                case ABoolType:
                case ACharType:
                case AStringType:
                case APinType:
                case AUnitType:
                    symbolTable.AddIdToNode(id, arg);
                    break;
                default:
                    symbolTable.AddNode(node, Symbol.NotOk);
                    tempResult += IndentedString($"{arg.GetId()} is not a valid Type\n");
                    break;
            }
        }
    }
    public override void InAUntypedGlobal(AUntypedGlobal node)
    {
        locations.Push($"In function {node.GetId()}\n");
        indent++;
        symbolTable = symbolTable.EnterScope();
        AddArgsToScope(node, node.GetArg());
    }

    public override void OutAUntypedGlobal(AUntypedGlobal node)
    {
        // Check om args er ok
        // Check om stmts er ok
        bool untypedIsOk = true;
        List<AArg> args = node.GetArg().OfType<AArg>().ToList();
        foreach (AArg arg in args)
        {
            if (symbolTable.GetSymbol(arg) == Symbol.NotOk)
                untypedIsOk = false;
        }
        List<PStmt> stmts = node.GetStmt().OfType<PStmt>().ToList();
        foreach (PStmt stmt in stmts)
        {
            if (symbolTable.GetSymbol(stmt) == Symbol.NotOk)
                untypedIsOk = false;
        }
        symbolTable = symbolTable.ExitScope();
        if (!untypedIsOk)
        {
            // Overwrite the saved type with notOk
            if (symbolTable._nodeToSymbol.ContainsKey(node))
            {
                symbolTable._nodeToSymbol.Remove(node);
            }
            symbolTable.AddNode(node, Symbol.NotOk);
        }
        else
        {
            if (!node.GetStmt().OfType<AReturnStmt>().Any())
            {
                symbolTable.AddNode(node, Symbol.Func);
            }
        }
        locations.Clear();
        reported = false;
        indent--;
    }
    public override void InATypedGlobal(ATypedGlobal node)
    {
        locations.Push($"In function {node.GetId()}\n");
        indent++;
        symbolTable = symbolTable.EnterScope();
        AddArgsToScope(node, node.GetArg());
    }
    public override void OutATypedGlobal(ATypedGlobal node)
    {
        // Check om args er ok
        // Check om stmts er ok
        bool typedIsOk = true;
        List<AArg> args = node.GetArg().OfType<AArg>().ToList();
        foreach (AArg arg in args)
        {
            if (symbolTable.GetSymbol(arg) == Symbol.NotOk)
                typedIsOk = false;
        }
        List<PStmt> stmts = node.GetStmt().OfType<PStmt>().ToList();
        foreach (PStmt stmt in stmts)
        {
            if (symbolTable.GetSymbol(stmt) == Symbol.NotOk)
                typedIsOk = false;
        }
        if (typedIsOk)
        {
            PType typedType = node.GetType();
            switch (typedType)
            {
                case AIntType:
                    //symbolTable = symbolTable.ExitScope();
                    symbolTable.AddNode(node, Symbol.Int);
                    break;
                case ADecimalType:
                    symbolTable.AddNode(node, Symbol.Decimal);
                    break;
                case ABoolType:
                    symbolTable.AddNode(node, Symbol.Bool);
                    break;
                case ACharType:
                    symbolTable.AddNode(node, Symbol.Char);
                    break;
                case AStringType:
                    symbolTable.AddNode(node, Symbol.String);
                    break;
                case APinType:
                    symbolTable.AddNode(node, Symbol.Pin);
                    break;
                case AVoidType:
                    symbolTable.AddNode(node, Symbol.Func);
                    break;
                case AUnitType a:
                    // Missing a reference from this node to the unitType
                    symbolTable.GetUnit(a, out var unit);
                    symbolTable = symbolTable.ExitScope();
                    symbolTable.AddNodeToUnit(node, unit);
                    symbolTable.AddNode(node, Symbol.Ok);
                    break;
            }
        }
        else
        {
            symbolTable = symbolTable.ExitScope();
            symbolTable.AddNode(node, Symbol.NotOk);
        }

        locations.Clear();
        reported = false;
        indent--;
    }
    public override void InALoopGlobal(ALoopGlobal node)
    {
        locations.Push("In Loop \n");
        indent++;
        symbolTable.Loop++;
        if (symbolTable.Loop != 1)
        {
            symbolTable.AddNode(node, Symbol.NotOk);
        }
    }

    public override void OutALoopGlobal(ALoopGlobal node)
    {
        if (symbolTable.GetSymbol(node) == null)
        {
            bool loopIsOk = true;
            List<PStmt> stmts = node.GetStmt().OfType<PStmt>().ToList();
            foreach (PStmt stmt in stmts)
            {
                if (symbolTable.GetSymbol(stmt) == Symbol.NotOk)
                    loopIsOk = false;
            }
            symbolTable.AddNode(node, loopIsOk ? Symbol.Ok : Symbol.NotOk);
        }
        locations.Clear();
        reported = false;
        indent--;
    }

    public override void InAProgGlobal(AProgGlobal node)
    {
        locations.Push("In Prog \n");
        indent++;
        symbolTable.Prog++;
        if (symbolTable.Prog != 1)
        {
            symbolTable.AddNode(node, Symbol.NotOk);
        }
        symbolTable = symbolTable.EnterScope();
    }
    public override void OutAProgGlobal(AProgGlobal node)
    {
        if (symbolTable.GetSymbol(node) == null)
        {
            bool progIsOk = true;

            List<PStmt> stmts = node.GetStmt().OfType<PStmt>().ToList();
            foreach (PStmt stmt in stmts)
            {
                if (symbolTable.GetSymbol(stmt) == Symbol.NotOk)
                    progIsOk = false;
            }
            symbolTable = symbolTable.ExitScope();
            symbolTable.AddNode(node, progIsOk ? Symbol.Ok : Symbol.NotOk);
            
        }
        locations.Clear();
        reported = false;
        indent--;
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