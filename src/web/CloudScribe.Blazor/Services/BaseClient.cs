using System.Net.Http.Headers;
using CloudScribe.Blazor.Services.Auth;
using Microsoft.Extensions.Logging;

namespace CloudScribe.Blazor.Services;

public abstract class BaseClient(
    IHttpClientFactory factory,
    TokenService tokenService,
    ILogger logger,
    string apiName)
{
    private async Task<HttpClient> CreateClientAsync()
    {
        var client = factory.CreateClient(apiName);

        var token = await tokenService.GetAccessTokenAsync();

        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        return client;
    }

    protected async Task<T?> Get<T>(string uri)
    {
        try
        {
            var client = await CreateClientAsync();
            return await client.GetFromJsonAsync<T>(uri);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "HTTP GET request failed for {Uri}. Status: {StatusCode}", uri, ex.StatusCode);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during GET request to {Uri}", uri);
            throw;
        }
    }

    protected async Task Post<T>(string uri, T value)
    {
        try
        {
            var client = await CreateClientAsync();
            var response = await client.PostAsJsonAsync(uri, value);

            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "HTTP POST request failed for {Uri}. Status: {StatusCode}", uri, ex.StatusCode);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during POST request to {Uri}", uri);
            throw;
        }
    }

    protected async Task Put<T>(string uri, T value)
    {
        try
        {
            var client = await CreateClientAsync();
            var response = await client.PutAsJsonAsync(uri, value);

            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "HTTP PUT request failed for {Uri}. Status: {StatusCode}", uri, ex.StatusCode);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during PUT request to {Uri}", uri);
            throw;
        }
    }

    protected async Task<bool> Delete(string uri)
    {
        try
        {
            var client = await CreateClientAsync();
            var response = await client.DeleteAsync(uri);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("HTTP DELETE request returned non-success status for {Uri}. Status: {StatusCode}",
                    uri, response.StatusCode);
            }

            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "HTTP DELETE request failed for {Uri}. Status: {StatusCode}", uri, ex.StatusCode);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during DELETE request to {Uri}", uri);
            throw;
        }
    }
}