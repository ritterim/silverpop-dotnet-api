using FluentValidation;
using Silverpop.Client.WebTester.Models;

namespace Silverpop.Client.WebTester.Validation
{
    public class IndexModelValidator : AbstractValidator<SendModel>
    {
        public IndexModelValidator()
        {
            RuleFor(x => x.CampaignId).NotEmpty();
            RuleFor(x => x.ToAddress).NotEmpty();
            RuleFor(x => x.PersonalizationTags).NotNull();
        }
    }
}