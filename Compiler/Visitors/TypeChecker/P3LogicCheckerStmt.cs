using Compiler.Visitors.TypeChecker.Utils;
using Moduino.node;

namespace Compiler.Visitors.TypeChecker;


internal partial class P3LogicChecker
{
    public override void InAAssignStmt(AAssignStmt node)
    {
        DefaultIn(node);
    }

    public override void OutAAssignStmt(AAssignStmt node) {
        Symbol? idToType = symbolTable.GetSymbol(node.GetId().ToString().Trim());
        Node exp = node.GetExp();
        
        Symbol? expSymbol = symbolTable.GetSymbol(exp);
        switch (idToType)
        {
            case Symbol.NotOk:
                symbolTable.AddNode(node, Symbol.NotOk);
                
                _logger.ThrowError("Id in assign contains error");
                break;
            case Symbol.Int:
                symbolTable.AddNode(node, expSymbol is Symbol.Int or Symbol.Pin ? Symbol.Int : Symbol.NotOk);
                if (expSymbol != Symbol.Int)
                    _logger.ThrowError("expression is not an Int");
                break;
            case Symbol.Decimal:
                symbolTable.AddNode(node, expSymbol == Symbol.Decimal ? Symbol.Decimal : Symbol.NotOk);
                if (expSymbol != Symbol.Decimal)
                    _logger.ThrowError("expression is not an Decimal");
                break;
            case Symbol.Bool:
                symbolTable.AddNode(node, expSymbol == Symbol.Bool ? Symbol.Bool : Symbol.NotOk);
                if (expSymbol != Symbol.Bool)
                    _logger.ThrowError("expression is not an Bool");
                break;
            case Symbol.Char:
                symbolTable.AddNode(node, expSymbol == Symbol.Char ? Symbol.Char : Symbol.NotOk);
                if (expSymbol != Symbol.Char)
                    _logger.ThrowError("expression is not an Char");
                break;
            case Symbol.String:
                symbolTable.AddNode(node, expSymbol is Symbol.String or Symbol.Char ? Symbol.String : Symbol.NotOk);
                if (expSymbol is not (Symbol.String or Symbol.Char))
                    _logger.ThrowError("expression is not a string or char");
                break;
            case Symbol.Pin:
                symbolTable.AddNode(node, expSymbol is Symbol.Int or Symbol.Pin ? Symbol.Int : Symbol.NotOk);
                if (expSymbol != Symbol.Int)
                    _logger.ThrowError("expression is not a pin or int");
                break;
            default:
                if (symbolTable.GetNodeFromId(node.GetId().ToString().Trim(), out var typeUn) &&
                    symbolTable.GetUnit(typeUn, out var typeUnit) &&
                    symbolTable.GetUnit(exp, out var expUnit))
                {
                    symbolTable.AddNode(node, symbolTable.CompareUnitTypes(typeUnit, expUnit) ? Symbol.Ok : Symbol.NotOk);
                    if (!symbolTable.CompareUnitTypes(typeUnit, expUnit))
                        _logger.ThrowError("expression is not correct unitType\n");
                }
                else
                {
                    symbolTable.AddNode(node, Symbol.NotOk);
                    _logger.ThrowError(idToType == null
                        ? $"Id: {node.GetId().ToString().Trim()} does not exist in this scope\n"
                        : $"Id: {node.GetId().ToString().Trim()} is not correct Type\n");
                }
                break;
        }
        DefaultOut(node);
    }

    public override void InAPlusassignStmt(APlusassignStmt node)
    {
        DefaultIn(node);
    }

    public override void OutAPlusassignStmt(APlusassignStmt node)
    {
        // OVervej om man kan sige += med string, char og bool typer
        Symbol? idType = symbolTable.GetSymbol(node.GetId().ToString().Trim());
        Node exp = node.GetExp();
     
        Symbol? expSymbol = symbolTable.GetSymbol(exp);
        switch (idType)
        {
            case Symbol.Int:
                symbolTable.AddNode(node, symbolTable.GetSymbol(exp) == Symbol.Int ? Symbol.Ok : Symbol.NotOk);
                if (expSymbol != Symbol.Int)
                    _logger.ThrowError("expression is not an Int");
                break;
            case Symbol.Decimal:
                symbolTable.AddNode(node, symbolTable.GetSymbol(exp) == Symbol.Decimal ? Symbol.Ok : Symbol.NotOk);
                if (expSymbol != Symbol.Decimal)
                    _logger.ThrowError("expression is not an Decimal");
                break;
            case Symbol.Bool:
                symbolTable.AddNode(node, symbolTable.GetSymbol(exp) == Symbol.Bool ? Symbol.Ok : Symbol.NotOk);
                if (expSymbol != Symbol.Bool)
                    _logger.ThrowError("expression is not an Bool");
                break;
            case Symbol.Char:
                symbolTable.AddNode(node, symbolTable.GetSymbol(exp) == Symbol.Int ? Symbol.Ok : Symbol.NotOk);
                if (expSymbol != Symbol.Int)
                    _logger.ThrowError("expression is not an integer\n");
                break;
            case Symbol.String:
                symbolTable.AddNode(node, symbolTable.GetSymbol(exp) is Symbol.String or Symbol.Char ? Symbol.String : Symbol.NotOk);
                if (expSymbol is not (Symbol.String or Symbol.Char))
                    _logger.ThrowError("expression is not a string or char\n");
                break;
            case Symbol.Pin:
                symbolTable.AddNode(node, symbolTable.GetSymbol(exp) is Symbol.Int or Symbol.Pin ? Symbol.Int : Symbol.NotOk);
                if (expSymbol != Symbol.Int)
                    _logger.ThrowError("expression is not a pin or int\n");
                break;
            default:
                if (symbolTable.GetNodeFromId(node.GetId().ToString(), out var b))
                {
                    symbolTable.GetUnit(b, out var typeUnit);
                    symbolTable.GetUnit(exp, out var expUnit);
                    symbolTable.AddNode(node, symbolTable.CompareUnitTypes(typeUnit, expUnit) ? Symbol.Ok : Symbol.NotOk);
                    if (!symbolTable.CompareUnitTypes(typeUnit, expUnit))
                        _logger.ThrowError("expression is not correct unitType\n");
                }
                else
                {
                    symbolTable.AddNode(node, Symbol.NotOk);
                    _logger.ThrowError(idType == null
                        ? $"Id: {node.GetId().ToString().Trim()} does not exist in this scope"
                        : $"Id: {node.GetId().ToString().Trim()} is not correct Type");
                }
                break;
        }
        DefaultOut(node);
    }

    public override void InAMinusassignStmt(AMinusassignStmt node)
    {
        DefaultOut(node);
    }

    public override void OutAMinusassignStmt(AMinusassignStmt node)
    {
        Symbol? type = symbolTable.GetSymbol("" + node.GetId().ToString().Trim());
        Node exp = node.GetExp();
       
        Symbol? expSymbol = symbolTable.GetSymbol(exp);
        switch (type)
        {
            case Symbol.Int:
                symbolTable.AddNode(node, symbolTable.GetSymbol(exp) == Symbol.Int ? Symbol.Ok : Symbol.NotOk);
                if (expSymbol != Symbol.Int)
                    _logger.ThrowError("expression is not an Int");
                break;
            case Symbol.Decimal:
                symbolTable.AddNode(node, symbolTable.GetSymbol(exp) == Symbol.Decimal ? Symbol.Ok : Symbol.NotOk);
                if (expSymbol != Symbol.Decimal)
                    _logger.ThrowError("expression is not an Decimal");
                break;
            case Symbol.Bool:
                symbolTable.AddNode(node, symbolTable.GetSymbol(exp) == Symbol.Bool ? Symbol.Ok : Symbol.NotOk);
                if (expSymbol != Symbol.Bool)
                    _logger.ThrowError("expression is not an Bool");
                break;
            case Symbol.Char:
                symbolTable.AddNode(node, symbolTable.GetSymbol(exp) == Symbol.Int ? Symbol.Ok : Symbol.NotOk);
                if (expSymbol != Symbol.Int)
                    _logger.ThrowError("expression is not an integer");
                break;
            case Symbol.Pin:
                symbolTable.AddNode(node, symbolTable.GetSymbol(exp) is Symbol.Int or Symbol.Pin ? Symbol.Int : Symbol.NotOk);
                if (expSymbol != Symbol.Int)
                    _logger.ThrowError("expression is not a pin or int");
                break;
            default:
                if (symbolTable.GetNodeFromId(node.GetId().ToString(), out var b))
                {
                    symbolTable.GetUnit(b, out var typeUnit);
                    symbolTable.GetUnit(exp, out var expUnit);
                    symbolTable.AddNode(node, symbolTable.CompareUnitTypes(typeUnit, expUnit) ? Symbol.Ok : Symbol.NotOk);
                    if (!symbolTable.CompareUnitTypes(typeUnit, expUnit))
                        _logger.ThrowError("expression is not correct unitType");
                }
                else
                {
                    symbolTable.AddNode(node, Symbol.NotOk);
                    _logger.ThrowError(type == null
                        ? $"Id: {node.GetId().ToString().Trim()} does not exist in this scope"
                        : $"Id: {node.GetId().ToString().Trim()} is not correct Type");
                }
                break;
        }
        DefaultOut(node);
    }

    public override void InAPrefixplusStmt(APrefixplusStmt node)
    {
        DefaultIn(node);
    }

    public override void OutAPrefixplusStmt(APrefixplusStmt node)
    {
        Symbol? idType = symbolTable.GetSymbol(node.GetId().ToString().Trim());
        switch (idType)
        {
            case Symbol.Int:
                symbolTable.AddNode(node,Symbol.Int); 
                break;
            case Symbol.Decimal:
                symbolTable.AddNode(node,Symbol.Decimal);
                break;
            case Symbol.Char:
                symbolTable.AddNode(node,Symbol.Char);
                break;
            case Symbol.Pin:
                symbolTable.AddNode(node,Symbol.Int);
                break;
            default:
                symbolTable.AddNode(node,Symbol.NotOk);
                _logger.ThrowError(idType == null
                    ? $"Id: {node.GetId().ToString().Trim()} does not exist in this scope"
                    : "Type can only be an int, decimal, char or pin");
                break;
        }
        DefaultOut(node);
    }

    public override void InAPrefixminusStmt(APrefixminusStmt node)
    {
        DefaultIn(node);
    }

    public override void OutAPrefixminusStmt(APrefixminusStmt node)
    {
        Symbol? idType = symbolTable.GetSymbol(node.GetId().ToString().Trim());
        switch (idType)
        {
            case Symbol.Int:
                symbolTable.AddNode(node,Symbol.Int); 
                break;
            case Symbol.Decimal:
                symbolTable.AddNode(node,Symbol.Decimal);
                break;
            case Symbol.Char:
                symbolTable.AddNode(node,Symbol.Char);
                break;
            case Symbol.Pin:
                symbolTable.AddNode(node,Symbol.Int);
                break;
            default:
                symbolTable.AddNode(node,Symbol.NotOk);
                _logger.ThrowError(idType == null
                    ? $"Id: {node.GetId().ToString().Trim()} does not exist in this scope"
                    : "Type can only be an int, decimal char or pin");
                break;
        }
        DefaultOut(node);
    }

    public override void InASuffixplusStmt(ASuffixplusStmt node)
    {
        DefaultIn(node);
    }

    public override void OutASuffixplusStmt(ASuffixplusStmt node)
    {
        Symbol? idType = symbolTable.GetSymbol(node.GetId().ToString().Trim());
        switch (idType)
        {
            case Symbol.Int:
                symbolTable.AddNode(node,Symbol.Int); 
                break;
            case Symbol.Decimal:
                symbolTable.AddNode(node,Symbol.Decimal);
                break;
            case Symbol.Char:
                symbolTable.AddNode(node,Symbol.Char);
                break;
            case Symbol.Pin:
                symbolTable.AddNode(node,Symbol.Int);
                break;
            default:
                symbolTable.AddNode(node,Symbol.NotOk);
                if (idType == null)
                {
                    _logger.ThrowError($"Id: {node.GetId().ToString().Trim()} does not exist in this scope");
                }
                else
                {
                    _logger.ThrowError("Type can only be an int, decimal char or pin");                
                }
                break;
        }
        DefaultOut(node);
    }

    public override void InASuffixminusStmt(ASuffixminusStmt node)
    {
        DefaultIn(node);
    }

    public override void OutASuffixminusStmt(ASuffixminusStmt node)
    {
        Symbol? idType = symbolTable.GetSymbol(node.GetId().ToString().Trim());
        switch (idType)
        {
            case Symbol.Int:
                symbolTable.AddNode(node,Symbol.Int); 
                break;
            case Symbol.Decimal:
                symbolTable.AddNode(node,Symbol.Decimal);
                break;
            case Symbol.Char:
                symbolTable.AddNode(node,Symbol.Char);
                break;
            case Symbol.Pin:
                symbolTable.AddNode(node,Symbol.Int);
                break;
            default:
                symbolTable.AddNode(node,Symbol.NotOk);
                _logger.ThrowError(idType == null
                    ? $"Id: {node.GetId().ToString().Trim()} does not exist in this scope"
                    : "Type can only be an int, decimal char or pin");
                break;
        }
        DefaultOut(node);
    }

    public override void CaseADeclStmt(ADeclStmt node)
    {
        if (symbolTable._parent == null)
        {
           
        }
        else
        {
            InADeclStmt(node);
            if(node.GetType() != null)
            {
                node.GetType().Apply(this);
            }
            if(node.GetId() != null)
            {
                node.GetId().Apply(this);
            }
            OutADeclStmt(node);
        }
    }

    public override void InADeclStmt(ADeclStmt node)
    {
        DefaultIn(node);
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
                case APinType:
                    symbolTable.AddId(node.GetId().ToString(), node, Symbol.Pin);
                    break;
                case AUnitType customType:
                    symbolTable.GetUnit(customType, out var unit);
                    symbolTable.AddIdToNode(node.GetId().ToString(), node);
                    symbolTable.AddNodeToUnit(node, unit);
                    symbolTable.AddNode(node, Symbol.Ok);
                    break; 
                default:
                    symbolTable.AddNode(node, Symbol.NotOk);
                    _logger.ThrowError("Type not recognized");
                    break;
            }
        }
        else
        {
            symbolTable.AddNode(node, Symbol.NotOk);
            _logger.ThrowError("id is allready declared in this scope");

        }
        DefaultOut(node);
    }
    public override void CaseADeclassStmt(ADeclassStmt node)
    {
        if (symbolTable._parent == null)
        {
           
        }
        else
        {
            InADeclassStmt(node);
            if (node.GetExp() != null)
            {
                node.GetExp().Apply(this);
            }

            if (node.GetId() != null)
            {
                node.GetId().Apply(this);
            }

            if (node.GetType() != null)
            {
                node.GetType().Apply(this);
            }

            OutADeclassStmt(node);
        }
    }
    public override void InADeclassStmt(ADeclassStmt node)
    {
        DefaultIn(node);
    }
    public override void OutADeclassStmt(ADeclassStmt node)
    {
        // Assignment have to be typechecked before Decl should add to symbolTable
        if (!symbolTable.IsInCurrentScope(node.GetId()))
        {
            Node expression = node.GetExp();
            Symbol? exprType = symbolTable.GetSymbol(expression);
            PType unit = node.GetType();
            switch (unit)
                {
                    case AIntType:
                        symbolTable.AddId(node.GetId().ToString(), node, exprType == Symbol.Int ? Symbol.Int : Symbol.NotOk);
                        if (exprType != Symbol.Int)
                            _logger.ThrowError("expression is not an Int");
                        break;
                    case ADecimalType:
                        symbolTable.AddId(node.GetId().ToString(), node, exprType == Symbol.Decimal ? Symbol.Decimal : Symbol.NotOk);
                        if (exprType != Symbol.Decimal)
                            _logger.ThrowError("expression is not an Decimal");
                        break;
                    case ABoolType:
                        symbolTable.AddId(node.GetId().ToString(), node, exprType == Symbol.Bool ? Symbol.Bool : Symbol.NotOk);
                        if (exprType != Symbol.Bool)
                            _logger.ThrowError("expression is not an Bool");
                        break;
                    case ACharType:
                        symbolTable.AddId(node.GetId().ToString(), node, exprType == Symbol.Char ? Symbol.Char : Symbol.NotOk);
                        if (exprType != Symbol.Char)
                            _logger.ThrowError("expression is not an Char");
                        break;
                    case AStringType:
                        symbolTable.AddId(node.GetId().ToString(), node, exprType is Symbol.String or Symbol.Char ? Symbol.String : Symbol.NotOk);
                        if (exprType != Symbol.String)
                            _logger.ThrowError("expression is not a string or char");
                        break;
                    case APinType:
                        symbolTable.AddId(node.GetId().ToString(), node, exprType is Symbol.Int or Symbol.Pin ? Symbol.Int : Symbol.NotOk);
                        if (exprType is not (Symbol.Int or Symbol.Pin))
                            _logger.ThrowError("expression is not an an pin or integer");
                        break;
                    case AUnitType customType:
                        if (symbolTable.GetUnit(customType, out var unitType) && 
                            symbolTable.GetUnit(expression, out var expType))
                        {
                            if (symbolTable.CompareUnitTypes(unitType, expType))
                            {
                                symbolTable.AddIdToNode(node.GetId().Text, node);
                                symbolTable.AddNodeToUnit(node, unitType);
                                symbolTable.AddNode(node, Symbol.Ok); 
                            }
                            else
                            {
                                symbolTable.AddIdToNode(node.GetId().ToString(), node);
                                symbolTable.AddNode(node, Symbol.NotOk);
                                _logger.ThrowError("expression is not correct unitType");
                            }
                        }
                        else
                        {
                            symbolTable.AddIdToNode(node.GetId().ToString(), node);
                            symbolTable.AddNode(node, Symbol.NotOk);
                            _logger.ThrowError("expression have no unitType associated");
                        }
                        break; 
                    default:
                        symbolTable.AddIdToNode(node.GetId().ToString(), node);
                        symbolTable.AddNode(node, Symbol.NotOk);
                        _logger.ThrowError("Wrong declaretype");
                        break;
                }
        }
        else
        {
            symbolTable.AddNode(node, Symbol.NotOk);
            _logger.ThrowError("id is allready declared in this scope");
        }
        DefaultOut(node);
    }

    public override void InAFunccallStmt(AFunccallStmt node)
    {
        DefaultIn(node);
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
                   
               switch (args[i])
               {
                   case AIntType:
                       if (symbolTable.GetSymbol(parameter) != Symbol.Int)
                       {
                           matches = false;
                           _logger.ThrowError($"Parameter {i} is not an integer\n");
                       }
                       break;
                   case ADecimalType:
                       if (symbolTable.GetSymbol(parameter) != Symbol.Decimal)
                       {
                           matches = false;
                           _logger.ThrowError($"Parameter {i} is not an Decimal\n");
                       }
                       break;
                   case ABoolType:
                       if (symbolTable.GetSymbol(parameter) != Symbol.Bool)
                       {
                           matches = false;
                           _logger.ThrowError($"Parameter {i} is not an Bool\n");
                       }
                       break;
                   case ACharType:
                       if (symbolTable.GetSymbol(parameter) != Symbol.Char)
                       {
                           matches = false;
                           _logger.ThrowError($"Parameter {i} is not an Char\n");
                       }
                       break;
                   case AStringType:
                       if (symbolTable.GetSymbol(parameter) != Symbol.String)
                       {
                           matches = false;
                           _logger.ThrowError($"Parameter {i} is not an String\n");
                       }
                       break;
                   case APinType:
                       if (symbolTable.GetSymbol(parameter) != Symbol.Pin)
                       {
                           matches = false;
                           _logger.ThrowError($"Parameter {i} is not a pin\n");
                       }
                       break;
                   case AUnitType argType:
                   {
                       symbolTable.GetUnit(argType, out var argUnit);
                       symbolTable.GetUnit(parameter, out var paramUnit);
                       if (!symbolTable.CompareUnitTypes(argUnit, paramUnit))
                       {
                           matches = false;
                           symbolTable.AddNodeToUnit(node, argUnit);
                           _logger.ThrowError($"Parameter {i} is not correct unitType\n");
                       }
                       break; 
                   }
                   default:
                       matches = false;
                       _logger.ThrowError($"Arg {i} is not correct Type\n");
                       break;
               }
           }
           symbolTable.AddNode(node, matches ? Symbol.Ok : Symbol.NotOk);
       }
       else
       {
           symbolTable.AddNode(node, Symbol.NotOk);
           _logger.ThrowError($"Function {node.GetId()} is not found\n");
       }
       DefaultOut(node);
    }

    public override void InAReturnStmt(AReturnStmt node)
    {
        DefaultIn(node);
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
      Symbol? returnSymbol = symbolTable.GetSymbol(returnExp);
      switch (parent)
      {
          case ALoopGlobal:
              // Should not have return in Loop
              symbolTable.AddNode(node, Symbol.NotOk);
              _logger.ThrowError("Returnstmt should not be in Loop");
              break;
          case AProgGlobal:
              // Should not have return in Prog
              symbolTable.AddNode(node, Symbol.NotOk);
              _logger.ThrowError("Returnstmt should not be in Prog");
              break;
          case ATypedGlobal aTypedFunc:
              PType typedType = aTypedFunc.GetType();
              switch (typedType)
              {
                  case AIntType:
                      symbolTable.AddNode(node, returnSymbol == Symbol.Int ? Symbol.Int : Symbol.NotOk);
                      if (returnSymbol != Symbol.Int)
                          _logger.ThrowError("expression is not an Int");
                      break;
                  case ADecimalType:
                      symbolTable.AddNode(node, returnSymbol == Symbol.Decimal ? Symbol.Decimal : Symbol.NotOk);
                      if (returnSymbol != Symbol.Decimal)
                          _logger.ThrowError("expression is not an Decimal");
                      break;
                  case ABoolType:
                      symbolTable.AddNode(node, returnSymbol == Symbol.Bool ? Symbol.Bool : Symbol.NotOk);
                      if (returnSymbol != Symbol.Bool)
                          _logger.ThrowError("expression is not an Bool");
                      break;
                  case ACharType:
                      symbolTable.AddNode(node, returnSymbol == Symbol.Char ? Symbol.Char : Symbol.NotOk);
                      if (returnSymbol != Symbol.Char)
                          _logger.ThrowError("expression is not an Char");
                      break;
                  case AStringType:
                      symbolTable.AddNode(node, returnSymbol == Symbol.String ? Symbol.String : Symbol.NotOk);
                      if (returnSymbol != Symbol.String)
                          _logger.ThrowError("expression is not an String");
                      break;
                  case APinType:
                      symbolTable.AddNode(node, returnSymbol == Symbol.Pin ? Symbol.Pin : Symbol.NotOk);
                      if (returnSymbol != Symbol.Pin)
                          _logger.ThrowError("expression is not a pin");
                      break;
                  case AVoidType:
                      symbolTable.AddNode(node, Symbol.NotOk);
                      _logger.ThrowError("ReturnStmt should not be in a void function");
                      break;
                  case AUnitType:
                      symbolTable.GetUnit(typedType, out (SortedList<string, AUnitdeclGlobal>, SortedList<string, AUnitdeclGlobal>) funcType);
                      if (symbolTable.GetUnit(returnExp, out (SortedList<string, AUnitdeclGlobal>, SortedList<string, AUnitdeclGlobal>) returnType))
                      {
                          symbolTable.AddNode(node, symbolTable.CompareUnitTypes(funcType, returnType) ? Symbol.Ok : Symbol.NotOk);
                          if (!symbolTable.CompareUnitTypes(funcType, returnType))
                              _logger.ThrowError("return is not correct unitType");
                      }
                      else
                      {
                          symbolTable.AddNode(node, Symbol.NotOk);
                          _logger.ThrowError("return have no unitType");
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
                  symbolTable.AddNode(node, symbolTable.CompareUnitTypes(func, expUnit) ? Symbol.Ok : Symbol.NotOk);
                  if (!symbolTable.CompareUnitTypes(func, expUnit))
                      _logger.ThrowError("return is not correct unitType");
              } 
              else if (symbolTable.GetSymbol(aUntypedFunc) != null && symbolTable.GetSymbol(aUntypedFunc) != Symbol.NotOk)
              {
                  Symbol? symbol = symbolTable.GetSymbol(returnExp);
                  if (symbol != null)
                  {
                      Symbol nonNullSymbol = (Symbol)symbol;
                      symbolTable.AddNode(node, symbolTable.GetSymbol(aUntypedFunc) == nonNullSymbol ? nonNullSymbol : Symbol.NotOk);
                      if (symbolTable.GetSymbol(aUntypedFunc) != nonNullSymbol)
                          _logger.ThrowError("return is not correct type");
                  }
              }
              else
              {
                  if (symbolTable.GetUnit(returnExp, out var expUnit))
                  {
                      symbolTable.AddNodeToUnit(node, expUnit);
                      symbolTable.AddNode(node, Symbol.Ok);
                  } else if (symbolTable.GetSymbol(returnExp) != null)
                  {
                      symbolTable.AddNode(node, (Symbol)symbolTable.GetSymbol(returnExp));
                  }
              }
              break;
          default:
              symbolTable.AddNode(node, Symbol.NotOk);
              _logger.ThrowError("returnStmt not in a function");
              break;
      }
      DefaultOut(node);
    }

    public override void InAIfStmt(AIfStmt node)
    {
        DefaultIn(node);
        symbolTable = symbolTable.EnterScope();
    }

    public override void OutAIfStmt(AIfStmt node)
    {
        Symbol? condExpr = symbolTable.GetSymbol(node.GetExp());
        symbolTable = symbolTable.ExitScope();
        symbolTable.AddNode(node, condExpr == Symbol.Bool ? Symbol.Bool: Symbol.NotOk);
        if (condExpr == null)
        {
            _logger.ThrowError("The id does not exist in this scope");
        }
        else
        {
            if (condExpr != Symbol.Bool)
                _logger.ThrowError("Condition is not a boolean");
        }
        DefaultOut(node);
    }

    public override void InAElseifStmt(AElseifStmt node)
    {
        DefaultOut(node);
        symbolTable = symbolTable.EnterScope();
    }

    public override void OutAElseifStmt(AElseifStmt node)
    {
        Symbol? condExpr = symbolTable.GetSymbol(node.GetExp());
        symbolTable = symbolTable.ExitScope();
        symbolTable.AddNode(node, condExpr == Symbol.Bool ? Symbol.Bool: Symbol.NotOk);
        if (condExpr == null)
        {
            _logger.ThrowError("The id does not exist in this scope");
        }
        else
        {
            if (condExpr != Symbol.Bool)
                _logger.ThrowError("Condition is not a boolean\n");
        }
        DefaultOut(node);
    }

    public override void InAElseStmt(AElseStmt node)
    {
        DefaultIn(node);
        symbolTable = symbolTable.EnterScope();
    }

    public override void OutAElseStmt(AElseStmt node)
    {
        // Check om der er en if eller else if inden
        symbolTable = symbolTable.ExitScope();
        symbolTable.AddNode(node, Symbol.Ok);
        DefaultOut(node);
    }

    public override void InAForStmt(AForStmt node)
    {
        DefaultIn(node);
        symbolTable = symbolTable.EnterScope();
    }

    public override void OutAForStmt(AForStmt node)
    {
        Symbol? condExpr = symbolTable.GetSymbol(node.GetCond());
        symbolTable = symbolTable.ExitScope();
        symbolTable.AddNode(node, condExpr == Symbol.Bool ? Symbol.Bool: Symbol.NotOk);
        if (condExpr == null)
        {
            _logger.ThrowError("The id does not exist in this scope");
        }
        else
        {
            if (condExpr != Symbol.Bool)
                _logger.ThrowError("Condition is not a boolean");
        }
        DefaultOut(node);
    }

    public override void InAWhileStmt(AWhileStmt node)
    {
        DefaultIn(node);
        symbolTable = symbolTable.EnterScope();
    }

    public override void OutAWhileStmt(AWhileStmt node)
    {
        Node? exp = node.GetExp();
        Symbol? condExpr = symbolTable.GetSymbol(exp);
        symbolTable = symbolTable.ExitScope();
        symbolTable.AddNode(node, condExpr == Symbol.Bool ? Symbol.Bool: Symbol.NotOk);
        if (condExpr == null)
        {
            _logger.ThrowError("The id does not exist in this scope");
        }
        else
        {
            if (condExpr != Symbol.Bool)
                _logger.ThrowError("Condition is not a boolean\n");
        }
        DefaultOut(node);
      
    }

    public override void InADowhileStmt(ADowhileStmt node)
    {
        DefaultIn(node);
        symbolTable = symbolTable.EnterScope();
    }

    public override void OutADowhileStmt(ADowhileStmt node)
    {
        Node? exp = node.GetExp();
        Symbol? condExpr = symbolTable.GetSymbol(exp);
        symbolTable = symbolTable.ExitScope();
        symbolTable.AddNode(node, condExpr == Symbol.Bool ? Symbol.Bool: Symbol.NotOk);
        if (condExpr == null)
        {
            _logger.ThrowError("The id does not exist in this scope");
        }
        else
        {
            if (condExpr != Symbol.Bool)
                _logger.ThrowError("Condition is not a boolean");
        }
        DefaultOut(node);
    }

    public override void InADelayStmt(ADelayStmt node)
    {
        DefaultIn(node);
    }

    public override void OutADelayStmt(ADelayStmt node)
    {
        Symbol? exp = symbolTable.GetSymbol(node.GetExp());
        
        if (exp == Symbol.Int)
        {
            symbolTable.AddNode(node, Symbol.Ok);
        }
        else
        {
            symbolTable.AddNode(node, Symbol.NotOk);
            if (exp == null)
            {
                _logger.ThrowError("The id does not exist in this scope");
            }
            else
            {
                _logger.ThrowError("Delay statement needs an integer value");          
            }
            
        }
        DefaultOut(node);
    }

    public override void InASetpinStmt(ASetpinStmt node)
    {
        DefaultIn(node);
    }

    public override void OutASetpinStmt(ASetpinStmt node)
    {
        Symbol? exp = symbolTable.GetSymbol(node.GetExp());
        
        if (exp is Symbol.Int or Symbol.Pin)
        {
            symbolTable.AddNode(node, Symbol.Ok);
        }
        else
        {
            symbolTable.AddNode(node, Symbol.NotOk);
            _logger.ThrowError(exp == null
                ? "The id does not exist in this scope"
                : "Setpin statement is not of int or pin type");
        }
        DefaultOut(node);
    }
}