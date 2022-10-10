using System.Diagnostics;
using System.IO.Compression;
using Gaming.ContainerManager.Contracts.V1.Models;
using Gaming.Facade;
using Gaming.Facade.Configuration;
using ProjectBuilder;

const string archiveName = "image.zip";
const string lastExecutionArgs = "LastExecutionArgs.json";

const string token =
    "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiIzNWU5MWZjNC02YjllLTQ5MDMtYjYyYS1lZDIzZGE0NjU5Y2QiLCJhdXRoIjoiR3Vlc3QiLCJwcm06YWNjZXNzLW1hbmFnZXI6djI6Z2V0LXVzZXItcGVybWlzc2lvbnMiOiJzZWxmIiwicHJtOmFjY2Vzcy1tYW5hZ2VyOnYyOmdldC11c2VyLXJvbGUtbmFtZXMiOiJzZWxmIiwicHJtOmF0dHJpYnV0ZS1tYW5hZ2VyOnYyOmdldC11c2VyLWF0dHJpYnV0ZXMiOiJzZWxmIiwicHJtOmNvbnRhaW5lci1odWI6djE6Y29ubmVjdC1yZWxpYWJsZSI6InNlbGYiLCJwcm06Y29udGFpbmVyLWh1Yjp2MTpjb25uZWN0LXVucmVsaWFibGUiOiJzZWxmIiwicHJtOmZpbGUtc3RvcmFnZTp2MTpsaXN0Ijoic2VsZiIsInBybTpmaWxlLXN0b3JhZ2U6djE6dXBsb2FkIjoic2VsZiIsInBybTppZGVudGl0eS1wcm92aWRlcjp2Mjphc3NpZ24tYWNjb3VudCI6InNlbGYiLCJwcm06aWRlbnRpdHktcHJvdmlkZXI6djI6dW5hc3NpZ24tYWNjb3VudCI6InNlbGYiLCJwcm06c2NvcGUtbWFuYWdlcjp2MjpnZXQtdXNlci1zY29wZXMiOiJzZWxmIiwicHJtOmNvbnRhaW5lci1pbWFnZS1tYW5hZ2VyOnYxOmdldC1pbWFnZS1jb250ZW50IjoiIiwicHJtOmNvbnRhaW5lci1pbWFnZS1tYW5hZ2VyOnYxOmxpc3QtaW1hZ2VzIjoiIiwicHJtOmNvbnRhaW5lci1pbWFnZS1tYW5hZ2VyOnYxOnJlZ2lzdGVyLWltYWdlIjoiIiwicHJtOmNvbnRhaW5lci1pbnNwZWN0b3I6djE6Z2V0LWluZm8iOiIiLCJwcm06Y29udGFpbmVyLWluc3BlY3Rvcjp2MTpnZXQtbG9ncyI6IiIsInBybTpjb250YWluZXItaW5zcGVjdG9yOnYxOmdldC1zdGF0ZSI6IiIsInBybTpjb250YWluZXItbWFuYWdlcjp2MTpjcmVhdGUtY29udGFpbmVyIjoiIiwicHJtOmNvbnRhaW5lci1tYW5hZ2VyOnYxOmxpc3QtY29udGFpbmVycyI6IiIsInBybTpjb250YWluZXItbWFuYWdlcjp2MTpzdG9wLWNvbnRhaW5lciI6InNlbGYiLCJuYmYiOjAsImV4cCI6MjUzNDAyMzAwODAwLCJpYXQiOjE2NjQzNTc1OTgsImlzcyI6IkdhbWluZy5JZGVudGl0eVByb3ZpZGVyIiwiYXVkIjoiR2FtaW5nIn0.o3BdSaEabspqEn_LkCEGkQM7F6tKEOfdAaKHPlapteWbYftuf3g82dPp-VdYfT58QzvX8Wu_pxsCsJjXP_prfNkYem_CT4U26pOleX4Y5E0TxWgUD2Zm-l7xJ6mfm1FKdgcS6snBMcHD90H5Bp3CIiyPhm85gHoA2XxmnJzATiTtZ1fNOwiw-3-TuN55xUtDSLO1Baa3T2dd6xjTlFaH3c31-7XyIJ__nTOXJ8Gf26xQ4NqZDUXuhJa_PnqwCQ6ytiwb2pzkL2xsdW8khXdcScokGwogEa-jiJN2mutVZV_r5qJ29G5k_GqUviyh1H5dmMBjSNiroPEzn8AQxNfjXQ";

try
{
    var executionArgs = GetExecutionArgs(args);

    if (!await BuildProject(executionArgs.Path))
    {
        Console.Read();
        return;
    }

    var buildPath = Path.Combine(executionArgs.Path, "bin/Debug/netcoreapp3.1/");

    if (File.Exists(GetArchivePath(buildPath)))
        File.Delete(GetArchivePath(buildPath));

    var files = GetFilesToAdding(buildPath);

    if (!CreateArchive(buildPath, files))
    {
        Console.Read();
        return;
    }

    var successful = await SendArchive(File.ReadAllBytes(GetArchivePath(buildPath)),
        new ContainerImageName(executionArgs.Name),
        executionArgs.Version);

    if (successful)
        SaveExecutionArgs(executionArgs);
}
catch (Exception e)
{
    Console.WriteLine(e);
    Console.Read();
    throw;
}

static async Task<bool> SendArchive(byte[] archive, ContainerImageName name, Version version)
{
    var success = false;
    var url = new Uri("https://dev.containers.xfin.net");

    GamingServices.V1.Configure(
        configuration => configuration.WithServerUrl(url),
        Console.WriteLine);

    var image = new ContainerImage(name, version, archive);
    await GamingServices.V1
        .RegisterContainerImage(image)
        .WithAccessToken(token)
        .HandleResultAsync(
            descriptor =>
            {
                success = true;
                Console.WriteLine("Successfully added");
                Console.WriteLine($"Name = {descriptor.Name.Value}");
                Console.WriteLine($"Version = {descriptor.Version.ToString()}");
                Console.WriteLine($"AddedAt = {descriptor.AddedAt.ToString()}");
                return Task.CompletedTask;
            }, () =>
            {
                Console.WriteLine("Has conflict with same name or version");
                return Task.CompletedTask;
            }, errors =>
            {
                Console.WriteLine("Invalid container image");
                foreach (var error in errors)
                {
                    Console.WriteLine($"{error.Kind} {error.Text}");
                    Console.WriteLine(error.StackTrace);
                }

                return Task.CompletedTask;
            }, ex =>
            {
                Console.WriteLine("Unexpected exception");
                Console.WriteLine(ex.ToString());
                return Task.CompletedTask;
            });

    return success;
}

static string GetArchivePath(string rootPath) => Path.Combine(rootPath, archiveName);

static bool CreateArchive(string root, ICollection<string> paths)
{
    var archivePath = GetArchivePath(root);

    File.Delete(archivePath);

    if (paths.Count == 0)
    {
        Console.WriteLine($"No files to adding to archive");
        return false;
    }

    var zip = ZipFile.Open(archivePath, ZipArchiveMode.Create);
    foreach (var p in paths)
    {
        Console.WriteLine($"Add file to archive {p}");
        zip.CreateEntryFromFile(p, Path.GetFileName(p));
    }

    zip.Dispose();

    Console.WriteLine($"Archive created at {archivePath}");
    return true;
}

static List<string> GetFilesToAdding(string root)
{
    return Directory.GetFiles(root).ToList();
}

static async Task<bool> BuildProject(string rootPath)
{
    try
    {
        var info = new ProcessStartInfo
        {
            FileName = "dotnet",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        info.ArgumentList.Add("build");
        info.ArgumentList.Add(rootPath);
        info.ArgumentList.Add("--no-incremental");

        var process = Process.Start(info);

        process.OutputDataReceived += (_, eventArgs) => Console.WriteLine(eventArgs.Data);
        process.ErrorDataReceived += (_, eventArgs) => Console.WriteLine(eventArgs.Data);
        process.Start();

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();

        var exitCode = process.ExitCode;
        process.Dispose();

        if (exitCode != 0)
        {
            Console.WriteLine("Build has errors");
            return false;
        }

        Console.WriteLine("Build successful");
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        return false;
    }

    return true;
}

static Version GetProjectVersion(params string[] args)
{
    var rawVersion = "";
    if (args.Length < 3)
    {
        Console.WriteLine($"Container version not specified, enter it");
        rawVersion = Console.ReadLine();
    }
    else
        rawVersion = args[2];

    if (Version.TryParse(rawVersion, out var version))
        return version;

    Console.WriteLine($"Invalid container version, input={version}");
    return null;
}

static string GetProjectName(params string[] args)
{
    if (args.Length < 2)
    {
        Console.WriteLine($"Container name not specified, enter it");
        return Console.ReadLine();
    }

    return args[1];
}

static string GetProjectPath(params string[] args)
{
    if (args.Length < 1)
    {
        Console.WriteLine($"Path to solution not specified, enter it");
        return Console.ReadLine();
    }

    return args[0];
}

static ExecutionArgs GetExecutionArgs(params string[] args)
{
    var executionArgs = GetPrevExecutionArgs();
    if (executionArgs != null)
    {
        executionArgs.Version = new Version(executionArgs.Version.Major,
            executionArgs.Version.Minor,
            executionArgs.Version.Build + 1);
        return executionArgs;
    }

    executionArgs = new ExecutionArgs(
        GetProjectPath(args),
        GetProjectName(args),
        GetProjectVersion(args)
    );

    return executionArgs.Version == null
        ? null
        : executionArgs;
}

static ExecutionArgs GetPrevExecutionArgs()
{
    if (!File.Exists(lastExecutionArgs))
        return null;

    var json = File.ReadAllText(lastExecutionArgs);

    return ExecutionArgs.ParseJson(json);
}

static void SaveExecutionArgs(ExecutionArgs args)
{
    if (!File.Exists(lastExecutionArgs))
        File.Delete(lastExecutionArgs);

    File.WriteAllText(lastExecutionArgs, args.ToJsonString());
}