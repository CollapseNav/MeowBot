namespace MeowBot;

public class GroupConfig
{
    public long GroupId { get; set; }
    public string? Role { get; set; }
    public int MaxHistory { get; set; } = AppConfig.MaxHistory;
}