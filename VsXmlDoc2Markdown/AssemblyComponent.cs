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
        /// Gets or sets the component name.
        /// </summary>
        public string Name { get; set; }

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
            return $"{Name} - {ComponentType}";
        }
    }
}
