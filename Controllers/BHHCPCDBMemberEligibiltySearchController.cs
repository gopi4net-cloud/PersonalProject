using ANGDDEAPI.Common;
using ANGDDEAPIBO;
using ANGDDEAPIDO;
using ANGDDEAPIDO.CDB;
using ANGDDEAPIDO.CDB.PlanBenifit;
using ANGDDEAPIDO.Interface;
using ANGDDEAPIFoundation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
//using System.Web.Http.Results;

namespace ANGDDEAPI.Controllers
{
    /// <summary>
    /// NEW DotNet core
    /// </summary>
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class BHHCPCDBMemberEligibiltySearchController : ControllerBase
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

        public BHHCPCDBMemberEligibiltySearchController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _memoryCacheHelper = memoryCacheHelper;
            _objBOCommon = bOCommon;
            _cache = cache;
            exResult = _objBOCommon.GetRRTGPSAPiConfig(out lstATSLookupMaster);
           _objBOGPSMemberDetails = new BOGPSMemberDetails(objConfiguration, _cache);
        }


        [HttpGet]
        public ActionResult GetMemberSearchDetails(string details = null)
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

        [HttpGet]
        public ActionResult GetMemberSearchDetailsCDCFromHCPCDB(string details = null)
        {
            DOMemberCDBSearchInput memberDetails = null;
            DOHCPCDBMemberElgibilityResponse memberSearchDetails = null;
            source = BOCommon.GetRefererURI(Request);
            try
            {
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                //_httpContextAccessor.HttpContext.User.Identity.Name.Split(new string[] { @"\" }, StringSplitOptions.RemoveEmptyEntries);


                if (!string.IsNullOrEmpty(details))
                {
                    memberDetails = new DOMemberCDBSearchInput();
                    memberDetails = JsonConvert.DeserializeObject<DOMemberCDBSearchInput>(details);
                    bool isValidRequest = false;
                    if (!string.IsNullOrEmpty(memberDetails.FirstName)
                        || !string.IsNullOrEmpty(memberDetails.LastName)
                        || !string.IsNullOrEmpty(memberDetails.DateOfBirth))
                    {
                        isValidRequest = true;
                    }
                    if (!isValidRequest)
                    {
                        return BadRequest(new { error = "bad request" });
                    }

                    if (memberDetails != null)
                    {
                        LoggedInUserId = memberDetails.UserId;
                    }
                    memberSearchDetails = new DOHCPCDBMemberElgibilityResponse();
                    //BOHCPMemberDetails objBOHCPMemberDetails = new BOHCPMemberDetails(_objConfiguration);
                    HCPCDBMemberMethods objHCPMemberMethods = new HCPCDBMemberMethods(_memoryCacheHelper, _objConfiguration);
                    memberSearchDetails = objHCPMemberMethods.GetMemberCDCDetails(memberDetails);

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

        public ActionResult GetMemberBenifitDetails(string details = null)
        {
            DOMemberbhPlanEligibilitySearchCriteria memberDetails = null;
            DOGPSMemberDetails dOGPSMemberDetails = null;
            MemberBenifitPackagePlanResponse dOMemberBenifitDetails = null;
            List<DOGPSMemberDetails> memberSearchDetails = null;
            source = BOCommon.GetRefererURI(Request);
            try
            {
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                //_httpContextAccessor.HttpContext.User.Identity.Name.Split(new string[] { @"\" }, StringSplitOptions.RemoveEmptyEntries);


                if (!string.IsNullOrEmpty(details))
                {
                    memberDetails = new DOMemberbhPlanEligibilitySearchCriteria();
                    memberDetails = JsonConvert.DeserializeObject<DOMemberbhPlanEligibilitySearchCriteria>(details);
                    #region MemberEligibilty empty request handling on 02/15/2021 by Sunil/Naseer/Pallavi
                    bool isValidRequest = false;
                    if (!string.IsNullOrEmpty(memberDetails.subscriberNbr)
                        || !string.IsNullOrEmpty(memberDetails.birthDate) || !string.IsNullOrEmpty(memberDetails.firstName)
                        || !string.IsNullOrEmpty(memberDetails.lastName))
                    {
                        isValidRequest = true;
                    }
                    if (!isValidRequest)
                    {
                        return BadRequest(new { error = "bad request" });
                    }
                    #endregion
                    ////if (memberDetails != null)
                    ////{
                    ////    LoggedInUserId = memberDetails.UserId;
                    ////}
                    memberSearchDetails = new List<DOGPSMemberDetails>();
                    //BOHCPMemberDetails objBOHCPMemberDetails = new BOHCPMemberDetails(_objConfiguration);
                    HCPCDBMemberMethods objHCPMemberMethods = new HCPCDBMemberMethods(_memoryCacheHelper, _objConfiguration);
                    dOMemberBenifitDetails = objHCPMemberMethods.GetMemberBenifitPackagePlanDetails(memberDetails);

                }
                return Ok(dOMemberBenifitDetails);
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

        public ActionResult GetMemberBenifitPackagePlanDetails(string details = null)
        {
            DOMemberbhPlanEligibilitySearchCriteria memberDetails = null;
            DOGPSMemberDetails dOGPSMemberDetails = null;
            DOMemberBenifitDetailsResponse dOMemberBenifitDetails = null;
            List<DOGPSMemberDetails> memberSearchDetails = null;
            source = BOCommon.GetRefererURI(Request);
            try
            {
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                //_httpContextAccessor.HttpContext.User.Identity.Name.Split(new string[] { @"\" }, StringSplitOptions.RemoveEmptyEntries);


                if (!string.IsNullOrEmpty(details))
                {
                    memberDetails = new DOMemberbhPlanEligibilitySearchCriteria();
                    memberDetails = JsonConvert.DeserializeObject<DOMemberbhPlanEligibilitySearchCriteria>(details);
                    #region MemberEligibilty empty request handling on 02/15/2021 by Sunil/Naseer/Pallavi
                    bool isValidRequest = false;
                    if (!string.IsNullOrEmpty(memberDetails.packageId)
                        || !string.IsNullOrEmpty(memberDetails.groupId)) 
                    {
                        isValidRequest = true;
                    }
                    if (!isValidRequest)
                    {
                        return BadRequest(new { error = "bad request" });
                    }
                    #endregion
                    ////if (memberDetails != null)
                    ////{
                    ////    LoggedInUserId = memberDetails.UserId;
                    ////}
                    memberSearchDetails = new List<DOGPSMemberDetails>();
                    //BOHCPMemberDetails objBOHCPMemberDetails = new BOHCPMemberDetails(_objConfiguration);
                    HCPCDBMemberMethods objHCPMemberMethods = new HCPCDBMemberMethods(_memoryCacheHelper, _objConfiguration);
                    dOMemberBenifitDetails = objHCPMemberMethods.GetCbmsMemberBenifitDetails(memberDetails);

                }
                return Ok(dOMemberBenifitDetails);
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
        //GetCbmsAltAccountId
        public ActionResult GetCbmsAltAccountDetails(string details = null)
        {
            DOMemberbhPlanEligibilitySearchCriteria memberDetails = null;
            DOGPSMemberDetails dOGPSMemberDetails = null;
            List<DOCbmsXRefAltAccountIdDetails> dOMemberBenifitDetails = null;
            List<DOGPSMemberDetails> memberSearchDetails = null;
            source = BOCommon.GetRefererURI(Request);
            try
            {
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                //_httpContextAccessor.HttpContext.User.Identity.Name.Split(new string[] { @"\" }, StringSplitOptions.RemoveEmptyEntries);


                if (!string.IsNullOrEmpty(details))
                {
                    memberDetails = new DOMemberbhPlanEligibilitySearchCriteria();
                    memberDetails = JsonConvert.DeserializeObject<DOMemberbhPlanEligibilitySearchCriteria>(details);
                    #region MemberEligibilty empty request handling on 02/15/2021 by Sunil/Naseer/Pallavi
                    bool isValidRequest = false;
                    if (!string.IsNullOrEmpty(memberDetails.packageId) || memberDetails.accountId !=0)
                    {
                        isValidRequest = true;
                    }
                    if (!isValidRequest)
                    {
                        return BadRequest(new { error = "bad request" });
                    }
                    #endregion
                    ////if (memberDetails != null)
                    ////{
                    ////    LoggedInUserId = memberDetails.UserId;
                    ////}
                    memberSearchDetails = new List<DOGPSMemberDetails>();
                    //BOHCPMemberDetails objBOHCPMemberDetails = new BOHCPMemberDetails(_objConfiguration);
                    HCPCDBMemberMethods objHCPMemberMethods = new HCPCDBMemberMethods(_memoryCacheHelper, _objConfiguration);
                    dOMemberBenifitDetails = objHCPMemberMethods.GetCbmsAltAccountId(memberDetails);

                }
                return Ok(dOMemberBenifitDetails);
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
