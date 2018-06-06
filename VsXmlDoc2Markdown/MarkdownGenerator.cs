using System;
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

        ComponentNamespaceComparer _namespaceComparer = new ComponentNamespaceComparer();

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
                    string fn = SanitizeFilename(component.Name);
                    writer.Write($"{(depth > 0 ? indent : "#")} [{component.Name}]({path}/{fn}.md)");
                }

                depth++;
            }

            List<AssemblyComponent> children = component.Children.Values.ToList();
            children.Sort(_namespaceComparer);

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
                AssemblyComponent com = ParseComponent(assembly, node.Attributes["name"].Value);
                if (com == null)
                    continue;

                ParseSummary(assembly, com, node);
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

            string fn = SanitizeFilename(typeComponent.Name);
            using (FileStream stream = new FileStream($"{fullDir}/{fn}.md", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
                {
                    writer.WriteLine($"# {typeComponent.ParentNamespace}.{typeComponent.Name}");
                    writer.WriteLine($"{typeComponent.Summary}");
                }
            }
        }

        private void ParseSummary(AssemblyComponent assembly, AssemblyComponent component, XmlNode node)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                string nName = child.Name.ToLower();
                switch (nName) {
                    case "summary":
                        component.Summary = ParseSummaryText(assembly, child.InnerXml);
                        break;
                }
            }
        }

        private string ParseSummaryText(AssemblyComponent assembly, string innerXml)
        {
            string result = innerXml;
            Match xmlMatch = Regex.Match(result, @"(<.*?>.*</.*?>)|(<.*?/>)");
            while (xmlMatch.Success)
            {
                XmlDocument inlineDoc = new XmlDocument();
                inlineDoc.LoadXml(xmlMatch.Value);

                foreach (XmlNode child in inlineDoc.ChildNodes)
                {
                   foreach(XmlAttribute att in child.Attributes)
                    {
                        string attName = att.Name.ToLower();
                        switch(attName)
                        {
                            case "cref":
                                AssemblyComponent com = ParseComponent(assembly, att.Value);
                                result = result.Replace(xmlMatch.Value, com.QualifiedName);
                                break; 
                        }
                    }
                }

                xmlMatch = xmlMatch.NextMatch();
            }

            return result;
        }

        private string SanitizeFilename(string fn)
        {
            return Regex.Replace(fn, @"<|>|\[|\]|(&gt;)|(&lt;)", "_");
        }

        private AssemblyComponent ParseComponent(AssemblyComponent assembly, string nameString)
        {
            Match methodParams = Regex.Match(nameString, @"\((.*?)\)");
            if (methodParams.Success)
                nameString = nameString.Replace(methodParams.Value, "");

            Match genericParams = Regex.Match(nameString, "(`+[0-9])");
            if (genericParams.Success)
            {
                int genericCount = 0;
                string generic = "";

                if (int.TryParse(genericParams.Value.Replace("`", ""), out genericCount))
                {
                    if (genericCount > 1)
                    {
                        generic = "&lt;T1";
                        for (int g = 1; g < genericCount; g++)
                            generic += $",T{g+1}";

                        generic += "&gt;";
                    }
                    else
                    {
                        generic = "&lt;T&gt;";
                    }
                }

                nameString = nameString.Replace(genericParams.Value, generic);
            }

            string[] nameParts = nameString.Split(":");

            string[] tnParts = nameParts[1].Split("~");
            string[] nsParts = tnParts[0].Split(".");
            int typeNameID = nsParts.Length - 1;
            string returnType = tnParts.Length > 1 ? tnParts[1] : "";

            int i = 0;
            if (nsParts[0] == assembly.Name)
                i++;

            // Add or locate namespace components
            AssemblyComponent parent = assembly;
            string parentNamespace = "";
            bool first = true;
            for (; i < typeNameID; i++)
            {
                parent = parent[nsParts[i]];
                parentNamespace += first ? parent.Name : $".{parent.Name}";
                first = false;
            }

            if (parent.ComponentType == ComponentType.OperatorMethod)
                return null;

            AssemblyComponent com = new AssemblyComponent(ComponentType.Namespace, nsParts[typeNameID]);
            com.Parent = parent;
            com.Parameters = methodParams.Success ? methodParams.Value : "";
            com.ReturnType = returnType;
            com.ParentNamespace = parentNamespace;

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

            // TODO improve this. Do not create a new component
            if(!parent.Children.ContainsKey(com.FullName))
                parent.Children.Add(com.FullName, com);
            return com;
        }
    }
}
