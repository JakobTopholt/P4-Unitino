
package org.sablecc.sablecc.xss2;

import org.sablecc.sablecc.xss2.analysis.*;
import org.sablecc.sablecc.xss2.parser.*;
import org.sablecc.sablecc.xss2.lexer.*;
import org.sablecc.sablecc.xss2.node.*;
import org.sablecc.sablecc.DataTree;

import java.util.*;
import java.io.*;

interface Function
{
  Object call (Environment env, List args, Node context);
}

class TokenFinder extends DepthFirstAdapter
{
  Token find (Node n)
  {
    token = null;
    n.apply(this);
    return token;
  }

  private Token token;

  public void defaultCase (Node n)
  {
    if ( n instanceof Token && token == null)
        token = (Token)n;
  }
}

class Environment
{
  private TokenFinder tf = new TokenFinder ();

  DataTree root = null;

  Stack loop_stack = new Stack ();
  ArrayList loop = null;

  Stack iterator_stack = new Stack ();
  ListIterator iterator = null;

  Stack current_stack = new Stack ();
  DataTree current = null;

  Stack vars_stack = new Stack ();
  Map vars = new HashMap();

  Stack out_stack = new Stack ();
  Writer out = null;

  void write (String str, Node ctx)
  {
    try {
        out.write (str);
//        out.flush();
    } catch (Exception e) {
        error (ctx, "Stream output error: " + e.getMessage());
    }
  }

  File output_path;

  Stack xss_path_stack = new Stack();
  File xss_path;

  Map functions = new HashMap ();
  Map templates = new HashMap ();

  Map template2xss = new HashMap (); // mapping template name to file

  void error (Node ok, String msg)
  {
    Token tok = tf.find(ok);
    if ( tok != null )
        throw new RuntimeException (xss_path.toString() + ":[" + tok.getLine() + "," + tok.getPos() + "]: " + msg);
    else
        throw new RuntimeException ("[Unknown]: " + msg);
  }
}

class XSSInterpreterWalker extends DepthFirstAdapter
{
  private Environment env = new Environment();
  private Map data = new HashMap();

  private Map varmap = new HashMap ();

  XSSInterpreterWalker (DataTree init, Writer out, File output_path, File xss_path)
  {
    env.root = init;
    env.out = out;
    env.output_path = output_path;
    env.xss_path = xss_path;

    env.loop = env.root.childList ("parser");
    env.iterator = env.loop.listIterator();
    env.current = (DataTree)env.iterator.next();

    env.functions.put ("position", new Function () {
        public Object call (Environment env, List args, Node context)
        {
            if ( args.size() != 0 ) error (context, "position(): does not take any arguments!");
            return "" + env.iterator.nextIndex();
        }
    });

    env.functions.put ("count", new Function () {
        public Object call (Environment env, List args, Node context)
        {
            if ( args.size() != 1 ) error (context, "count(): takes exactly one argument!");
            ArrayList l = o2l(args.get(0));
            if ( l == null ) error (context, "count(): first argument must be node selection!");
            return "" + l.size();
        }
    });

    env.functions.put ("translate", new Function () {
        public Object call (Environment env, List args, Node context)
        {
            if ( args.size() != 3 ) error (context, "translate(): takes exactly three arguments!");
            String str = o2s(args.get(0));
            String from = o2s(args.get(1));
            String to = o2s(args.get(2));
            if ( str == null || from == null || to == null ) error (context, "translate(): one of the argument was null!");
            StringBuffer ret = new StringBuffer ();
            for ( int i = 0; i < str.length(); i++ ) {
                char c = str.charAt(i);
                int index = from.indexOf(c);
                if ( index >= 0 && index < to.length() )
                    c = to.charAt(index);
                ret.append(c);
            }
            return ret.toString();
        }
    });

    env.functions.put ("concat", new Function () {
        public Object call (Environment env, List args, Node context)
        {
            if ( args.size() == 0 ) error (context, "concat(): takes at least 1 argument!");
            StringBuffer ret = new StringBuffer ();
            for ( ListIterator it = args.listIterator(); it.hasNext(); ) {
                String s = o2s(it.next());
                if ( s == null ) error (context, "concat(): argument " + it.nextIndex() + " was null!");
                ret.append (s);
            }
            return ret.toString();
        }
    });

    env.functions.put ("substring", new Function () {
        public Object call (Environment env, List args, Node context)
        {
            if ( args.size() < 2 ) error (context, "substring(): takes at least 2 arguments!");
            String full = o2s(args.get(0));
            int start = Integer.parseInt(o2s(args.get(1)))-1;
            int len = full.length();
            if (args.size() == 3) len = start+Integer.parseInt(o2s(args.get(2)));
            return full.substring(start, len);
        }
    });

    env.functions.put ("substring_before", new Function () {
        public Object call (Environment env, List args, Node context)
        {
            if ( args.size() != 2 ) error (context, "substring_before(): takes exactly 2 arguments!");
            String str1 = o2s(args.get(0));
            String str2 = o2s(args.get(1));
            int index = str1.indexOf(str2);
            if ( index == -1 ) return "";
            return str1.substring(0, index);
        }
    });

    env.functions.put ("substring_after", new Function () {
        public Object call (Environment env, List args, Node context)
        {
            if ( args.size() != 2 ) error (context, "substring_after(): takes exactly 2 arguments!");
            String str1 = o2s(args.get(0));
            String str2 = o2s(args.get(1));
            int index = str1.indexOf(str2);
            if ( index == -1 ) return "";
            return str1.substring(index + str2.length());
        }
    });

    env.functions.put ("not", new Function () {
        public Object call (Environment env, List args, Node context)
        {
            if ( args.size() != 1 ) error (context, "not(): takes exactly 1 argument!");
            String s = o2s(args.get(0));
            if ( s == null )
                return "1";
            else
                return null;
        }
    });

    env.functions.put ("reverse", new Function () {
        public Object call (Environment env, List args, Node context)
        {
            if ( args.size() != 1 ) error (context, "reverse(): takes exactly 1 argument!");
            ArrayList l = o2l(args.get(0));
            if ( l == null ) error (context, "reverse(): takes node selection as argument!");
            ArrayList ret = new ArrayList();
            for ( Iterator it = l.iterator(); it.hasNext(); ) {
                ret.add(0, it.next());
            }
            return ret;
        }
    });

    env.functions.put ("string", new Function () {
        public Object call (Environment env, List args, Node context)
        {
            if ( args.size() != 1 ) error (context, "string(): takes exactly 1 argument!");
            String s = o2s(args.get(0));
            if ( s == null )
                return "";
            else
                return s;
        }
    });

    env.functions.put ("sablecc:touppercase", new Function () {
        public Object call (Environment env, List args, Node context)
        {
            if ( args.size() != 1 ) error (context, "string(): takes exactly 1 argument!");
            String s = o2s(args.get(0));
            if ( s == null )
                return "";
            else
                return s.toUpperCase();
        }
    });

    env.functions.put ("sablecc:string2escaped_unicode", new Function () {
        public Object call (Environment env, List args, Node context)
        {
            if ( args.size() != 1 ) error (context, "sablecc:string2escaped_unicode(): Only take one argument!");

            String s = o2s(args.get(0));
            if ( s == null ) error (context, "sablecc:string2escaped_unicode(): No value found!");

            StringBuffer out = new StringBuffer ();
            for ( int i = 0; i < s.length(); i++ ) {
                char n = s.charAt(i);
                if ( n == '\n' ) {
                    out.append ("\\n");
                } else if ( n == '\r' ) {
                    out.append ("\\r");
                } else if ( n == '"' ) {
                    out.append ("\\\"");
                } else if ( n == '\\' ) {
                    out.append ("\\\\");
                } else if ( n == 0 ) {
                    out.append ("\\000");
                } else if ( n < 0x20 || n > 0x7f ) {
                    String h = Integer.toHexString((int)n);
                    while ( h.length() < 4 ) h = "0" + h;
                    out.append ("\\u" + h);
                } else {
                    out.append (n);
                }
            }
            return out.toString();
        }
    });

    env.functions.put ("sablecc:toxml", new Function () {
        public Object call (Environment env, List args, Node context)
        {
            if ( args.size() != 2 ) error (context, "sablecc:toxml(): Takes two arguments!");

            ArrayList l = o2l(args.get(0));
            String s = o2s(args.get(1));
            if ( l == null || l.size() == 0 )
                error (context, "sablecc:toxml(): First argument must be a valid node selection!");
            if ( s == null )
                error (context, "sablecc:toxml(): No valid second value found!");

            int ident = -1;
            try {
                ident = Integer.parseInt(s);
            } catch (Exception e) {
                error (context, "sablecc:toxml(): Second argument must be an integer!");
            }

            DataTree t = (DataTree)l.get(0);
            return t.toXML (ident);
        }
    });

    env.functions.put ("contains", new Function () {
        public Object call (Environment env, List args, Node context)
        {
            if ( args.size() != 2 ) error (context, "contains(): Takes two arguments!");

            String l = o2s(args.get(0));
            String r = o2s(args.get(1));
            if ( l == null || r == null ) error (context, "contains(): Both arguments have to be strings!");

            if ( l.indexOf(r) == -1 )
                return null;
            else
                return "1";
        }
    });

    env.functions.put ("sablecc:toggle", new Function () {
        private Map tmap = new HashMap ();

        public Object call (Environment env, List args, Node context)
        {
            if ( args.size() != 2 ) error (context, "sablecc:toggle(): Takes two arguments!");

            String l = o2s(args.get(0));
            String r = o2s(args.get(1));
            if ( l == null || r == null ) error (context, "sablecc:toggle(): Both arguments have to be strings!");

            Set m = (Set)tmap.get(l);
            if ( m == null ) {
                m = new HashSet ();
                tmap.put(l, m);
            }

            if ( m.contains(r) )
                return "1";

            m.add(r);
            return null;
        }
    });

    env.functions.put ("sablecc:varput", new Function () {
        public Object call (Environment env, List args, Node context)
        {
            if ( args.size() != 2 ) error (context, "sablecc:varput(): Takes two arguments!");

            String l = o2s(args.get(0));
            String r = o2s(args.get(1));
            if ( l == null || r == null ) error (context, "sablecc:varput(): Both arguments have to be strings!");

            varmap.put(l, r);
            return null;
        }
    });

    env.functions.put ("sablecc:varget", new Function () {
        public Object call (Environment env, List args, Node context)
        {
            if ( args.size() != 1 ) error (context, "sablecc:varput(): Takes one argument!");

            String l = o2s(args.get(0));
            if ( l == null ) error (context, "sablecc:varput(): Argument has to be string!");

            return varmap.get(l);
        }
    });
  }

  public void setVariable (String name, String value)
  {
    env.vars.put(name, value);
  }

  private Object consume (Node node)
  {
    Object ret = data.get(node);
    data.remove(node);
    return ret;
  }

  private void provide (Node node, Object obj)
  {
    data.put (node, obj);
  }

  public void outADataStatement (ADataStatement node)
  {
    env.write (data(node.getData().getText()), node);
  }

  public void outAPrintStatement (APrintStatement node)
  {
    String str = o2s(consume(node.getExpr()));
    if ( str == null ) str = "";
    env.write (str, node);
  }

  public void caseAChooseStatement (AChooseStatement node)
  {
    List stmts = node.getOtherwise();
    for ( Iterator it = node.getChooseWhen().iterator(); it.hasNext(); ) {
        AChooseWhen when = (AChooseWhen)it.next();
        when.getTest().apply(this);
        String s = o2s(consume(when.getTest()));
        if ( s != null ) {
            stmts = when.getStatement();
            break;
        }
    }

    for ( Iterator it = stmts.iterator(); it.hasNext(); ) {
        PStatement stmt = (PStatement)it.next();
        stmt.apply(this);
    }
  }

  public void caseATemplateStatement (ATemplateStatement node)
  {
    env.templates.put (node.getName().getText(), node);
    env.template2xss.put (node, env.xss_path);
  }

  public void outACallStatement (ACallStatement node)
  {
    env.vars_stack.push (env.vars);
    env.vars = new HashMap (env.vars);

    ATemplateStatement tlt = (ATemplateStatement)env.templates.get(node.getName().getText());
    if ( tlt == null ) error (node, "Undefined template '" + node.getName().getText() + "'!");

    env.xss_path_stack.push(env.xss_path);
    env.xss_path = (File)env.template2xss.get(tlt);

    Map args = new HashMap ();
    for ( Iterator it = tlt.getArgs().iterator(); it.hasNext(); ) {
        AOarg arg = (AOarg)it.next();
        String val = "";
        if ( arg.getExpr() != null ) {
            arg.getExpr().apply(this);
            val = o2s(consume (arg.getExpr()));
        }

        String var = arg.getVar().getText();
        if ( !args.containsKey(var) )
            args.put (var, val);

    }

    env.xss_path = (File)env.xss_path_stack.pop();

    for ( Iterator it = node.getArgs().iterator(); it.hasNext(); ) {
        AArg arg = (AArg)it.next();
        arg.getExpr().apply(this);
        String val = o2s(consume (arg.getExpr()));

        String var = arg.getVar().getText();
        if ( !args.containsKey(var) )
            error (arg, "Template '" + node.getName().getText() + "' does not take an argument named '" + var + "'!");
        else
            args.put (var, val);
    }

    env.xss_path_stack.push(env.xss_path);
    env.xss_path = (File)env.template2xss.get(tlt);

    for ( Iterator it = args.entrySet().iterator(); it.hasNext(); ) {
        Map.Entry entry = (Map.Entry)it.next();
        String var = (String)entry.getKey();
        String val = (String)entry.getValue();
        env.vars.put (var, val);
    }

    for ( Iterator it = tlt.getStatement().iterator(); it.hasNext(); ) {
        PStatement stmt = (PStatement)it.next();
        stmt.apply(this);
    }

    env.vars = (Map)env.vars_stack.pop();
    env.xss_path = (File)env.xss_path_stack.pop();
  }

  public void outAIncludeStatement (AIncludeStatement node)
  {
    String path = literal(node.getPath().getText());
    File ctx = env.xss_path.getParentFile();

    InputStream is = null;
    File f = null;

    try {
        if ( ctx.isAbsolute() ) {
            f = new File (ctx, path).getAbsoluteFile();
            is = new FileInputStream (f);
        } else {
            is = getClass().getResourceAsStream(ctx.toString().replace(File.separatorChar, '/') + "/" + path);
            f = new File (ctx, path);
        }
    } catch (IOException e) {
        error (node, "Unable to open file '" + path + "' from context '" + ctx + "': " + e.getMessage());
    }

    env.xss_path_stack.push(env.xss_path);
    env.xss_path = f;

    Start start = null;

    try {
        Lexer lexer = new Lexer (new PushbackReader (new BufferedReader(new XSSReader (new InputStreamReader (is))), 65536));
        Parser parser = new Parser (lexer);
        start = parser.parse();
    } catch (Exception e) {
        error (node, "Error: " + e.getMessage());
    }

    start.apply(this);

    env.xss_path = (File)env.xss_path_stack.pop();
  }

  public void outASeparatorStatement (ASeparatorStatement node)
  {
    String str = (String)consume(node.getExpr());
    if ( env.iterator.hasNext() )
        env.write (str, node);
  }

  public void outALiteralExpr (ALiteralExpr node)
  {
    provide (node, literal(node.getLiteral().getText()));
  }

  public void outAIattribExpr (AIattribExpr node)
  {
    String attrib = attrib(node.getIattrib().getText());
    if ( env.current == null ) error (node.getIattrib(), "No current node!");
    String ret = env.current.getAttrib (attrib);
    provide (node, ret);
  }

  public void outAIvarExpr (AIvarExpr node)
  {
    provide (node, env.vars.get(node.getIvar().getText().substring(1)));
  }

  public void outAConcatExpr (AConcatExpr node)
  {
    String l = o2s(consume (node.getE1()));
    String r = o2s(consume (node.getE2()));
    if ( l == null ) error (node.getE1(), "Expression can't be null!");
    if ( r == null ) error (node.getE2(), "Expression can't be null!");
    provide (node, l + r);
  }

  public void outAParamStatement (AParamStatement node)
  {
    for ( Iterator it = node.getArgs().iterator(); it.hasNext(); ) {
        AOarg arg = (AOarg)it.next();
        Object val = null;
        if ( arg.getExpr() != null ) val = consume (arg.getExpr());
        String var = arg.getVar().getText();
        if ( !env.vars.containsKey(var) )
            env.vars.put (var, val);
    }
  }

  public void outASetStatement (ASetStatement node)
  {
    for ( Iterator it = node.getArgs().iterator(); it.hasNext(); ) {
        AArg arg = (AArg)it.next();
        Object val = null;
        if ( arg.getExpr() != null ) val = consume (arg.getExpr());
        String var = arg.getVar().getText();
        env.vars.put (var, val);
    }
  }

  public void outAVarStatement (AParamStatement node)
  {
    for ( Iterator it = node.getArgs().iterator(); it.hasNext(); ) {
        AArg arg = (AArg)it.next();
        String val = o2s(consume (arg.getExpr()));
        String var = arg.getVar().getText();
        env.vars.put (var, val);
    }
  }

  public void outAXpathExpr (AXpathExpr node)
  {
    provide(node, consume (node.getXpathExpr()));
  }

  public void caseAForeachStatement (AForeachStatement node)
  {
    node.getXpath().apply(this);
    env.loop_stack.push(env.loop);
    env.loop = o2l(consume(node.getXpath()));
    if ( env.loop == null ) error (node.getXpath(), "Foreach takes a node selection as argument!");

    env.iterator_stack.push (env.iterator);
    env.iterator = env.loop.listIterator();

    env.vars_stack.push (env.vars);
    env.vars = new HashMap (env.vars);

    env.current_stack.push(env.current);

    while ( env.iterator.hasNext())
    {
        env.current = (DataTree)env.iterator.next();
        if ( node.getVar() != null ) {
            ArrayList list = new ArrayList ();
            list.add(env.current);
            env.vars.put (node.getVar().getText(), list);
        }

        for ( Iterator it = node.getStatement().iterator(); it.hasNext(); ) {
            PStatement stmt = (PStatement)it.next();
            stmt.apply(this);
        }
    }

    env.loop = (ArrayList)env.loop_stack.pop();
    env.iterator = (ListIterator)env.iterator_stack.pop();
    env.current = (DataTree)env.current_stack.pop();
    env.vars = (Map)env.vars_stack.pop();
  }

  public void caseAIfStatement (AIfStatement node)
  {
    node.getTest().apply(this);
    String res = o2s(consume(node.getTest()));

    if ( res == null ) {
        for ( Iterator it = node.getElse().iterator(); it.hasNext(); ) {
            PStatement stmt = (PStatement)it.next();
            stmt.apply(this);
        }
    } else {
        for ( Iterator it = node.getStatement().iterator(); it.hasNext(); ) {
            PStatement stmt = (PStatement)it.next();
            stmt.apply(this);
        }
    }
  }

  public void caseAOutputStatement (AOutputStatement node)
  {
    node.getExpr().apply(this);
    String res = o2s(consume(node.getExpr()));
    if ( res == null || "".equals(res)) error (node, "Output file can't be null or empty!");
    File f = new File (env.output_path, res);
    f.getParentFile().mkdirs();
    env.out_stack.push (env.out);
    try {
        env.out = new BufferedWriter (new FileWriter (f));
        for ( Iterator it = node.getStatement().iterator(); it.hasNext(); ) {
            PStatement stmt = (PStatement)it.next();
            stmt.apply(this);
        }
        env.out.flush();
        env.out.close();
        env.out = (Writer)env.out_stack.pop();
    } catch (IOException e) {
        e.printStackTrace();
        error (node, "Unable to open file '" + f.toString() + "' for writing!");
    }
  }

  public void outAFunctionXpathExpr (AFunctionXpathExpr node)
  {
    String name = node.getName().getText();
    Function f = (Function)env.functions.get(name);
    if ( f == null ) error (node.getName(), "Function '" + name + "' not found!");
    ArrayList args = new ArrayList();
    for ( Iterator it = node.getArgs().iterator(); it.hasNext(); ) {
        args.add(consume((Node)it.next()));
    }
    provide (node, f.call(env, args, node.getName()));
  }

  private static String literal (String str)
  {
    StringBuffer buf = new StringBuffer ();
    for ( int i = 1; i < str.length() - 1; i++ ) {
        char n = str.charAt(i);
        if ( n == str.charAt(0) ) {
            buf.append (n);
            i++;
        } else {
            buf.append (n);
        }
    }
    return buf.toString();
  }

  private static String attrib (String str)
  {
    if ( str.charAt(1) == '{' )
        return str.substring(2, str.length()-1);
    else
        return str.substring(1);
  }

  private void error (Node ok, String msg)
  {
    env.error (ok, msg);
  }

  public void outALiteralXpathExpr (ALiteralXpathExpr node)
  {
    provide (node, literal(node.getXliteral().getText()));
  }

  public void outANumberXpathExpr (ANumberXpathExpr node)
  {
    provide (node, node.getXnumber().getText());
  }

  public void outAAddXpathExpr (AAddXpathExpr node)
  {
    String l = (String)consume(node.getE1());
    String r = (String)consume(node.getE2());
    if ( l == null ) error (node.getE1(), "Expression can't be null!");
    if ( r == null ) error (node.getE2(), "Expression can't be null!");
    try {
        provide (node, "" + (Integer.parseInt(l) + Integer.parseInt(r)));
    } catch (Exception e) {
        e.printStackTrace();
        error (node, "Unable to add two arguments!");
    }
  }

  public void outASubXpathExpr (ASubXpathExpr node)
  {
    String l = (String)consume(node.getE1());
    String r = (String)consume(node.getE2());
    if ( l == null ) error (node.getE1(), "Expression can't be null!");
    if ( r == null ) error (node.getE2(), "Expression can't be null!");
    try {
        provide (node, "" + (Integer.parseInt(l) - Integer.parseInt(r)));
    } catch (Exception e) {
        e.printStackTrace();
        error (node, "Unable to add two arguments!");
    }
  }

  public void outAMulXpathExpr (ADivXpathExpr node)
  {
    String l = (String)consume(node.getE1());
    String r = (String)consume(node.getE2());
    if ( l == null ) error (node.getE1(), "Expression can't be null!");
    if ( r == null ) error (node.getE2(), "Expression can't be null!");
    try {
        provide (node, "" + (Integer.parseInt(l) * Integer.parseInt(r)));
    } catch (Exception e) {
        e.printStackTrace();
        error (node, "Unable to mul two arguments!");
    }
  }

  public void outADivXpathExpr (ADivXpathExpr node)
  {
    String l = (String)consume(node.getE1());
    String r = (String)consume(node.getE2());
    if ( l == null ) error (node.getE1(), "Expression can't be null!");
    if ( r == null ) error (node.getE2(), "Expression can't be null!");
    try {
        provide (node, "" + (Integer.parseInt(l) / Integer.parseInt(r)));
    } catch (Exception e) {
        e.printStackTrace();
        error (node, "Unable to divide two arguments!");
    }
  }

  public void outAModXpathExpr (AModXpathExpr node)
  {
    String l = (String)consume(node.getE1());
    String r = (String)consume(node.getE2());
    if ( l == null ) error (node.getE1(), "Expression can't be null!");
    if ( r == null ) error (node.getE2(), "Expression can't be null!");
    try {
        provide (node, "" + (Integer.parseInt(l) % Integer.parseInt(r)));
    } catch (Exception e) {
        e.printStackTrace();
        error (node, "Unable to mod two arguments!");
    }
  }

  public void outAOrXpathExpr (AOrXpathExpr node)
  {
    String l = (String)consume(node.getE1());
    String r = (String)consume(node.getE2());
    if ( l != null || r != null )
        provide (node, "1");
    else
        provide (node, null);
  }

  public void outAAndXpathExpr (AAndXpathExpr node)
  {
    String l = (String)consume(node.getE1());
    String r = (String)consume(node.getE2());
    if ( l != null && r != null )
        provide (node, "1");
    else
        provide (node, null);
  }

  public void outAEqualsXpathExpr (AEqualsXpathExpr node)
  {
    String l = (String)consume(node.getE1());
    String r = (String)consume(node.getE2());
    if ( l == null || r == null || !l.equals(r) )
        provide (node, null);
    else
        provide (node, "1");
  }

  public void outANotEqualsXpathExpr (ANotEqualsXpathExpr node)
  {
    String l = (String)consume(node.getE1());
    String r = (String)consume(node.getE2());
    if ( l == null || r == null || !l.equals(r) )
        provide (node, "1");
    else
        provide (node, null);
  }

  public void outALessXpathExpr (ALessXpathExpr node)
  {
    String l = (String)consume(node.getE1());
    String r = (String)consume(node.getE2());
    if ( l == null ) error (node.getE1(), "Expression can't be null!");
    if ( r == null ) error (node.getE2(), "Expression can't be null!");
    try {
        provide (node, Integer.parseInt(l) < Integer.parseInt(r) ? "1" : null);
    } catch (Exception e) {
        e.printStackTrace();
        error (node, "Unable to compare two arguments!");
    }
  }

  public void outALessOrEqualXpathExpr (ALessOrEqualXpathExpr node)
  {
    String l = (String)consume(node.getE1());
    String r = (String)consume(node.getE2());
    if ( l == null ) error (node.getE1(), "Expression can't be null!");
    if ( r == null ) error (node.getE2(), "Expression can't be null!");
    try {
        provide (node, Integer.parseInt(l) <= Integer.parseInt(r) ? "1" : null);
    } catch (Exception e) {
        e.printStackTrace();
        error (node, "Unable to compare two arguments!");
    }
  }

  public void outAGreaterXpathExpr (AGreaterXpathExpr node)
  {
    String l = (String)consume(node.getE1());
    String r = (String)consume(node.getE2());
    if ( l == null ) error (node.getE1(), "Expression can't be null!");
    if ( r == null ) error (node.getE2(), "Expression can't be null!");
    try {
        provide (node, Integer.parseInt(l) > Integer.parseInt(r) ? "1" : null);
    } catch (Exception e) {
        e.printStackTrace();
        error (node, "Unable to compare two arguments!");
    }
  }

  public void outAGreaterOrEqualXpathExpr (AGreaterOrEqualXpathExpr node)
  {
    String l = (String)consume(node.getE1());
    String r = (String)consume(node.getE2());
    if ( l == null ) error (node.getE1(), "Expression can't be null!");
    if ( r == null ) error (node.getE2(), "Expression can't be null!");
    try {
        provide (node, Integer.parseInt(l) >= Integer.parseInt(r) ? "1" : null);
    } catch (Exception e) {
        e.printStackTrace();
        error (node, "Unable to compare two arguments!");
    }
  }

  public void caseAPathXpathExpr (APathXpathExpr node)
  {
    boolean first = true;

    ArrayList search = new ArrayList ();

    for ( Iterator it = node.getXpathPathElem().iterator(); it.hasNext(); ) {
        AXpathPathElem elem = (AXpathPathElem)it.next();

//        System.err.println ("caseAPathXpathExpr loop: " + elem.getElem().getClass().getName());

        if (first) {
            if ( elem.getSeparator() != null )
                search.add (env.root);
            else
                search.add (env.current);
        }

        if ( elem.getElem() instanceof ANameXpathElem ) {
            ANameXpathElem e = (ANameXpathElem)elem.getElem();
            if ( elem.getSeparator() == null || elem.getSeparator() instanceof AOneXpathSeparator ) {
                search = DataTree.childList (search, e.getXname().getText());
            } else {
                search = DataTree.allChildList (search, e.getXname().getText());
/*                System.err.println ("allChildList " + e.getXname().getText());
                for ( Iterator jt = search.iterator(); jt.hasNext(); ) {
                    Object o =jt.next();
                    System.err.println ("  " + o.getClass().getName());
                } */
            }
        } else if ( elem.getElem() instanceof AVarXpathElem ) {
            AVarXpathElem e = (AVarXpathElem)elem.getElem();
            search = new ArrayList ();
            Object var = env.vars.get(e.getXvar().getText().substring(1));
            if ( !it.hasNext()) {
                provide (node, var);
                return;
            }
            if ( var == null || !(var instanceof ArrayList) ) {
                error (elem, "Variable is not a node selection!");
            }
            search.addAll ((ArrayList)var);
        } else if ( elem.getElem() instanceof AParentXpathElem ) {
            AParentXpathElem e = (AParentXpathElem)elem.getElem();
            if ( elem.getSeparator() == null || elem.getSeparator() instanceof AOneXpathSeparator ) {
                search = DataTree.parentList (search);
            } else {
                search = DataTree.allParentList (search);
            }
        } else if ( elem.getElem() instanceof ACurrentXpathElem ) {
            ACurrentXpathElem e = (ACurrentXpathElem)elem.getElem();
            if ( elem.getSeparator() == null || elem.getSeparator() instanceof AOneXpathSeparator ) {
                // no change
            } else {
                search = DataTree.allList (search);
            }
        } else if ( elem.getElem() instanceof AAttribXpathElem ) {
            AAttribXpathElem e = (AAttribXpathElem)elem.getElem();

            String attrib;
            String aname = attrib(e.getXattrib().getText());

            if ( elem.getSeparator() == null || elem.getSeparator() instanceof AOneXpathSeparator ) {
                attrib = DataTree.attribList (search, aname);
            } else {
                attrib = DataTree.allAttribList (search, aname);
            }

            provide (node, attrib);
            return;
        }

        if ( elem.getCondition() != null ) {
            ArrayList s2 = new ArrayList ();

//            System.err.println ("Conditional filtering!");

            env.iterator_stack.push (env.iterator);
            env.loop_stack.push(env.loop);
            env.current_stack.push (env.current);

            env.loop = search;

            for ( env.iterator = env.loop.listIterator(); env.iterator.hasNext(); ) {
                env.current = (DataTree)env.iterator.next();
                elem.getCondition().apply(this);
                String res = o2s(consume(elem.getCondition()));
                if ( res != null )
                    s2.add (env.current);
            }

            env.iterator = (ListIterator)env.iterator_stack.pop();
            env.loop = (ArrayList)env.loop_stack.pop();
            env.current = (DataTree)env.current_stack.pop();
            search = s2;
        }

        first = false;
    }

    provide (node, search);
  }

  private static String o2s (Object o)
  {
    if ( o == null ) return null;

    if ( o instanceof String) return (String)o;

    if ( o instanceof ArrayList ) {
        ArrayList l = (ArrayList)o;
        if ( !l.isEmpty() ) {
            return ((DataTree)l.get(0)).getContent();
        }
    }

    return null;
  }

  private static ArrayList o2l (Object o)
  {
    if ( o instanceof ArrayList ) return (ArrayList)o;
    return null;
  }

  private static String data (String str)
  {
    StringBuffer ret = new StringBuffer ();
    for ( int i = 0; i < str.length(); i++ ) {
        char n = str.charAt(i);
        switch (n) {
            case '@':
            case '$':
                ret.append(n);
                i++;
                break;
            case '[':
                ret.append (n);
                if ( i + 2 < str.length() && str.charAt(i + 1) == '-' && str.charAt(i + 2) == '-' )
                    i++; // turn [-- into [-
                break;
            default:
                ret.append(n);
                break;
        }
    }
    return ret.toString();
  }
}

public class XSSInterpreter
{
  public void run (DataTree tree, Writer out, String tlt, Map args) throws Exception
  {
    String dest_dir = (String)args.get("output_dir");
    if ( dest_dir == null ) dest_dir = new File(".").getAbsolutePath();

    InputStream is = null;
    File f = null;

    if ( tlt.indexOf(File.separator) == -1 ) {
        try {
            is = getClass().getResourceAsStream("data/" + tlt + "/main.xss");
            f = new File ("data" + File.separator + tlt + File.separator + "main.xss");
        } catch (Exception e) {
            // ignore
        }
    }

    if ( is == null ) {
        is = new FileInputStream (tlt);
        f = new File(tlt).getAbsoluteFile();
    }

    Lexer lexer = new Lexer (new PushbackReader (new BufferedReader(new XSSReader (new InputStreamReader (is))), 65536));
    Parser parser = new Parser (lexer);
    Start start = parser.parse();

    XSSInterpreterWalker interpreter = new XSSInterpreterWalker (tree, out, new File(dest_dir).getAbsoluteFile(), f);
    for ( Iterator it = args.entrySet().iterator(); it.hasNext(); ) {
        Map.Entry entry = (Map.Entry)it.next();
        String key = (String)entry.getKey();
        String val = (String)entry.getValue();
        interpreter.setVariable(key, val);
//        System.err.println ("setVariable: " + key + " = " + val);
    }
    start.apply(interpreter);
    out.flush();
  }
}


