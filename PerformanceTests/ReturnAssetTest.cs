using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using NBomber.CSharp;

namespace PerformanceTests;

public static class ReturnAssetTest
{
    public static void Run(string cleanUrl, string vsaUrl, string cleanToken, string vsaToken)
    {
        var targetBorrowingIdClean = "858c0bbc-4b6f-4fff-ac04-7afaf20e5b6a";
        var targetBorrowingIdVsa = "b032b7a8-7c95-443c-adc1-b641d0474b8e";

        using var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        using var httpClient = new HttpClient(handler);

        var scenarioClean = Scenario.Create("CleanArchitecture_ReturnAsset", async context =>
        {
            try
            {
                var payload = new
                {
                    Remarks = "Returned during load test",
                    IsDefective = false
                };

                using var request = new HttpRequestMessage(HttpMethod.Post, $"{cleanUrl}/api/borrowings/{targetBorrowingIdClean}/return");
                request.Content = JsonContent.Create(payload);

                if (!string.IsNullOrWhiteSpace(cleanToken))
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", cleanToken);

                var response = await httpClient.SendAsync(request);
                return response.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
            }
            catch { return Response.Fail(); }
        })
        .WithLoadSimulations(Simulation.Inject(rate: 30, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30)));

        var scenarioVsa = Scenario.Create("VerticalSlice_ReturnAsset", async context =>
        {
            try
            {
                var payload = new
                {
                    Remarks = "Returned during load test",
                    IsDefective = false
                };

                using var request = new HttpRequestMessage(HttpMethod.Post, $"{vsaUrl}/api/borrowings/{targetBorrowingIdVsa}/return");
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
            .WithTestName("ReturnAsset_LoadTest")
            .WithReportFileName("ReturnAsset_Report")
            .WithReportFolder("./reports/ReturnAsset")
            .Run();
    }
}