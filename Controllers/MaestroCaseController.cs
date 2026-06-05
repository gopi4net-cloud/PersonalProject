using ANGDDEAPI.Common;
using ANGDDEAPIBO;
using ANGDDEAPIDO;
using ANGDDEAPIDO.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;

namespace ANGDDEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class MaestroCaseController : ControllerBase
    {
        public static long LoggedInUserId = 0;
        public string _username = string.Empty;
        string source = string.Empty;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DOConfiguration _objConfiguration;
        private readonly IMemoryCacheHelper _memoryCacheHelper;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly BOCommon _objBOCommon;

        public MaestroCaseController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, IHttpClientFactory httpClientFactory, BOCommon bOCommon)
        {
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _memoryCacheHelper = memoryCacheHelper;
            _httpClientFactory = httpClientFactory;
            _objBOCommon = bOCommon;
        }

        /// <summary>
        /// Gets combined case details by performing an S-type lookup first, then an I-type lookup
        /// using the CurrentInteractionID from the S-case response.
        /// </summary>
        /// <param name="caseId">The full S-type case ID (e.g., "UHG-MEDRET-IIM-WORK S-780344887")</param>
        /// <returns>Combined mapped case details from both S-case and I-case</returns>
        [HttpGet]
        public ActionResult GetCaseDetails(string caseId)
        {
            source = BOCommon.GetRefererURI(Request);
            try
            {
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;

                if (string.IsNullOrEmpty(caseId))
                {
                    return BadRequest("caseId is required");
                }

                MaestroCaseMethods maestroCaseMethods = new(_httpClientFactory, _memoryCacheHelper, _objConfiguration);
                DOMaestroCaseDetailsResponse result = maestroCaseMethods.GetCombinedCaseDetails(caseId);

                if (result != null)
                {
                    return Ok(result);
                }

                return NotFound();
            }
            catch (Exception ex)
            {
                _objBOCommon.LogError(this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), source, 10001, ex.Message, " ", ex.StackTrace?.ToString(), _username);
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Updates/closes an S-Case by performing a PUT with case update content
        /// </summary>
        /// <param name="caseId">The full case ID (e.g., "UHG-MEDRET-IIM-WORK S-791224276")</param>
        /// <param name="actionID">The action ID (e.g., "pyUpdateCaseDetails")</param>
        /// <param name="ifMatch">The ETag value for optimistic concurrency</param>
        /// <param name="request">The case update request body</param>
        /// <returns>Update response</returns>
        [HttpPut]
        public ActionResult UpdateCase(string caseId, string actionID, [FromHeader(Name = "if-match")] string ifMatch, [FromBody] DOMaestroCaseUpdateRequest request)
        {
            source = BOCommon.GetRefererURI(Request);
            try
            {
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;

                if (string.IsNullOrEmpty(caseId))
                {
                    return BadRequest("caseId is required");
                }

                if (string.IsNullOrEmpty(actionID))
                {
                    return BadRequest("actionID is required");
                }

                if (request == null)
                {
                    return BadRequest("Request body is required");
                }

                MaestroCaseMethods maestroCaseMethods = new(_httpClientFactory, _memoryCacheHelper, _objConfiguration);
                string result = maestroCaseMethods.UpdateCase(caseId, actionID, ifMatch, request);

                if (!string.IsNullOrEmpty(result))
                {
                    var updateResponse = JsonConvert.DeserializeObject<DOMaestroCaseUpdateResponse>(result);
                    return Ok(updateResponse);
                }

                return StatusCode((int)HttpStatusCode.InternalServerError, "No response from Maestro API");
            }
            catch (Exception ex)
            {
                _objBOCommon.LogError(this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), source, 10001, ex.Message, " ", ex.StackTrace?.ToString(), _username);
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
