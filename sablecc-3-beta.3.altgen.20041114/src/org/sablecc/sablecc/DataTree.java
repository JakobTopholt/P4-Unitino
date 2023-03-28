
package org.sablecc.sablecc;

import java.util.*;
import java.io.*;
import java.text.SimpleDateFormat;

// Currently DataTree is created from ParserData
// In future we might do the following:
//  * drop ParserData and fill DataTree directly
//  * user sablescript to fill DataTree

public class DataTree implements Serializable
{
  private final String name;
  private final Map attr = new HashMap();
  private DataTree parent;
  private final ArrayList children = new ArrayList();
  private String content = "";

  public DataTree (String name)
  {
    this.name = name;
  }

  public DataTree (String name, String content)
  {
    this.name = name;
    this.content = content;
  }

  public String getName()
  {
    return name;
  }

  public boolean hasAttrib(String key)
  {
    return attr.containsKey(key);
  }

  public String getAttrib (String key)
  {
    return (String)attr.get(key);
  }

  public void setAttrib (String key, String value)
  {
    attr.put(key, value);
  }

  public String getContent ()
  {
    return content;
  }

  public void setContent (String str)
  {
    content = str;
  }

  public void add (DataTree child)
  {
    child.parent(this);
    children.add(child);
  }

  public void addAll (Collection c)
  {
    for ( Iterator it = c.iterator(); it.hasNext(); ) {
        add ((DataTree)it.next());
    }
  }

  public DataTree parent()
  {
    return parent;
  }

  void parent (DataTree parent)
  {
    if ( this.parent != null ) throw new RuntimeException ("Parent already set!");
    this.parent = parent;
  }

  public ArrayList childList (String name)
  {
    ArrayList ret = new ArrayList();
    for ( Iterator it = children.iterator(); it.hasNext(); ) {
        DataTree n = (DataTree)it.next();
        if ( n.getName().equals(name) ) {
            ret.add(n);
        }
    }
    return ret;
  }

  public ArrayList allChildList (String name)
  {
    ArrayList ret = new ArrayList();
    for ( Iterator it = children.iterator(); it.hasNext(); ) {
        DataTree n = (DataTree)it.next();
        if ( n.getName().equals(name) ) {
            ret.add(n);
        }
        ret.addAll(n.allChildList(name));
    }
    return ret;
  }

  public ArrayList parentList ()
  {
    ArrayList ret = new ArrayList();
    if ( parent != null ) ret.add(parent);
    return ret;
  }

  public String toXML (int ident_level)
  {
    if ( name.equals("/") ) {
        return ((DataTree)children.get(0)).toXML(ident_level);
    }

    StringWriter out = new StringWriter ();

    try {
        out.write ("<?xml version=\"1.0\"?>");
        if ( ident_level >= 0 ) out.write ("\n");
        toXML (out, 0, ident_level);
    } catch (Exception e) {
        e.printStackTrace (); // should never happen with stringwriter
    }

    return out.toString();
  }

  private void toXML(Writer out, int ident, int ident_level) throws IOException
  {
    xmlIdent (out, ident);
    out.write ("<" + name);

    for ( Iterator it = attr.entrySet().iterator(); it.hasNext(); ) {
        Map.Entry entry = (Map.Entry)it.next();
        out.write (" " + (String)entry.getKey() + "=\"" + xmlEscape((String)entry.getValue()) + "\"");
    }

    if ( content.equals("") && children.size() == 0 ) {
        out.write ("/>");
        if ( ident_level >= 0 ) out.write ("\n");
    } else {
        out.write (">");

        if ( !"".equals(content) ) {
            out.write (xmlEscape(content));
        } else {
            if ( ident_level >= 0 ) out.write ("\n");
        }

        for ( Iterator it = children.iterator(); it.hasNext(); ) {
            ((DataTree)it.next()).toXML (out, ident + (ident_level > 0 ? ident_level : 0), ident_level);
        }

        if ( "".equals(content) )
            xmlIdent (out, ident);

        out.write ("</" + name + ">");
        if ( ident_level >= 0 ) out.write ("\n");
    }
  }

  private static void xmlIdent (Writer out, int ident) throws IOException
  {
    for ( int i = 0; i < ident; i++ )
        out.write (" ");
  }

  private static String xmlEscape (String str)
  {
//    if ( str == null ) return "!!!!!!!!!!!NULL!!!!!!!!!!!!!!";
    StringBuffer out = new StringBuffer ();
    for ( int i = 0; i < str.length(); i++ ) {
        char n = str.charAt(i);
        if ( (n >= 'a' && n <= 'z') || (n >= 'A' && n <= 'Z') || (n >= '0' && n <= '9')) {
            out.append (n);
            continue;
        }

        switch (n) {
            case '!':
            case '?':
            case '{':
            case '}':
            case '(':
            case ')':
            case '[':
            case ']':
            case ',':
            case '.':
            case ':':
            case ';':
            case '=':
            case '-':
            case '+':
            case '*':
            case '_':
            case '/':
            case '\\':
            case '|':
            case '^':
            case '%':
            case '$':
            case ' ':
            case '#':
            case '@':
            case '\'':
            case '`':
            case '~':
                out.append (n);
                break;
            default:
                out.append ("&#" + (int)n + ";");
                break;
        }
    }
    return out.toString();
  }

  public static ArrayList childList (ArrayList list, String name)
  {
    ArrayList ret = new ArrayList ();
    for ( Iterator it = list.iterator(); it.hasNext(); ) {
        DataTree n = (DataTree)it.next();
        ret.addAll(n.childList(name));
    }
    return ret;
  }

  public static ArrayList allChildList (ArrayList list, String name)
  {
    ArrayList ret = new ArrayList ();
    for ( Iterator it = list.iterator(); it.hasNext(); ) {
        DataTree n = (DataTree)it.next();
        ret.addAll(n.allChildList(name));
    }
    return ret;
  }

  public static ArrayList parentList (ArrayList list)
  {
    DataTree p = null;
    ArrayList ret = new ArrayList ();
    for ( Iterator it = list.iterator(); it.hasNext(); ) {
        DataTree n = (DataTree)it.next();
        if ( p != n.parent() ) {
            ret.add(n.parent());
            p = n.parent();
        }
    }
    return ret;
  }

  public static ArrayList allParentList (ArrayList list)
  {
    return parentList(allList(list));
  }

  public ArrayList list ()
  {
    return children;
  }

  public static ArrayList allList (ArrayList list)
  {
    ArrayList ret = new ArrayList ();
    for ( Iterator it = list.iterator(); it.hasNext(); ) {
        DataTree n = (DataTree)it.next();
        ret.add(n);
        ret.addAll(DataTree.allList (n.children));
    }
    return ret;
  }

  public static String attribList (ArrayList list, String name)
  {
    for ( Iterator it = list.iterator(); it.hasNext(); ) {
        DataTree n = (DataTree)it.next();
        String ret = n.getAttrib (name);
//        System.err.println ("attribList loop: " + n.getName() + ", attrib[" + name + "] = " + ret);
        if ( ret != null ) return ret;
    }
    return null;
  }

  public static String allAttribList (ArrayList list, String name)
  {
    for ( Iterator it = list.iterator(); it.hasNext(); ) {
        DataTree n = (DataTree)it.next();
        String ret = n.getAttrib (name);
        if ( ret != null ) return ret;
        ret = DataTree.allAttribList (n.children, name);
        if ( ret != null ) return ret;
    }
    return null;
  }

  public DataTree copy ()
  {
    DataTree tree = new DataTree (name, content);
    tree.attr.putAll (attr);
    for ( Iterator it = children.iterator(); it.hasNext(); ) {
        tree.add(((DataTree)it.next()).copy());
    }
    return tree;
  }
}

class DataTreeMaker 
{
  public static DataTree make (ParserData data)
  {
    DataTree root = new DataTree ("/");
    DataTree parser = new DataTree ("parser");
    root.add(parser);
    parser.setAttrib ("package", data.package_name);
    parser.setAttrib ("generator", "SableCC3");
    parser.setAttrib ("date", new SimpleDateFormat( "yyyy-MM-dd'T'HH:mm:ss.S zzz" ).format(new Date()));
    if ( data.inlined ) parser.setAttrib ("inlined", "true");

    DataTree tokens = new DataTree ("tokens");
    parser.add (tokens);
    for ( Iterator it = data.token_list.iterator(); it.hasNext(); ) {
        ParserData.Token token = (ParserData.Token)it.next();
        DataTree token_node = new DataTree ("token");
        tokens.add(token_node);
        token_node.setAttrib ("name", token.name);
        token_node.setAttrib ("ename", token.ename);
        if ( token.value != null )
            token_node.setAttrib ("text", token.value);
        if ( token.index != -1 )
            token_node.setAttrib ("parser_index", "" + token.index);

        if ( token.transitions.size() == 0 ) {
        } else {
            for ( Iterator jt = token.transitions.entrySet().iterator(); jt.hasNext(); ) {
                Map.Entry entry = (Map.Entry)jt.next();
                DataTree transition = new DataTree ("transition");
                token_node.add(transition);
                transition.setAttrib ("from", (String)entry.getKey());
                transition.setAttrib ("to", (String)entry.getValue());
            }
        }
    }

    DataTree eof = new DataTree ("eof");
    tokens.add(eof);
    eof.setAttrib ("parser_index", "" + data.eof_index);

    DataTree productions = new DataTree ("prods");
    parser.add(productions);

    for ( Iterator it = data.production_list.iterator(); it.hasNext(); ) {
        ParserData.Production prod = (ParserData.Production)it.next();
        DataTree production = new DataTree ("prod");
        productions.add(production);
        production.setAttrib ("name", prod.name);
        production.setAttrib ("ename", prod.ename);
        for ( Iterator jt = prod.alts.iterator(); jt.hasNext(); ) {
            ParserData.Alt alt = (ParserData.Alt)jt.next();
            DataTree alternative = new DataTree ("alt");
            production.add(alternative);
            alternative.setAttrib ("name", alt.name);
            alternative.setAttrib ("ename", alt.ename);
            for ( Iterator kt = alt.elems.iterator(); kt.hasNext(); ) {
                ParserData.Elem elem = (ParserData.Elem)kt.next();
                DataTree element = new DataTree ("elem");
                alternative.add(element);
                element.setAttrib ("name", elem.name);
                element.setAttrib ("ename", elem.ename);
                element.setAttrib ("type", elem.type);
                element.setAttrib ("etype", elem.etype);
                if ( elem.modifier != null ) element.setAttrib ("modifier", elem.modifier);
                if ( elem.is_token ) element.setAttrib ("is_token", "true");
                if ( elem.is_list ) element.setAttrib ("is_list", "true");
            }
        }
    }

    DataTree lexer_data = new DataTree ("lexer_data");
    parser.add(lexer_data);
    for ( Iterator it = data.state_list.iterator(); it.hasNext(); ) {
        ParserData.LexerState state = (ParserData.LexerState)it.next();
        DataTree state_node = new DataTree("state");
        lexer_data.add(state_node);
        state_node.setAttrib("name", state.name);
        state_node.setAttrib("id", "" + state.id);
    }

    DataTree accept_table = new DataTree ("accept_table");
    lexer_data.add(accept_table);
    for ( Enumeration it = data.lexer_accept.elements(); it.hasMoreElements(); ) {
        Vector row = (Vector)it.nextElement();
        DataTree state = new DataTree ("state");
        accept_table.add(state);
        for ( Enumeration jt = row.elements(); jt.hasMoreElements(); ) {
            DataTree i = new DataTree ("i", "" + ((Number)jt.nextElement()).intValue());
            state.add(i);
        }
    }

    DataTree goto_table = new DataTree ("goto_table");
    lexer_data.add(goto_table);
    for ( Enumeration it = data.lexer_goto_table.elements(); it.hasMoreElements(); ) {
        Vector state = (Vector)it.nextElement();
        DataTree state_node = new DataTree ("state");
        goto_table.add(state_node);
        for ( Enumeration jt = state.elements(); jt.hasMoreElements(); ) {
            Vector row = (Vector)jt.nextElement();
            DataTree row_node = new DataTree ("row");
            state_node.add(row_node);
            for ( Enumeration kt = row.elements(); kt.hasMoreElements(); ) {
                int[] elem = (int[])kt.nextElement();
                DataTree goto_node = new DataTree ("goto");
                row_node.add(goto_node);
                goto_node.setAttrib("low", "" + elem[0]);
                goto_node.setAttrib("high", "" + elem[1]);
                goto_node.setAttrib("state", "" + elem[2]);
            }
        }
    }

    DataTree parser_data = new DataTree ("parser_data");
    parser.add(parser_data);

    DataTree action_table = new DataTree ("action_table");
    parser_data.add(action_table);
    for ( Enumeration it = data.parser_action_table.elements(); it.hasMoreElements(); ) {
        Vector row = (Vector)it.nextElement();
        DataTree row_node = new DataTree ("row");
        action_table.add(row_node);
        for ( Enumeration jt = row.elements(); jt.hasMoreElements(); ) {
            DataTree action_node = new DataTree ("action");
            row_node.add(action_node);
            int[] action = (int[])jt.nextElement();
            action_node.setAttrib ("from", "" + action[0]);
            action_node.setAttrib ("action", "" + action[1]);
            action_node.setAttrib ("to", "" + action[2]);
        }
    }

    goto_table = new DataTree ("goto_table");
    parser_data.add(goto_table);
    for ( Enumeration it = data.parser_goto_table.elements(); it.hasMoreElements(); ) {
        Vector row = (Vector)it.nextElement();
        DataTree row_node = new DataTree ("row");
        goto_table.add(row_node);
        for ( Enumeration jt = row.elements(); jt.hasMoreElements(); ) {
            DataTree goto_node = new DataTree ("goto");
            row_node.add(goto_node);
            int[] action = (int[])jt.nextElement();
            goto_node.setAttrib ("from", "" + action[0]);
            goto_node.setAttrib ("to", "" + action[1]);

        }
    }

    DataTree errors = new DataTree ("errors");
    parser_data.add(errors);
    for ( Enumeration it = data.parser_errors.elements(); it.hasMoreElements(); ) {
        DataTree i = new DataTree ("i", "" +  ((Integer)it.nextElement()).intValue());
        errors.add(i);
    }

    DataTree error_messages = new DataTree ("error_messages");
    parser_data.add(error_messages);
    for ( Enumeration it = data.parser_error_messages.elements(); it.hasMoreElements(); ) {
        DataTree msg = new DataTree ("msg", (String)it.nextElement());
        error_messages.add(msg);
    }

    String cmd2str[] = new String[] {"POP", "FETCHNODE", "FETCHLIST", "ADDNODE", "ADDLIST",
      "MAKELIST", "MAKENODE", "RETURNNODE", "RETURNLIST"};

    DataTree rules = new DataTree ("rules");
    parser.add(rules);

    for ( Iterator it = data.rules_list.iterator(); it.hasNext(); ) {
        ParserData.Rule rule = (ParserData.Rule)it.next();
        DataTree rule_node = new DataTree ("rule");
        rules.add(rule_node);
        rule_node.setAttrib ("ename", rule.ename);
        rule_node.setAttrib ("index", "" + rule.index);
        rule_node.setAttrib ("leftside", "" + rule.leftside);
        for ( Iterator jt = rule.actions.iterator(); jt.hasNext(); ) {
            ParserData.Action action = (ParserData.Action)jt.next();
            DataTree action_node = new DataTree ("action");
            rule_node.add(action_node);
            action_node.setAttrib ("cmd", cmd2str[action.cmd]);
            if ( action.result != null ) action_node.setAttrib ("result", action.result);
            if ( action.index != -1 ) action_node.setAttrib ("index", "" + action.index);
            if ( action.etype != null ) action_node.setAttrib ("etype", action.etype);
            switch (action.cmd) {
                case ParserData.Action.FETCHNODE:
                case ParserData.Action.FETCHLIST:
                    action_node.setAttrib ("from", action.arg[0]);
                    continue;
                case ParserData.Action.RETURNNODE:
                    if ( action.arg[0] == null )
                        action_node.setAttrib ("null", "true");
                    else
                        action_node.setAttrib ("node", action.arg[0]);
                    continue;
                case ParserData.Action.RETURNLIST:
                    action_node.setAttrib ("list", action.arg[0]);
                    continue;
                case ParserData.Action.ADDNODE:
                    action_node.setAttrib ("tolist", action.arg[0]);
                    action_node.setAttrib ("node", action.arg[1]);
                    continue;
                case ParserData.Action.ADDLIST:
                    action_node.setAttrib ("tolist", action.arg[0]);
                    action_node.setAttrib ("fromlist", action.arg[1]);
                    continue;
            }
            if ( action.arg.length != 0 ) {
                for ( int i = 0; i < action.arg.length; i++ ) {
                    DataTree arg = new DataTree ("arg");
                    action_node.add(arg);
                    if ( action.arg[i] == null )
                        arg.setAttrib ("null", "true");
                    else {
                        if (action.arg[i].startsWith("listNode"))
                            arg.setAttrib ("is_list", "true");
                        arg.setContent (action.arg[i]);
                    }
                }
            }
        }
    }

    // and now the C++ specific Data, I hope some day this can be
    // moved to a sablescript domain
    fillCXX (root);

    return root;
  }

  // what we do here is we create an alternative tree inside the DataTRee
  // named cxx and augument it with information needed to generate cxx output
  private static void fillCXX (DataTree root)
  {
    int maxnodes = 0;
    int maxlists = 0;
    int maxpops = 0;
    int maxargs = 0;
    int maxreturns = 0;
    int makenodeid = 0;

//    DataTree cxx = new DataTree ("cxx");
//    root.add(cxx);

//    DataTree rules_node = ((DataTree)root.allChildList("rules").get(0)).copy();
    DataTree rules_node = (DataTree)root.allChildList("rules").get(0);
//    cxx.add(rules_node);

    ArrayList rule_list = rules_node.childList ("rule");

    for ( int i = 0; i < rule_list.size(); i++ ) {
        DataTree rule = (DataTree)rule_list.get(i);
        ArrayList action_list = rule.childList ("action");

        int returns = 0;
        Map pop2id = new HashMap ();
        Map list2id = new HashMap ();
        Map node2id = new HashMap ();

        for ( int j = 0; j < action_list.size(); j++ ) {
            DataTree action = (DataTree)action_list.get(j);
            String cmd = action.getAttrib ("cmd");
            if ( cmd.equals ("POP") ) {
                String pid = "" + pop2id.size();
                pop2id.put(action.getAttrib("result"),  pid);
                action.setAttrib ("result_id", pid);
            } else if ( cmd.equals ("FETCHLIST") ) {
                String pid = (String)pop2id.get(action.getAttrib("from"));
                String lid = "" + list2id.size();
                list2id.put(action.getAttrib("result"), lid);
                action.setAttrib ("from_id", pid);
                action.setAttrib ("result_id", lid);
            } else if ( cmd.equals ("FETCHNODE") ) {
                String pid = (String)pop2id.get(action.getAttrib("from"));
                String nid = "" + node2id.size();
                node2id.put(action.getAttrib("result"), nid);
                action.setAttrib ("from_id", pid);
                action.setAttrib ("result_id", nid);
            } else if ( cmd.equals ("ADDLIST") ) {
                String fid = (String)list2id.get(action.getAttrib("fromlist"));
                String tid = (String)list2id.get(action.getAttrib("tolist"));
                action.setAttrib ("fromlist_id", fid);
                action.setAttrib ("tolist_id", tid);
            } else if ( cmd.equals ("ADDNODE") ) {
                String fid = (String)node2id.get(action.getAttrib("node"));
                String tid = (String)list2id.get(action.getAttrib("tolist"));
                action.setAttrib ("node_id", fid);
                action.setAttrib ("tolist_id", tid);
            } else if ( cmd.equals ("MAKELIST") ) {
                String lid = "" + list2id.size();
                list2id.put(action.getAttrib("result"), lid);
                action.setAttrib ("result_id", lid);
            } else if ( cmd.equals ("MAKENODE") ) {
                String nid = "" + node2id.size();
                node2id.put(action.getAttrib("result"), nid);
                action.setAttrib ("result_id", nid);
                int acount = action.childList("arg").size();
                if ( acount > maxargs ) maxargs = acount;
                action.setAttrib ("arglist_id", "" + makenodeid++);
                String args = "";
                for ( Iterator it = action.childList("arg").iterator(); it.hasNext(); ) {
                    DataTree arg = (DataTree)it.next();
                    String id = (String)node2id.get(arg.getContent());
                    if ( id == null )
                        id = (String)list2id.get(arg.getContent());
                    if ( id == null ) {
                        arg.setAttrib ("id", "-1");
                        args += "-1";
                    } else {
                        arg.setAttrib ("id", id);
                        args += id;
                    }
                    if ( it.hasNext () ) args += ", ";
                }
                action.setAttrib ("arglist", args);
            } else if ( cmd.equals ("RETURNLIST") ) {
                String lid = (String)list2id.get(action.getAttrib("list"));
                action.setAttrib ("list_id", lid);
                returns++;
            } else if ( cmd.equals ("RETURNNODE") ) {
                String nid = (String)node2id.get(action.getAttrib("node"));
                if ( nid != null ) action.setAttrib ("node_id", nid);
                returns++;
            } else {
                throw new RuntimeException ("Unknown command " + cmd + "!!!");
            }
        }

        if ( maxnodes < node2id.size() ) maxnodes = node2id.size();
        if ( maxlists < list2id.size() ) maxlists = list2id.size();
        if ( maxpops < pop2id.size() ) maxpops = pop2id.size();
        if ( maxreturns < returns ) maxreturns = returns;
    }

    rules_node.setAttrib ("maxnodes", "" + maxnodes);
    rules_node.setAttrib ("maxlists", "" + maxlists);
    rules_node.setAttrib ("maxpops", "" + maxpops);
    rules_node.setAttrib ("maxargs", "" + maxargs);
    rules_node.setAttrib ("maxreturns", "" + maxreturns);
  }
}

/** This class contains all the state that is required to genereate
  * a parser. All the state is already "calculated and resolved" so
  * no grammar analysis is neccessary. */
class ParserData {
  public String package_name;
  public boolean inlined;

  /** Class representing what is neede for a token. */
  public static class Token {
    Token (String name, String ename, String value, int index)
    {
      this.name = name;
      this.ename = ename;
      this.value = value;
      this.index = index;
      if ( verbose )
        System.out.println ("new Token (" + name + ", " + ename + ", " + value + ", " + index + ")");
    }

    /** Name of token, like 'token'. */
    public String name;
    /** Encoded name, like 'TToken'. */
    public String ename;
    /** Value of a fixed token, null if variable token. */
    public String value;
    /** Index of the token for parser. */
    public int index;

    public Map transitions = new HashMap();
  }

  public final List token_list = new LinkedList ();
  public int eof_index;

  /** Vector[Vector[Integer]] */
  public Vector lexer_accept;
  /** Vector[Vector[Vector[int[]]]]] */
  public Vector lexer_goto_table;
//  public int[][][][] lexer_goto_table;

  /** Class representing what is neede for a lexical analyzer state. */
  public static class LexerState {
    LexerState (String name, int id)
    {
      this.name = name;
      this.id = id;
      if ( verbose )
        System.out.println ("new LexerState (" + name + ", " + id + ")");
    }

    public String name;     /* State name, like INITIAL */
    public int id;          /* State id */
  };

  public final List state_list = new LinkedList ();

  /** Class representing what is neede for a alternative element. */
  public static class Elem {
    Elem (String name, String ename, boolean is_token, String type, String etype, String modifier)
    {
      this.name = name;
      this.ename = ename;
      this.is_token = is_token;
      this.type = type;
      this.etype = etype;
      this.modifier = modifier;
      if ( "*".equals(modifier) || "+".equals(modifier) )
          this.is_list = true;
      else
          this.is_list = false;
      if ( verbose )
        System.out.println ("new Elem (" + name + ", " + ename + ", " + is_token + ", " +
          type + ", " + etype + ", " + modifier + ")");
    }

    public String name;
    public String ename;
    public boolean is_token;
    public String type;
    public String etype;
    public String modifier; /* Modifier, "*", "+", "?" or null */
    public boolean is_list;
  }

  /** Class representing what is needed for an alternative. */
  public static class Alt {
    Alt (String n, String en)
    {
      name = n;
      ename = en;

      if (verbose)
        System.out.println ("new Alt (" + name + ", " + ename + ")");
    }

    public String name;   /* Name of the alternative. */
    public String ename;
    public final List elems = new LinkedList ();
  }

  /** Class representing what is needed for a production. */
  public static class Production {
    Production (String n, String en)
    {
      name = n;
      ename = en;

      if (verbose)
        System.out.println ("new Production (" + name + ", " + ename + ")");
    }

    public String name;
    public String ename;
    public final List alts = new LinkedList ();
  }

  public final List production_list = new LinkedList ();

  /** Vector[Vector[int[3]]] */
  public Vector parser_action_table;
  /** Vector[Vector[int[2]]] */
  public Vector parser_goto_table;
  /** Vector[String] */
  public Vector parser_error_messages;
  /** Vector[Integer] */
  public Vector parser_errors;

  /** This is a really silly "language", i couldn't figure a better way.
    * We probably need goto and conditionals to be turing complete. */
  public static class Action {
    Action (int cmd, String result, int index, String[] arg, String etype)
    {
      if ( arg == null ) arg = new String[] {};
      this.cmd = cmd;
      this.result = result;
      this.index = index;
      this.arg = arg;
      this.etype = etype;

      if (verbose)
      {
        String a = "";
        if ( arg != null )
        for ( int i = 0; i < arg.length; i++ ) { a += arg[i]; if ( i + 1 < arg.length ) a += ", "; }
        System.out.println ("new Action (" + cmd2str[cmd] + ", " + result + ", (" + a + "), " + index + ", " + etype + ")");
      }
    }

    public int cmd;                 // POP, FETCH, etc.
    public String result;           // result = cmd(arg1, ..)
    public int index;               // used for indexing at FETCH and FETCHLIST 
    public String arg[];
    public String etype;

    public final static int POP = 0;       // pop()-s a vector from stack (reduce) => result
    public final static int FETCHNODE = 1; // fetch node from vector arg[0] at pos index of type etype => result
    public final static int FETCHLIST = 2; // fetch list from vector arg[0] at pos index of type etype (list elems) => result
    public final static int ADDNODE = 3;   // add node arg[1] into list arg[0]
    public final static int ADDLIST = 4;   // add all elements from list arg[1] into list arg[0] (both are lists)
    public final static int MAKELIST = 5;  // makes an empty list of type etype => result
    public final static int MAKENODE = 6;  // makes a new object with given encoded type in etype and arg[] => result
    public final static int RETURNNODE = 7;// adds arg[0] into return vector, can be null
    public final static int RETURNLIST = 8;// adds a list arg[0] into return vector

    private static String cmd2str[] = new String[] {"POP", "FETCHNODE", "FETCHLIST", "ADDNODE", "ADDLIST",
      "MAKELIST", "MAKENODE", "RETURNNODE", "RETURNLIST"};
  };

  public static class Rule {
    Rule (String ename, int index, int leftside)
    {
      this.ename = ename;
      this.index = index;
      this.leftside = leftside;

      if (verbose)
        System.out.println ("new Rule (" + ename + ", " + index + ", " + leftside + ")");
    }

    public String ename;
    public int index;                               // same as rule index/position in rules_list
    public int leftside;

    public final List actions = new LinkedList();   // a series of Action-s to be executed for this rule
  }

  /** Vector[Rule] */
  public final List rules_list = new LinkedList();

  static boolean verbose = false;
}

