using BEAPI.Dtos.Common;
using BEAPI.Services;
using BEAPI.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace BEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService) { 
            _roleService = roleService;
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetRolesAsync()
        {
            var provinces = await _roleService.GetRolesAsync();
            return Ok(new ResponseDto
            {
                Message = "Get Roles successfully",
                Data = provinces
            });
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> GetRolesFilterAsync()
        {
            var provinces = await _roleService.GetRolesFilterAsync();
            return Ok(new ResponseDto
            {
                Message = "Get Roles successfully",
                Data = provinces
            });
        }
    }
}
