﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Linq;

namespace VsXmlDoc2Markdown
{
    /// <summary>
    /// A class for generating Markdown from Visual Studio XML documentation.
    /// </summary>
    public class MarkdownGenerator
    {
        AssemblyComponentComparer _componentComparer = new AssemblyComponentComparer();

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
                    writer.Write($"# {assembly.Name}");
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
            if (component.Parent != null && component.ComponentType == ComponentType.Namespace)
            {
                string ns = component.Name;

                // Get full namespace by travelling backup the tree.
                AssemblyComponent p = component.Parent;
                while (p != null && p.ComponentType == ComponentType.Namespace)
                {
                    ns = $"{p.Name}.{ns}";
                    p = p.Parent;
                }

                depth = 1;
                writer.Write("  " + Environment.NewLine);
                writer.Write($"* {ns}");
            }
            else
            {
                if (!string.IsNullOrEmpty(path) && component.ComponentType == ComponentType.Type)
                {
                    string indent = "";

                    if (depth > 0)
                    {
                        for (int i = 0; i < depth - 1; i++)
                            indent += "    ";
                        indent += "* ";
                    }

                    writer.Write("  " + Environment.NewLine);
                    writer.Write($"{(depth > 0 ? indent : "#")} [{component.Name}]({path}/{component.Name}.md)");
                }

                depth++;
            }

            List<AssemblyComponent> children = component.Children.Values.ToList();
            children.Sort(_componentComparer);

            foreach (AssemblyComponent child in children)
                GenerateIndex(child, writer, depth + 1, $"{path}/{component.Name}");
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

                Match genericParams = Regex.Match(fullName, "(`+[0-9])");
                if (genericParams.Success)
                {
                    int genericCount = 0;
                    string generic = "";

                    if (int.TryParse(genericParams.Value.Replace("`", ""), out genericCount))
                    {
                        if (genericCount > 1)
                        {
                            generic = "&lt;T0";
                            for (int g = 1; g < genericCount; g++)
                                generic += $",T{g}";

                            generic += "&gt;";
                        }
                        else
                        {
                            generic = "&lt;T&gt;";
                        }
                    }
                    
                    fullName = fullName.Replace(genericParams.Value, generic);
                }

                string[] nameParts = fullName.Split(":");

                string[] tnParts = nameParts[1].Split("~");
                string[] nsParts = tnParts[0].Split(".");
                int typeNameID = nsParts.Length - 1;
                string returnType = tnParts.Length > 1 ? tnParts[1] : "";

                int i = 0;
                if (nsParts[0] == assembly.Name)
                    i++;

                // Add or locate namespace components
                AssemblyComponent parent = assembly;
                for(; i < typeNameID; i++)
                    parent = parent[nsParts[i]];

                if (parent.ComponentType == ComponentType.OperatorMethod)
                    continue;

                AssemblyComponent com = new AssemblyComponent(ComponentType.Namespace, nsParts[typeNameID]);
                com.Parent = parent;
                com.Parameters = methodParams.Success ? methodParams.Value : "";
                com.ReturnType = returnType;

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

                        com.ComponentType = ComponentType.Method;
                        break;

                    case "P": // Property
                        parent.ComponentType = ComponentType.Type;

                        if (!string.IsNullOrWhiteSpace(com.Parameters))
                            com.ComponentType = ComponentType.IndexerProperty;
                        else
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

            string fn = Regex.Replace(typeComponent.Name, @"<|>|\[|\]|(&gt;)|(&lt;)", "_");
            using (FileStream stream = new FileStream($"{fullDir}/{fn}.md", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
                {
                   
                }
            }
        }
    }
}
