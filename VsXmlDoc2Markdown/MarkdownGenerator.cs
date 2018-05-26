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

                }
            }

            return result;
        }

        private MarkdownAssembly ParseAssembly(ref string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            MarkdownAssembly assembly = new MarkdownAssembly();
            foreach (XmlNode root in doc.ChildNodes)
            {
                switch (root.Name.ToLower())
                {
                    case "assembly":
                        ParseAssemblyNode(assembly, root);
                        break;

                    case "members":
                        ParseMembers(assembly, membersNode);
                        break;
                }
            }

            return assembly;
        }

        private void ParseAssemblyNode(MarkdownAssembly assembly, XmlNode assemblyNode)
        {
            foreach (XmlNode node in assemblyNode.ChildNodes)
            {
                switch (node.Name.ToLower())
                {
                    case "name":
                        assembly.Name = node.InnerText;
                        break;
                }
            }
        }

        private void ParseMembers(MarkdownAssembly assembly, XmlNode membersNode)
        {
            foreach(XmlNode node in membersNode.ChildNodes)
            {
                // Parts: 0 = member type, 1 = namespace and name.
                string[] nameParts = node.Attributes["name"].Value.Split(":");
                string[] namespaceParts = nameParts[1].Split(".");
                int lastNsPart = namespaceParts.Length - 1;

                switch (nameParts[0])
                {
                    case "F": // Field

                        break;

                    case "T": // Type:

                        break;

                    case "M": // Method

                        break;

                    case "P": // Property

                        break;

                    case "E": // Event

                        break;
                }
            }
        }
    }
}
