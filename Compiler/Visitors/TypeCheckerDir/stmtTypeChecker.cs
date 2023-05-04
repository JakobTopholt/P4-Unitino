using Moduino.analysis;
using Moduino.node;

namespace Compiler.Visitors.TypeCheckerDir;

public class stmtTypeChecker : DepthFirstAdapter
{
    protected SymbolTable symbolTable;
    public stmtTypeChecker(SymbolTable symbolTable)
    {
        this.symbolTable = symbolTable;
    }
    
    public override void OutAAssignStmt(AAssignStmt node) {
        Symbol? type = symbolTable.GetSymbol("" + node.GetId());
        Symbol? exprType = symbolTable.GetSymbol(node.GetExp());

        // if types match stmt is ok, else notok
        symbolTable.AddNode(node, type == null || type != exprType ? Symbol.notOk : Symbol.ok);
    }

    public override void OutAPlusassignStmt(APlusassignStmt node)
    {
        Symbol? type = symbolTable.GetSymbol(node.GetId());
        Symbol? exprType = symbolTable.GetSymbol(node.GetExp());
        
        symbolTable.AddNode(node,type == null ||type != exprType? Symbol.notOk: Symbol.ok);
    }

    public override void OutAMinusassignStmt(AMinusassignStmt node)
    {
        Symbol? type = symbolTable.GetSymbol(node.GetId());
        Symbol? exprType = symbolTable.GetSymbol(node.GetExp());
        
        symbolTable.AddNode(node,type == null ||type != exprType? Symbol.notOk: Symbol.ok);
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
                        symbolTable.AddId(node.GetId(), node, 
                            symbolTable.IsInCurrentScope(node.GetId()) ? Symbol.notOk : Symbol.Int);
                    }
                    break;
                case ADecimalType b:
                    if (b.Parent() != null)
                    {
                        symbolTable.AddId(node.GetId(), node,
                            symbolTable.IsInCurrentScope(node.GetId()) ? Symbol.notOk : Symbol.Decimal);
                    }
                    break;
                case ABoolType c:
                    if (c.Parent() != null)
                    {
                        symbolTable.AddId(node.GetId(), node,
                            symbolTable.IsInCurrentScope(node.GetId()) ? Symbol.notOk : Symbol.Bool);
                    }
                    break;
                case ACharType d:
                    if (d.Parent() != null)
                    {
                        symbolTable.AddId(node.GetId(), node,
                            symbolTable.IsInCurrentScope(node.GetId()) ? Symbol.notOk : Symbol.Char);
                    }
                    break;
                case AStringType e:
                    if (e.Parent() != null)
                    {
                        symbolTable.AddId(node.GetId(), node,
                            symbolTable.IsInCurrentScope(node.GetId()) ? Symbol.notOk : Symbol.String);
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
        // Assignment have to be typechecked before Decl should add to symbolTable
        bool declared = symbolTable.IsInCurrentScope(node.GetId());
        if (!declared)
        {
            Symbol? exprType = symbolTable.GetSymbol(node.GetExp());
            PUnittype unit = node.GetUnittype();
            var standardType = unit as ATypeUnittype;
            if (standardType != null)
            {
                switch (standardType.GetType())
                {
                    case AIntType a:
                        symbolTable.AddId(node.GetId(), node, exprType == Symbol.Int ? Symbol.notOk : Symbol.Int);
                        break;
                    case ADecimalType b:
                        symbolTable.AddId(node.GetId(), node, exprType == Symbol.Decimal ? Symbol.notOk : Symbol.Decimal);
                        break;
                    case ABoolType c:
                        symbolTable.AddId(node.GetId(), node, exprType == Symbol.Bool ? Symbol.notOk : Symbol.Bool);
                        break;
                    case ACharType d:
                        symbolTable.AddId(node.GetId(), node, exprType == Symbol.Char ? Symbol.notOk : Symbol.Char);
                        break;
                    case AStringType e:
                        symbolTable.AddId(node.GetId(), node, exprType == Symbol.String ? Symbol.notOk : Symbol.String);
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
            symbolTable.AddId(node.GetId(), node, Symbol.notOk);
        }
    }
    public override void OutAFunccallStmt(AFunccallStmt node)
    {
        
    }

    public override void InAReturnStmt(AReturnStmt node)
    {
      //already set before
      Node parent = node.Parent();
      while (parent is not PFunc)
      {
          parent = parent.Parent();
      }
      switch (parent)
      {
          case ALoopFunc:
              symbolTable.AddNode(node, Symbol.notOk);
              throw new Exception("Should not have return in Loop");
              break;
          case AProgFunc:
              symbolTable.AddNode(node, Symbol.notOk);
              throw new Exception("Should not have return in Prog");
              break;
          case ATypedFunc aTypedFunc:
              // Does the return-expressions' type match function type
              PUnittype returnType = aTypedFunc.GetUnittype();
              PUnittype exprType = symbolTable.GetUnitFromExpr(node.GetExp());

              // Does exprSymbol match returnType?
              symbolTable.AddNode(node, exprType == returnType ? Symbol.ok : Symbol.notOk);
              break;
          
          case AUntypedFunc aUntypedFunc:
              // Does all return-expressions evaluate to the same type
              // Gem først returnstatement type to the function node
              // If the next iteration does not match throw errors
              
              
              break;
      }
    }

    public override void OutAIfStmt(AIfStmt node)
    {
        Symbol? CondExpr = symbolTable.GetSymbol(node.GetExp());
        symbolTable.AddNode(node,CondExpr != Symbol.Bool|| CondExpr == null ? Symbol.notOk: Symbol.ok);
    }

    public override void OutAElseifStmt(AElseifStmt node)
    {
        Symbol? CondExpr = symbolTable.GetSymbol(node.GetExp());
        symbolTable.AddNode(node,CondExpr != Symbol.Bool || CondExpr == null ? Symbol.notOk: Symbol.ok);
    }

    public override void OutAElseStmt(AElseStmt node)
    {
        Symbol? symbol = symbolTable.GetSymbol(node);
        symbolTable.AddNode(node, symbol == null ? Symbol.notOk : Symbol.ok);
    }

    public override void OutAForStmt(AForStmt node)
    {
        Symbol? cond = symbolTable.GetSymbol(node.GetCond());
        Symbol? Incr = symbolTable.GetSymbol(node.GetIncre());
        symbolTable.AddNode(node, cond != Symbol.Bool ? Symbol.notOk : Symbol.ok);
    }

    public override void OutAWhileStmt(AWhileStmt node)
    {
        Symbol? cond = symbolTable.GetSymbol(node.GetExp());
        symbolTable.AddNode(node, cond != Symbol.Bool? Symbol.notOk: Symbol.ok);
    }

    public override void OutADowhileStmt(ADowhileStmt node)
    {
        Symbol? cond = symbolTable.GetSymbol(node.GetExp());
        symbolTable.AddNode(node, cond != Symbol.Bool? Symbol.notOk: Symbol.ok);  
    }
    

    private void UnaryoperatorToSymbolTable(Node node)
    {
        Symbol? expr = symbolTable.GetSymbol(node);
        switch (expr)
        {
            case Symbol.Decimal:
                symbolTable.AddNode(node,Symbol.ok);
                break;
            case Symbol.Int:
                symbolTable.AddNode(node,Symbol.ok); 
                break;
            case Symbol.Char:
                symbolTable.AddNode(node,Symbol.ok);
                break;
            default:
                symbolTable.AddNode(node,Symbol.notOk);
                break;
        }
    }
    
}