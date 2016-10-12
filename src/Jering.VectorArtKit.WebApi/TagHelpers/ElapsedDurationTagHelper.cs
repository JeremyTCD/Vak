using Jering.VectorArtKit.WebApi.Extensions;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;

namespace Jering.VectorArtKit.WebApi.TagHelpers
{
    [HtmlTargetElement(Attributes = _durationStartAttributeName)]
    public class ElapsedDurationTagHelper : TagHelper
    {
        private const string _durationStartAttributeName = "duration-start";

        /// <summary>
        /// Start of elapsed duration.
        /// </summary>
        [HtmlAttributeName(_durationStartAttributeName)]
        public DateTime DurationStart { get; set; }

        /// <summary>
        /// Modifies contents of span elements with the duration-start attribute.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.Content.SetContent((DateTime.UtcNow - DurationStart).ToElapsedDurationString());
        }
    }
}
