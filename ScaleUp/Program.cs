using DeepAI;
using Microsoft.Extensions.Configuration;
using ScaleUp;
using System.Threading.Tasks.Dataflow;
using static System.Console;

var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables()
    .AddCommandLine(args)
    .Build();

var apiKey = config["ApiKey"];
var file = config["File"];
var folder = config["Folder"];
var saveTo = config["SaveTo"];
var threads = config["Threads"] ?? "-1";

if (!IsValid(apiKey, v => v.IsGuid()))
    return;

if (!IsValid(file, v => v == null || File.Exists(v)))
    return;

if (!IsValid(folder, v => v == null || Directory.Exists(v)))
    return;

if (!IsValid(file, v => v != null || folder != null))
    return;

if (!IsValid(saveTo, v => v.IsFolderName(false)))
    return;

if (!IsValid(threads, v => v.IsThreads()))
    return;

var mdop = int.Parse(threads);

if (mdop == -1)
    mdop = Environment.ProcessorCount;

var api = new DeepAI_API(apiKey);

saveTo.EnsurePathExists();

var fileNames = new List<string>();

if (!string.IsNullOrEmpty(file))
    fileNames.Add(file);

if (!string.IsNullOrEmpty(folder))
{
    fileNames.AddRange(
        Directory.GetFiles(folder).Where(f => f.IsImageFile()));
}

if (fileNames.Count == 0)
{
    ShowHelp();

    return;
}

var jobs = new List<Job>();

foreach (var fileName in fileNames)
    jobs.Add(new Job(fileName));

var client = new HttpClient();

var worker = new ActionBlock<Job>(
    async job =>
    {
        try
        {
            await job.FetchAndScaleUpAsync(api, client, saveTo);

            WriteLine($"FETCHED / SCALED {job.FileName}");
        }
        catch (Exception error)
        {
            WriteLine(error.Message);
        }
    },
    new ExecutionDataflowBlockOptions()
    {
        MaxDegreeOfParallelism = mdop
    });

jobs.ForEach(j => worker.Post(j));

worker.Complete();

await worker.Completion;

static bool IsValid(string value, Func<string, bool> isValid)
{
    if (!isValid(value))
    {
        ShowHelp();

        return false;
    };

    return true;
}

static void ShowHelp()
{
    WriteLine("Upscales image file(s) using DeepAI Super Resolution API");
    WriteLine();
    WriteLine("SCALEUP [File|Folder] SaveTo");
    WriteLine();
    WriteLine("  File     An image file");
    WriteLine("  Folder   A folder with zero or more image files");
    WriteLine("  SaveTo   The folder to save upscaled image(s) to");
    WriteLine("  Threads  The number of threads to employ");
    WriteLine();
    WriteLine("A \"ScaleUpApiKey\" must also be provided, via an EnvVar.");
    WriteLine("To get an API key, go to https://deepai.org/ then click");
    WriteLine("on the \"Signup\" button.");
}