using ANGDDEAPI.Common;
using ANGDDEAPIBO;
using ANGDDEAPIDO;
using ANGDDEAPIDO.Interface;
using ANGDDEAPIDO.MGRL;
using ANGDDEAPIFoundation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ANGDDEAPI.Controllers
{

    [ApiController]
    [Route("api/[controller]/[action]")]
    public class ATSTokenAPIController : Controller
    {
        public static long LoggedInUserId = 0;
        ATSStargateTokenMethods _Objtoken;
        string _result;
        public string _username = string.Empty;
        string source = string.Empty;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DOConfiguration _objConfiguration;
        private readonly IMemoryCacheHelper _memoryCacheHelper;
        public readonly BOCommon _objBOCommon;

        private readonly ICacheService _cache;

        public ATSTokenAPIController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _memoryCacheHelper = memoryCacheHelper;
            _objBOCommon = bOCommon;
            _cache = cache;
        }

        [HttpGet]
        public IActionResult GetStargetToken()
        {
            source = BOCommon.GetRefererURI(Request);
            try
            {
               
                _Objtoken = new ATSStargateTokenMethods(_memoryCacheHelper, _objConfiguration, _httpContextAccessor, _objBOCommon);
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;

                _result = _Objtoken.StargateTokenDetails();

                return Ok(_result);
            }
            catch (Exception ex)
            {
                if (source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(LoggedInUserId, this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), ex.Message, ex.ToString());
                }
                else
                {
                    _objBOCommon.LogError(this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                }

                return BadRequest();
            }

        }



    }
}
