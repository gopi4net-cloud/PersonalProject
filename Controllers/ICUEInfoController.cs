using ANGDDEAPI.Common;
using ANGDDEAPIBO;
using ANGDDEAPIDO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ANGDDEAPI.Controllers
{
    [ApiController]
    public class ICUEInfoController : ControllerBase
    {
        #region Variables and Constructor

        public string _username = string.Empty;
        public string ModuleName = "ICUEController";
        private ICUECommon objICUECommon = null;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DOConfiguration _objConfiguration;
        private readonly IMemoryCacheHelper _memoryCacheHelper;
        private readonly BOCommon _objBOCommon;

        public ICUEInfoController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon)
        {
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _memoryCacheHelper = memoryCacheHelper;
            objICUECommon = new ICUECommon(_objConfiguration, _httpContextAccessor, _memoryCacheHelper);
            _objBOCommon = bOCommon;
        }

        #endregion Variables and Constructor

        /// <summary>
        /// Get: Get HSC Details from ICUE
        /// </summary>
        /// <param name="searchId"></param>
        /// <param name="applicationName"></param>
        /// <returns></returns>
        [Route("api/GetHscInfo")]
        [HttpGet]
        public async Task<IActionResult> GetHscInfo(string searchId, string applicationName)
        {
            string responseResult = string.Empty;
            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _username = _httpContextAccessor.HttpContext.User.Identity.Name;
            try
            {
                //BOCommon.Trace(location, location + source + "GetHscDetails - Get - Request object", objICUECommon.JsonConvertObjectToString(new { searchId = searchId, applicationName = applicationName }), _username);
                var result = await objICUECommon.GetHscInfo(searchId, applicationName);
                //BOCommon.Trace(location, location + source + "GetHscDetails - Get - Response object", objICUECommon.JsonConvertObjectToString(new { response = result }), _username);
                return new JsonResult(result);
            }
            catch (Exception ex)
            {
                _objBOCommon.LogError(location, source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get: Get HSC Details by member from ICUE
        /// </summary>
        /// <returns></returns>
        [Route("api/GetHscSummaryByMember")]
        [HttpGet]
        public async Task<IActionResult> GetHscSummaryByMember(string srcMemberID, string firstName, string lastName, DateTime dob)
        {
            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _username = _httpContextAccessor.HttpContext.User.Identity.Name;
            try
            {
                DOICUEHscSummaryResponse result = await objICUECommon.GetHscSummaryByMember(srcMemberID, firstName, lastName, dob);
                return new JsonResult(result);
            }
            catch (Exception ex)
            {
                _objBOCommon.LogError(location, source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                return BadRequest(ex.Message);
            }
        }
    }
}