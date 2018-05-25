using System;
using System.Collections.Generic;
using System.Text;

namespace VsXmlDoc2Markdown
{
    public class MarkdownMethod : MarkdownMember
    {
        List<MarkdownMethodParameter> _parameters;

        public MarkdownMethod()
        {
            _parameters = new List<MarkdownMethodParameter>();
            Parameters = _parameters.AsReadOnly();
        }

        /// <summary>
        /// Gets the list of parameters the documented <see cref="MarkdownMethod"/> expects.
        /// </summary>
        public IReadOnlyList<MarkdownMethodParameter> Parameters { get; private set; }
    }
}
