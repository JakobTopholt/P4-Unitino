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
    
    public override void CaseAGrammar(AGrammar node)
    {
        InAGrammar(node);
        {
            Object[] temp = new Object[node.GetGlobal().Count];
            node.GetGlobal().CopyTo(temp, 0);
            for(int i = 0; i < temp.Length; i++)
            {
                if (temp[i] is AProgGlobal or ALoopGlobal or AUntypedGlobal or ATypedGlobal or AUnitdeclGlobal)
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
        symbolTable.AddNode(node, grammarIsOk ? Symbol.Ok : Symbol.NotOk);
    }
    
    
    public override void CaseAUnitdeclGlobal(AUnitdeclGlobal node)
    {
        //symbolTable = symbolTable.EnterScope().ExitScope();
        //symbolTable = symbolTable.ResetScope();
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
        unitUseDens.Add(id, unitDecl);
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
        string id = node.GetId().Text;
            switch (node.GetType())
            {
                case AIntType:
                    symbolTable.AddIdToNode(id, node);
                    symbolTable.AddNode(node, Symbol.Int);
                    break;
                case ADecimalType:
                    symbolTable.AddIdToNode(id, node);
                    symbolTable.AddNode(node, Symbol.Decimal);
                    break;
                case ABoolType:
                    symbolTable.AddIdToNode(id, node);
                    symbolTable.AddNode(node, Symbol.Bool);
                    break;
                case ACharType:
                    symbolTable.AddIdToNode(id, node);
                    symbolTable.AddNode(node, Symbol.Char);
                    break;
                case AStringType:
                    symbolTable.AddIdToNode(id, node);
                    symbolTable.AddNode(node, Symbol.String);
                    break;
                case APinType:
                    symbolTable.AddIdToNode(id, node);
                    symbolTable.AddNode(node, Symbol.Pin);
                    break;
                case AUnitType customType:
                {
                    symbolTable.GetUnit(customType, out var unit);
                    symbolTable.AddNodeToUnit(node, unit);
                    symbolTable.AddNode(node, Symbol.Ok);
                    symbolTable.AddIdToNode(id, node);
                    break;
                }
                default:
                    symbolTable.AddNode(node, Symbol.NotOk);
                    symbolTable.AddIdToNode(id, node);
                    tempResult += IndentedString("Is not a valid type\n");
                    break;
            }
            PrintError();
        indent--;
    }

    public override void CaseAUntypedGlobal(AUntypedGlobal node)
    {
        if (symbolTable._parent == null)
        {
            symbolTable = symbolTable.EnterScope().ExitScope();
            symbolTable = symbolTable.ResetScope();
        }
        else
        {
            InAUntypedGlobal(node);
            if(node.GetId() != null)
            {
                node.GetId().Apply(this);
            }
            {
                Object[] temp = new Object[node.GetArg().Count];
                node.GetArg().CopyTo(temp, 0);
                for(int i = 0; i < temp.Length; i++)
                {
                    ((PArg) temp[i]).Apply(this);
                }
            }
            {
                Object[] temp = new Object[node.GetStmt().Count];
                node.GetStmt().CopyTo(temp, 0);
                for(int i = 0; i < temp.Length; i++)
                {
                    ((PStmt) temp[i]).Apply(this);
                }
            }
            OutAUntypedGlobal(node);
        }
    }

    public override void InAUntypedGlobal(AUntypedGlobal node)
    {
        locations.Push($"In function {node.GetId()}\n");
        indent++;
        symbolTable = symbolTable.EnterScope();
        //symbolTable.AddArgsToScope(node, node.GetArg());
    }

    public override void OutAUntypedGlobal(AUntypedGlobal node)
    {
        // no returnstmts
        if (!node.GetStmt().OfType<AReturnStmt>().Any())
        {
            symbolTable = symbolTable.ExitScope();
            symbolTable.AddNode(node, Symbol.Func);
        }
        else
        {
            symbolTable = symbolTable.ExitScope();
        }
        locations.Clear();
        reported = false;
        indent--;
    }

    public override void CaseATypedGlobal(ATypedGlobal node)
    {
        if (symbolTable._parent == null)
        {
            symbolTable = symbolTable.EnterScope().ExitScope();
            symbolTable = symbolTable.ResetScope();
        }
        else
        {
            InATypedGlobal(node);
            if(node.GetType() != null)
            {
                node.GetType().Apply(this);
            }
            if(node.GetId() != null)
            {
                node.GetId().Apply(this);
            }
            {
                Object[] temp = new Object[node.GetArg().Count];
                node.GetArg().CopyTo(temp, 0);
                for(int i = 0; i < temp.Length; i++)
                {
                    ((PArg) temp[i]).Apply(this);
                }
            }
            {
                Object[] temp = new Object[node.GetStmt().Count];
                node.GetStmt().CopyTo(temp, 0);
                for(int i = 0; i < temp.Length; i++)
                {
                    ((PStmt) temp[i]).Apply(this);
                }
            }
            OutATypedGlobal(node);
        }
    }

    public override void InATypedGlobal(ATypedGlobal node)
    {
        locations.Push($"In function {node.GetId()}\n");
        indent++;
        symbolTable = symbolTable.EnterScope();
        //symbolTable.AddArgsToScope(node, node.GetArg());
    }
    public override void OutATypedGlobal(ATypedGlobal node)
    {
        // Check om args er ok
        // Check om stmts er ok
        PType typedType = node.GetType();
        switch (typedType)
        {
                case AIntType:
                    symbolTable = symbolTable.ExitScope();
                    symbolTable.AddNode(node, Symbol.Int);
                    break;
                case ADecimalType:
                    symbolTable = symbolTable.ExitScope();
                    symbolTable.AddNode(node, Symbol.Decimal);
                    break;
                case ABoolType:
                    symbolTable = symbolTable.ExitScope();
                    symbolTable.AddNode(node, Symbol.Bool);
                    break;
                case ACharType:
                    symbolTable = symbolTable.ExitScope();
                    symbolTable.AddNode(node, Symbol.Char);
                    break;
                case AStringType:
                    symbolTable = symbolTable.ExitScope();
                    symbolTable.AddNode(node, Symbol.String);
                    break;
                case APinType:
                    symbolTable = symbolTable.ExitScope();
                    symbolTable.AddNode(node, Symbol.Pin);
                    break;
                case AVoidType:
                    symbolTable = symbolTable.ExitScope();
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
        symbolTable = symbolTable.EnterScope();
    }

    public override void OutALoopGlobal(ALoopGlobal node)
    {
        bool loopIsOk = true;
        if (symbolTable.GetSymbol(node) == null)
        {
            List<PStmt> stmts = node.GetStmt().OfType<PStmt>().ToList();
            foreach (PStmt stmt in stmts)
            {
                if (symbolTable.GetSymbol(stmt) == Symbol.NotOk)
                    loopIsOk = false;
            }
        }
        symbolTable = symbolTable.ExitScope();
        symbolTable.AddNode(node, loopIsOk ? Symbol.Ok : Symbol.NotOk);
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

}