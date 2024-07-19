namespace CEAconsole.Thermo.Services
{
    public abstract class BaseService(HttpClient httpClient)
    {
        protected readonly HttpClient _httpClient = httpClient;
        // TODO add common methods
    }
}
