using System;

namespace Silverpop.Core
{
    [AttributeUsage(AttributeTargets.Property)]
    public class SilverpopPersonalizationTag : Attribute
    {
        public SilverpopPersonalizationTag(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }
}