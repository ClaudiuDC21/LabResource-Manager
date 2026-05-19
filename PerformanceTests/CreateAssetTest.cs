using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using NBomber.CSharp;

namespace PerformanceTests;

public static class CreateAssetTest
{
    public static void Run(string cleanUrl, string vsaUrl, string cleanToken, string vsaToken)
    {
        using var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        using var httpClient = new HttpClient(handler);

        var scenarioClean = Scenario.Create("CleanArchitecture_CreateAsset", async context =>
        {
            try
            {
                var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
                var payload = new
                {
                    Name = $"Asset-Clean-{uniqueId}",
                    SerialNumber = $"SN-C-{uniqueId}",
                    Location = "LoadTest-Lab",
                    AssignedTeacherId = (Guid?)null
                };

                using var request = new HttpRequestMessage(HttpMethod.Post, $"{cleanUrl}/api/labassets");
                request.Content = JsonContent.Create(payload);

                if (!string.IsNullOrWhiteSpace(cleanToken))
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", cleanToken);

                var response = await httpClient.SendAsync(request);
                return response.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
            }
            catch { return Response.Fail(); }
        })
        .WithLoadSimulations(Simulation.Inject(rate: 30, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30)));

        var scenarioVsa = Scenario.Create("VerticalSlice_CreateAsset", async context =>
        {
            try
            {
                var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
                var payload = new
                {
                    Name = $"Asset-VSA-{uniqueId}",
                    SerialNumber = $"SN-V-{uniqueId}",
                    Location = "LoadTest-Lab",
                    AssignedTeacherId = (Guid?)null
                };

                using var request = new HttpRequestMessage(HttpMethod.Post, $"{vsaUrl}/api/labassets");
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
            .WithTestName("CreateAsset_LoadTest")
            .WithReportFileName("CreateAsset_Report")
            .WithReportFolder("./reports/CreateAsset")
            .Run();
    }
}