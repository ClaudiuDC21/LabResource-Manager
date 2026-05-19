using System;
using System.Net.Http;
using System.Net.Http.Headers;
using NBomber.CSharp;

namespace PerformanceTests;

public static class GetAssetByIdTest
{
    public static void Run(string cleanUrl, string vsaUrl, string cleanToken, string vsaToken)
    {
        var targetAssetIdClean = "bc27cf57-c8f8-4a67-bf65-74b0434257cb";
        var targetAssetIdVsa = "15ee549a-32fc-47da-8cf6-c39a771a1c36";

        using var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        using var httpClient = new HttpClient(handler);

        var scenarioClean = Scenario.Create("CleanArchitecture_GetAssetById", async context =>
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, $"{cleanUrl}/api/labassets/{targetAssetIdClean}");
                if (!string.IsNullOrWhiteSpace(cleanToken))
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", cleanToken);

                var response = await httpClient.SendAsync(request);
                return response.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
            }
            catch { return Response.Fail(); }
        })
        .WithLoadSimulations(Simulation.Inject(rate: 30, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30)));

        var scenarioVsa = Scenario.Create("VerticalSlice_GetAssetById", async context =>
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, $"{vsaUrl}/api/labassets/{targetAssetIdVsa}");
                if (!string.IsNullOrWhiteSpace(vsaToken))
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", vsaToken);

                var response = await httpClient.SendAsync(request);
                return response.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
            }
            catch { return Response.Fail(); }
        })
        .WithLoadSimulations(Simulation.Inject(rate: 30, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30)));

        NBomberRunner
            .RegisterScenarios(scenarioClean, scenarioVsa)
            .WithTestSuite("LabResource_Architecture_Comparison") 
            .WithTestName("GetAssetById_LoadTest")
            .WithReportFileName("GetAssetById_Report")
            .WithReportFolder("./reports/GetAssetById")
            .Run();
    }
}