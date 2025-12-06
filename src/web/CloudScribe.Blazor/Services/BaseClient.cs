using System.Net.Http.Headers;
using CloudScribe.Blazor.Services.Auth;

namespace CloudScribe.Blazor.Services;

public abstract class BaseClient(IHttpClientFactory factory, TokenService tokenService, string apiName)
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
        var client = await CreateClientAsync();
        return await client.GetFromJsonAsync<T>(uri);
    }

    protected async Task Post<T>(string uri, T value)
    {
        var client = await CreateClientAsync();
        var response = await client.PostAsJsonAsync(uri, value);
        
        response.EnsureSuccessStatusCode();
    }

    protected async Task Put<T>(string uri, T value)
    {
        var client = await CreateClientAsync();
        var response = await client.PutAsJsonAsync(uri, value);
        
        response.EnsureSuccessStatusCode();
    }

    protected async Task<bool> Delete(string uri)
    {
        var client = await CreateClientAsync();
        var response = await client.DeleteAsync(uri);
        return response.IsSuccessStatusCode;
    }
}