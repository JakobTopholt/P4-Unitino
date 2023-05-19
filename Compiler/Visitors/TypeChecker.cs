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
        symbolTable.AddNode(node, grammarIsOk ? Symbol.Ok : Symbol.NotOk);
        symbolTable = symbolTable.ResetScope();
    }

    public override void CaseAUnitdeclGlobal(AUnitdeclGlobal node)
    {
        
    }

    public override void InANumUnituse(ANumUnituse node)
    {
        tempLocation += IndentedString($"in A NumUnit: {node.GetId()}\n");
        indent++;
    }

    public override void OutANumUnituse(ANumUnituse node)
    {
        // Does id to Unit exist?
        symbolTable.AddNode(node,
            symbolTable.GetUnitdeclFromId(node.GetId().ToString()) != null ? Symbol.Ok : Symbol.NotOk);
        tempLocation += symbolTable.GetUnitdeclFromId(node.GetId().ToString()) != null ? "" : IndentedString($"{node.GetId()} not a valid subunit\n");
        PrintError();
        indent--;
    }

    public override void InADenUnituse(ADenUnituse node)
    {
        tempLocation += IndentedString($"in A DenUnit: {node.GetId()}\n");
        indent++;
    }

    public override void OutADenUnituse(ADenUnituse node)
    {
        symbolTable.AddNode(node,
            symbolTable.GetUnitdeclFromId(node.GetId().ToString()) != null ? Symbol.Ok : Symbol.NotOk);
        tempLocation += symbolTable.GetUnitdeclFromId(node.GetId().ToString()) != null ? "" : IndentedString($"{node.GetId()} not a valid subunit\n");
        PrintError();
        indent--;
    }

    public override void InAUnitType(AUnitType node)
    {
        tempLocation += IndentedString($"in a Unittype: {node.GetUnituse()}\n");
        indent++;
    }

    public override void OutAUnitType(AUnitType node)
    {
        List<ANumUnituse> nums = node.GetUnituse().OfType<ANumUnituse>().ToList();
        List<ADenUnituse> dens = node.GetUnituse().OfType<ADenUnituse>().ToList();
        List<AUnitdeclGlobal> newNums = new List<AUnitdeclGlobal>();
        List<AUnitdeclGlobal> newDens = new List<AUnitdeclGlobal>();

        bool aunittypeIsOk = true;
        foreach (ANumUnituse num in nums)
        {
            AUnitdeclGlobal? newNum = symbolTable.GetUnitdeclFromId(num.GetId().ToString());
            if (newNum != null)
            {
                newNums.Add(newNum);
            }
            else
            {
                // Not a recognized unit
                aunittypeIsOk = false;
            }
        }
        foreach (ADenUnituse den in dens)
        {
            AUnitdeclGlobal? newDen = symbolTable.GetUnitdeclFromId(den.GetId().ToString());
            if (newDen != null)
            {
                newDens.Add(newDen);
            }
            else
            {
                // Not a recognized unit
                aunittypeIsOk = false;
            }
        }

        if (aunittypeIsOk)
        {
            (List<AUnitdeclGlobal>, List<AUnitdeclGlobal>) unituse = (newNums, newDens);
            symbolTable.AddNodeToUnit(node, unituse);
            symbolTable.AddNode(node, Symbol.Ok);
        }
        else
        {
            symbolTable.AddNode(node, Symbol.NotOk);
        }
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

    public override void InAArg(AArg node)
    {
        tempLocation += IndentedString($"in argument: {node.GetType() + " " + node.GetId()}\n");
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
            case AUnitType customType:
            {
                symbolTable.GetUnit(customType, out var unit);
                symbolTable.AddNodeToUnit(node, unit);
                symbolTable.AddNode(node, Symbol.Ok);
                break; 
            }
            default:
                symbolTable.AddNode(node, Symbol.NotOk);
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
                    symbolTable.AddIdToNode(id, arg);
                    break;
                case ADecimalType:
                    symbolTable.AddIdToNode(id, arg);
                    break;
                case ABoolType:
                    symbolTable.AddIdToNode(id, arg);
                    break;
                case ACharType:
                    symbolTable.AddIdToNode(id, arg);
                    break;
                case AStringType:
                    symbolTable.AddIdToNode(id, arg);
                    break;
                case AUnitType customType:
                    if (symbolTable.GetUnit(customType, out var unit))
                    {
                        // overvej om AddNodeToUnit skal fjernes her
                        symbolTable.AddNodeToUnit(arg, unit);
                        symbolTable.AddIdToNode(id, arg);
                    }
                    else
                    {
                        symbolTable.AddNode(node, Symbol.NotOk);
                    }
                    break;
                default:
                    symbolTable.AddNode(node, Symbol.NotOk);
                    break;
            }
        }
    }
    public override void InAUntypedGlobal(AUntypedGlobal node)
    {
        Location += $"In function {node.GetId()}\n";
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
        symbolTable.AddNode(node, untypedIsOk ? Symbol.Ok : Symbol.NotOk);
        Location = "";
        tempResult = "";
        reported = false;
        indent--;
        index++;
    }
    public override void InATypedGlobal(ATypedGlobal node)
    {
        Location += $"In function {node.GetId()}\n";
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
        symbolTable = symbolTable.ExitScope();
        symbolTable.AddNode(node, typedIsOk ? Symbol.Ok : Symbol.NotOk);
        Location = "";
        tempResult = "";
        reported = false;
        indent--;
        index++;
    }
    public override void InALoopGlobal(ALoopGlobal node)
    {
        Location += "In Loop \n";
        indent++;
        symbolTable.Loop++;
        if (symbolTable.Loop != 1)
        {
            symbolTable.AddNode(node, Symbol.NotOk);
        }
        symbolTable = symbolTable.EnterScope();
        
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
            symbolTable = symbolTable.ExitScope();
            symbolTable.AddNode(node, loopIsOk ? Symbol.Ok : Symbol.NotOk);
            Location = "";
            tempResult = "";
            reported = false;
            indent--;
            index++;
        }
    }

    public override void InAProgGlobal(AProgGlobal node)
    {
        Location += "In Prog \n";
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
            Location = "";
            tempResult = "";
            reported = false;
            indent--;
            index++;
            
        }
        
    }
    
}