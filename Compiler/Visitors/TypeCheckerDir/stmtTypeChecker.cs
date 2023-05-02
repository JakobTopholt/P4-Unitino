using Moduino.analysis;
using Moduino.node;

namespace Compiler.Visitors.TypeCheckerDir;

public class stmtTypeChecker : DepthFirstAdapter
{
    public override void OutAAssignStmt(AAssignStmt node) {
        Symbol? type = SymbolTable.GetSymbol("" + node.GetId());
        Symbol? exprType = SymbolTable.GetSymbol(node.GetExp());

        // if types match stmt is ok, else notok
        SymbolTable.AddNode(node, type == null || type != exprType ? Symbol.notOk : Symbol.ok);
    }

    public override void OutAPlusassignStmt(APlusassignStmt node)
    {
        Symbol? type = SymbolTable.GetSymbol(node.GetId());
        Symbol? exprType = SymbolTable.GetSymbol(node.GetExp());
        
        SymbolTable.AddNode(node,type == null ||type != exprType? Symbol.notOk: Symbol.ok);
    }

    public override void OutAMinusassignStmt(AMinusassignStmt node)
    {
        Symbol? type = SymbolTable.GetSymbol(node.GetId());
        Symbol? exprType = SymbolTable.GetSymbol(node.GetExp());
        
        SymbolTable.AddNode(node,type == null ||type != exprType? Symbol.notOk: Symbol.ok);
    }

    public override void OutAPrefixplusStmt(APrefixplusStmt node)
    {
        UnaryoperatorToSymbolTable(node);
    }
    public override void OutAPrefixminusStmt(APrefixminusStmt node)
    {
        UnaryoperatorToSymbolTable(node);
    }
    public override void OutASuffixplusStmt(ASuffixplusStmt node)
    {
        UnaryoperatorToSymbolTable(node);
    }
    public override void OutASuffixminusStmt(ASuffixminusStmt node)
    {
        UnaryoperatorToSymbolTable(node);
    }
    public override void OutADeclStmt(ADeclStmt node)
    {
        PUnittype unit = node.GetUnittype();
        var standardType = unit as ATypeUnittype;
        if (standardType != null)
        {
            switch (standardType.GetType())
            {
                case AIntType a:
                    if (a.Parent() != null)
                    {
                        SymbolTable.AddId(node.GetId(), node, 
                            SymbolTable.IsInCurrentScope(node.GetId()) ? Symbol.notOk : Symbol.Int);
                    }
                    break;
                case ADecimalType b:
                    if (b.Parent() != null)
                    {
                        SymbolTable.AddId(node.GetId(), node,
                            SymbolTable.IsInCurrentScope(node.GetId()) ? Symbol.notOk : Symbol.Decimal);
                    }
                    break;
                case ABoolType c:
                    if (c.Parent() != null)
                    {
                        SymbolTable.AddId(node.GetId(), node,
                            SymbolTable.IsInCurrentScope(node.GetId()) ? Symbol.notOk : Symbol.Bool);
                    }
                    break;
                case ACharType d:
                    if (d.Parent() != null)
                    {
                        SymbolTable.AddId(node.GetId(), node,
                            SymbolTable.IsInCurrentScope(node.GetId()) ? Symbol.notOk : Symbol.Char);
                    }
                    break;
                case AStringType e:
                    if (e.Parent() != null)
                    {
                        SymbolTable.AddId(node.GetId(), node,
                            SymbolTable.IsInCurrentScope(node.GetId()) ? Symbol.notOk : Symbol.String);
                    }
                    break;
            }
        }
        var customType = unit as AUnitUnittype;
        if (customType != null)
        {
            IEnumerable<ANumUnituse> numerator = customType.GetUnituse().OfType<ANumUnituse>();
            IEnumerable<ADenUnituse> denomerator = customType.GetUnituse().OfType<ADenUnituse>();
            
            // Her skal logikken implementeres 
            
        }
    }
    
    public override void OutADeclassStmt(ADeclassStmt node)
    { 
        // Assignment have to be typechecked before Decl should add to symboltable
        bool declared = SymbolTable.IsInCurrentScope(node.GetId());
        if (!declared)
        {
            Symbol? exprType = SymbolTable.GetSymbol(node.GetExp());
            PUnittype unit = node.GetUnittype();
            var standardType = unit as ATypeUnittype;
            if (standardType != null)
            {
                switch (standardType.GetType())
                {
                    case AIntType a:
                        SymbolTable.AddId(node.GetId(), node, exprType == Symbol.Int ? Symbol.notOk : Symbol.Int);
                        break;
                    case ADecimalType b:
                        SymbolTable.AddId(node.GetId(), node, exprType == Symbol.Decimal ? Symbol.notOk : Symbol.Decimal);
                        break;
                    case ABoolType c:
                        SymbolTable.AddId(node.GetId(), node, exprType == Symbol.Bool ? Symbol.notOk : Symbol.Bool);
                        break;
                    case ACharType d:
                        SymbolTable.AddId(node.GetId(), node, exprType == Symbol.Char ? Symbol.notOk : Symbol.Char);
                        break;
                    case AStringType e:
                        SymbolTable.AddId(node.GetId(), node, exprType == Symbol.String ? Symbol.notOk : Symbol.String);
                        break;
                }
            }
            var customType = unit as AUnitUnittype;
            if (customType != null)
            {
                IEnumerable<ANumUnituse> numerator = customType.GetUnituse().OfType<ANumUnituse>();
                IEnumerable<ADenUnituse> denomerator = customType.GetUnituse().OfType<ADenUnituse>();

                // Her skal logikken implementeres 

            }
        }
        else
        {
            SymbolTable.AddId(node.GetId(), node, Symbol.notOk);
        }
    }
    public override void OutAFunccallStmt(AFunccallStmt node)
    {
        
    }

    public override void OutAIfStmt(AIfStmt node)
    {
        Symbol? CondExpr = SymbolTable.GetSymbol(node.GetExp());
        SymbolTable.AddNode(node,CondExpr != Symbol.Bool|| CondExpr == null ? Symbol.notOk: Symbol.ok);
    }

    public override void OutAElseifStmt(AElseifStmt node)
    {
        Symbol? CondExpr = SymbolTable.GetSymbol(node.GetExp());
        SymbolTable.AddNode(node,CondExpr != Symbol.Bool || CondExpr == null ? Symbol.notOk: Symbol.ok);
    }

    public override void OutAElseStmt(AElseStmt node)
    {
        Symbol? symbol = SymbolTable.GetSymbol(node);
        SymbolTable.AddNode(node, symbol == null ? Symbol.notOk : Symbol.ok);
    }

    public override void OutAForStmt(AForStmt node)
    {
        Symbol? cond = SymbolTable.GetSymbol(node.GetCond());
        Symbol? Incr = SymbolTable.GetSymbol(node.GetIncre());
        SymbolTable.AddNode(node, cond != Symbol.Bool ? Symbol.notOk : Symbol.ok);
    }

    public override void OutAWhileStmt(AWhileStmt node)
    {
        Symbol? cond = SymbolTable.GetSymbol(node.GetExp());
        SymbolTable.AddNode(node, cond != Symbol.Bool? Symbol.notOk: Symbol.ok);
    }

    public override void OutADowhileStmt(ADowhileStmt node)
    {
        Symbol? cond = SymbolTable.GetSymbol(node.GetExp());
        SymbolTable.AddNode(node, cond != Symbol.Bool? Symbol.notOk: Symbol.ok);  
    }
    

    private void UnaryoperatorToSymbolTable(Node node)
    {
        Symbol? expr = SymbolTable.GetSymbol(node);
        switch (expr)
        {
            case Symbol.Decimal:
                SymbolTable.AddNode(node,Symbol.ok);
                break;
            case Symbol.Int:
                SymbolTable.AddNode(node,Symbol.ok); 
                break;
            case Symbol.Char:
                SymbolTable.AddNode(node,Symbol.ok);
                break;
            default:
                SymbolTable.AddNode(node,Symbol.notOk);
                break;
        }
    }
    
}