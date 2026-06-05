using ANGDDEAPI.Common;
using ANGDDEAPIBO;
using ANGDDEAPIDO.Interface;
using ANGDDEAPIDO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System;
using ANGDDEAPI.Common;
using ANGDDEAPIBO;
using ANGDDEAPIDO;
using ANGDDEAPIDO.Interface;
using ANGDDEAPIFoundation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;


namespace ANGDDEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class ClaimCirrusEmployerGroupsController : ControllerBase
    {
        public static long LoggedInUserId = 0;
        EmployerGroupsMethods _objBOEmployerGroupsSearch;
        DOClaimCirrusEmployerGroupsResponseToATS _result;
        public string _username = string.Empty;
        string source = string.Empty;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DOConfiguration _objConfiguration;
        private readonly IMemoryCacheHelper _memoryCacheHelper;
        public readonly BOCommon _objBOCommon;

        private readonly ICacheService _cache;

        public ClaimCirrusEmployerGroupsController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _memoryCacheHelper = memoryCacheHelper;
            APIAuthorization.Instance.SetDOConfiguration(_objConfiguration);
            _memoryCacheHelper = memoryCacheHelper;
            _objBOCommon = bOCommon;
            //_cache = cache;
        }
        /// <summary>
        /// Get Employer Groups information based on search criteria groupNumber
        /// </summary>
        /// <param name="groupNumber,setNumber,eligibilityStartDate"></param>
        /// <returns></returns>

        [HttpGet]
        public IActionResult GetEmployerGroupsLookup(string groupNumber,string setNumber,string eligibilityStartDate)
        {
            source = BOCommon.GetRefererURI(Request);
            try
            {
                DOClaimCirrusEmployerGroupsResponseToATS lstObjDOClaimCirrusEmployerGroupsResponse = null;
                _objBOEmployerGroupsSearch = new EmployerGroupsMethods(_memoryCacheHelper, _objConfiguration, _httpContextAccessor, _objBOCommon);
                _username = _httpContextAccessor?.HttpContext?.User?.Identity?.Name;
                if (!string.IsNullOrEmpty(groupNumber?.ToString()))
                {
                    EmployerGroupsMethods _objCSPFacetsMethods = new EmployerGroupsMethods(_memoryCacheHelper, _objConfiguration, _httpContextAccessor, _objBOCommon);
                    _result = _objBOEmployerGroupsSearch.GetEmployerGroupsLookup(groupNumber, setNumber, eligibilityStartDate, out lstObjDOClaimCirrusEmployerGroupsResponse);

                    if (_result == null)
                    {
                        return BadRequest();
                    }
                }
                return Ok(lstObjDOClaimCirrusEmployerGroupsResponse);
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(source) &&  source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(LoggedInUserId, this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), ex.Message, ex.ToString());
                }
                else
                {
                    _objBOCommon.LogError(this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                }
                // _logger.Error(this.GetType().Name + "", (long)ErrorModuleName.WebService, (long)500, ex.ToString, " ", ex.StackTrace.ToString(), new DO.DOLoginUserDetails { ADM_UserInfoId = 10001 });
                return BadRequest();
            }
        }

    }
}

