namespace AiAgent.Models;

public record ProviderInfo(string Id, string Name, string Description, string Placeholder, string HelpUrl);

public static class Providers
{
    public static readonly IReadOnlyList<ProviderInfo> All = new[]
    {
        new ProviderInfo("openai", "OpenAI", "GPT-4o, GPT-4o-mini, o1", "sk-...", "https://platform.openai.com/api-keys"),
        new ProviderInfo("anthropic", "Anthropic", "Claude 3.5 Sonnet", "sk-ant-...", "https://console.anthropic.com/"),
        new ProviderInfo("gemini", "Google Gemini", "Gemini 2.0 Flash", "AIza...", "https://aistudio.google.com/app/apikey"),
        new ProviderInfo("groq", "Groq", "Llama 3 fast inference", "gsk_...", "https://console.groq.com/keys"),
        new ProviderInfo("openrouter", "OpenRouter", "200+ models gateway", "sk-or-...", "https://openrouter.ai/keys"),
        new ProviderInfo("mistral", "Mistral", "Mistral Large", "...", "https://console.mistral.ai/api-keys"),
        new ProviderInfo("deepseek", "DeepSeek", "DeepSeek V3 / R1", "sk-...", "https://platform.deepseek.com/api_keys"),
        new ProviderInfo("together", "Together AI", "Open models", "...", "https://api.together.xyz/settings/api-keys"),
        new ProviderInfo("perplexity", "Perplexity", "Sonar models", "pplx-...", "https://www.perplexity.ai/settings/api"),
        new ProviderInfo("azure", "Azure OpenAI", "Azure-hosted GPT", "...", "https://portal.azure.com/"),
    };
}

public class ChatMessage
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Role { get; set; } = "user"; // user | assistant | system
    public string Content { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Conversation
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Title { get; set; } = "New conversation";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public List<ChatMessage> Messages { get; set; } = new();
}

public class AppSettings
{
    public string SelectedProviderId { get; set; } = "openai";
    public string SelectedModel { get; set; } = "gpt-4o-mini";
    public string SystemPrompt { get; set; } = "You are Ginger, a helpful AI assistant.";
    public double Temperature { get; set; } = 0.7;
    public int MaxTokens { get; set; } = 1024;
}
