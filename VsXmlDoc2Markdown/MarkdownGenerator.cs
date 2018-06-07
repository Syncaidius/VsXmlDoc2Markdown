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
        TypeNameParser _nameParser = new TypeNameParser();

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
                    GenerateIndexPage(assembly, writer, 0, path);
                }
            }
        }

        private void GenerateIndexPage(AssemblyComponent component, StreamWriter writer, int depth, string path)
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
                    for (int i = 0; i < depth - 1; i++)
                        indent += "    ";
                    indent += "* ";

                    writer.Write("  " + Environment.NewLine);
                    string fn = SanitizeFilename(component.Name);
                    writer.Write($"{indent} [{component.Name}]({path}/{fn}.md)");
                    GenerateTypePage(path, component);
                }

                depth++;
            }

            List<AssemblyComponent> children = component.Children.Values.ToList();
            children.Sort(_namespaceComparer);

            foreach (AssemblyComponent child in children)
                GenerateIndexPage(child, writer, depth + 1, $"{path}/{component.Name}");
        }

        private void GenerateTypeIndex(AssemblyComponent component, StreamWriter writer, int depth, string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                string indent = "";

                if (depth > 0)
                {
                    for (int i = 0; i < depth - 1; i++)
                        indent += "    ";
                    indent += "* ";
                }

                writer.Write("  " + Environment.NewLine);
                string fn = SanitizeFilename($"{path}/{component.Name}");

                if (depth > 0)
                {
                    writer.Write($"{indent} [{component.FullName}]({fn}.md)");
                }
                else
                {
                    writer.WriteLine($"# {path}.{component.Name}");
                    writer.WriteLine($"{component.Summary}");
                }
            }

            List<AssemblyComponent> children = component.Children.Values.ToList();
            children.Sort(_namespaceComparer);

            foreach (AssemblyComponent child in children)
                GenerateTypeIndex(child, writer, depth + 1, $"{path}/{component.Name}");
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
                AssemblyComponent com = _nameParser.Parse(assembly, node.Attributes["name"].Value);
                if (com == null)
                    continue;

                ParseSummary(assembly, com, node);
            }
        }

        private void GenerateTypePage(string path, AssemblyComponent typeComponent)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            string fn = SanitizeFilename(typeComponent.Name);
            using (FileStream stream = new FileStream($"{path}/{fn}.md", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
                {
                    GenerateTypeIndex(typeComponent, writer, 0, path);
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
                                AssemblyComponent com = _nameParser.Parse(assembly, att.Value);
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
    }
}
