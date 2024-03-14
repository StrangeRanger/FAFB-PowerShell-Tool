using System.Management.Automation.Runspaces;
using System.Text.Json.Serialization;

namespace ActiveDirectoryQuerier.Queries;

public class Query
{
    /// <summary>
    /// Command that should help with Binding the command to the query for the buttons sake
    /// </summary>
    [JsonIgnore]
    public Command? Command { get; set; }

    /// <summary>
    /// Used for serializing the Command Name
    /// </summary>
    public string? PSCommandName { get; set; }

    /// <summary>
    /// Used for Serializing the Commands parameters
    /// </summary>
    public string[]? PSCommandParameters { get; set; }

    /// <summary>
    /// Used for Serializing the Commands parameters
    /// </summary>
    public string[]? PSCommandParameterValues { get; set; }

    /// <summary>
    /// Used for the name of the custom Query
    /// </summary>
    public string? QueryName { get; set; }

    /// <summary>
    /// Used for the Custom Queries Description
    /// </summary>
    public string? QueryDescription { get; set; }

    /// <summary>
    /// Constructor for the query class
    /// </summary>
    /// <param name="psCommandName">Name of the Command EX. get-adusers</param>
    public Query(string psCommandName)
    {
        PSCommandName = psCommandName;
    }

    /// <summary>
    /// Empty constructor for the query class
    /// </summary>
    public Query()
    { }
}
