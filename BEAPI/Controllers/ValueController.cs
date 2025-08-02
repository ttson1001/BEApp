using BEAPI.Constants;
using BEAPI.Dtos.Common;
using BEAPI.Dtos.ListOfValue;
using BEAPI.Dtos.Value;
using BEAPI.Exceptions;
using BEAPI.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ValueController : ControllerBase
    {
        private readonly IListOfValueService _listOfValueService;
        private readonly IValueService _valueService;
        public ValueController(IListOfValueService listOfValueService, IValueService valueService)
        {
            _listOfValueService = listOfValueService;
            _valueService = valueService;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("[action]")]
        public async Task<IActionResult> CreateListOfValue([FromBody] ListOfValueCreateDto dto)
        {
            var res = new ResponseDto();
            try
            {
                await _listOfValueService.Create(dto);
                res.Message = MessageConstants.ListOfValueSuccess;
                return StatusCode(StatusCodes.Status201Created, res);
            }
            catch (Exception ex)
            {
                res.Message = ex.Message;
                if(ex.Message == ExceptionConstant.ListOfValueAlreadyExists)
                {
                    return Conflict(res);
                }
                return BadRequest(res);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("[action]")]
        public async Task<IActionResult> UpdateListOfValue([FromBody] ListOfValueUpdateDto dto)
        {
            var res = new ResponseDto();
            try
            {
                await _listOfValueService.Update(dto);
                res.Message = MessageConstants.ListOfValueSuccess;
                return Ok(res);
            }
            catch (Exception ex)
            {
                res.Message = ex.Message;
                if (ex.Message == ExceptionConstant.ListOfValueNotFounds)
                {
                    return Conflict(res);
                }
                return BadRequest(res);
            }
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetAllListOfValue()
        {
            try
            {
                var result = await _listOfValueService.GetAllAsync();
                return Ok(new ResponseDto
                {
                    Message = "Get all ListOfValue successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message });
            }
        }

        [HttpGet("[action]/{id}")]
        public async Task<IActionResult> GetListOfValueById(string id)
        {
            try
            {
                var result = await _listOfValueService.GetByIdAsync(id);
                return Ok(new ResponseDto
                {
                    Message = "Get ListOfValue by Id successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message });
            }
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetListOfValueByNote([FromQuery] string note)
        {
            try
            {
                var result = await _listOfValueService.GetByNoteAsync(note);
                return Ok(new ResponseDto
                {
                    Message = "Get ListOfValue by Note successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message });
            }
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetAllValues()
        {
            try
            {
                var result = await _valueService.GetAllValuesAsync();
                return Ok(new ResponseDto
                {
                    Message = "Get all values successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message });
            }
        }

        [HttpGet("[action]/{listOfValueId}")]
        public async Task<IActionResult> GetValuesByListId(string listOfValueId)
        {
            try
            {
                var result = await _valueService.GetValuesByListIdAsync(listOfValueId);
                return Ok(new ResponseDto
                {
                    Message = "Get values by ListOfValueId successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message });
            }
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetValuesByNote([FromQuery] string note)
        {
            try
            {
                var result = await _valueService.GetValuesByNoteAsync(note);
                return Ok(new ResponseDto
                {
                    Message = "Get values by note successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message });
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> CreateValue ([FromBody] ValueCreateDto dto)
        {
            try
            {
                await _valueService.Create(dto);
                return StatusCode(StatusCodes.Status201Created, new ResponseDto
                {
                    Message = "Value created successfully",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message });
            }
        }

        [HttpPut("[action]")]
        public async Task<IActionResult> UpdateValue([FromBody] ValueUpdateDto dto)
        {
            try
            {
                 await _valueService.Update(dto);
                return Ok(new ResponseDto
                {
                    Message = "Value updated successfully",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message });
            }
        }

    }
}
