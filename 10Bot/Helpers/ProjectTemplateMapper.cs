using Newtonsoft.Json;
using TenBot.Enums;
using TenBot.Models;

namespace TenBot.Helpers;
internal static class ProjectTemplateMapper
{
    public static ProjectTemplate GetProjectTemplateFromProjectType(ProjectType projectType) 
        => JsonConvert.DeserializeObject<ProjectTemplate>(File.ReadAllText(Directory.GetCurrentDirectory().Split(@"\bin")[0] + "/Data/ProjectTemplates" + $"/{projectType.ToString().ToLower()}.json"))!;
}
