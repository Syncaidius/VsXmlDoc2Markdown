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
            string directory = "docs";
            string xml = "";

            // NOTE: For testing only.
            if(args.Length == 0)
            {
                Array.Resize(ref args, 1);
                args[0] = "Molten.Math.xml";
            }

            foreach (string fn in args)
            {
                if(File.Exists(fn))
                    Console.WriteLine($"Checking file...{fn}");
                else
                    Console.WriteLine("No XML file was specified.");


                // Chop the / off the end, if found. It's not needed, but will cause issues on some platforms due to producing "//" during path concatenation.
                if (directory.EndsWith('/'))
                    directory = directory.Substring(0, directory.Length - 1);

                using (FileStream stream = new FileStream(fn, FileMode.Open, FileAccess.Read))
                {
                    using (StreamReader reader = new StreamReader(stream))
                        xml = reader.ReadToEnd();
                }

                MarkdownGenerator gen = new MarkdownGenerator();
                gen.ToMarkdown(directory, xml);
            }
        }
    }
}
