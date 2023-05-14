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
        Symbol? idToType = symbolTable.GetSymbol(node.GetId().ToString().Trim());
        PExp exp = node.GetExp();
        switch (idToType)
        {
            case Symbol.notOk:
                symbolTable.AddNode(node, Symbol.notOk);
                break;
            case Symbol.Int:
                symbolTable.AddNode(node, symbolTable.GetSymbol(exp) == Symbol.Int ? Symbol.ok : Symbol.notOk);
                break;
            case Symbol.Decimal:
                symbolTable.AddNode(node, symbolTable.GetSymbol(exp) == Symbol.Decimal ? Symbol.ok : Symbol.notOk);
                break;
            case Symbol.Bool:
                symbolTable.AddNode(node, symbolTable.GetSymbol(exp) == Symbol.Bool ? Symbol.ok : Symbol.notOk);
                break;
            case Symbol.Char:
                symbolTable.AddNode(node, symbolTable.GetSymbol(exp) == Symbol.Char ? Symbol.ok : Symbol.notOk);
                break;
            case Symbol.String:
                symbolTable.AddNode(node, symbolTable.GetSymbol(exp) == Symbol.String ? Symbol.ok : Symbol.notOk);
                break;
            default:
                var typeUnit =symbolTable.GetUnit(symbolTable.GetNodeFromId(node.GetId().ToString().Trim()));
                var expUnit = symbolTable.GetUnit(exp);
                if (typeUnit != null && expUnit != null)
                {
                    symbolTable.AddNode(node, symbolTable.CompareCustomUnits(typeUnit, expUnit) ? Symbol.ok : Symbol.notOk);
                }
                else
                {
                    symbolTable.AddNode(node, Symbol.notOk);
                }
                break;
        }
    }
    public override void OutAPlusassignStmt(APlusassignStmt node)
    {
        // OVervej om man kan sige += med string, char og bool typer
        Symbol? type = symbolTable.GetSymbol("" + node.GetId());
        PExp exp = node.GetExp();
        switch (type)
        {
            case Symbol.Int:
                symbolTable.AddNode(node, symbolTable.GetSymbol(exp) == Symbol.Int ? Symbol.ok : Symbol.notOk);
                break;
            case Symbol.Decimal:
                symbolTable.AddNode(node, symbolTable.GetSymbol(exp) == Symbol.Decimal ? Symbol.ok : Symbol.notOk);
                break;
            case Symbol.Bool:
                symbolTable.AddNode(node, symbolTable.GetSymbol(exp) == Symbol.Bool ? Symbol.ok : Symbol.notOk);
                break;
            case Symbol.Char:
                symbolTable.AddNode(node, symbolTable.GetSymbol(exp) == Symbol.Char ? Symbol.ok : Symbol.notOk);
                break;
            case Symbol.String:
                symbolTable.AddNode(node, symbolTable.GetSymbol(exp) == Symbol.String ? Symbol.ok : Symbol.notOk);
                break;
            default:
                var typeUnit =symbolTable.GetUnit(symbolTable.GetNodeFromId(node.GetId().ToString()));
                var expUnit = symbolTable.GetUnit(exp);
                symbolTable.AddNode(node, symbolTable.CompareCustomUnits(typeUnit, expUnit) ? Symbol.ok : Symbol.notOk);
                break;
        }
    }
    public override void OutAMinusassignStmt(AMinusassignStmt node)
    {
        Symbol? type = symbolTable.GetSymbol("" + node.GetId());
        PExp exp = node.GetExp();
        switch (type)
        {
            case Symbol.Int:
                symbolTable.AddNode(node, symbolTable.GetSymbol(exp) == Symbol.Int ? Symbol.ok : Symbol.notOk);
                break;
            case Symbol.Decimal:
                symbolTable.AddNode(node, symbolTable.GetSymbol(exp) == Symbol.Decimal ? Symbol.ok : Symbol.notOk);
                break;
            case Symbol.Bool:
                symbolTable.AddNode(node, symbolTable.GetSymbol(exp) == Symbol.Bool ? Symbol.ok : Symbol.notOk);
                break;
            case Symbol.Char:
                symbolTable.AddNode(node, symbolTable.GetSymbol(exp) == Symbol.Char ? Symbol.ok : Symbol.notOk);
                break;
            case Symbol.String:
                symbolTable.AddNode(node, symbolTable.GetSymbol(exp) == Symbol.String ? Symbol.ok : Symbol.notOk);
                break;
            default:
                var typeUnit =symbolTable.GetUnit(symbolTable.GetNodeFromId(node.GetId().ToString()));
                var expUnit = symbolTable.GetUnit(exp);
                symbolTable.AddNode(node, symbolTable.CompareCustomUnits(typeUnit, expUnit) ? Symbol.ok : Symbol.notOk);
                break;
        }
    }
    public override void OutAPrefixplusStmt(APrefixplusStmt node) => UnaryoperatorToSymbolTable(node);
    public override void OutAPrefixminusStmt(APrefixminusStmt node) => UnaryoperatorToSymbolTable(node);
    public override void OutASuffixplusStmt(ASuffixplusStmt node) => UnaryoperatorToSymbolTable(node);
    public override void OutASuffixminusStmt(ASuffixminusStmt node) => UnaryoperatorToSymbolTable(node);
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
    public override void OutADeclStmt(ADeclStmt node)
    {
        if (!symbolTable.IsInCurrentScope(node.GetId()))
        {
            switch (node.GetType())
            {
                case AIntType:
                    symbolTable.AddId(node.GetId().ToString(), node, Symbol.Int);
                    break;
                case ADecimalType:
                    symbolTable.AddId(node.GetId().ToString(), node, Symbol.Decimal);
                    break;
                case ABoolType:
                    symbolTable.AddId(node.GetId().ToString(), node, Symbol.Bool);
                    break;
                case ACharType:
                    symbolTable.AddId(node.GetId().ToString(), node, Symbol.Char);
                    break;
                case AStringType:
                    symbolTable.AddId(node.GetId().ToString(), node, Symbol.String);
                    break;
                case AUnitType customType:
                    var unit = symbolTable.GetUnit(customType);
                    symbolTable.AddIdToNode(node.GetId().ToString(), node);
                    symbolTable.AddNodeToUnit(node, unit);
                    symbolTable.AddNode(node, Symbol.ok);
                    break; 
                default:
                    symbolTable.AddNode(node, Symbol.notOk);
                    break;
            }
        }
        else
        {
            symbolTable.AddNode(node, Symbol.notOk);
        }
    }
    public override void OutADeclassStmt(ADeclassStmt node)
    { 
        // Assignment have to be typechecked before Decl should add to symbolTable
        if (!symbolTable.IsInCurrentScope(node.GetId()))
        {
            Symbol? exprType = symbolTable.GetSymbol(node.GetExp());
            PType unit = node.GetType();
            if (unit != null)
            {
                switch (unit)
                {
                    case AIntType a:
                        symbolTable.AddId(node.GetId().ToString(), node, exprType == Symbol.Int ? Symbol.Int : Symbol.notOk);
                        break;
                    case ADecimalType b:
                        symbolTable.AddId(node.GetId().ToString(), node, exprType == Symbol.Decimal ? Symbol.Decimal : Symbol.notOk);
                        break;
                    case ABoolType c:
                        symbolTable.AddId(node.GetId().ToString(), node, exprType == Symbol.Bool ? Symbol.Bool : Symbol.notOk);
                        break;
                    case ACharType d:
                        symbolTable.AddId(node.GetId().ToString(), node, exprType == Symbol.Char ? Symbol.Char : Symbol.notOk);
                        break;
                    case AStringType e:
                        symbolTable.AddId(node.GetId().ToString(), node, exprType == Symbol.String ? Symbol.String : Symbol.notOk);
                        break;
                    case AUnitType customType:
                        var unitType = symbolTable.GetUnit(customType);
                        var expType = symbolTable.GetUnit(node.GetExp());
                        if (unitType != null && expType != null)
                        {
                            if (symbolTable.CompareCustomUnits(unitType, expType))
                            {
                                symbolTable.AddIdToNode(node.GetId().ToString(), node);
                                symbolTable.AddNodeToUnit(node, unitType);
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
                    default:
                        symbolTable.AddNode(node, Symbol.notOk);
                        break;
                }
            }
        }
        else
        {
            symbolTable.AddNode(node, Symbol.notOk);
        }
    }
    public override void OutAFunccallStmt(AFunccallStmt node)
    {
       // STANDALONE FUNCCAL
       // Returtype er ubetydlig
       Node? func = symbolTable.GetFuncFromId(node.GetId().ToString().Trim());
       if (func != null)
       {
           List<PType> args = symbolTable.GetFunctionArgs(func);
           List<PExp>? parameters = node.GetParams() as List<PExp>;
           if (args.Count() == parameters.Count())
           {
               bool matches = true;
               for(int i = 0; i < args.Count(); i++)
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
               symbolTable.AddNode(node, matches ? Symbol.ok : Symbol.notOk);
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

    public Symbol? PTypeToSymbol(PType type)
    {
        switch (type)
        {
            case AIntType:
                return Symbol.Bool;
            case ADecimalType:
                return Symbol.Decimal;
            case ABoolType:
                return Symbol.Bool;
            case ACharType:
                return Symbol.Char;
            case AStringType:
                return Symbol.String;
            case AVoidType:
                return Symbol.Func;
            default:
                return null;
        }
    }

    public override void InAReturnStmt(AReturnStmt node)
    {
      //already set before
      Node parent = node.Parent();
      while (parent is not PGlobal)
      {
          parent = parent.Parent();
      }
      PExp? returnExp = node.GetExp();
      switch (parent)
      {
          case ALoopGlobal:
              // Should not have return in Loop
              symbolTable.AddNode(node, Symbol.notOk);
              break;
          case AProgGlobal:
              // Should not have return in Prog
              symbolTable.AddNode(node, Symbol.notOk);
              break;
          case ATypedGlobal aTypedFunc:
              PType typedType = aTypedFunc.GetType();
              //Symbol? symbol = symbolTable.GetSymbol(returnExp);
              Symbol? symbol = PTypeToSymbol(typedType);
              switch (symbol)
              {
                  case Symbol.Int:
                      //Int function
                      symbolTable.AddNode(node, symbolTable.GetSymbol(returnExp) == Symbol.Int ? Symbol.ok : Symbol.notOk);
                      break;
                  case Symbol.Decimal:
                      symbolTable.AddNode(node, symbolTable.GetSymbol(returnExp) == Symbol.Decimal ? Symbol.ok : Symbol.notOk);
                      break;
                  case Symbol.Bool:
                      symbolTable.AddNode(node, symbolTable.GetSymbol(returnExp) == Symbol.Bool ? Symbol.ok : Symbol.notOk);
                      break;
                  case Symbol.Char:
                      symbolTable.AddNode(node, symbolTable.GetSymbol(returnExp) == Symbol.Char ? Symbol.ok : Symbol.notOk);
                      break;
                  case Symbol.String:
                      symbolTable.AddNode(node, symbolTable.GetSymbol(returnExp) == Symbol.String ? Symbol.ok : Symbol.notOk);
                      break;
                  case Symbol.Func:
                      symbolTable.AddNode(node, symbolTable.GetSymbol(returnExp) == Symbol.Func ? Symbol.ok : Symbol.notOk);
                      break;
                  default:
                      Tuple<List<AUnitdeclGlobal>, List<AUnitdeclGlobal>> funcType = symbolTable.GetUnit(typedType);
                      Tuple<List<AUnitdeclGlobal>, List<AUnitdeclGlobal>>? returnType = symbolTable.GetUnit(returnExp);
                      if (symbolTable.GetUnit(returnExp) != null)
                      {
                          if (symbolTable.CompareCustomUnits(funcType, returnType))
                          {
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
              break;
          case AUntypedGlobal aUntypedFunc:
              if (symbolTable.GetUnit(aUntypedFunc) != null)
              {
                  symbolTable.AddNode(node, symbolTable.CompareCustomUnits(symbolTable.GetUnit(aUntypedFunc), symbolTable.GetUnit(returnExp)) ? Symbol.ok : Symbol.notOk);
              } else if (symbolTable.GetReturnFromNode(aUntypedFunc) != null)
              {
                  symbolTable.AddNode(node, symbolTable.GetReturnFromNode(aUntypedFunc) == symbolTable.GetSymbol(returnExp) ? Symbol.ok : Symbol.notOk);
              }
              else
              {
                  if (symbolTable.GetUnit(returnExp) != null)
                  {
                      symbolTable.AddNodeToUnit(aUntypedFunc, symbolTable.GetUnit(returnExp));
                      symbolTable.AddNode(node, Symbol.ok);
                  } else if (symbolTable.GetSymbol(returnExp) != null)
                  {
                      symbolTable.AddReturnSymbol(aUntypedFunc, symbolTable.GetSymbol(returnExp));
                      symbolTable.AddNode(node, (Symbol)symbolTable.GetSymbol(returnExp));
                  }
              }
              break;
          default:
              symbolTable.AddNode(node, Symbol.notOk);
              break;
      }
    }
    public override void OutAIfStmt(AIfStmt node)
    {
        Symbol? condExpr = symbolTable.GetSymbol(node.GetExp());
        symbolTable.AddNode(node, condExpr == Symbol.Bool ? Symbol.Bool: Symbol.ok);
    }
    public override void OutAElseifStmt(AElseifStmt node)
    {
        Symbol? condExpr = symbolTable.GetSymbol(node.GetExp());
        symbolTable.AddNode(node, condExpr == Symbol.Bool ? Symbol.Bool: Symbol.ok);
    }
    public override void OutAElseStmt(AElseStmt node)
    {
        // Check om der er en if eller else if inden

        symbolTable.AddNode(node, Symbol.ok);
    }
    public override void OutAForStmt(AForStmt node)
    {
        Symbol? condExpr = symbolTable.GetSymbol(node.GetCond());
        symbolTable.AddNode(node, condExpr == Symbol.Bool ? Symbol.Bool: Symbol.ok);
    }
    public override void OutAWhileStmt(AWhileStmt node)
    {
        Symbol? condExpr = symbolTable.GetSymbol(node.GetExp());
        symbolTable.AddNode(node, condExpr == Symbol.Bool ? Symbol.Bool: Symbol.ok);
    }
    public override void OutADowhileStmt(ADowhileStmt node)
    {
        Symbol? condExpr = symbolTable.GetSymbol(node.GetExp());
        symbolTable.AddNode(node, condExpr == Symbol.Bool ? Symbol.Bool: Symbol.ok);
    }

}