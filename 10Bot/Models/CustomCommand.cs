namespace TenBot.Models;
public sealed class CustomCommand
{
	public string Name { get; }
	public string Content { get; set; }


	public CustomCommand(string name, string content)
	{
		Name = name;
		Content = content;
	}
}
