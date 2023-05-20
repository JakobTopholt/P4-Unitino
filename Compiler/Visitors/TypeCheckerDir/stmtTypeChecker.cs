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
    
    public Stack<string> locations = new ();
    public string tempResult = "";
    public List<string?> errorResults = new ();
    public int indent = 0;
    public bool reported = false;

    protected string IndentedString(string s)
    {
        return new string(' ', indent * 3) + s;
    }

    protected void PrintError()
    {
        if (tempResult != "" && !reported)
        {
            string error = "";
            error += tempResult;
            foreach(string location in locations)
            {
                error += location;
            }
            errorResults.Add(error);
            locations.Pop();
            reported = true;
        }
        else
        {
            locations.Pop();
        }
    }
    
    
    public override void InAAssignStmt(AAssignStmt node)
    {
        locations.Push(IndentedString($"in Assignment {node}\n"));
        indent++;
    }

    public override void OutAAssignStmt(AAssignStmt node) {
        Symbol? idToType = symbolTable.GetSymbol(node.GetId().ToString().Trim());
        PExp exp = node.GetExp();
        Symbol? expSymbol = symbolTable.GetSymbol(exp);
        switch (idToType)
        {
            case Symbol.NotOk:
                symbolTable.AddNode(node, Symbol.NotOk);
                tempResult += IndentedString("Id in assign contains error\n");
                break;
            case Symbol.Int:
                symbolTable.AddNode(node, expSymbol == Symbol.Int ? Symbol.Int : Symbol.NotOk);
                tempResult += expSymbol == Symbol.Int ? "" : IndentedString("expression is not an Int\n");                
                break;
            case Symbol.Decimal:
                symbolTable.AddNode(node, expSymbol == Symbol.Decimal ? Symbol.Decimal : Symbol.NotOk);
                tempResult += expSymbol == Symbol.Decimal ? "" : IndentedString("expression is not an Decimal\n");                
                break;
            case Symbol.Bool:
                symbolTable.AddNode(node, expSymbol == Symbol.Bool ? Symbol.Bool : Symbol.NotOk);
                tempResult += expSymbol == Symbol.Bool ? "" : IndentedString("expression is not an Bool\n");                
                break;
            case Symbol.Char:
                symbolTable.AddNode(node, expSymbol == Symbol.Char ? Symbol.Char : Symbol.NotOk);
                tempResult += expSymbol == Symbol.Char ? "" : IndentedString("expression is not an Char\n");                
                break;
            case Symbol.String:
                symbolTable.AddNode(node, expSymbol is Symbol.String or Symbol.Char ? Symbol.String : Symbol.NotOk);
                tempResult += expSymbol is Symbol.String or Symbol.Char ? "" : IndentedString("expression is not a string or char\n");                
                break;
            default:
                if (symbolTable.GetNodeFromId(node.GetId().ToString().Trim(), out var typeUn) &&
                    symbolTable.GetUnit(typeUn, out var typeUnit) &&
                    symbolTable.GetUnit(exp, out var expUnit))
                {
                    symbolTable.AddNode(node, symbolTable.CompareCustomUnits(typeUnit, expUnit) ? Symbol.Ok : Symbol.NotOk);
                    tempResult += symbolTable.CompareCustomUnits(typeUnit, expUnit) ? "" : IndentedString("expression is not correct unitType\n");
                }
                else
                {
                    symbolTable.AddNode(node, Symbol.NotOk);
                    tempResult += IndentedString("Type of Expression is not correct\n");
                }
                break;
        }
        PrintError();
        indent--;
    }

    public override void InAPlusassignStmt(APlusassignStmt node)
    {
        locations.Push(IndentedString($"in Plusassign {node}\n"));
        indent++;
    }

    public override void OutAPlusassignStmt(APlusassignStmt node)
    {
        // OVervej om man kan sige += med string, char og bool typer
        Symbol? type = symbolTable.GetSymbol(node.GetId().ToString().Trim());
        PExp exp = node.GetExp();
        Symbol? expSymbol = symbolTable.GetSymbol(exp);
        switch (type)
        {
            case Symbol.Int:
                symbolTable.AddNode(node, symbolTable.GetSymbol(exp) == Symbol.Int ? Symbol.Ok : Symbol.NotOk);
                tempResult += expSymbol == Symbol.Int ? "" : IndentedString("expression is not an Int\n");                
                break;
            case Symbol.Decimal:
                symbolTable.AddNode(node, symbolTable.GetSymbol(exp) == Symbol.Decimal ? Symbol.Ok : Symbol.NotOk);
                tempResult += expSymbol == Symbol.Decimal ? "" : IndentedString("expression is not an Decimal\n");                
                break;
            case Symbol.Bool:
                symbolTable.AddNode(node, symbolTable.GetSymbol(exp) == Symbol.Bool ? Symbol.Ok : Symbol.NotOk);
                tempResult += expSymbol == Symbol.Bool ? "" : IndentedString("expression is not an Bool\n");                
                break;
            case Symbol.Char:
                symbolTable.AddNode(node, symbolTable.GetSymbol(exp) == Symbol.Int ? Symbol.Ok : Symbol.NotOk);
                tempResult += expSymbol == Symbol.Int ? "" : IndentedString("expression is not an integer\n");
                break;
            case Symbol.String:
                symbolTable.AddNode(node, symbolTable.GetSymbol(exp) is Symbol.String or Symbol.Char ? Symbol.String : Symbol.NotOk);
                tempResult += expSymbol is Symbol.String or Symbol.Char ? "" : IndentedString("expression is not a string or char\n");
                break;
            default:
                if (symbolTable.GetNodeFromId(node.GetId().ToString(), out var b))
                {
                    symbolTable.GetUnit(b, out var typeUnit);
                    symbolTable.GetUnit(exp, out var expUnit);
                    symbolTable.AddNode(node, symbolTable.CompareCustomUnits(typeUnit, expUnit) ? Symbol.Ok : Symbol.NotOk);
                    tempResult += symbolTable.CompareCustomUnits(typeUnit, expUnit) ? "" : IndentedString("expression is not correct unitType\n");
                }
                else
                {
                    symbolTable.AddNode(node, Symbol.NotOk);
                    tempResult += IndentedString("is not correct Type\n");
                }
                break;
        }
        PrintError();
        indent--;
    }

    public override void InAMinusassignStmt(AMinusassignStmt node)
    {
        locations.Push(IndentedString($"in MinusAssignment {node}\n"));
        indent++;
    }

    public override void OutAMinusassignStmt(AMinusassignStmt node)
    {
        Symbol? type = symbolTable.GetSymbol("" + node.GetId().ToString().Trim());
        PExp exp = node.GetExp();
        Symbol? expSymbol = symbolTable.GetSymbol(exp);
        switch (type)
        {
            case Symbol.Int:
                symbolTable.AddNode(node, symbolTable.GetSymbol(exp) == Symbol.Int ? Symbol.Ok : Symbol.NotOk);
                tempResult += expSymbol == Symbol.Int ? "" : IndentedString("expression is not an Int\n");                
                break;
            case Symbol.Decimal:
                symbolTable.AddNode(node, symbolTable.GetSymbol(exp) == Symbol.Decimal ? Symbol.Ok : Symbol.NotOk);
                tempResult += expSymbol == Symbol.Decimal ? "" : IndentedString("expression is not an Decimal\n");                
                break;
            case Symbol.Bool:
                symbolTable.AddNode(node, symbolTable.GetSymbol(exp) == Symbol.Bool ? Symbol.Ok : Symbol.NotOk);
                tempResult += expSymbol == Symbol.Bool ? "" : IndentedString("expression is not an Bool\n");                
                break;
            case Symbol.Char:
                symbolTable.AddNode(node, symbolTable.GetSymbol(exp) == Symbol.Int ? Symbol.Ok : Symbol.NotOk);
                tempResult += expSymbol == Symbol.Int ? "" : IndentedString("expression is not an integer\n");                
                break;
            default:
                if (symbolTable.GetNodeFromId(node.GetId().ToString(), out var b))
                {
                    symbolTable.GetUnit(b, out var typeUnit);
                    symbolTable.GetUnit(exp, out var expUnit);
                    symbolTable.AddNode(node, symbolTable.CompareCustomUnits(typeUnit, expUnit) ? Symbol.Ok : Symbol.NotOk);
                    tempResult += symbolTable.CompareCustomUnits(typeUnit, expUnit) ? "" : IndentedString("expression is not correct unitType\n");
                }
                else
                {
                    symbolTable.AddNode(node, Symbol.NotOk);
                    tempResult += IndentedString("is not correct Type\n");
                }
                break;
        }
        PrintError();
        indent--;
    }

    public override void InAPrefixplusStmt(APrefixplusStmt node)
    {
        locations.Push(IndentedString($"in PrefixplusAssignment {node}\n"));
        indent++;
    }

    public override void OutAPrefixplusStmt(APrefixplusStmt node)
    {
        Symbol? expr = symbolTable.GetSymbol(node.GetId().ToString().Trim());
        switch (expr)
        {
            case Symbol.Decimal:
                symbolTable.AddNode(node,Symbol.Ok);
                break;
            case Symbol.Int:
                symbolTable.AddNode(node,Symbol.Ok); 
                break;
            case Symbol.Char:
                symbolTable.AddNode(node,Symbol.Ok);
                break;
            default:
                symbolTable.AddNode(node,Symbol.NotOk);
                tempResult += IndentedString("Type can only be an int, decimal or char\n");
                break;
        }
        PrintError();
        indent--;
    }

    public override void InAPrefixminusStmt(APrefixminusStmt node)
    {
        locations.Push(IndentedString($"in PrefixminusAssignment {node}\n"));
        indent++;
    }

    public override void OutAPrefixminusStmt(APrefixminusStmt node)
    {
        Symbol? expr = symbolTable.GetSymbol(node.GetId().ToString().Trim());
        switch (expr)
        {
            case Symbol.Decimal:
                symbolTable.AddNode(node,Symbol.Ok);
                break;
            case Symbol.Int:
                symbolTable.AddNode(node,Symbol.Ok); 
                break;
            case Symbol.Char:
                symbolTable.AddNode(node,Symbol.Ok);
                break;
            default:
                symbolTable.AddNode(node,Symbol.NotOk);
                tempResult += IndentedString("Type can only be an int, decimal or char\n");
                break;
        }
        PrintError();
        indent--;
    }

    public override void InASuffixplusStmt(ASuffixplusStmt node)
    {
        locations.Push(IndentedString($"in SuffixplusAssignment {node}\n"));
        indent++;
    }

    public override void OutASuffixplusStmt(ASuffixplusStmt node)
    {
        Symbol? expr = symbolTable.GetSymbol(node.GetId().ToString().Trim());
        switch (expr)
        {
            case Symbol.Decimal:
                symbolTable.AddNode(node,Symbol.Ok);
                break;
            case Symbol.Int:
                symbolTable.AddNode(node,Symbol.Ok); 
                break;
            case Symbol.Char:
                symbolTable.AddNode(node,Symbol.Ok);
                break;
            default:
                symbolTable.AddNode(node,Symbol.NotOk);
                tempResult += IndentedString("Type can only be an int, decimal or \n");
                break;
        }
        PrintError();
        indent--;
    }

    public override void InASuffixminusStmt(ASuffixminusStmt node)
    {
        locations.Push(IndentedString($"in SuffixminusAssignment {node}\n"));
        indent++;
    }

    public override void OutASuffixminusStmt(ASuffixminusStmt node)
    {
        Symbol? expr = symbolTable.GetSymbol(node.GetId().ToString().Trim());
        switch (expr)
        {
            case Symbol.Decimal:
                symbolTable.AddNode(node,Symbol.Ok);
                break;
            case Symbol.Int:
                symbolTable.AddNode(node,Symbol.Ok); 
                break;
            case Symbol.Char:
                symbolTable.AddNode(node,Symbol.Ok);
                break;
            default:
                symbolTable.AddNode(node,Symbol.NotOk);
                tempResult += IndentedString("Type can only be an int, decimal or char\n");
                break;
        }
        PrintError();
        indent--;
    }

    public override void InADeclStmt(ADeclStmt node)
    {
        locations.Push( IndentedString($"in Declaration {node}\n"));
        indent++;
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
                    symbolTable.GetUnit(customType, out var unit);
                    symbolTable.AddIdToNode(node.GetId().ToString(), node);
                    symbolTable.AddNodeToUnit(node, unit);
                    symbolTable.AddNode(node, Symbol.Ok);
                    break; 
                default:
                    symbolTable.AddNode(node, Symbol.NotOk);
                    tempResult += IndentedString("Type not recognized\n");
                    break;
            }
        }
        else
        {
            symbolTable.AddNode(node, Symbol.NotOk);
            tempResult += IndentedString("id is allready declared in this scope\n");

        }
        PrintError();
        indent--;
    }

    public override void InADeclassStmt(ADeclassStmt node)
    {
        locations.Push(IndentedString($"in DeclarationAssignment {node}\n"));
        indent++;
    }

    public override void OutADeclassStmt(ADeclassStmt node)
    { 
        // Assignment have to be typechecked before Decl should add to symbolTable
        if (!symbolTable.IsInCurrentScope(node.GetId()))
        {
            Symbol? exprType = symbolTable.GetSymbol(node.GetExp());
            if (node.GetExp() is AIdExp id)
            {
                exprType = symbolTable.GetSymbol(id.GetId().ToString().Trim());
            }
            PType unit = node.GetType();
            switch (unit)
                {
                    case AIntType a:
                        symbolTable.AddId(node.GetId().ToString(), node, exprType == Symbol.Int ? Symbol.Int : Symbol.NotOk);
                        tempResult += exprType == Symbol.Int ? "" : IndentedString("expression is not an Int\n");
                        break;
                    case ADecimalType b:
                        symbolTable.AddId(node.GetId().ToString(), node, exprType == Symbol.Decimal ? Symbol.Decimal : Symbol.NotOk);
                        tempResult += exprType == Symbol.Decimal ? "" : IndentedString("expression is not an Decimal\n");
                        break;
                    case ABoolType c:
                        symbolTable.AddId(node.GetId().ToString(), node, exprType == Symbol.Bool ? Symbol.Bool : Symbol.NotOk);
                        tempResult += exprType == Symbol.Bool ? "" : IndentedString("expression is not an Bool\n");
                        break;
                    case ACharType d:
                        symbolTable.AddId(node.GetId().ToString(), node, exprType == Symbol.Char ? Symbol.Char : Symbol.NotOk);
                        tempResult += exprType == Symbol.Char ? "" : IndentedString("expression is not an Char\n");
                        break;
                    case AStringType e:
                        symbolTable.AddId(node.GetId().ToString(), node, exprType is Symbol.String or Symbol.Char ? Symbol.String : Symbol.NotOk);
                        tempResult += exprType == Symbol.String ? "" : IndentedString("expression is not a string or char\n");
                        break;
                    case AUnitType customType:
                        //throw new Exception("bruh");
                        if (symbolTable.GetUnit(customType, out var unitType) && 
                            symbolTable.GetUnit(node.GetExp(), out var expType))
                        {
                            if (symbolTable.CompareCustomUnits(unitType, expType))
                            {
                                symbolTable.AddIdToNode(node.GetId().ToString(), node);
                                symbolTable.AddNodeToUnit(node, unitType);
                                symbolTable.AddNode(node, Symbol.Ok); 
                            }
                            else
                            {
                                symbolTable.AddNode(node, Symbol.NotOk);
                                symbolTable.AddNode(node, Symbol.NotOk);
                                tempResult += IndentedString("expression is not correct unitType\n");
                            }
                        }
                        else
                        {
                            symbolTable.AddNode(node, Symbol.NotOk);
                            tempResult += IndentedString("expression have no unitType associated\n");
                        }
                        break; 
                    default:
                        symbolTable.AddNode(node, Symbol.NotOk);
                        tempResult += IndentedString("Wrong declaretype\n");
                        break;
                }
        }
        else
        {
            symbolTable.AddNode(node, Symbol.NotOk);
            tempResult += IndentedString("id is allready declared in this scope\n");
        }
        PrintError();
        indent--;
    }

    public override void InAFunccallStmt(AFunccallStmt node)
    {
        locations.Push( IndentedString($"in Funccall {node}\n"));
        indent++;
    }

    public override void OutAFunccallStmt(AFunccallStmt node)
    {
       // STANDALONE FUNCCAL
       // Returtype er ubetydlig
       symbolTable.GetNodeFromId(node.GetId().ToString().Trim(), out Node? func);
       if (func != null)
       {
           List<PType>? args = symbolTable.GetFunctionArgs(func);
           List<PExp> parameters = node.GetParams().OfType<PExp>().ToList();
           bool matches = true;
           for(int i = 0; i < args.Count(); i++)
           {
               Node? parameter = parameters[i];
               if (parameters[i] is AIdExp id)
               {
                   symbolTable.GetNodeFromId(id.GetId().ToString().Trim(), out parameter);
               }
                       
               switch (args[i])
               {
                   case AIntType:
                       if (symbolTable.GetSymbol(parameter) != Symbol.Int)
                       {
                           matches = false;
                           tempResult += IndentedString($"Parameter {i} is not an integer\n");
                       }
                       break;
                   case ADecimalType:
                       if (symbolTable.GetSymbol(parameter) != Symbol.Decimal)
                       {
                           matches = false;
                           tempResult += IndentedString($"Parameter {i} is not an Decimal\n");
                       }
                       break;
                   case ABoolType:
                       if (symbolTable.GetSymbol(parameter) != Symbol.Bool)
                       {
                           matches = false;
                           tempResult += IndentedString($"Parameter {i} is not an Bool\n");
                       }
                       break;
                   case ACharType:
                       if (symbolTable.GetSymbol(parameter) != Symbol.Char)
                       {
                           matches = false;
                           tempResult += IndentedString($"Parameter {i} is not an Char\n");
                       }
                       break;
                   case AStringType:
                       if (symbolTable.GetSymbol(parameter) != Symbol.String)
                       {
                           matches = false;
                           tempResult += IndentedString($"Parameter {i} is not an String\n");
                       }
                       break;
                   case AUnitType argType:
                   {
                       symbolTable.GetUnit(argType, out var argUnit);
                       symbolTable.GetUnit(parameter, out var paramUnit);
                       if (!symbolTable.CompareCustomUnits(argUnit, paramUnit))
                       {
                           matches = false;
                           symbolTable.AddNodeToUnit(node, argUnit);
                           tempResult += IndentedString($"Parameter {i} is not correct unitType\n");
                       }
                       break; 
                   }
                   default:
                       matches = false;
                       tempResult += IndentedString($"Arg {i} is not correct Type\n");
                       break;
               }
           }
           symbolTable.AddNode(node, matches ? Symbol.Ok : Symbol.NotOk);
       }
       else
       {
           symbolTable.AddNode(node, Symbol.NotOk);
           tempResult += IndentedString($"Function {node.GetId()} is not found\n");
       }
       PrintError();
       indent--;
    }

    public override void InAReturnStmt(AReturnStmt node)
    {
        locations.Push( IndentedString($"in returnstmt {node}\n"));
        indent++;
    }

    public override void OutAReturnStmt(AReturnStmt node)
    {
      //already set before
      Node parent = node.Parent();
      while (parent is not PGlobal)
      {
          parent = parent.Parent();
      }
      Node? returnExp = node.GetExp();
      if (returnExp is AIdExp id)
      {
          symbolTable.GetNodeFromId(id.GetId().ToString().Trim(), out returnExp);
      }
      Symbol? returnSymbol = symbolTable.GetSymbol(returnExp);
      switch (parent)
      {
          case ALoopGlobal:
              // Should not have return in Loop
              symbolTable.AddNode(node, Symbol.NotOk);
              tempResult += IndentedString("Returnstmt should not be in Loop\n");
              break;
          case AProgGlobal:
              // Should not have return in Prog
              symbolTable.AddNode(node, Symbol.NotOk);
              tempResult += IndentedString("Returnstmt should not be in Prog\n");
              break;
          case ATypedGlobal aTypedFunc:
              PType typedType = aTypedFunc.GetType();
              switch (typedType)
              {
                  case AIntType:
                      symbolTable.AddNode(node, returnSymbol == Symbol.Int ? Symbol.Ok : Symbol.NotOk);
                      tempResult += returnSymbol == Symbol.Int ? "" : IndentedString("expression is not an Int\n");
                      break;
                  case ADecimalType:
                      symbolTable.AddNode(node, returnSymbol == Symbol.Decimal ? Symbol.Ok : Symbol.NotOk);
                      tempResult += returnSymbol == Symbol.Decimal ? "" : IndentedString("expression is not an Decimal\n");
                      break;
                  case ABoolType:
                      symbolTable.AddNode(node, returnSymbol == Symbol.Bool ? Symbol.Ok : Symbol.NotOk);
                      tempResult += returnSymbol == Symbol.Bool ? "" : IndentedString("expression is not an Bool\n");
                      break;
                  case ACharType:
                      symbolTable.AddNode(node, returnSymbol == Symbol.Char ? Symbol.Ok : Symbol.NotOk);
                      tempResult += returnSymbol == Symbol.Char ? "" : IndentedString("expression is not an Char\n");
                      break;
                  case AStringType:
                      symbolTable.AddNode(node, returnSymbol == Symbol.String ? Symbol.Ok : Symbol.NotOk);
                      tempResult += returnSymbol == Symbol.String ? "" : IndentedString("expression is not an String\n");
                      break;
                  case AVoidType:
                      symbolTable.AddNode(node, Symbol.NotOk);
                      tempResult += IndentedString("ReturnStmt should not be in a void function\n");

                      break;
                  case AUnitType:
                      symbolTable.GetUnit(typedType, out (List<AUnitdeclGlobal>, List<AUnitdeclGlobal>) funcType);
                      if (symbolTable.GetUnit(returnExp, out (List<AUnitdeclGlobal>, List<AUnitdeclGlobal>) returnType))
                      {
                          symbolTable.AddNode(node, symbolTable.CompareCustomUnits(funcType, returnType) ? Symbol.Ok : Symbol.NotOk);
                          tempResult += symbolTable.CompareCustomUnits(funcType, returnType) ? "" : IndentedString("return is not correct unitType\n");
                      }
                      else
                      {
                          symbolTable.AddNode(node, Symbol.NotOk);
                          tempResult += IndentedString("return have no unitType\n");
                      }
                      break;
                  default:
                      symbolTable.AddNode(node, Symbol.NotOk);
                      break;
              }
              break;
          case AUntypedGlobal aUntypedFunc:
              if (symbolTable.GetUnit(aUntypedFunc, out var func))
              {
                  symbolTable.GetUnit(returnExp, out var expUnit);
                  symbolTable.AddNode(node, symbolTable.CompareCustomUnits(func, expUnit) ? Symbol.Ok : Symbol.NotOk);
                  tempResult += symbolTable.CompareCustomUnits(func, expUnit) ? "" : IndentedString("return is not correct unitType\n");
              } else if (symbolTable.GetReturnFromNode(aUntypedFunc) != null)
              {
                  symbolTable.AddNode(node, symbolTable.GetReturnFromNode(aUntypedFunc) == symbolTable.GetSymbol(returnExp) ? Symbol.Ok : Symbol.NotOk);
                  tempResult += symbolTable.GetReturnFromNode(aUntypedFunc) == symbolTable.GetSymbol(returnExp) ? "" : IndentedString("return is not correct type\n");
              }
              else
              {
                  if (symbolTable.GetUnit(returnExp, out var expUnit))
                  {
                      symbolTable.AddNodeToUnit(aUntypedFunc, expUnit);
                      symbolTable.AddNode(node, Symbol.Ok);
                  } else if (symbolTable.GetSymbol(returnExp) != null)
                  {
                      symbolTable.AddReturnSymbol(aUntypedFunc, symbolTable.GetSymbol(returnExp));
                      symbolTable.AddNode(node, (Symbol)symbolTable.GetSymbol(returnExp));
                  }
              }
              break;
          default:
              symbolTable.AddNode(node, Symbol.NotOk);
              tempResult += IndentedString("returnStmt not in a function\n");
              break;
      }
      PrintError();
      indent--;
    }

    public override void InAIfStmt(AIfStmt node)
    {
        locations.Push(IndentedString($"in IfStmt {node}\n"));
        indent++;
    }

    public override void OutAIfStmt(AIfStmt node)
    {
        Symbol? condExpr = symbolTable.GetSymbol(node.GetExp());
        if (node.GetExp() is AIdExp id)
        {
            condExpr = symbolTable.GetSymbol(id.ToString().Trim());
        }
        symbolTable.AddNode(node, condExpr == Symbol.Bool ? Symbol.Bool: Symbol.NotOk);
        tempResult += condExpr == Symbol.Bool ? "" : IndentedString("Condition is not a boolean\n");
        PrintError();
        indent--;
    }

    public override void InAElseifStmt(AElseifStmt node)
    {
        locations.Push( IndentedString($"in ElseifStmt {node}\n"));
        indent++;
    }

    public override void OutAElseifStmt(AElseifStmt node)
    {
        Symbol? condExpr = symbolTable.GetSymbol(node.GetExp());
        if (node.GetExp() is AIdExp id)
        {
            condExpr = symbolTable.GetSymbol(id.ToString().Trim());
        }
        symbolTable.AddNode(node, condExpr == Symbol.Bool ? Symbol.Bool: Symbol.NotOk);
        tempResult += condExpr == Symbol.Bool ? "" : IndentedString("Condition is not a boolean\n");
        PrintError();
        indent--;
    }

    public override void InAElseStmt(AElseStmt node)
    {
        locations.Push( IndentedString($"in ElseStmt {node}\n"));
        indent++;
    }

    public override void OutAElseStmt(AElseStmt node)
    {
        // Check om der er en if eller else if inden
        symbolTable.AddNode(node, Symbol.Ok);
        PrintError();
        indent--;
    }

    public override void InAForStmt(AForStmt node)
    {
        locations.Push( IndentedString($"in forloop {node}\n"));
        indent++;
    }

    public override void OutAForStmt(AForStmt node)
    {
        Symbol? condExpr = symbolTable.GetSymbol(node.GetCond());
        symbolTable.AddNode(node, condExpr == Symbol.Bool ? Symbol.Bool: Symbol.NotOk);
        tempResult += condExpr == Symbol.Bool ? "" : IndentedString("Condition is not a boolean\n");
        PrintError();
        indent--;
    }

    public override void InAWhileStmt(AWhileStmt node)
    {
        locations.Push( IndentedString($"in while loop {node}\n"));
        indent++;
    }

    public override void OutAWhileStmt(AWhileStmt node)
    {
        Node? exp = node.GetExp();
        if (exp is AIdExp id)
        {
            symbolTable.GetNodeFromId(id.GetId().ToString().Trim(), out exp);
        }
        Symbol? condExpr = symbolTable.GetSymbol(exp);
        symbolTable.AddNode(node, condExpr == Symbol.Bool ? Symbol.Bool: Symbol.NotOk);
        tempResult += condExpr == Symbol.Bool ? "" : IndentedString("Condition is not a boolean\n");
        PrintError();
        indent--;
    }

    public override void InADowhileStmt(ADowhileStmt node)
    {
        locations.Push(IndentedString($"in do-while loop {node}\n"));
        indent++;
    }

    public override void OutADowhileStmt(ADowhileStmt node)
    {
        Node? exp = node.GetExp();
        if (exp is AIdExp id)
        {
            symbolTable.GetNodeFromId(id.GetId().ToString().Trim(), out exp);
        }
        Symbol? condExpr = symbolTable.GetSymbol(exp);
        symbolTable.AddNode(node, condExpr == Symbol.Bool ? Symbol.Bool: Symbol.NotOk);
        tempResult += condExpr == Symbol.Bool ? "" : IndentedString("Condition is not a boolean\n");
        PrintError();
        indent--;
    }

    public override void InADelayStmt(ADelayStmt node)
    {
        locations.Push( IndentedString($"in Delaystmt {node}\n"));
        indent++;
    }

    public override void OutADelayStmt(ADelayStmt node)
    {
        Node? exp = node.GetExp();
        if (exp is AIdExp id)
        {
            symbolTable.GetNodeFromId(id.GetId().ToString().Trim(), out exp);
        }
        // Missing logic 
        
        PrintError();
        indent--;
    }

    public override void InASetpinStmt(ASetpinStmt node)
    {
        locations.Push( IndentedString($"in SetPinStmt {node}\n"));
        indent++;
    }

    public override void OutASetpinStmt(ASetpinStmt node)
    {
        Node? exp = node.GetExp();
        if (exp is AIdExp id)
        {
            symbolTable.GetNodeFromId(id.GetId().ToString().Trim(), out exp);
        }
        // missing logic
        
        PrintError();
        indent--;
    }
}