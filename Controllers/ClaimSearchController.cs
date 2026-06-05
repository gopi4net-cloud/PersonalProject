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
    public class ClaimSearchController : ControllerBase
    {
        public static long LoggedInUserId = 0;
        BOClaimsSearch _objBOClaimsSearch;
        ExceptionTypes _result;
        public string _username = string.Empty;
        string source = string.Empty;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DOConfiguration _objConfiguration;
        private readonly IMemoryCacheHelper _memoryCacheHelper;
        public readonly BOCommon _objBOCommon;
        List<DOATSLookupMaster> lstATSLookupMaster = null;
        private readonly ICacheService _cache;
        private readonly BOHCPClaimSearch _BOHCPClaimSearch;
        public ClaimSearchController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _memoryCacheHelper = memoryCacheHelper;
            _objBOCommon = bOCommon;
            _cache = cache;
            _BOHCPClaimSearch = new BOHCPClaimSearch(_objConfiguration, _cache);
        }

        /// <summary>
        /// Get Claims information based on search criteria
        /// </summary>
        /// <param name="strClaimSearch"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetClaimSearch(string strClaimSearch)
        {
            source = BOCommon.GetRefererURI(Request);
            try
            {
                List<DOClaimInfo> lstObjDOClaimInfo = null;
                DOClaimInfo objDOClaimInfo = null;
                _objBOClaimsSearch = new BOClaimsSearch(_objConfiguration, _cache);
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                if (!string.IsNullOrEmpty(strClaimSearch))
                {
                    objDOClaimInfo = new DOClaimInfo();
                    objDOClaimInfo = JsonConvert.DeserializeObject<DOClaimInfo>(strClaimSearch);

                    _result = _objBOClaimsSearch.GetClaimInformation(objDOClaimInfo, out lstObjDOClaimInfo);

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
                // _logger.Error(this.GetType().Name + "", (long)ErrorModuleName.WebService, (long)500, ex.ToString, " ", ex.StackTrace.ToString(), new DO.DOLoginUserDetails { ADM_UserInfoId = 10001 });
                return BadRequest();
            }
        }

        /// <summary>
        /// Get Claims information based on search criteria
        /// </summary>
        /// <param name="strClaimSearch"></param>
        /// <returns></returns>

        [HttpGet]
        public IActionResult GetRelatedClaimSearch(string strClaimSearch)
        {
            source = BOCommon.GetRefererURI(Request);
            try
            {
                List<DOClaimInfo> lstObjDOClaimInfo = null;
                DOClaimInfo objDOClaimInfo = null;
                _objBOClaimsSearch = new BOClaimsSearch(_objConfiguration, _cache);
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                if (!string.IsNullOrEmpty(strClaimSearch))
                {
                    objDOClaimInfo = new DOClaimInfo();
                    objDOClaimInfo = JsonConvert.DeserializeObject<DOClaimInfo>(strClaimSearch);

                    _result = _objBOClaimsSearch.GetRelatedClaims(objDOClaimInfo, out lstObjDOClaimInfo);

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
        public IActionResult GetAllClaim(string strClaimSearch)
        {
            string hcpFlag = "0";
            source = BOCommon.GetRefererURI(Request);
            try
            {
                Console.WriteLine("GetAllClaim - Entered");
                List<DOClaimInfo> lstObjDOClaimInfo = null;
                DOClaimInfo objDOClaimInfo = null;
                _objBOClaimsSearch = new BOClaimsSearch(_objConfiguration, _cache);
                HCPClaimMethods _objBOHCPClaimSearch = new(_memoryCacheHelper, _objConfiguration);
                _username = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? string.Empty;
                _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Request", strClaimSearch, _username);
                Console.WriteLine("GetAllClaim - Trace completed");
                if (!string.IsNullOrEmpty(strClaimSearch))
                {
                    objDOClaimInfo = new DOClaimInfo();
                    objDOClaimInfo = JsonConvert.DeserializeObject<DOClaimInfo>(strClaimSearch);

                    if (objDOClaimInfo.ClaimSystemLkup == (int)SourceSystemType.FACETS)
                    {
                        Console.WriteLine("GetAllClaim - Facets");
                        CSPFacetsMethods _objCSPFacetsMethods = new CSPFacetsMethods(_memoryCacheHelper, _objConfiguration, _httpContextAccessor, _objBOCommon);
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
                        if (!string.IsNullOrEmpty(objDOClaimInfo.MemberID))
                        {
                            lstObjDOClaimInfo = lstObjDOClaimInfo != null ? lstObjDOClaimInfo.Where(l => l.MemberID == objDOClaimInfo.MemberID).ToList() : null;
                        }
                        if (!string.IsNullOrEmpty(objDOClaimInfo.ProviderTaxID))
                        {
                            lstObjDOClaimInfo = lstObjDOClaimInfo != null ? lstObjDOClaimInfo.Where(l => l.TaxID == objDOClaimInfo.ProviderTaxID).ToList() : null;
                        }
                        if (!string.IsNullOrEmpty(objDOClaimInfo.ProviderName))
                        {
                            string ProviderName = objDOClaimInfo.ProviderName.ToLower();
                            if (objDOClaimInfo.SearchCriteriaProviderName == 1858004)//Exact match
                            {
                                lstObjDOClaimInfo = lstObjDOClaimInfo != null ? lstObjDOClaimInfo.Where(l => l.ProviderName.ToLower() == ProviderName).ToList() : null;
                            }
                            else if (objDOClaimInfo.SearchCriteriaProviderName == 1858005)//starts with
                            {
                                lstObjDOClaimInfo = lstObjDOClaimInfo != null ? lstObjDOClaimInfo.Where(l => l.ProviderName.ToLower().StartsWith(ProviderName)).ToList() : null;
                            }
                            else if (objDOClaimInfo.SearchCriteriaProviderName == 1858006)// Ends With
                            {
                                lstObjDOClaimInfo = lstObjDOClaimInfo != null ? lstObjDOClaimInfo.Where(l => l.ProviderName.ToLower().EndsWith(ProviderName)).ToList() : null;
                            }
                            else
                            {
                                lstObjDOClaimInfo = lstObjDOClaimInfo != null ? lstObjDOClaimInfo.Where(l => l.ProviderName.ToLower().Contains(ProviderName)).ToList() : null;
                            }
                        }
                        if (_result != ExceptionTypes.Success && _result != ExceptionTypes.ZeroRecords)
                        {
                            return BadRequest();
                        }
                    }
                    else if (objDOClaimInfo.ClaimSystemLkup == (int)SourceSystemType.COSMOS ||
                        objDOClaimInfo.ClaimSystemLkup == (int)SourceSystemType.NICE
                        || objDOClaimInfo.ClaimSystemLkup == (int)SourceSystemType.VAS 
                        || objDOClaimInfo.ClaimSystemLkup == (int)SourceSystemType.UHD)
                    {
                        Console.WriteLine("GetAllClaim - COSMOS/NICE");
                        if (objDOClaimInfo.ClaimSystemLkup == (int)SourceSystemType.COSMOS
                            || objDOClaimInfo.ClaimSystemLkup == (int)SourceSystemType.VAS
                            || objDOClaimInfo.ClaimSystemLkup == (int)SourceSystemType.UHD)
                        {
                            var exResult = _objBOCommon.GetRRTGPSAPiConfig(out lstATSLookupMaster);
                            hcpFlag = _objBOCommon.GetLookupValueBasedonCon(lstATSLookupMaster, (long)RRTGPSAPIConfig.EnableHCPForCOSMOS, 0);
                        }
                         if (hcpFlag == "1" && (objDOClaimInfo.ClaimSystemLkup == (int)SourceSystemType.COSMOS || objDOClaimInfo.ClaimSystemLkup == (int)SourceSystemType.VAS || objDOClaimInfo.ClaimSystemLkup == (int)SourceSystemType.UHD))
                        {
                            _result = _objBOHCPClaimSearch.GetAllClaims(objDOClaimInfo, out lstObjDOClaimInfo);
                        }
                        else
                        {
                            _result = _objBOClaimsSearch.GetAllClaims(objDOClaimInfo, out lstObjDOClaimInfo);
                        }
                    }
                    _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Response", _result.ToString(), _username);
                    if (lstObjDOClaimInfo != null && lstObjDOClaimInfo.Count > 0)
                    {
                        Console.WriteLine("GetAllClaim - response - " + lstObjDOClaimInfo[0].ClaimNumber);

                    }
                    Console.WriteLine("GetAllClaim - Final completed - ");
                    if (_result != ExceptionTypes.Success && _result != ExceptionTypes.ZeroRecords)
                    {
                        return BadRequest();
                    }
                }
                return Ok(lstObjDOClaimInfo);
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(source) && source.Contains("DDE"))
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
        public IActionResult GetAllClaimV2(string strClaimSearch)
        {
            source = BOCommon.GetRefererURI(Request);
            try
            {
                List<DOClaimInfo> lstObjDOClaimInfo = null;
                DOClaimInfo objDOClaimInfo = null;
                _objBOClaimsSearch = new BOClaimsSearch(_objConfiguration, _cache);
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                if (!string.IsNullOrEmpty(strClaimSearch))
                {
                    objDOClaimInfo = new DOClaimInfo();
                    objDOClaimInfo = JsonConvert.DeserializeObject<DOClaimInfo>(strClaimSearch);

                    if (objDOClaimInfo.ClaimSystemLkup == (int)SourceSystemType.FACETS)
                    {
                        CSPFacetsMethods _objCSPFacetsMethods = new CSPFacetsMethods(_memoryCacheHelper, _objConfiguration, _httpContextAccessor, _objBOCommon);
                        DOClaimSummaryRequest objDOClaimSummaryRequest = null;

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
                        _result = _objBOClaimsSearch.GetAllClaimsV2(objDOClaimInfo, out lstObjDOClaimInfo);
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
        public IActionResult GetClaimDetail(string strClaimSearch)
        {
            string hcpFlag = "0";
            source = BOCommon.GetRefererURI(Request);
            try
            {
                List<DOClaimInfo> lstObjDOClaimInfo = null;
                DOClaimInfo objDOClaimInfo = null;
                HCPClaimMethods _objBOHCPClaimSearch = new(_memoryCacheHelper, _objConfiguration);
                _objBOClaimsSearch = new BOClaimsSearch(_objConfiguration, _cache);
                long? requestID = null;
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Request", strClaimSearch, _username);

                if (!string.IsNullOrEmpty(strClaimSearch))
                {
                    objDOClaimInfo = new DOClaimInfo();
                    objDOClaimInfo = JsonConvert.DeserializeObject<DOClaimInfo>(strClaimSearch);
                    if (objDOClaimInfo.ClaimSystemLkup == (int)SourceSystemType.FACETS)
                    {
                        CSPFacetsMethods _objCSPFacetsMethods = new CSPFacetsMethods(_memoryCacheHelper, _objConfiguration, _httpContextAccessor, _objBOCommon);
                        //objDOClaimInfo = new DOClaimInfo();
                        //objDOClaimInfo.ClaimNumber = "19E119458800";

                        _result = _objCSPFacetsMethods.ClaimDetails(objDOClaimInfo, out lstObjDOClaimInfo);

                        if (_result != ExceptionTypes.Success && _result != ExceptionTypes.ZeroRecords)
                        {
                            return BadRequest();
                        }
                    }
                    else if (objDOClaimInfo.ClaimSystemLkup == (int)SourceSystemType.COSMOS || 
                        objDOClaimInfo.ClaimSystemLkup == (int)SourceSystemType.NICE ||
                        objDOClaimInfo.ClaimSystemLkup == (int)SourceSystemType.VAS ||
                        objDOClaimInfo.ClaimSystemLkup == (int)SourceSystemType.UHD)
                    {
                        if (objDOClaimInfo.ClaimSystemLkup == (int)SourceSystemType.COSMOS ||
                            objDOClaimInfo.ClaimSystemLkup == (int)SourceSystemType.VAS ||
                            objDOClaimInfo.ClaimSystemLkup == (int)SourceSystemType.UHD)
                        {
                            var exResult = _objBOCommon.GetRRTGPSAPiConfig(out lstATSLookupMaster);
                            hcpFlag = _objBOCommon.GetLookupValueBasedonCon(lstATSLookupMaster, (long)RRTGPSAPIConfig.EnableHCPForCOSMOS, 0);
                        }
                        if (hcpFlag == "1" && objDOClaimInfo.ClaimSystemLkup == (int)SourceSystemType.COSMOS)
                        {
                            _result = _objBOHCPClaimSearch.GetClaimDetail(objDOClaimInfo, out lstObjDOClaimInfo, out requestID);
                        }
                        else if (hcpFlag == "1" && (objDOClaimInfo.ClaimSystemLkup == (int)SourceSystemType.VAS || objDOClaimInfo.ClaimSystemLkup == (int)SourceSystemType.UHD))
                        {
                            _result = _objBOHCPClaimSearch.GetClaimDetail(objDOClaimInfo, out lstObjDOClaimInfo, out requestID);
                        }
                        else
                        {
                            _result = _objBOClaimsSearch.GetClaimDetail(objDOClaimInfo, out lstObjDOClaimInfo, out requestID);
                        }

                    }
                    _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Response", _result.ToString(), _username);
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
        public IActionResult GetClaimDetailV2(string strClaimSearch)
        {
            source = BOCommon.GetRefererURI(Request);
            try
            {
                List<DOClaimInfo> lstObjDOClaimInfo = null;
                DOClaimInfo objDOClaimInfo = null;
                _objBOClaimsSearch = new BOClaimsSearch(_objConfiguration, _cache);
                long? requestID = null;
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                if (!string.IsNullOrEmpty(strClaimSearch))
                {
                    objDOClaimInfo = new DOClaimInfo();
                    objDOClaimInfo = JsonConvert.DeserializeObject<DOClaimInfo>(strClaimSearch);
                    if (objDOClaimInfo.ClaimSystemLkup == (int)SourceSystemType.FACETS)
                    {
                        CSPFacetsMethods _objCSPFacetsMethods = new CSPFacetsMethods(_memoryCacheHelper, _objConfiguration, _httpContextAccessor, _objBOCommon);

                        _result = _objCSPFacetsMethods.ClaimDetails(objDOClaimInfo, out lstObjDOClaimInfo);

                        if (_result != ExceptionTypes.Success && _result != ExceptionTypes.ZeroRecords)
                        {
                            return BadRequest();
                        }
                    }
                    else if (objDOClaimInfo.ClaimSystemLkup == (int)SourceSystemType.COSMOS || objDOClaimInfo.ClaimSystemLkup == (int)SourceSystemType.NICE)
                    {
                        _result = _objBOClaimsSearch.GetClaimDetailV2(objDOClaimInfo, out lstObjDOClaimInfo, out requestID);
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
        public IActionResult GetClaimDetailProxy(string strClaimSearch)
        {
            //source = BOCommon.GetRefererURI(Request);

            //RRTGPSAPICALL
            List<DOClaimInfo> lstObjDOClaimInfo = null;
            string rrtGPSAPIURL = string.Empty;
            long RequestId = 0;

            try
            {
                rrtGPSAPIURL = _objConfiguration.AppSettings.RRTGPSAPIUrl;
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
                using (var client = new HttpClient(new HttpClientHandler() { UseDefaultCredentials = true }))
                {
                    //client.BaseAddress = new Uri();
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var result = client.GetAsync(rrtGPSAPIURL + "ClaimSearch/GetClaimDetail?strClaimSearch=" + strClaimSearch);

                    HttpResponseMessage response = result.Result;

                    if (response.IsSuccessStatusCode)
                    {
                        var claim_data = response.Content.ReadAsStringAsync();

                        string jsonString = claim_data.Result;

                        if (long.TryParse(jsonString, out RequestId))
                        {
                            return Ok(RequestId);
                        }
                        else
                        {
                            lstObjDOClaimInfo = JsonConvert.DeserializeObject<List<DOClaimInfo>>(claim_data.Result);

                        }
                    }
                    else
                    {
                        return BadRequest("Something Went Wrong" + response.ToString());
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
            #region DB2 Provider Method Call Direct Call Fix commented on 12/16/2020 by Naseer
            //try
            //{
            //    DOClaimInfo objDOClaimInfo = null;
            //    _objBOClaimsSearch = new BOClaimsSearch();
            //    long? requestID = null;
            //    _username = System.Web.HttpContext.Current.User.Identity.Name;
            //    if (!string.IsNullOrEmpty(strClaimSearch))
            //    {
            //        objDOClaimInfo = new DOClaimInfo();
            //        objDOClaimInfo = JsonConvert.DeserializeObject<DOClaimInfo>(strClaimSearch);
            //        if (objDOClaimInfo.ClaimSystemLkup == (int)SourceSystemType.FACETS)
            //        {
            //            CSPFacetsMethods _objCSPFacetsMethods = new CSPFacetsMethods();
            //            //objDOClaimInfo = new DOClaimInfo();
            //            //objDOClaimInfo.ClaimNumber = "19E119458800";

            //            _result = _objCSPFacetsMethods.ClaimDetails(objDOClaimInfo, out lstObjDOClaimInfo);

            //            if (_result != ExceptionTypes.Success && _result != ExceptionTypes.ZeroRecords)
            //            {
            //                return BadRequest();
            //            }
            //        }
            //        else if (objDOClaimInfo.ClaimSystemLkup == (int)SourceSystemType.COSMOS || objDOClaimInfo.ClaimSystemLkup == (int)SourceSystemType.NICE)
            //        {
            //            _result = _objBOClaimsSearch.GetClaimDetail(objDOClaimInfo, out lstObjDOClaimInfo, out requestID);
            //        }
            //        if (_result != ExceptionTypes.Success && _result != ExceptionTypes.ZeroRecords)
            //        {
            //            return BadRequest();
            //        }
            //    }
            //    if (requestID != null)
            //        return Ok(requestID);
            //    else
            //        return Ok(lstObjDOClaimInfo);
            //}
            //catch (Exception ex)
            //{
            //    if (source.Contains("DDE"))
            //    {
            //        _objBOCommon.ErrorLog(LoggedInUserId, this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), ex.Message, ex.ToString());
            //    }
            //    else
            //    {
            //        BOCommon.LogError(this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
            //    }
            //    throw ex;
            //}
            #endregion




        }

        [HttpGet]
        public IActionResult RxGetClaimDetails(string strClaimSearch)
        {
            source = BOCommon.GetRefererURI(Request);

            String Message = string.Empty;
            List<DORxClaiminfo> LstDORxClaiminfo = null;
            DORxClaiminfo objDORxClaiminfo = new DORxClaiminfo();
            try
            {
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Request", strClaimSearch, _username);

                if (!string.IsNullOrEmpty(strClaimSearch))
                {
                    DOClaimInfo objDOClaimInfo = JsonConvert.DeserializeObject<DOClaimInfo>(strClaimSearch);
                    RxClaimmethods objRxClaimMethods = new RxClaimmethods(_memoryCacheHelper, _objConfiguration);

                    // Read DB flag to decide which approach to use
                    string rxClaimNewAPIFlag = "0";
                    var exResult = _objBOCommon.GetRRTGPSAPiConfig(out lstATSLookupMaster);
                    rxClaimNewAPIFlag = _objBOCommon.GetLookupValueBasedonCon(lstATSLookupMaster, (long)RRTGPSAPIConfig.EnableRxClaimNewAPI, 0);

                    if (rxClaimNewAPIFlag == "1")
                    {
                        // NEW approach: single consolidated API call
                        DORxClaimAdvDetailRequest objRequest = new DORxClaimAdvDetailRequest();

                        RxClaimAdvSearchInputMetaData objSearchInputMetaData = new RxClaimAdvSearchInputMetaData();
                        RxClaimSystemParms objRxClaimSystemParms = new RxClaimSystemParms();
                        objRxClaimSystemParms.key = "PBMUserId";
                        objRxClaimSystemParms.value = "BCBSMI-USER";
                        objSearchInputMetaData.rxClaimSystemParms = objRxClaimSystemParms;
                        objSearchInputMetaData.consumerAppId = "RRT.ATS";
                        objSearchInputMetaData.consumerAppType = "IA";
                        objSearchInputMetaData.consumerInstanceId = null;
                        objSearchInputMetaData.consumerType = "CA";
                        objSearchInputMetaData.externalCorrelationId = Guid.NewGuid().ToString();
                        objSearchInputMetaData.rXClaimInstanceId = objDOClaimInfo.RxProviderInstanceId ?? "BOOK1";
                        objSearchInputMetaData.pagination = null;
                        objRequest.searchInputMetaData = objSearchInputMetaData;

                        objRequest.claimNumber = objDOClaimInfo.ClaimNumber;
                        objRequest.claimSequenceNumber = objDOClaimInfo.claimSequenceNumber;
                        objRequest.internalId = "ReadRxClaimAdvDetail-" + Guid.NewGuid().ToString();

                        _result = objRxClaimMethods.GetRxClaimAdvDetails(objRequest, ref objDORxClaiminfo, out Message);

                        if (Message == "Success" && !string.IsNullOrEmpty(objDORxClaiminfo.MemberID))
                        {
                            DOHemiMemberSearchReuest objDOHemiMemberSearchReuest = null;
                            List<DOGPSMemberDetails> memberSearchDetails = new List<DOGPSMemberDetails>();
                            try
                            {
                                SearchInputMetaData objSearchInputData = new SearchInputMetaData();
                                objDOHemiMemberSearchReuest = new DOHemiMemberSearchReuest();
                                HemiMethods objHemiMethods = new HemiMethods(_memoryCacheHelper, _objConfiguration, _cache);
                                objSearchInputData.applicationId = "RRT-ATS";
                                objSearchInputData.consumerAppType = "IA";
                                objSearchInputData.consumerType = "CA";
                                objSearchInputData.externalCorrelationId = "RRTATS-SAMPLE";
                                objDOHemiMemberSearchReuest.searchInputMetaData = objSearchInputData;
                                objDOHemiMemberSearchReuest.id = objDORxClaiminfo.MemberID;//ACUAZQ2TEST1

                                objHemiMethods.GetHemiMemberSearch(objDOHemiMemberSearchReuest, out memberSearchDetails);

                                // Map ClientBenefitCode, BPL, and PlanCode from member search results
                                if (memberSearchDetails != null && memberSearchDetails.Count > 0)
                                {
                                    var matchedMember = memberSearchDetails.FirstOrDefault(m =>
                                    m.CarrierId == objDORxClaiminfo.Carrier && m.AccountId == objDORxClaiminfo.Account && m.GroupId == objDORxClaiminfo.Group) ?? memberSearchDetails[0];

                                    if (!string.IsNullOrEmpty(matchedMember.ClientBenefitCode))
                                    {
                                       objDORxClaiminfo.ClientBenefitCode = matchedMember.ClientBenefitCode;
                                        objDORxClaiminfo.BPL = matchedMember.BPL;
                                    }
                                    if (!string.IsNullOrEmpty(matchedMember.PlanCode))
                                    {
                                        objDORxClaiminfo.PlanCode = matchedMember.PlanCode;
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                // Do not interrupt claim detail flow
                            }
                        }
                    }
                    else
                    {
                        // OLD approach: five separate API calls
                        DorxGetClaimDetailsRequest claimDetails = new DorxGetClaimDetailsRequest();
                        requestContext objRequestContext = new requestContext();
                        invocationContext objInvocationContext = new invocationContext();
                        objInvocationContext.consumerAppId = "CAT_UHCATS";
                        objInvocationContext.consumerInstanceId = "?";
                        objInvocationContext.providerAppId = "SXC";
                        objInvocationContext.providerInstanceId = objDOClaimInfo.RxProviderInstanceId ?? "BOOK1";
                        serviceContext objServiceContext = new serviceContext();
                        objRequestContext.serviceContext = objServiceContext;
                        objRequestContext.invocationContext = objInvocationContext;
                        claimDetails.requestContext = objRequestContext;
                        claimDetails.claimId = objDOClaimInfo.ClaimNumber;
                        claimDetails.claimSequenceNumber = objDOClaimInfo.claimSequenceNumber;
                        requestedSections objRequestedSections = new requestedSections();
                        List<string> claimSubEntity = new List<string>
                        { "Claim" };
                        objRequestedSections.subEntity = claimSubEntity;
                        claimDetails.requestedSections = objRequestedSections;
                        objRxClaimMethods.GetRxClaimDetails(claimDetails, ref objDORxClaiminfo, out Message);

                        if (Message == "Success" && !string.IsNullOrEmpty(objDORxClaiminfo.ProductID) && !string.IsNullOrEmpty(objDORxClaiminfo.ProductGPI) && !string.IsNullOrEmpty(objDORxClaiminfo.Qualifier))
                        {
                            try
                            {
                                string DrugMessage = String.Empty;
                                DorxGetDrugDetailsRequest drugdetails = null;
                                drugdetails = new DorxGetDrugDetailsRequest();
                                drugdetails.requestContext = objRequestContext;
                                drugdetails.productId = objDORxClaiminfo.ProductID;
                                drugdetails.productIDQualifier = objDORxClaiminfo.Qualifier;
                                drugdetails.gpi14 = objDORxClaiminfo.ProductGPI;
                                requestedSections objRequestedSections4 = new requestedSections();
                                List<string> drugSubEntity = new List<string> { "Drug" };
                                objRequestedSections4.subEntity = drugSubEntity;
                                drugdetails.requestedSections = objRequestedSections4;
                                objRxClaimMethods.GetRxDrugDetails(drugdetails, ref objDORxClaiminfo, out DrugMessage);
                            }
                            catch (Exception)
                            {

                                // throw ex;
                            }
                        }

                        if (Message == "Success" && !string.IsNullOrEmpty(objDORxClaiminfo.MemberID) && objDORxClaiminfo.DOB != null)
                        {
                            try
                            {
                                String membercagMessage = String.Empty;
                                DoRxgetmemberallcagsncRequest membercag = null;
                                membercag = new DoRxgetmemberallcagsncRequest();
                                //objInvocationContext.providerInstanceId = "BOOK1";
                                applicationContext objapplicationContext1 = new applicationContext();
                                objRequestContext.applicationContext = objapplicationContext1;
                                objRequestContext.invocationContext = objInvocationContext;
                                membercag.requestContext = objRequestContext;
                                dob objdob = new dob();
                                objdob.date = objDORxClaiminfo.DOB;
                                membercag.dob = objdob;
                                membercag.memberId = objDORxClaiminfo.MemberID;
                                objRxClaimMethods.GetRxgetmemberallcagsnc(membercag, ref objDORxClaiminfo, out membercagMessage);
                            }
                            catch (Exception)
                            {
                                //throw ex;
                            }
                        }

                        if (Message == "Success" && !string.IsNullOrEmpty(objDORxClaiminfo.MemberID))
                        {
                            try
                            {
                                String MemberMessage = String.Empty;
                                DorxGetMemberDetailsRequest MemberDetails = null;
                                MemberDetails = new DorxGetMemberDetailsRequest();
                                applicationContext objapplicationContext = new applicationContext();
                                ClientHierarchy objclientHierarchy = new ClientHierarchy();
                                List<ClientHierarchyLevel> lstClientHierarchyLevel = new List<ClientHierarchyLevel>();
                                ClientHierarchyLevel objClientHierarchyLevel = new ClientHierarchyLevel();
                                objClientHierarchyLevel.hierarchyLevelName = "Carrier";
                                objClientHierarchyLevel.hierarchyLevelDesc = "AFL";
                                objClientHierarchyLevel.hierarchyLevelValue = objDORxClaiminfo.Carrier; //"PSI3510";
                                objClientHierarchyLevel.hierarchyOrderNumber = 1;
                                lstClientHierarchyLevel.Add(objClientHierarchyLevel);
                                objClientHierarchyLevel.hierarchyLevelName = "Account";
                                objClientHierarchyLevel.hierarchyLevelDesc = "AFL";
                                objClientHierarchyLevel.hierarchyLevelValue = objDORxClaiminfo.Account;//"07124810712481";
                                objClientHierarchyLevel.hierarchyOrderNumber = 2;
                                lstClientHierarchyLevel.Add(objClientHierarchyLevel);
                                objClientHierarchyLevel.hierarchyLevelName = "Group";
                                objClientHierarchyLevel.hierarchyLevelDesc = "ACTIVES (MED/RX/VISION)";
                                objClientHierarchyLevel.hierarchyLevelValue = objDORxClaiminfo.Group;//"00590059";
                                objClientHierarchyLevel.hierarchyOrderNumber = 3;
                                lstClientHierarchyLevel.Add(objClientHierarchyLevel);

                                objclientHierarchy.ClientHierarchyLevel = lstClientHierarchyLevel;
                                objapplicationContext.ClientHierarchy = objclientHierarchy;
                                objapplicationContext.memberId = objDORxClaiminfo.MemberID;
                                requestedSections objRequestedSections1 = new requestedSections();
                                List<string> memberSubEntity = new List<string>
                            { "GroupEligibility" };
                                objRequestedSections1.subEntity = memberSubEntity;
                                MemberDetails.requestedSections = objRequestedSections1;
                                MemberDetails.requestContext = objRequestContext;
                                MemberDetails.requestContext.applicationContext = objapplicationContext;

                                objRxClaimMethods.GetRxMemberDetails(MemberDetails, ref objDORxClaiminfo, out MemberMessage);
                            }
                            catch (Exception)
                            {
                                //throw ex;
                            }
                        }

                        if (Message == "Success" && !string.IsNullOrEmpty(objDORxClaiminfo.PlanId) && !string.IsNullOrEmpty(objDORxClaiminfo.planEffectiveDate))
                        {
                            try
                            {
                                String PlanMessage = String.Empty;

                                DorxGetPlanDetailsRequest PlanDetails = null;
                                PlanDetails = new DorxGetPlanDetailsRequest();
                                requestedSections objRequestedSections2 = new requestedSections();
                                List<string> PlanSubEntity = new List<string>
                        { "PlanDetails" };
                                objRequestedSections2.subEntity = PlanSubEntity;
                                PlanDetails.requestedSections = objRequestedSections2;
                                PlanDetails.requestContext = objRequestContext;
                                //PlanDetails.planID = "001066";
                                //PlanDetails.planEffectiveDate = "2001-01-01-06:00";
                                PlanDetails.planID = objDORxClaiminfo.PlanId;
                                PlanDetails.planEffectiveDate = objDORxClaiminfo.planEffectiveDate;
                                PlanDetails.requestContext.applicationContext = null;
                                objRxClaimMethods.GetRxPlanDetails(PlanDetails, ref objDORxClaiminfo, out PlanMessage);
                                //if (Message == "Success")
                                //{
                                //    DorxGetPlanProductDetailsRequest PlanProductDetails = null;
                                //    PlanProductDetails = new DorxGetPlanProductDetailsRequest();
                                //    applicationContext objapplicationContext1 = new applicationContext();
                                //    objRequestContext.applicationContext = objapplicationContext1;
                                //    PlanProductDetails.requestContext = objRequestContext;

                                //    PlanProductDetails.planId = "20DPF001";
                                //    PlanEffectiveDate objplanEffectiveDate = new PlanEffectiveDate();
                                //    objplanEffectiveDate.date = "1991-01-01-06:00";
                                //    PlanProductDetails.planEffectiveDate = objplanEffectiveDate;
                                //    PlanProductDetails.gpi = "36100030000310";
                                //    objRxClaimMethods.GetRxProductDetails(PlanProductDetails, ref objDORxClaiminfo, out Message);

                                //}

                            }
                            catch (Exception)
                            {
                                // throw ex;
                            }
                        }


                        _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Response", Message, _username);

                    }
                    //if (LstDORxClaiminfo!=null)
                    //{
                    //    return Ok(LstDORxClaiminfo);
                    //}
                    //else
                    //{
                    //    return Ok(Message);
                    //}

                }
                LstDORxClaiminfo = new List<DORxClaiminfo>();
                LstDORxClaiminfo.Add(objDORxClaiminfo);
                if (Message != "Success")
                {
                    return Ok(Message);
                }
                else
                {
                    return Ok(LstDORxClaiminfo);
                }
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
        public IActionResult HemiGetClaimDetails(string strClaimSearch)
        {
            source = BOCommon.GetRefererURI(Request);
            DOHemiClaimSearchReuest claimDetails = null;
            List<DORxClaiminfo> LstDORxClaiminfo = null;
            DORxClaiminfo objDORxClaiminfo = new DORxClaiminfo();
            String Message = string.Empty;
            source = BOCommon.GetRefererURI(Request);
            try
            {
                DOClaimInfo objDOClaimInfo = null;
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Request", strClaimSearch, _username);

                claimDetails = new DOHemiClaimSearchReuest();

                objDOClaimInfo = JsonConvert.DeserializeObject<DOClaimInfo>(strClaimSearch);
                HemiMethods objHemiMethods = new HemiMethods(_memoryCacheHelper, _objConfiguration, _cache);
                searchInputMetaData objSearchInputMetaData = new searchInputMetaData();
                objSearchInputMetaData.consumerAppId = "RRT-ATS";
                objSearchInputMetaData.consumerAppType = "IA";
                objSearchInputMetaData.consumerType = "CA";
                objSearchInputMetaData.externalCorrelationId = "RRT-ATS-Sample884";
                cagmiInfo objCagmiInfo = new cagmiInfo();
                objCagmiInfo.sourceSystemInstance = objDOClaimInfo.RxProviderInstanceId ?? "BOOK1";
                objCagmiInfo.carrierId = objDOClaimInfo.carrierId; //"ACUAZ";
                objCagmiInfo.accountId = objDOClaimInfo.accountId; // "ACUAZCHP";
                objCagmiInfo.groupId = objDOClaimInfo.groupId; //"ACUAZCHP";
                objCagmiInfo.memberId = objDOClaimInfo.MemberID;// "ACUNYCHPPAIDTST";
                objCagmiInfo.memberIdSearchOperator = "E";

                claimDetails.searchInputMetaData = objSearchInputMetaData;
                claimDetails.cagmiInfo = objCagmiInfo;
                if (objDOClaimInfo.dateOfServiceFrom != null)
                    claimDetails.fillDateFrom = objDOClaimInfo.dateOfServiceFrom.Value.ToString("yyyyMMdd"); // "20210801";
                if (objDOClaimInfo.dateOfServiceTo != null)
                    claimDetails.fillDateThru = objDOClaimInfo.dateOfServiceTo.Value.ToString("yyyyMMdd"); //"20210831";

                _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Request collected all data", "", _username);

                List<DOHemiClaimInfo> objlstHemiClaimInfo = new List<DOHemiClaimInfo>();
                objHemiMethods.GetHemiClaimDetails(claimDetails, ref objDORxClaiminfo, out objlstHemiClaimInfo, out Message);
                _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Request collected all data", Message, _username);

                if (!string.IsNullOrEmpty(Message))
                {
                    return Ok(Message);
                }

                return Ok(objlstHemiClaimInfo);
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
        public IActionResult HemiMemberSearch(string strHemiMemberSearch)
        {
            source = BOCommon.GetRefererURI(Request);
            DOHemiMemberSearchReuest objDOHemiMemberSearchReuest = null;
            DOMemberSearchCriteria memberDetails = null;
            List<DOGPSMemberDetails> memberSearchDetails = new List<DOGPSMemberDetails>();
            String Message = string.Empty;
            source = BOCommon.GetRefererURI(Request);
            try
            {
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Request", strHemiMemberSearch, _username);

                objDOHemiMemberSearchReuest = new DOHemiMemberSearchReuest();
                memberDetails = new DOMemberSearchCriteria();
                memberDetails = JsonConvert.DeserializeObject<DOMemberSearchCriteria>(strHemiMemberSearch);
                HemiMethods objHemiMethods = new HemiMethods(_memoryCacheHelper, _objConfiguration, _cache);
                SearchInputMetaData objSearchInputMetaData = new SearchInputMetaData();
                objSearchInputMetaData.applicationId = "RRT-ATS";
                objSearchInputMetaData.consumerAppType = "IA";
                objSearchInputMetaData.consumerType = "CA";
                objSearchInputMetaData.externalCorrelationId = "RRTATS-SAMPLE";


                objDOHemiMemberSearchReuest.searchInputMetaData = objSearchInputMetaData;
                objDOHemiMemberSearchReuest.id = memberDetails.MemberId;

                _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Request collected all data", "", _username);

                objHemiMethods.GetHemiMemberSearch(objDOHemiMemberSearchReuest, out memberSearchDetails);
                _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Request collected all data", Message, _username);

                if (!string.IsNullOrEmpty(Message))
                {
                    return Ok(Message);
                }

                return Ok(memberSearchDetails);
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
        public IActionResult HemiTest(string strClaimSearch)
        {
            _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Request", strClaimSearch, _username);

            source = BOCommon.GetRefererURI(Request);
            DOHemiClaimSearchReuest claimDetails = null;
            List<DORxClaiminfo> LstDORxClaiminfo = null;
            DORxClaiminfo objDORxClaiminfo = new DORxClaiminfo();
            String Message = string.Empty;
            source = BOCommon.GetRefererURI(Request);
            try
            {
                DOClaimInfo objDOClaimInfo = null;
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Request", strClaimSearch, _username);

                claimDetails = new DOHemiClaimSearchReuest();


                return Ok("Test Hemi called");
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
        public IActionResult GetClaimSearchHistoryCOS(string strClaimSearch)
        {
            source = BOCommon.GetRefererURI(Request)??string.Empty;
            try
            {
                List<string> lstClaimNumber = null;
                DOClaimInfo objDOClaimInfo = null;
                HCPClaimMethods _objBOHCPClaimSearch = new(_memoryCacheHelper, _objConfiguration);
                _objBOClaimsSearch = new BOClaimsSearch(_objConfiguration, _cache);
                long? requestID = null;
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Request", strClaimSearch, _username);

                if (!string.IsNullOrEmpty(strClaimSearch))
                {
                    objDOClaimInfo = new DOClaimInfo();
                    objDOClaimInfo = JsonConvert.DeserializeObject<DOClaimInfo>(strClaimSearch);

                    _result = _objBOHCPClaimSearch.GetClaimSearchHistoryCOS(objDOClaimInfo, out lstClaimNumber, out requestID);

                    _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Response", _result.ToString(), _username);
                    if (_result != ExceptionTypes.Success && _result != ExceptionTypes.ZeroRecords)
                    {
                        return BadRequest();
                    }
                }
                if (requestID != null)
                    return Ok(requestID);
                else
                    return Ok(lstClaimNumber);
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(source) && source.Contains("DDE"))
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
