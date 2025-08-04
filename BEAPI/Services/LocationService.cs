using BEAPI.Database;
using BEAPI.Entities;
using BEAPI.Services.IServices;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

public class LocationService : ILocationService
{
    private readonly BeContext _context;
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _token;

    public LocationService(BeContext context, HttpClient httpClient, IConfiguration config)
    {
        _context = context;
        _httpClient = httpClient;
        _baseUrl = config["GHN:BaseUrl"];
        _token = config["GHN:Token"];
    }

    private async Task<JsonElement> GetJsonAsync(string endpoint, HttpMethod method, object? body = null)
    {
        var request = new HttpRequestMessage(method, $"{_baseUrl}/{endpoint}");
        request.Headers.Add("Token", _token);

        if (body != null)
        {
            request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        }

        var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Error {response.StatusCode}: {content}");

        return JsonDocument.Parse(content).RootElement.GetProperty("data");
    }

    public async Task SyncProvincesAsync()
    {
        var data = await GetJsonAsync("master-data/province", HttpMethod.Get);

        var apiProvinces = data.EnumerateArray()
            .Select(item => new Province
            {
                ProvinceID = item.GetProperty("ProvinceID").GetInt32(),
                ProvinceName = item.GetProperty("ProvinceName").GetString()!,
                Code = item.TryGetProperty("Code", out var code) ? code.GetString() : null,
                IsDeleted = false
            })
            .ToList();

        var dbProvinces = await _context.Provinces.ToListAsync();

        foreach (var dbProvince in dbProvinces)
        {
            if (!apiProvinces.Any(p => p.ProvinceID == dbProvince.ProvinceID))
            {
                dbProvince.IsDeleted = true;
                _context.Provinces.Update(dbProvince);
            }
        }

        foreach (var apiProvince in apiProvinces)
        {
            var existing = dbProvinces.FirstOrDefault(p => p.ProvinceID == apiProvince.ProvinceID);

            if (existing == null)
            {
                _context.Provinces.Add(apiProvince);
            }
            else
            {
                existing.ProvinceName = apiProvince.ProvinceName;
                existing.Code = apiProvince.Code;
                existing.IsDeleted = false;

                _context.Provinces.Update(existing);
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task SyncDistrictsAsync()
    {
        var data = await GetJsonAsync("master-data/district", HttpMethod.Get);

        var apiDistricts = data.EnumerateArray()
            .Select(item => new District
            {
                DistrictID = item.GetProperty("DistrictID").GetInt32(),
                ProvinceID = item.GetProperty("ProvinceID").GetInt32(),
                DistrictName = item.GetProperty("DistrictName").GetString()!,
                Code = item.TryGetProperty("Code", out var code) ? code.GetString() : null,
                Type = item.TryGetProperty("Type", out var type) ? type.GetInt32() : 0,
                SupportType = item.TryGetProperty("SupportType", out var st) ? st.GetInt32() : 0,
                IsDeleted = false
            })
            .ToList();

        var dbDistricts = await _context.Districts.ToListAsync();

        foreach (var dbDistrict in dbDistricts)
        {
            if (!apiDistricts.Any(d => d.DistrictID == dbDistrict.DistrictID))
            {
                dbDistrict.IsDeleted = true;
                _context.Districts.Update(dbDistrict);
            }
        }

        foreach (var apiDistrict in apiDistricts)
        {
            var existing = dbDistricts.FirstOrDefault(d => d.DistrictID == apiDistrict.DistrictID);

            if (existing == null)
            {
                _context.Districts.Add(apiDistrict);
            }
            else
            {
                existing.ProvinceID = apiDistrict.ProvinceID;
                existing.DistrictName = apiDistrict.DistrictName;
                existing.Code = apiDistrict.Code;
                existing.Type = apiDistrict.Type;
                existing.SupportType = apiDistrict.SupportType;
                existing.IsDeleted = false;

                _context.Districts.Update(existing);
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task SyncAllWardsAsync(List<int> districtIds)
    {
        var dbWards = await _context.Wards.ToListAsync();
        var allTasks = districtIds.Select(districtId => SyncWardForDistrict(districtId, dbWards)).ToList();

        await Task.WhenAll(allTasks);

        await _context.SaveChangesAsync();
    }

    private async Task SyncWardForDistrict(int districtId, List<Ward> dbWards)
    {
        var data = await GetJsonAsync("master-data/ward", HttpMethod.Post, new { district_id = districtId });

        if (data.ValueKind != JsonValueKind.Array) return;

        var apiWards = data.EnumerateArray()
            .Select(item => new Ward
            {
                WardCode = item.GetProperty("WardCode").GetString()!,
                DistrictID = item.GetProperty("DistrictID").GetInt32(),
                WardName = item.GetProperty("WardName").GetString()!,
                IsDeleted = false
            })
            .ToList();

        var dbWardsForDistrict = dbWards.Where(w => w.DistrictID == districtId).ToList();

        foreach (var dbWard in dbWardsForDistrict)
        {
            if (!apiWards.Any(w => w.WardCode == dbWard.WardCode))
                dbWard.IsDeleted = true;
        }

        foreach (var apiWard in apiWards)
        {
            var existing = dbWardsForDistrict.FirstOrDefault(w => w.WardCode == apiWard.WardCode);

            if (existing == null)
            {
                _context.Wards.Add(apiWard);
                dbWards.Add(apiWard);
            }
            else
            {
                existing.WardName = apiWard.WardName;
                existing.IsDeleted = false;
            }
        }
    }

    public async Task SyncAllAsync()
    {
        await SyncProvincesAsync();
        await SyncDistrictsAsync();

        var districtIds = await _context.Districts
                                        .Select(d => d.DistrictID)
                                        .ToListAsync();

        await SyncAllWardsAsync(districtIds);

        await _context.SaveChangesAsync();
    }


}
