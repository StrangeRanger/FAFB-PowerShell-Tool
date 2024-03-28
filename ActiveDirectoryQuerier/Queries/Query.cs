using System.Management.Automation.Runspaces;
using System.Text.Json.Serialization;

namespace ActiveDirectoryQuerier.Queries;

public class Query
{
    public string QueryName { get; set; } = string.Empty;

    public string QueryDescription { get; set; } = string.Empty;

    // ReSharper disable once InconsistentNaming
    public string? PSCommandName { get; set; }

    // ReSharper disable once InconsistentNaming
    public string[] PSCommandParameters { get; set; } = Array.Empty<string>();

    // ReSharper disable once InconsistentNaming
    public string[] PSCommandParameterValues { get; set; } = Array.Empty<string>();

    [JsonIgnore]
    public Command? Command { get; set; }

    public Query(string psCommandName)
    {
        PSCommandName = psCommandName;
    }

    public Query()
    { }
}
