using Apps.Confluence.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;

namespace Apps.Confluence.Models.Requests.Properties
{
    public class SetPropertyRequest : PropertyIdentifier
    {
        [Display("Property value (JSON)")]
        public object Value { get; set; }
    }
}
