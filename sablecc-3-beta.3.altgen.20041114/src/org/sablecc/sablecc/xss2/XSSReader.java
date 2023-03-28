
package org.sablecc.sablecc.xss2;

import java.io.*;
import java.util.*;

class XSSReader extends CharArrayReader {
  public XSSReader (Reader reader) throws IOException
  {
    super(new char[] { });

    CharArrayWriter out = new CharArrayWriter ();
    BufferedReader in = new BufferedReader (reader);

    String stag = "[-"; 
    String etag = "-]";

    String line;
    while ( ( line = in.readLine() ) != null ) {
        if ( line.equals ("$") ) {
            out.write (stag + "\n" + etag);
            continue;
        }

        if ( line.startsWith("$ ") ) {
            out.write (stag + line.substring(2) + "\n" + etag);
            continue;
        }

        out.write(line);
        out.write("\n");
    }

    buf = out.toCharArray();
    count = out.size();
    markedPos = pos = 0;
  }
}

