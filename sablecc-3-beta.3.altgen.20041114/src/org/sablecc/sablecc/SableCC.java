/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * This file is part of SableCC.                             *
 * See the file "LICENSE" for copyright information and the  *
 * terms and conditions for copying, distribution and        *
 * modification of SableCC.                                  *
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

package org.sablecc.sablecc;

import java.io.*;
import java.awt.*;
import java.util.*;
import org.sablecc.sablecc.node.*;
import org.sablecc.sablecc.analysis.*;
import org.sablecc.sablecc.lexer.*;
import org.sablecc.sablecc.parser.*;
import org.sablecc.sablecc.xss2.*;
import java.util.Vector;
import java.util.zip.GZIPInputStream;
import java.util.zip.GZIPOutputStream;

public class SableCC
{
  private static boolean processInlining = true;
  private static boolean prettyPrinting = false;

  private static String cacheFile = null;
  private static Map parameters = new HashMap ();
  private static OutputStream output = System.out;
  private static Map languages = new HashMap ();


  private static void displayCopyright()
  {
    System.out.println();
    System.out.println("SableCC version " + Version.VERSION);
    System.out.println("Copyright (C) 1997-2003 Etienne M. Gagnon <etienne.gagnon@uqam.ca> and");
    System.out.println("others.  All rights reserved.");
    System.out.println();
    System.out.println("This software comes with ABSOLUTELY NO WARRANTY.  This is free software,");
    System.out.println("and you are welcome to redistribute it under certain conditions.");
    System.out.println();
    System.out.println("Type 'sablecc -license' to view");
    System.out.println("the complete copyright notice and license.");
    System.out.println();
  }

  private static void displayUsage()
  {
    System.out.println("Usage:");
    System.out.println("  sablecc [-c cache.file] [--no-inline] [-d destination] [-t target|file.xss[,..]] \\");
    System.out.println("      [-p key value] [-o path] filename [filename]...");
    System.out.println("  sablecc -license");
    System.out.println("Builtin targets:");
    System.out.println("  One or multiple of: java, java-build, cxx, cxx-build, csharp, csharp-build");
    System.out.println("  ocaml, ocaml-build, dotgraph-simple, dotgraph-tree, dotgraph-graph, xml");
  }

  public static void main(String[] arguments)
  {
    String d_option = null;
    Vector filename = new Vector();

    if(arguments.length == 0)
    {
      displayCopyright();
      displayUsage();
      System.exit(1);
    }

    if((arguments.length == 1) && (arguments[0].equals("-license")))
    {
      new DisplayLicense();
      System.exit(0);
    }

    displayCopyright();

    {
      int arg = 0;
      while(arg < arguments.length)
      {
        if(arguments[arg].equals("-h") || arguments[arg].equals("--help"))
        {
          displayUsage ();
          System.exit(0);
        }
        else if(arguments[arg].equals("-t"))
        {
          if ( ++arg < arguments.length )
          {
            StringTokenizer tt = new StringTokenizer (arguments[arg], ",");
            while ( tt.hasMoreTokens () )
            {
              String lt = tt.nextToken();
              String langname = null, targetname = null;

              if ( lt.indexOf("-") == -1 )
              {
                langname = lt;
              }
              else
              {
                langname = lt.substring(0, lt.indexOf("-"));
                targetname = lt.substring(lt.indexOf("-") + 1);
              }

              Set lang = (Set)languages.get(langname);
              if ( lang == null )
              {
                lang = new HashSet();
                languages.put(langname, lang);
              }

              if ( targetname != null )
              {
                lang.add(targetname);
              }
            }
          }
          else
          {
            displayUsage();
            System.exit(1);
          }
        }
        else if(arguments[arg].equals("-o"))
        {
          if ( ++arg < arguments.length )
          {
            try
            {
              output = new FileOutputStream (arguments[arg]);
            }
            catch (Exception e)
            {
              e.printStackTrace();
              displayUsage();
              System.exit(1);
            }
          }
          else
          {
            displayUsage();
            System.exit(1);
          }
        }
        else if(arguments[arg].equals("-d"))
        {
          if((d_option == null) && (++arg < arguments.length))
          {
            d_option = arguments[arg];
          }
          else
          {
            displayUsage();
            System.exit(1);
          }
        }
        else if(arguments[arg].equals("-p"))
        {
          arg += 2;
          if(arg < arguments.length)
          {
            parameters.put (arguments[arg-1], arguments[arg]);
          }
          else
          {
            displayUsage();
            System.exit(1);
          }
        }
        else if(arguments[arg].equals("-c"))
        {
          if ( ++arg < arguments.length )
          {
            cacheFile = arguments[arg];
          }
          else
          {
            displayUsage();
            System.exit(1);
          }
        }
        else if(arguments[arg].equals("--no-inline"))
        {
          processInlining = false;
        }
        /*
          if prettyprint is set to true, only the transformed
          grammar is printed on standard output
        */
        else if(arguments[arg].equals("--prettyprint"))
        {
          prettyPrinting = true;
        }
        else
        {
          filename.addElement(arguments[arg]);
        }
        arg++;
      }

      if(filename.size() == 0)
      {
        displayUsage();
        System.exit(1);
      }
    }

    if ( languages.size() == 0 ) // no target given, defaulting to java
    {
      languages.put("java", new HashSet());
    }

    try
    {
      for(int i=0; i<filename.size(); i++)
      {
        processGrammar((String)filename.elementAt(i), d_option);
      }
    }
    catch(Exception e)
    {
      e.printStackTrace();
      System.exit(1);
    }
    System.exit(0);
  }

  /**
   * The main method for processing grammar file and generating the parser/lexer.
   * @param grammar input grammar file name
   * @param destDir output directory name
   */
  public static void processGrammar(String grammar, String destDir) throws Exception
  {
    File in;
    File dir;

    in = new File(grammar);
    in = new File(in.getAbsolutePath());

    if(destDir == null)
    {
      dir = new File(in.getParent());
    }
    else
    {
      dir = new File(destDir);
      dir = new File(dir.getAbsolutePath());
    }

    if ( cacheFile != null )
    {
      File cf = new File(cacheFile);
      if ( cf.exists() && in.exists() && cf.lastModified() == in.lastModified() )
      {
        DataTree data_tree = null;
        try
        {
          ObjectInputStream ois = new ObjectInputStream (new GZIPInputStream(new FileInputStream (cf)));
          data_tree = (DataTree)ois.readObject ();
          ois.close();
        }
        catch (Exception e)
        {
          // ignore
        }

        if ( data_tree != null )
        {
          System.out.println ("Loaded parser data from " + cf.getPath());
          generate (data_tree, dir);
          return;
        }
      }
    }

    processGrammar(in, dir);
  }

  /**
   * The main method for processing grammar file and generating the parser/lexer.
   * @param in input grammar file
   * @param dir output directory
   */
  public static void processGrammar(File in,  File dir) throws Exception
  {
    if(!in.exists())
    {
      System.out.println("ERROR: grammar file "+in.getName()+" does not exist.");
      System.exit(1);
    }
    if(!dir.exists())
    {
      System.out.println("ERROR: destination directory "+dir.getName()+" does not exist.");
      System.exit(1);
    }

    // re-initialize all static structures in the engine
    LR0Collection.reinit();
    Symbol.reinit();
    Production.reinit();
    Grammar.reinit();

    System.out.println("\n -- Generating parser for "+in.getName()+" in "+dir.getPath());

    FileReader temp = new FileReader(in);

    ParserData data = new ParserData ();  // first collect the parser state into this structure

    // Build the AST
    Start tree = new Parser(new Lexer(new PushbackReader(
                                        temp = new FileReader(in), 1000))).parse();

    temp.close();

    boolean hasTransformations = false;

    if( ((AGrammar)tree.getPGrammar()).getAst() == null )
    {
      System.out.println("Adding productions and alternative of section AST.");
      //AddAstProductions astProductions = new AddAstProductions();
      tree.apply(new AddAstProductions());
    }
    else
    {
      hasTransformations = true;
    }

    System.out.println("Verifying identifiers.");
    ResolveIds ids = new ResolveIds(dir, data);
    tree.apply(ids);

    System.out.println("Verifying ast identifiers.");
    ResolveAstIds ast_ids = new ResolveAstIds(ids);
    tree.apply(ast_ids);

    System.out.println("Adding empty productions and empty alternative transformation if necessary.");
    tree.apply( new AddEventualEmptyTransformationToProductions(ids, ast_ids) );

    System.out.println("Adding productions and alternative transformation if necessary.");
    AddProdTransformAndAltTransform adds = new AddProdTransformAndAltTransform();
    tree.apply(adds);
    /*
    System.out.println("Replacing AST + operator by * and removing ? operator if necessary");
    tree.apply( new AstTransformations() );
    */
    System.out.println("computing alternative symbol table identifiers.");
    ResolveAltIds alt_ids = new ResolveAltIds(ids);
    tree.apply(alt_ids);

    System.out.println("Verifying production transform identifiers.");
    ResolveProdTransformIds ptransform_ids = new ResolveProdTransformIds(ast_ids);
    tree.apply(ptransform_ids);

    System.out.println("Verifying ast alternatives transform identifiers.");
    ResolveTransformIds transform_ids = new ResolveTransformIds(ast_ids, alt_ids, ptransform_ids);
    tree.apply(transform_ids);

    System.out.println("Generating token classes.");
    tree.apply(new GenTokens(ids));

    System.out.println("Generating production classes.");
    tree.apply(new GenProds(ast_ids));

    System.out.println("Generating alternative classes.");
    tree.apply(new GenAlts(ast_ids));

    System.out.println("Generating analysis classes.");
    tree.apply(new GenAnalyses(ast_ids));

    System.out.println("Generating utility classes.");
    tree.apply(new GenUtils(ast_ids));

    try
    {
      System.out.println("Generating the lexer.");
      tree.apply(new GenLexer(ids));
    }
    catch(Exception e)
    {
      System.out.println(e.getMessage());
      throw e;
    }

    try
    {
      System.out.println("Generating the parser.");
      tree.apply(new GenParser(ids, alt_ids, transform_ids, ast_ids.getFirstAstProduction(), processInlining, prettyPrinting, hasTransformations) );
    }
    catch(Exception e)
    {
      System.out.println(e.getMessage());
      throw e;
    }

    // some day we might get rid of ParserData and operator
    // on DataTree directly or through sablescript
    fillData (data, tree, ids, ast_ids, transform_ids);
    DataTree data_tree = DataTreeMaker.make(data);
    if ( cacheFile != null )
    {
      try
      {
        File cf = new File (cacheFile);
        ObjectOutputStream oos = new ObjectOutputStream (new GZIPOutputStream(new FileOutputStream (cf)));
        oos.writeObject (data_tree);
        oos.close();
        cf.setLastModified (in.lastModified());
      }
      catch (Exception e)
      {
        System.err.println ("Error at creating cache: " + e.getMessage());
        // ignore
      }
    }
    generate (data_tree, dir);
  }

  public static void generate (DataTree tree, File dir) throws Exception
  {
    for ( Iterator it = languages.entrySet().iterator(); it.hasNext(); )
    {
      Map.Entry entry = (Map.Entry)it.next();
      String langname = (String)entry.getKey();
      Set targets = (Set)entry.getValue();

      Map params = new HashMap(parameters);
      for ( Iterator jt = targets.iterator(); jt.hasNext(); ) {
          String targname = (String)jt.next();
          params.put ("target_" + targname, "1");
      }

      params.put("output_dir", dir.getPath());

      XSSInterpreter interpreter = new XSSInterpreter();
      StringWriter outbuf = new StringWriter ();
      interpreter.run (tree, outbuf, langname, params);
      String str = outbuf.toString();

      if ( !str.trim().equals("") )
      {
        Writer out = new OutputStreamWriter (output);
        // this is a bug in Java, it seems it can't write a huge string into file
        // at least here on HotSpot 1.4.2-b28 the limit is 66*4096=270336
        // weird, huh
        for ( int j = 0; j < str.length(); j += 4096 )
        {
          out.write (str, j, j + 4096 > str.length() ? str.length() - j : 4096);
        }
        out.flush();
      }
    }

  }

  private static void fillData (ParserData data, Start tree, final ResolveIds ids, final ResolveAstIds ast_ids,
      ResolveTransformIds transform_ids)
  {
    FillParserData fps = new FillParserData (data, ids);
    tree.apply (fps);

    if ( data.state_list.size() == 0 )
      data.state_list.add(new ParserData.LexerState("INITIAL", 0));
    data.package_name = ids.pkgName;
    data.inlined = processInlining;
  }
}

class FillParserData extends DepthFirstAdapter {
  private ParserData data;
  private ResolveIds ids;

  private Map terminal2index = new HashMap ();

  FillParserData (ParserData data, ResolveIds ids)
  {
    this.data = data;
    this.ids = ids;

    Symbol[] terminals = Symbol.terminals();
    for ( int i = 0; i < terminals.length - 1; i++ )
      terminal2index.put (terminals[i].name, new Integer(i));
    data.eof_index = terminals.length - 2;
  }

  ParserData.Token current_token;
  private int tid = 0;
  public void inATokenDef (ATokenDef node)
  {
    String name = node.getId().getText();
    String ename = "T" + ids.name(name);
    String text = null;

    ARegExp regExp = (ARegExp) node.getRegExp();

    java.util.List concats = regExp.getConcats();

    if(concats.size() == 1)
    {
      AConcat concat = (AConcat)concats.get(0);
      java.util.List unExps = concat.getUnExps();

      if(unExps.size() == 1)
      {
        AUnExp unExp = (AUnExp) unExps.get(0);

        PBasic basic = unExp.getBasic();

        if((basic instanceof AStringBasic) &&
            (unExp.getUnOp() == null))
        {
          text = ((AStringBasic) basic).getString().getText();
          text = text.substring(1, text.length() - 1);
        }
        else if((basic instanceof ACharBasic) &&
                (unExp.getUnOp() == null))
        {
          PChar pChar = ((ACharBasic) basic).getChar();

          if(pChar instanceof ACharChar)
          {
            text = ((ACharChar) pChar).getChar().getText();
            text = text.substring(1, text.length() - 1);
          }
        }
      }
    }

    int index = -1;
    Integer i = (Integer)terminal2index.get (ename);
    if ( i != null ) index = i.intValue();

    current_token = new ParserData.Token(name, ename, text, index);
    data.token_list.add(current_token);

    node.apply (new DepthFirstAdapter() {
      public void inAStateList (AStateList node)
      {
        String state = node.getId().getText().toUpperCase();
        String transition = state;
        if ( node.getTransition() != null )
            transition = ((ATransition)node.getTransition()).getId().getText().toUpperCase();
        current_token.transitions.put(state, transition);
      }

      public void inAStateListTail (AStateListTail node)
      {
        String state = node.getId().getText().toUpperCase();
        String transition = state;
        if ( node.getTransition() != null )
            transition = ((ATransition)node.getTransition()).getId().getText().toUpperCase();
        current_token.transitions.put(state, transition);
      }
    });

    tid++;
  }

  private ParserData.Production current_prod;

  public void inAStates (AStates node)
  {
    int id = 0;
    java.util.List l = (java.util.List)node.getListId();
    for ( Iterator i = l.iterator(); i.hasNext(); )
    {
      TId state = (TId)i.next();
      String name = state.getText().toUpperCase();
      data.state_list.add (new ParserData.LexerState (name, id));
      id++;
    }
  }

  public void inAAstProd (AAstProd node)
  {
    String name = node.getId().getText();
    String ename = "P" + ids.name(name);
    current_prod = new ParserData.Production(name, ename);
    data.production_list.add(current_prod);
  }

  private ParserData.Alt current_alt;
  public void inAAstAlt (AAstAlt node)
  {
    String name;
    String ename;

    if ( node.getAltName() != null )
    {
      name = node.getAltName().getText();
      ename = "A" + ids.name(name) + ids.name(current_prod.name);
    }
    else
    {
      name = current_prod.name;
      ename = "A" + ids.name(name);
    }

    current_alt = new ParserData.Alt (name, ename);
    current_prod.alts.add(current_alt);

    node.apply (new DepthFirstAdapter () {
      public void inAElem(AElem node)
      {
        String name;
        if ( node.getElemName () != null )
            name = node.getElemName().getText();
        else
            name = node.getId().getText();

        String ename = ids.name(name);

        String type = node.getId().getText();
        String etype = ids.name(type);

        boolean is_token = false;

        if(node.getSpecifier() != null)
        {
          if(node.getSpecifier() instanceof ATokenSpecifier)
          {
            is_token = true;
            etype = "T" + etype;
          }
          else
          {
            etype = "P" + etype;
          }
        }
        else
        {
          Object token = ids.tokens.get("T" + etype);

          if(token != null)
          {
            is_token = true;
            etype = "T" + etype;
          }
          else
          {
            etype = "P" + etype;
          }
         }

        String modifier = null;
        if ( node.getUnOp() != null ) {
          if ( node.getUnOp() instanceof AQMarkUnOp )
            modifier = "?";
          else if ( node.getUnOp() instanceof AStarUnOp )
            modifier = "*";
          else if ( node.getUnOp() instanceof APlusUnOp )
            modifier = "+";
          else
            System.err.println ("Unrecognized modifier class " + node.getUnOp().getClass().getName() +"!");
        }
        current_alt.elems.add (new ParserData.Elem (name, ename, is_token, type, etype, modifier));
      }
    });
  }
}

class DummyWriter extends Writer {
  DummyWriter (File file) throws IOException { }
  DummyWriter (String dummy_path) throws IOException { }
  public void close () throws IOException { }
  public void flush () throws IOException { }
  public void write (int c) throws IOException { }
  public void write (char[] buf) throws IOException { }
  public void write (String str) throws IOException { }
  public void write (char[] buf, int off, int len) throws IOException { }
}

class DummyOutputStream extends OutputStream
{
  DummyOutputStream (File file) throws IOException { }
  DummyOutputStream (String path) throws IOException { }
  public void close () throws IOException { }
  public void flush () throws IOException { }
  public void write (byte[] b) throws IOException { }
  public void write (byte[] b, int off, int len) throws IOException { }
  public void write (int b) throws IOException { }
}


