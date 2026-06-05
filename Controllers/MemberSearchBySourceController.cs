using System;
using ANGDDEAPI.Common;
using ANGDDEAPIBO;
using ANGDDEAPIDO;
using ANGDDEAPIFoundation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ANGDDEAPIDO.Interface;
using System.Net;
using ANGDDEAPIBO.Interface;

namespace ANGDDEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class MemberSearchBySourceController : ControllerBase
    {
        public static long LoggedInUserId = 0;
        public string _username = string.Empty;
        string source = string.Empty;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DOConfiguration _objConfiguration;
        private readonly IMemoryCacheHelper _memoryCacheHelper;
        public BOGPSMemberDetails _objBOGPSMemberDetails = null;
        public readonly BOCommon _objBOCommon;
        private readonly ICacheService _cache;
        List<DOATSLookupMaster> lstATSLookupMaster = null;
        public ExceptionTypes exResult;
        public AccessAPI objAPICall = null;
        public MemberSearchBySourceController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
        {
            _cache = cache;
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _memoryCacheHelper = memoryCacheHelper;
            _objBOGPSMemberDetails = new BOGPSMemberDetails(objConfiguration, _cache);
            _objBOCommon = bOCommon;
            exResult = _objBOCommon.GetRRTGPSAPiConfig(out lstATSLookupMaster);
            objAPICall = new AccessAPI(_memoryCacheHelper, _objConfiguration);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="details"></param>
        /// <returns></returns>

        [HttpGet]
        public ActionResult GetMemberSearchDetailsFromGPS(string details = null)
        {
            DOMemberSearchCriteria memberDetails = null;
            List<DOGPSMemberDetails> memberSearchDetails = null;
            source = BOCommon.GetRefererURI(Request);
            try
            {
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                if (!string.IsNullOrEmpty(details))
                {
                    _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Request", details, _username);
                    memberDetails = new DOMemberSearchCriteria();
                    memberDetails = JsonConvert.DeserializeObject<DOMemberSearchCriteria>(details);
                    #region MemberEligibilty empty request handling on 02/15/2021 by Sunil/Naseer/Pallavi
                    bool isValidRequest = false;
                    if (!string.IsNullOrEmpty(memberDetails.MemberId)
                        || !string.IsNullOrEmpty(memberDetails.HICNumber) || !string.IsNullOrEmpty(memberDetails.FirstName)
                        || !string.IsNullOrEmpty(memberDetails.LastName) || !string.IsNullOrEmpty(memberDetails.PhoneNumber)
                        || !string.IsNullOrEmpty(memberDetails.State) || !string.IsNullOrEmpty(memberDetails.ZIP)
                        || !string.IsNullOrEmpty(memberDetails.MemberSuffix))
                    {
                        isValidRequest = true;
                    }
                    if (!isValidRequest)
                    {
                        return BadRequest(new { error = "bad request" });
                    }
                    #endregion

                    if (memberDetails != null)
                    {
                        LoggedInUserId = memberDetails.UserId;
                    }

                    string query = ConstantTexts.MemberSearchQuery;///oracle query string
                    query = GetMemberSearchQuery(query, memberDetails);
                    memberSearchDetails = new List<DOGPSMemberDetails>();
                    if (!string.IsNullOrWhiteSpace(query))
                    {
                        memberSearchDetails = BOGPSMemberDetails.GetMemberDetails(query, LoggedInUserId);

                    }
                }
                _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Response", BOCommon.JsonConvertObjectToString(memberSearchDetails), _username);

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
                //return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
                throw ex;
            }
        }

        [HttpGet]
        public ActionResult GetMemberSearchDetailsFromCSP(string details = null)
        {
            DOMemberSearchCriteria memberDetails = null;
            List<DOGPSMemberDetails> memberSearchDetails = null;
            source = BOCommon.GetRefererURI(Request);
            try
            {
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                if (!string.IsNullOrEmpty(details))
                {
                    _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Request", details, _username);
                    memberDetails = new DOMemberSearchCriteria();
                    memberDetails = JsonConvert.DeserializeObject<DOMemberSearchCriteria>(details);
                    #region MemberEligibilty empty request handling on 02/15/2021 by Sunil/Naseer/Pallavi
                    bool isValidRequest = false;
                    if (!string.IsNullOrEmpty(memberDetails.MemberId)
                        || !string.IsNullOrEmpty(memberDetails.HICNumber) || !string.IsNullOrEmpty(memberDetails.FirstName)
                        || !string.IsNullOrEmpty(memberDetails.LastName) || !string.IsNullOrEmpty(memberDetails.PhoneNumber)
                        || !string.IsNullOrEmpty(memberDetails.State) || !string.IsNullOrEmpty(memberDetails.ZIP)
                        || !string.IsNullOrEmpty(memberDetails.MemberSuffix)
                        || !string.IsNullOrEmpty(memberDetails.groupId)
                        || !string.IsNullOrEmpty(memberDetails.medicaidId))
                    {
                        isValidRequest = true;
                    }
                    if (!isValidRequest)
                    {
                        return BadRequest(new { error = "bad request" });
                    }
                    #endregion
                    if (memberDetails != null)
                    {
                        LoggedInUserId = memberDetails.UserId;
                    }
                    CSPEligibilityMethods objCSPEligibilityMethods = new CSPEligibilityMethods(_memoryCacheHelper, _objConfiguration, objAPICall);
                    DOCSPEligMemberDemographicsRequest objDOCSPEligMemberDemographicsRequest = new DOCSPEligMemberDemographicsRequest();
                    if (memberDetails != null && !string.IsNullOrEmpty(memberDetails.MemberId) || !(string.IsNullOrEmpty(memberDetails.medicaidId) && string.IsNullOrEmpty(memberDetails.groupId)))
                    {
                        objDOCSPEligMemberDemographicsRequest.subscriberId = memberDetails.MemberId;
                        objDOCSPEligMemberDemographicsRequest.groupId = memberDetails.groupId;
                        objDOCSPEligMemberDemographicsRequest.medicaidId = memberDetails.medicaidId;
                        if (!string.IsNullOrEmpty(memberDetails.MemberSuffix))
                        {
                            objDOCSPEligMemberDemographicsRequest.memberSuffix = memberDetails.MemberSuffix;
                        }
                        objCSPEligibilityMethods.CSPMemberDemgraphicsDetailsV3(objDOCSPEligMemberDemographicsRequest, out memberSearchDetails);
                    }

                }
                _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Response", BOCommon.JsonConvertObjectToString(memberSearchDetails), _username);

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
                //return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
                throw ex;
            }
        }

        public ActionResult GetMemberSearchDetailsFromHCP(string details = null)
        {
            DOMemberSearchCriteria memberDetails = null;
            List<DOGPSMemberDetails> memberSearchDetails = null;
            //source = BOCommon.GetRefererURI(Request);
            try
            {
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;

                _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Request", details, _username);

                if (!string.IsNullOrEmpty(details))
                {
                    memberDetails = new DOMemberSearchCriteria();
                    memberDetails = JsonConvert.DeserializeObject<DOMemberSearchCriteria>(details);
                    #region MemberEligibilty empty request handling on 02/15/2021 by Sunil/Naseer/Pallavi
                    bool isValidRequest = false;
                    if (!string.IsNullOrEmpty(memberDetails.MemberId)
                        || !string.IsNullOrEmpty(memberDetails.HICNumber) || !string.IsNullOrEmpty(memberDetails.FirstName)
                        || !string.IsNullOrEmpty(memberDetails.LastName) || !string.IsNullOrEmpty(memberDetails.PhoneNumber)
                        || !string.IsNullOrEmpty(memberDetails.State) || !string.IsNullOrEmpty(memberDetails.ZIP)
                        || !string.IsNullOrEmpty(memberDetails.MemberSuffix))
                    {
                        isValidRequest = true;
                    }
                    if (!isValidRequest)
                    {
                        return BadRequest(new { error = "bad request" });
                    }
                    #endregion
                    if (memberDetails != null)
                    {
                        LoggedInUserId = memberDetails.UserId;
                    }
                    memberSearchDetails = new List<DOGPSMemberDetails>();
                    //BOHCPMemberDetails objBOHCPMemberDetails = new BOHCPMemberDetails(_objConfiguration);
                    HCPMemberMethods objHCPMemberMethods = new(_memoryCacheHelper, _objConfiguration);
                    memberSearchDetails = objHCPMemberMethods.GetMemberDetails(memberDetails);
                    var isfilterenabled = _objBOCommon.GetLookupValueBasedonCon(lstATSLookupMaster, (long)RRTGPSAPIConfig.FilterOffshoreMember, 0);

                    if (memberSearchDetails != null && memberDetails?.IsOnshoreUser != true && !String.IsNullOrEmpty(isfilterenabled) && isfilterenabled == "1")
                    {
                        memberSearchDetails = memberSearchDetails?.Where(x => x.IsRestrictedMember == false).ToList();
                    }

                    if (memberSearchDetails == null || memberSearchDetails.Count == 0)
                    {
                        CSPEligibilityMethods objCSPEligibilityMethods = new CSPEligibilityMethods(_memoryCacheHelper, _objConfiguration, objAPICall);
                        DOCSPEligMemberDemographicsRequest objDOCSPEligMemberDemographicsRequest = new DOCSPEligMemberDemographicsRequest();
                        if (!string.IsNullOrEmpty(memberDetails.MemberId))
                        {
                            objDOCSPEligMemberDemographicsRequest.subscriberId = memberDetails.MemberId;
                            if (!string.IsNullOrEmpty(memberDetails.MemberSuffix))
                            {
                                objDOCSPEligMemberDemographicsRequest.memberSuffix = memberDetails.MemberSuffix;
                            }
                        }
                        objCSPEligibilityMethods.CSPMemberDemgraphicsDetailsV3(objDOCSPEligMemberDemographicsRequest, out memberSearchDetails);
                    }
                }
                _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Response", BOCommon.JsonConvertObjectToString(memberSearchDetails), _username);

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
                //return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
                throw ex;
            }
        }

        public ActionResult GetMemberSearchDetailsFromHCPCDB(string details = null)
        {
            DOMemberCDBSearchInput memberDetails = null;
            List<DOGPSMemberDetails> memberSearchDetails = null;
            source = BOCommon.GetRefererURI(Request);
            try
            {
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                //_httpContextAccessor.HttpContext.User.Identity.Name.Split(new string[] { @"\" }, StringSplitOptions.RemoveEmptyEntries);


                if (!string.IsNullOrEmpty(details))
                {
                    memberDetails = new DOMemberCDBSearchInput();
                    memberDetails = JsonConvert.DeserializeObject<DOMemberCDBSearchInput>(details);
                    #region MemberEligibilty empty request handling on 02/15/2021 by Sunil/Naseer/Pallavi
                    bool isValidRequest = false;
                    if (!string.IsNullOrEmpty(memberDetails.MemberId)
                        || !string.IsNullOrEmpty(memberDetails.FirstName)
                        || !string.IsNullOrEmpty(memberDetails.LastName))
                    {
                        isValidRequest = true;
                    }
                    if (!isValidRequest)
                    {
                        return BadRequest(new { error = "bad request" });
                    }
                    #endregion
                    if (memberDetails != null)
                    {
                        LoggedInUserId = memberDetails.UserId;
                    }
                    memberSearchDetails = new List<DOGPSMemberDetails>();
                    //BOHCPMemberDetails objBOHCPMemberDetails = new BOHCPMemberDetails(_objConfiguration);
                    HCPCDBMemberMethods objHCPMemberMethods = new HCPCDBMemberMethods(_memoryCacheHelper, _objConfiguration);
                    memberSearchDetails = objHCPMemberMethods.GetMemberDetails(memberDetails);

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
                return StatusCode((int)HttpStatusCode.InternalServerError, ex);
            }
        }

        private string GetMemberSearchQuery(string query, DOMemberSearchCriteria memberDetails)
        {
            StringBuilder memberSearchQueryBuilder = new StringBuilder();
            source = BOCommon.GetRefererURI(Request);
            try
            {
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                if (memberDetails != null)
                {
                    string memberId = memberDetails.MemberId;
                    if (!string.IsNullOrWhiteSpace(memberId))
                    {
                        string[] memberIDArr = new string[2];
                        if (memberId.Contains('-'))
                        {
                            memberIDArr = memberId.Split('-');
                            if (memberIDArr != null && memberIDArr.Length > 0)
                            {
                                memberId = memberIDArr[0];
                            }
                        }
                        memberDetails.MemberId = memberId;
                    }
                    memberSearchQueryBuilder.Append(query);
                    if (!string.IsNullOrEmpty(memberDetails.HICNumber))
                    {
                        memberSearchQueryBuilder.Append(" AND IND.MEDICARE_CLAIM_NUM ='" + memberDetails.HICNumber + "'");
                    }
                    else if ((!string.IsNullOrEmpty(memberDetails.FirstName) && (!string.IsNullOrEmpty(memberDetails.LastName))))
                    {
                        if (!string.IsNullOrEmpty(memberDetails.MemberId))
                        {
                            memberSearchQueryBuilder.Append(" AND HHP.MEMBERSHIP_NUMBER = '" + memberDetails.MemberId + "'");
                        }
                        else if (memberDetails.DateOfBirth != null)
                        {
                            string date = memberDetails.DateOfBirth.ToString("MM/dd/yyyy", null);
                            memberSearchQueryBuilder.Append(" AND to_char(DATE_OF_BIRTH,'MM/DD/YYYY')='" + date + "'");
                        }

                        if (memberDetails.LastName.EndsWith("*"))
                            memberSearchQueryBuilder.Append(" AND IND.LAST_NAME LIKE '" + memberDetails.LastName.ToUpper().Replace("*", "") + "%'");
                        else
                            memberSearchQueryBuilder.Append(" AND IND.LAST_NAME = '" + memberDetails.LastName.ToUpper() + "'");
                        if (memberDetails.FirstName.EndsWith("*"))
                            memberSearchQueryBuilder.Append(" AND IND.FIRST_NAME LIKE '" + memberDetails.FirstName.ToUpper().Replace("*", "") + "%'");
                        else
                            memberSearchQueryBuilder.Append(" AND IND.FIRST_NAME = '" + memberDetails.FirstName.ToUpper() + "'");
                    }
                    else if (!string.IsNullOrWhiteSpace(memberDetails.MemberId))
                    {
                        memberSearchQueryBuilder.Append(" AND HHP.MEMBERSHIP_NUMBER ='" + memberDetails.MemberId + "'");
                    }
                    else if ((!string.IsNullOrEmpty(memberDetails.State)) && (!string.IsNullOrEmpty(memberDetails.ZIP)))
                    {
                        memberSearchQueryBuilder.Append(" AND HAD1.state_cd='" + memberDetails.State + "'" + " And HAD1.zip_cd='" + memberDetails.ZIP + "'");
                    }
                    else if (!String.IsNullOrEmpty(memberDetails.PhoneNumber))
                    {
                        memberSearchQueryBuilder.Append(" AND HH.DAYTIME_PHONE_NUM LIKE '" + memberDetails.PhoneNumber + "%'");
                    }
                    else
                    {
                        memberSearchQueryBuilder = null;
                    }
                    var isfilterenabled = _objBOCommon.GetLookupValueBasedonCon(lstATSLookupMaster, (long)RRTGPSAPIConfig.FilterOffshoreMember, 0);
                    //if (memberDetails.IsOnshoreUser != true && _objConfiguration.AppSettings.FilterOffshoreMember.ToString() == "true")
                    if (memberDetails.IsOnshoreUser != true && !String.IsNullOrEmpty(isfilterenabled) && isfilterenabled == "1")
                    {
                        memberSearchQueryBuilder.Append("AND HH.ONSHORE_ONLY_IND='" + 'N' + "'");//NUll and 'Y' Considering for onshore in oracle db
                    }


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
            }
            return ((memberSearchQueryBuilder != null && !string.IsNullOrWhiteSpace(memberSearchQueryBuilder.ToString())) ? memberSearchQueryBuilder.ToString() : string.Empty);
            //return memberSearchQueryBuilder.ToString();
        }

        [HttpGet]
        public IActionResult GetMemberSearchDetailsFromHEMIRx(string details)
        {
            source = BOCommon.GetRefererURI(Request);
            DOHemiMemberSearchReuest objDOHemiMemberSearchReuest = null;
            DOMemberSearchCriteria memberDetails = null;
            List<DOGPSMemberDetails> memberSearchDetails = new List<DOGPSMemberDetails>();
            String Message = string.Empty;
            source = BOCommon.GetRefererURI(Request);
            try
            {
                if (details == null)
                {
                    return Ok("Not a valid request!");
                }
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Request", details, _username);

                objDOHemiMemberSearchReuest = new DOHemiMemberSearchReuest();
                memberDetails = new DOMemberSearchCriteria();
                memberDetails = JsonConvert.DeserializeObject<DOMemberSearchCriteria>(details);
                HemiMethods objHemiMethods = new HemiMethods(_memoryCacheHelper, _objConfiguration, _cache);
                SearchInputMetaData objSearchInputMetaData = new SearchInputMetaData();
                objSearchInputMetaData.applicationId = "RRT-ATS";
                objSearchInputMetaData.consumerAppType = "IA";
                objSearchInputMetaData.consumerType = "CA";
                objSearchInputMetaData.externalCorrelationId = "RRTATS-SAMPLE";



                objDOHemiMemberSearchReuest.searchInputMetaData = objSearchInputMetaData;
                objDOHemiMemberSearchReuest.id = memberDetails.MemberId;//ACUAZQ2TEST1
                objDOHemiMemberSearchReuest.idSearchOperator = memberDetails.idSearchOperator ?? null;

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
        public IActionResult GetRxMember(string details)
        {
            source = BOCommon.GetRefererURI(Request);
            DOHemiMemberSearchReuest objDOHemiMemberSearchReuest = null;
            DOMemberSearchCriteria memberDetails = null;
            List<DORxHemiMemberDetails> memberSearchDetails = new List<DORxHemiMemberDetails>();
            String Message = string.Empty;
            source = BOCommon.GetRefererURI(Request);
            try
            {
                if (details == null)
                {
                    return Ok("Not a valid request!");
                }
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Request", details, _username);

                objDOHemiMemberSearchReuest = new DOHemiMemberSearchReuest();
                memberDetails = new DOMemberSearchCriteria();
                memberDetails = JsonConvert.DeserializeObject<DOMemberSearchCriteria>(details);
                HemiMethods objHemiMethods = new HemiMethods(_memoryCacheHelper, _objConfiguration, _cache);
                SearchInputMetaData objSearchInputMetaData = new SearchInputMetaData();
                objSearchInputMetaData.applicationId = "RRT-ATS";
                objSearchInputMetaData.consumerAppType = "IA";
                objSearchInputMetaData.consumerType = "CA";
                objSearchInputMetaData.externalCorrelationId = "RRTATS-SAMPLE";


                objDOHemiMemberSearchReuest.searchInputMetaData = objSearchInputMetaData;
                objDOHemiMemberSearchReuest.id = memberDetails.MemberId;//ACUAZQ2TEST1
                objDOHemiMemberSearchReuest.includeExtendedData = true;

                _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Request collected all data", "", _username);

                objHemiMethods.GetHemiMemberDetailsforRx(objDOHemiMemberSearchReuest, out memberSearchDetails);
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

        /// <summary>
        /// For CSPMember Additional Details
        /// </summary>
        /// <param name="details"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetMemberAdditionalDetailsFromCSP(string details = null)
        {
            DOMemberSearchCriteria memberDetails = null;
            DOCSPMemberData objDOCSPMemberData = null;
            DOCSPMemberData objDOCSPMemberDataPcpNetwork = null;
            DOCSPMemberGroup objDOMemberGroup = null;
            DOCSPMemberReq request = new DOCSPMemberReq();
            DOMemberPCPNetworkReq objDOMemberPCPNetworkReq = new DOMemberPCPNetworkReq();
            DOMemberGroupReq objDOMemberGroupReq = new DOMemberGroupReq();
            source = BOCommon.GetRefererURI(Request);
            try
            {
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                if (!string.IsNullOrEmpty(details))
                {
                    _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Request", details, _username);
                    memberDetails = new DOMemberSearchCriteria();
                    memberDetails = JsonConvert.DeserializeObject<DOMemberSearchCriteria>(details);


                    if (string.IsNullOrEmpty(memberDetails.MemberId)
                        || string.IsNullOrEmpty(memberDetails.groupId))
                    {
                        return BadRequest(new { error = "bad request" });
                    }
                    if (memberDetails != null)
                    {
                        LoggedInUserId = memberDetails.UserId;
                    }
                    CSPEligibilityMethods objCSPEligibilityMethods = new CSPEligibilityMethods(_memoryCacheHelper, _objConfiguration, objAPICall);


                    if (memberDetails != null && !string.IsNullOrEmpty(memberDetails.MemberId) && !string.IsNullOrEmpty(memberDetails.groupId) && !string.IsNullOrEmpty(memberDetails.dos))
                    {
                        request.memberInquiry.subscriberId = memberDetails.MemberId;
                        request.memberInquiry.groupId = memberDetails.groupId;
                        objCSPEligibilityMethods.CSPGetMemberV6(request, out objDOCSPMemberData);

                        if (objDOCSPMemberData != null)
                        {
                            if (objDOCSPMemberData.data != null && objDOCSPMemberData.data.Count > 0)
                            {
                                if (objDOCSPMemberData.data[0].attributes != null)
                                {
                                    if (objDOCSPMemberData.data[0].attributes.subscriberClasses.Count > 0)
                                    {
                                        var subscriberClass = objDOCSPMemberData.data[0].attributes.subscriberClasses.Where(x => x.effDate.Value.Date == Convert.ToDateTime(memberDetails.dos).Date);
                                        if (subscriberClass != null && subscriberClass.Count() > 0)
                                        {
                                            memberDetails.classId = subscriberClass.FirstOrDefault().classId;
                                        }
                                        else
                                        {
                                            memberDetails.classId = objDOCSPMemberData.data[0].attributes.subscriberClasses.LastOrDefault().classId;
                                        }


                                    }
                                    if (objDOCSPMemberData.data[0].attributes.subGroups.Count > 0)
                                    {
                                        var subscriberGroup = objDOCSPMemberData.data[0].attributes.subGroups.Where(x => x.effDate.Value.Date == Convert.ToDateTime(memberDetails.dos).Date);

                                        if (!subscriberGroup.Any())
                                        {
                                            subscriberGroup = objDOCSPMemberData.data[0].attributes.subGroups.Where(x => x.termDate.Date > DateTime.Now.Date);
                                        }

                                        if (subscriberGroup != null && subscriberGroup.Count() > 0)
                                        {
                                            memberDetails.subgroupId = subscriberGroup.FirstOrDefault().subgroupId;
                                        }
                                        else
                                        {
                                            memberDetails.subgroupId = objDOCSPMemberData.data[0].attributes.subGroups.LastOrDefault().subgroupId;
                                        }

                                    }

                                }

                            }

                        }

                      //  if (string.IsNullOrEmpty(memberDetails.subgroupId))
                      //  {
                            objDOMemberPCPNetworkReq.subscriberId = memberDetails.MemberId;
                            objDOMemberPCPNetworkReq.dateOfService = memberDetails.dos;
                            objCSPEligibilityMethods.CSPGetMemberPCPNetworkV3(objDOMemberPCPNetworkReq, out objDOCSPMemberDataPcpNetwork);
                            if (objDOCSPMemberDataPcpNetwork != null)
                            {
                                if (objDOCSPMemberDataPcpNetwork.data != null && objDOCSPMemberDataPcpNetwork.data.Count > 0)
                                {
                                    if (objDOCSPMemberDataPcpNetwork.data[0].attributes != null)
                                    {
                                        if (objDOCSPMemberDataPcpNetwork.data[0].attributes.provRelationshipNetworks.Count > 0)
                                        {
                                            var provRelationshipPrefix = objDOCSPMemberDataPcpNetwork.data[0].attributes.provRelationshipNetworks.Where(x => x.effDate.Value.Date == Convert.ToDateTime(memberDetails.dos).Date);
                                            if (provRelationshipPrefix != null && provRelationshipPrefix.Count() > 0)
                                            {

                                                memberDetails.subgroupId = string.IsNullOrEmpty(memberDetails.subgroupId) ? provRelationshipPrefix.FirstOrDefault().provRelationshipPrefix: memberDetails.subgroupId;
                                            }
                                           
                                                memberDetails.provRelationshipPrefix = objDOCSPMemberDataPcpNetwork.data[0].attributes.provRelationshipNetworks.LastOrDefault().provRelationshipPrefix;
                                            

                                        }
                                    }
                                }
                            }
                     //   }


                        objDOMemberGroupReq.groupId = memberDetails.groupId;
                        objCSPEligibilityMethods.CSPGetMemberGroupV1(objDOMemberGroupReq, out objDOMemberGroup);

                        if (objDOMemberGroup != null)
                        {
                            if (objDOMemberGroup.data != null)
                            {
                                memberDetails.clientId = objDOMemberGroup.data?.attributes?.group?.clientId;


                            }

                        }
                    }

                }

                _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Response", BOCommon.JsonConvertObjectToString(memberDetails), _username);

                return Ok(memberDetails);
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
                //return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
                throw ex;
            }
        }

        /// <summary>
        /// This is to get member details from CSP member service  cel/shield/members/v7/search(v6 or v7). This is added to get HCPC details. if we need additional fields we can add here only
        /// </summary>
        /// <param name="details">https://localhost:44327/api/MemberSearchBySource/CSPMemberServiceDetails/?details=%7B%22LastName%22%3A%22%22%2C%22FirstName%22%3A%22%22%2C%22groupId%22%3A%22AZMCARE%22%2C%22MemberId%22%3A%22111399356%22%2C%22HICNumber%22%3A%22%22%2C%22ZIP%22%3A%22%22%2C%22PhoneNumber%22%3A%22%22%2C%22UserId%22%3A1%2C%22IndividualId%22%3Anull%2C%22MedicaidMemberId%22%3Anull%2C%22memberSuffix%22%3A%221%22%2C%22IsOnshoreUser%22%3Atrue%7D</param>
        /// <returns></returns>
        public ActionResult CSPMemberServiceDetails(string details = null)
        {
            DOMemberSearchCriteria memberDetails = null;
            DOCSPMemberData objDOCSPMemberData = null;
            DOCSPMemberReq request = new DOCSPMemberReq();
            List<MedicareInfo> LstMedicareInfo = new();
            source = BOCommon.GetRefererURI(Request);
            try
            {
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                if (!string.IsNullOrEmpty(details))
                {
                    _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Request", details, _username);
                    memberDetails = new DOMemberSearchCriteria();
                    memberDetails = JsonConvert.DeserializeObject<DOMemberSearchCriteria>(details);


                    if (string.IsNullOrEmpty(memberDetails.MemberId)
                        || string.IsNullOrEmpty(memberDetails.groupId))
                    {
                        return BadRequest(new { error = "bad request" });
                    }
                    if (memberDetails != null)
                    {
                        LoggedInUserId = memberDetails.UserId;
                    }
                    CSPEligibilityMethods objCSPEligibilityMethods = new CSPEligibilityMethods(_memoryCacheHelper, _objConfiguration, objAPICall);
                    if (memberDetails != null && !string.IsNullOrEmpty(memberDetails.MemberId) && !string.IsNullOrEmpty(memberDetails.groupId))
                    {
                        request.memberInquiry.subscriberId = memberDetails.MemberId;//AZMCARE
                        request.memberInquiry.groupId = memberDetails.groupId;//111399356
                        request.memberInquiry.memberSuffix = memberDetails.MemberSuffix;//1
                        if (request.memberInquiry.groupId != null && request.memberInquiry.groupId.ToLower().EndsWith("ex"))
                        {
                            request.requestType = "Exchange";
                        }
                        else
                        {
                            request.requestType = "Medicaid";
                        }
                        objCSPEligibilityMethods.CSPGetMemberV6(request, out objDOCSPMemberData);

                        if (objDOCSPMemberData != null)
                        {
                            if (objDOCSPMemberData.data != null && objDOCSPMemberData.data.Count > 0 && objDOCSPMemberData.data[0] != null && objDOCSPMemberData.data[0].attributes?.member?.MedicareDetails?.Count > 0)
                            {
                                if ((bool)objDOCSPMemberData.data[0].attributes?.member?.MedicareDetails.Any(x => x.MedicareInfos.Count > 0 && x.MedicareInfos.Any(s => s.EventCode == "HSPC")))
                                {
                                    LstMedicareInfo = objDOCSPMemberData.data[0].attributes?.member?.MedicareDetails[0]?.MedicareInfos?.Where(x => x.EventCode == "HSPC").ToList();
                                }
                            }
                        }
                    }
                }
                _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Response", BOCommon.JsonConvertObjectToString(memberDetails), _username);

                return Ok(LstMedicareInfo);
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
                //return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
                throw ex;
            }
        }

        /// <summary>
        /// This is to get member Billing summary details from CSP billing shield API  cel/shield/billings/v3/search. 
        /// </summary>
        /// <param name="details"></param>
        /// <returns></returns>
        public ActionResult CSPMemberBillingDetails(string details = null)
        {
            DOMemberSearchCriteria memberDetails = null;
            DOCSPMemberBillingSummary objDOCSPMemberBillingSummary = null;
            CSPMemberBilling request = new CSPMemberBilling();
            source = BOCommon.GetRefererURI(Request);
            BillInvoice objnetduewamount = null;
            try
            {
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                if (!string.IsNullOrEmpty(details))
                {
                    _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Request", details, _username);
                    memberDetails = new DOMemberSearchCriteria();
                    memberDetails = JsonConvert.DeserializeObject<DOMemberSearchCriteria>(details);


                    if (string.IsNullOrEmpty(memberDetails.MemberId)
                        || string.IsNullOrEmpty(memberDetails.groupId))
                    {
                        return BadRequest(new { error = "bad request" });
                    }
                    if (memberDetails != null)
                    {
                        LoggedInUserId = memberDetails.UserId;
                    }
                    CSPEligibilityMethods objCSPEligibilityMethods = new CSPEligibilityMethods(_memoryCacheHelper, _objConfiguration, objAPICall);
                    if (memberDetails != null && !string.IsNullOrEmpty(memberDetails.MemberId))
                    {
                        request.subscriberId = memberDetails.MemberId;//120951360
                        request.startDate = memberDetails.EligibilityFrom;//2024-01-01
                        request.endDate = memberDetails.EligibilityTo;//2024-12-01

                        objCSPEligibilityMethods.GetMemberBilling(request, out objDOCSPMemberBillingSummary);

                        if (objDOCSPMemberBillingSummary is not null && objDOCSPMemberBillingSummary.Data is not null && objDOCSPMemberBillingSummary.Data.Count > 0)
                        {
                            var billingInvoice = objDOCSPMemberBillingSummary.Data.OrderByDescending(x => x.Attributes?.BillInvoice?.InvoiceCreateDate).FirstOrDefault();
                            if (billingInvoice != null)
                            {
                                objnetduewamount = billingInvoice.Attributes?.BillInvoice;
                            }
                        }
                    }
                }
                _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Response", BOCommon.JsonConvertObjectToString(memberDetails), _username);

                return Ok(objnetduewamount);
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
                //return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
                throw ex;
            }
        }

        /// <summary>
        /// This is to get member ReceiptHistory details from CSP Receipt shield API  shield/receipts/v1.0/search. 
        /// </summary>
        /// <param name="details"></param>
        /// <returns></returns>
        public ActionResult CSPMemberReceiptHistory(string details = null)
        {
            DOMemberSearchCriteria memberDetails = null;
            DOCSPMemberReceiptHistoryResp objDOCSPMemberReceiptHistoryResp = null;
            CSPMemberBilling request = new CSPMemberBilling();
            source = BOCommon.GetRefererURI(Request);
            Receipt receiptHistory = null;
            try
            {
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                if (!string.IsNullOrEmpty(details))
                {
                    _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Request", details, _username);
                    memberDetails = new DOMemberSearchCriteria();
                    memberDetails = JsonConvert.DeserializeObject<DOMemberSearchCriteria>(details);


                    if (string.IsNullOrEmpty(memberDetails.MemberId)
                        || string.IsNullOrEmpty(memberDetails.groupId))
                    {
                        return BadRequest(new { error = "bad request" });
                    }
                    if (memberDetails != null)
                    {
                        LoggedInUserId = memberDetails.UserId;
                    }
                    CSPEligibilityMethods objCSPEligibilityMethods = new CSPEligibilityMethods(_memoryCacheHelper, _objConfiguration, objAPICall);
                    if (memberDetails != null && !string.IsNullOrEmpty(memberDetails.MemberId))
                    {
                        request.subscriberId = memberDetails.MemberId;//120951360
                        request.startDate = memberDetails.EligibilityFrom;//2024-01-01
                        request.endDate = memberDetails.EligibilityTo;//2024-12-01
                        request.consumerData = new ConsumerDataRequest();
                        request.consumerData.clientCode = "ATS";
                        request.consumerData.instance = "CSP";

                        objCSPEligibilityMethods.GetMemberReceiptHistory(request, out objDOCSPMemberReceiptHistoryResp);

                        if (objDOCSPMemberReceiptHistoryResp is not null && objDOCSPMemberReceiptHistoryResp.receipts is not null && objDOCSPMemberReceiptHistoryResp.receipts.Count > 0)
                        {
                            receiptHistory = objDOCSPMemberReceiptHistoryResp.receipts.OrderByDescending(x => Convert.ToDateTime(x.receivedDate)).FirstOrDefault();
                        }
                    }
                }
                _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Response", BOCommon.JsonConvertObjectToString(memberDetails), _username);

                return Ok(receiptHistory);
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
                return BadRequest(ex.Message);
            }
        }
    }
}