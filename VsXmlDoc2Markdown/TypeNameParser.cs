using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace VsXmlDoc2Markdown
{
    internal class TypeNameParser
    {
        internal AssemblyComponent Parse(AssemblyComponent assembly, string nameString)
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
                            generic += $",T{g + 1}";

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
            if (!parent.Children.ContainsKey(com.FullName))
                parent.Children.Add(com.FullName, com);
            return com;
        }
    }
}
