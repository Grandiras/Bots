using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using TenBot.Models;

namespace TenBot.Services;
public sealed class MigrationService : IService
{
	private readonly Dictionary<int, List<Func<IServiceProvider, bool>>> MigrationTasks = new()
	{
		{ 2, new() {
			x =>
			{
				var serverService = x.GetRequiredService<DiscordServerSettingsStorage>();
				var fileSystemManager = x.GetRequiredService<FileSystemManager>();

				foreach (var server in serverService.ServerSettings.Keys)
				{
					var serverConfig = serverService.ServerSettings[server];

					fileSystemManager.CreateServerDirectoryIfNotExisting(server);
					fileSystemManager.WriteToServerFile(server, "config", JsonConvert.SerializeObject(new ServerConfiguration(serverConfig)));
				}
			}
		} }
	};

	private readonly IServiceProvider Services;


	public MigrationService(IServiceProvider services) => Services = services;
}
