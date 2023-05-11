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
            if (symbolTable.GetSymbol(global) == Symbol.notOk)
                grammarIsOk = false;
        }
        symbolTable.AddNode(node, grammarIsOk ? Symbol.ok : Symbol.notOk);
    }

    public override void OutAUnitType(AUnitType node)
    {
        List<ANumUnituse> nums = node.GetUnituse().OfType<ANumUnituse>().ToList();
        List<ADenUnituse> dens = node.GetUnituse().OfType<ADenUnituse>().ToList();
        List<AUnitdeclGlobal> newNums = new List<AUnitdeclGlobal>();
        List<AUnitdeclGlobal> newDens = new List<AUnitdeclGlobal>();

        foreach (ANumUnituse num in nums)
        {
            AUnitdeclGlobal? newNum = symbolTable.GetUnitFromId(num.GetId().ToString());
            if (newNum != null)
            {
                newNums.Add(newNum);
            }
            else
            {
                // Not a recognized unit
                symbolTable.AddNode(node, Symbol.notOk);
            }
        }
        foreach (ADenUnituse den in dens)
        {
            AUnitdeclGlobal? newDen = symbolTable.GetUnitFromId(den.GetId().ToString());
            if (newDen != null)
            {
                newDens.Add(newDen);
            }
            else
            {
                // Not a recognized unit
                symbolTable.AddNode(node, Symbol.notOk);
            }
        }
        Tuple<List<AUnitdeclGlobal>, List<AUnitdeclGlobal>> unituse = new Tuple<List<AUnitdeclGlobal>, List<AUnitdeclGlobal>>(newNums, newDens);
        symbolTable.AddNodeToUnit(node, unituse);
        symbolTable.AddNode(node, Symbol.ok);
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

    public override void InAUntypedGlobal(AUntypedGlobal node) 
    {
        symbolTable = symbolTable.EnterScope();
    }
    public override void OutAUntypedGlobal(AUntypedGlobal node) 
    {
        symbolTable = symbolTable.ExitScope();
    }
    public override void InATypedGlobal(ATypedGlobal node)
    {
        symbolTable = symbolTable.EnterScope();
    }
    public override void OutATypedGlobal(ATypedGlobal node)
    {
        symbolTable = symbolTable.ExitScope();
    }
    public override void InALoopGlobal(ALoopGlobal node)
    {
        symbolTable = symbolTable.EnterScope();
    }
    public override void OutALoopGlobal(ALoopGlobal node)
    {
        symbolTable = symbolTable.ExitScope();
    }
    public override void InAProgGlobal(AProgGlobal node)
    {
        symbolTable = symbolTable.EnterScope();
    }
    public override void OutAProgGlobal(AProgGlobal node)
    {
        symbolTable = symbolTable.ExitScope();
    }
}