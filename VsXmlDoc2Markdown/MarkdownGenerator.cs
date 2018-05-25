using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace VsXmlDoc2Markdown
{
    public class MarkdownGenerator
    {
        public string ToMarkdown(string xml)
        {
            string result = "";

            using (MemoryStream stream = new MemoryStream())
            {
                using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(xml);
                }
            }

            return result;
        }
    }
}
