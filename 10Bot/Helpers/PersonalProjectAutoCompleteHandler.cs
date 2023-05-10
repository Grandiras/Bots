using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using TenBot.Services;

namespace TenBot.Helpers;
public sealed class PersonalProjectAutoCompleteHandler : AutocompleteHandler
{
	private readonly ServerService ServerService;
	private readonly DiscordSocketClient Client;


	public PersonalProjectAutoCompleteHandler(ServerService serverService, DiscordSocketClient client)
	{
		ServerService = serverService;
		Client = client;
	}


	public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context,
																			  IAutocompleteInteraction autocompleteInteraction,
																			  IParameterInfo parameter,
																			  IServiceProvider services)
	{
		var results = ServerService.GetCategoriesByRoles(ServerService.GetRoles(x => x.Name.EndsWith(" - Project") || x.Name.EndsWith(" - Project - Public"), context.Guild.Id), context.Guild.Id)
								   .Where(x => ((IGuildUser)context.User).RoleIds.Any(y => x.PermissionOverwrites.Any(z => z.Permissions.ViewChannel == PermValue.Allow && z.TargetId == y)))
								   .Select(x => new AutocompleteResult(x.Name, x.Name));

		await Task.CompletedTask;

		return AutocompletionResult.FromSuccess(results.Take(25));
	}
}
