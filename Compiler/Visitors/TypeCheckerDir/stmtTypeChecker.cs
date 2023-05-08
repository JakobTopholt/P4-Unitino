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
        
        // if type == null (id was never declared) (The reason we dont use .isInCurrentScope here is we want to iclude foward refrences
        // if type != exprType (Incompatible types)
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
    public override void OutAPrefixplusStmt(APrefixplusStmt node) => UnaryoperatorToSymbolTable(node);
    public override void OutAPrefixminusStmt(APrefixminusStmt node) => UnaryoperatorToSymbolTable(node);
    public override void OutASuffixplusStmt(ASuffixplusStmt node) => UnaryoperatorToSymbolTable(node);
    public override void OutASuffixminusStmt(ASuffixminusStmt node) => UnaryoperatorToSymbolTable(node);
    public override void OutADeclStmt(ADeclStmt node)
    {
        if (!symbolTable.IsInCurrentScope(node.GetId()))
        {
            switch (node.GetType())
            {
                case AIntType:
                    symbolTable.AddId(node.GetId(), node, Symbol.Int);
                    break;
                case ADecimalType:
                    symbolTable.AddId(node.GetId(), node, Symbol.Decimal);
                    break;
                case ABoolType:
                    symbolTable.AddId(node.GetId(), node, Symbol.Bool);
                    break;
                case ACharType:
                    symbolTable.AddId(node.GetId(), node, Symbol.Char);
                    break;
                case AStringType:
                    symbolTable.AddId(node.GetId(), node, Symbol.String);
                    break;
                case AUnitType customType:
                {
                    // Declared a custom sammensat unit (Ikke en baseunit declaration)
                    IEnumerable<ANumUnituse> numerator = customType.GetUnituse().OfType<ANumUnituse>();
                    IEnumerable<ADenUnituse> denomerator = customType.GetUnituse().OfType<ADenUnituse>();
                    
                    // Declaration validering for sammensat unit her
                    // Check if Numerators or denomarots contains units that does not exist

                    symbolTable.AddNumerators(node.GetId(), node, numerator);
                    symbolTable.AddDenomerators(node.GetId(), node, denomerator);
                    break; 
                }
            }
        }
        else
        {
            symbolTable.AddId(node.GetId(), node, Symbol.notOk);
        }
    }
    public override void OutADeclassStmt(ADeclassStmt node)
    { 
        // Assignment have to be typechecked before Decl should add to symbolTable
        bool declared = symbolTable.IsInCurrentScope(node.GetId());
        if (!declared)
        {
            Symbol? exprType = symbolTable.GetSymbol(node.GetExp());
            PType unit = node.GetType();
            if (unit != null)
            {
                switch (unit)
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
            var customType = unit as AUnitType;
            if (customType != null)
            {
                IEnumerable<ANumUnituse> numerator = customType.GetUnituse().OfType<ANumUnituse>();
                IEnumerable<ADenUnituse> denomerator = customType.GetUnituse().OfType<ADenUnituse>();

                // Her skal logikken implementeres 
                // Mangler assignment typecheck logic

                symbolTable.AddNumerators(node.GetId(), node, numerator);
                symbolTable.AddDenomerators(node.GetId(), node, denomerator);
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
              // Implement void type? PUnittype?
              PType returnType = aTypedFunc.GetType();
              PType? exprType = symbolTable.GetUnitFromExpr(node.GetExp());
              if(exprType == null)
                  throw new Exception("Return is null value");
              // Does exprSymbol match returnType?
              symbolTable.AddNode(node, exprType == returnType ? Symbol.ok : Symbol.notOk);
              break;
          
          case AUntypedFunc aUntypedFunc:
              // Does all return-expressions evaluate to the same type
              // Gem først returnstatement type to the function node
              // If the next iteration does not match throw errors
              PType? exprType2 = symbolTable.GetUnitFromExpr(node.GetExp());

              if (!symbolTable.funcToReturn.ContainsKey(aUntypedFunc.GetId()))
              {
                  // add the return to dictionary
                  symbolTable.funcToReturn.Add(aUntypedFunc.GetId(), exprType2);  
                  symbolTable.AddNode(node, Symbol.ok);
              }
              else
              {
                  if (symbolTable.DoesReturnStmtMatch(aUntypedFunc.GetId(), exprType2))
                      symbolTable.AddNode(node, Symbol.ok);
                  else
                  {
                      symbolTable.AddNode(node, Symbol.notOk);
                      throw new Exception("Return is not correct type");
                  }
              }
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