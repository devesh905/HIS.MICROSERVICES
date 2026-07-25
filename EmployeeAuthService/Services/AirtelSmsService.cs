using Newtonsoft.Json;
using RestSharp;

namespace EmployeeAuthService.Services;

public class AirtelSmsService : ISmsService
{
    private readonly IConfiguration _config;

    public AirtelSmsService(IConfiguration config) => _config = config;

    private string SendReceiptdetailSMS(string mobileNumber, string message)
    {
        try
        {
            var baseUrl = _config["AirtelSmsApi:BaseUrl"]!;
            var customerId = _config["AirtelSmsApi:CustomerId"]!;
            var authToken = _config["AirtelSmsApi:AuthorizationToken"]!;
            var sourceAddress = _config["AirtelSmsApi:SourceAddress"]!;
            var messageType = _config["AirtelSmsApi:MessageType"]!;
            //var dltTemplateId = _config["AirtelSmsApi:ReceiptTemplateId"]!;
            var dltTemplateId = _config["AirtelSmsApi:OTPTemplateId"]!;
            var entityId = _config["AirtelSmsApi:EntityId"]!;

            var client = new RestSharp.RestClient($"{baseUrl}/sendSms?customerId={customerId}");

            var request = new RestSharp.RestRequest(RestSharp.Method.POST);
            request.Timeout = -1;
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", authToken);
            var smsObj = new
            {
                customerId = customerId,
                destinationAddress = new List<string> { mobileNumber },
                message = message,
                sourceAddress = sourceAddress,
                messageType = messageType,
                dltTemplateId = dltTemplateId,
                entityId = entityId
            };
            string jsonString = JsonConvert.SerializeObject(smsObj);
            request.AddParameter("application/json", jsonString, ParameterType.RequestBody);
            //RestResponse response = client.Execute(request);
            RestSharp.IRestResponse response = client.Execute(request);
            //Console.WriteLine(response.Content);
            return $"API_RESPONSE: {response.Content}";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending Text: {ex.Message}");
            return "FAIL_MOBILE_SMS";
        }
    }

    public async Task SendAsync(string mobileNumber, string message)
    {
        try
        {
            // Run the existing synchronous helper on the thread pool to avoid blocking callers.
            var result = await Task.Run(() => SendReceiptdetailSMS(mobileNumber, message));

            if (string.IsNullOrWhiteSpace(result) || result.ToUpperInvariant() == "FAIL_MOBILE_SMS")
                throw new Exception($"Airtel SMS failed: {result}");
        }
        catch (Exception ex)
        {
            // Propagate a meaningful exception to callers.
            throw new Exception($"AirtelSmsService.SendAsync failed: {ex.Message}", ex);
        }
    }
}

