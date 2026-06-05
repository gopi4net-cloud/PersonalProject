using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ANGDDEAPI.Common;
using ANGDDEAPIBO;
using ANGDDEAPIDO;
using ANGDDEAPIFoundation;
using ANGDDEAPIDO.Interface;

namespace ANGDDEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class BHHCPClaimSearchController : ControllerBase
    {

        public static long LoggedInUserId = 0;
        BOClaimsSearch _objBOClaimsSearch;
        ExceptionTypes _result;
        public string _username = string.Empty;
        string source = string.Empty;


        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DOConfiguration _objConfiguration;
        private readonly IMemoryCacheHelper _memoryCacheHelper;
        private readonly BOCommon _objBOCommon;
        private readonly ICacheService _cache;

        public BHHCPClaimSearchController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _memoryCacheHelper = memoryCacheHelper;
            _objBOCommon = bOCommon;
            _cache = cache;
        }
        [HttpGet]
        public ActionResult GetAllClaim(string strClaimSearch)
        {
            //source = BOCommon.GetRefererURI(Request);
            try
            {
               
                List<DOClaimInfo> lstObjDOClaimInfo = null;
                DOClaimInfo objDOClaimInfo = null;
                //_objBOClaimsSearch = new BOClaimsSearch();
                //BOHCPClaimSearch _objBOHCPClaimSearch = new BOHCPClaimSearch(_objConfiguration);
                BHHCPClaimMethods _objBOHCPClaimSearch = new(_memoryCacheHelper, _objConfiguration);
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                if (!string.IsNullOrEmpty(strClaimSearch))
                {
                    objDOClaimInfo = new DOClaimInfo();
                    objDOClaimInfo = JsonConvert.DeserializeObject<DOClaimInfo>(strClaimSearch);

                    if (objDOClaimInfo.ClaimSystemLkup == (int)SourceSystemType.FACETS)
                    {
                        BHCSPFacetsMethods _objCSPFacetsMethods = new BHCSPFacetsMethods(_memoryCacheHelper, _objConfiguration, _httpContextAccessor, _objBOCommon);
                        DOClaimSummaryRequest objDOClaimSummaryRequest = null;
                        //objDOClaimInfo = new DOClaimInfo();
                        //objDOClaimInfo.ClaimNumber = "19E119458800";

                        objDOClaimSummaryRequest = new DOClaimSummaryRequest();
                        objDOClaimSummaryRequest.claimId = string.IsNullOrEmpty(objDOClaimInfo.ClaimNumber) ? null : objDOClaimInfo.ClaimNumber;
                        objDOClaimSummaryRequest.dateOfServiceFrom = objDOClaimInfo.StartDate == null ? null : objDOClaimInfo.StartDate;
                        objDOClaimSummaryRequest.dateOfServiceTo = objDOClaimInfo.EndDate == null ? null : objDOClaimInfo.EndDate;
                        objDOClaimSummaryRequest.limit = 150;
                        objDOClaimSummaryRequest.subscriberId = string.IsNullOrEmpty(objDOClaimInfo.MemberID) ? null : objDOClaimInfo.MemberID;
                        objDOClaimSummaryRequest.offset = 1;
                        objDOClaimSummaryRequest.providerTin = string.IsNullOrEmpty(objDOClaimInfo.TaxID) ? null : objDOClaimInfo.TaxID;
                        objDOClaimSummaryRequest.sortOrder = "D";

                        _result = _objCSPFacetsMethods.ClaimSummary(objDOClaimSummaryRequest, out lstObjDOClaimInfo);
                        lstObjDOClaimInfo = lstObjDOClaimInfo != null ? lstObjDOClaimInfo.Where(l => l.MemberID == objDOClaimInfo.MemberID).ToList() : null;

                        if (_result != ExceptionTypes.Success && _result != ExceptionTypes.ZeroRecords)
                        {
                            return BadRequest();
                        }
                    }
                    else if (objDOClaimInfo.ClaimSystemLkup == (int)SourceSystemType.COSMOS || objDOClaimInfo.ClaimSystemLkup == (int)SourceSystemType.NICE)
                    {
                        _result = _objBOHCPClaimSearch.GetAllClaims(objDOClaimInfo, out lstObjDOClaimInfo);
                    }
                    if (_result != ExceptionTypes.Success && _result != ExceptionTypes.ZeroRecords)
                    {
                        return BadRequest();
                    }
                }
                return Ok(lstObjDOClaimInfo);
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
                throw ex;
            }
        }

        [HttpGet]
        public ActionResult GetClaimDetail(string strClaimSearch)
        {

            try
            {
                List<DOClaimInfo> lstObjDOClaimInfo = null;
                DOClaimInfo objDOClaimInfo = null;
                _objBOClaimsSearch = new BOClaimsSearch(_objConfiguration, _cache);
                //BOHCPClaimSearch _objBOHCPClaimSearch = new BOHCPClaimSearch(_objConfiguration);
                BHHCPClaimMethods _objBOHCPClaimSearch = new(_memoryCacheHelper, _objConfiguration);
                long? requestID = null;
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                //strClaimSearch = @"{'ClaimNumber': '22Q220694700','ClaimSystemLkup': '18'}";
                if (!string.IsNullOrEmpty(strClaimSearch))
                {
                    objDOClaimInfo = new DOClaimInfo();
                    objDOClaimInfo = JsonConvert.DeserializeObject<DOClaimInfo>(strClaimSearch);
                    if (objDOClaimInfo.ClaimSystemLkup == (int)SourceSystemType.FACETS)
                    {
                        BHCSPFacetsMethods _objCSPFacetsMethods = new BHCSPFacetsMethods(_memoryCacheHelper, _objConfiguration, _httpContextAccessor, _objBOCommon);
                        //objDOClaimInfo = new DOClaimInfo();
                        //objDOClaimInfo.ClaimNumber = "19E119458800";

                        _result = _objCSPFacetsMethods.ClaimDetails(objDOClaimInfo, out lstObjDOClaimInfo);

                        if (_result != ExceptionTypes.Success && _result != ExceptionTypes.ZeroRecords)
                        {
                            return BadRequest();
                        }
                    }
                    else if (objDOClaimInfo.ClaimSystemLkup == (int)SourceSystemType.COSMOS || objDOClaimInfo.ClaimSystemLkup == (int)SourceSystemType.NICE)
                    {
                        _result = _objBOHCPClaimSearch.GetClaimDetail(objDOClaimInfo, out lstObjDOClaimInfo, out requestID);
                    }
                    if (_result != ExceptionTypes.Success && _result != ExceptionTypes.ZeroRecords)
                    {
                        return BadRequest();
                    }
                }
                if (requestID != null)
                    return Ok(requestID);
                else
                    return Ok(lstObjDOClaimInfo);
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
                throw ex;
            }
        }

        [HttpGet]
        public ActionResult GetAllClaimForMES(string strClaimSearch)
        {
            //source = BOCommon.GetRefererURI(Request);
            try
            {
                List<DOClaimInfo> lstObjDOClaimInfo = null;
                DOClaimInfo objDOClaimInfo = null;
                //BOHCPClaimSearch _objBOHCPClaimSearch = new BOHCPClaimSearch(_objConfiguration);
                BHHCPClaimMethods _objBOHCPClaimSearch = new(_memoryCacheHelper, _objConfiguration);
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                if (!string.IsNullOrEmpty(strClaimSearch))
                {
                    objDOClaimInfo = new DOClaimInfo();
                    objDOClaimInfo = JsonConvert.DeserializeObject<DOClaimInfo>(strClaimSearch);

                    _result = _objBOHCPClaimSearch.GetAllClaimsForMES(objDOClaimInfo, out lstObjDOClaimInfo);

                    if (_result != ExceptionTypes.Success && _result != ExceptionTypes.ZeroRecords)
                    {
                        return BadRequest();
                    }
                }
                return Ok(lstObjDOClaimInfo);
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
                throw ex;
            }
        }

        [HttpGet]
        public ActionResult GetClaimDetailForMES(string strClaimSearch)
        {

            try
            {
                List<DOClaimInfo> lstObjDOClaimInfo = null;
                DOClaimInfo objDOClaimInfo = null;
                _objBOClaimsSearch = new BOClaimsSearch(_objConfiguration, _cache);
                //BOHCPClaimSearch _objBOHCPClaimSearch = new BOHCPClaimSearch(_objConfiguration);
                BHHCPClaimMethods _objBOHCPClaimSearch = new(_memoryCacheHelper, _objConfiguration);
                long? requestID = null;
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                if (!string.IsNullOrEmpty(strClaimSearch))
                {
                    objDOClaimInfo = new DOClaimInfo();
                    objDOClaimInfo = JsonConvert.DeserializeObject<DOClaimInfo>(strClaimSearch);
                    _result = _objBOHCPClaimSearch.GetClaimDetailForMES(objDOClaimInfo, out lstObjDOClaimInfo, out requestID);

                    if (_result != ExceptionTypes.Success && _result != ExceptionTypes.ZeroRecords)
                    {
                        return BadRequest();
                    }
                }
                if (requestID != null)
                    return Ok(requestID);
                else
                    return Ok(lstObjDOClaimInfo);
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
                throw ex;
            }
        }

    }
}