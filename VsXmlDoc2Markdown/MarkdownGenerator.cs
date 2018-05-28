using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace VsXmlDoc2Markdown
{
    /// <summary>
    /// A class for generating Markdown from Visual Studio XML documentation.
    /// </summary>
    public class MarkdownGenerator
    {
        /// <summary>
        /// Generates Markdown from Visual Studio XML documentation.
        /// </summary>
        /// <param name="xml">The xml documentation string.</param>
        /// <returns></returns>
        public string ToMarkdown(string xml)
        {
            string result = "";
            AssemblyComponent assembly = ParseAssembly(ref xml);

            using (MemoryStream stream = new MemoryStream())
            {
                using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
                {
                    WriteMarkdown(assembly, writer, 0);
                }

                stream.Flush();
                result = Encoding.UTF8.GetString(stream.ToArray());
            }

            return result;
        }

        private void WriteMarkdown(AssemblyComponent component, StreamWriter writer, int depth)
        {
            writer.WriteLine($"{(depth > 0 ? new string('*', depth) : "#")} {component.Name}  ");
            foreach (string name in component.Children.Keys)
                WriteMarkdown(component.Children[name], writer, depth + 1);
        }

        private AssemblyComponent ParseAssembly(ref string xml)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            AssemblyComponent assembly = new AssemblyComponent(ComponentType.Assembly, "");
            XmlNode root = xmlDoc["doc"];

            foreach (XmlNode node in root.ChildNodes)
            {
                switch (node.Name.ToLower())
                {
                    case "assembly":
                        ParseAssemblyNode(assembly, node);
                        break;

                    case "members":
                        ParseMembers(assembly, node);
                        break;
                }
            }

            return assembly;
        }

        private void ParseAssemblyNode(AssemblyComponent assembly, XmlNode assemblyNode)
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

        private void ParseMembers(AssemblyComponent assembly, XmlNode membersNode)
        {
            foreach(XmlNode node in membersNode.ChildNodes)
            {
                // Parts: 0 = member type, 1 = namespace and name.
                string fullName = node.Attributes["name"].Value;
                Match m = Regex.Match(fullName, @"\((.*?)\)");
                if (m.Success)
                    fullName = fullName.Replace(m.Value, "");

                string[] nameParts = fullName.Split(":");
                string[] nsParts = nameParts[1].Split(".");
                int typeNameID = nsParts.Length - 1;

                int i = 0;
                if (nsParts[0] == assembly.Name)
                    i++;

                // Add or locate namespace components
                AssemblyComponent parent = assembly;
                for(; i < typeNameID; i++)
                {
                    string name = nsParts[i];
                    AssemblyComponent com;

                    if (!parent.Children.TryGetValue(name, out com))
                    {
                        com = new AssemblyComponent(ComponentType.Namespace, name);
                        parent.Children.Add(name, com);
                    }

                    parent = com;
                }

                switch (nameParts[0])
                {
                    case "F": // Field
                        // We now know it's parent is a type.
                        parent.ComponentType = ComponentType.Type;

                        break;

                    case "T": // Type:

                        break;

                    case "M": // Method
                        // We now know it's parent is a type.
                        parent.ComponentType = ComponentType.Type;

                        break;

                    case "P": // Property
                        // We now know it's parent is a type.
                        parent.ComponentType = ComponentType.Type;
                        break;

                    case "E": // Event

                        break;
                }
            }
        }
    }
}
