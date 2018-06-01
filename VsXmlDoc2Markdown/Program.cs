using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace VsXmlDoc2Markdown
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Args: ");
            foreach (string s in args)
                Console.WriteLine(s);

            string sourceFilename = args.Length > 0 ? args[0] : null;
            string directory = args.Length > 1 ? args[1] : "docs";
            string xml = "";

            // Chop the / off the end, if found. It's not needed, but will cause issues on some platforms due to producing "//" during path concatenation.
            if (directory.EndsWith('/'))
                directory = directory.Substring(0, directory.Length - 1);
            
            if (sourceFilename == null)
                Console.WriteLine("No XML file was specified.");

            // NOTE: FOR TESTING ONLY. UNCOMMENT LINES BELOW TO TEST.
            sourceFilename = sourceFilename ?? "Molten.Math.xml";

            using (FileStream stream = new FileStream(sourceFilename, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader reader = new StreamReader(stream))
                    xml = reader.ReadToEnd();
            }

            MarkdownGenerator gen = new MarkdownGenerator();
            gen.ToMarkdown(directory, xml);
        }
    }
}
