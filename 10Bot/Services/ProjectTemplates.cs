using Newtonsoft.Json;
using TenBot.Models;

namespace TenBot.Services;
public sealed class ProjectTemplates
{
    private readonly string FILE_PATH = Directory.GetCurrentDirectory().Split(@"\bin")[0] + "/Data/ProjectTemplates";
    
    public Dictionary<string, ProjectTemplate> Templates { get; } = new();


    public ProjectTemplates()
    {
        var files = Directory.GetFiles(FILE_PATH).Where(x => x.EndsWith(".json"));
        foreach (var file in files) Templates.Add(file.Split(@"\").Last().Split(".json")[0], 
                                                  JsonConvert.DeserializeObject<ProjectTemplate>(File.ReadAllText(file))!);
    }
}
