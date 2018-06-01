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
        /// Generates Markdown file from Visual Studio XML documentation.
        /// </summary>
        /// <param name="path">The path to store the markdown files.</param>
        /// <param name="xml">The xml documentation string.</param>
        /// <returns></returns>
        public void ToMarkdown(string path, string xml)
        {
            AssemblyComponent assembly = ParseAssembly(ref xml);

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            using (FileStream stream = new FileStream($"{path}/{assembly.Name}.md", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
                {
                    GenerateIndex(assembly, writer, 0, "");
                }
            }

            // Produce individual markdown page results for all types within the assembly.
            GeneratePages(path, assembly);
        }

        private void GeneratePages(string path, AssemblyComponent component)
        {
            if (component.ComponentType == ComponentType.Type)
                GenerateTypePage(path, component);

            foreach (AssemblyComponent child in component.Children.Values)
                GeneratePages(path, child);
        }

        private void GenerateIndex(AssemblyComponent component, StreamWriter writer, int depth, string path)
        {
            if (component.Parent != null && component.ComponentType == ComponentType.Namespace && component.Parent.ComponentType == ComponentType.Namespace)
            {
                writer.Write($".{component.Name}  ");
            }
            else
            {
                string indent = "";
                writer.Write("  " + Environment.NewLine);

                if (depth > 0)
                {
                    for (int i = 0; i < depth - 1; i++)
                        indent += "    ";
                    indent += "- ";
                }

                string derp = "";
                if(!string.IsNullOrEmpty(path) && component.ComponentType == ComponentType.Type)
                    derp = $"{(depth > 0 ? indent : "#")} [{component.Name}]({path}/{component.Name}.md)";
                else
                    derp = ($"{(depth > 0 ? indent : "#")} {component.Name}");

                writer.Write(derp);
                depth++;
            }

            
            foreach (string name in component.Children.Keys)
                GenerateIndex(component.Children[name], writer, depth, $"{path}/{component.Name}");
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
                        com.Parameters = methodParams.Success ? methodParams.Value : "";
                        com.ComponentType = ComponentType.Method;
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

                parent.Children.Add(com.FullName, com);
            }
        }

        private void GenerateTypePage(string path, AssemblyComponent typeComponent)
        {
            string directory = "";

            AssemblyComponent parent = typeComponent.Parent;
            while(parent != null)
            {
                directory = $"{parent.Name}/{directory}";
                parent = parent.Parent;
            }

            string fullDir = $"{path}/{directory}";
            if (!Directory.Exists(fullDir))
                Directory.CreateDirectory(fullDir);

            using (FileStream stream = new FileStream($"{fullDir}/{typeComponent.Name}.md", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
                {
                   
                }
            }
        }
    }
}
