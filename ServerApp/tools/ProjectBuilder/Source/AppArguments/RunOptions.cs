using CommandLine;

namespace ProjectBuilder.AppArguments;

[Verb("process", HelpText = "Run app with arguments")]
public class RunOptions
{
    [Option('p', "path", Required = true,
        HelpText = "Full path to container solution"
    )]
    public string Path { get; set; }

    [Option('n', "name", Required = true,
        HelpText = "Container name"
    )]
    public string Name { get; set; }

    [Option('v', "vers", Required = true,
        HelpText = "Container version"
    )]
    public string Version { get; set; }

    [Option('t', "token", Required = true,
        HelpText = "Token"
    )]
    public string Token { get; set; }
}