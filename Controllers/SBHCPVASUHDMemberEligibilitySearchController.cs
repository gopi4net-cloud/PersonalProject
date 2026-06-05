using ANGDDEAPI.Common;
using ANGDDEAPIBO;
using ANGDDEAPIBO.Interface;
using ANGDDEAPIDO;
using ANGDDEAPIDO.CDB;
using ANGDDEAPIDO.Interface;
using ANGDDEAPIFoundation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;

namespace ANGDDEAPI.Controllers
{
    /// <summary>
    /// NEW DotNet core
    /// </summary>
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class SBHCPVASUHDMemberEligibilitySearchController : ControllerBase
    {
        public static long LoggedInUserId = 0;
        public string _username = string.Empty;
        string source = string.Empty;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DOConfiguration _objConfiguration;
        private readonly IMemoryCacheHelper _memoryCacheHelper;
        private readonly BOCommon _objBOCommon;
        private readonly ICacheService _cache;
        List<DOATSLookupMaster> lstATSLookupMaster = null;
        public ExceptionTypes exResult;
        public BOGPSMemberDetails _objBOGPSMemberDetails = null;
        public AccessAPI objAPICall = null;

        public SBHCPVASUHDMemberEligibilitySearchController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _memoryCacheHelper = memoryCacheHelper;
            _objBOCommon = bOCommon;
            _cache = cache;
            exResult = _objBOCommon.GetRRTGPSAPiConfig(out lstATSLookupMaster);
            _objBOGPSMemberDetails = new BOGPSMemberDetails(objConfiguration, _cache);
            objAPICall = new AccessAPI(_memoryCacheHelper, _objConfiguration);
        }

        /// <summary>
        /// Get Member information based on search criteria
        /// </summary>
        /// <param name="altID"></param>
        /// <param name="startDate"></param>
        /// <param name="stopDate"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetMemberVASandUHD(string details = null)
        {
            DOMemberSearchCriteria memberDetails = null;
            source = BOCommon.GetRefererURI(Request);
            List<DOMemberCDBInfo> dOMemberCDBInfos = null;
            MemberCBDMethods _objCSPFacetsMethods = new MemberCBDMethods(_memoryCacheHelper, _objConfiguration, _httpContextAccessor, _objBOCommon);
            SBHCPVASUHDMemberEligibilitySearchMethods objHCPMemberMethods = new SBHCPVASUHDMemberEligibilitySearchMethods(_memoryCacheHelper, _objConfiguration);
            //memberCBDDetails = objHCPMemberMethods.GetMembeVASandDentaldetailsID(memberDetails);
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
                List<MemberCBDDetails> memberCBDDetails1 =  new List<MemberCBDDetails>();

                if (memberDetails.MemberSearchType== "CDB")
                {

                    memberCBDDetails1 = objHCPMemberMethods.GetMemberforCDB(memberDetails, memberDetails.MemberId, memberDetails.EligibilityFrom, memberDetails.EligibilityTo, out dOMemberCDBInfos);

                    if (memberCBDDetails1.Count <= 0) {
                        memberDetails.MemberRecordsource = memberDetails.planTypeCode;
                        memberCBDDetails1 = objHCPMemberMethods.GetMembeVASandDentaldetailsID(memberDetails);
                    }
                }
                else
                {
                    memberCBDDetails1 = objHCPMemberMethods.GetMembeVASandDentaldetailsID(memberDetails);
                }

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
                                        where memberCBDDetail.MemberFirstName.ToUpper() == memberDetails.FirstName && memberCBDDetail.MemberLastName.ToUpper() == memberDetails.LastName && memberCBDDetail.DateOfBirth == memberDetails.DateOfBirth.ToString("MM/dd/yyyy", null)
                                        select memberCBDDetail).ToList();
                    return Ok(memberCBDDetails);
                }
                else if (!string.IsNullOrEmpty(memberDetails.FirstName) && !string.IsNullOrEmpty(memberDetails.LastName) && !string.IsNullOrEmpty(memberDetails.MemberId))
                {
                    memberCBDDetails = (from memberCBDDetail in memberCBDDetails
                                        where memberCBDDetail.MemberFirstName.ToUpper() == memberDetails.FirstName && memberCBDDetail.MemberLastName.ToUpper() == memberDetails.LastName
                                        select memberCBDDetail).ToList(); return Ok(memberCBDDetails);

                }
                else if (!string.IsNullOrEmpty(memberDetails.FirstName) && !string.IsNullOrEmpty(memberDetails.MemberId))
                {
                    memberCBDDetails = (from memberCBDDetail in memberCBDDetails
                                        where memberCBDDetail.MemberFirstName.ToUpper() == memberDetails.FirstName
                                        select memberCBDDetail).ToList(); return Ok(memberCBDDetails);

                }
                else if (!string.IsNullOrEmpty(memberDetails.LastName) && !string.IsNullOrEmpty(memberDetails.MemberId))
                {
                    memberCBDDetails = (from memberCBDDetail in memberCBDDetails
                                        where memberCBDDetail.MemberLastName.ToUpper() == memberDetails.LastName
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
        public IActionResult GetEligibilityVASandUHD(string details = null)
        {
            DOMemberSearchCriteria memberDetails = null;
            source = BOCommon.GetRefererURI(Request);
            List<DOMemberCDBInfo> dOMemberCDBInfos = null;
            MemberCBDMethods _objCSPFacetsMethods = new MemberCBDMethods(_memoryCacheHelper, _objConfiguration, _httpContextAccessor, _objBOCommon);
            SBHCPVASUHDMemberEligibilitySearchMethods objHCPMemberMethods = new SBHCPVASUHDMemberEligibilitySearchMethods(_memoryCacheHelper, _objConfiguration);

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
                if (memberDetails.MemberSearchType == "CDB")
                {

                    memberCBDDetails = objHCPMemberMethods.GetMemberforCDB(memberDetails, memberDetails.MemberId, memberDetails.EligibilityFrom, memberDetails.EligibilityTo, out dOMemberCDBInfos);

                    if (memberCBDDetails.Count <= 0)
                    {
                       /// memberDetails.MemberRecordsource = memberDetails.planTypeCode;
                        memberCBDDetails = objHCPMemberMethods.GetMembeVASandDentaldetailsID(memberDetails);
                    }
                }
                else
                {
                    memberCBDDetails = objHCPMemberMethods.GetMembeVASandDentaldetailsID(memberDetails);
                }

               // memberCBDDetails = objHCPMemberMethods.GetMembeVASandDentaldetailsID(memberDetails);
                memberCBDDetails = (from memberCBDDetail in memberCBDDetails
                                    where string.Equals(memberCBDDetail.MemberFirstName, memberDetails.FirstName, StringComparison.OrdinalIgnoreCase) && memberCBDDetail.DateOfBirth == memberDetails.DateOfBirth.ToString("MM/dd/yyyy", null) && memberCBDDetail.MemberRecordsource == memberDetails.MemberRecordsource 
                                    select memberCBDDetail).ToList();




                if (!string.IsNullOrEmpty(memberDetails.groupId))
                {
                    memberCBDDetails = (from memberCBDDetail in memberCBDDetails
                                        where string.Equals(memberCBDDetail.MemberFirstName, memberDetails.FirstName, StringComparison.OrdinalIgnoreCase) && memberCBDDetail.EmployerGroupNumber == memberDetails.groupId
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
        public IActionResult GetEligibilityMemberVASandUHD(string details = null)
        {
            DOMemberSearchCriteria memberDetails = null;
            source = BOCommon.GetRefererURI(Request);
            List<DOMemberCDBInfo> dOMemberCDBInfos = null;
            MemberCBDMethods _objCSPFacetsMethods = new MemberCBDMethods(_memoryCacheHelper, _objConfiguration, _httpContextAccessor, _objBOCommon);
            SBHCPVASUHDMemberEligibilitySearchMethods objHCPMemberMethods = new SBHCPVASUHDMemberEligibilitySearchMethods(_memoryCacheHelper, _objConfiguration);

            try
            {
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                if (!string.IsNullOrEmpty(details))
                {
                    _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Request", details, _username);


                    details=objHCPMemberMethods.ValidateDateOfBirth(details);

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
                if (memberDetails.MemberSearchType == "CDB")
                {

                    memberCBDDetails = objHCPMemberMethods.GetMemberforCDB(memberDetails, memberDetails.MemberId, memberDetails.EligibilityFrom, memberDetails.EligibilityTo, out dOMemberCDBInfos);

                    if (memberCBDDetails.Count <= 0)
                    {
                        //memberDetails.MemberRecordsource = memberDetails.planTypeCode;
                        memberCBDDetails = objHCPMemberMethods.GetMembeVASandDentaldetailsID(memberDetails);
                    }
                }
                else
                {
                    memberCBDDetails = objHCPMemberMethods.GetMembeVASandDentaldetailsID(memberDetails);
                }
               // memberCBDDetails = objHCPMemberMethods.GetMembeVASandDentaldetailsID(memberDetails);
                if (string.IsNullOrEmpty(memberDetails.EligibilityTo))
                {
                    memberCBDDetails = (from memberCBDDetail in memberCBDDetails
                                        where memberCBDDetail.EmployerGroupNumber == memberDetails.groupId && string.Equals(memberCBDDetail.MemberFirstName, memberDetails.FirstName, StringComparison.OrdinalIgnoreCase) && memberCBDDetail.DateOfBirth == memberDetails.DateOfBirth.ToString("MM/dd/yyyy", null) && memberCBDDetail.MemberRecordsource == memberDetails.MemberRecordsource && memberCBDDetail.EligibilityFrom == memberDetails.EligibilityFrom
                                        select memberCBDDetail).ToList();
                }
                else {
                    memberCBDDetails = (from memberCBDDetail in memberCBDDetails
                                        where memberCBDDetail.EmployerGroupNumber == memberDetails.groupId && string.Equals(memberCBDDetail.MemberFirstName, memberDetails.FirstName, StringComparison.OrdinalIgnoreCase) && memberCBDDetail.DateOfBirth == memberDetails.DateOfBirth.ToString("MM/dd/yyyy", null) && memberCBDDetail.MemberRecordsource == memberDetails.MemberRecordsource && memberCBDDetail.EligibilityFrom == memberDetails.EligibilityFrom && memberCBDDetail.EligibilityTo == memberDetails.EligibilityTo 
                                        select memberCBDDetail).ToList();
                }

                if (memberCBDDetails != null && memberCBDDetails.Count() > 0)
                {
                    
                    DODentalFacetEligibilityHPResponse eligibilityHPResponse = null;
                    DODentalFacetEligibilityHPSearchInputRequest ObjDODentalFacetEligibilityHPSearchInputRequest = new DODentalFacetEligibilityHPSearchInputRequest()
                    {
                        searchInput = new DODentalFacetEligibilityHPSearchInput()
                        {
                            dateRange = new DentalFacetCommonDateRange()
                        }
                    };
                    string[] dateFormats = { "M/d/yyyy", "MM/dd/yyyy", "M/d/yyyy h:mm:ss tt", "M/d/yyyy h:m:s tt", "M/d/yyyy h:m:s", "M/d/yyyy h:m:s tt", "M/d/yyyy h:m" };

                    ObjDODentalFacetEligibilityHPSearchInputRequest.searchInput.facetsIdentity = "FXIGUESTATS";
                    ObjDODentalFacetEligibilityHPSearchInputRequest.searchInput.clientCode = "ATSo7Y36nheU";
                    ObjDODentalFacetEligibilityHPSearchInputRequest.searchInput.lapAndHcrInfoNeeded = "Y";
                    ObjDODentalFacetEligibilityHPSearchInputRequest.searchInput.memberContrivedKey = memberCBDDetails[0].individualIdentifier;
                    ObjDODentalFacetEligibilityHPSearchInputRequest.searchInput.subscriberId = memberCBDDetails[0].EnrolleAlternateId;
                    ObjDODentalFacetEligibilityHPSearchInputRequest.searchInput.dateOfBirth = memberCBDDetails[0].DateOfBirth!=null?DateTime.ParseExact(memberCBDDetails[0].DateOfBirth, dateFormats, null).ToString("yyyy-MM-dd", null):null;
                    ObjDODentalFacetEligibilityHPSearchInputRequest.searchInput.dateRange.startDate = memberCBDDetails[0].EligibilityFrom !=null?DateTime.ParseExact(memberCBDDetails[0].EligibilityFrom, dateFormats, null).ToString("yyyy-MM-dd", null):null;


                    if (string.IsNullOrEmpty(memberCBDDetails[0].EligibilityTo))
                    {
                        ObjDODentalFacetEligibilityHPSearchInputRequest.searchInput.dateRange.stopDate = DateTime.ParseExact("12/31/9999", "MM/dd/yyyy", null).ToString("yyyy-MM-dd", null);

                       
                    }
                    else {
                        ObjDODentalFacetEligibilityHPSearchInputRequest.searchInput.dateRange.stopDate = DateTime.ParseExact(memberCBDDetails[0].EligibilityTo, dateFormats, null).ToString("yyyy-MM-dd", null);
                    }
                        var exceptionType = objHCPMemberMethods.GetEligibilitySummary(ObjDODentalFacetEligibilityHPSearchInputRequest, out eligibilityHPResponse);

                    if (exceptionType == ExceptionTypes.Success)
                    {
                        var searchResult = eligibilityHPResponse.searchResult.searchOutput.dentalCoverage.subscriber;

                        if (exceptionType == ExceptionTypes.Success
                            && eligibilityHPResponse != null
                            && searchResult.memberDetails != null
                            && searchResult.memberDetails.Count() > 0
                           
                            && memberCBDDetails[0].DateOfBirth != null
                            && memberCBDDetails[0].EnrolleAlternateId.Equals(eligibilityHPResponse.searchResult.searchOutput.dentalCoverage.subscriber.subscriberId, StringComparison.OrdinalIgnoreCase)
                            && DateTime.ParseExact(memberCBDDetails[0].DateOfBirth, dateFormats, null).ToString("yyyy-MM-dd", null).Equals(eligibilityHPResponse.searchResult.searchOutput.dentalCoverage.subscriber.memberDetails[0].memberProfile.dateOfBirth, StringComparison.OrdinalIgnoreCase)
                            )
                        {
                            DODentalFacetMemberPreferenceRequest memberPreferenceRequestObj = new DODentalFacetMemberPreferenceRequest()
                            {
                                dateRange = new DentalFacetCommonDateRange()
                            };
                            memberPreferenceRequestObj.sourceSpecificId = "FXIGUESTI";
                            memberPreferenceRequestObj.requestType = "P";
                            memberPreferenceRequestObj.memberKey = searchResult.memberDetails[0].memberContrivedKey;
                            memberPreferenceRequestObj.dateRange.startDate = ObjDODentalFacetEligibilityHPSearchInputRequest.searchInput.dateRange.startDate;
                            memberPreferenceRequestObj.dateRange.stopDate = ObjDODentalFacetEligibilityHPSearchInputRequest.searchInput.dateRange.stopDate;

                            DODentalFacetMemberPreferenceResponse memeberPreferenceHPResponse = null;

                            var memPrefExceptionType = objHCPMemberMethods.GetMemberPreference(memberPreferenceRequestObj, out memeberPreferenceHPResponse);
                                if (memberDetails.MemberRecordsource == "UHD")
                                {
                                   // memberCBDDetails[0].EmbededOrStandalone = searchResult.memberDetails[0].lapAndHcrInfo.embeddedMemberFlag.ToUpper()=="YES"? "HCRY": searchResult.memberDetails[0].lapAndHcrInfo.embeddedMemberFlag.ToUpper() == "NO"? "HCRN":"";
                                    memberCBDDetails[0].LegalEntityName = searchResult.memberDetails[0].eligibility[0].legalEntity.codeDesc== "Excellus Health Plan, Inc. (Excellus BlueCross Blu"? "Excellus Health Plan, Inc. (Excellus BlueCross BlueShield)": searchResult.memberDetails[0].eligibility[0].legalEntity.codeDesc== "HEALTHPLEX, INC."? "Healthplex, Inc": searchResult.memberDetails[0].eligibility[0].legalEntity.codeDesc;

                                if (searchResult.memberDetails[0].groupType == "MDCR")
                                {
                                    memberCBDDetails[0].provRelationshipPrefix = searchResult.memberDetails[0].groupType;

                                }
                                else
                                {
                                    memberCBDDetails[0].provRelationshipPrefix = searchResult.memberDetails[0].eligibility[0].productPlanType.codeValue == "DHMO" ? "HMO" : searchResult.memberDetails[0].eligibility[0].productPlanType.codeValue;
                                }
                                    memberCBDDetails[0].StateofSitus = searchResult.memberDetails[0].groupState;
                                //memberCBDDetails[0].provRelationshipPrefix = searchResult.memberDetails[0].eligibility[0].productPlanType.codeValue == "DHMO" ? "HMO" : searchResult.memberDetails[0].eligibility[0].productPlanType.codeValue;
                                    memberCBDDetails[0].StateofSitus = searchResult.memberDetails[0].groupState;
                                memberCBDDetails[0].FundingTypeCode = searchResult.memberDetails[0].eligibility[0].fundingType;
                                memberCBDDetails[0].ProductCode = searchResult.memberDetails[0].eligibility[0].product.codeValue;
                                memberCBDDetails[0].EmbededOrStandalone = memPrefExceptionType == ExceptionTypes.Success ? memeberPreferenceHPResponse.memberPreference.isEmbeddedPlan == "Y" ? "HCRY" : memeberPreferenceHPResponse.memberPreference.isEmbeddedPlan == "N" ? "HCRN" : "" : "";

                            }
                            memberCBDDetails[0].LOBDesc = searchResult.memberDetails[0].lineOfBusinessId;
                                
                            memberCBDDetails[0].ALTLOB = searchResult.memberDetails[0].alternateLineOfBusinessId;
                            memberCBDDetails[0].MecmEmailAddress = memPrefExceptionType == ExceptionTypes.Success ? memeberPreferenceHPResponse.memberPreference.mecmEmailAddress : "";
                            memberCBDDetails[0].MecmPrefIndicatorEmail = memPrefExceptionType == ExceptionTypes.Success ? memeberPreferenceHPResponse.memberPreference.mecmPrefIndicatorEmail.codeValue : "";


                        }
                    }
                }

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
