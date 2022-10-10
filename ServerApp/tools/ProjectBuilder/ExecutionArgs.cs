using System.Text.Json.Nodes;

namespace ProjectBuilder;

public class ExecutionArgs
{
    public string Path;
    public string Name;
    public Version Version;

    public ExecutionArgs(string path, string name, Version version)
    {
        Path = path;
        Name = name;
        Version = version;
    }

    public static ExecutionArgs ParseJson(string json)
    {
        var parsed = JsonNode.Parse(json) as JsonObject;

        return new ExecutionArgs(
            (string)parsed[nameof(Path)],
            (string)parsed[nameof(Name)],
            Version.Parse((string)parsed[nameof(Version)])
        );
    }

    public string ToJsonString()
    {
        var obj = new JsonObject
        {
            { nameof(Path), Path },
            { nameof(Name), Name },
            { nameof(Version), Version.ToString() }
        };

        return obj.ToJsonString();
    }
}