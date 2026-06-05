using ANGDDEAPI.Common;
using ANGDDEAPIBO;
using ANGDDEAPIDO;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;

namespace ANGDDEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class AccountGroupSearchController : ControllerBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DOConfiguration _objConfiguration;
        private readonly IMemoryCacheHelper _memoryCacheHelper;
        private readonly BOCommon _objBOCommon;
        private string _username = string.Empty;
        private string source = string.Empty;

        public AccountGroupSearchController(
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

        [HttpGet]
        public IActionResult GetAccountGroupSearch(string details = null)
        {
            DOAccountGroupSearchInput searchInput = null;
            List<DOAccountGroupSearchInfo> searchResults = null;
            source = BOCommon.GetRefererURI(Request);

            try
            {
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;

                if (string.IsNullOrEmpty(details))
                {
                    return BadRequest(new { error = "Request details cannot be empty." });
                }

                _objBOCommon.Trace(
                    source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(),
                    System.Reflection.MethodBase.GetCurrentMethod().Name + "-Request",
                    details, _username);

                searchInput = JsonConvert.DeserializeObject<DOAccountGroupSearchInput>(details);

                // Validate at least one search field is provided
                bool isValidRequest = !string.IsNullOrEmpty(searchInput.accountId)
                    || !string.IsNullOrEmpty(searchInput.accountName)
                    || !string.IsNullOrEmpty(searchInput.groupName)
                    || !string.IsNullOrEmpty(searchInput.groupId)
                    || !string.IsNullOrEmpty(searchInput.packageId);

                if (!isValidRequest)
                {
                    return BadRequest(new { error = "At least one search criteria must be provided." });
                }

                BHAccountGroupSearchMethods searchMethods = new BHAccountGroupSearchMethods(_memoryCacheHelper, _objConfiguration);
                searchResults = searchMethods.GetAccountGroupSearchSummary(searchInput);

                _objBOCommon.Trace(
                    source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(),
                    System.Reflection.MethodBase.GetCurrentMethod().Name + "-Response",
                    BOCommon.JsonConvertObjectToString(searchResults), _username);

                return Ok(searchResults);
            }
            catch (Exception ex)
            {
                _objBOCommon.LogError(
                    this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(),
                    source, 10001, ex.Message, " ", ex.StackTrace?.ToString(), _username);

                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult GetAccountGroupSearchInDetails(string details = null)
        {
            DOAccountGroupSearchInput searchInput = null;
            List<DOAccountGroupSearchInfo> searchResults = null;
            source = BOCommon.GetRefererURI(Request);

            try
            {
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;

                if (string.IsNullOrEmpty(details))
                {
                    return BadRequest(new { error = "Request details cannot be empty." });
                }

                _objBOCommon.Trace(
                    source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(),
                    System.Reflection.MethodBase.GetCurrentMethod().Name + "-Request",
                    details, _username);

                searchInput = JsonConvert.DeserializeObject<DOAccountGroupSearchInput>(details);

                // Validate at least one search field is provided
                bool isValidRequest = !string.IsNullOrEmpty(searchInput.accountId)
                    || !string.IsNullOrEmpty(searchInput.accountName)
                    || !string.IsNullOrEmpty(searchInput.groupName)
                    || !string.IsNullOrEmpty(searchInput.groupId)
                    || !string.IsNullOrEmpty(searchInput.packageId);

                if (!isValidRequest)
                {
                    return BadRequest(new { error = "At least one search criteria must be provided." });
                }

                BHAccountGroupSearchMethods searchMethods = new BHAccountGroupSearchMethods(_memoryCacheHelper, _objConfiguration);
                searchResults = searchMethods.GetAccountGroupSearchDetails(searchInput);

                _objBOCommon.Trace(
                    source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(),
                    System.Reflection.MethodBase.GetCurrentMethod().Name + "-Response",
                    BOCommon.JsonConvertObjectToString(searchResults), _username);

                return Ok(searchResults);
            }
            catch (Exception ex)
            {
                _objBOCommon.LogError(
                    this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(),
                    source, 10001, ex.Message, " ", ex.StackTrace?.ToString(), _username);

                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult GetPackageComponentDetails(string details = null)
        {
            DOAccountGroupSearchInput searchInput = null;
            List<DOPackageComponentListInfo> searchResults = null;
            source = BOCommon.GetRefererURI(Request);

            try
            {
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;

                if (string.IsNullOrEmpty(details))
                {
                    return BadRequest(new { error = "Request details cannot be empty." });
                }

                _objBOCommon.Trace(
                    source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(),
                    System.Reflection.MethodBase.GetCurrentMethod().Name + "-Request",
                    details, _username);

                searchInput = JsonConvert.DeserializeObject<DOAccountGroupSearchInput>(details);

                // Validate at least one search field is provided
                bool isValidRequest = !string.IsNullOrEmpty(searchInput.accountId)
                    || !string.IsNullOrEmpty(searchInput.packageId)
                    || !string.IsNullOrEmpty(searchInput.groupId);

                if (!isValidRequest)
                {
                    return BadRequest(new { error = "At least one search criteria (accountId, packageId, or groupId) must be provided." });
                }

                BHAccountGroupSearchMethods searchMethods = new BHAccountGroupSearchMethods(_memoryCacheHelper, _objConfiguration);
                searchResults = searchMethods.GetPackageComponentList(searchInput);

                _objBOCommon.Trace(
                    source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(),
                    System.Reflection.MethodBase.GetCurrentMethod().Name + "-Response",
                    BOCommon.JsonConvertObjectToString(searchResults), _username);

                return Ok(searchResults);
            }
            catch (Exception ex)
            {
                _objBOCommon.LogError(
                    this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(),
                    source, 10001, ex.Message, " ", ex.StackTrace?.ToString(), _username);

                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = ex.Message });
            }
        }

    }
}