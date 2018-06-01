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

            string sourceFilename = args.Length > 0 ? args[1] : null;
            string directory = args.Length > 1 ? args[2] : "docs/";
            string xml = "";

            if (sourceFilename == null)
                Console.WriteLine("No XML file was specified.");

            // NOTE: FOR TESTING ONLY. UNCOMMENT LINES BELOW TO TEST.
            sourceFilename = sourceFilename ?? "VsXmlDoc2Markdown.xml";

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
