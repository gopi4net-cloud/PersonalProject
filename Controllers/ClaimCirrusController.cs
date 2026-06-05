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
    public class ClaimCirrusController: ControllerBase
    {
        public static long LoggedInUserId = 0;
        ClaimCirrusMethods _objBOClaimsSearch;
        List<DOClaimInfo> _result;
        public string _username = string.Empty;
        string source = string.Empty;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DOConfiguration _objConfiguration;
        private readonly IMemoryCacheHelper _memoryCacheHelper;
        public readonly BOCommon _objBOCommon;

        private readonly ICacheService _cache;

        public ClaimCirrusController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
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
        /// Get Claims information based on search criteria
        /// </summary>
        /// <param name="strClaimSearch"></param>
        /// <returns></returns>
        
        [HttpGet]
        public IActionResult GetClaimLookup(string strClaimSearch)
        {
            source = BOCommon.GetRefererURI(Request);
            try
            {
                List<DOClaimInfo> lstObjDOClaimCirrusResponse = null;
                 DOClaimInfo objDOClaimInfo = null;
                _objBOClaimsSearch = new ClaimCirrusMethods(_memoryCacheHelper, _objConfiguration, _httpContextAccessor, _objBOCommon);
                _username = _httpContextAccessor?.HttpContext?.User?.Identity?.Name;
                if (!string.IsNullOrEmpty(strClaimSearch))
                {
                    objDOClaimInfo = new DOClaimInfo();
                    objDOClaimInfo = JsonConvert.DeserializeObject<DOClaimInfo>(strClaimSearch);
                    ClaimCirrusMethods _objCSPFacetsMethods = new ClaimCirrusMethods(_memoryCacheHelper, _objConfiguration, _httpContextAccessor, _objBOCommon);
                    string CirrusClaim = string.Empty; 
                    string MemberID = string.Empty; 
                    CirrusClaim = objDOClaimInfo?.ClaimNumber;
                    MemberID = objDOClaimInfo?.MemberID;
                    bool isRemarkCode = false;
                    isRemarkCode = objDOClaimInfo?.IsRemarkCode ?? false;
                    _result = _objBOClaimsSearch?.GetClaimLookup(CirrusClaim,MemberID, isRemarkCode, out lstObjDOClaimCirrusResponse);

                    if (_result == null)
                    {
                        return BadRequest();
                    }
                }
                return Ok(lstObjDOClaimCirrusResponse);
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(source) && source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(LoggedInUserId, this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), ex?.Message, ex?.ToString());
                }
                else
                {
                    _objBOCommon.LogError(this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), source, 10001, ex?.Message, " ", ex?.StackTrace?.ToString(), _username);
                }
                // _logger.Error(this.GetType().Name + "", (long)ErrorModuleName.WebService, (long)500, ex.ToString, " ", ex.StackTrace.ToString(), new DO.DOLoginUserDetails { ADM_UserInfoId = 10001 });
                return BadRequest();
            }
        }
        [HttpGet]
        public IActionResult GetClaimSummary(string strClaimSearch)
        {
            source = BOCommon.GetRefererURI(Request);
            try
            {
                List<DOClaimInfo> lstObjDOClaimCirrusResponse = null;
                DOCirrusClaimsRequest objDOClaimsrequest = new DOCirrusClaimsRequest();
                DOClaimInfo objDOClaimInfo = null;
                _objBOClaimsSearch = new ClaimCirrusMethods(_memoryCacheHelper, _objConfiguration, _httpContextAccessor, _objBOCommon);
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                if (!string.IsNullOrEmpty(strClaimSearch.ToString()))
                {

                    objDOClaimInfo = JsonConvert.DeserializeObject<DOClaimInfo>(strClaimSearch);

                   
                    DateTime? dtStartDate = objDOClaimInfo.StartDate;
                    DateTime? dtEndDate = objDOClaimInfo.EndDate;
                    string strStartDate = dtStartDate.HasValue ? dtStartDate.Value.ToString("yyyy-MM-dd") : string.Empty;
                    string strEndDate = dtEndDate.HasValue ? dtEndDate.Value.ToString("yyyy-MM-dd") : string.Empty;
                    objDOClaimsrequest.claimSearchStartDate = strStartDate;
                    objDOClaimsrequest.claimSearchEndDate = strEndDate;
                    objDOClaimsrequest.MemberId = objDOClaimInfo.MemberID;
                    objDOClaimsrequest.LegacyMemberId = objDOClaimInfo.LegacyMemberId;
                    //objDOClaimsrequest.RelationShipCode = objDOClaimInfo.RelationshipCode;
                   lstObjDOClaimCirrusResponse = _objBOClaimsSearch.GetCirrusClaimsSummary(objDOClaimsrequest);

                    if (lstObjDOClaimCirrusResponse == null)
                    {
                        return BadRequest();
                    }
                }
                return Ok(lstObjDOClaimCirrusResponse);
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
                // _logger.Error(this.GetType().Name + "", (long)ErrorModuleName.WebService, (long)500, ex.ToString, " ", ex.StackTrace.ToString(), new DO.DOLoginUserDetails { ADM_UserInfoId = 10001 });
                return BadRequest();
            }
        }

    }
}
