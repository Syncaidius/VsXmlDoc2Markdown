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
            List<MarkdownResult> results = gen.ToMarkdown(xml);

            foreach(MarkdownResult result in results)
            {
                string path = $"docs/{result.Path}";
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                using (FileStream stream = new FileStream($"{path}/{result.Title}.md", FileMode.Create, FileAccess.Write))
                {
                    using (StreamWriter writer = new StreamWriter(stream))
                        writer.Write(result.Markdown);
                }
            }
        }
    }
}
