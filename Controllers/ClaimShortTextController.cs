using Amazon.Runtime.Internal.Util;
using ANGDDEAPI.Common;
using ANGDDEAPIBO;
using ANGDDEAPIDO.Interface;
using ANGDDEAPIDO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using ANGDDEAPIFoundation;
using System.Net;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace ANGDDEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
  
    public class ClaimShortTextController : ControllerBase
    {
        ExceptionTypes _result;
        public string _username = string.Empty;
        string source = string.Empty;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DOConfiguration _objConfiguration;
        private readonly IMemoryCacheHelper _memoryCacheHelper;
        private readonly BOCommon _objBOCommon;
        private readonly ICacheService _cache;
        public ClaimShortTextController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _memoryCacheHelper = memoryCacheHelper;
            _objBOCommon = bOCommon;
            _cache = cache;
        }
        [HttpGet]
        public ActionResult GetFacetsExplanationCode(string CliamCode)
        {
            string CliamDescription = string.Empty;
            try
            {

                source = BOCommon.GetRefererURI(Request);
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                //_objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Request", CliamCode, _username);

                if (!string.IsNullOrEmpty(CliamCode))
                {
                    CliamDescription = "test";
                    FacetsExplanationCodeMethods objFacetsExplanationCodeMethods = new FacetsExplanationCodeMethods(_memoryCacheHelper, _objConfiguration);
                    _result = objFacetsExplanationCodeMethods.FacetsExplanationCodeService(CliamCode);

                    if (_result != ExceptionTypes.Success)
                    {
                        return StatusCode((int)HttpStatusCode.InternalServerError, "An Error occured");
                    }
                }
            }
            catch (Exception ex)
            {
                _objBOCommon.LogError(this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
            }
            return Ok(CliamDescription);
        }

        [HttpGet]
        public IActionResult GetRemarkCodeDetails(string ClaimId,string RemarkCode)
        {
            string Desc = null;

            try
            {
                source = BOCommon.GetRefererURI(Request);
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;

                if (!string.IsNullOrEmpty(ClaimId))
                {
                    FacetsExplanationCodeMethods objFacetsExplanationCodeMethods = new FacetsExplanationCodeMethods(_memoryCacheHelper, _objConfiguration);
                    Desc = objFacetsExplanationCodeMethods.GetRemarkCode(ClaimId, RemarkCode);

                }
            }
            catch(Exception ex)
            {
                _objBOCommon.LogError(this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                //return StatusCode((int)HttpStatusCode.InternalServerError, ex);
            }           
            return Ok(Desc);
        }
    }
}
