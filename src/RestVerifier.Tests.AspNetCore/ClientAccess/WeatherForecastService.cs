using RestVerifier.Tests.AspNetCore.Model;

namespace RestVerifier.Tests.AspNetCore.ClientAccess;

public class WeatherForecastService: DataServiceBase
{
    private HttpClient _client;

    public WeatherForecastService(HttpClient client):base(client)
    {
        _client = client;
    }

    public async Task<IEnumerable<WeatherForecast>> GetMethod1()
    {
        var url = GetUrl("weatherforecast/GetMethod1");

        return await Execute(async httpClient =>
        {
            var res = await httpClient.GetFromJsonAsync<IEnumerable<WeatherForecast>>(url, CreateOptions()).ConfigureAwait(false);
            return res!;
        }).ConfigureAwait(false);
    }

    public async Task<IEnumerable<WeatherForecast>> GetMethod2()
    {
        var url = GetUrl("weatherforecast/GetMethod2");

        return await Execute(async httpClient =>
        {
            var res = await httpClient.GetFromJsonAsync<IEnumerable<WeatherForecast>>(url, CreateOptions()).ConfigureAwait(false);
            return res!;
        }).ConfigureAwait(false);
    }

    public async Task<PersonDTO> GetPerson(PersonDTO person)
    {
        var url = GetUrl($"weatherforecast/GetPerson?id={person.Id}");

        return await Execute(async httpClient =>
        {
            var res = await httpClient.GetFromJsonAsync<PersonDTO>(url, CreateOptions()).ConfigureAwait(false);
            return res!;
        }).ConfigureAwait(false);
    }

    public async Task<PersonDTO> GetPersonAction(Guid id)
    {
        var url = GetUrl($"weatherforecast/GetPersonAction?id={id}");

        return await Execute(async httpClient =>
        {
            var res = await httpClient.GetFromJsonAsync<PersonDTO>(url, CreateOptions()).ConfigureAwait(false);
            return res!;
        }).ConfigureAwait(false);
    }

    public async Task<PersonDTO> GetPersonTaskAction(Guid id)
    {
        var url = GetUrl($"weatherforecast/GetPersonTaskAction?id={id}");

        return await Execute(async httpClient =>
        {
            var res = await httpClient.GetFromJsonAsync<PersonDTO>(url, CreateOptions()).ConfigureAwait(false);
            return res!;
        }).ConfigureAwait(false);
    }
}