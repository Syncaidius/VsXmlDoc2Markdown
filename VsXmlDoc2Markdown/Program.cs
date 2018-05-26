using System;
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

            MarkdownGenerator gen = new MarkdownGenerator();

            Console.ReadKey();
        }
    }
}
