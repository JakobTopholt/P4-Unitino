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

    public override void OutAPlusExp(APlusExp node)
    {
        /*
        if (symbolTable._currentsymbolTable.nodeToUnit.ContainsKey(node.GetL()) || symbolTable._currentsymbolTable.nodeToUnit.ContainsKey(node.GetR()))
        {
            if (!symbolTable._currentsymbolTable.nodeToUnit.ContainsKey(node.GetL()) && symbolTable._currentsymbolTable.nodeToUnit.ContainsKey(node.GetR()))
            {
                
            }
            else if (symbolTable._currentsymbolTable.nodeToUnit.TryGetValue(node.GetL(),out AUnitdecl leftSide) && symbolTable._currentsymbolTable.nodeToUnit.TryGetValue(node.GetR(),out AUnitdecl rightSide))
            {
                symbolTable.AddNode(node, leftSide == rightSide ? Symbol.ok : Symbol.notOk);

                return;
            }
            symbolTable.AddNode(node,Symbol.notOk);  
        }
        else*/
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
            }
        }
    }

    public override void OutAMinusExp(AMinusExp node)
    {
        /*
        if (symbolTable._currentsymbolTable.nodeToUnit.ContainsKey(node.GetL()) || symbolTable._currentsymbolTable.nodeToUnit.ContainsKey(node.GetR()))
        {
            if (!symbolTable._currentsymbolTable.nodeToUnit.ContainsKey(node.GetL()) && symbolTable._currentsymbolTable.nodeToUnit.ContainsKey(node.GetR()))
            {
                
            }
            else if (symbolTable._currentsymbolTable.nodeToUnit.TryGetValue(node.GetL(),out AUnitdecl leftSide) && symbolTable._currentsymbolTable.nodeToUnit.TryGetValue(node.GetR(),out AUnitdecl rightSide))
            {
                symbolTable.AddNode(node, leftSide == rightSide ? Symbol.ok : Symbol.notOk);

                return;
            }
            symbolTable.AddNode(node,Symbol.notOk);  
        }
        else*/
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

