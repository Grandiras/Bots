using Discord;
using Discord.Interactions;
using TenBot.ServerAbstractions;

namespace TenBot.Features.Projects;
public sealed class ProjectTemplateAutoCompleteHandler(ProjectService ProjectService) : ServerAutoCompleteHandler
{
    public override Task<AutocompletionResult> GenerateSuggestionsAsync(ServerInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        => Task.FromResult(AutocompletionResult.FromSuccess(ProjectService.GetTemplates(context.ServerID).Take(25).Select(x => new AutocompleteResult(x.Name, x.Name))));
}
public sealed class ServerProjectTemplateAutoCompleteHandler(ProjectService ProjectService) : ServerAutoCompleteHandler
{
    public override Task<AutocompletionResult> GenerateSuggestionsAsync(ServerInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        => Task.FromResult(AutocompletionResult.FromSuccess(ProjectService.GetTemplates(context.ServerID, false).Take(25).Select(x => new AutocompleteResult(x.Name, x.Name))));
}

public sealed class ProjectAutoCompleteHandler : ServerAutoCompleteHandler
{
    public override Task<AutocompletionResult> GenerateSuggestionsAsync(ServerInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        => Task.FromResult(AutocompletionResult.FromSuccess(context.Guild.Roles.Where(x => x.Name.Contains(" - Project")).Take(25).Select(x => x.Name.Split(" - ").First()).Select(x => new AutocompleteResult(x, x))));
}

