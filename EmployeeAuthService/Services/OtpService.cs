using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;

namespace EmployeeAuthService.Services;

public class OtpService : IOtpService
{
    private readonly IMemoryCache _cache;

    public OtpService(IMemoryCache cache) => _cache = cache;

    public Task<string> GenerateAsync(string mobile)
    {
        var otp = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        _cache.Set($"otp:{mobile}", otp, TimeSpan.FromMinutes(5));
        return Task.FromResult(otp);
    }

    public Task<bool> VerifyAsync(string mobile, string otp)
    {
        var cached = _cache.Get<string>($"otp:{mobile}");
        if (cached != null && cached == otp)
        {
            _cache.Remove($"otp:{mobile}");
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }
}