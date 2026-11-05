namespace Beep.OilandGas.Web.Services;
public sealed partial class DrillingServiceClient(ApiClient api):IDrillingServiceClient
{
    private readonly ApiClient _apiClient=api;
}
