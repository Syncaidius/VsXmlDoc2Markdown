using System;
using System.Collections.Generic;
using System.Text;

namespace VsXmlDoc2Markdown
{
    public class AssemblyComponent
    {
        /// <summary>
        /// Gets a dictionary of child <see cref="AssemblyComponent"/>.
        /// </summary>
        public Dictionary<string, AssemblyComponent> Children { get; set; }

        /// <summary>
        /// Gets or sets teh component type.
        /// </summary>
        public ComponentType ComponentType { get; set; }

        /// <summary>
        /// Gets or sets the parent component.
        /// </summary>
        public AssemblyComponent Parent { get; set; }

        /// <summary>
        /// Gets or sets the component name, without any parameters or parenthesis attached.
        /// </summary>
        public string ShortName { get; set; } = "";

        /// <summary>
        /// Gets the component name, with any generic parameters attached.
        /// </summary>
        public string GenericName
        {
            get
            {
                string generics = ConcatParameters(GenericParameters, "&lt;", "&gt;", ",");
                return $"{ShortName}{generics}";
            }
        }

        /// <summary>
        /// Gets the full component name, with any parameters and parenthesis attached.
        /// </summary>
        public string FullName
        {
            get
            {
                string generics = ConcatParameters(GenericParameters, "&lt;", "&gt;", ",");
                string input = ConcatParameters(InputParameters, "(", ")", ",");
                return $"{ShortName}{generics}{input}";
            }
        }

        /// <summary>
        /// Gets or sets the parameter information.
        /// </summary>
        public List<ComponentParameter> InputParameters { get; } = new List<ComponentParameter>();

        /// <summary>
        /// Gets or sets the return type.
        /// </summary>
        public string ReturnType { get; set; }

        /// <summary>
        /// Gets or sets the parent namespace.
        /// </summary>
        public string ParentNamespace { get; set; }

        /// <summary>
        /// Gets a list of generic parameter names.
        /// </summary>
        public List<ComponentParameter> GenericParameters { get; } = new List<ComponentParameter>();

        /// <summary>
        /// Gets the full definition name of the component, which includes the return type and any parameters it may have.
        /// </summary>
        public string Definition
        {
            get
            {
                string generics = ConcatParameters(GenericParameters, "&lt;", "&gt;", ",");
                string input = ConcatParameters(InputParameters, "(", ")", ",");
                if (string.IsNullOrWhiteSpace(ReturnType))
                    return $"{ShortName}{generics}{input}";
                else
                    return $"{ShortName}{generics}{input} [{ReturnType}]";
            }
        }

        /// <summary>
        /// Gets the assembly-qualified name which includes the <see cref="ParentNamespace"/> and the <see cref="Definition"/>.
        /// </summary>
        public string QualifiedName => $"{ParentNamespace}.{Definition}";

        /// <summary>
        /// Gets or sets the component summary.
        /// </summary>
        public string Summary { get; set; } = "{{MISSING SUMMARY}}";

        /// <summary>
        /// Creates a new instance of <see cref="AssemblyComponent"/>.
        /// </summary>
        /// <param name="type">The type of the component.</param>
        /// <param name="name">The name of the component.</param>
        public AssemblyComponent(ComponentType type)
        {
            Children = new Dictionary<string, AssemblyComponent>();
            ComponentType = type;
        }

        /// <summary>
        /// Returns the name and component type as a formatted string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{FullName} - {ComponentType}";
        }

        /// <summary>
        /// Gets a child component with the specified name. If the child does not exist, it will be created.
        /// </summary>
        /// <param name="childName">The name of the child component to be retrieved.</param>
        /// <returns>A <see cref="AssemblyComponent"/>.</returns>
        public AssemblyComponent this[string childName]
        {
            get
            {
                if (Children.TryGetValue(childName, out AssemblyComponent child))
                {
                    return child;
                }
                else
                {
                    child = new AssemblyComponent(ComponentType.Namespace);
                    child.ShortName = childName;
                    child.Parent = this;
                    Children.Add(childName, child);
                    return child;
                }
            }
        }

        private static string ConcatParameters(List<ComponentParameter> parameters, string leftParenthesis, string rightParenthesis, string separator)
        {
            string result = "";

            if (parameters.Count > 0)
            {
                bool first = true;
                foreach (ComponentParameter g in parameters)
                {
                    result += first ? $"{leftParenthesis}{g}" : $"{separator}{g}";
                    first = false;
                }

                result += $"{rightParenthesis}";
            }

            return result;
        }
    }
}
