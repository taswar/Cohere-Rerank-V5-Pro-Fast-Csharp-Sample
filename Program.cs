using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables()
    .Build();

var credential = new DefaultAzureCredential();

var pro = ReadModel(config, "Pro", "Cohere-rerank-v5-pro", 0.50);
var fast = ReadModel(config, "Fast", "Cohere-rerank-v5-fast", 0.45);

var query = "What is the refund window for annual plans?";
var documents = new[]
{
    "Monthly plans can be cancelled at any time. The change takes effect at the end of the billing period.",
    "Annual plans can be refunded in full within 30 days of purchase. After 30 days, the remaining months are prorated.",
    "Refund requests are submitted from the billing page under Invoices.",
    "Our offices are closed on public holidays in Ottawa and Toronto.",
    "Enterprise contracts follow the terms in the signed order form, not this page.",
    "Gift cards cannot be refunded but can be transferred to another account."
};

using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };

foreach (var model in new[] { pro, fast })
{
    var stopwatch = Stopwatch.StartNew();
    var results = await RerankAsync(http, credential, model, query, documents, topN: 3);
    stopwatch.Stop();

    Console.WriteLine($"{model.Name} ({stopwatch.ElapsedMilliseconds} ms, threshold {model.Threshold:0.00})");
    foreach (var result in results)
    {
        var decision = result.Score >= model.Threshold ? "keep" : "drop";
        Console.WriteLine($"  {result.Score:0.00}  {decision}  {documents[result.Index]}");
    }

    var kept = results.Count(r => r.Score >= model.Threshold);
    Console.WriteLine(kept == 0
        ? "  No passage cleared the threshold. Answer 'I do not know' instead of guessing."
        : $"  {kept} passage(s) passed to the chat model.");
    Console.WriteLine();
}

static RerankModel ReadModel(IConfiguration config, string name, string modelId, double threshold)
{
    var key = name.ToUpperInvariant();
    var endpoint = config[$"COHERE_RERANK_{key}_ENDPOINT"]
        ?? throw new InvalidOperationException($"Set COHERE_RERANK_{key}_ENDPOINT with dotnet user-secrets.");
    // The API key is optional. Without one, the app signs in with Entra ID through az login.
    var apiKey = config[$"COHERE_RERANK_{key}_API_KEY"];

    return new RerankModel(name, modelId, endpoint.TrimEnd('/'), apiKey, threshold);
}

static async Task<List<RerankResult>> RerankAsync(
    HttpClient http, TokenCredential credential, RerankModel model, string query,
    IReadOnlyList<string> documents, int topN)
{
    var body = new JsonObject
    {
        ["model"] = model.ModelId,
        ["query"] = query,
        ["top_n"] = topN,
        ["documents"] = new JsonArray(documents.Select(d => (JsonNode)d).ToArray())
    };

    // Foundry serves Cohere's native API under /providers/cohere, not under the /models inference route.
    using var request = new HttpRequestMessage(HttpMethod.Post, $"{model.Endpoint}/providers/cohere/v2/rerank");
    if (!string.IsNullOrWhiteSpace(model.ApiKey))
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", model.ApiKey);
    }
    else
    {
        var token = await credential.GetTokenAsync(
            new TokenRequestContext(["https://cognitiveservices.azure.com/.default"]),
            CancellationToken.None);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
    }

    // StringContent sets a fixed Content-Length; JsonContent uses chunked encoding.
    request.Content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json");

    using var response = await http.SendAsync(request);
    var text = await response.Content.ReadAsStringAsync();
    if (!response.IsSuccessStatusCode)
    {
        throw new InvalidOperationException($"{model.Name} returned {(int)response.StatusCode}: {text}");
    }

    using var doc = JsonDocument.Parse(text);
    var results = new List<RerankResult>();
    foreach (var item in doc.RootElement.GetProperty("results").EnumerateArray())
    {
        results.Add(new RerankResult(
            item.GetProperty("index").GetInt32(),
            item.GetProperty("relevance_score").GetDouble()));
    }

    return results;
}

record RerankModel(string Name, string ModelId, string Endpoint, string? ApiKey, double Threshold);

record RerankResult(int Index, double Score);
