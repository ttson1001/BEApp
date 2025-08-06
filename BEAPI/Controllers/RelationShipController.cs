using BEAPI.Dtos.Category;
using BEAPI.Dtos.Common;
using BEAPI.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace BEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RelationShipController : ControllerBase
    {
        private readonly IRelationShipService _relationShipService;

        public RelationShipController(IRelationShipService relationShipService)
        {
            _relationShipService = relationShipService;
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetListValueRelationship()
        {
            try
            {
                var rs = await _relationShipService.GetListValueRelationship();
                return Ok(new ResponseDto
                {
                    Message = "Relationship getlist successfully",
                    Data = rs,
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto
                {
                    Data = null,
                    Message = ex.Message
                });
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> CreateValueOfRelationshipRoot([FromBody] List<CreateCategoryValueDto> dtos)
        {
            try
            {
                await _relationShipService.CreateListRelationship(dtos);
                return Ok(new ResponseDto
                {
                    Message = "Relationship created successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto
                {
                    Data = null,
                    Message = ex.Message
                });
            }
        }
    }
}
