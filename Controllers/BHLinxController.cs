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
    public class BHLinxController : ControllerBase
    {
        public static long LoggedInUserId = 0;
        BHLinxNotesMethods _objBOAuthSearch;
        ExceptionTypes _result;
        public string _username = string.Empty;
        string source = string.Empty;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DOConfiguration _objConfiguration;
        private readonly IMemoryCacheHelper _memoryCacheHelper;
        public readonly BOCommon _objBOCommon;

        private readonly ICacheService _cache;

        public BHLinxController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
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

        public IActionResult GetLinxNotesSummarySearch(string strSearch)
        {
            source = BOCommon.GetRefererURI(Request);
            try
            {
                List<DOBHLinxNotesDetailInfo> lstObjDOAuthDetail = new List<DOBHLinxNotesDetailInfo>();
                List<DOBHLinxNotesDetailInfo> lstObjDOAuthDetailResponse = null;
                List<BHLinxNotes> lstObjDOlinxAppealNotesResponse = null;

                DOBHAuthInputRequest objDOAuthRequestInput = null;

                _objBOAuthSearch = new BHLinxNotesMethods(_memoryCacheHelper, _objConfiguration, _httpContextAccessor, _objBOCommon);
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                if (!string.IsNullOrEmpty(strSearch.ToString()))
                {
                    //objDOAuthRequestInput = new DOBHAuthInputRequest();
                    //objDOAuthRequestInput = JsonConvert.DeserializeObject<DOBHAuthInputRequest>(strSearch);
                    //objDOAuthRequestInput.accountId = "";

                    //_result = _objBOAuthSearch.AuthSummary(objDOAuthRequestInput, out lstObjDOAuthDetailResponse);
                    //if (lstObjDOAuthDetailResponse != null && lstObjDOAuthDetailResponse.Count > 0)
                    //{
                    //    lstObjDOAuthDetail.AddRange(lstObjDOAuthDetailResponse);
                    //}
                    objDOAuthRequestInput = JsonConvert.DeserializeObject<DOBHAuthInputRequest>(strSearch);
                    if (!string.IsNullOrEmpty(objDOAuthRequestInput.searchBeginDate) && !string.IsNullOrEmpty(objDOAuthRequestInput.searchEndDate))
                    {
                        objDOAuthRequestInput.searchBeginDate = Convert.ToDateTime(objDOAuthRequestInput.searchBeginDate).ToString("yyyy-MM-dd");
                        objDOAuthRequestInput.searchEndDate = Convert.ToDateTime(objDOAuthRequestInput.searchEndDate).ToString("yyyy-MM-dd");

                        _result = _objBOAuthSearch.AuthlinxAppealNotesSummary(objDOAuthRequestInput, out lstObjDOlinxAppealNotesResponse);


                    }

                }
                return Ok(lstObjDOlinxAppealNotesResponse);
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
        public IActionResult GetLinxAppealNotes(string strSearch)
        {
            source = BOCommon.GetRefererURI(Request);
            try
            {
                List<DOBHLinxNotesDetailInfo> lstObjDOAuthDetail = new List<DOBHLinxNotesDetailInfo>();
                List<DOBHLinxNotesDetailInfo> lstObjDOAuthDetailResponse = null;
                List<DOBHPLinxInfo> lstObjDOlinxAppealNotesResponse = null;

                DOBHAuthInputRequest objDOAuthRequestInput = null;

                _objBOAuthSearch = new BHLinxNotesMethods(_memoryCacheHelper, _objConfiguration, _httpContextAccessor, _objBOCommon);
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                if (!string.IsNullOrEmpty(strSearch.ToString()))
                {
                    objDOAuthRequestInput = JsonConvert.DeserializeObject<DOBHAuthInputRequest>(strSearch);
                    if (!string.IsNullOrEmpty(objDOAuthRequestInput.searchBeginDate) && !string.IsNullOrEmpty(objDOAuthRequestInput.searchEndDate))
                    {
                        objDOAuthRequestInput.searchBeginDate = Convert.ToDateTime(objDOAuthRequestInput.searchBeginDate).ToString("yyyy-MM-dd");
                        objDOAuthRequestInput.searchEndDate = Convert.ToDateTime(objDOAuthRequestInput.searchEndDate).ToString("yyyy-MM-dd");

                        _result = _objBOAuthSearch.linxAppealNotes(objDOAuthRequestInput, out lstObjDOlinxAppealNotesResponse);


                    }

                }
                return Ok(lstObjDOlinxAppealNotesResponse);
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
