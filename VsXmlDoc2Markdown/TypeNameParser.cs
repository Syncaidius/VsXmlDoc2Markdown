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
            AssemblyComponent com = new AssemblyComponent(ComponentType.Namespace);

            ParseGenericParameters(com, ref nameString);
            ParseMethodParameters(com, ref nameString);
            ParseIndexerParameters(com, ref nameString); 

            string[] nameParts = nameString.Split(":");
            string[] tnParts = nameParts[1].Split("~");
            string[] nsParts = tnParts[0].Split(".");
            int typeNameID = nsParts.Length - 1;
            string returnType = tnParts.Length > 1 ? tnParts[1] : "";

            int i = 0;
            if (nsParts[0] == assembly.ShortName)
                i++;

            // Add or locate namespace components
            AssemblyComponent parent = assembly;
            for (; i < typeNameID; i++)
            {
                AssemblyComponent newParent = parent[nsParts[i]];
                if (parent != assembly)
                    newParent.ParentNamespace = parent.ParentNamespace != null ? $"{parent.ParentNamespace}.{parent.ShortName}" : parent.ShortName;

                parent = newParent;
            }

            if (parent.ComponentType == ComponentType.OperatorMethod)
                return null;

            com.ShortName = nsParts[typeNameID];
            com.Parent = parent;
            com.ReturnType = returnType;
            com.ParentNamespace = $"{parent.ParentNamespace}.{parent.ShortName}";

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
                    if (com.InputParameters.Count == 0)
                        com.ComponentType = ComponentType.Property;
                    else
                        com.ComponentType = ComponentType.IndexerProperty;
                    break;

                case "E": // Event
                    parent.ComponentType = ComponentType.Type;
                    com.ComponentType = ComponentType.Event;
                    break;
            }

            // TODO improve this. Do not create a new component
            if (!parent.Children.ContainsKey(com.Definition))
                parent.Children.Add(com.Definition, com);

            return com;
        }

        private void ParseGenericParameters(AssemblyComponent com, ref string nameString)
        {
            Match genericParams = Regex.Match(nameString, "(`+[0-9])");
            if (genericParams.Success)
            {
                int genericCount = 0;
                string generic = "";

                if (int.TryParse(genericParams.Value.Replace("`", ""), out genericCount))
                {
                    if (genericCount > 1)
                    {
                        for (int i = 0; i < genericCount; i++)
                            com.GenericParameters.Add(new ComponentParameter()
                            {
                                Name = $"T{i + 1}",
                            });
                    }
                    else
                    {
                        com.GenericParameters.Add(new ComponentParameter()
                        {
                            Name = "T"
                        });
                    }
                }

                nameString = nameString.Replace(genericParams.Value, generic);
            }
        }

        private void ParseMethodParameters(AssemblyComponent com, ref string nameString)
        {
            Match methodParams = Regex.Match(nameString, @"\((.*?)\)");
            if (methodParams.Success)
            {
                nameString = nameString.Replace(methodParams.Value, "");
                string[] parameters = methodParams.Value.Replace("(", "").Replace(")", "").Split(",");
                for(int i = 0; i < parameters.Length; i++)
                {
                    com.InputParameters.Add(new ComponentParameter()
                    {
                        Name = parameters[i],
                    });
                }
            }
        }

        private void ParseIndexerParameters(AssemblyComponent com, ref string nameString)
        {
            Match indexerParams = Regex.Match(nameString, @"\[(.*?)\]");
            if (indexerParams.Success)
            {
                nameString = nameString.Replace(indexerParams.Value, "");
            }
        }
    }
}
