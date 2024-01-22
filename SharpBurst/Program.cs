using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

class Program
{
    static async Task Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("Sharpburst.exe <action> <params>");
            return;
        }

        string action = args[0];

        switch (action.ToLower())
        {
            case "gettenantid":
                await GetTenantID(args);
                break;
            // Add more cases for other actions if needed
            default:
                Console.WriteLine($"Unknown action: {action}");
                break;
        }
    }

    public static async Task GetTenantID(string[] args)
    {
        if (args.Length != 2)
        {
            Console.WriteLine("Usage: Sharpburst.exe GetTenantID <domain>");
            return;
        }

        string domain = args[1];
        string url = $"https://login.microsoft.com/{domain}/.well-known/openid-configuration";

        Dictionary<string, string> requestHeaders = new Dictionary<string, string>
        {
            { "Accept", "application/json" }
            // Add more headers if needed
        };

        try
        {
            string result = await MakeHttpRequest(url, "GET", requestHeaders);
            string tokenEndpoint = GetTokenEndpoint(result);
            string[] endpointParts = SplitAndPrintFourthElement(tokenEndpoint);

            Console.WriteLine($"Tenant ID: {endpointParts[3]}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    public static async Task<string> MakeHttpRequest(string url, string method, Dictionary<string, string> requestHeaders)
    {
        HttpClient client = new HttpClient();
        HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(method), url);

        foreach (var header in requestHeaders)
        {
            request.Headers.Add(header.Key, header.Value);
        }

        HttpResponseMessage response = await client.SendAsync(request);
        string content = await response.Content.ReadAsStringAsync();

        return content;
    }

    public static string GetTokenEndpoint(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return "Invalid JSON response";
        }

        try
        {
            dynamic parsedJson = JsonConvert.DeserializeObject(json);
            return parsedJson["token_endpoint"];
        }
        catch (JsonException)
        {
            return "Error parsing JSON";
        }
    }

    public static string[] SplitAndPrintFourthElement(string tokenEndpoint)
    {
        if (string.IsNullOrEmpty(tokenEndpoint))
        {
            return new string[] { "Invalid Token Endpoint" };
        }

        try
        {
            string[] endpointParts = tokenEndpoint.Split('/');

            return endpointParts;
        }
        catch (Exception)
        {
            return new string[] { "Error splitting Token Endpoint" };
        }
    }
}
