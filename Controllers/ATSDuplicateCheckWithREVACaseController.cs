using ANGDDEAPI.Common;
using ANGDDEAPIBO;
using ANGDDEAPIDO.Interface;
using ANGDDEAPIDO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using Newtonsoft.Json;

namespace ANGDDEAPI.Controllers
{    
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class ATSDuplicateCheckWithREVACaseController : ControllerBase
    {

        public static long LoggedInUserId = 0;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DOConfiguration _objConfiguration;
        private readonly IMemoryCacheHelper _memoryCacheHelper;
        private readonly BOCommon _objBOCommon;
        private readonly ICacheService _cache;


        public ATSDuplicateCheckWithREVACaseController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _memoryCacheHelper = memoryCacheHelper;
            _objBOCommon = bOCommon;
            _cache = cache;
        }

        [HttpGet]
        public IActionResult DuplicateCaseCheck(string details = null)
        {
            DODuplicateCaseResponse response = new DODuplicateCaseResponse();
            try
            {
                if (!string.IsNullOrEmpty(details))
                {
                    var doATSCaseInfo = new DOATSCaseInfo();
                    doATSCaseInfo = JsonConvert.DeserializeObject<DOATSCaseInfo>(details);
                    BOATSDuplicateCheckWithREVACase objBOATSDuplicateCheckWithREVACase = new BOATSDuplicateCheckWithREVACase(_objConfiguration, _cache, _objBOCommon);
                    response = objBOATSDuplicateCheckWithREVACase.CheckForDuplicateCase(doATSCaseInfo);
                    if (response != null)
                    {
                        return Ok(response);
                    }
                    else
                    {
                        return BadRequest();
                    }
                }
                return BadRequest("Details parameter is required.");
            }
            catch (Exception ex)
            {
                _objBOCommon.ErrorLog(LoggedInUserId, this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), ex.Message, ex.ToString());
                return StatusCode((int)HttpStatusCode.InternalServerError, ex);
            }
        }
    }
}
