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
        /// Gets or sets the component name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the parameter information.
        /// </summary>
        public string Parameters { get; set; }

        /// <summary>
        /// Gets or sets the return type.
        /// </summary>
        public string ReturnType { get; set; }

        /// <summary>
        /// Gets the full name of the component, which includes any generic or method paramters it may have.
        /// </summary>
        public string FullName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(ReturnType))
                    return $"{Name}{Parameters}";
                else
                    return $"{Name}{Parameters} [{ReturnType}]";
            }
        }

        /// <summary>
        /// Gets or sets the component summary.
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// Creates a new instance of <see cref="AssemblyComponent"/>.
        /// </summary>
        /// <param name="type">The type of the component.</param>
        /// <param name="name">The name of the component.</param>
        public AssemblyComponent(ComponentType type, string name)
        {
            Children = new Dictionary<string, AssemblyComponent>();
            ComponentType = type;
            Name = name;
        }

        /// <summary>
        /// Returns the name and component type as a formatted string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{Name}{Parameters} - {ComponentType}";
        }
    }
}
