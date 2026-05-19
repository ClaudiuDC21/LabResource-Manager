using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using NBomber.CSharp;

namespace PerformanceTests;

public static class RequestAssetTest
{
    public static void Run(string cleanUrl, string vsaUrl, string cleanToken, string vsaToken)
    {
        var targetUserIdClean = "e7a6273b-3b0d-4ba8-b911-ac17454d3dd4";
        var targetAssetIdClean = "3cb959cf-782b-4948-8695-009ef531df8d";

        var targetUserIdVsa = "c2c7b9cb-50cf-4367-a729-b5ddda758bc4";
        var targetAssetIdVsa = "b25bc97c-1c2d-4916-a327-103638f89399";

        using var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        using var httpClient = new HttpClient(handler);

        var scenarioClean = Scenario.Create("CleanArchitecture_RequestAsset", async context =>
        {
            try
            {
                var payload = new
                {
                    UserId = targetUserIdClean,
                    LabAssetId = targetAssetIdClean,
                    RequestedStartDate = DateTime.UtcNow.AddDays(1),
                    RequestedEndDate = DateTime.UtcNow.AddDays(5)
                };

                using var request = new HttpRequestMessage(HttpMethod.Post, $"{cleanUrl}/api/borrowings");
                request.Content = JsonContent.Create(payload);

                if (!string.IsNullOrWhiteSpace(cleanToken))
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", cleanToken);

                var response = await httpClient.SendAsync(request);
                return response.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
            }
            catch { return Response.Fail(); }
        })
        .WithLoadSimulations(Simulation.Inject(rate: 30, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30)));

        var scenarioVsa = Scenario.Create("VerticalSlice_RequestAsset", async context =>
        {
            try
            {
                var payload = new
                {
                    UserId = targetUserIdVsa,
                    LabAssetId = targetAssetIdVsa,
                    RequestedStartDate = DateTime.UtcNow.AddDays(1),
                    RequestedEndDate = DateTime.UtcNow.AddDays(5)
                };

                using var request = new HttpRequestMessage(HttpMethod.Post, $"{vsaUrl}/api/borrowings");
                request.Content = JsonContent.Create(payload);

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
            .WithTestName("RequestAsset_LoadTest")
            .WithReportFileName("RequestAsset_Report")
            .WithReportFolder("./reports/RequestAsset")
            .Run();
    }
}