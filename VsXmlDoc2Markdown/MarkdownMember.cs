using System;
using System.Collections.Generic;
using System.Text;

namespace VsXmlDoc2Markdown
{
    public class MarkdownMember
    {
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the member's
        /// </summary>
        public MarkdownMemberType Type { get; internal set; }

        /// <summary>
        /// Gets the member's documentation summary.
        /// </summary>
        public string Summary { get; internal set; }
    }
}
