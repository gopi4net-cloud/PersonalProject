using ANGDDEAPIBO;
using ANGDDEAPIDO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System;
using MongoDB.Bson.IO;
using Microsoft.Extensions.Configuration;
using ANGDDEAPIDO.Interface;
using ANGDDEAPIDO.ORS;
//using System.Web.Http.Results;

namespace ANGDDEAPI.Controllers
{

    //https://localhost:60148/api/starsmiim/getmembers?memberId=002314777-1&memFirstName=SONNY &memLastName=EARLEY&birthDate=1951-09-22

    [ApiController]
    [Route("api/[controller]/[action]")]
    public class StarsmiimController : ControllerBase
    {

        private readonly BOCommon _objBOCommon;
        private readonly DOConfiguration _objConfiguration;
        private readonly ICacheService _cache;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public StarsmiimController(BOCommon bOCommon, DOConfiguration objConfiguration, ICacheService cache, IHttpContextAccessor httpContextAccessor)
        {
            _objBOCommon = bOCommon;
            _objConfiguration = objConfiguration;
            _cache = cache;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        public ActionResult GetMembers(string memberId, string memFirstName, string memLastName, string birthDate)
        {
            string RequestId = Guid.NewGuid().ToString();

            _objBOCommon.Trace("StartsMiimController/GetMembers", " GetMembers - Request object of -" + RequestId, BOCommon.JsonConvertObjectToString(new { memberId = memberId, memFirstName = memFirstName, memLastName = memLastName, birthDate= birthDate }), _httpContextAccessor.HttpContext.User.Identity.Name);

            BOStarsmiimDetails obj = new BOStarsmiimDetails(_objConfiguration, _cache);
            try
            {
                List<DOStarsmiim> _result = obj.GetMemberDetails(memberId, memFirstName, memLastName, birthDate);
                
                _objBOCommon.Trace("StartsMiimController/GetMembers", " GetMembers - Response object of -" + RequestId, BOCommon.JsonConvertObjectToString(new { response = _result }), _httpContextAccessor.HttpContext.User.Identity.Name);
                return Ok(_result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }

        }
    }

}

