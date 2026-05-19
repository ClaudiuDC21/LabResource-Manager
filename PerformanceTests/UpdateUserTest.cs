using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using NBomber.CSharp;

namespace PerformanceTests;

public static class UpdateUserTest
{
    public static void Run(string cleanUrl, string vsaUrl, string cleanToken, string vsaToken)
    {
        var targetUserIdClean = "15bfe041-9a43-4b66-bb18-954bfef15aa6";
        var targetUserIdVsa = "8a0f3eee-3508-4422-bae0-752ccdc6ceed";

        using var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        using var httpClient = new HttpClient(handler);

        var scenarioClean = Scenario.Create("CleanArchitecture_UpdateUser", async context =>
        {
            try
            {
                var uniqueSuffix = Guid.NewGuid().ToString("N").Substring(0, 6);

                var payload = new
                {
                    FullName = $"Updated User {uniqueSuffix}",
                    MatriculationNumber = $"MAT-{uniqueSuffix}"
                };

                using var request = new HttpRequestMessage(HttpMethod.Put, $"{cleanUrl}/api/users/{targetUserIdClean}");
                request.Content = JsonContent.Create(payload);

                if (!string.IsNullOrWhiteSpace(cleanToken))
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", cleanToken);

                var response = await httpClient.SendAsync(request);
                return response.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
            }
            catch { return Response.Fail(); }
        })
        .WithLoadSimulations(Simulation.Inject(rate: 30, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30)));

        var scenarioVsa = Scenario.Create("VerticalSlice_UpdateUser", async context =>
        {
            try
            {
                var uniqueSuffix = Guid.NewGuid().ToString("N").Substring(0, 6);

                var payload = new
                {
                    Id = targetUserIdVsa,
                    FullName = $"Updated User {uniqueSuffix}",
                    MatriculationNumber = $"MAT-{uniqueSuffix}"
                };

                using var request = new HttpRequestMessage(HttpMethod.Put, $"{vsaUrl}/api/users/{targetUserIdVsa}");
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
            .WithTestName("UpdateUser_LoadTest")
            .WithReportFileName("UpdateUser_Report")
            .WithReportFolder("./reports/UpdateUser")
            .Run();
    }
}