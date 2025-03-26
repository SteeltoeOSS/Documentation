using System.ComponentModel.DataAnnotations;

namespace Steeltoe.Client.Models;

public class DocsSiteOptions
{
    [Required] public string BaseAddress { get; set; } = "https://docs.steeltoe.io";

    public string ApiBrowserHome => $"{BaseAddress}/api/browser/v4/all";

    public string BlogHome => $"{BaseAddress}/articles/";

    public string DocsHome => $"{BaseAddress}/api/v4/welcome/";

    public string DocsStreamHome => $"{BaseAddress}/api/v3/stream/";

    public string GuidesHome => $"{BaseAddress}/guides/";

    public string FileShareHome => $"{BaseAddress}/api/v4/fileshares/";

    public string DynamicLoggingHome => $"{BaseAddress}/api/v4/logging/";
}
