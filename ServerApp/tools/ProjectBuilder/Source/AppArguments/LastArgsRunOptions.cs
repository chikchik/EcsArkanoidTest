using CommandLine;

namespace ProjectBuilder.AppArguments;

[Verb("last", HelpText = "Run with previous args saved in LastExecutionArgs.json")]
public class LastArgsRunOptions { }