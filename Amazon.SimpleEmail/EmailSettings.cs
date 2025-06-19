namespace Amazon.SimpleEmail;

public sealed class EmailSettings
{
    public const string SectionName = "EmailSettings";
    public string SenderEmail { get; set; } = string.Empty;
}