using System.Text.Json.Nodes;
using CommandLine;

namespace ProjectBuilder.AppArguments;

public class ExecutionArgs
{
    const string lastExecutionArgsName = "LastExecutionArgs.json";

    public string Path;
    public string Name;
    public Version Version;
    public string Token;

    public ExecutionArgs(string path, string name, Version version, string token)
    {
        Path = path;
        Name = name;
        Version = version;
        Token = token;
    }

    public static ExecutionArgs ParseCommandlineArgs(params string[] args)
    {
        ExecutionArgs result = null;
        Parser.Default.ParseArguments<RunOptions, LastArgsRunOptions>(args)
            .WithParsed<RunOptions>(options =>
            {
                result = new ExecutionArgs(options.Path, options.Name, Version.Parse(options.Version),
                    options.Token);
            })
            .WithParsed<LastArgsRunOptions>(_ =>
            {
                result = LoadSavedArgs();
                result.Version = new Version(result.Version.Major, result.Version.Minor, result.Version.Build + 1);
            })
            .WithNotParsed(e =>
            {
                foreach (var error in e) Console.WriteLine(error);
            });

        return result;
    }

    public static void Save(ExecutionArgs args)
    {
        if (!File.Exists(lastExecutionArgsName))
            File.Delete(lastExecutionArgsName);

        File.WriteAllText(lastExecutionArgsName, args.ToJsonString());
    }

    private static ExecutionArgs LoadSavedArgs()
    {
        if (!File.Exists(lastExecutionArgsName))
        {
            Console.WriteLine($"Can`t found {lastExecutionArgsName}");
            return null;
        }

        return ParseJson(File.ReadAllText(lastExecutionArgsName));
    }

    private static ExecutionArgs ParseJson(string json)
    {
        var parsed = JsonNode.Parse(json) as JsonObject;

        return new ExecutionArgs(
            (string)parsed[nameof(Path)],
            (string)parsed[nameof(Name)],
            Version.Parse((string)parsed[nameof(Version)]),
            (string)parsed[nameof(Token)]
        );
    }

    private string ToJsonString()
    {
        var obj = new JsonObject
        {
            { nameof(Path), Path },
            { nameof(Name), Name },
            { nameof(Version), Version.ToString() },
            { nameof(Token), Token }
        };

        return obj.ToJsonString();
    }
}