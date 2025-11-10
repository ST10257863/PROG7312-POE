using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Municipality_Application.TagHelpers
{
    [HtmlTargetElement("google-maps-script")]
    public class GoogleMapsScriptTagHelper : TagHelper
    {
        private readonly IConfiguration _config;
        public GoogleMapsScriptTagHelper(IConfiguration config) => _config = config;

        public string Callback { get; set; } = "";

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var apiKey = _config["ApiKeys:GoogleMaps"];
            output.TagName = "script";
            output.Attributes.SetAttribute("src", $"https://maps.googleapis.com/maps/api/js?key={apiKey}&libraries=places&callback={Callback}");
            output.Attributes.SetAttribute("async", "async");
            output.Attributes.SetAttribute("defer", "defer");
            output.TagMode = TagMode.StartTagAndEndTag;
        }
    }
}