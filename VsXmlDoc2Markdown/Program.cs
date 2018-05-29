using System;
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

            string fn = args.Length > 0 ? args[1] ?? null : null;
            string xml = "";

            if (fn == null)
                Console.WriteLine("No XML file was specified.");

            // NOTE: FOR TESTING ONLY. UNCOMMENT LINES BELOW TO TEST.
            using (FileStream stream = new FileStream("Molten.Render.xml", FileMode.Open, FileAccess.Read))
            {
                using (StreamReader reader = new StreamReader(stream))
                    xml = reader.ReadToEnd();
            }

            MarkdownGenerator gen = new MarkdownGenerator();
            string md = gen.ToMarkdown(xml);


            using (FileStream stream = new FileStream("test.md", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                    writer.Write(md);
            }
        }
    }
}
