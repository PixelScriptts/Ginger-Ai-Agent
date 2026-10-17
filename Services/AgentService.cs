using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AiAgent.Models;

namespace AiAgent.Services;

public class AgentService
{
    private readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(60) };
    private readonly ConversationStore _store;

    public AgentService(ConversationStore store) => _store = store;

    public async Task<string> SendAsync(string prompt, CancellationToken ct = default)
    {
        var settings = _store.LoadSettings();
        var apiKey = await ProviderCredentialService.GetAsync(settings.SelectedProviderId);
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("Provider connection is required. Go to Settings and save your API key.");

        var system = settings.SystemPrompt;
        return settings.SelectedProviderId switch
        {
            "anthropic" => await SendAnthropicAsync(apiKey, settings.SelectedModel, system, prompt, settings, ct),
            "gemini" => await SendGeminiAsync(apiKey, settings.SelectedModel, system, prompt, settings, ct),
            _ => await SendOpenAICompatibleAsync(apiKey, settings, system, prompt, ct),
        };
    }

    private async Task<string> SendOpenAICompatibleAsync(string apiKey, AppSettings s, string system, string prompt, CancellationToken ct)
    {
        var (url, model) = s.SelectedProviderId switch
        {
            "openai" => ("https://api.openai.com/v1/chat/completions", string.IsNullOrWhiteSpace(s.SelectedModel) ? "gpt-4o-mini" : s.SelectedModel),
            "groq" => ("https://api.groq.com/openai/v1/chat/completions", string.IsNullOrWhiteSpace(s.SelectedModel) ? "llama-3.3-70b-versatile" : s.SelectedModel),
            "openrouter" => ("https://openrouter.ai/api/v1/chat/completions", string.IsNullOrWhiteSpace(s.SelectedModel) ? "openai/gpt-4o-mini" : s.SelectedModel),
            "mistral" => ("https://api.mistral.ai/v1/chat/completions", string.IsNullOrWhiteSpace(s.SelectedModel) ? "mistral-large-latest" : s.SelectedModel),
            "deepseek" => ("https://api.deepseek.com/chat/completions", string.IsNullOrWhiteSpace(s.SelectedModel) ? "deepseek-chat" : s.SelectedModel),
            "together" => ("https://api.together.xyz/v1/chat/completions", string.IsNullOrWhiteSpace(s.SelectedModel) ? "meta-llama/Meta-Llama-3.1-70B-Instruct-Turbo" : s.SelectedModel),
            "perplexity" => ("https://api.perplexity.ai/chat/completions", string.IsNullOrWhiteSpace(s.SelectedModel) ? "sonar" : s.SelectedModel),
            "azure" => throw new InvalidOperationException("Azure OpenAI requires endpoint override. Use OpenAI-compatible URL or set AZURE endpoint in model field as https://{resource}.openai.azure.com/openai/deployments/{deployment}/chat/completions?api-version=2024-02-15-preview"),
            _ => ("https://api.openai.com/v1/chat/completions", s.SelectedModel),
        };

        var payload = new
        {
            model,
            messages = new object[]
            {
                new { role = "system", content = system },
                new { role = "user", content = prompt },
            },
            temperature = s.Temperature,
            max_tokens = s.MaxTokens,
        };

        using var req = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"),
        };
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        if (s.SelectedProviderId == "openrouter")
        {
            req.Headers.Add("HTTP-Referer", "https://ginger.ai");
            req.Headers.Add("X-Title", "Ginger Agent");
        }

        using var res = await _http.SendAsync(req, ct);
        var body = await res.Content.ReadAsStringAsync(ct);
        if (!res.IsSuccessStatusCode)
            throw new InvalidOperationException($"Provider error ({(int)res.StatusCode}): {body}");
        using var doc = JsonDocument.Parse(body);
        return doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "(empty)";
    }

    private async Task<string> SendAnthropicAsync(string apiKey, string model, string system, string prompt, AppSettings s, CancellationToken ct)
    {
        var m = string.IsNullOrWhiteSpace(model) ? "claude-3-5-sonnet-20241022" : model;
        var payload = new
        {
            model = m,
            system,
            max_tokens = s.MaxTokens,
            temperature = s.Temperature,
            messages = new[] { new { role = "user", content = prompt } },
        };
        using var req = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages")
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"),
        };
        req.Headers.Add("x-api-key", apiKey);
        req.Headers.Add("anthropic-version", "2023-06-01");
        using var res = await _http.SendAsync(req, ct);
        var body = await res.Content.ReadAsStringAsync(ct);
        if (!res.IsSuccessStatusCode) throw new InvalidOperationException($"Anthropic error ({(int)res.StatusCode}): {body}");
        using var doc = JsonDocument.Parse(body);
        return doc.RootElement.GetProperty("content")[0].GetProperty("text").GetString() ?? "(empty)";
    }

    private async Task<string> SendGeminiAsync(string apiKey, string model, string system, string prompt, AppSettings s, CancellationToken ct)
    {
        var m = string.IsNullOrWhiteSpace(model) ? "gemini-2.0-flash" : model;
        var combined = string.IsNullOrWhiteSpace(system) ? prompt : system + "\n\n" + prompt;
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{Uri.EscapeDataString(m)}:generateContent?key={Uri.EscapeDataString(apiKey)}";
        var payload = new
        {
            contents = new[] { new { parts = new[] { new { text = combined } } } },
            generationConfig = new { temperature = s.Temperature, maxOutputTokens = s.MaxTokens },
        };
        using var req = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"),
        };
        using var res = await _http.SendAsync(req, ct);
        var body = await res.Content.ReadAsStringAsync(ct);
        if (!res.IsSuccessStatusCode) throw new InvalidOperationException($"Gemini error ({(int)res.StatusCode}): {body}");
        using var doc = JsonDocument.Parse(body);
        return doc.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString() ?? "(empty)";
    }
}
