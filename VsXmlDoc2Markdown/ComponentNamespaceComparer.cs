﻿using System;
using System.Collections.Generic;
using System.Text;

namespace VsXmlDoc2Markdown
{
    internal class ComponentNamespaceComparer : IComparer<AssemblyComponent>
    {
        public int Compare(AssemblyComponent x, AssemblyComponent y)
        {
            if (x.ComponentType == ComponentType.Namespace && y.ComponentType != ComponentType.Namespace)
                return 1;
            else if (x.ComponentType != ComponentType.Namespace && y.ComponentType == ComponentType.Namespace)
                return -1;
            else if (x.ComponentType == ComponentType.Namespace && y.ComponentType == ComponentType.Namespace)
                return 0;

            return x.ShortName.CompareTo(y.ShortName);
        }
    }
}
