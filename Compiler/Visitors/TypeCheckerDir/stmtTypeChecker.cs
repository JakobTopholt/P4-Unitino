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

    public Symbol PTypeToSymbol(PType type)
    {
        switch (type)
        {
            case AIntType:
                return Symbol.Bool;
            default:
                return Symbol.notOk;
        }
    }

    public bool CompareCustomUnits(Node unit1, Node unit2)
    {
        var a = symbolTable.GetUnit(unit1);
        var b = symbolTable.GetUnit(unit2);
        
        // Compare logic here
        
        return true;
    }

    public override void InAReturnStmt(AReturnStmt node)
    {
      //already set before
      Node parent = node.Parent();
      while (parent is not PFunc)
      {
          parent = parent.Parent();
      }
      PExp? returnExp = node.GetExp();
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
              PType typedType = aTypedFunc.GetType();
              switch (returnExp)
              {
                  case AIdExp Id:
                      symbolTable.AddNode(node, PTypeToSymbol(typedType) == symbolTable.GetSymbol(Id.GetId()) ? Symbol.ok : Symbol.notOk);
                      break;
                  case AUnitExp unit:
                      symbolTable.AddNode(node, CompareCustomUnits(unit, typedType) ? Symbol.ok : Symbol.notOk);
                      break;
                  case null:
                      symbolTable.AddNode(node, typedType is AVoidType ? Symbol.ok : Symbol.notOk);
                      break;
                  default:
                      symbolTable.AddNode(node, PTypeToSymbol(typedType) == symbolTable.GetSymbol(returnExp) ? Symbol.ok : Symbol.notOk);
                      break;
              }
              break;
          case AUntypedFunc aUntypedFunc:
              if (!symbolTable.funcToReturn.ContainsKey(aUntypedFunc.GetId()))
              {
                  // Add the first returnExp in an untyped func as the "Correct"
                  var unit = symbolTable.GetUnit(node.GetExp());
                  symbolTable.AddNodeToUnit(node, unit);
                  symbolTable.AddNode(node, Symbol.ok);
              }
              else
              {
                  if (CompareCustomUnits(node, node.GetExp()))
                  {
                      //Return statement match earlier decleration
                      symbolTable.AddNode(node, Symbol.ok);
                  }
                  else
                  {
                      // Return staement differs from previous declared
                      symbolTable.AddNode(node, Symbol.notOk);
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