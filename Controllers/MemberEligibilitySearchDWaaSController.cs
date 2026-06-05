using ANGDDEAPI.Common;
using ANGDDEAPIBO;
using ANGDDEAPIBO.Interface;
using ANGDDEAPIDO;
using ANGDDEAPIDO.Interface;
using ANGDDEAPIFoundation;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ANGDDEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class MemberEligibilitySearchDWaaSController : ControllerBase
    {
        public static long LoggedInUserId = 0;
        public string _username = string.Empty;
        string source = string.Empty;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DOConfiguration _objConfiguration;
        private readonly IMemoryCacheHelper _memoryCacheHelper;
        public BOGPSMemberDetails _objBOGPSMemberDetails = null;
        public readonly BOCommon _objBOCommon = null;
        List<DOATSLookupMaster> lstATSLookupMaster = null;
        public ExceptionTypes exResult;
        private readonly ICacheService _cache;
        public AccessAPI objAPICall = null;

        public MemberEligibilitySearchDWaaSController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
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
        public ActionResult GetMemberSearchDetails(string details = null)
        {
            DOMemberSearchCriteria memberDetails = null;
            List<DOGPSMemberDetails> memberSearchDetails = null;
            source = BOCommon.GetRefererURI(Request);
            try
            {
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;


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

                    string query = ConstantTexts.MemberSearchDWaaSQuery;///snowsql query string
                    query = GetMemberSearchQuery(query, memberDetails);
                    memberSearchDetails = new List<DOGPSMemberDetails>();
                    if (!string.IsNullOrWhiteSpace(query))
                    {
                        memberSearchDetails = BOGPSMemberDetails.GetDWaaSMemberDetails(query, LoggedInUserId);
                        if (memberSearchDetails == null || memberSearchDetails.Count == 0)
                        {
                            CSPEligibilityMethods objCSPEligibilityMethods = new CSPEligibilityMethods(_memoryCacheHelper, _objConfiguration, objAPICall);
                            DOCSPEligMemberDemographicsRequest objDOCSPEligMemberDemographicsRequest = new DOCSPEligMemberDemographicsRequest();
                            if (!string.IsNullOrEmpty(memberDetails.MemberId))
                            {
                                objDOCSPEligMemberDemographicsRequest.subscriberId = memberDetails.MemberId;
                            }
                            else if (!string.IsNullOrEmpty(memberDetails.HICNumber))
                            {
                                objDOCSPEligMemberDemographicsRequest.medicareId = memberDetails.HICNumber;
                            }

                            objCSPEligibilityMethods.CSPMemberDemgraphicsDetails(objDOCSPEligMemberDemographicsRequest, out memberSearchDetails);
                        }
                    }
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

        [HttpGet]
        public ActionResult GetMemberSearchDetailsFromGPSnCSP(string details = null)
        {
            DOMemberSearchCriteria memberDetails = null;
            List<DOGPSMemberDetails> memberSearchDetails = null;
            source = BOCommon.GetRefererURI(Request);
            try
            {
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
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

                    string query = ConstantTexts.MemberSearchDWaaSQuery;///snowsql query string
                    query = GetMemberSearchQuery(query, memberDetails);
                    memberSearchDetails = new List<DOGPSMemberDetails>();
                    if (!string.IsNullOrWhiteSpace(query))
                    {
                        memberSearchDetails = BOGPSMemberDetails.GetDWaaSMemberDetails(query, LoggedInUserId);
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

        [HttpGet]
        public ActionResult GetMemberSearchDetailsForIEX(string details = null)
        {
            DOMemberSearchCriteria memberDetails = null;
            List<DOGPSMemberDetails> memberSearchDetails = null;
            source = BOCommon.GetRefererURI(Request);
            try
            {
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
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
                    CSPEligibilityMethods objCSPEligibilityMethods = new CSPEligibilityMethods(_memoryCacheHelper, _objConfiguration, objAPICall);
                    DOCSPEligMemberDemographicsRequest objDOCSPEligMemberDemographicsRequest = new DOCSPEligMemberDemographicsRequest();
                    if (memberDetails != null && !string.IsNullOrEmpty(memberDetails.MemberId))
                    {
                        objDOCSPEligMemberDemographicsRequest.subscriberId = memberDetails.MemberId;
                        if (!string.IsNullOrEmpty(memberDetails.MemberSuffix))
                        {
                            objDOCSPEligMemberDemographicsRequest.memberSuffix = memberDetails.MemberSuffix;
                        }
                        objCSPEligibilityMethods.CSPMemberDemgraphicsDetailsV3(objDOCSPEligMemberDemographicsRequest, out memberSearchDetails);
                    }
                    if (memberSearchDetails == null || memberSearchDetails.Count == 0)
                    {
                        string query = ConstantTexts.MemberSearchDWaaSQuery;///snowsql query string
                        query = GetMemberSearchQuery(query, memberDetails);
                        memberSearchDetails = new List<DOGPSMemberDetails>();
                        if (!string.IsNullOrWhiteSpace(query))
                        {
                            memberSearchDetails = BOGPSMemberDetails.GetDWaaSMemberDetails(query, LoggedInUserId);
                        }
                    }
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



        [HttpGet]
        public ActionResult GetMemberSearchDetailsFromGPSnCSP_Restriction(string details = null)
        {
            //BOCommon.Trace("Entry-MembersearchController-GetMemberSearchDetailsFromGPSnCSP_Restriction", "",details,"4");
            DOMemberSearchCriteria memberDetails = null;
            List<DOGPSMemberDetails> memberSearchDetails = null;
            source = BOCommon.GetRefererURI(Request);
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

                    string query = ConstantTexts.MemberSearchDWaaSQuery;///snowsql query string
                    //BOCommon.Trace("Line 302 - MembersearchController-GetMemberSearchDetailsFromGPSnCSP_Restriction", "", details, "4");
                    query = GetMemberSearchQuery_Restriction(query, memberDetails);
                    memberSearchDetails = new List<DOGPSMemberDetails>();
                    if (!string.IsNullOrWhiteSpace(query))
                    {
                        memberSearchDetails = BOGPSMemberDetails.GetDWaaSMemberDetails(query, LoggedInUserId);
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
                }
                _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Response", BOCommon.JsonConvertObjectToString(memberSearchDetails), _username);
                return Ok(memberSearchDetails);
            }
            catch (Exception ex)
            {
                _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), "DWaaS Controller Exception", ex.StackTrace, "4");
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



        [HttpGet]
        public ActionResult GetMemberSearchDetailsForIEX_Restriction(string details = null)
        {

            //BOCommon.Trace("Entry- MembersearchController-GetMemberSearchDetailsForIEX_Restriction", "", details, "4");
            DOMemberSearchCriteria memberDetails = null;
            List<DOGPSMemberDetails> memberSearchDetails = null;
            source = BOCommon.GetRefererURI(Request);
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
                    CSPEligibilityMethods objCSPEligibilityMethods = new CSPEligibilityMethods(_memoryCacheHelper, _objConfiguration, objAPICall);
                    DOCSPEligMemberDemographicsRequest objDOCSPEligMemberDemographicsRequest = new DOCSPEligMemberDemographicsRequest();
                    if (memberDetails != null && !string.IsNullOrEmpty(memberDetails.MemberId))
                    {
                        objDOCSPEligMemberDemographicsRequest.subscriberId = memberDetails.MemberId;
                        if (!string.IsNullOrEmpty(memberDetails.MemberSuffix))
                        {
                            objDOCSPEligMemberDemographicsRequest.memberSuffix = memberDetails.MemberSuffix;
                        }
                        objCSPEligibilityMethods.CSPMemberDemgraphicsDetailsV3(objDOCSPEligMemberDemographicsRequest, out memberSearchDetails);
                    }
                    if (memberSearchDetails == null || memberSearchDetails.Count == 0)
                    {
                        string query = ConstantTexts.MemberSearchDWaaSQuery;///snowsql query string
                        query = GetMemberSearchQuery_Restriction(query, memberDetails);
                        memberSearchDetails = new List<DOGPSMemberDetails>();
                        if (!string.IsNullOrWhiteSpace(query))
                        {
                            memberSearchDetails = BOGPSMemberDetails.GetDWaaSMemberDetails(query, LoggedInUserId);
                        }
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

        private string GetMemberSearchQuery_Restriction(string query, DOMemberSearchCriteria memberDetails)
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
        /// <summary>
        /// Get Plan Details based on Individual ID
        /// </summary>
        /// <param name="details"></param>
        /// <returns></returns>

        [HttpGet]
        public ActionResult GetPlanDetails(string details = null)
        {
            DOMemberSearchCriteria planDetails = null;
            List<DOMemberPlanDetails> planSearchDetails = null;
            source = BOCommon.GetRefererURI(Request);
            try
            {
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Request", details, _username);

                if (!string.IsNullOrEmpty(details))
                {

                    planDetails = new DOMemberSearchCriteria();
                    planDetails = JsonConvert.DeserializeObject<DOMemberSearchCriteria>(details);
                    if (planDetails != null)
                    {
                        LoggedInUserId = planDetails.UserId;
                    }

                    if (planDetails.MemberRecordsource != "CSP FACETS")
                    {
                        string PCPQuery = string.Empty;
                        string query = GetPlanSearchQuery(planDetails);
                        planSearchDetails = new List<DOMemberPlanDetails>();
                        if (!string.IsNullOrWhiteSpace(query))
                        {
                            planSearchDetails = BOGPSMemberDetails.GetDWaaSPlanDetails(query, PCPQuery, LoggedInUserId);
                        }
                    }
                    else
                    {
                        CSPEligibilityMethods objCSPEligibilityMethods = new CSPEligibilityMethods(_memoryCacheHelper, _objConfiguration, objAPICall);
                        DOCSPEligMemberCoverageRequest objDOCSPEligMemberCoverageRequest = new DOCSPEligMemberCoverageRequest();
                        DOCSPEligMemberPCPRequest objDOCSPEligMemberPCPRequest = new DOCSPEligMemberPCPRequest();
                        List<DOMemberPlanDetails> pcpPlanSearchDetails = null;
                        if (!string.IsNullOrEmpty(planDetails.MemberId))
                        {
                            objDOCSPEligMemberCoverageRequest.subscriberId = planDetails.MemberId;
                            objDOCSPEligMemberCoverageRequest.dateOfServiceFrom = "2016-01-01";
                            objDOCSPEligMemberCoverageRequest.dateOfServiceTo = DateTime.Today.ToString("yyyy-MM-dd");
                            objDOCSPEligMemberPCPRequest.subscriberId = planDetails.MemberId;
                        }

                        objCSPEligibilityMethods.CSPMemberCoverageDetails(objDOCSPEligMemberCoverageRequest, out planSearchDetails);
                        for (int i = 0; i < planSearchDetails.Count; i++)
                        {
                            //objDOCSPEligMemberPCPRequest.dateOfService = Convert.ToDateTime(planSearchDetails[i].EligibilityFrom).ToString("yyyy-MM-dd");
                            objDOCSPEligMemberPCPRequest.dateOfServiceFrom = Convert.ToDateTime(planSearchDetails[i].EligibilityFrom).ToString("yyyy-MM-dd");
                            objDOCSPEligMemberPCPRequest.dateOfServiceTo = Convert.ToDateTime(planSearchDetails[i].EligibilityTo).ToString("yyyy-MM-dd");
                            objCSPEligibilityMethods.CSPMemberPCPDetails(objDOCSPEligMemberPCPRequest, out pcpPlanSearchDetails);

                            if (pcpPlanSearchDetails != null && pcpPlanSearchDetails.Count > 0)
                            {
                                planSearchDetails[i].PCPNumber = pcpPlanSearchDetails[0].PCPNumber;
                                planSearchDetails[i].PCPName = pcpPlanSearchDetails[0].PCPName;
                                planSearchDetails[i].PCPEffectiveDateFrom = pcpPlanSearchDetails[0].PCPEffectiveDateFrom;
                                planSearchDetails[i].PCPEffectiveDateTo = pcpPlanSearchDetails[0].PCPEffectiveDateTo;
                            }
                        }

                        DOCSPEligMemberDemographicsRequest objDOCSPEligMemberDemographicsRequest = new DOCSPEligMemberDemographicsRequest();
                        if (!string.IsNullOrEmpty(planDetails.MemberId))
                        {
                            objDOCSPEligMemberDemographicsRequest.subscriberId = planDetails.MemberId;
                        }
                        List<DOGPSMemberDetails> lstDOGPSMemberDetails = null;
                        objCSPEligibilityMethods.CSPMemberDemgraphicsDetails(objDOCSPEligMemberDemographicsRequest, out lstDOGPSMemberDetails);
                        if (lstDOGPSMemberDetails != null && lstDOGPSMemberDetails.Count > 0)
                        {
                            for (int i = 0; i < planSearchDetails.Count; i++)
                            {
                                planSearchDetails[i].StateAssignedID = lstDOGPSMemberDetails[0].MedicaidMemberId;
                                planSearchDetails[i].EmployerGroupNumber = lstDOGPSMemberDetails[0].EmployerGroupNumber;
                                planSearchDetails[i].GroupName = lstDOGPSMemberDetails[0].EmployerGroupName;


                            }
                        }
                    }
                }
                return Ok(planSearchDetails);
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


        [HttpGet]
        public ActionResult GetPlanDetailsFromGPSnCSP(string details = null)
        {
            DOMemberSearchCriteria planDetails = null;
            List<DOMemberPlanDetails> planSearchDetails = null;
            source = BOCommon.GetRefererURI(Request);
            try
            {
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                if (!string.IsNullOrEmpty(details))
                {
                    _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Request", details, _username);

                    planDetails = new DOMemberSearchCriteria();
                    planDetails = JsonConvert.DeserializeObject<DOMemberSearchCriteria>(details);
                    if (planDetails != null)
                    {
                        LoggedInUserId = planDetails.UserId;
                    }

                    if (planDetails.MemberRecordsource != "CSP FACETS")
                    {
                        string PCPQuery = string.Empty;
                        string query = GetPlanSearchQuery(planDetails);
                        planSearchDetails = new List<DOMemberPlanDetails>();
                        if (!string.IsNullOrWhiteSpace(query))
                        {
                            planSearchDetails = BOGPSMemberDetails.GetDWaaSPlanDetails(query, PCPQuery, LoggedInUserId);
                        }
                    }
                    else
                    {
                        CSPEligibilityMethods objCSPEligibilityMethods = new CSPEligibilityMethods(_memoryCacheHelper, _objConfiguration, objAPICall);
                        DOCSPEligMemberCoverageRequest objDOCSPEligMemberCoverageRequest = new DOCSPEligMemberCoverageRequest();
                        DOCSPEligMemberPCPRequest objDOCSPEligMemberPCPRequest = new DOCSPEligMemberPCPRequest();
                        List<DOMemberPlanDetails> pcpPlanSearchDetails = null;
                        if (!string.IsNullOrEmpty(planDetails.MemberId))
                        {
                            objDOCSPEligMemberCoverageRequest.subscriberId = planDetails.MemberId;
                            objDOCSPEligMemberCoverageRequest.dateOfServiceFrom = "2016-01-01";
                            objDOCSPEligMemberCoverageRequest.dateOfServiceTo = DateTime.Today.ToString("yyyy-MM-dd");
                            objDOCSPEligMemberPCPRequest.subscriberId = planDetails.MemberId;

                            if (!string.IsNullOrEmpty(planDetails.MemberSuffix))
                            {
                                objDOCSPEligMemberCoverageRequest.memberSuffix = planDetails.MemberSuffix;
                                objDOCSPEligMemberPCPRequest.memberSuffix = planDetails.MemberSuffix;
                            }
                        }

                        objCSPEligibilityMethods.CSPMemberCoverageDetails(objDOCSPEligMemberCoverageRequest, out planSearchDetails);
                        for (int i = 0; i < planSearchDetails.Count; i++)
                        {
                            //objDOCSPEligMemberPCPRequest.dateOfService = Convert.ToDateTime(planSearchDetails[i].EligibilityFrom).ToString("yyyy-MM-dd");
                            objDOCSPEligMemberPCPRequest.dateOfServiceFrom = Convert.ToDateTime(planSearchDetails[i].EligibilityFrom).ToString("yyyy-MM-dd");
                            objDOCSPEligMemberPCPRequest.dateOfServiceTo = Convert.ToDateTime(planSearchDetails[i].EligibilityTo).ToString("yyyy-MM-dd");
                            objCSPEligibilityMethods.CSPMemberPCPDetails(objDOCSPEligMemberPCPRequest, out pcpPlanSearchDetails);

                            if (pcpPlanSearchDetails != null && pcpPlanSearchDetails.Count > 0)
                            {
                                planSearchDetails[i].PCPNumber = pcpPlanSearchDetails[0].PCPNumber;
                                planSearchDetails[i].PCPName = pcpPlanSearchDetails[0].PCPName;
                                planSearchDetails[i].PCPEffectiveDateFrom = pcpPlanSearchDetails[0].PCPEffectiveDateFrom;
                                planSearchDetails[i].PCPEffectiveDateTo = pcpPlanSearchDetails[0].PCPEffectiveDateTo;
                            }
                        }

                        DOCSPEligMemberDemographicsRequest objDOCSPEligMemberDemographicsRequest = new DOCSPEligMemberDemographicsRequest();
                        if (!string.IsNullOrEmpty(planDetails.MemberId))
                        {
                            objDOCSPEligMemberDemographicsRequest.subscriberId = planDetails.MemberId;
                            if (!string.IsNullOrEmpty(planDetails.MemberSuffix))
                            {
                                objDOCSPEligMemberDemographicsRequest.memberSuffix = planDetails.MemberSuffix;
                            }
                        }
                        List<DOGPSMemberDetails> lstDOGPSMemberDetails = null;
                        objCSPEligibilityMethods.CSPMemberDemgraphicsDetailsV3(objDOCSPEligMemberDemographicsRequest, out lstDOGPSMemberDetails);
                        if (lstDOGPSMemberDetails != null && lstDOGPSMemberDetails.Count > 0)
                        {
                            for (int i = 0; i < planSearchDetails.Count; i++)
                            {
                                planSearchDetails[i].StateAssignedID = lstDOGPSMemberDetails[0].MedicaidMemberId;
                                planSearchDetails[i].EmployerGroupNumber = lstDOGPSMemberDetails[0].EmployerGroupNumber;
                                planSearchDetails[i].GroupName = lstDOGPSMemberDetails[0].EmployerGroupName;
                            }
                        }
                    }
                }
                _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Response", BOCommon.JsonConvertObjectToString(planSearchDetails), _username);

                return Ok(planSearchDetails);
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

        /// <summary>
        /// Build dynamic query for plan search
        /// </summary>
        /// <param name="planDetails"></param>
        /// <returns></returns>
        private string GetPlanSearchQuery(DOMemberSearchCriteria planDetails)
        {
            StringBuilder planSearchQueryBuilder = new StringBuilder();
            try
            {
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                if (planDetails != null)
                {
                    bool isSearchCriteriaSet = false;
                    var LISSwitchToNew = _objBOCommon.GetLookupValueBasedonCon(lstATSLookupMaster, (long)RRTGPSAPIConfig.LISSwitchToNew, 0);

                    //if (_objConfiguration.AppSettings.LISSwitchToNew!= null && _objConfiguration.AppSettings.LISSwitchToNew.ToString() == "true")

                    if (!string.IsNullOrEmpty(LISSwitchToNew!) && LISSwitchToNew == "1")

                    {
                        planSearchQueryBuilder.Append(ConstantTexts.GPSPlanSelectDWaaSScript1LIS);///snowsql query string
                    }
                    else
                    {
                        planSearchQueryBuilder.Append(ConstantTexts.GPSPlanSelectDWaaSScript1);///snowsql query string

                    }
                    if (!string.IsNullOrEmpty(planDetails.HICNumber) && planDetails.HICNumber != ConstantTexts.ConstantZero)
                    {
                        planSearchQueryBuilder.Append(" IND.MEDICARE_CLAIM_NUM ='" + planDetails.HICNumber + "'");
                        isSearchCriteriaSet = true;
                    }
                    if (!string.IsNullOrEmpty(planDetails.IndividualId) && planDetails.IndividualId != ConstantTexts.ConstantZero)
                    {
                        if (isSearchCriteriaSet)
                        {
                            planSearchQueryBuilder.Append(" AND");
                        }
                        planSearchQueryBuilder.Append(" IND.INDIVIDUAL_ID=" + planDetails.IndividualId);
                        isSearchCriteriaSet = true;
                    }
                    if (!string.IsNullOrEmpty(planDetails.MemberId) && planDetails.MemberId != ConstantTexts.ConstantZero)
                    {
                        string memberId = planDetails.MemberId;
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
                            planDetails.MemberId = memberId;
                        }
                        if (isSearchCriteriaSet)
                        {
                            planSearchQueryBuilder.Append(" AND");
                        }
                        planSearchQueryBuilder.Append(" HHP.MEMBERSHIP_NUMBER ='" + planDetails.MemberId + "'");
                        isSearchCriteriaSet = true;
                    }
                    if (!isSearchCriteriaSet)
                    {
                        planSearchQueryBuilder = null;
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
            return planSearchQueryBuilder != null && !string.IsNullOrWhiteSpace(planSearchQueryBuilder.ToString()) ? planSearchQueryBuilder.ToString() : string.Empty;
        }


        [HttpGet]
        public ActionResult GetMemberSearchByID(string details = null)
        {
            DOMemberSearchCriteria memberDetails = null;
            List<DOGPSMemberDetails> memberSearchDetails = null;
            source = BOCommon.GetRefererURI(Request);
            try
            {
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                if (!string.IsNullOrEmpty(details))
                {
                    memberDetails = new DOMemberSearchCriteria();
                    memberDetails = JsonConvert.DeserializeObject<DOMemberSearchCriteria>(details);
                    if (memberDetails != null)
                    {
                        LoggedInUserId = memberDetails.UserId;
                    }
                    if (memberDetails.MemberRecordsource != "CSP FACETS")
                    {
                        string query = ConstantTexts.GPSMemberSelectDWaaSScript;///snowsql query string
                        query = GetMemberSearchByID(query, memberDetails);
                        memberSearchDetails = new List<DOGPSMemberDetails>();
                        if (!string.IsNullOrWhiteSpace(query))
                        {
                            memberSearchDetails = BOGPSMemberDetails.GetDWaaSMemberDetails(query, LoggedInUserId);
                        }
                    }
                    else
                    {
                        CSPEligibilityMethods objCSPEligibilityMethods = new CSPEligibilityMethods(_memoryCacheHelper, _objConfiguration, objAPICall);
                        DOCSPEligMemberDemographicsRequest objDOCSPEligMemberDemographicsRequest = new DOCSPEligMemberDemographicsRequest();
                        if (!string.IsNullOrEmpty(memberDetails.MemberId))
                        {
                            objDOCSPEligMemberDemographicsRequest.subscriberId = memberDetails.MemberId;
                        }
                        else if (!string.IsNullOrEmpty(memberDetails.HICNumber))
                        {
                            objDOCSPEligMemberDemographicsRequest.medicareId = memberDetails.HICNumber;
                        }

                        objCSPEligibilityMethods.CSPMemberDemgraphicsDetails(objDOCSPEligMemberDemographicsRequest, out memberSearchDetails);
                    }
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


        [HttpGet]
        public ActionResult GetMemberSearchByIDFromGPSnCSP(string details = null)
        {
            DOMemberSearchCriteria memberDetails = null;
            List<DOGPSMemberDetails> memberSearchDetails = null;
            source = BOCommon.GetRefererURI(Request);
            try
            {
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Request", details, _username);

                if (!string.IsNullOrEmpty(details))
                {
                    memberDetails = new DOMemberSearchCriteria();
                    memberDetails = JsonConvert.DeserializeObject<DOMemberSearchCriteria>(details);
                    if (memberDetails != null)
                    {
                        LoggedInUserId = memberDetails.UserId;
                    }
                    if (memberDetails.MemberRecordsource != "CSP FACETS")
                    {
                        string query = ConstantTexts.GPSMemberSelectDWaaSScript;///snowsql query string
                        query = GetMemberSearchByID(query, memberDetails);
                        memberSearchDetails = new List<DOGPSMemberDetails>();
                        if (!string.IsNullOrWhiteSpace(query))
                        {
                            memberSearchDetails = BOGPSMemberDetails.GetDWaaSMemberDetails(query, LoggedInUserId);
                        }
                    }
                    else
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
                return StatusCode((int)HttpStatusCode.InternalServerError, ex);
            }
        }

        /// <summary>
        /// Build dynamic member search detailed query
        /// </summary>
        /// <param name="query"></param>
        /// <param name="memberDetails"></param>
        /// <returns></returns>
        private string GetMemberSearchByID(string query, DOMemberSearchCriteria memberDetails)
        {
            StringBuilder memberSearchQueryBuilder = new StringBuilder();
            try
            {
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                if (memberDetails != null)
                {
                    memberSearchQueryBuilder.Append(query);
                    memberSearchQueryBuilder.Append("WHERE 1=1");
                    if (!string.IsNullOrEmpty(memberDetails.HICNumber) && memberDetails.HICNumber != ConstantTexts.ConstantZero)
                    {
                        memberSearchQueryBuilder.Append(" AND IND.MEDICARE_CLAIM_NUM ='" + memberDetails.HICNumber + "'");
                    }
                    if (!string.IsNullOrEmpty(memberDetails.IndividualId) && memberDetails.IndividualId != ConstantTexts.ConstantZero)
                    {
                        memberSearchQueryBuilder.Append(" AND IND.INDIVIDUAL_ID='" + memberDetails.IndividualId + "'");
                    }
                    if (!string.IsNullOrEmpty(memberDetails.MemberId) && memberDetails.MemberId != ConstantTexts.ConstantZero)
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

                        memberSearchQueryBuilder.Append(" AND HHP.MEMBERSHIP_NUMBER='" + memberDetails.MemberId + "'");
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return memberSearchQueryBuilder.ToString();
        }
    }
}
