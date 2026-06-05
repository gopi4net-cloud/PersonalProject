using ANGDDEAPI.Common;
using ANGDDEAPIBO;
using ANGDDEAPIDO;
using ANGDDEAPIDO.Interface;
using ANGDDEAPIFoundation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace ANGDDEAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AutoDialCallController : ControllerBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        ExceptionTypes _resException;
        public string _username = string.Empty;
        public string ModuleName = "AutoDialCallController";
        private readonly DOConfiguration _objConfiguration;
        private readonly IMemoryCacheHelper _memoryCacheHelper;
        private readonly BOCommon _objBOCommon;
        private readonly ICacheService _cache;

        public AutoDialCallController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _memoryCacheHelper = memoryCacheHelper;
            _objBOCommon = bOCommon;
            _cache = cache;
        }

        [HttpPost]
        public ActionResult AbletoCallDetails(DOAbletoCallRequest ableToCallRequest = null)
        {
            DOAbletoCallRequest objDOAbletoCallRequest = ableToCallRequest;
            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            string ableToCallFlag = null;
            DOMemberDetailsForAutoDialerResponse objDOMemberDetailsForAutoDialerResponse = new DOMemberDetailsForAutoDialerResponse();

            try
            {
                if (objDOAbletoCallRequest != null)
                {
                    AutoDialCallMethods objAutoDialCallMethods = new AutoDialCallMethods(_memoryCacheHelper, _objConfiguration);
                    _resException = objAutoDialCallMethods.GetAbleToCallFlagValue(objDOAbletoCallRequest, out objDOMemberDetailsForAutoDialerResponse);
                }
                if (_resException != ExceptionTypes.Success && _resException != ExceptionTypes.ZeroRecords)
                {
                    return BadRequest();
                }
                if (objDOMemberDetailsForAutoDialerResponse?.data != null)
                {
                    ableToCallFlag = objDOMemberDetailsForAutoDialerResponse.data.FirstOrDefault()?.able_to_call;
                }
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(source) && source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(0, location, ex.Message, ex.ToString());
                }
                else
                {
                    _objBOCommon.LogError(location, source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                }
                return BadRequest(ex.Message);
            }
            return Ok(objDOMemberDetailsForAutoDialerResponse);
        }
    }
}
