using System.Collections;
using Compiler.Visitors.TypeCheckerDir;
using Moduino.analysis;
using Moduino.node;

namespace Compiler.Visitors;

// Dette er tredje og sidste pass af typecheckeren
// Den bruger symbolTablen vi har populated til at tjekke

public class TypeChecker : exprTypeChecker
{
    
    // TASKS
    
    // 1
    // Implement all the declarations, assignments and all nodes which need to be scanned by the visitor
    // To be controlled

    // 2
    // Implement unitTypechecking into DelcStmnt og DeclAssStmt
    // WIP
    
    // 3 
    // Mangler at håndtere funktions parametre
    // WIP

    // 4
    // Typechecking for functioncall (Functioncalls needs to get returntype based on functioncallID) or node reference?
    // Then compare in exp or whatever
    // To do
    
    // 5
    // Typecast hierachry
    // We need to implement an understanding of the types precedence. int --> float,  float --> string eg. basicly the implicit typecasting
    // This is probably a feauture we will have to work on more, when we want to implement precedence for custom unit types.
    // To do
    
    public TypeChecker(SymbolTable symbolTable) : base(symbolTable)
    {
    }

    public override void OutAGrammar(AGrammar node)
    {
        bool grammarIsOk = true;
        foreach (PGlobal global in node.GetGlobal())
        {
            if (symbolTable.GetSymbol(global) == Symbol.NotOk)
                grammarIsOk = false;
        }
        symbolTable.AddNode(node, grammarIsOk ? Symbol.Ok : Symbol.NotOk);
        symbolTable = symbolTable.ResetScope();
    }

    public override void OutANumUnituse(ANumUnituse node)
    {
        // Does id to Unit exist?
        symbolTable.AddNode(node,
            symbolTable.GetUnitdeclFromId(node.GetId().ToString()) != null ? Symbol.Ok : Symbol.NotOk);
    }
    public override void OutADenUnituse(ADenUnituse node)
    {
        symbolTable.AddNode(node,
            symbolTable.GetUnitdeclFromId(node.GetId().ToString()) != null ? Symbol.Ok : Symbol.NotOk);
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
        symbolTable = symbolTable.EnterScope();
        AddArgsToScope(node, node.GetArg());
    }
    public override void OutAUntypedGlobal(AUntypedGlobal node)
    {
        string symbols = "";
        // Stacktracing
        // Check om args er ok
        // Check om stmts er ok
        bool untypedIsOk = true;
        foreach (AArg arg in node.GetArg())
        {
            symbols += symbolTable.GetSymbol(arg).ToString();

            if (symbolTable.GetSymbol(arg) == Symbol.NotOk)
                untypedIsOk = false;
        }
        foreach (PStmt stmt in node.GetStmt())
        {
            symbols += symbolTable.GetSymbol(stmt).ToString();

            if (symbolTable.GetSymbol(stmt) == Symbol.NotOk)
                untypedIsOk = false;
        }
        // throw new Exception(symbols);

        symbolTable = symbolTable.ExitScope();
        symbolTable.AddNode(node, untypedIsOk ? Symbol.Ok : Symbol.NotOk);
    }
    public override void InATypedGlobal(ATypedGlobal node)
    {
        symbolTable = symbolTable.EnterScope();
        AddArgsToScope(node, node.GetArg());
    }
    public override void OutATypedGlobal(ATypedGlobal node)
    {
        string symbols = "";
        // Stacktracing
        // Check om args er ok
        // Check om stmts er ok
        bool untypedIsOk = true;
        foreach (AArg arg in node.GetArg())
        {
            symbols += symbolTable.GetSymbol(arg).ToString();
            if (symbolTable.GetSymbol(arg) == Symbol.NotOk)
                untypedIsOk = false;
        }
        foreach (PStmt stmt in node.GetStmt())
        {
            symbols += symbolTable.GetSymbol(stmt).ToString();
            if (symbolTable.GetSymbol(stmt) == Symbol.NotOk)
                untypedIsOk = false;
        }
        //throw new Exception(symbols);
        
        symbolTable = symbolTable.ExitScope();
        if (symbolTable.GetSymbol(node) != null)
        {
            
        }
        symbolTable.AddNode(node, untypedIsOk ? Symbol.Ok : Symbol.NotOk);
    }
    public override void InALoopGlobal(ALoopGlobal node)
    {
        symbolTable = symbolTable.EnterScope();
    }

    public override void OutALoopGlobal(ALoopGlobal node)
    {
        symbolTable = symbolTable.ExitScope();
        if (symbolTable.GetSymbol(node) != null)
        {
            bool loopIsOk = true;
            foreach (PStmt stmt in node.GetStmt())
            {
                if (symbolTable.GetSymbol(stmt) == Symbol.NotOk)
                    loopIsOk = false;
            }

            symbolTable.AddNode(node, loopIsOk ? Symbol.Ok : Symbol.NotOk);
        }
    }

    public override void InAProgGlobal(AProgGlobal node)
    {
        symbolTable = symbolTable.EnterScope();
    }
    public override void OutAProgGlobal(AProgGlobal node)
    {
        symbolTable = symbolTable.ExitScope();
        if (symbolTable.GetSymbol(node) != null)
        {
            bool loopIsOk = true;
            foreach (PStmt stmt in node.GetStmt())
            {
                if (symbolTable.GetSymbol(stmt) == Symbol.NotOk)
                    loopIsOk = false;
            }
            symbolTable.AddNode(node, loopIsOk ? Symbol.Ok : Symbol.NotOk);
        }
    }
}