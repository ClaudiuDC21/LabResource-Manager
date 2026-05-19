using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace PerformanceTests;

class Program
{
    static void Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var cleanToken = configuration["JwtSettings:CleanToken"] ?? "";
        var vsaToken = configuration["JwtSettings:VsaToken"] ?? "";

        var cleanUrl = configuration["Urls:CleanArch"] ?? "https://localhost:6001";
        var vsaUrl = configuration["Urls:Vsa"] ?? "https://localhost:5001";

        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== LabResource Performance Tests (NBomber) ===");
            Console.WriteLine($"Environment: Clean ({cleanUrl}), VSA ({vsaUrl})");
            Console.WriteLine($"Security: Clean Token ({(string.IsNullOrEmpty(cleanToken) ? "MISSING" : "OK")}), VSA Token ({(string.IsNullOrEmpty(vsaToken) ? "MISSING" : "OK")})");
            Console.WriteLine("-----------------------------------------------");
            Console.WriteLine("1. Get Asset By Id (LOW)");
            Console.WriteLine("2. Get All Users (LOW)");
            Console.WriteLine("3. Create Asset (MEDIUM)");
            Console.WriteLine("4. Update User (MEDIUM)");
            Console.WriteLine("5. Request Asset (HIGH)");
            Console.WriteLine("6. Return Asset (HIGH)");
            Console.WriteLine("0. Exit");
            Console.WriteLine("===============================================");
            Console.Write("Select the test you want to run (0-6): ");

            var choice = Console.ReadLine();
            Console.WriteLine();

            switch (choice)
            {
                case "1":
                    Console.WriteLine("--> Starting Load Test for GetAssetById...");
                    GetAssetByIdTest.Run(cleanUrl, vsaUrl, cleanToken, vsaToken);
                    break;
                case "2":
                    Console.WriteLine("--> Starting Load Test for GetAllUsers...");
                    GetAllUsersTest.Run(cleanUrl, vsaUrl, cleanToken, vsaToken);
                    break;
                case "3":
                    Console.WriteLine("--> Starting Load Test for CreateAsset...");
                    CreateAssetTest.Run(cleanUrl, vsaUrl, cleanToken, vsaToken);
                    break;
                case "4":
                    Console.WriteLine("--> Starting Load Test for UpdateUser...");
                    UpdateUserTest.Run(cleanUrl, vsaUrl, cleanToken, vsaToken);
                    break;
                case "5":
                    Console.WriteLine("--> Starting Load Test for RequestAsset...");
                    RequestAssetTest.Run(cleanUrl, vsaUrl, cleanToken, vsaToken);
                    break;
                case "6":
                    Console.WriteLine("--> Starting Load Test for ReturnAsset...");
                    ReturnAssetTest.Run(cleanUrl, vsaUrl, cleanToken, vsaToken);
                    break;
                case "0":
                    return;
                default:
                    Console.WriteLine("Invalid option.");
                    break;
            }

            Console.WriteLine("\nPress Enter to continue...");
            Console.ReadLine();
        }
    }
}