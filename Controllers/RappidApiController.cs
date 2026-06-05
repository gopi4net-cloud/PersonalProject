
using ANGDDEAPI.Common;
using ANGDDEAPIBO;
using ANGDDEAPIDO;
using ANGDDEAPIDO.RappidApi;
using ANGDDEAPIFoundation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Net;

namespace ANGDDEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class RappidApiController : ControllerBase
    {
        public static long LoggedInUserId = 0;
        public string _username = string.Empty;
        string source = string.Empty;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DOConfiguration _objConfiguration;
        private readonly IMemoryCacheHelper _memoryCacheHelper;
        public readonly BOCommon _objBOCommon;

        public RappidApiController(
            IHttpContextAccessor httpContextAccessor,
            DOConfiguration objConfiguration,
            IMemoryCacheHelper memoryCacheHelper,
            BOCommon bOCommon)
        {
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _memoryCacheHelper = memoryCacheHelper;
            _objBOCommon = bOCommon;
        }

        /// <summary>
        /// Accepts a Rappid API request as a JSON string, deserializes it,
        /// retrieves the Stargate bearer token, and forwards it to the Rappid API via HTTP POST.
        /// </summary>
        /// <param name="strRappidRequest">JSON string matching the DORappidApiRequest schema</param>
        [HttpGet]
        public IActionResult SubmitRappidRequest(string strRappidRequest)
        {
            source = BOCommon.GetRefererURI(Request);

            try
            {
                _username = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? string.Empty;

                if (string.IsNullOrEmpty(strRappidRequest))
                    return BadRequest("Request body cannot be null or empty.");

                // Deserialize the JSON string — same pattern as GetProviderSearch
                var request = JsonConvert.DeserializeObject<DORappidApiRequest>(strRappidRequest);

                if (request == null)
                    return BadRequest("Invalid input: Unable to parse the request body.");

                // --- Required field validations ---

                if (string.IsNullOrWhiteSpace(request.fln))
                    return BadRequest("Field 'fln' is required.");

                // refID is int (default 0 is invalid — must be explicitly provided as non-zero)
                // refID is int (default 0 is invalid — must be explicitly provided as non-zero)
                if (request.refID == 0)
                    return BadRequest("Field 'refID' is required and must be a non-zero value.");

                if (string.IsNullOrWhiteSpace(request.source))
                    return BadRequest("Field 'source' is required. Valid values: ATS, Ingress, Databank, PIQ.");

                if (string.IsNullOrWhiteSpace(request.taskId))
                    return BadRequest("Field 'taskId' is required.");

                if (string.IsNullOrWhiteSpace(request.flnClass))
                    return BadRequest("Field 'flnClass' is required. Valid values: u_opt_bh_fwa, u_prov_attch, u_clm_corsp_lwso_doc.");

                if (string.IsNullOrWhiteSpace(request.divEngine))
                    return BadRequest("Field 'divEngine' is required.");

                if (string.IsNullOrWhiteSpace(request.claimPlatform))
                    return BadRequest("Field 'claimPlatform' is required. Valid values: CSP, OHBS_Facets, UNET, COSMOS, USP.");

                if (string.IsNullOrWhiteSpace(request.orgReceivedDate))
                    return BadRequest("Field 'orgReceivedDate' is required and must be a valid date.");

                // --- End required field validations ---

                // Initialize service with Stargate token support
                var rappidApiMethods = new RappidApiMethods(
                    _memoryCacheHelper,
                    _objConfiguration,
                    _httpContextAccessor,
                    _objBOCommon
                );

                // Retrieve token and POST JSON to Rappid API
                string responseBody = rappidApiMethods.SubmitToRappidApi(request);

                // Deserialize and return the Rappid API response
                //var deserializedResponse = JsonConvert.DeserializeObject(responseBody);

                return Ok(responseBody);
            }
            catch (UnauthorizedAccessException authEx)
            {
                _objBOCommon.LogError(
                    this.GetType().Name + "." + nameof(SubmitRappidRequest),
                    source, 10001,
                    authEx.Message, " ",
                    authEx.StackTrace?.ToString(),
                    _username);

                return StatusCode((int)HttpStatusCode.Unauthorized, authEx.Message);
            }
            catch (JsonException jsonEx)
            {
                _objBOCommon.LogError(
                    this.GetType().Name + "." + nameof(SubmitRappidRequest),
                    source, 10001,
                    jsonEx.Message, " ",
                    jsonEx.StackTrace?.ToString(),
                    _username);

                return BadRequest("Invalid JSON format in the request body.");
            }
            catch (Exception ex)
            {
                _objBOCommon.LogError(
                    this.GetType().Name + "." + nameof(SubmitRappidRequest),
                    source, 10001,
                    ex.Message, " ",
                    ex.StackTrace?.ToString(),
                    _username);

                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}