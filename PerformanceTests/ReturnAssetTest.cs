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
        var targetBorrowingIdVsa = "6f7f1c68-0abb-40dc-9e62-0e6717703475";
        var targetBorrowingIdClean = "833861e0-8ef7-46f3-b431-be8f9ef1bdef";

        var cleanLabAssetId = "b6dc072f-b74e-4fcd-9e2c-0e80a40ffb6e";

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
                    LabAssetId = Guid.Parse(cleanLabAssetId),
                    Remarks = "Returned during load test",
                    IsDefective = false
                };

                using var request = new HttpRequestMessage(HttpMethod.Post, $"{cleanUrl}/api/borrowings/{targetBorrowingIdClean}/return");
                request.Content = JsonContent.Create(payload);

                if (!string.IsNullOrWhiteSpace(cleanToken))
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", cleanToken);

                var response = await httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode && context.InvocationNumber == 0)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[CLEAN FAIL] Status: {response.StatusCode} | Msg: {error}");
                }

                return response.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CRITICAL ERROR] {ex.Message}");
                return Response.Fail();
            }
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

                if (!response.IsSuccessStatusCode && context.InvocationNumber == 0)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[VSA FAIL] Status: {response.StatusCode} | Msg: {error}");
                }

                return response.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CRITICAL ERROR] {ex.Message}");
                return Response.Fail();
            }
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