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
using System.Text;
using System.Threading.Tasks;

namespace ANGDDEAPI.Controllers
{
    
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class MemberSearchController : ControllerBase
    {
        public static long LoggedInUserId = 0;
        public string _username = string.Empty;
        public string source = string.Empty;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DOConfiguration _objConfiguration;
        public readonly BOCommon _objBOCommon;
        private readonly ICacheService _cache; 


        public MemberSearchController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, BOCommon bOCommon, ICacheService cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _objBOCommon = bOCommon;
            _cache = cache;
        }
        /// <summary>
        /// Get Member Details from Oracle GPS DB
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
                //throw new Exception();
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                _objBOCommon.ErrorLog(LoggedInUserId, "GPSAPI:GetMemberSearchDetails", "member Search API working", "member Search API working");
                if (!string.IsNullOrEmpty(details))
                {
                    memberDetails = new DOMemberSearchCriteria();
                    memberDetails = JsonConvert.DeserializeObject<DOMemberSearchCriteria>(details);
                    if (memberDetails != null)
                    {
                        LoggedInUserId = memberDetails.UserId;
                    }
                }
                string query = ConstantTexts.GPSMemberSelectScript;///oracle query string
                //string query = ConstantTexts.SQLSelectScripts;// SQL Query String
                query = GetMemberSearchQuery(query, memberDetails);
                memberSearchDetails = new List<DOGPSMemberDetails>();
                if (!string.IsNullOrWhiteSpace(query))
                {
                    memberSearchDetails = BOGPSMemberDetails.GetMemberDetails(query, LoggedInUserId);
                    if (memberSearchDetails != null && memberSearchDetails.Count > 0)
                    {
                        _objBOCommon.ErrorLog(LoggedInUserId, "GPSAPI:GetMemberSearchDetails", "member Search API working:Count>0", "member Search API working");
                    }
                }
                else
                {
                    _objBOCommon.ErrorLog(LoggedInUserId, "GPSAPI:GetMemberSearchDetails", "no input found", "GetMemberSearchDetails API found no input params");
                }
                return Ok(memberSearchDetails);
            }
            catch (Exception ex)
            {

                if (source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(0, this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), ex.Message, ex.ToString());
                }
                else
                {
                    _objBOCommon.LogError(this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                }
                return StatusCode((int)HttpStatusCode.InternalServerError, ex);
            }

            //return Request.CreateResponse(HttpStatusCode.OK, memberSearchDetails);
        }
        /// <summary>
        /// Build Query string based on search parameters
        /// </summary>
        /// <param name="query"></param>
        /// <param name="memberSearch"></param>
        /// <returns></returns>
        public string GetMemberSearchQuery(string query, DOMemberSearchCriteria memberSearch)
        {
            StringBuilder memberSearchQueryBuilder = new StringBuilder();
            try
            {
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                //// for ORACLE QUERY STRING
                if (memberSearch != null)
                {
                    string memberId = memberSearch.MemberId;
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
                        memberSearch.MemberId = memberId;
                    }
                    memberSearchQueryBuilder.Append(query);
                    if (!string.IsNullOrEmpty(memberSearch.HICNumber))
                    {
                        //query = query + "WHERE IND.MEDICARE_CLAIM_NUM =" + "'" + memberSearch.HICNumber + "'";
                        memberSearchQueryBuilder.Append("WHERE IND.MEDICARE_CLAIM_NUM =" + "'" + memberSearch.HICNumber + "'");
                    }
                    else if ((!string.IsNullOrEmpty(memberSearch.FirstName) && (!string.IsNullOrEmpty(memberSearch.LastName))))
                    {
                        if (!string.IsNullOrEmpty(memberSearch.MemberId))
                        {
                            memberSearchQueryBuilder.Append(" WHERE HHP.MEMBERSHIP_NUMBER LIKE" + "'%" + memberSearch.MemberId + "%'" + " AND IND.LAST_NAME LIKE" + "'%" + memberSearch.LastName + "%'" + " AND IND.FIRST_NAME LIKE" + "'%" + memberSearch.FirstName + "%'");
                            //query = query + " WHERE HHP.MEMBERSHIP_NUMBER LIKE" + "'%" + memberSearch.MemberId + "%'" + " AND IND.LAST_NAME LIKE" + "'%" + memberSearch.LastName + "%'" + " AND IND.FIRST_NAME LIKE" + "'%" + memberSearch.FirstName + "%'";
                        }
                        else if (memberSearch.DateOfBirth != null)
                        {
                            string date = memberSearch.DateOfBirth.ToString("MM/dd/yyyy", null);
                            memberSearchQueryBuilder.Append(" WHERE to_char(DATE_OF_BIRTH,'MM/DD/YYYY')=" + "'" + date + "'" + " AND IND.LAST_NAME LIKE" + "'%" + memberSearch.LastName + "%'" + " AND IND.FIRST_NAME LIKE" + "'%" + memberSearch.FirstName + "%'");
                            //query = query + " WHERE to_char(DATE_OF_BIRTH,'MM/DD/YYYY')=" + "'" + date + "'" + " AND IND.LAST_NAME LIKE" + "'%" + memberSearch.LastName + "%'" + " AND IND.FIRST_NAME LIKE" + "'%" + memberSearch.FirstName + "%'";
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(memberSearch.MemberId))
                    {
                        memberSearchQueryBuilder.Append("WHERE HHP.MEMBERSHIP_NUMBER =" + "'" + memberSearch.MemberId + "'");
                        //query = query + "WHERE HHP.MEMBERSHIP_NUMBER =" + "'" + memberSearch.MemberId + "'";
                    }
                    else if ((!string.IsNullOrEmpty(memberSearch.State)) && (!string.IsNullOrEmpty(memberSearch.ZIP)))
                    {
                        memberSearchQueryBuilder.Append("WHERE HAD1.state_cd=" + "'" + memberSearch.State + "'" + " And HAD1.zip_cd=" + "'" + memberSearch.ZIP + "'");
                        //query = query + "WHERE HAD1.state_cd=" + "'" + memberSearch.State + "'" + " And HAD1.zip_cd=" + "'" + memberSearch.ZIP + "'";
                    }
                    else if (!String.IsNullOrEmpty(memberSearch.PhoneNumber))
                    {
                        memberSearchQueryBuilder.Append("WHERE HH.DAYTIME_PHONE_NUM LIKE" + "'%" + memberSearch.PhoneNumber + "%'");
                        //query = query + "WHERE HH.DAYTIME_PHONE_NUM LIKE" + "'%" + memberSearch.PhoneNumber + "%'";
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
                    _objBOCommon.ErrorLog(0, this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), ex.Message, ex.ToString());
                }
                else
                {
                    _objBOCommon.LogError(this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                }
            }
            return ((memberSearchQueryBuilder != null && !string.IsNullOrWhiteSpace(memberSearchQueryBuilder.ToString())) ? memberSearchQueryBuilder.ToString() : string.Empty);
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
                //throw new Exception();
                _objBOCommon.ErrorLog(LoggedInUserId, "GPSAPI:GetPlanDetails", "GetPlanDetails API working", "GetPlanDetails API working");
                if (!string.IsNullOrEmpty(details))
                {
                    planDetails = new DOMemberSearchCriteria();
                    planDetails = JsonConvert.DeserializeObject<DOMemberSearchCriteria>(details);
                    if (planDetails != null)
                    {
                        LoggedInUserId = planDetails.UserId;
                    }
                }
                string PCPQuery = string.Empty;
                string query = GetPlanSearchQuery(planDetails, out PCPQuery);
                planSearchDetails = new List<DOMemberPlanDetails>();
                if (!string.IsNullOrWhiteSpace(query))
                {
                    planSearchDetails = BOGPSMemberDetails.GetPlanDetails(query, PCPQuery, LoggedInUserId);
                    if (planSearchDetails != null && planSearchDetails.Count > 0)
                    {
                        _objBOCommon.ErrorLog(LoggedInUserId, "GPSAPI:GetPlanDetails", "GetPlanDetails API working:Count>0", "GetPlanDetails API working");
                    }
                }
                else
                {
                    _objBOCommon.ErrorLog(LoggedInUserId, "GPSAPI:GetPlanDetails", "no input found", "GetPlanDetails API found no input params");
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

        private string GetPlanSearchQuery(DOMemberSearchCriteria planSearch, out string PCPQuery)
        {
            StringBuilder planSearchQueryBuilder = new StringBuilder();
            StringBuilder planSearchQuery2Builder = new StringBuilder();
            PCPQuery = string.Empty;
            try
            {
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                if (planSearch != null)
                {
                    bool isSearchCriteriaSet = false;
                    planSearchQueryBuilder.Append(ConstantTexts.GPSPlanSelectScript1);///oracle query string
                    planSearchQuery2Builder.Append(ConstantTexts.GPSPlanPCPNameSelectScript);
                    if (!string.IsNullOrEmpty(planSearch.HICNumber) && planSearch.HICNumber != ConstantTexts.ConstantZero)
                    {
                        planSearchQueryBuilder.Append(" IND.MEDICARE_CLAIM_NUM ='" + planSearch.HICNumber + "'");
                        planSearchQuery2Builder.Append(" IND.MEDICARE_CLAIM_NUM ='" + planSearch.HICNumber + "'");
                        isSearchCriteriaSet = true;
                    }
                    if (!string.IsNullOrEmpty(planSearch.IndividualId) && planSearch.IndividualId != ConstantTexts.ConstantZero)
                    {
                        if (isSearchCriteriaSet)
                        {
                            planSearchQueryBuilder.Append(" AND");
                            planSearchQuery2Builder.Append(" AND");
                        }
                        planSearchQueryBuilder.Append(" IND.INDIVIDUAL_ID=" + planSearch.IndividualId);
                        planSearchQuery2Builder.Append(" IND.INDIVIDUAL_ID=" + planSearch.IndividualId);
                        isSearchCriteriaSet = true;
                    }
                    if (!string.IsNullOrEmpty(planSearch.MemberId) && planSearch.MemberId != ConstantTexts.ConstantZero)
                    {
                        string memberId = planSearch.MemberId;
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
                            planSearch.MemberId = memberId;
                        }
                        if (isSearchCriteriaSet)
                        {
                            planSearchQueryBuilder.Append(" AND");
                            planSearchQuery2Builder.Append(" AND");
                        }
                        planSearchQueryBuilder.Append(" HHP.MEMBERSHIP_NUMBER ='" + planSearch.MemberId + "'");
                        planSearchQuery2Builder.Append(" HHP.MEMBERSHIP_NUMBER ='" + planSearch.MemberId + "'");
                        isSearchCriteriaSet = true;
                    }
                    if (!isSearchCriteriaSet)
                    {
                        planSearchQueryBuilder = null;
                        planSearchQuery2Builder = null;
                    }
                    if (isSearchCriteriaSet)
                    {
                        PCPQuery = planSearchQuery2Builder != null && !string.IsNullOrWhiteSpace(planSearchQuery2Builder.ToString()) ? planSearchQuery2Builder.ToString() : string.Empty;
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
    }
}
