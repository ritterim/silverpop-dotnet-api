using System.Reflection;

namespace Silverpop.Core.Internal
{
    public static class Constants
    {
        public const BindingFlags DefaultPersonalizationTagsPropertyReflectionBindingFlags =
            BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance;

        public const TransactMessageRecipientBodyType TransactMessageBodyTypeDefault =
            TransactMessageRecipientBodyType.Html;
    }
}