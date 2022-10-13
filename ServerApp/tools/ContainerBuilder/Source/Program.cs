using System.Diagnostics;
using System.IO.Compression;
using Gaming.ContainerManager.Contracts.V1.Models;
using Gaming.Facade;
using Gaming.Facade.Configuration;
using ProjectBuilder.AppArguments;

const string archiveName = "image.zip";

try
{
    var executionArgs = ExecutionArgs.ParseCommandlineArgs(args);
    if (executionArgs == null)
        return;

    if (!await BuildProject(executionArgs.Path))
        return;

    var buildPath = Path.Combine(executionArgs.Path, "bin/Debug/netcoreapp3.1/");

    if (File.Exists(GetArchivePath(buildPath)))
        File.Delete(GetArchivePath(buildPath));

    var files = Directory.GetFiles(buildPath).ToList();

    if (!CreateArchive(buildPath, files))
        return;

    var successful = await SendArchive(File.ReadAllBytes(GetArchivePath(buildPath)),
        new ContainerImageName(executionArgs.Name),
        executionArgs.Version, executionArgs.Token);

    if (successful)
        ExecutionArgs.Save(executionArgs);
}
catch (Exception e)
{
    Console.WriteLine(e);
    throw;
}
finally
{
    Console.Read();
}

static async Task<bool> SendArchive(byte[] archive, ContainerImageName name, Version version, string token)
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

        while (!process.HasExited)
            await Task.Delay(10);

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