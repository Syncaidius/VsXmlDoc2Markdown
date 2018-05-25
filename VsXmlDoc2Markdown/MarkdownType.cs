using System;
using System.Collections.Generic;
using System.Text;

namespace VsXmlDoc2Markdown
{
    public class MarkdownType
    {
        List<MarkdownMember> _members;

        internal MarkdownType()
        {
            _members = new List<MarkdownMember>();
            Members = _members.AsReadOnly();
        }

        public IReadOnlyList<MarkdownMember> Members { get; private set; }
    }
}
