using DeepAI;
using System.Text.Json;

namespace ScaleUp;

public class Job
{
    public Job(string fileName)
    {
        FileName = fileName;
    }

    public string FileName { get; }

    public async Task FetchAndScaleUpAsync(
        DeepAI_API api, HttpClient client, string folder)
    {
        var sar = api.callStandardApi(
            "torch-srgan", new { image = File.OpenRead(FileName) });

        var response = JsonSerializer.Deserialize<Response>(
            api.objectAsJsonString(sar));

        var saveTo = Path.Join(folder, Path.GetFileName(FileName));

        using var target = File.OpenWrite(saveTo);

        var source = await client!.GetStreamAsync(response!.OutputUrl);

        await source.CopyToAsync(target);
    }
}