using System;
using System.Collections.Generic;
using System.Text;

namespace VsXmlDoc2Markdown
{
    public class MarkdownResult
    {
        /// <summary>
        /// Gets the markdown string.
        /// </summary>
        public string Markdown { get; internal set; }

        /// <summary>
        /// Gets the title of the markdown file or block.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the markdown path.
        /// </summary>
        public string Path { get; set; }
    }
}
