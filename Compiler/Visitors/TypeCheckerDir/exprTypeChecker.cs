using System.Collections;
using Microsoft.VisualBasic;

namespace Compiler.Visitors.TypeCheckerDir;
using Moduino.node;
using Moduino.analysis;

public class exprTypeChecker : stmtTypeChecker
{
    public exprTypeChecker(SymbolTable symbolTable) : base(symbolTable)
    {
        
    }
    public override void OutAUnitType(AUnitType node)
    {
        // Save reference from node to tuple
        // Implement logic here
       
        List<AUnitdecl> newNums = new List<AUnitdecl>();
        List<AUnitdecl> newDens = new List<AUnitdecl>();
       
        Tuple<List<AUnitdecl>, List<AUnitdecl>> unit = new Tuple<List<AUnitdecl>, List<AUnitdecl>>(newNums, newDens);
        symbolTable.AddNodeToUnit(node, unit);
    }
    public override void OutADecimalExp(ADecimalExp node) => symbolTable.AddNode(node, Symbol.Decimal);
    public override void OutANumberExp(ANumberExp node) => symbolTable.AddNode(node, Symbol.Int);
    public override void OutABooleanExp(ABooleanExp node) => symbolTable.AddNode(node, Symbol.Bool);
    public override void OutAStringExp(AStringExp node) => symbolTable.AddNode(node, Symbol.String);
    public override void OutACharExp(ACharExp node) => symbolTable.AddNode(node, Symbol.Char);
    public override void OutAFunccallExp(AFunccallExp node)
    {
        Symbol? funcId = symbolTable.GetSymbol(node.GetId());
        //Symbol? funcExpr = symbolTable.GetSymbol(node.GetExp());
    }

    public override void OutAIdExp(AIdExp node)
    {
        //ik helt hundred
        Symbol? symbol = symbolTable.GetSymbol(node.GetId());
        symbolTable.AddNode(node, symbol == null ? Symbol.notOk : Symbol.ok);
    }
    public override void OutAUnitExp(AUnitExp node)
    {
        // A single unitnumber eg. 50ms
        var unitType = GetUnitFromUnitnumber(node.GetUnitnumber());
        if (unitType != null)
        {
            // Create a new unit tuple and add the unitnumber as a lone numerator
            List<AUnitdecl> nums = new List<AUnitdecl>();
            nums.Add(unitType);
            List<AUnitdecl> dens = new List<AUnitdecl>();
            var unit = new Tuple<List<AUnitdecl>, List<AUnitdecl>>(nums, dens);
            
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

    private AUnitdecl? GetUnitFromUnitnumber(PUnitnumber unitnumber)
    {
        AUnitdecl? unit;
        switch (unitnumber)
        {
            case ADecimalUnitnumber a:
                unit = symbolTable.GetUnitFromSubunit(a.GetId());
                return unit;
            case ANumberUnitnumber b:
                unit = symbolTable.GetUnitFromSubunit(b.GetId());
                return unit;
            default:
                return null;
        }
    }
    public override void OutADivideExp(ADivideExp node)
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
                if (symbolTable.nodeToUnit.ContainsKey(leftExpr) && symbolTable.nodeToUnit.ContainsKey(rightExpr))
                {
                    Tuple<List<AUnitdecl>, List<AUnitdecl>> left = symbolTable.GetUnit(leftExpr); // unit 1
                    Tuple<List<AUnitdecl>, List<AUnitdecl>> right = symbolTable.GetUnit(rightExpr); // unit 2

                    List<AUnitdecl> a = left.Item1;
                    List<AUnitdecl> b = left.Item2;
                    List<AUnitdecl> c = right.Item1;
                    List<AUnitdecl> d = right.Item2;

                    List<AUnitdecl> ac = a.Intersect(c).ToList();
                    List<AUnitdecl> bd = b.Intersect(d).ToList();
            
                    List<AUnitdecl> numerators = a.Except(ac).Union(d.Except(bd)).ToList();
                    List<AUnitdecl> denomerators = c.Except(ac).Union(b.Except(bd)).ToList();
            
                    Tuple<List<AUnitdecl>, List<AUnitdecl>> unituse = new Tuple<List<AUnitdecl>, List<AUnitdecl>>(numerators, denomerators);
                    symbolTable.AddNodeToUnit(node, unituse);
                    symbolTable.AddNode(node, Symbol.ok);
                } 
                else
                { 
                    // not valid input expressions to a divide expression
                    symbolTable.AddNode(node, Symbol.notOk);
                }
                break;
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
                if (symbolTable.nodeToUnit.ContainsKey(leftExpr) && symbolTable.nodeToUnit.ContainsKey(rightExpr))
                {
                    Tuple<List<AUnitdecl>, List<AUnitdecl>> left = symbolTable.GetUnit(leftExpr); // unit 1
                    Tuple<List<AUnitdecl>, List<AUnitdecl>> right = symbolTable.GetUnit(rightExpr); // unit 2
            
                    List<AUnitdecl> a = left.Item1;
                    List<AUnitdecl> b = left.Item2;
                    List<AUnitdecl> c = right.Item1;
                    List<AUnitdecl> d = right.Item2;
            
                    List<AUnitdecl> ad = a.Intersect(d).ToList();
                    List<AUnitdecl> bc = b.Intersect(c).ToList();
            
                    List<AUnitdecl> numerators = a.Except(ad).Union(d.Except(bc)).ToList();
                    List<AUnitdecl> denomerators = c.Except(ad).Union(b.Except(bc)).ToList();
            
                    Tuple<List<AUnitdecl>, List<AUnitdecl>> unituse = new Tuple<List<AUnitdecl>, List<AUnitdecl>>(numerators, denomerators);
                    symbolTable.AddNodeToUnit(node, unituse);
                    symbolTable.AddNode(node, Symbol.ok);
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
                if (symbolTable.nodeToUnit.ContainsKey(leftExpr) && symbolTable.nodeToUnit.ContainsKey(rightExpr))
                {
                    Tuple<List<AUnitdecl>, List<AUnitdecl>> left = symbolTable.GetUnit(leftExpr); // unit 1
                    Tuple<List<AUnitdecl>, List<AUnitdecl>> right = symbolTable.GetUnit(rightExpr); // unit 2
            
                    List<AUnitdecl> a = left.Item1;
                    List<AUnitdecl> b = left.Item2;
                    List<AUnitdecl> c = right.Item1;
                    List<AUnitdecl> d = right.Item2;
                    
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
                    
                    if ((dontCompareNums || sortedNums1.SequenceEqual(sortedNums2)) && (dontCompareDens || sortedDens1.SequenceEqual(sortedDens2)))
                    {
                        // Create a new unitTyple and add it to NodeToUnit and return symbol.ok
                        List<AUnitdecl> numerators = isEmptyNums1 ? isEmptyNums2 ? new List<AUnitdecl>() : sortedNums2 : sortedNums1;
                        List<AUnitdecl> denomerators = isEmptyDens1 ? isEmptyDens2 ? new List<AUnitdecl>() : sortedDens2 : sortedDens1;
                        
                        Tuple<List<AUnitdecl>, List<AUnitdecl>> unituse = new Tuple<List<AUnitdecl>, List<AUnitdecl>>(numerators, denomerators);
                        symbolTable.AddNodeToUnit(node, unituse);
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
                if (symbolTable.nodeToUnit.ContainsKey(leftExpr) && symbolTable.nodeToUnit.ContainsKey(rightExpr))
                {
                    Tuple<List<AUnitdecl>, List<AUnitdecl>> left = symbolTable.GetUnit(leftExpr); // unit 1
                    Tuple<List<AUnitdecl>, List<AUnitdecl>> right = symbolTable.GetUnit(rightExpr); // unit 2
            
                    List<AUnitdecl> a = left.Item1;
                    List<AUnitdecl> b = left.Item2;
                    List<AUnitdecl> c = right.Item1;
                    List<AUnitdecl> d = right.Item2;
                    
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
                    
                    if ((dontCompareNums || sortedNums1.SequenceEqual(sortedNums2)) && (dontCompareDens || sortedDens1.SequenceEqual(sortedDens2)))
                    {
                        // Create a new unitTyple and add it to NodeToUnit and return symbol.ok
                        List<AUnitdecl> numerators = isEmptyNums1 ? isEmptyNums2 ? new List<AUnitdecl>() : sortedNums2 : sortedNums1;
                        List<AUnitdecl> denomerators = isEmptyDens1 ? isEmptyDens2 ? new List<AUnitdecl>() : sortedDens2 : sortedDens1;
                        
                        Tuple<List<AUnitdecl>, List<AUnitdecl>> unituse = new Tuple<List<AUnitdecl>, List<AUnitdecl>>(numerators, denomerators);
                        symbolTable.AddNodeToUnit(node, unituse);
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

    public override void OutARemainderExp(ARemainderExp node) => AddBinaryNumberToSymbolTable(node,node.GetL(),node.GetR());
    public override void OutATernaryExp(ATernaryExp node)
    {
        Symbol? Cond = symbolTable.GetSymbol(node.GetCond());
        if (Cond == Symbol.Bool)
        {
            AddBinaryToSymbolTable(node, node.GetTrue(), node.GetFalse());
        }
        else
        {
            symbolTable.AddNode(node,Symbol.notOk);
        }
    }
    public override void OutAOrExp(AOrExp node)
    {
        Symbol? leftside = symbolTable.GetSymbol(node.GetL());
        Symbol? rightside = symbolTable.GetSymbol(node.GetR());
        if (leftside == Symbol.Bool && rightside == Symbol.Bool)
        {
            symbolTable.AddNode(node,Symbol.Bool);
        }
        else
        {
            symbolTable.AddNode(node,Symbol.notOk);
        }
    }
    public override void OutAAndExp(AAndExp node)
    {
        Symbol? leftside = symbolTable.GetSymbol(node.GetL());
        Symbol? rightside = symbolTable.GetSymbol(node.GetR());
        if (leftside == Symbol.Bool && rightside == Symbol.Bool)
        {
            symbolTable.AddNode(node,Symbol.Bool);
        }
        else
        {
            symbolTable.AddNode(node,Symbol.notOk);
        }
    }
    public override void OutAEqualExp(AEqualExp node) => AddBinaryToSymbolTable(node,node.GetL(),node.GetR());
    public override void OutANotequalExp(ANotequalExp node) => AddBinaryToSymbolTable(node,node.GetL(),node.GetR());
    public override void OutAGreaterExp(AGreaterExp node) => AddBinaryNumberToSymbolTable(node,node.GetL(),node.GetR());
    public override void OutALessExp(ALessExp node) => AddBinaryNumberToSymbolTable(node,node.GetL(),node.GetR());
    public override void OutAGreaterequalExp(AGreaterequalExp node) => AddBinaryNumberToSymbolTable(node,node.GetL(),node.GetR());
    public override void OutALessequalExp(ALessequalExp node) => AddBinaryNumberToSymbolTable(node,node.GetL(),node.GetR());
    public override void OutASuffixplusplusExp(ASuffixplusplusExp node) => AddUnaryToSymbolTable(node);
    public override void OutASuffixminusminusExp(ASuffixminusminusExp node) => AddUnaryToSymbolTable(node);
    public override void OutAUnaryminusExp(AUnaryminusExp node) => AddUnaryToSymbolTable(node);
    public override void OutALogicalnotExp(ALogicalnotExp node) => symbolTable.AddNode(node, symbolTable.GetSymbol(node) == Symbol.Bool ? Symbol.Bool : Symbol.notOk);
    public override void OutACastExp(ACastExp node)
    {
        Symbol? targetExpr = symbolTable.GetSymbol(node.GetType()); // er dette rigtigt?
        Symbol? expr = symbolTable.GetSymbol(node.GetExp());
        switch (targetExpr,expr)
        {
            //typecasting to the same lmao
            case (Symbol.Bool, Symbol.Bool):
                symbolTable.AddNode(node,Symbol.Bool);
                break;
            case (Symbol.Int,Symbol.Int):
                symbolTable.AddNode(node,Symbol.Int);
                break;
            case (Symbol.Decimal,Symbol.Decimal):
                symbolTable.AddNode(node,Symbol.Decimal);
                break;
            case(Symbol.String,Symbol.String):
                symbolTable.AddNode(node,Symbol.String);
                break;
            case(Symbol.Char,Symbol.Char):
                symbolTable.AddNode(node,Symbol.Char);
                break;
            /*-- casting to new types --*/
            case(Symbol.Decimal,Symbol.Int):
                symbolTable.AddNode(node,Symbol.Decimal);
                break;
            case(Symbol.String,Symbol.Char):
                symbolTable.AddNode(node,Symbol.String);
                break;
            case(Symbol.String,Symbol.Int):
                symbolTable.AddNode(node,Symbol.String);
                break;
            case(Symbol.String,Symbol.Decimal):
                symbolTable.AddNode(node,Symbol.String);
                break;
            case(Symbol.Char,Symbol.Int):
                symbolTable.AddNode(node,Symbol.Char);
                break;
            default:
                symbolTable.AddNode(node,Symbol.notOk);
                break;
        }
    }
    public override void OutAPrefixminusminusExp(APrefixminusminusExp node) => AddUnaryToSymbolTable(node);
    public override void OutAPrefixplusplusExp(APrefixplusplusExp node) => AddUnaryToSymbolTable(node);
    private void AddBinaryToSymbolTable(Node Parent, Node L, Node R)
    {
        Symbol? l = symbolTable.GetSymbol(L);
        Symbol? r = symbolTable.GetSymbol(R);
        switch (l, r)
        {
            /*case (Symbol.ok, Symbol.Bool):
                symbolTable.AddNode(Parent, Symbol.Bool);
                break;*/
            case (Symbol.Bool, Symbol.Bool):
                symbolTable.AddNode(Parent,Symbol.Bool);
                break;
            case (Symbol.Int,Symbol.Int):
                symbolTable.AddNode(Parent,Symbol.Int);
                break;
            case (Symbol.Decimal,Symbol.Decimal):
                symbolTable.AddNode(Parent,Symbol.Decimal);
                break;
            case(Symbol.String,Symbol.String):
                symbolTable.AddNode(Parent,Symbol.String);
                break;
            case(Symbol.Char,Symbol.Char):
                symbolTable.AddNode(Parent,Symbol.Char);
                break;
            default:
                symbolTable.AddNode(Parent,Symbol.notOk);
                break;
        }
    }
    private void AddUnaryToSymbolTable(Node node)
    {
        Symbol? expr = symbolTable.GetSymbol(node);
        switch (expr)
        {
            case Symbol.Decimal:
                symbolTable.AddNode(node,Symbol.Decimal);
                break;
            case Symbol.Int:
                symbolTable.AddNode(node,Symbol.Int);
                break;
            case Symbol.Char:
                symbolTable.AddNode(node,Symbol.Char);
                break;
            default:
                symbolTable.AddNode(node,Symbol.notOk);
                break;
        }
    }
    private void AddBinaryNumberToSymbolTable(Node Parent, Node L, Node R)
    {
        Symbol? leftSide = symbolTable.GetSymbol(L);
        Symbol? rightSide = symbolTable.GetSymbol(R);
        switch (leftSide)
        {
            case Symbol.Int when rightSide is Symbol.Int or Symbol.Decimal:
                symbolTable.AddNode(Parent,Symbol.Decimal);
                break;
            case Symbol.Int or Symbol.Decimal when rightSide is Symbol.Decimal:
                symbolTable.AddNode(Parent,Symbol.Decimal);
                break;
            case Symbol.Int when rightSide is Symbol.Int:
                symbolTable.AddNode(Parent,Symbol.Int);
                break;
            default:
                symbolTable.AddNode(Parent,Symbol.notOk);
                break;
        }
    }
}

