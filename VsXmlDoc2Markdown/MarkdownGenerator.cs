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
            if (component.Parent != null && component.ComponentType == ComponentType.Namespace && component.Parent.ComponentType == ComponentType.Namespace)
            {
                writer.Write($".{component.Name}  ");
            }
            else
            {
                string indent = "";
                writer.Write("  " + Environment.NewLine);
                for (int i = 0; i < depth; i++)
                    indent += "    ";

                writer.Write($"{(depth > 0 ? indent : "#")} {component.Name}");
                depth++;
            }

            
            foreach (string name in component.Children.Keys)
                WriteMarkdown(component.Children[name], writer, depth);
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
                if (node.Name == "#comment")
                    continue;

                // Parts: 0 = member type, 1 = namespace and name.
                string fullName = node.Attributes["name"].Value;
                Match methodParams = Regex.Match(fullName, @"\((.*?)\)");
                if (methodParams.Success)
                    fullName = fullName.Replace(methodParams.Value, "");

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
                    string nsName = nsParts[i];
                    AssemblyComponent nsCom;

                    if (!parent.Children.TryGetValue(nsName, out nsCom))
                    {
                        nsCom = new AssemblyComponent(ComponentType.Namespace, nsName);
                        nsCom.Parent = parent;
                        parent.Children.Add(nsName, nsCom);
                    }

                    parent = nsCom;
                }

                AssemblyComponent com = new AssemblyComponent(ComponentType.Namespace, nsParts[typeNameID]);
                com.Parent = parent;

                switch (nameParts[0])
                {
                    case "F": // Field
                        parent.ComponentType = ComponentType.Type;
                        com.ComponentType = ComponentType.Field;
                        break;

                    case "T": // Type:
                        com.ComponentType = ComponentType.Type;
                        break;

                    case "M": // Method
                        parent.ComponentType = ComponentType.Type;
                        com.Name += methodParams.Success ? methodParams.Value : "";
                        com.ComponentType = ComponentType.Type;
                        break;

                    case "P": // Property
                        parent.ComponentType = ComponentType.Type;
                        com.ComponentType = ComponentType.Property;
                        break;

                    case "E": // Event
                        parent.ComponentType = ComponentType.Type;
                        com.ComponentType = ComponentType.Event;
                        break;
                }

                parent.Children.Add(com.Name, com);
            }
        }
    }
}
