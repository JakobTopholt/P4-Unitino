namespace Compiler.Visitors.TypeCheckerDir;
using Moduino.node;
using Moduino.analysis;

public class exprTypeChecker : stmtTypeChecker
{
     //Expressions
    public override void OutADivideExp(ADivideExp node)
    {
        /*if (SymbolTable._currentSymbolTable.nodeToUnit.ContainsKey(node.GetL()) ||
            SymbolTable._currentSymbolTable.nodeToUnit.ContainsKey(node.GetR()))
        {
            if (!SymbolTable._currentSymbolTable.nodeToUnit.ContainsKey(node.GetL()) &&
                SymbolTable._currentSymbolTable.nodeToUnit.ContainsKey(node.GetR()))
            {

            }
            else if (SymbolTable._currentSymbolTable.nodeToUnit.TryGetValue(node.GetL(), out AUnitdecl leftSide) &&
                     SymbolTable._currentSymbolTable.nodeToUnit.TryGetValue(node.GetR(), out AUnitdecl rightSide))
            {
                SymbolTable.AddNode(node, leftSide == rightSide ? Symbol.ok : Symbol.notOk);

                return;
            }

            SymbolTable.AddNode(node, Symbol.notOk);
        }
        else*/
        {
            var l = SymbolTable.GetSymbol(node.GetL());
            var r = SymbolTable.GetSymbol(node.GetR());
            switch (l)
            {
                case Symbol.Int when r is Symbol.Int:
                    SymbolTable.AddNode(node, Symbol.Int);
                    break;
                case Symbol.Decimal when r is Symbol.Decimal or Symbol.Int:
                    SymbolTable.AddNode(node, Symbol.Decimal);
                    break;
                case Symbol.Decimal or Symbol.Int when r is Symbol.Decimal:
                    SymbolTable.AddNode(node, Symbol.Decimal);
                    break;

                default:
                    SymbolTable.AddNode(node, Symbol.notOk);
                    break;
            }
        }
    }

    public override void OutAMultiplyExp(AMultiplyExp node)
    {
        /*
        if (SymbolTable._currentSymbolTable.nodeToUnit.ContainsKey(node.GetL()) ||
            SymbolTable._currentSymbolTable.nodeToUnit.ContainsKey(node.GetR()))
        {
            if (!SymbolTable._currentSymbolTable.nodeToUnit.ContainsKey(node.GetL()) &&
                SymbolTable._currentSymbolTable.nodeToUnit.ContainsKey(node.GetR()))
            {

            }
            else if (SymbolTable._currentSymbolTable.nodeToUnit.TryGetValue(node.GetL(), out AUnitdecl leftSide) &&
                     SymbolTable._currentSymbolTable.nodeToUnit.TryGetValue(node.GetR(), out AUnitdecl rightSide))
            {
                SymbolTable.AddNode(node, leftSide == rightSide ? Symbol.ok : Symbol.notOk);

                return;
            }

            SymbolTable.AddNode(node, Symbol.notOk);
        }
        else*/
        {
            var l = SymbolTable.GetSymbol(node.GetL());
            var r = SymbolTable.GetSymbol(node.GetR());
            switch (l)
            {
                case Symbol.Int when r is Symbol.Int:
                    SymbolTable.AddNode(node, Symbol.Int);
                    break;
                case Symbol.Decimal when r is Symbol.Decimal or Symbol.Int:
                    SymbolTable.AddNode(node, Symbol.Decimal);
                    break;
                case Symbol.Decimal or Symbol.Int when r is Symbol.Decimal:
                    SymbolTable.AddNode(node, Symbol.Decimal);
                    break;
                default:
                    SymbolTable.AddNode(node, Symbol.notOk);
                    break;
            }
        }
    }

    public override void OutAPlusExp(APlusExp node)
    {
        /*
        if (SymbolTable._currentSymbolTable.nodeToUnit.ContainsKey(node.GetL()) || SymbolTable._currentSymbolTable.nodeToUnit.ContainsKey(node.GetR()))
        {
            if (!SymbolTable._currentSymbolTable.nodeToUnit.ContainsKey(node.GetL()) && SymbolTable._currentSymbolTable.nodeToUnit.ContainsKey(node.GetR()))
            {
                
            }
            else if (SymbolTable._currentSymbolTable.nodeToUnit.TryGetValue(node.GetL(),out AUnitdecl leftSide) && SymbolTable._currentSymbolTable.nodeToUnit.TryGetValue(node.GetR(),out AUnitdecl rightSide))
            {
                SymbolTable.AddNode(node, leftSide == rightSide ? Symbol.ok : Symbol.notOk);

                return;
            }
            SymbolTable.AddNode(node,Symbol.notOk);  
        }
        else*/
        {
            var l = SymbolTable.GetSymbol(node.GetL());
            var r = SymbolTable.GetSymbol(node.GetR());
            switch (l)
            {
                case Symbol.Int when r is Symbol.Int:
                    SymbolTable.AddNode(node, Symbol.Int);
                    break;
                case Symbol.Decimal or Symbol.Int when r is Symbol.Decimal or Symbol.Int:
                    SymbolTable.AddNode(node, Symbol.Decimal);
                    break;
                case Symbol.Int or Symbol.Decimal when r is Symbol.Decimal:
                    SymbolTable.AddNode(node, Symbol.Decimal);
                    break;
                case Symbol.String when r is Symbol.Int or Symbol.String:
                    SymbolTable.AddNode(node, Symbol.String);
                    break;
                case Symbol.Int or Symbol.String when r is Symbol.String:
                    SymbolTable.AddNode(node, Symbol.String);
                    break;
                case Symbol.Decimal or Symbol.String when r is Symbol.Decimal:
                    SymbolTable.AddNode(node, Symbol.String);
                    break;
                case Symbol.String when r is Symbol.String or Symbol.Decimal:
                    SymbolTable.AddNode(node, Symbol.String);
                    break;
                default:
                    SymbolTable.AddNode(node, Symbol.notOk);
                    break;
            }
        }
    }

    public override void OutAMinusExp(AMinusExp node)
    {
        /*
        if (SymbolTable._currentSymbolTable.nodeToUnit.ContainsKey(node.GetL()) || SymbolTable._currentSymbolTable.nodeToUnit.ContainsKey(node.GetR()))
        {
            if (!SymbolTable._currentSymbolTable.nodeToUnit.ContainsKey(node.GetL()) && SymbolTable._currentSymbolTable.nodeToUnit.ContainsKey(node.GetR()))
            {
                
            }
            else if (SymbolTable._currentSymbolTable.nodeToUnit.TryGetValue(node.GetL(),out AUnitdecl leftSide) && SymbolTable._currentSymbolTable.nodeToUnit.TryGetValue(node.GetR(),out AUnitdecl rightSide))
            {
                SymbolTable.AddNode(node, leftSide == rightSide ? Symbol.ok : Symbol.notOk);

                return;
            }
            SymbolTable.AddNode(node,Symbol.notOk);  
        }
        else*/
        {
            var l = SymbolTable.GetSymbol(node.GetL());
            var r = SymbolTable.GetSymbol(node.GetR());
            switch (l)
            {
                case Symbol.Int when r is Symbol.Int:
                    SymbolTable.AddNode(node, Symbol.Int);
                    break;
                case Symbol.Decimal when r is Symbol.Decimal or Symbol.Int:
                    SymbolTable.AddNode(node, Symbol.Decimal);
                    break;
                case Symbol.Decimal or Symbol.Int when r is Symbol.Decimal:
                    SymbolTable.AddNode(node, Symbol.Decimal);
                    break;
                default:
                    SymbolTable.AddNode(node, Symbol.notOk);
                    break;
            }
        }
    }

    public override void OutARemainderExp(ARemainderExp node) => AddBinaryNumberToSymbolTable(node,node.GetL(),node.GetR());

    public override void OutATernaryExp(ATernaryExp node)
    {
        Symbol? Cond = SymbolTable.GetSymbol(node.GetCond());
        if (Cond == Symbol.Bool)
        {
            AddBinaryToSymbolTable(node, node.GetTrue(), node.GetFalse());
        }
        else
        {
            SymbolTable.AddNode(node,Symbol.notOk);
        }
    }

    public override void OutAOrExp(AOrExp node)
    {
        Symbol? leftside = SymbolTable.GetSymbol(node.GetL());
        Symbol? rightside = SymbolTable.GetSymbol(node.GetR());
        if (leftside == Symbol.Bool && rightside == Symbol.Bool)
        {
            SymbolTable.AddNode(node,Symbol.Bool);
        }
        else
        {
            SymbolTable.AddNode(node,Symbol.notOk);
        }

    }

    public override void OutAAndExp(AAndExp node)
    {
        Symbol? leftside = SymbolTable.GetSymbol(node.GetL());
        Symbol? rightside = SymbolTable.GetSymbol(node.GetR());
        if (leftside == Symbol.Bool && rightside == Symbol.Bool)
        {
            SymbolTable.AddNode(node,Symbol.Bool);
        }
        else
        {
            SymbolTable.AddNode(node,Symbol.notOk);
        }
    }

    public override void OutAEqualExp(AEqualExp node) => AddBinaryToSymbolTable(node,node.GetL(),node.GetR());
    public override void OutANotequalExp(ANotequalExp node) => AddBinaryToSymbolTable(node,node.GetL(),node.GetR());
    public override void OutAGreaterExp(AGreaterExp node) => AddBinaryNumberToSymbolTable(node,node.GetL(),node.GetR());
    public override void OutALessExp(ALessExp node) => AddBinaryNumberToSymbolTable(node,node.GetL(),node.GetR());
    public override void OutAGreaterequalExp(AGreaterequalExp node) => AddBinaryNumberToSymbolTable(node,node.GetL(),node.GetR());
    public override void OutALessequalExp(ALessequalExp node) => AddBinaryNumberToSymbolTable(node,node.GetL(),node.GetR());
    public override void OutASuffixplusplusExp(ASuffixplusplusExp node)
    {
        AddUnaryToSymbolTable(node);
    }

    public override void OutASuffixminusminusExp(ASuffixminusminusExp node)
    {
        AddUnaryToSymbolTable(node);
    }

    public override void OutAUnaryminusExp(AUnaryminusExp node)
    {
        AddUnaryToSymbolTable(node);
    }
    
    public override void OutALogicalnotExp(ALogicalnotExp node) => SymbolTable.AddNode(node, SymbolTable.GetSymbol(node) == Symbol.Bool ? Symbol.Bool : Symbol.notOk);

    public override void OutACastExp(ACastExp node)
    {
        Symbol? targetExpr = SymbolTable.GetSymbol(node.GetType()); // er dette rigtigt?
        Symbol? expr = SymbolTable.GetSymbol(node.GetExp());
        switch (targetExpr,expr)
        {
            //typecasting to the same lmao
            case (Symbol.Bool, Symbol.Bool):
                SymbolTable.AddNode(node,Symbol.Bool);
                break;
            case (Symbol.Int,Symbol.Int):
                SymbolTable.AddNode(node,Symbol.Int);
                break;
            case (Symbol.Decimal,Symbol.Decimal):
                SymbolTable.AddNode(node,Symbol.Decimal);
                break;
            case(Symbol.String,Symbol.String):
                SymbolTable.AddNode(node,Symbol.String);
                break;
            case(Symbol.Char,Symbol.Char):
                SymbolTable.AddNode(node,Symbol.Char);
                break;
            /*-- casting to new types --*/
            case(Symbol.Decimal,Symbol.Int):
                SymbolTable.AddNode(node,Symbol.Decimal);
                break;
            case(Symbol.String,Symbol.Char):
                SymbolTable.AddNode(node,Symbol.String);
                break;
            case(Symbol.String,Symbol.Int):
                SymbolTable.AddNode(node,Symbol.String);
                break;
            case(Symbol.String,Symbol.Decimal):
                SymbolTable.AddNode(node,Symbol.String);
                break;
            case(Symbol.Char,Symbol.Int):
                SymbolTable.AddNode(node,Symbol.Char);
                break;
            default:
                SymbolTable.AddNode(node,Symbol.notOk);
                break;
        }
    }

    public override void OutAPrefixminusminusExp(APrefixminusminusExp node)
    {
        AddUnaryToSymbolTable(node);
    }

    public override void OutAPrefixplusplusExp(APrefixplusplusExp node)
    {
        AddUnaryToSymbolTable(node);
    }
    public override void OutAFunccallExp(AFunccallExp node)
    {
        Symbol? funcId = SymbolTable.GetSymbol(node.GetId());
        //Symbol? funcExpr = SymbolTable.GetSymbol(node.GetExp());
    }

    public override void OutAIdExp(AIdExp node)
    {
        //ik helt hundred
        Symbol? symbol = SymbolTable.GetSymbol(node.GetId());
        SymbolTable.AddNode(node, symbol == null ? Symbol.notOk : Symbol.ok);
    }

    public override void OutAValueExp(AValueExp node)
    {
        SymbolTable.AddNode(node, UnitVisitor.StateUnit ? Symbol.ok : Symbol.notOk);
    }

    private void AddBinaryToSymbolTable(Node Parent, Node L, Node R)
    {
        Symbol? l = SymbolTable.GetSymbol(L);
        Symbol? r = SymbolTable.GetSymbol(R);
        switch (l, r)
        {
            case (Symbol.ok, Symbol.Bool):
                SymbolTable.AddNode(Parent, Symbol.Bool);
                break;
            case (Symbol.Bool, Symbol.Bool):
                SymbolTable.AddNode(Parent,Symbol.Bool);
                break;
            case (Symbol.Int,Symbol.Int):
                SymbolTable.AddNode(Parent,Symbol.Int);
                break;
            case (Symbol.Decimal,Symbol.Decimal):
                SymbolTable.AddNode(Parent,Symbol.Decimal);
                break;
            case(Symbol.String,Symbol.String):
                SymbolTable.AddNode(Parent,Symbol.String);
                break;
            case(Symbol.Char,Symbol.Char):
                SymbolTable.AddNode(Parent,Symbol.Char);
                break;
            default:
                SymbolTable.AddNode(Parent,Symbol.notOk);
                break;
        }
    }

    private void AddUnaryToSymbolTable(Node node)
    {
        Symbol? expr = SymbolTable.GetSymbol(node);
        switch (expr)
        {
            case Symbol.Decimal:
                SymbolTable.AddNode(node,Symbol.Decimal);
                break;
            case Symbol.Int:
                SymbolTable.AddNode(node,Symbol.Int);
                break;
            case Symbol.Char:
                SymbolTable.AddNode(node,Symbol.Char);
                break;
            default:
                SymbolTable.AddNode(node,Symbol.notOk);
                break;
        }
    }
    private void AddBinaryNumberToSymbolTable(Node Parent, Node L, Node R)
    {
        Symbol? leftSide = SymbolTable.GetSymbol(L);
        Symbol? rightSide = SymbolTable.GetSymbol(R);
        switch (leftSide)
        {
            case Symbol.Int when rightSide is Symbol.Int or Symbol.Decimal:
                SymbolTable.AddNode(Parent,Symbol.Decimal);
                break;
            case Symbol.Int or Symbol.Decimal when rightSide is Symbol.Decimal:
                SymbolTable.AddNode(Parent,Symbol.Decimal);
                break;
            case Symbol.Int when rightSide is Symbol.Int:
                SymbolTable.AddNode(Parent,Symbol.Int);
                break;
            default:
                SymbolTable.AddNode(Parent,Symbol.notOk);
                break;
        }
    }

}

