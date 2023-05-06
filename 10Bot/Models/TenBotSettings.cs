﻿using Newtonsoft.Json;

namespace TenBot.Models;
public sealed class TenBotSettings
{
	private TenBotConfiguration? CachedConfiguration;

	public bool IsBeta { get; init; } = false;
	public required string RootPath { get; init; }

	public TenBotConfiguration Configuration
	{
		get
		{
			CachedConfiguration ??= JsonConvert.DeserializeObject<TenBotConfiguration>(File.ReadAllText(IsBeta ? RootPath + @"\beta_config.json" : @"\config.json"))!;
			return CachedConfiguration;
		}
	}
}

public sealed record TenBotConfiguration(string Token);
