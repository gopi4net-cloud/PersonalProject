using ANGDDEAPI.Common;
using ANGDDEAPIBO;
using ANGDDEAPIDO;
using ANGDDEAPIDO.BHLinxAuth;
using ANGDDEAPIDO.Interface;
using ANGDDEAPIDO.PSaaS;
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
    public class BHAuthController : ControllerBase
    {
        public static long LoggedInUserId = 0;
        BHLinxAuthMethods _objBOAuthSearch;
        ExceptionTypes _result;
        public string _username = string.Empty;
        string source = string.Empty;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DOConfiguration _objConfiguration;
        private readonly IMemoryCacheHelper _memoryCacheHelper;
        public readonly BOCommon _objBOCommon;

        private readonly ICacheService _cache;

        public BHAuthController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _memoryCacheHelper = memoryCacheHelper;
            _objBOCommon = bOCommon;
            _cache = cache;
        }

        /// <summary>
        /// Get BH Auth information based on search criteria
        /// </summary>
        /// <param name="strClaimSearch"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetAuthSummarySearch(string strSearch)
        {
            source = BOCommon.GetRefererURI(Request);
            try
            {
                List<DOBHAuthDetailInfo> lstObjDOAuthDetail = new List<DOBHAuthDetailInfo>();
                List<DOBHAuthDetailInfo> lstObjDOAuthDetailResponse = null;
                List<DOBHAuthDetailInfo> lstObjDOlinxAppealNotesResponse = null;

                DOBHAuthInputRequest objDOAuthRequestInput = null;
              
                _objBOAuthSearch = new BHLinxAuthMethods(_memoryCacheHelper, _objConfiguration, _httpContextAccessor, _objBOCommon);
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                if (!string.IsNullOrEmpty(strSearch.ToString()))
                {
                    objDOAuthRequestInput = new DOBHAuthInputRequest();
                    objDOAuthRequestInput = JsonConvert.DeserializeObject<DOBHAuthInputRequest>(strSearch);
                    //objDOAuthRequestInput.accountId = "";

                      _result = _objBOAuthSearch.AuthSummary(objDOAuthRequestInput, out lstObjDOAuthDetailResponse);
                    if(lstObjDOAuthDetailResponse != null && lstObjDOAuthDetailResponse.Count>0)
                    {
                        lstObjDOAuthDetail.AddRange(lstObjDOAuthDetailResponse);
                    }
                    objDOAuthRequestInput = JsonConvert.DeserializeObject<DOBHAuthInputRequest>(strSearch);
                    if (!string.IsNullOrEmpty(objDOAuthRequestInput.searchBeginDate) && !string.IsNullOrEmpty(objDOAuthRequestInput.searchEndDate))
                    {
                        objDOAuthRequestInput.searchBeginDate = Convert.ToDateTime(objDOAuthRequestInput.searchBeginDate).ToString("yyyy-MM-dd");
                        objDOAuthRequestInput.searchEndDate = Convert.ToDateTime(objDOAuthRequestInput.searchEndDate).ToString("yyyy-MM-dd");
                        //objDOAuthRequestInput.noteTypeId = 97;// 93 - restros and 97 - Denial
                        objDOAuthRequestInput.subscriberNbr = objDOAuthRequestInput.subscriberId;
                        _result = _objBOAuthSearch.AuthlinxAppealNotesSummary(objDOAuthRequestInput, out lstObjDOlinxAppealNotesResponse);
                        if (lstObjDOlinxAppealNotesResponse != null && lstObjDOlinxAppealNotesResponse.Count > 0)
                        {
                            lstObjDOAuthDetail.AddRange(lstObjDOlinxAppealNotesResponse);
                        }
                       
                    }

                }
                return Ok(lstObjDOAuthDetail);
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

        [HttpGet]
        public IActionResult GetAuthDetails(string strSearch)
        {
            source = BOCommon.GetRefererURI(Request);
            try
            {
                List<DOBHAuthDetailInfo> lstObjDOAuthDetailResponse = null;

                DOBHAuthInputRequest objDOAuthRequestInput = null;
                _objBOAuthSearch = new BHLinxAuthMethods(_memoryCacheHelper, _objConfiguration, _httpContextAccessor, _objBOCommon);
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                if (!string.IsNullOrEmpty(strSearch.ToString()))
                {
                    objDOAuthRequestInput = new DOBHAuthInputRequest();
                    objDOAuthRequestInput = JsonConvert.DeserializeObject<DOBHAuthInputRequest>(strSearch);

                    _result = _objBOAuthSearch.AuthDetail(objDOAuthRequestInput, out lstObjDOAuthDetailResponse);

                }
                return Ok(lstObjDOAuthDetailResponse);
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

        /// <summary>
        /// Get BH Auth information based on search criteria
        /// </summary>
        /// <param name="strClaimSearch"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetICUEAuthSummarySearch(string strSearch)
        {
            source = BOCommon.GetRefererURI(Request);
            try
            {
                List<DOBHAuthDetailInfo> lstObjDOAuthDetail = new List<DOBHAuthDetailInfo>();
                List<DOICUEAuthSummaryInfo> lstObjDOAuthDetailResponse = null;
                List<DOBHAuthDetailInfo> lstObjDOlinxAppealNotesResponse = null;

                DOBHAuthInputRequest objDOAuthRequestInput = null;

                _objBOAuthSearch = new BHLinxAuthMethods(_memoryCacheHelper, _objConfiguration, _httpContextAccessor, _objBOCommon);
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                if (!string.IsNullOrEmpty(strSearch.ToString()))
                {
                    objDOAuthRequestInput = new DOBHAuthInputRequest();
                    objDOAuthRequestInput = JsonConvert.DeserializeObject<DOBHAuthInputRequest>(strSearch);
                    //objDOAuthRequestInput.accountId = "";

                    _result = _objBOAuthSearch.ICUEAuthSummaryInformation(objDOAuthRequestInput, out lstObjDOAuthDetailResponse);
                    //if (lstObjDOAuthDetailResponse != null && lstObjDOAuthDetailResponse.Count > 0)
                    //{
                    //    lstObjDOAuthDetail.AddRange(lstObjDOAuthDetailResponse);
                    //}

                }
                return Ok(lstObjDOAuthDetailResponse);
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

        [HttpGet]
        public IActionResult GetICUEAuthDetails(string strSearch)
        {
            source = BOCommon.GetRefererURI(Request);
            try
            {
                //List<DOBHAuthDetailInfo> lstObjDOAuthDetailResponse = null;

                List<DOBHICUEAuthDetailInfo> lstObjDOAuthDetailResponse = null;

                DOBHAuthInputRequest objDOAuthRequestInput = null;
                _objBOAuthSearch = new BHLinxAuthMethods(_memoryCacheHelper, _objConfiguration, _httpContextAccessor, _objBOCommon);
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                if (!string.IsNullOrEmpty(strSearch.ToString()))
                {
                    objDOAuthRequestInput = new DOBHAuthInputRequest();
                    objDOAuthRequestInput = JsonConvert.DeserializeObject<DOBHAuthInputRequest>(strSearch);

                    _result = _objBOAuthSearch.ICUEAuthDetails(objDOAuthRequestInput, out lstObjDOAuthDetailResponse);

                }
                return Ok(lstObjDOAuthDetailResponse);
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
