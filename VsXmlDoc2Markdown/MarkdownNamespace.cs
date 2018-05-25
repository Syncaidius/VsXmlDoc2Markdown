using System;
using System.Collections.Generic;
using System.Text;

namespace VsXmlDoc2Markdown
{
    public class MarkdownNamespace
    {
        List<MarkdownType> _types;

        public MarkdownNamespace()
        {
            _types = new List<MarkdownType>();
            Parameters = _types.AsReadOnly();
        }

        /// <summary>
        /// Gets the list of types that are part of the current <see cref="MarkdownNamespace"/>.
        /// </summary>
        public IReadOnlyList<MarkdownType> Parameters { get; private set; }
    }
}
