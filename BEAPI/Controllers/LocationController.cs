using BEAPI.Dtos.Common;
using BEAPI.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;

[ApiController]
[Route("api/[controller]")]
public class LocationController : ControllerBase
{
    private readonly ILocationService _locationService;
    private readonly IinternalLocationService _internalLocationService;

    public LocationController(ILocationService locationService, IinternalLocationService internalLocationService)
    {
        _locationService = locationService;
        _internalLocationService = internalLocationService;
    }

    [HttpPost("[action]")]
    public async Task<IActionResult> SyncAll()
    {
        await _locationService.SyncAllAsync();
        return Ok(new ResponseDto { Data = null, Message = "All Provinces, Districts, and Wards synced" });
    }
    [HttpGet("[action]")]
    public async Task<IActionResult> GetProvinces()
    {
        var provinces = await _internalLocationService.GetProvincesAsync();
        return Ok(new ResponseDto
        {
            Message = "Get provinces successfully",
            Data = provinces
        });
    }

    [HttpGet("[action]/{provinceId}")]
    public async Task<IActionResult> GetDistricts(int provinceId)
    {
        try
        {
            var districts = await _internalLocationService.GetDistrictsByProvinceAsync(provinceId);
            return Ok(new ResponseDto
            {
                Message = "Get districts successfully",
                Data = districts
            });
        }
        catch (Exception ex)
        {
            return NotFound(new ResponseDto
            {
                Message = ex.Message,
                Data = null
            });
        }
    }

    [HttpGet("[action]/{districtId}")]
    public async Task<IActionResult> GetWards(int districtId)
    {
        try
        {
            var wards = await _internalLocationService.GetWardsByDistrictAsync(districtId);
            return Ok(new ResponseDto
            {
                Message = "Get wards successfully",
                Data = wards
            });
        }
        catch (Exception ex)
        {
            return NotFound(new ResponseDto
            {
                Message = ex.Message,
                Data = null
            });
        }
    }
}
