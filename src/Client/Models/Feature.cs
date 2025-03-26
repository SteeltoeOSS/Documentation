using Microsoft.AspNetCore.Components;

namespace Steeltoe.Client.Models;

public class Feature
{
    public MarkupString Title { get; set; }

    public RenderFragment Description { get; set; }

    public string Img { get; set; }
}
