using Discord;
using Discord.Interactions;
using TenBot.Services;

namespace TenBot.Helpers;
public sealed class ProjectTypeAutoCompleteHandler : AutocompleteHandler
{
	private readonly ProjectTemplates ProjectTemplates;


	public ProjectTypeAutoCompleteHandler(ProjectTemplates projectTemplates) => ProjectTemplates = projectTemplates;


	public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context,
																			  IAutocompleteInteraction autocompleteInteraction,
																			  IParameterInfo parameter,
																			  IServiceProvider services)
	{
		var results = ProjectTemplates.Templates.Keys.Select(x => new AutocompleteResult(x, x));

		await Task.CompletedTask;

		return AutocompletionResult.FromSuccess(results.Take(25));
	}
}
