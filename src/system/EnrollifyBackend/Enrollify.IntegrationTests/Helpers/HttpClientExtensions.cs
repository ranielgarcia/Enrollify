using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Enrollify.IntegrationTests.Helpers;

/// <summary>
/// Extension methods for HttpClient to simplify common operations in integration tests.
/// </summary>
public static class HttpClientExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Sends a GET request and deserializes the response to the specified type.
    /// </summary>
    public static async Task<T?> GetFromJsonAsync<T>(this HttpClient client, string requestUri)
    {
        var response = await client.GetAsync(requestUri);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(_jsonOptions);
    }

    /// <summary>
    /// Sends a POST request with JSON content and deserializes the response.
    /// </summary>
    public static async Task<TResponse?> PostAsJsonAsync<TRequest, TResponse>(
        this HttpClient client,
        string requestUri,
        TRequest content)
    {
        var response = await client.PostAsJsonAsync(requestUri, content, _jsonOptions);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions);
    }

    /// <summary>
    /// Sends a PUT request with JSON content and deserializes the response.
    /// </summary>
    public static async Task<TResponse?> PutAsJsonAsync<TRequest, TResponse>(
        this HttpClient client,
        string requestUri,
        TRequest content)
    {
        var response = await client.PutAsJsonAsync(requestUri, content, _jsonOptions);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions);
    }

    /// <summary>
    /// Sends a DELETE request and ensures success.
    /// </summary>
    public static async Task<HttpResponseMessage> DeleteAndEnsureSuccessAsync(
        this HttpClient client,
        string requestUri)
    {
        var response = await client.DeleteAsync(requestUri);
        response.EnsureSuccessStatusCode();
        return response;
    }

    /// <summary>
    /// Sends a PATCH request with JSON content.
    /// </summary>
    public static async Task<HttpResponseMessage> PatchAsJsonAsync<T>(
        this HttpClient client,
        string requestUri,
        T content)
    {
        var json = JsonSerializer.Serialize(content, _jsonOptions);
        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await client.PatchAsync(requestUri, httpContent);
        response.EnsureSuccessStatusCode();
        return response;
    }

    /// <summary>
    /// Adds an Authorization Bearer token to the request headers.
    /// </summary>
    public static HttpClient WithBearerToken(this HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    /// <summary>
    /// Reads the response content as a string.
    /// </summary>
    public static async Task<string> GetResponseContentAsync(this HttpResponseMessage response)
    {
        return await response.Content.ReadAsStringAsync();
    }

    /// <summary>
    /// Deserializes the response content to the specified type.
    /// </summary>
    public static async Task<T?> ReadAsJsonAsync<T>(this HttpResponseMessage response)
    {
        return await response.Content.ReadFromJsonAsync<T>(_jsonOptions);
    }
}
