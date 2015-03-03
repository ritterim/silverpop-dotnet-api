namespace Silverpop.Core
{
    public class TransactMessageRecipientPersonalizationTag
    {
        public TransactMessageRecipientPersonalizationTag()
        {
        }

        public TransactMessageRecipientPersonalizationTag(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; set; }

        public string Value { get; set; }
    }
}