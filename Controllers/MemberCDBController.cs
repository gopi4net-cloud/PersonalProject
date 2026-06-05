using ANGDDEAPI.Common;
using ANGDDEAPIBO;
using ANGDDEAPIDO;
using ANGDDEAPIDO.Interface;
using ANGDDEAPIFoundation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net;

namespace ANGDDEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class MemberCDBController : ControllerBase
    {
        public static long LoggedInUserId = 0;
        public string _username = string.Empty;
        string source = string.Empty;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DOConfiguration _objConfiguration;
        private readonly IMemoryCacheHelper _memoryCacheHelper;
        public readonly BOCommon _objBOCommon;

        public MemberCDBController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _memoryCacheHelper = memoryCacheHelper;
            _objBOCommon = bOCommon;

        }
        /// <summary>
        /// Get Member information based on search criteria
        /// </summary>
        /// <param name="altID"></param>
        /// <param name="startDate"></param>
        /// <param name="stopDate"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetMemberCDB(string details = null)
        {
            DOMemberSearchCriteria memberDetails = null;
            source = BOCommon.GetRefererURI(Request);
            List<DOMemberCDBInfo> dOMemberCDBInfos = null;
            MemberCBDMethods _objCSPFacetsMethods = new MemberCBDMethods(_memoryCacheHelper, _objConfiguration, _httpContextAccessor, _objBOCommon);
            try
            {
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                if (!string.IsNullOrEmpty(details))
                {
                    _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Request", details, _username);
                    memberDetails = new DOMemberSearchCriteria();
                    memberDetails = JsonConvert.DeserializeObject<DOMemberSearchCriteria>(details);
                    bool isValidRequest = _objCSPFacetsMethods.ValidateMember(memberDetails);
                    if (!isValidRequest)
                    {
                        return BadRequest(new { error = "bad request" });
                    }

                    if (memberDetails != null)
                    {
                        LoggedInUserId = memberDetails.UserId;
                    }
                }
                List<MemberCBDDetails> memberCBDDetails = new List<MemberCBDDetails>();
                List<MemberCBDDetails> memberCBDDetails1 = _objCSPFacetsMethods.GetMemberforCDB(memberDetails, memberDetails.MemberId, memberDetails.EligibilityFrom, memberDetails.EligibilityTo, out dOMemberCDBInfos);

                Hashtable hashid = new Hashtable();
                foreach (MemberCBDDetails memberCBDDetail in memberCBDDetails1)
                {

                    // if (memberCBDDetail.MemberRecordsource == "UNET" || memberCBDDetail.MemberRecordsource == "CIRRUS")
                    // {
                    if (string.IsNullOrEmpty(memberDetails.groupId))
                    {

                        if (hashid.ContainsKey(memberCBDDetail.MemberFirstName + " " + memberCBDDetail.DateOfBirth + " " + memberCBDDetail.MemberRecordsource + " " + memberCBDDetail.EmployerGroupNumber + " " + memberCBDDetail.MemberSSN + " " + memberCBDDetail.CoverageTypeCode) && hashid.ContainsValue(memberCBDDetail.MemberRecordsource))
                        {
                            continue;
                        }
                        memberCBDDetails.Add(memberCBDDetail);
                        hashid.Add(memberCBDDetail.MemberFirstName + " " + memberCBDDetail.DateOfBirth + " " + memberCBDDetail.MemberRecordsource + " " + memberCBDDetail.EmployerGroupNumber + " " + memberCBDDetail.MemberSSN + " " + memberCBDDetail.CoverageTypeCode, memberCBDDetail.MemberRecordsource);
                    }
                    else
                    {
                        if (hashid.ContainsKey(memberCBDDetail.MemberFirstName + " " + memberCBDDetail.DateOfBirth + " " + memberCBDDetail.MemberRecordsource + " " + memberCBDDetail.EmployerGroupNumber + " " + memberCBDDetail.MemberSSN + " " + memberCBDDetail.CoverageTypeCode) && hashid.ContainsValue(memberCBDDetail.MemberRecordsource + " " + memberCBDDetail.EmployerGroupNumber + " " + memberCBDDetail.MemberSSN + " " + memberCBDDetail.CoverageTypeCode))
                        {
                            continue;
                        }
                        memberCBDDetails.Add(memberCBDDetail);
                        hashid.Add(memberCBDDetail.MemberFirstName + " " + memberCBDDetail.DateOfBirth + " " + memberCBDDetail.MemberRecordsource + " " + memberCBDDetail.EmployerGroupNumber + " " + memberCBDDetail.MemberSSN + " " + memberCBDDetail.CoverageTypeCode, memberCBDDetail.MemberRecordsource + " " + memberCBDDetail.EmployerGroupNumber + " " + memberCBDDetail.MemberSSN + " " + memberCBDDetail.CoverageTypeCode);
                    }
                    // }
                }


                if (!string.IsNullOrEmpty(memberDetails.groupId))
                {
                    memberCBDDetails = (from memberCBDDetail in memberCBDDetails
                                        where memberCBDDetail.EmployerGroupNumber == memberDetails.groupId
                                        select memberCBDDetail).ToList();
                    return Ok(memberCBDDetails);
                }
                else if (!string.IsNullOrEmpty(memberDetails.FirstName) && memberDetails.DateOfBirth.ToString("yyyy-MM-dd") != "0001-01-01" && !string.IsNullOrWhiteSpace(memberDetails.DateOfBirth.ToString("yyyy-mm-dd")) && !string.IsNullOrEmpty(memberDetails.LastName))
                {
                    memberCBDDetails = (from memberCBDDetail in memberCBDDetails
                                        where memberCBDDetail.MemberFirstName == memberDetails.FirstName && memberCBDDetail.MemberLastName == memberDetails.LastName && memberCBDDetail.DateOfBirth == memberDetails.DateOfBirth.ToString("MM/dd/yyyy", null)
                                        select memberCBDDetail).ToList();
                    return Ok(memberCBDDetails);
                }
                else if (!string.IsNullOrEmpty(memberDetails.FirstName) && !string.IsNullOrEmpty(memberDetails.LastName) && !string.IsNullOrEmpty(memberDetails.MemberId))
                {
                    memberCBDDetails = (from memberCBDDetail in memberCBDDetails
                                        where memberCBDDetail.MemberFirstName == memberDetails.FirstName && memberCBDDetail.MemberLastName == memberDetails.LastName
                                        select memberCBDDetail).ToList(); return Ok(memberCBDDetails);

                }
                else if (!string.IsNullOrEmpty(memberDetails.FirstName) && !string.IsNullOrEmpty(memberDetails.MemberId))
                {
                    memberCBDDetails = (from memberCBDDetail in memberCBDDetails
                                        where memberCBDDetail.MemberFirstName == memberDetails.FirstName
                                        select memberCBDDetail).ToList(); return Ok(memberCBDDetails);

                }
                else
                {
                    return Ok(memberCBDDetails);
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
                return StatusCode((int)HttpStatusCode.InternalServerError, ex);
            }
        }

        /// <summary>
        /// Get Member information based on search criteria
        /// </summary>
        /// <param name="altID"></param>
        /// <param name="startDate"></param>
        /// <param name="stopDate"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetEligibilityCDB(string details = null)
        {
            DOMemberSearchCriteria memberDetails = null;
            source = BOCommon.GetRefererURI(Request);
            List<DOMemberCDBInfo> dOMemberCDBInfos = null;
            MemberCBDMethods _objCSPFacetsMethods = new MemberCBDMethods(_memoryCacheHelper, _objConfiguration, _httpContextAccessor, _objBOCommon);
            try
            {
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                if (!string.IsNullOrEmpty(details))
                {
                    _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Request", details, _username);
                    memberDetails = new DOMemberSearchCriteria();
                    memberDetails = JsonConvert.DeserializeObject<DOMemberSearchCriteria>(details);
                    bool isValidRequest = _objCSPFacetsMethods.ValidateMember(memberDetails);
                    if (!isValidRequest)
                        if (!isValidRequest)
                        {
                            return BadRequest(new { error = "bad request" });
                        }

                    if (memberDetails != null)
                    {
                        LoggedInUserId = memberDetails.UserId;
                    }
                }
                List<MemberCBDDetails> memberCBDDetails = new List<MemberCBDDetails>();
                memberCBDDetails = _objCSPFacetsMethods.GetMemberforCDB(memberDetails, memberDetails.MemberId, memberDetails.EligibilityFrom, memberDetails.EligibilityTo, out dOMemberCDBInfos);
                memberCBDDetails = (from memberCBDDetail in memberCBDDetails
                                    where memberCBDDetail.MemberFirstName == memberDetails.FirstName && memberCBDDetail.DateOfBirth == memberDetails.DateOfBirth.ToString("MM/dd/yyyy", null) && memberCBDDetail.MemberRecordsource == memberDetails.MemberRecordsource && memberCBDDetail.CoverageTypeCode == memberDetails.CoverageType && memberCBDDetail.MemberSuffix==memberDetails.MemberSuffix
                                    select memberCBDDetail).ToList();




                if (!string.IsNullOrEmpty(memberDetails.groupId))
                {
                    memberCBDDetails = (from memberCBDDetail in memberCBDDetails
                                        where memberCBDDetail.MemberFirstName == memberDetails.FirstName && memberCBDDetail.EmployerGroupNumber == memberDetails.groupId && memberCBDDetail.MemberSuffix == memberDetails.MemberSuffix
                                        select memberCBDDetail).ToList();
                    return Ok(memberCBDDetails);
                }
                else
                {
                    return Ok(memberCBDDetails);
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
                return StatusCode((int)HttpStatusCode.InternalServerError, ex);
            }
        }


        /// <summary>
        /// Get Member information based on search criteria
        /// </summary>
        /// <param name="altID"></param>
        /// <param name="startDate"></param>
        /// <param name="stopDate"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetEligibilityMemberCDB(string details = null)
        {
            DOMemberSearchCriteria memberDetails = null;
            source = BOCommon.GetRefererURI(Request);
            List<DOMemberCDBInfo> dOMemberCDBInfos = null;
            MemberCBDMethods _objCSPFacetsMethods = new MemberCBDMethods(_memoryCacheHelper, _objConfiguration, _httpContextAccessor, _objBOCommon);
            try
            {
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                if (!string.IsNullOrEmpty(details))
                {
                    _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Request", details, _username);
                    memberDetails = new DOMemberSearchCriteria();
                    memberDetails = JsonConvert.DeserializeObject<DOMemberSearchCriteria>(details);
                    bool isValidRequest = _objCSPFacetsMethods.ValidateMember(memberDetails);

                    if (!isValidRequest)
                    {
                        return BadRequest(new { error = "bad request" });
                    }

                    if (memberDetails != null)
                    {
                        LoggedInUserId = memberDetails.UserId;
                    }

                }
                List<MemberCBDDetails> memberCBDDetails = new List<MemberCBDDetails>();
                memberCBDDetails = _objCSPFacetsMethods.GetMemberforCDB(memberDetails, memberDetails.MemberId, memberDetails.EligibilityFrom, memberDetails.EligibilityTo, out dOMemberCDBInfos);

                // added by gopi to extract enrollee ssn for dependents
                if (memberCBDDetails.Any() && memberDetails.RelationshipTypeCode != "EE")
                {
                    // Optional: avoid repeated ToString call  
                    var enrolleeDob = memberDetails.EnrolleeDOB?.ToString("MM/dd/yyyy", null) ?? string.Empty;

                    var enrolleeSSN = memberCBDDetails.Where(item =>
                               item.RelationshipCode    == "EE" &&
                               item.EmployerGroupNumber == memberDetails.groupId &&
                               item.DateOfBirth         == enrolleeDob &&
                               item.MemberRecordsource  == memberDetails.MemberRecordsource &&
                               item.EligibilityTo       == memberDetails.EligibilityTo &&
                               item.EligibilityFrom     == memberDetails.EligibilityFrom &&
                               item.CoverageTypeCode    == memberDetails.CoverageType).Select(item => item.MemberSSN).FirstOrDefault();

                    if (!string.IsNullOrEmpty(enrolleeSSN))
                    {
                        foreach (var item in memberCBDDetails)
                        {
                            item.EnrolleeSSN = enrolleeSSN;
                        }
                    }
                }
                memberCBDDetails = (from memberCBDDetail in memberCBDDetails
                                    where memberCBDDetail.EmployerGroupNumber == memberDetails.groupId &&
                                    memberCBDDetail.MemberFirstName == memberDetails.FirstName && 
                                    memberCBDDetail.DateOfBirth == memberDetails.DateOfBirth.ToString("MM/dd/yyyy", null) &&
                                    memberCBDDetail.MemberRecordsource == memberDetails.MemberRecordsource && 
                                    memberCBDDetail.EligibilityTo == memberDetails.EligibilityTo && 
                                    memberCBDDetail.EligibilityFrom == memberDetails.EligibilityFrom && 
                                    memberCBDDetail.CoverageTypeCode == memberDetails.CoverageType &&
                                   memberCBDDetail.RelationshipCode == memberDetails.RelationshipTypeCode
                                    select memberCBDDetail).ToList();



                return Ok(memberCBDDetails);
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

    }
}
