﻿using System.Collections;
using Microsoft.VisualBasic;

namespace Compiler.Visitors.TypeCheckerDir;
using Moduino.node;
using Moduino.analysis;

public class exprTypeChecker : stmtTypeChecker
{
    public override void OutADecimalExp(ADecimalExp node)
    {
        symbolTable.AddNode(node, Symbol.Decimal);
    }

    public override void OutANumberExp(ANumberExp node)
    {
        symbolTable.AddNode(node, Symbol.Int);
    }

    public override void OutABooleanExp(ABooleanExp node)
    {
        symbolTable.AddNode(node, Symbol.Bool);
    }

    public override void OutAStringExp(AStringExp node)
    {
        symbolTable.AddNode(node, Symbol.String);
    }

    public override void OutACharExp(ACharExp node)
    {
        symbolTable.AddNode(node, Symbol.Char);
    }
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

    public override void OutAValueExp(AValueExp node)
    {
        symbolTable.AddNode(node, UnitVisitor.StateUnit ? Symbol.ok : Symbol.notOk);
    }
    public override void OutAUnitExp(AUnitExp node)
    {
        // A single unitnumber eg. 50ms
        // Typecheck
        // Save its parent "type" in symboltable aswell
        var unitType = GetUnittypeFromUnitnumber(node.GetUnitnumber());
        symbolTable.AddNodeToUnit(node, unitType);
        symbolTable.AddNode(node, Symbol.ok);

        AUnitType GetUnittypeFromUnitnumber(PUnitnumber unitnumber)
        {
            switch (unitnumber)
            {
                case ADecimalUnitnumber a:
                    AUnitdecl? unit = symbolTable.GetUnitFromSubunit(a.GetId());
                    ANumUnituse num = new ANumUnituse(unit.GetId());
                    IList unituse = new Collection();
                    unituse.Add(num);
                    AUnitType unittype = new AUnitType(unituse);
                    return unittype;
                case ANumberUnitnumber b:
                    AUnitdecl? unit2 = symbolTable.GetUnitFromSubunit(b.GetId());
                    ANumUnituse num2 = new ANumUnituse(unit2.GetId());
                    IList unituse2 = new Collection();
                    unituse2.Add(num2);
                    AUnitType unittype2 = new AUnitType(unituse2);
                    return unittype2;
                default:
                    throw new Exception("Is not a valid unitnumber");
            }
        }
    }

    public override void OutADivideExp(ADivideExp node)
    {
        // Standard types
        var l = symbolTable.GetSymbol(node.GetL());
        var r = symbolTable.GetSymbol(node.GetR());
        switch (l)
        {
            case Symbol.Int when r is Symbol.Int:
                symbolTable.AddNode(node, Symbol.Int);
                break;
            case Symbol.Decimal when r is Symbol.Decimal or Symbol.Int:
                symbolTable.AddNode(node, Symbol.Decimal);
                break;
            case Symbol.Decimal or Symbol.Int when r is Symbol.Decimal:
                symbolTable.AddNode(node, Symbol.Decimal);
                break;
            default:
                symbolTable.AddNode(node, Symbol.notOk);
                break;
        }

        PExp leftExpr = node.GetL();
        PExp rightExpr = node.GetR();
        
        AUnitType? left = symbolTable.GetUnitFromExpr(leftExpr);
        AUnitType? right = symbolTable.GetUnitFromExpr(rightExpr);
        if (left != null && right != null)
        {
            // Left is Num, Right is Den
            IEnumerable<ANumUnituse> numerator = left.GetUnituse().OfType<ANumUnituse>();
            IEnumerable<ADenUnituse> denomerator = right.GetUnituse().OfType<ADenUnituse>();
            
            
            
            
            IList resultUnituse = new Collection();

            AUnitType unitType = null;
            symbolTable.AddNodeToUnit(node, unitType);
            symbolTable.AddNode(node, Symbol.ok);
        }
        
        //SubunitToUnit and combine parents. eg. 50km/2h -> Distance/Time

        // Implement logic which adds a AUnitUnittype entry to the symboltable dictioary
        // Maybe not the most effecient approach for now as each subPart of the unitexpression will be saved
        // symbolTable.AddUnit(node, AUnitUnittype);
        
    }

    public override void OutAMultiplyExp(AMultiplyExp node)
    {
        var l = symbolTable.GetSymbol(node.GetL());
        var r = symbolTable.GetSymbol(node.GetR());
        switch (l)
        {
            case Symbol.Int when r is Symbol.Int:
                symbolTable.AddNode(node, Symbol.Int);
                break;
            case Symbol.Decimal when r is Symbol.Decimal or Symbol.Int:
                symbolTable.AddNode(node, Symbol.Decimal);
                break;
            case Symbol.Decimal or Symbol.Int when r is Symbol.Decimal:
                symbolTable.AddNode(node, Symbol.Decimal);
                break;
            default:
                symbolTable.AddNode(node, Symbol.notOk);
                break;
        }
        // -------------- LOGIK TIL CUSTOM UNITS TYPECHECKING ------------------ //
        // skal save fra OutAUnitnumberExp til Dictionary<Node, PUnitUnittype> nodeToUnit
        // Unitnumber skal håndteres i divideExprcase og multExprcase for at elevate types
        // PUnitUnittype variablen bliver created ved at håndtere unitnumbers i ovenbenævnte case
        
    }

    public override void OutAPlusExp(APlusExp node)
    {
        {
            var l = symbolTable.GetSymbol(node.GetL());
            var r = symbolTable.GetSymbol(node.GetR());
            switch (l)
            {
                case Symbol.Int when r is Symbol.Int:
                    symbolTable.AddNode(node, Symbol.Int);
                    break;
                case Symbol.Decimal or Symbol.Int when r is Symbol.Decimal or Symbol.Int:
                    symbolTable.AddNode(node, Symbol.Decimal);
                    break;
                case Symbol.Int or Symbol.Decimal when r is Symbol.Decimal:
                    symbolTable.AddNode(node, Symbol.Decimal);
                    break;
                case Symbol.String when r is Symbol.Int or Symbol.String:
                    symbolTable.AddNode(node, Symbol.String);
                    break;
                case Symbol.Int or Symbol.String when r is Symbol.String:
                    symbolTable.AddNode(node, Symbol.String);
                    break;
                case Symbol.Decimal or Symbol.String when r is Symbol.Decimal:
                    symbolTable.AddNode(node, Symbol.String);
                    break;
                case Symbol.String when r is Symbol.String or Symbol.Decimal:
                    symbolTable.AddNode(node, Symbol.String);
                    break;
                default:
                    symbolTable.AddNode(node, Symbol.notOk);
                    break;
                //Custom unit logik for at plus to subunits sammen fx (50ms+5s)/10km
                
                
                
                
            }
        }
    }

    public override void OutAMinusExp(AMinusExp node)
    {
        {
            var l = symbolTable.GetSymbol(node.GetL());
            var r = symbolTable.GetSymbol(node.GetR());
            switch (l)
            {
                case Symbol.Int when r is Symbol.Int:
                    symbolTable.AddNode(node, Symbol.Int);
                    break;
                case Symbol.Decimal when r is Symbol.Decimal or Symbol.Int:
                    symbolTable.AddNode(node, Symbol.Decimal);
                    break;
                case Symbol.Decimal or Symbol.Int when r is Symbol.Decimal:
                    symbolTable.AddNode(node, Symbol.Decimal);
                    break;
                default:
                    symbolTable.AddNode(node, Symbol.notOk);
                    break;
            }
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

    public exprTypeChecker(SymbolTable symbolTable) : base(symbolTable)
    {
        
    }
}

