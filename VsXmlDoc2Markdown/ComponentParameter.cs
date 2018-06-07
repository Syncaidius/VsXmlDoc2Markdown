using System;
using System.Collections.Generic;
using System.Text;

namespace VsXmlDoc2Markdown
{
    public class ComponentParameter
    {
        /// <summary>
        /// Gets or sets the component which describes the parameter's type.
        /// </summary>
        public AssemblyComponent TypeComponent { get; set; }

        /// <summary>
        /// Gets or sets the parameter's name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the parameter's summary text.
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// Returns the name of the parameter, as a string. If no name was defined, it's component name will be returned instead.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name ?? (TypeComponent?.FullName ?? "");
        }
    }
}
