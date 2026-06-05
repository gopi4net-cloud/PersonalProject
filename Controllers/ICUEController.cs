using ANGDDEAPI.Common;
using ANGDDEAPIBO;
using ANGDDEAPIDO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ANGDDEAPI.Controllers
{

    [ApiController]
    public class ICUEController : ControllerBase
    {
        #region Variables and Constructor
        public string _username = string.Empty;
        public string ModuleName = "ICUEController";
        ICUECommon objICUECommon = null;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DOConfiguration _objConfiguration;
        private readonly IMemoryCacheHelper _memoryCacheHelper;
        private readonly BOCommon _objBOCommon;
        public ICUEController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon)
        {
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _memoryCacheHelper = memoryCacheHelper;
            objICUECommon = new ICUECommon(_objConfiguration, _httpContextAccessor, _memoryCacheHelper);
            _objBOCommon = bOCommon;
        }
        #endregion
        /// <summary>
        /// Invoke Ping federate 
        /// </summary>
        /// <param name="searchId"></param>
        /// <returns></returns>
        [Route("api/InvokePingFederate")]
        [HttpGet]
        public async Task<string> InvokePingFederate(string searchId)
        {
            string responseResult = string.Empty;
            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            var response = new HttpResponseMessage();
            try
            {
                //get requested user name
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                string[] strLogiName = _username.Split(new string[] { @"\" }, StringSplitOptions.RemoveEmptyEntries);
                string loginName = strLogiName[1];
                _objBOCommon.Trace(location, location + source + "InvokePingFederate - Get - Request object", objICUECommon.JsonConvertObjectToString(new { searchId = searchId }), _username);
                responseResult = await objICUECommon.InvokePingFederate(searchId, loginName);
                //return HTML content
                response.Content = new StringContent(responseResult, Encoding.UTF8, "text/html");
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
                response.StatusCode = HttpStatusCode.OK;
                _objBOCommon.Trace(location, location + source + "InvokePingFederate - Get - Response object", objICUECommon.JsonConvertObjectToString(new { response = responseResult }), _username);
                return responseResult;
            }
            catch (Exception ex)
            {
                _objBOCommon.LogError(location, source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                response.Content = new StringContent("An error occurred while consuming the API !", Encoding.UTF8, "text/plain");
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
                response.StatusCode = HttpStatusCode.BadRequest;
                return response.ReasonPhrase;
            }
        }
        /// <summary>
        /// Get: Get HSC Details from ICUE
        /// </summary>
        /// <param name="searchId"></param>
        /// <param name="applicationName"></param>
        /// <returns></returns>
        [Route("api/GetHscDetails")]
        [HttpGet]
        public async Task<IActionResult> GetHscDetails(string searchId, string applicationName)
        {
            string responseResult = string.Empty;
            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _username = _httpContextAccessor.HttpContext.User.Identity.Name;
            try
            {
                //BOCommon.Trace(location, location + source + "GetHscDetails - Get - Request object", objICUECommon.JsonConvertObjectToString(new { searchId = searchId, applicationName = applicationName }), _username);
                var result = await objICUECommon.GetHscDetails(searchId, applicationName);
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
        /// Post: Update Notes in ICUE
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("api/UpdateTaskNotes")]
        [HttpPost]
        public async Task<IActionResult> UpdateTaskNotes(DOUpdateNoteRequestAPI request)
        {
            DOUpdateNoteRequest objRequest = null;
            string responseResult = string.Empty;
            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _username = _httpContextAccessor.HttpContext.User.Identity.Name;
            try
            {
                if (request != null)
                {
                    objRequest = JsonConvert.DeserializeObject<DOUpdateNoteRequest>(request.request);
                    _objBOCommon.Trace(location, location + source + "UpdateTaskNotes - Post - Request object", objICUECommon.JsonConvertObjectToString(new { request = request }), _username);
                    if (objRequest != null && objRequest.AuthNtfUpdateRequest != null && objRequest.AuthNtfUpdateRequest.RequestHeader != null)
                    {
                        var result = await objICUECommon.UpdateNote(objRequest);
                        _objBOCommon.Trace(location, location + source + "UpdateTaskNotes - Post - Response object", objICUECommon.JsonConvertObjectToString(new { response = result }), _username);
                        return Ok(result);
                    }
                }
                return BadRequest("Request cannot be empty");
            }
            catch (Exception ex)
            {
                _objBOCommon.LogError(location, source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                return BadRequest(ex.Message);
            }
        }
    }
}
