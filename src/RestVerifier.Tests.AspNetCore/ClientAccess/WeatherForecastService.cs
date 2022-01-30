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

    public async Task GetMethod2Void()
    {
        var url = GetUrl("weatherforecast/GetMethod2");

        await Execute(async httpClient =>
        {
            var res = await httpClient.GetAsync(url).ConfigureAwait(false);
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


    public async Task<string> GetPersonName(Guid id)
    {
        var url = GetUrl($"weatherforecast/GetPerson?id={id}");

        return await Execute(async httpClient =>
        {
            var res = await httpClient.GetFromJsonAsync<PersonDTO>(url, CreateOptions()).ConfigureAwait(false);
            return res!.Name;
        }).ConfigureAwait(false);
    }
    public async Task<string> GetPersonNameAction(Guid id)
    {
        var url = GetUrl($"weatherforecast/GetPersonAction?id={id}");

        return await Execute(async httpClient =>
        {
            var res = await httpClient.GetFromJsonAsync<PersonDTO>(url, CreateOptions()).ConfigureAwait(false);
            return res!.Name;
        }).ConfigureAwait(false);
    }

    public async Task UpdatePersonName(Guid id,string name)
    {
        var url = GetUrl($"weatherforecast/UpdatePersonName");

        await Execute(async httpClient =>
        {
            var res = await httpClient.PostAsJsonAsync(url, (id:id, personName: name), CreateOptions()).ConfigureAwait(false);
            return res;
        }).ConfigureAwait(false);
    }

    public async Task<string> ParametersOrder(string address,string name)
    {
        var url = GetUrl($"weatherforecast/ParametersOrder?address={address}&name={name}");

        return await Execute(async httpClient =>
        {
            var res = await httpClient.GetFromJsonAsync<string>(url, CreateOptions()).ConfigureAwait(false);
            return res!;
        }).ConfigureAwait(false);
    }

    public async Task<string> ParametersOrderWithDifferentParamNames(string address1, string name1)
    {
        var url = GetUrl($"weatherforecast/ParametersOrder?address={address1}&name={name1}");

        return await Execute(async httpClient =>
        {
            var res = await httpClient.GetFromJsonAsync<string>(url, CreateOptions()).ConfigureAwait(false);
            return res!;
        }).ConfigureAwait(false);
    }

    public async Task GetPersonNoReturn(PersonDTO person)
    {
        var url = GetUrl($"weatherforecast/GetPerson?id={person.Id}");

        await Execute(async httpClient =>
        {
            var res = await httpClient.GetFromJsonAsync<PersonDTO>(url, CreateOptions()).ConfigureAwait(false);
            return res!;
        }).ConfigureAwait(false);
    }

    public void WrongAsync()
    {
        var url = GetUrl($"weatherforecast/GetPerson");

        Execute(async httpClient =>
        {
            var res = httpClient.GetFromJsonAsync<PersonDTO>(url, CreateOptions()).ConfigureAwait(false);
            return res!;
        }).ConfigureAwait(false);
    }
}