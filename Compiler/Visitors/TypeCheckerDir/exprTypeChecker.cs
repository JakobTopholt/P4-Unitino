﻿using System.Collections;
using Microsoft.VisualBasic;

namespace Compiler.Visitors.TypeCheckerDir;
using Moduino.node;
using Moduino.analysis;

public class exprTypeChecker : stmtTypeChecker
{
    public exprTypeChecker(SymbolTable symbolTable) : base(symbolTable)
    {

    }

    public override void OutAExpExp(AExpExp node)
    {
        Symbol? type = symbolTable.GetSymbol(node.GetExp());
        switch (type)
        {
            case Symbol.Int:
                symbolTable.AddNode(node, Symbol.Int);
                break;
            case Symbol.Decimal:
                symbolTable.AddNode(node, Symbol.Decimal);
                break;
            case Symbol.Bool:
                symbolTable.AddNode(node, Symbol.Bool);
                break;
            case Symbol.Char:
                symbolTable.AddNode(node, Symbol.Char);
                break;
            case Symbol.String:
                symbolTable.AddNode(node, Symbol.String);
                break;
            case Symbol.Func:
                symbolTable.AddNode(node, Symbol.Func);
                break;
            case Symbol.ok:
                symbolTable.AddNode(node, Symbol.ok);
                break;
            default:
                var unit = symbolTable.GetUnit(node.GetExp());
                symbolTable.AddNode(node, unit != null ? Symbol.ok : Symbol.notOk);
                break;
        }
    }

    public override void OutAUnitType(AUnitType node)
    {
        // Save reference from node to tuple
        // Implement logic here

        List<AUnitdeclGlobal> newNums = new List<AUnitdeclGlobal>();
        List<AUnitdeclGlobal> newDens = new List<AUnitdeclGlobal>();

        Tuple<List<AUnitdeclGlobal>, List<AUnitdeclGlobal>> unit =
            new Tuple<List<AUnitdeclGlobal>, List<AUnitdeclGlobal>>(newNums, newDens);
        symbolTable.AddNodeToUnit(node, unit);
    }

    public override void OutADecimalExp(ADecimalExp node) => symbolTable.AddNode(node, Symbol.Decimal);
    public override void OutANumberExp(ANumberExp node) => symbolTable.AddNode(node, Symbol.Int);
    public override void OutABooleanExp(ABooleanExp node) => symbolTable.AddNode(node, Symbol.Bool);
    public override void OutATrueBoolean(ATrueBoolean node) => symbolTable.AddNode(node, Symbol.Bool);
    public override void OutAFalseBoolean(AFalseBoolean node) => symbolTable.AddNode(node, Symbol.Bool);
    public override void OutAStringExp(AStringExp node) => symbolTable.AddNode(node, Symbol.String);
    public override void OutACharExp(ACharExp node) => symbolTable.AddNode(node, Symbol.Char);

    public override void OutAValueExp(AValueExp node)
    {
        //symbolTable.AddNode(node, Symbol.Decimal);
        // ----- Logic missing here---- (tag stilling til hvad det under betød)
        symbolTable.AddNode(node, UnitVisitor.StateUnit ? Symbol.Decimal : Symbol.notOk);
    }

    public override void OutAFunccallExp(AFunccallExp node)
    {
        // Funccall som en del af et return udtryk
        // returnvalue har betydning for om det er et correct call

        List<PType> args = symbolTable.GetFunctionArgs(symbolTable.GetFuncFromId(node.GetId().ToString()));
        List<PExp>? parameters = node.GetExp() as List<PExp>;
        if (args.Count() == parameters.Count())
        {
            bool matches = true;
            for (int i = 0; i < args.Count(); i++)
            {
                switch (args[i])
                {
                    case AIntType:
                        if (symbolTable.GetSymbol(parameters[i]) != Symbol.Int)
                            matches = false;
                        break;
                    case ADecimalType:
                        if (symbolTable.GetSymbol(parameters[i]) != Symbol.Decimal)
                            matches = false;
                        break;
                    case ABoolType:
                        if (symbolTable.GetSymbol(parameters[i]) != Symbol.Bool)
                            matches = false;
                        break;
                    case ACharType:
                        if (symbolTable.GetSymbol(parameters[i]) != Symbol.Char)
                            matches = false;
                        break;
                    case AStringType:
                        if (symbolTable.GetSymbol(parameters[i]) != Symbol.String)
                            matches = false;
                        break;
                    case AUnitType argType:
                    {
                        var argUnit = symbolTable.GetUnit(argType);
                        var paramUnit = symbolTable.GetUnit(parameters[i]);

                        if (!symbolTable.CompareCustomUnits(argUnit, paramUnit))
                        {
                            matches = false;
                            symbolTable.AddNodeToUnit(node, argUnit);
                        }

                        break;
                    }
                    default:
                        matches = false;
                        break;
                }
            }

            if (matches)
            {
                var symbol = symbolTable.GetSymbol(symbolTable.GetNodeFromId(node.GetId().ToString()));
                switch (symbol)
                {
                    case Symbol.Int:
                        symbolTable.AddNode(node, Symbol.Int);
                        break;
                    case Symbol.Decimal:
                        symbolTable.AddNode(node, Symbol.Decimal);
                        break;
                    case Symbol.Bool:
                        symbolTable.AddNode(node, Symbol.Bool);
                        break;
                    case Symbol.Char:
                        symbolTable.AddNode(node, Symbol.Char);
                        break;
                    case Symbol.String:
                        symbolTable.AddNode(node, Symbol.String);
                        break;
                    default:
                        var unit = symbolTable.GetUnit(symbolTable.GetNodeFromId(node.GetId().ToString()));
                        if (unit != null)
                        {
                            symbolTable.AddNodeToUnit(node, unit);
                            symbolTable.AddNode(node, Symbol.ok);
                        }
                        else
                        {
                            symbolTable.AddNode(node, Symbol.notOk);
                        }

                        break;
                }
            }
            else
            {
                symbolTable.AddNode(node, Symbol.notOk);
            }
        }
        else
        {
            symbolTable.AddNode(node, Symbol.notOk);
        }
    }

    public override void OutAIdExp(AIdExp node)
    {
        switch (symbolTable.GetSymbol(node.GetId().ToString().Trim()))
        {
            case Symbol.Int:
                symbolTable.AddNode(node, Symbol.Int);
                break;
            case Symbol.Decimal:
                symbolTable.AddNode(node, Symbol.Decimal);
                break;
            case Symbol.Bool:
                symbolTable.AddNode(node, Symbol.Bool);
                break;
            case Symbol.Char:
                symbolTable.AddNode(node, Symbol.Char);
                break;
            case Symbol.String:
                symbolTable.AddNode(node, Symbol.String);
                break;
            default:
                var unit = symbolTable.GetUnit(node.GetId().ToString().Trim());
                if (unit != null)
                {
                    symbolTable.AddNodeToUnit(node, unit);
                    symbolTable.AddNode(node, Symbol.ok);
                }
                else
                {
                    symbolTable.AddNode(node, Symbol.notOk);
                }

                break;
        }
    }

    public override void OutAUnitdecimalExp(AUnitdecimalExp node)
    {
        // A single unitnumber eg. 50ms
        throw new Exception(node.GetId().ToString().Trim());
        AUnitdeclGlobal unitType = symbolTable.GetUnitdeclFromId(node.GetId().ToString().Trim());
        if (unitType != null)
        {
            // Create a new unit tuple and add the unitnumber as a lone numerator
            List<AUnitdeclGlobal> nums = new List<AUnitdeclGlobal>();
            nums.Add(unitType);
            List<AUnitdeclGlobal> dens = new List<AUnitdeclGlobal>();
            var unit = new Tuple<List<AUnitdeclGlobal>, List<AUnitdeclGlobal>>(nums, dens);
            symbolTable.AddNodeToUnit(node, unit);
            symbolTable.AddNode(node, Symbol.ok);
        }
        else
        {
            // Id is not a valid subunit
            symbolTable.AddNode(node, Symbol.notOk);
        }
    }

    public override void OutAUnitnumberExp(AUnitnumberExp node)
    {
        // A single unitnumber eg. 50ms
        AUnitdeclGlobal unitType = symbolTable.GetUnitdeclFromId(node.GetId().ToString());
        if (unitType != null)
        {
            // Create a new unit tuple and add the unitnumber as a lone numerator
            List<AUnitdeclGlobal> nums = new List<AUnitdeclGlobal>();
            nums.Add(unitType);
            List<AUnitdeclGlobal> dens = new List<AUnitdeclGlobal>();
            var unit = new Tuple<List<AUnitdeclGlobal>, List<AUnitdeclGlobal>>(nums, dens);

            // Map node to the unit
            symbolTable.AddNodeToUnit(node, unit);
            symbolTable.AddNode(node, Symbol.ok);
        }
        else
        {
            // Id is not a valid subunit
            symbolTable.AddNode(node, Symbol.notOk);
        }
    }
    public override void OutADivideExp(ADivideExp node)
    {
        PExp leftExpr = node.GetL();
        PExp rightExpr = node.GetR();
        Symbol? leftSymbol = symbolTable.GetSymbol(leftExpr);
        Symbol? rightSymbol = symbolTable.GetSymbol(rightExpr);
        if (leftExpr is ANumberExp or ADecimalExp && rightExpr is AIdExp id)
        {
            AUnitdeclGlobal? unitDecl = symbolTable.GetUnitdeclFromId(id.GetId().ToString().Trim());
            //throw new Exception(unitD);
            symbolTable.AddNodeToUnit(node, symbolTable.GetUnit(unitDecl));
            symbolTable.AddNode(node, Symbol.ok);
        }
        else
        {
          switch (leftSymbol)
        {
            case Symbol.Int when rightSymbol is Symbol.Int:
                symbolTable.AddNode(node, Symbol.Int);
                break;
            case Symbol.Decimal when rightSymbol is Symbol.Decimal or Symbol.Int:
                symbolTable.AddNode(node, Symbol.Decimal);
                break;
            case Symbol.Decimal or Symbol.Int when rightSymbol is Symbol.Decimal:
                symbolTable.AddNode(node, Symbol.Decimal);
                break;
            default:
                bool leftContainsUnit = symbolTable.nodeToUnit.ContainsKey(leftExpr);
                bool rightContainsUnit = symbolTable.nodeToUnit.ContainsKey(rightExpr);
                if (symbolTable.nodeToUnit.ContainsKey(leftExpr) && symbolTable.nodeToUnit.ContainsKey(rightExpr))
                {
                    Tuple<List<AUnitdeclGlobal>, List<AUnitdeclGlobal>> left = symbolTable.GetUnit(leftExpr); // unit 1
                    Tuple<List<AUnitdeclGlobal>, List<AUnitdeclGlobal>> right = symbolTable.GetUnit(rightExpr); // unit 2

                    List<AUnitdeclGlobal> a = left.Item1;
                    List<AUnitdeclGlobal> b = left.Item2;
                    List<AUnitdeclGlobal> c = right.Item1;
                    List<AUnitdeclGlobal> d = right.Item2;

                    List<AUnitdeclGlobal> ac = a.Intersect(c).ToList();
                    List<AUnitdeclGlobal> bd = b.Intersect(d).ToList();

                    List<AUnitdeclGlobal> numerators = a.Except(ac).Union(d.Except(bd)).ToList();
                    List<AUnitdeclGlobal> denomerators = c.Except(ac).Union(b.Except(bd)).ToList();

                    Tuple<List<AUnitdeclGlobal>, List<AUnitdeclGlobal>> unituse =
                        new Tuple<List<AUnitdeclGlobal>, List<AUnitdeclGlobal>>(numerators, denomerators);
                    symbolTable.AddNodeToUnit(node, unituse);
                    symbolTable.AddNode(node, Symbol.ok);
                }
                else if ((leftContainsUnit || rightContainsUnit) && (symbolTable.GetSymbol(leftExpr) == Symbol.Int || symbolTable.GetSymbol(leftExpr) == Symbol.Decimal) 
                         || (symbolTable.GetSymbol(rightExpr) == Symbol.Int || symbolTable.GetSymbol(rightExpr) == Symbol.Decimal))
                {
                    // Unitnumber + decimal/int eller decimal/int + UnitNumber
                    var unit = leftContainsUnit ? symbolTable.GetUnit(leftExpr) :
                        rightContainsUnit ? symbolTable.GetUnit(rightExpr) : null;
                    
                    if (unit != null)
                    {
                        symbolTable.AddNodeToUnit(node, unit);
                        symbolTable.AddNode(node, Symbol.ok);
                    }
                    else
                    {
                        symbolTable.AddNode(node, Symbol.notOk);
                    }
                }
                else
                {
                    // not valid input expressions to a divide expression
                    symbolTable.AddNode(node, Symbol.notOk);
                }
                break;
        }  
        }
    }
    public override void OutAMultiplyExp(AMultiplyExp node)
    {
        PExp leftExpr = node.GetL();
        PExp rightExpr = node.GetR();
        Symbol? leftSymbol = symbolTable.GetSymbol(leftExpr);
        Symbol? rightSymbol = symbolTable.GetSymbol(rightExpr);
        switch (leftSymbol)
        {
            case Symbol.Int when rightSymbol is Symbol.Int:
                symbolTable.AddNode(node, Symbol.Int);
                break;
            case Symbol.Decimal when rightSymbol is Symbol.Decimal or Symbol.Int:
                symbolTable.AddNode(node, Symbol.Decimal);
                break;
            case Symbol.Decimal or Symbol.Int when rightSymbol is Symbol.Decimal:
                symbolTable.AddNode(node, Symbol.Decimal);
                break;
            default:
                bool leftContainsUnit = symbolTable.nodeToUnit.ContainsKey(leftExpr);
                bool rightContainsUnit = symbolTable.nodeToUnit.ContainsKey(rightExpr);
                if (symbolTable.nodeToUnit.ContainsKey(leftExpr) && symbolTable.nodeToUnit.ContainsKey(rightExpr))
                {
                    Tuple<List<AUnitdeclGlobal>, List<AUnitdeclGlobal>> left = symbolTable.GetUnit(leftExpr); // unit 1
                    Tuple<List<AUnitdeclGlobal>, List<AUnitdeclGlobal>> right = symbolTable.GetUnit(rightExpr); // unit 2

                    List<AUnitdeclGlobal> a = left.Item1;
                    List<AUnitdeclGlobal> b = left.Item2;
                    List<AUnitdeclGlobal> c = right.Item1;
                    List<AUnitdeclGlobal> d = right.Item2;

                    List<AUnitdeclGlobal> ad = a.Intersect(d).ToList();
                    List<AUnitdeclGlobal> bc = b.Intersect(c).ToList();

                    List<AUnitdeclGlobal> numerators = a.Except(ad).Union(d.Except(bc)).ToList();
                    List<AUnitdeclGlobal> denomerators = c.Except(ad).Union(b.Except(bc)).ToList();

                    Tuple<List<AUnitdeclGlobal>, List<AUnitdeclGlobal>> unituse =
                        new Tuple<List<AUnitdeclGlobal>, List<AUnitdeclGlobal>>(numerators, denomerators);
                    symbolTable.AddNodeToUnit(node, unituse);
                    symbolTable.AddNode(node, Symbol.ok);
                }
                else if ((leftContainsUnit || rightContainsUnit) && (symbolTable.GetSymbol(leftExpr) == Symbol.Int || symbolTable.GetSymbol(leftExpr) == Symbol.Decimal) 
                         || (symbolTable.GetSymbol(rightExpr) == Symbol.Int || symbolTable.GetSymbol(rightExpr) == Symbol.Decimal))
                {
                    // Unitnumber + decimal/int eller decimal/int + UnitNumber
                    var unit = leftContainsUnit ? symbolTable.GetUnit(leftExpr) :
                        rightContainsUnit ? symbolTable.GetUnit(rightExpr) : null;
                    
                    if (unit != null)
                    {
                        symbolTable.AddNodeToUnit(node, unit);
                        symbolTable.AddNode(node, Symbol.ok);
                    }
                    else
                    {
                        symbolTable.AddNode(node, Symbol.notOk);
                    }
                }
                else
                {
                    // not valid input expressions to a multiply expression
                    symbolTable.AddNode(node, Symbol.notOk);
                }
                break;
        }
    }

    public override void OutAPlusExp(APlusExp node)
    {
        PExp leftExpr = node.GetL();
        PExp rightExpr = node.GetR();
        Symbol? leftSymbol = symbolTable.GetSymbol(leftExpr);
        Symbol? rightSymbol = symbolTable.GetSymbol(rightExpr);
        switch (leftSymbol)
        {
            case Symbol.Int when rightSymbol is Symbol.Int:
                symbolTable.AddNode(node, Symbol.Int);
                break;
            case Symbol.Decimal or Symbol.Int when rightSymbol is Symbol.Decimal or Symbol.Int:
                symbolTable.AddNode(node, Symbol.Decimal);
                break;
            case Symbol.Int or Symbol.Decimal when rightSymbol is Symbol.Decimal:
                symbolTable.AddNode(node, Symbol.Decimal);
                break;
            case Symbol.String when rightSymbol is Symbol.Int or Symbol.String:
                symbolTable.AddNode(node, Symbol.String);
                break;
            case Symbol.Int or Symbol.String when rightSymbol is Symbol.String:
                symbolTable.AddNode(node, Symbol.String);
                break;
            case Symbol.Decimal or Symbol.String when rightSymbol is Symbol.Decimal:
                symbolTable.AddNode(node, Symbol.String);
                break;
            case Symbol.String when rightSymbol is Symbol.String or Symbol.Decimal:
                symbolTable.AddNode(node, Symbol.String);
                break;
            default:
                // Implement logikken for custom units her
                bool leftContainsUnit = symbolTable.nodeToUnit.ContainsKey(leftExpr);
                bool rightContainsUnit = symbolTable.nodeToUnit.ContainsKey(rightExpr);
                if (symbolTable.nodeToUnit.ContainsKey(leftExpr) && symbolTable.nodeToUnit.ContainsKey(rightExpr))
                {
                    // har burgt noget nullable warning?
                    Tuple<List<AUnitdeclGlobal>, List<AUnitdeclGlobal>> left = symbolTable.GetUnit(leftExpr); // unit 1
                    Tuple<List<AUnitdeclGlobal>, List<AUnitdeclGlobal>> right = symbolTable.GetUnit(rightExpr); // unit 2

                    List<AUnitdeclGlobal> a = left.Item1;
                    List<AUnitdeclGlobal> b = left.Item2;
                    List<AUnitdeclGlobal> c = right.Item1;
                    List<AUnitdeclGlobal> d = right.Item2;

                    var sortedNums1 = a.OrderBy(x => x).ToList();
                    bool isEmptyNums1 = sortedNums1.Count == 0;
                    var sortedNums2 = c.OrderBy(x => x).ToList();
                    bool isEmptyNums2 = sortedNums2.Count == 0;
                    var sortedDens1 = b.OrderBy(x => x).ToList();
                    bool isEmptyDens1 = sortedDens1.Count == 0;
                    var sortedDens2 = d.OrderBy(x => x).ToList();
                    bool isEmptyDens2 = sortedDens2.Count == 0;

                    bool dontCompareNums = isEmptyNums1 || isEmptyNums2;
                    bool dontCompareDens = isEmptyDens1 || isEmptyDens2;

                    if ((dontCompareNums || sortedNums1.SequenceEqual(sortedNums2)) &&
                        (dontCompareDens || sortedDens1.SequenceEqual(sortedDens2)))
                    {
                        // Create a new unitTyple and add it to NodeToUnit and return symbol.ok
                        List<AUnitdeclGlobal> numerators = isEmptyNums1
                            ? isEmptyNums2 ? new List<AUnitdeclGlobal>() : sortedNums2
                            : sortedNums1;
                        List<AUnitdeclGlobal> denomerators = isEmptyDens1
                            ? isEmptyDens2 ? new List<AUnitdeclGlobal>() : sortedDens2
                            : sortedDens1;

                        Tuple<List<AUnitdeclGlobal>, List<AUnitdeclGlobal>> unituse =
                            new Tuple<List<AUnitdeclGlobal>, List<AUnitdeclGlobal>>(numerators, denomerators);
                        symbolTable.AddNodeToUnit(node, unituse);
                        symbolTable.AddNode(node, Symbol.ok);
                    }
                    else
                    {
                        symbolTable.AddNode(node, Symbol.notOk);
                    }
                }
                else if ((leftContainsUnit || rightContainsUnit) && (symbolTable.GetSymbol(leftExpr) == Symbol.Int || symbolTable.GetSymbol(leftExpr) == Symbol.Decimal) 
                         || (symbolTable.GetSymbol(rightExpr) == Symbol.Int || symbolTable.GetSymbol(rightExpr) == Symbol.Decimal))
                {
                    // Unitnumber + decimal/int eller decimal/int + UnitNumber
                    var unit = leftContainsUnit ? symbolTable.GetUnit(leftExpr) :
                        rightContainsUnit ? symbolTable.GetUnit(rightExpr) : null;
                    
                    if (unit != null)
                    {
                        symbolTable.AddNodeToUnit(node, unit);
                        symbolTable.AddNode(node, Symbol.ok);
                    }
                    else
                    {
                        symbolTable.AddNode(node, Symbol.notOk);
                    }
                }
                else
                {
                    // not valid input expression
                    symbolTable.AddNode(node, Symbol.notOk);
                }
                break;
        }
    }

    public override void OutAMinusExp(AMinusExp node)
    {
        PExp leftExpr = node.GetL();
        PExp rightExpr = node.GetR();
        Symbol? leftSymbol = symbolTable.GetSymbol(leftExpr);
        Symbol? rightSymbol = symbolTable.GetSymbol(rightExpr);
        switch (leftSymbol)
        {
            case Symbol.Int when rightSymbol is Symbol.Int:
                symbolTable.AddNode(node, Symbol.Int);
                break;
            case Symbol.Decimal when rightSymbol is Symbol.Decimal or Symbol.Int:
                symbolTable.AddNode(node, Symbol.Decimal);
                break;
            case Symbol.Decimal or Symbol.Int when rightSymbol is Symbol.Decimal:
                symbolTable.AddNode(node, Symbol.Decimal);
                break;
            default:
                // Implement logikken for custom units her
                bool leftContainsUnit = symbolTable.nodeToUnit.ContainsKey(leftExpr);
                bool rightContainsUnit = symbolTable.nodeToUnit.ContainsKey(rightExpr);
                if (symbolTable.nodeToUnit.ContainsKey(leftExpr) && symbolTable.nodeToUnit.ContainsKey(rightExpr))
                {
                    Tuple<List<AUnitdeclGlobal>, List<AUnitdeclGlobal>> left = symbolTable.GetUnit(leftExpr); // unit 1
                    Tuple<List<AUnitdeclGlobal>, List<AUnitdeclGlobal>> right = symbolTable.GetUnit(rightExpr); // unit 2

                    List<AUnitdeclGlobal> a = left.Item1;
                    List<AUnitdeclGlobal> b = left.Item2;
                    List<AUnitdeclGlobal> c = right.Item1;
                    List<AUnitdeclGlobal> d = right.Item2;

                    var sortedNums1 = a.OrderBy(x => x).ToList();
                    bool isEmptyNums1 = sortedNums1.Count == 0;
                    var sortedNums2 = c.OrderBy(x => x).ToList();
                    bool isEmptyNums2 = sortedNums2.Count == 0;
                    var sortedDens1 = b.OrderBy(x => x).ToList();
                    bool isEmptyDens1 = sortedDens1.Count == 0;
                    var sortedDens2 = d.OrderBy(x => x).ToList();
                    bool isEmptyDens2 = sortedDens2.Count == 0;

                    bool dontCompareNums = isEmptyNums1 || isEmptyNums2;
                    bool dontCompareDens = isEmptyDens1 || isEmptyDens2;

                    if ((dontCompareNums || sortedNums1.SequenceEqual(sortedNums2)) &&
                        (dontCompareDens || sortedDens1.SequenceEqual(sortedDens2)))
                    {
                        // Create a new unitTyple and add it to NodeToUnit and return symbol.ok
                        List<AUnitdeclGlobal> numerators = isEmptyNums1
                            ? isEmptyNums2 ? new List<AUnitdeclGlobal>() : sortedNums2
                            : sortedNums1;
                        List<AUnitdeclGlobal> denomerators = isEmptyDens1
                            ? isEmptyDens2 ? new List<AUnitdeclGlobal>() : sortedDens2
                            : sortedDens1;

                        Tuple<List<AUnitdeclGlobal>, List<AUnitdeclGlobal>> unituse =
                            new Tuple<List<AUnitdeclGlobal>, List<AUnitdeclGlobal>>(numerators, denomerators);
                        symbolTable.AddNodeToUnit(node, unituse);
                        symbolTable.AddNode(node, Symbol.ok);
                    }
                    else
                    {
                        symbolTable.AddNode(node, Symbol.notOk);
                    }
                }
                else if ((leftContainsUnit || rightContainsUnit) && (symbolTable.GetSymbol(leftExpr) == Symbol.Int || symbolTable.GetSymbol(leftExpr) == Symbol.Decimal) 
                         || (symbolTable.GetSymbol(rightExpr) == Symbol.Int || symbolTable.GetSymbol(rightExpr) == Symbol.Decimal))
                {
                    // Unitnumber + decimal/int eller decimal/int + UnitNumber
                    var unit = leftContainsUnit ? symbolTable.GetUnit(leftExpr) :
                        rightContainsUnit ? symbolTable.GetUnit(rightExpr) : null;
                    
                    if (unit != null)
                    {
                        symbolTable.AddNodeToUnit(node, unit);
                        symbolTable.AddNode(node, Symbol.ok);
                    }
                    else
                    {
                        symbolTable.AddNode(node, Symbol.notOk);
                    }
                }
                else
                {
                    // not valid input expressions to a multiply expression
                    symbolTable.AddNode(node, Symbol.notOk);
                }
                break;
        }
    }

    public override void OutARemainderExp(ARemainderExp node)
    {
        throw new Exception("Yoyoy");
        PExp L = node.GetL();
        PExp R = node.GetR();
        
        Symbol? left = symbolTable.GetSymbol(L);
        Symbol? right = symbolTable.GetSymbol(R);
        switch (left)
        {
            case Symbol.Int:
                if (right == Symbol.Int)
                {
                    symbolTable.AddNode(node, Symbol.Int);
                } else if (right == Symbol.Decimal)
                {
                    symbolTable.AddNode(node, Symbol.Decimal);
                }
                else
                { 
                    symbolTable.AddNode(node, Symbol.notOk);  
                }
                break;
            case Symbol.Decimal:
                if (right == Symbol.Int || right == Symbol.Decimal)
                {
                    symbolTable.AddNode(node, Symbol.Decimal);
                }
                else
                { 
                    symbolTable.AddNode(node, Symbol.notOk);  
                }
                break;
            default:
                var unit1 = symbolTable.GetUnit(L);
                var unit2 = symbolTable.GetUnit(R);
                if (unit1 != null && unit2 != null)
                {
                    if (symbolTable.CompareCustomUnits(unit1, unit2))
                    {
                        symbolTable.AddNodeToUnit(node, unit1);
                        symbolTable.AddNode(node, Symbol.ok);
                    }
                    else
                    {
                        symbolTable.AddNode(node, Symbol.notOk);
                    } 
                }
                else
                {
                    symbolTable.AddNode(node, Symbol.notOk);
                }

                
                break;
        }
    }

    public override void OutATernaryExp(ATernaryExp node)
    {
        Symbol? Cond = symbolTable.GetSymbol(node.GetCond());
        if (Cond == Symbol.Bool)
        {
            AddBinaryToSymbolTable(node, node.GetTrue(), node.GetFalse());
        }
        else
        {
            symbolTable.AddNode(node, Symbol.notOk);
        }
    }
    
    public override void OutALogicalnotExp(ALogicalnotExp node) => symbolTable.AddNode(node,
        symbolTable.GetSymbol(node.GetExp()) == Symbol.Bool ? Symbol.Bool : Symbol.notOk);

    public override void OutACastExp(ACastExp node)
    {
        Symbol? targetExpr = symbolTable.GetSymbol(node.GetType());
        Symbol? expr = symbolTable.GetSymbol(node.GetExp());
        switch (targetExpr, expr)
        {
            //typecasting to the same lmao
            case (Symbol.Bool, Symbol.Bool):
                symbolTable.AddNode(node, Symbol.Bool);
                break;
            case (Symbol.Int, Symbol.Int):
                symbolTable.AddNode(node, Symbol.Int);
                break;
            case (Symbol.Decimal, Symbol.Decimal):
                symbolTable.AddNode(node, Symbol.Decimal);
                break;
            case (Symbol.String, Symbol.String):
                symbolTable.AddNode(node, Symbol.String);
                break;
            case (Symbol.Char, Symbol.Char):
                symbolTable.AddNode(node, Symbol.Char);
                break;
            /*-- casting to new types --*/
            case (Symbol.Decimal, Symbol.Int):
                symbolTable.AddNode(node, Symbol.Decimal);
                break;
            case (Symbol.String, Symbol.Char):
                symbolTable.AddNode(node, Symbol.String);
                break;
            case (Symbol.String, Symbol.Int):
                symbolTable.AddNode(node, Symbol.String);
                break;
            case (Symbol.String, Symbol.Decimal):
                symbolTable.AddNode(node, Symbol.String);
                break;
            case (Symbol.Char, Symbol.Int):
                symbolTable.AddNode(node, Symbol.Char);
                break;
            default:
                symbolTable.AddNode(node, Symbol.notOk);
                break;
        }
    }
    public override void OutAOrExp(AOrExp node)
    {
        Symbol? leftside = symbolTable.GetSymbol(node.GetL());
        Symbol? rightside = symbolTable.GetSymbol(node.GetR());
        if (leftside == Symbol.Bool && rightside == Symbol.Bool)
        {
            symbolTable.AddNode(node, Symbol.Bool);
        }
        else
        {
            symbolTable.AddNode(node, Symbol.notOk);
        }
    }

    public override void OutAAndExp(AAndExp node)
    {
        Symbol? leftside = symbolTable.GetSymbol(node.GetL());
        Symbol? rightside = symbolTable.GetSymbol(node.GetR());
        if (leftside == Symbol.Bool && rightside == Symbol.Bool)
        {
            symbolTable.AddNode(node, Symbol.Bool);
        }
        else
        {
            symbolTable.AddNode(node, Symbol.notOk);
        }
    }
    public override void OutAEqualExp(AEqualExp node) => AddBinaryToSymbolTable(node, node.GetL(), node.GetR());
    public override void OutANotequalExp(ANotequalExp node) => AddBinaryToSymbolTable(node, node.GetL(), node.GetR());
    public override void OutAGreaterExp(AGreaterExp node) => AddBinaryNumberToSymbolTable(node, node.GetL(), node.GetR());
    public override void OutALessExp(ALessExp node) => AddBinaryNumberToSymbolTable(node, node.GetL(), node.GetR());
    public override void OutAGreaterequalExp(AGreaterequalExp node) => AddBinaryNumberToSymbolTable(node, node.GetL(), node.GetR());
    public override void OutALessequalExp(ALessequalExp node) => AddBinaryNumberToSymbolTable(node, node.GetL(), node.GetR());
    public override void OutASuffixplusplusExp(ASuffixplusplusExp node) => AddUnaryToSymbolTable(node);
    public override void OutASuffixminusminusExp(ASuffixminusminusExp node) => AddUnaryToSymbolTable(node);
    public override void OutAUnaryminusExp(AUnaryminusExp node) => AddUnaryToSymbolTable(node);
    public override void OutAPrefixminusminusExp(APrefixminusminusExp node) => AddUnaryToSymbolTable(node);
    public override void OutAPrefixplusplusExp(APrefixplusplusExp node) => AddUnaryToSymbolTable(node);

    // Equals, Notequal
    private void AddBinaryToSymbolTable(Node Parent, Node L, Node R)
    {
        Symbol? left = symbolTable.GetSymbol(L);
        switch (left)
        {
            case Symbol.Int:
                symbolTable.AddNode(Parent, symbolTable.GetSymbol(R) == Symbol.Int ? Symbol.Bool : Symbol.notOk);
                break;
            case (Symbol.Decimal):
                symbolTable.AddNode(Parent, symbolTable.GetSymbol(R) == Symbol.Decimal ? Symbol.Bool : Symbol.notOk);
                break;
            case Symbol.Bool:
                symbolTable.AddNode(Parent, symbolTable.GetSymbol(R) == Symbol.Bool ? Symbol.Bool : Symbol.notOk);
                break;
            case Symbol.String:
                symbolTable.AddNode(Parent, symbolTable.GetSymbol(R) == Symbol.String ? Symbol.Bool : Symbol.notOk);
                break;
            case Symbol.Char:
                symbolTable.AddNode(Parent, symbolTable.GetSymbol(R) == Symbol.Int ? Symbol.Bool : Symbol.notOk);
                break;
            default:
                var unit1 = symbolTable.GetUnit(L);
                var unit2 = symbolTable.GetUnit(R);

                if (symbolTable.CompareCustomUnits(unit1, unit2))
                {
                    symbolTable.AddNode(Parent, Symbol.Bool);
                }
                else
                {
                    symbolTable.AddNode(Parent, Symbol.notOk);
                }
                break;
        }
    }
    // Binary without string, bool and unit
    // Greater, GreaterEqual, Less, LessEqual
    private void AddBinaryNumberToSymbolTable(Node Parent, Node L, Node R)
    {
        Symbol? left = symbolTable.GetSymbol(L);
        switch (left)
        {
            case Symbol.Int:
                symbolTable.AddNode(Parent, symbolTable.GetSymbol(R) == Symbol.Int ? Symbol.Bool : Symbol.notOk);
                break;
            case (Symbol.Decimal):
                symbolTable.AddNode(Parent, symbolTable.GetSymbol(R) == Symbol.Decimal ? Symbol.Bool : Symbol.notOk);
                break;
            case Symbol.Char:
                symbolTable.AddNode(Parent, symbolTable.GetSymbol(R) == Symbol.Int ? Symbol.Bool : Symbol.notOk);
                break;
            default:
                symbolTable.AddNode(Parent, Symbol.notOk);
                break;
        }
    }
    private void AddUnaryToSymbolTable(Node node)
    {
        Symbol? expr = symbolTable.GetSymbol(node);
        switch (expr)
        {
            case Symbol.Decimal:
                symbolTable.AddNode(node, Symbol.Decimal);
                break;
            case Symbol.Int:
                symbolTable.AddNode(node, Symbol.Int);
                break;
            case Symbol.Char:
                symbolTable.AddNode(node, Symbol.Char);
                break;
            default:
                symbolTable.AddNode(node, Symbol.notOk);
                break;
        }
    }
}

