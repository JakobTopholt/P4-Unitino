using Compiler.Visitors.TypeChecker.Utils;
using Moduino.node;

namespace Compiler.Visitors.TypeChecker;

internal partial class P3LogicChecker
{
    
    public override void OutAParenthesisExp(AParenthesisExp node)
    {
        Node exp = node.GetExp();
        
        Symbol? type = symbolTable.GetSymbol(exp);
        switch (type)
        {
            case Symbol.Int:
                symbolTable.AddNode(node, Symbol.Int);
                break;
            case Symbol.Decimal:
                symbolTable.AddNode(node, Symbol.Decimal);
                break;
            case Symbol.Bool:
                symbolTable.AddNode(node, Symbol.Bool);
                break;
            case Symbol.Char:
                symbolTable.AddNode(node, Symbol.Char);
                break;
            case Symbol.String:
                symbolTable.AddNode(node, Symbol.String);
                break;
            case Symbol.Func:
                symbolTable.AddNode(node, Symbol.Func);
                break;
            case Symbol.Pin:
                symbolTable.AddNode(node, Symbol.Pin);
                break;
            case Symbol.NotOk:
                symbolTable.AddNode(node, Symbol.NotOk);
                break;
            default:
                symbolTable.GetUnit(exp, out var unit);
                symbolTable.AddNodeToUnit(node, unit);
                symbolTable.AddNode(node, Symbol.Ok);
                break;
        }
    }
    
    public override void OutADecimalExp(ADecimalExp node) => symbolTable.AddNode(node, Symbol.Decimal);
    public override void OutANumberExp(ANumberExp node) => symbolTable.AddNode(node, Symbol.Int);
    public override void OutABooleanExp(ABooleanExp node) => symbolTable.AddNode(node, Symbol.Bool);
    public override void OutATrueBoolean(ATrueBoolean node) => symbolTable.AddNode(node, Symbol.Bool);
    public override void OutAFalseBoolean(AFalseBoolean node) => symbolTable.AddNode(node, Symbol.Bool);
    public override void OutAStringExp(AStringExp node) => symbolTable.AddNode(node, Symbol.String);
    public override void OutACharExp(ACharExp node) => symbolTable.AddNode(node, Symbol.Char);

    public override void InAValueExp(AValueExp node)
    {
        DefaultIn(node);
    }

    public override void OutAValueExp(AValueExp node)
    {
        if (symbolTable.currentScope == 1)
        {
            symbolTable.AddNode(node, Symbol.NotOk);
            _logger.ThrowError("Value expression is not legal in globalscope");
        }
        else
        {
            symbolTable.AddNode(node, Symbol.Decimal); 
        }
        /*
        Node? parent = node.Parent();
            while (parent is not AUnitdeclGlobal)
            {
                parent = parent.Parent();
            }
            if (parent is AUnitdeclGlobal)
            { 
                symbolTable.AddNode(node, Symbol.Decimal); 
            }
            else
            {
                symbolTable.AddNode(node, Symbol.NotOk);
                tempResult += IndentedString("Value expressions have to be in a unitdeclaration");
            } 
        }*/
        DefaultOut(node);
        
    }

    public override void InAFunccallExp(AFunccallExp node)
    {
        DefaultIn(node);
    }

    public override void OutAFunccallExp(AFunccallExp node)
    {
        if (symbolTable._parent == null)
        {
            symbolTable.AddNode(node, Symbol.NotOk);
            _logger.ThrowError("funccall expression is not legal in globalscope");
        }
        else
        {
            symbolTable.GetNodeFromId(node.GetId().ToString(), out Node xxx);
            List<PType>? args = symbolTable.GetFunctionArgs(xxx);
            List<PExp>? parameters = node.GetExp().OfType<PExp>().ToList();

            int argAmount = args.Count;
            int paramAmount = parameters.Count;
            if (argAmount != paramAmount)
            {
                symbolTable.AddNode(node, Symbol.NotOk);
                _logger.ThrowError("Not same amount of Arguments and Parameters");
            }
            else
            {
                bool matches = true;
                for (int i = 0; i < args.Count; i++)
                {
                    Symbol? paramSymbol;
                    Node? returnUnit;
                    if (parameters[i] is AIdExp x)
                    {
                        paramSymbol = symbolTable.GetSymbol(x.GetId().ToString().Trim());
                        symbolTable.GetNodeFromId(x.GetId().ToString().Trim(), out returnUnit);
                    }
                    else
                    {
                        paramSymbol = symbolTable.GetSymbol(parameters[i]);
                        returnUnit = parameters[i];
                    }

                    switch (args[i])
                    {
                        case AIntType:
                            if (paramSymbol != Symbol.Int)
                            {
                                matches = false;
                                _logger.ThrowError("Parameter should be of type Int");
                            }

                            break;
                        case ADecimalType:
                            if (paramSymbol != Symbol.Decimal)
                            {
                                matches = false;
                                _logger.ThrowError("Parameter should be of type Decimal");
                            }

                            break;
                        case ABoolType:
                            if (paramSymbol != Symbol.Bool)
                            {
                                matches = false;
                                _logger.ThrowError("Parameter should be of type Bool");
                            }

                            break;
                        case ACharType:
                            if (paramSymbol != Symbol.Char)
                            {
                                matches = false;
                                _logger.ThrowError("Parameter should be of type Char");
                            }

                            break;
                        case AStringType:
                            if (paramSymbol != Symbol.String)
                            {
                                matches = false;
                                _logger.ThrowError("Parameter should be of type String");
                            }

                            break;
                        case APinType:
                            if (paramSymbol != Symbol.Pin)
                            {
                                matches = false;
                                _logger.ThrowError("Parameter should be of type Pin");
                            }

                            break;
                        case AUnitType argType:
                            if (symbolTable.GetUnit(argType, out var argUnit) &&
                                symbolTable.GetUnit(returnUnit, out var paramUnit) &&
                                !symbolTable.CompareUnitTypes(argUnit, paramUnit))
                            {
                                matches = false;
                                _logger.ThrowError($"Parameter nr{i}: is not same unitType as Argument");
                                symbolTable.AddNodeToUnit(node, argUnit);
                            }

                            break;
                        default:
                            matches = false;
                            break;
                    }
                }

                if (matches)
                {
                    symbolTable.GetNodeFromId(node.GetId().ToString(), out Node result);
                    //symbolTable = symbolTable.ResetScope();
                    var symbol = symbolTable.GetSymbol(result);
                    switch (symbol)
                    {
                        case Symbol.Int:
                            symbolTable.AddNode(node, Symbol.Int);
                            break;
                        case Symbol.Decimal:
                            symbolTable.AddNode(node, Symbol.Decimal);
                            break;
                        case Symbol.Bool:
                            symbolTable.AddNode(node, Symbol.Bool);
                            break;
                        case Symbol.Char:
                            symbolTable.AddNode(node, Symbol.Char);
                            break;
                        case Symbol.String:
                            symbolTable.AddNode(node, Symbol.String);
                            break;
                        case Symbol.Pin:
                            symbolTable.AddNode(node, Symbol.Pin);
                            break;
                        default:
                            if (symbolTable.GetUnit(result, out var unit))
                            {
                                symbolTable.AddNodeToUnit(node, unit);
                                symbolTable.AddNode(node, Symbol.Ok);
                            }
                            else
                            {
                                symbolTable.AddNode(node, Symbol.NotOk);
                            }

                            break;
                    }
                }
                else
                {
                    symbolTable.AddNode(node, Symbol.NotOk);
                    // Allready added error message
                }
            }
        }
        DefaultOut(node);
    }

    public override void InAIdExp(AIdExp node)
    {
        DefaultIn(node);
    }

    public override void OutAIdExp(AIdExp node)
    {
        switch (symbolTable.GetSymbol(node.GetId().ToString().Trim()))
        {
            case Symbol.Int:
                symbolTable.AddNode(node, Symbol.Int);
                break;
            case Symbol.Decimal:
                symbolTable.AddNode(node, Symbol.Decimal);
                break;
            case Symbol.Bool:
                symbolTable.AddNode(node, Symbol.Bool);
                break;
            case Symbol.Char:
                symbolTable.AddNode(node, Symbol.Char);
                break;
            case Symbol.String:
                symbolTable.AddNode(node, Symbol.String);
                break;
            case Symbol.Pin:
                symbolTable.AddNode(node, Symbol.Pin);
                break;
            default:
                if (symbolTable.GetUnit(node.GetId().ToString().Trim(), out var unit))
                {
                    symbolTable.AddNodeToUnit(node, unit);
                    symbolTable.AddNode(node, Symbol.Ok);
                }
                else
                {
                    symbolTable.AddNode(node, Symbol.NotOk);
                    _logger.ThrowError($"{node.GetId()} is not a valid id (no value associated with it)");
                }
                break;
        }
        DefaultOut(node);
    }

    public override void InAReadpinExp(AReadpinExp node)
    {
        DefaultIn(node);
    }

    public override void OutAReadpinExp(AReadpinExp node)
    {
        if (symbolTable._parent == null)
        {
            symbolTable.AddNode(node, Symbol.NotOk);
            _logger.ThrowError("A readpin expression is not legal in globalscope");
        }
        else
        {
            Symbol? readpinType = symbolTable.GetSymbol(node.GetExp());
            if (readpinType is Symbol.Pin or Symbol.Int)
            {
                symbolTable.AddNode(node, Symbol.Int);
            }
            else
            {
                symbolTable.AddNode(node, Symbol.NotOk);
                if (readpinType == null)
                {
                    _logger.ThrowError("The id does not exist in this scope");
                }
                else
                {
                    _logger.ThrowError("Readpin expression is not Pin");          
                }
            }
        }
        DefaultOut(node);
    }

    public override void InAUnitdecimalExp(AUnitdecimalExp node)
    {
        DefaultIn(node);
    }

    public override void OutAUnitdecimalExp(AUnitdecimalExp node)
    {
        // A single unitnumber eg. 50ms
        AUnitdeclGlobal unitType = symbolTable.GetUnitdeclFromId(node.GetId().ToString().Trim());
        if (unitType != null)
        {
            // Create a new unit tuple and add the unitnumber as a lone numerator
            SortedList<string, AUnitdeclGlobal> nums = new SortedList<string, AUnitdeclGlobal>();
            nums.Add(unitType.GetId().ToString().Trim(), unitType);
            SortedList<string, AUnitdeclGlobal> dens = new SortedList<string, AUnitdeclGlobal>();
            var unit = (nums, dens);
            symbolTable.AddNodeToUnit(node, unit);
            symbolTable.AddNode(node, Symbol.Ok);
        }
        else
        {
            // Id is not a valid subunit
            symbolTable.AddNode(node, Symbol.NotOk);
            _logger.ThrowError($"{node.GetId()} does not exist");
        }
        DefaultOut(node);
    }

    public override void InAUnitnumberExp(AUnitnumberExp node)
    {
        DefaultIn(node);
    }

    public override void OutAUnitnumberExp(AUnitnumberExp node)
    {
        // A single unitnumber eg. 50ms
        AUnitdeclGlobal unitType = symbolTable.GetUnitdeclFromId(node.GetId().ToString().Trim());
        if (unitType != null)
        {
            // Create a new unit tuple and add the unitnumber as a lone numerator
            SortedList<string, AUnitdeclGlobal> nums = new SortedList<string, AUnitdeclGlobal>();
            nums.Add(unitType.GetId().ToString().Trim(), unitType);
            SortedList<string, AUnitdeclGlobal> dens = new SortedList<string, AUnitdeclGlobal>();
            var unit = (nums, dens);

            // Map node to the unit
            symbolTable.AddNodeToUnit(node, unit);
            symbolTable.AddNode(node, Symbol.Ok);
        }
        else
        {
            // Id is not a valid subunit
            symbolTable.AddNode(node, Symbol.NotOk);
            _logger.ThrowError($"{node.GetId()} does not exist");
        }
        DefaultOut(node);
    }

    public override void InADivideExp(ADivideExp node)
    {
        DefaultIn(node);
    }

    public override void OutADivideExp(ADivideExp node)
    {
        Node leftExpr = node.GetL();
        Node rightExpr = node.GetR();
        
        Symbol? leftSymbol = symbolTable.GetSymbol(leftExpr);
        Symbol? rightSymbol = symbolTable.GetSymbol(rightExpr);
        /* if (leftExpr is ANumberExp or ADecimalExp && rightExpr is AIdExp id)
        {
            AUnitdeclGlobal? unitDecl = symbolTable.GetUnitdeclFromId(id.GetId().ToString().Trim());
            if (unitDecl is null || !symbolTable.GetUnit(unitDecl, out var tuple)) 
                return;
            symbolTable.AddNodeToUnit(node, tuple);
            symbolTable.AddNode(node, Symbol.Ok);
        }
        else */
        {
          switch (leftSymbol)
            {
            case Symbol.Int or Symbol.Pin when rightSymbol is Symbol.Int or Symbol.Pin:
                symbolTable.AddNode(node, Symbol.Int);
                break;
            case Symbol.Decimal when rightSymbol is Symbol.Decimal or Symbol.Int:
                symbolTable.AddNode(node, Symbol.Decimal);
                break;
            case Symbol.Decimal or Symbol.Int when rightSymbol is Symbol.Decimal:
                symbolTable.AddNode(node, Symbol.Decimal);
                break;
            default:
                bool leftContainsUnit = symbolTable.NodeToUnit.ContainsKey(leftExpr);
                bool rightContainsUnit = symbolTable.NodeToUnit.ContainsKey(rightExpr);
                if (symbolTable.GetUnit(leftExpr, out (SortedList<string, AUnitdeclGlobal> num, SortedList<string, AUnitdeclGlobal> den) left) 
                      && symbolTable.GetUnit(rightExpr, out (SortedList<string, AUnitdeclGlobal> num, SortedList<string, AUnitdeclGlobal> den) right))
                {
                    SortedList<string, AUnitdeclGlobal> unitLeftNums = left.num;
                    SortedList<string, AUnitdeclGlobal> unitLeftDens = left.den;
                    SortedList<string, AUnitdeclGlobal> unitRightNums = right.num;
                    SortedList<string, AUnitdeclGlobal> unitRightDens = right.den;
// 0
                    List<AUnitdeclGlobal> numOverlap = unitLeftNums.Values.Intersect(unitRightNums.Values).ToList();
                    List<AUnitdeclGlobal> denOverlap = unitLeftDens.Values.Intersect(unitRightDens.Values).ToList();

                    List<AUnitdeclGlobal> numerators = unitLeftNums.Values.Where(x => !numOverlap.Contains(x)).Concat(unitRightDens.Values.Where(x => !denOverlap.Contains(x))).ToList();
                    List<AUnitdeclGlobal> denominators = unitRightNums.Values.Where(x => !numOverlap.Contains(x)).Concat(unitLeftDens.Values.Where(x => !denOverlap.Contains(x))).ToList();
                    if (numerators.Count == 0 && denominators.Count == 0)
                    {
                        symbolTable.AddNode(node, Symbol.Decimal);
                    }
                    else
                    {
                        SortedList<string, AUnitdeclGlobal> newNums = new SortedList<string, AUnitdeclGlobal>(new DuplicateKeyComparer<string>());
                        SortedList<string, AUnitdeclGlobal> newDens = new SortedList<string, AUnitdeclGlobal>(new DuplicateKeyComparer<string>());
                        
                        foreach (AUnitdeclGlobal num in numerators)
                        {
                            newNums.Add(num.GetId().ToString().Trim(),num);
                        }
                        foreach (AUnitdeclGlobal den in denominators)
                        {
                            newDens.Add(den.GetId().ToString().Trim(),den);;
                        }
                        (SortedList<string, AUnitdeclGlobal>, SortedList<string, AUnitdeclGlobal>) unituse = (newNums, newDens);
                            symbolTable.AddNodeToUnit(node, unituse);
                            symbolTable.AddNode(node, Symbol.Ok);
                    }
                }
                else if ((leftContainsUnit || rightContainsUnit) && (symbolTable.GetSymbol(leftExpr) == Symbol.Int || symbolTable.GetSymbol(leftExpr) == Symbol.Decimal) 
                         || (symbolTable.GetSymbol(rightExpr) == Symbol.Int || symbolTable.GetSymbol(rightExpr) == Symbol.Decimal))
                {
                    // Unitnumber + decimal/int eller decimal/int + UnitNumber
                    (SortedList<string, AUnitdeclGlobal>, SortedList<string, AUnitdeclGlobal>) unit;
                    if (leftContainsUnit ? symbolTable.GetUnit(leftExpr, out unit) : rightContainsUnit && symbolTable.GetUnit(rightExpr, out unit))
                    {
                        symbolTable.AddNodeToUnit(node, unit);
                        symbolTable.AddNode(node, Symbol.Ok);
                    }
                    else
                    {
                        symbolTable.AddNode(node, Symbol.NotOk);
                        _logger.ThrowError("No valid units in expression");
                    }
                }
                else
                {
                    // not valid input expressions to a divide expression
                    symbolTable.AddNode(node, Symbol.NotOk);
                    _logger.ThrowError("Not a valid divide expression! only use allowed types");
                }
                break;
            }  
        }
        DefaultOut(node);
    }

    public override void InAMultiplyExp(AMultiplyExp node)
    {
        DefaultIn(node);
    }

    public override void OutAMultiplyExp(AMultiplyExp node)
    {
        Node leftExpr = node.GetL();
        Node rightExpr = node.GetR();
       
        Symbol? leftSymbol = symbolTable.GetSymbol(leftExpr);
        Symbol? rightSymbol = symbolTable.GetSymbol(rightExpr);
        switch (leftSymbol)
        {
            case Symbol.Int or Symbol.Pin when rightSymbol is Symbol.Int or Symbol.Pin:
                symbolTable.AddNode(node, Symbol.Int);
                break;
            case Symbol.Decimal when rightSymbol is Symbol.Decimal or Symbol.Int:
                symbolTable.AddNode(node, Symbol.Decimal);
                break;
            case Symbol.Decimal or Symbol.Int when rightSymbol is Symbol.Decimal:
                symbolTable.AddNode(node, Symbol.Decimal);
                break;
            default:
                bool leftContainsUnit = symbolTable.NodeToUnit.ContainsKey(leftExpr);
                bool rightContainsUnit = symbolTable.NodeToUnit.ContainsKey(rightExpr);
                if (symbolTable.NodeToUnit.ContainsKey(leftExpr) && symbolTable.NodeToUnit.ContainsKey(rightExpr))
                {
                    if (!(symbolTable.GetUnit(leftExpr, out (SortedList<string, AUnitdeclGlobal> num, SortedList<string, AUnitdeclGlobal> den) left) 
                          && symbolTable.GetUnit(rightExpr, out (SortedList<string, AUnitdeclGlobal> num, SortedList<string, AUnitdeclGlobal> den) right)))
                        return;

                    SortedList<string, AUnitdeclGlobal> leftNums = left.num;
                    SortedList<string, AUnitdeclGlobal> leftDens = left.den;
                    SortedList<string, AUnitdeclGlobal> rightNums = right.num;
                    SortedList<string, AUnitdeclGlobal> rightDens = right.den;
// (Volume / weight) * weight = Volume
                    List<AUnitdeclGlobal> numOverlap = leftNums.Values.Intersect(rightDens.Values).ToList();
                    List<AUnitdeclGlobal> denOverlap = leftDens.Values.Intersect(rightNums.Values).ToList();
                    
                    List<AUnitdeclGlobal> numerators = leftNums.Values.Where(x => !numOverlap.Contains(x)).Concat(rightNums.Values.Where(x => !denOverlap.Contains(x))).ToList();
                    List<AUnitdeclGlobal> denomerators = rightDens.Values.Where(x => !numOverlap.Contains(x)).Concat(leftDens.Values.Where(x => !denOverlap.Contains(x))).ToList();
                    
                    if (numerators.Count == 0 && denomerators.Count == 0)
                    {
                        symbolTable.AddNode(node, Symbol.Decimal);
                    }
                    else
                    {
                        SortedList<string, AUnitdeclGlobal> newNums = new SortedList<string, AUnitdeclGlobal>(new DuplicateKeyComparer<string>());
                        SortedList<string, AUnitdeclGlobal> newDens = new SortedList<string, AUnitdeclGlobal>(new DuplicateKeyComparer<string>());
                        foreach (AUnitdeclGlobal num in numerators)
                        {
                            newNums.Add(num.GetId().ToString().Trim(),num);
                        }
                        foreach (AUnitdeclGlobal den in denomerators)
                        {
                            newDens.Add(den.GetId().ToString().Trim(),den);;
                        }

                        (SortedList<string, AUnitdeclGlobal>, SortedList<string, AUnitdeclGlobal>) unituse = (newNums, newDens);
                        symbolTable.AddNodeToUnit(node, unituse);
                        symbolTable.AddNode(node, Symbol.Ok);
                        
                    }
                }
                else if ((leftContainsUnit || rightContainsUnit) && (symbolTable.GetSymbol(leftExpr) == Symbol.Int || symbolTable.GetSymbol(leftExpr) == Symbol.Decimal) 
                         || (symbolTable.GetSymbol(rightExpr) == Symbol.Int || symbolTable.GetSymbol(rightExpr) == Symbol.Decimal))
                {
                    // Unitnumber + decimal/int eller decimal/int + UnitNumber
                    (SortedList<string, AUnitdeclGlobal>, SortedList<string, AUnitdeclGlobal>) unit;
                    if (leftContainsUnit ? symbolTable.GetUnit(leftExpr, out unit) : rightContainsUnit && symbolTable.GetUnit(rightExpr, out unit))
                    {
                        symbolTable.AddNodeToUnit(node, unit);
                        symbolTable.AddNode(node, Symbol.Ok);
                    }
                    else
                    {
                        symbolTable.AddNode(node, Symbol.NotOk);
                        _logger.ThrowError("No valid units in expression");
                    }
                        
                }
                else
                {
                    // not valid input expressions to a multiply expression
                    symbolTable.AddNode(node, Symbol.NotOk);
                    _logger.ThrowError("Not a valid multiply expression! only use allowed types");
                }
                break;
        }
        DefaultOut(node);
    }

    public override void InAPlusExp(APlusExp node)
    {
        DefaultIn(node);
    }

    public override void OutAPlusExp(APlusExp node)
    {
        Node leftExpr = node.GetL();
        Node rightExpr = node.GetR();
        symbolTable.AddNode(node, (symbolTable.GetSymbol(leftExpr), symbolTable.GetSymbol(rightExpr)) switch
        {
            (Symbol.Int, Symbol.Int) => Symbol.Int,
            (Symbol.Pin, Symbol.Int) => Symbol.Int,
            (Symbol.Int, Symbol.Pin) => Symbol.Int,
            (Symbol.Pin, Symbol.Pin) => Symbol.Int,
            
            (Symbol.Decimal, Symbol.Decimal) => Symbol.Decimal,
            (Symbol.Decimal, Symbol.Int) => Symbol.Decimal,
            (Symbol.Int, Symbol.Decimal) => Symbol.Decimal,
            
            (Symbol.String, Symbol.String) => Symbol.String,
            (Symbol.String, Symbol.Decimal) => Symbol.String,
            (Symbol.Decimal, Symbol.String) => Symbol.String,
            (Symbol.String, Symbol.Int) => Symbol.String,
            (Symbol.Int, Symbol.String) => Symbol.String,
            _ => SameUnits(node, leftExpr, rightExpr) ? Symbol.Ok : Symbol.NotOk
        });
        DefaultOut(node);
    }

    private bool SameUnits(Node node, Node leftExpr, Node rightExpr)
    {
        if (symbolTable.GetUnit(leftExpr, out var unit1) && symbolTable.GetUnit(rightExpr, out var unit2))
        {
            if (symbolTable.CompareUnitTypes(unit1, unit2))
            {
                symbolTable.AddNodeToUnit(node, unit1);
                return true;
            }
            _logger.ThrowError("Not the same unitTypes used in expression");
            return false;
        }
        // not valid input expression
        _logger.ThrowError("Not valid Types used together in expression!");
        return false;
    }

    public override void InAMinusExp(AMinusExp node)
    {
        DefaultIn(node);
    }

    public override void OutAMinusExp(AMinusExp node)
    {
        Node leftExpr = node.GetL();
        Node rightExpr = node.GetR();
       
        Symbol? leftSymbol = symbolTable.GetSymbol(leftExpr);
        Symbol? rightSymbol = symbolTable.GetSymbol(rightExpr);
        switch (leftSymbol)
        {
            case Symbol.Int or Symbol.Pin when rightSymbol is Symbol.Int or Symbol.Pin:
                symbolTable.AddNode(node, Symbol.Int);
                break;
            case Symbol.Decimal when rightSymbol is Symbol.Decimal or Symbol.Int:
                symbolTable.AddNode(node, Symbol.Decimal);
                break;
            case Symbol.Decimal or Symbol.Int when rightSymbol is Symbol.Decimal:
                symbolTable.AddNode(node, Symbol.Decimal);
                break;
            default:
                // UnitTypes?
                if (symbolTable.GetUnit(leftExpr, out var unit1) && symbolTable.GetUnit(rightExpr, out var unit2))
                {
                    if (symbolTable.CompareUnitTypes(unit1, unit2))
                    {
                        symbolTable.AddNodeToUnit(node, unit1);
                        symbolTable.AddNode(node, Symbol.Ok);
                    } 
                    else
                    {
                        symbolTable.AddNode(node, Symbol.NotOk);
                        _logger.ThrowError("Not the same unitTypes used in expression");
                    }
                }
                else
                {
                    // not valid input expression
                    symbolTable.AddNode(node, Symbol.NotOk);
                    _logger.ThrowError("Not valid Types used together in expression!");
                }
                break;
        }
        DefaultOut(node);
    }

    public override void InARemainderExp(ARemainderExp node)
    {
        DefaultIn(node);
    }

    public override void OutARemainderExp(ARemainderExp node)
    {
        Node leftExpr = node.GetL();
        Node rightExpr = node.GetR();
        Symbol? left = symbolTable.GetSymbol(leftExpr);
        Symbol? right = symbolTable.GetSymbol(rightExpr);
        Symbol output;
        switch (left, right)
        {
            case (Symbol.Int, Symbol.Int):
            case (Symbol.Pin, Symbol.Int):
            case (Symbol.Int, Symbol.Pin):
                output = Symbol.Int;
                break;
            default:
                output = Symbol.NotOk;
                break;
        }
        
        symbolTable.AddNode(node, output);
        if (output == Symbol.NotOk)
            _logger.ThrowError("Only integers allowed in remainder expression");
        DefaultOut(node);
    }

    public override void InATernaryExp(ATernaryExp node)
    {
        DefaultIn(node);
    }

    public override void OutATernaryExp(ATernaryExp node)
    {
        Node condExp = node.GetCond();
        /* if (condExp is AIdExp id)
        {
            symbolTable.GetNodeFromId(id.GetId().ToString().Trim(), out condExp);
        } */
        Symbol? condition = symbolTable.GetSymbol(condExp);
        if (condition == Symbol.Bool)
        {
            Node trueExpr = node.GetTrue();
            Node falseExpr = node.GetFalse();
            var trueSymbol = symbolTable.GetSymbol(trueExpr);
            var falseSymbol = symbolTable.GetSymbol(falseExpr);
            switch (trueSymbol)
            {
                case Symbol.Int:
                    symbolTable.AddNode(node, falseSymbol == Symbol.Int ? Symbol.Int : Symbol.NotOk);
                    if (falseSymbol != Symbol.Int)
                        _logger.ThrowError("True and false exp should be of same type");
                    break;
                case Symbol.Decimal:
                    symbolTable.AddNode(node, falseSymbol == Symbol.Decimal ? Symbol.Decimal : Symbol.NotOk);
                    if (falseSymbol != Symbol.Decimal)
                        _logger.ThrowError("True and false exp should be of same type");
                    break;
                case Symbol.Bool:
                    symbolTable.AddNode(node, falseSymbol == Symbol.Bool ? Symbol.Bool : Symbol.NotOk);
                    if (falseSymbol != Symbol.Bool)
                        _logger.ThrowError("True and false exp should be of same type");
                    break;
                case Symbol.Char:
                    symbolTable.AddNode(node, falseSymbol == Symbol.Char ? Symbol.Char : Symbol.NotOk);
                    if (falseSymbol != Symbol.Char)
                        _logger.ThrowError("True and false exp should be of same type");
                    break;
                case Symbol.String:
                    symbolTable.AddNode(node, falseSymbol == Symbol.String ? Symbol.String : Symbol.NotOk);
                    if (falseSymbol != Symbol.String)
                        _logger.ThrowError("True and false exp should be of same type");
                    break;
                case Symbol.Pin:
                    symbolTable.AddNode(node, falseSymbol == Symbol.Pin ? Symbol.Pin : Symbol.NotOk);
                    if (falseSymbol != Symbol.Pin)
                        _logger.ThrowError("True and false exp should be of same type");
                    break;
                default:
                    // compare custom unit
                    bool leftContainsUnit = symbolTable.NodeToUnit.ContainsKey(trueExpr);
                    bool rightContainsUnit = symbolTable.NodeToUnit.ContainsKey(falseExpr);
                    if (leftContainsUnit && rightContainsUnit)
                    {
                        // Tjek at typerne er ens 
                        symbolTable.GetUnit(trueExpr, out var trueUnit); // unit 1
                        symbolTable.GetUnit(falseExpr, out var falseUnit); // unit 2
                        if (symbolTable.CompareUnitTypes(trueUnit, falseUnit))
                        {
                            symbolTable.AddNodeToUnit(node, trueUnit);
                            symbolTable.AddNode(node, Symbol.Ok);
                        }
                        else
                        {
                            symbolTable.AddNode(node, Symbol.NotOk);
                            _logger.ThrowError("UnitTypes does not match");
                        }
                    }
                    else
                    {
                        symbolTable.AddNode(node, Symbol.NotOk);
                        _logger.ThrowError("Illegal type used in trueExpression");
                    }
                    break;
            }
        }
        else
        {
            symbolTable.AddNode(node, Symbol.NotOk);
            _logger.ThrowError("Condition expression is not a boolean");
        }
        DefaultOut(node);
    }

    public override void InALogicalnotExp(ALogicalnotExp node)
    {
        DefaultIn(node);
    }

    public override void OutALogicalnotExp(ALogicalnotExp node)
    {
        Node exp = node.GetExp();
        if (exp == null)
        {
            symbolTable.AddNode(node, Symbol.NotOk);
            _logger.ThrowError("Expression is empty");
        }
        else if (exp != null)
        {
            Symbol? symbol = symbolTable.GetSymbol(exp);
            symbolTable.AddNode(node, symbol == Symbol.Bool ? Symbol.Bool : Symbol.NotOk);
            if (symbol != Symbol.Bool)
                _logger.ThrowError("Expression should be a boolean");
        }
        DefaultOut(node);
    }

    public override void InACastExp(ACastExp node)
    {
        DefaultIn(node);
    }

    public override void OutACastExp(ACastExp node)
    {
        Symbol? targetExpr = symbolTable.GetSymbol(node.GetType());
        Node exp = node.GetExp();
        Symbol? expr = symbolTable.GetSymbol(exp);
        switch (targetExpr, expr)
        {
            //typecasting to the same lmao
            case (Symbol.Bool, Symbol.Bool):
                symbolTable.AddNode(node, Symbol.Bool);
                break;
            case (Symbol.Int, Symbol.Int):
                symbolTable.AddNode(node, Symbol.Int);
                break;
            case (Symbol.Decimal, Symbol.Decimal):
                symbolTable.AddNode(node, Symbol.Decimal);
                break;
            case (Symbol.String, Symbol.String):
                symbolTable.AddNode(node, Symbol.String);
                break;
            case (Symbol.Char, Symbol.Char):
                symbolTable.AddNode(node, Symbol.Char);
                break;
            case (Symbol.Pin, Symbol.Pin):
                symbolTable.AddNode(node, Symbol.Pin);
                break;
            
            /*-- casting to new types --*/
            case (Symbol.Pin, Symbol.Int):
                symbolTable.AddNode(node, Symbol.Pin);
                break;
            case (Symbol.Int, Symbol.Pin):
                symbolTable.AddNode(node, Symbol.Int);
                break;
            case (Symbol.Decimal, Symbol.Int):
                symbolTable.AddNode(node, Symbol.Decimal);
                break;
            case (Symbol.Decimal, Symbol.Pin):
                symbolTable.AddNode(node, Symbol.Decimal);
                break;
            case (Symbol.Int, Symbol.Decimal):
                symbolTable.AddNode(node, Symbol.Int);
                break;
            case (Symbol.String, Symbol.Char):
                symbolTable.AddNode(node, Symbol.String);
                break;
            case (Symbol.String, Symbol.Int):
                symbolTable.AddNode(node, Symbol.String);
                break;
            case (Symbol.String, Symbol.Decimal):
                symbolTable.AddNode(node, Symbol.String);
                break;
            case (Symbol.Char, Symbol.Int):
                symbolTable.AddNode(node, Symbol.Char);
                break;
            default:
                symbolTable.AddNode(node, Symbol.NotOk);
                _logger.ThrowError("Illegal cast types");
                break;
        }
        DefaultOut(node);
    }

    public override void InAOrExp(AOrExp node)
    {
        DefaultIn(node);
    }

    public override void OutAOrExp(AOrExp node)
    {
        Node leftExpr = node.GetL();
        Node rightExpr = node.GetR();
        Symbol? leftside = symbolTable.GetSymbol(leftExpr);
        Symbol? rightside = symbolTable.GetSymbol(rightExpr);
        if (leftside == Symbol.Bool && rightside == Symbol.Bool)
        {
            symbolTable.AddNode(node, Symbol.Bool);
        }
        else
        {
            symbolTable.AddNode(node, Symbol.NotOk);
            _logger.ThrowError("Both expressions should be booleans");
        }
        DefaultOut(node);
    }

    public override void InAAndExp(AAndExp node)
    {
        DefaultIn(node);
    }

    public override void OutAAndExp(AAndExp node)
    {
        Node leftExpr = node.GetL();
        Node rightExpr = node.GetR();
        Symbol? leftside = symbolTable.GetSymbol(leftExpr);
        Symbol? rightside = symbolTable.GetSymbol(rightExpr);
        if (leftside == Symbol.Bool && rightside == Symbol.Bool)
        {
            symbolTable.AddNode(node, Symbol.Bool);
        }
        else
        {
            symbolTable.AddNode(node, Symbol.NotOk);
            _logger.ThrowError("Both expressions should be booleans");
        }
        DefaultOut(node);
    }

    public override void InASuffixplusplusExp(ASuffixplusplusExp node)
    {
        DefaultIn(node);
    }

    public override void OutASuffixplusplusExp(ASuffixplusplusExp node)
    {
        Node exp = node.GetExp();
        Symbol? expr = symbolTable.GetSymbol(exp);
        switch (expr)
        {
            case Symbol.Int:
                symbolTable.AddNode(node, Symbol.Int);
                break;
            case Symbol.Decimal:
                symbolTable.AddNode(node, Symbol.Decimal);
                break;
            case Symbol.Char:
                symbolTable.AddNode(node, Symbol.Char);
                break;
            case Symbol.Pin:
                symbolTable.AddNode(node, Symbol.Pin);
                break;
            default:
                symbolTable.AddNode(node, Symbol.NotOk);
                _logger.ThrowError("can only be int, decimal or char types");
                break;
        }
        DefaultOut(node);
    }

    public override void InASuffixminusminusExp(ASuffixminusminusExp node)
    {
        DefaultIn(node);
    }

    public override void OutASuffixminusminusExp(ASuffixminusminusExp node)
    {
        Node exp = node.GetExp();
        Symbol? expr = symbolTable.GetSymbol(exp);
        switch (expr)
        {
            case Symbol.Int:
                symbolTable.AddNode(node, Symbol.Int);
                break;
            case Symbol.Decimal:
                symbolTable.AddNode(node, Symbol.Decimal);
                break;
            case Symbol.Char:
                symbolTable.AddNode(node, Symbol.Char);
                break;
            case Symbol.Pin:
                symbolTable.AddNode(node, Symbol.Pin);
                break;
            default:
                symbolTable.AddNode(node, Symbol.NotOk);
                _logger.ThrowError("can only be int, decimal or char types");
                break;
        }
        DefaultOut(node);
    }

    public override void InAUnaryminusExp(AUnaryminusExp node)
    {
        DefaultIn(node);
    }

    public override void OutAUnaryminusExp(AUnaryminusExp node)
    {
        Node exp = node.GetExp();
        Symbol? expr = symbolTable.GetSymbol(exp);
        switch (expr)
        {
            case Symbol.Int:
                symbolTable.AddNode(node, Symbol.Int);
                break;
            case Symbol.Decimal:
                symbolTable.AddNode(node, Symbol.Decimal);
                break;
            case Symbol.Char:
                symbolTable.AddNode(node, Symbol.Char);
                break;
            case Symbol.Pin:
                symbolTable.AddNode(node, Symbol.Pin);
                break;
            default:
                symbolTable.AddNode(node, Symbol.NotOk);
                _logger.ThrowError("can only be int, decimal or char types");
                break;
        }
        DefaultOut(node);
    }

    public override void InAPrefixminusminusExp(APrefixminusminusExp node)
    {
        DefaultIn(node);
    }

    public override void OutAPrefixminusminusExp(APrefixminusminusExp node)
    {
        Node exp = node.GetExp();
        Symbol? expr = symbolTable.GetSymbol(exp);
        switch (expr)
        {
            case Symbol.Int:
                symbolTable.AddNode(node, Symbol.Int);
                break;
            case Symbol.Decimal:
                symbolTable.AddNode(node, Symbol.Decimal);
                break;
            case Symbol.Char:
                symbolTable.AddNode(node, Symbol.Char);
                break;
            case Symbol.Pin:
                symbolTable.AddNode(node, Symbol.Pin);
                break;
            default:
                symbolTable.AddNode(node, Symbol.NotOk);
                _logger.ThrowError("can only be int, decimal or char types");
                break;
        }
        DefaultOut(node);
    }

    public override void InAPrefixplusplusExp(APrefixplusplusExp node)
    {
        DefaultIn(node);
    }

    public override void OutAPrefixplusplusExp(APrefixplusplusExp node)
    {
        Node exp = node.GetExp();
        Symbol? expr = symbolTable.GetSymbol(exp);
        switch (expr)
        {
            case Symbol.Int:
                symbolTable.AddNode(node, Symbol.Int);
                break;
            case Symbol.Decimal:
                symbolTable.AddNode(node, Symbol.Decimal);
                break;
            case Symbol.Char:
                symbolTable.AddNode(node, Symbol.Char);
                break;
            case Symbol.Pin:
                symbolTable.AddNode(node, Symbol.Pin);
                break;
            default:
                symbolTable.AddNode(node, Symbol.NotOk);
                _logger.ThrowError("can only be int, decimal or char types");
                break;
        }
        DefaultOut(node);
    }
    public override void CaseAEqualExp(AEqualExp node)
    {
        DefaultIn(node);
        AddBinaryToSymbolTable(node, node.GetL(), node.GetR());
        DefaultOut(node);
    }
    public override void CaseANotequalExp(ANotequalExp node)
    {
        DefaultIn(node);
        AddBinaryToSymbolTable(node, node.GetL(), node.GetR());
        DefaultIn(node);
    }

    public override void CaseAGreaterExp(AGreaterExp node)
    {
        DefaultIn(node);
        AddBinaryNumberToSymbolTable(node, node.GetL(), node.GetR());
        DefaultOut(node);
    }
    public override void InALessExp(ALessExp node)
    {
        DefaultIn(node);
        AddBinaryNumberToSymbolTable(node, node.GetL(), node.GetR());
        
        DefaultOut(node);
    }
    public override void InAGreaterequalExp(AGreaterequalExp node)
    {
        DefaultIn(node);
        AddBinaryNumberToSymbolTable(node, node.GetL(), node.GetR());
        DefaultOut(node);
    }
    public override void InALessequalExp(ALessequalExp node)
    {
        DefaultIn(node);
        AddBinaryNumberToSymbolTable(node, node.GetL(), node.GetR());
        DefaultOut(node);
    }
    // Equals, Notequal
    private void AddBinaryToSymbolTable(Node node, Node L, Node R)
    {
        Node leftExpr = L;
        Node rightExpr = R;
        
        Symbol? left = symbolTable.GetSymbol(leftExpr);
        Symbol? right = symbolTable.GetSymbol(rightExpr);
        Symbol output;
        switch (left, right) {
            //int, decimal, string, bool, char, pin
            case (Symbol.Int, Symbol.Int):
            case (Symbol.Decimal, Symbol.Decimal):
            case (Symbol.String, Symbol.String):
            case (Symbol.Bool, Symbol.Bool):
            case (Symbol.Char, Symbol.Char):
            case (Symbol.Pin, Symbol.Pin):
            //decimal
            case (Symbol.Int, Symbol.Decimal):
            case (Symbol.Decimal, Symbol.Int):
            //string
            case (Symbol.String, Symbol.Int):
            case (Symbol.Int, Symbol.String):
            case (Symbol.String, Symbol.Decimal):
            case (Symbol.Decimal, Symbol.String):
            //char
            case (Symbol.Char, Symbol.String):
            case (Symbol.String, Symbol.Char):
                output = Symbol.Bool;
                break;
            default:
                output = SameUnits(node, leftExpr, rightExpr) ? Symbol.Bool : Symbol.NotOk;
                break;
        }
        symbolTable.AddNode(node, output);
        if (output == Symbol.NotOk)
            _logger.ThrowError("Types does not match");
        DefaultOut(node);
    }
    // Binary without string, bool
    // Greater, GreaterEqual, Less, LessEqual
    private void AddBinaryNumberToSymbolTable(Node node, Node L, Node R)
    {
        Node leftExpr = L;
        Node rightExpr = R;
        
        Symbol? left = symbolTable.GetSymbol(leftExpr);
        switch (left)
        {
            case Symbol.Int:
                symbolTable.AddNode(node, symbolTable.GetSymbol(rightExpr) is Symbol.Int or Symbol.Decimal or Symbol.Char or Symbol.Pin ? Symbol.Bool : Symbol.NotOk);
                break;
            case (Symbol.Decimal):
                symbolTable.AddNode(node, symbolTable.GetSymbol(rightExpr) is Symbol.Int or Symbol.Decimal ? Symbol.Bool : Symbol.NotOk);
                break;
            case Symbol.Char:
                symbolTable.AddNode(node, symbolTable.GetSymbol(rightExpr) is Symbol.Int or Symbol.Char ? Symbol.Bool : Symbol.NotOk);
                break;
            case Symbol.Pin:
                symbolTable.AddNode(node, symbolTable.GetSymbol(rightExpr) is Symbol.Int or Symbol.Pin ? Symbol.Bool : Symbol.NotOk);
                break;
            default:
                bool leftContainsUnit = symbolTable.NodeToUnit.ContainsKey(leftExpr);
                bool rightContainsUnit = symbolTable.NodeToUnit.ContainsKey(rightExpr);
                if (leftContainsUnit && rightContainsUnit)
                {
                    symbolTable.GetUnit(leftExpr, out var unit1);
                    symbolTable.GetUnit(rightExpr, out var unit2);
                    symbolTable.AddNode(node, symbolTable.CompareUnitTypes(unit1, unit2) ? Symbol.Bool : Symbol.NotOk);
                    if (!symbolTable.CompareUnitTypes(unit1, unit2))
                        _logger.ThrowError("unitTypes does not match");
                }
                else
                {
                    symbolTable.AddNode(node, Symbol.NotOk);
                    _logger.ThrowError("types does not match");
                }
                break;
        }
        DefaultOut(node);
    }
}

