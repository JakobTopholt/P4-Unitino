using Moduino.analysis;
using Moduino.node;

namespace Compiler.Visitors;

// Ikke sikkert der er brug for denne visitor T_T (idk yet)

public class GlobalReturnCollector : DepthFirstAdapter
{
    // Deal with untyped functions (figure out what symbol/type they return)
    // Deal with typed functions (Make sure that they actually return a value/symbol that corresponds to its defined type)
}