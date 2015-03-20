using System.Reflection;

namespace Silverpop.Core
{
    public static class Constants
    {
        public const BindingFlags DefaultPersonalizationTagsPropertyReflectionBindingFlags =
            BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance;
    }
}