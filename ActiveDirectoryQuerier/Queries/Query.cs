using System.Management.Automation.Runspaces;
using System.Text.Json.Serialization;

namespace ActiveDirectoryQuerier.Queries;

// TODO: Remove property descriptions if name is descriptive enough, or provide a detailed description if it is not.
public class Query
{
    /// <summary>
    /// Command that should help with Binding the command to the query for the buttons sake
    /// </summary>
    /// TODO: Give a better description of what this property does, and rename it to something more descriptive.
    /// TODO: Determine if this property should be nullable.
    [JsonIgnore]
    public Command? Command { get; set; }

    /// <summary>
    /// Used for serializing the Command Name
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public string PSCommandName { get; set; }

    /// <summary>
    /// Used for Serializing the Commands parameters
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public string[] PSCommandParameters { get; set; }

    /// <summary>
    /// Used for Serializing the Commands parameters
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public string[] PSCommandParameterValues { get; set; }

    /// <summary>
    /// Used for the name of the custom Query
    /// </summary>
    public string QueryName { get; set; }

    /// <summary>
    /// Used for the Custom Queries Description
    /// </summary>
    public string QueryDescription { get; set; }

    // Constructor for the query class
    // TODO: Remove commented code if it is not needed.
    /*public Query(string psCommandName, string[] commandParams)
    {
        PSCommandName = psCommandName;
        ADCommandParameters = commandParams;
    }*/

    /// <summary>
    /// Constructor for the query class
    /// </summary>
    /// <param name="psCommandName"></param>
    /// TODO: Fix any and all warnings about possible null values.
    public Query(string psCommandName)
    {
        PSCommandName = psCommandName;
    }

    /// <summary>
    /// Empty constructor for the query class
    /// </summary>
    /// TODO: Fix any and all warnings about possible null values.
    public Query()
    { }
}
