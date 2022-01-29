using System.Data;
using System.Net;
using System.Security.Authentication;
using System.Text;
using Newtonsoft.Json;
using RestVerifier.Tests.AspNetCore.Model;

namespace RestVerifier.Tests.AspNetCore.ClientAccess;

public static class NewtonsoftHttpClientExtensions
{
    public static async Task<T> GetFromJsonAsync<T>(this HttpClient httpClient, string uri, JsonSerializerSettings settings = null, CancellationToken cancellationToken = default)
    {
        ThrowIfInvalidParams(httpClient, uri);

        var response = await httpClient.GetAsync(uri, cancellationToken);

        //response.EnsureSuccessStatusCode();
        if (!response.IsSuccessStatusCode)
        {
            await ConvertToException(response);
        }

        var json = await response.Content.ReadAsStringAsync();

        return JsonConvert.DeserializeObject<T>(json, settings);
    }

    public static async Task<HttpResponseMessage> PostAsJsonAsync<T>(this HttpClient httpClient, string uri, T value, JsonSerializerSettings settings = null, CancellationToken cancellationToken = default)
    {
        ThrowIfInvalidParams(httpClient, uri);

        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        var json = JsonConvert.SerializeObject(value, settings);

        var res = await httpClient.PostAsync(uri, new StringContent(json, Encoding.UTF8, "application/json"), cancellationToken);

        if (!res.IsSuccessStatusCode)
        {
            await ConvertToException(res);
        }
        return res;
    }

    public static async Task<HttpResponseMessage> PostAsEmptyJsonAsync(this HttpClient httpClient, string uri, JsonSerializerSettings settings = null, CancellationToken cancellationToken = default)
    {
        ThrowIfInvalidParams(httpClient, uri);

        var json = JsonConvert.SerializeObject(new { }, settings);

        var res = await httpClient.PostAsync(uri, new StringContent(json, Encoding.UTF8, "application/json"), cancellationToken);

        if (!res.IsSuccessStatusCode)
        {
            await ConvertToException(res);
        }

        return res;
    }

    public static async Task<T> PostAsEmptyJsonAsync<T>(this HttpClient httpClient, string uri, JsonSerializerSettings settings = null, CancellationToken cancellationToken = default)
    {
        var res = await PostAsEmptyJsonAsync(httpClient, uri, settings, cancellationToken);

        var json = await res.Content.ReadAsStringAsync();

        return JsonConvert.DeserializeObject<T>(json, settings);
    }

    public static async Task ConvertToException(this HttpResponseMessage response)
    {
        var message = await response.Content.ReadAsStringAsync();
        var errorDetails = JsonConvert.DeserializeObject<ErrorDetails>(message);
        convertToException(response.StatusCode, errorDetails?.Message, errorDetails?.ServiceError);
    }

    public static void convertToException(HttpStatusCode? statusCode, string message, ServiceError? error = ServiceError.Unknown)
    {
        error ??= ServiceError.Unknown;
        message ??= "Unknown exception occured";
        switch (error)
        {
            case ServiceError.ObjectNotFoundException:
                throw new ObjectNotFoundException(message);
            case ServiceError.ConstraintException:
                throw new ConstraintException(message);
            case ServiceError.UnauthorizedAccessException:
                throw new UnauthorizedAccessException(message);
            case ServiceError.ArgumentNullException:
                throw new ArgumentNullException(message);
            case ServiceError.InvalidOperationException:
                throw new InvalidOperationException(message);
            case ServiceError.ArgumentOutOfRangeException:
                throw new ArgumentOutOfRangeException(message);
                throw new AuthenticationException(message);

        }


        switch (statusCode)
        {
            case HttpStatusCode.NotFound:
                throw new ObjectNotFoundException(message);
            case HttpStatusCode.Unauthorized:
                throw new UnauthorizedAccessException(message);
        }
        throw new Exception(message);
    }

    public static async Task<Result> PostAsJsonAsync<T, Result>(this HttpClient httpClient, string uri, T value, JsonSerializerSettings settings = null, CancellationToken cancellationToken = default)
    {
        var res = await PostAsJsonAsync<T>(httpClient, uri, value, settings, cancellationToken);


        var json = await res.Content.ReadAsStringAsync();

        return JsonConvert.DeserializeObject<Result>(json, settings);
    }

    private static void ThrowIfInvalidParams(HttpClient httpClient, string uri)
    {
        if (httpClient == null)
        {
            throw new ArgumentNullException(nameof(httpClient));
        }

        if (string.IsNullOrWhiteSpace(uri))
        {
            throw new ArgumentException("Can't be null or empty", nameof(uri));
        }
    }

}