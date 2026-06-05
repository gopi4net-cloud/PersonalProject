using ANGDDEAPIBO;
using ANGDDEAPIDO;
using ANGDDEAPIFoundation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ANGDDEAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class BEQAPIController : ControllerBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DOConfiguration _objConfiguration;
        private readonly BOCommon _bOCommon;
        public BEQAPIController(IHttpContextAccessor httpContextAccessor,DOConfiguration objConfiguration,BOCommon bOCommon)
        {
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _bOCommon = bOCommon;
        }
        // GET: api/<BEQAPIController>
        [HttpGet]
        public ActionResult GetBEQDetails([FromBody] DOBEQRequest doBEQRequest)
        {
            try
            { 
                
                BOBEQ objBOBEQAPI = new BOBEQ(_objConfiguration);
                BeqBenData objBEQResponse = null;
                ExceptionTypes result = objBOBEQAPI.GetBEQDetails(doBEQRequest, out objBEQResponse);
                if (result != ExceptionTypes.Success && result != ExceptionTypes.ZeroRecords)
                {
                    return BadRequest();
                }

                return Ok(objBEQResponse);

            }
            catch(Exception ex)
            {
                return BadRequest();
            }
        }

    }
}
