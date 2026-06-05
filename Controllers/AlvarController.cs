using ANGDDEAPI.Common;
using ANGDDEAPIBO;
using ANGDDEAPIDO.Interface;
using ANGDDEAPIDO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ANGDDEAPIFoundation;
using System;
using System.Collections.Generic;


namespace ANGDDEAPI.Controllers
{
    //[Route("api/[controller]")]
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class AlvarController : ControllerBase
    {
        ExceptionTypes _resException;
        public static long LoggedInUserId = 0;
        public string _username = string.Empty;
        public string ModuleName = "AlvarController";
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DOConfiguration _objConfiguration;
        private readonly IMemoryCacheHelper _memoryCacheHelper;
        public readonly BOCommon _objBOCommon;
        string source = string.Empty;
        private readonly ICacheService _cache;

        public AlvarController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _memoryCacheHelper = memoryCacheHelper;
            _objBOCommon = bOCommon;
            _cache = cache;
        }
        [HttpGet]
        public ActionResult GetAlvarDetails([FromQuery] DOAlvarSegmentRequest objDOAlvarSegmentRequest)
        {
            DOAlvarResponse objDOAlvarResponse = new DOAlvarResponse();
            DOAlvarDetailsResponse LstDOAlvarDetailsResponse = null;
            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "GetAlvarDetails", _username);

            try
            {
                AlvarMethods objAlvarMethods = new AlvarMethods(_memoryCacheHelper, _objConfiguration);
                _resException = objAlvarMethods.GetAlvarDetails(objDOAlvarSegmentRequest, out LstDOAlvarDetailsResponse);
                if (_resException != ExceptionTypes.Success && _resException != ExceptionTypes.ZeroRecords)
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                if (source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(0, location, ex.Message, ex.ToString());
                }
                else
                {
                    _objBOCommon.LogError(location, source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                }
                return BadRequest(ex.Message);
            }
            return Ok(LstDOAlvarDetailsResponse);
        }
    }
}
