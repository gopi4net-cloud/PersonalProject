using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ANGDDEAPIBO;
using ANGDDEAPIDO;
using Microsoft.AspNetCore.Http;
using ANGDDEAPI.Common;
using System.Net;
using Newtonsoft.Json;
using ANGDDEAPIDO.Interface;

namespace ANGDDEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class PASController : ControllerBase
    {
        public static long LoggedInUserId = 0;
        public string _username = string.Empty;
        string source = string.Empty;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DOConfiguration _objConfiguration;
        private readonly IMemoryCacheHelper _memoryCacheHelper;
        //public BOPASDetails _objBOPASDetails = null;
        public readonly BOCommon _objBOCommon;
        private readonly ICacheService _cache;

        public PASController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _memoryCacheHelper = memoryCacheHelper;
            _objBOCommon = bOCommon;
            _cache = cache;
        }


        [HttpGet]
        public ActionResult GetPriorAuthSearchDetails(string details = null)
        {
            DOSearchPriorAuthorizationRequest AuthDetails = null;
            String Message = string.Empty;
            List<DOAuthInfo> LstDOAuthInfo = null;
            source = BOCommon.GetRefererURI(Request);
            try
            {
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Request", details, _username);

                if (!string.IsNullOrEmpty(details))
                {
                    AuthDetails = new DOSearchPriorAuthorizationRequest();
                    AuthDetails = JsonConvert.DeserializeObject<DOSearchPriorAuthorizationRequest>(details);
                    PASMethods objPASMethods = new PASMethods(_memoryCacheHelper, _objConfiguration);
                    // DOSearchPriorAuthorizationRequest objDOSearchPriorAuthorizationRequest = new DOSearchPriorAuthorizationRequest();
                    // Provider objprovider = new Provider();
                    // objDOSearchPriorAuthorizationRequest.RequestId = Guid.NewGuid().ToString();
                    // objDOSearchPriorAuthorizationRequest.Application = "ATS";
                    //objDOSearchPriorAuthorizationRequest.DateFrom = "2021-08-30";
                    //objDOSearchPriorAuthorizationRequest.DateTo = "2021-09-06";
                    //objprovider.NPI = "1225229032";
                    //objDOSearchPriorAuthorizationRequest.Provider = objprovider;
                    //objPASMethods.PriorAuthDetails(objDOSearchPriorAuthorizationRequest, out LstDOAuthInfo, out Message);

                    if (AuthDetails != null && AuthDetails.Member != null && !string.IsNullOrEmpty(AuthDetails.Member.MemberDOB))
                    {
                        AuthDetails.Member.MemberDOB = Convert.ToDateTime(AuthDetails.Member.MemberDOB).ToString("yyyyMMdd");
                    }
                    objPASMethods.PriorAuthDetails(AuthDetails, out LstDOAuthInfo, out Message);
                    _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Response", Message, _username);

                }
                if (LstDOAuthInfo.Count > 0)
                {
                    return Ok(LstDOAuthInfo);
                }
                else
                {
                    return Ok(Message);
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

        [HttpGet]
        public ActionResult GetAuthDetails(string details = null)
        {
            DOCommonAuthRequest AuthDetails = null;
            List<DOAuthInfo> LstDOAuthInfo = null;
            DOAuthInfo objDOAuthInfo = new DOAuthInfo();
            source = BOCommon.GetRefererURI(Request);
            String Message = string.Empty;
            try
            {
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Request", details, _username);

                if (!string.IsNullOrEmpty(details))
                {
                    AuthDetails = new DOCommonAuthRequest();
                    AuthDetails = JsonConvert.DeserializeObject<DOCommonAuthRequest>(details);
                    PASMethods objPASMethods = new PASMethods(_memoryCacheHelper, _objConfiguration);
                    AuthDetails.RequestId = Guid.NewGuid().ToString();
                    AuthDetails.Application = "ATS";
                    DoReadpriorauthorizationRequest objDoReadpriorauthorizationRequest = new DoReadpriorauthorizationRequest();
                    objDoReadpriorauthorizationRequest.RequestId = AuthDetails.RequestId;
                    objDoReadpriorauthorizationRequest.Application = AuthDetails.Application;
                    objDoReadpriorauthorizationRequest.CaseID = AuthDetails.CaseID;
                    objDoReadpriorauthorizationRequest.IsFormularyInfoNeeded = true;
                    objDoReadpriorauthorizationRequest.IsClinicalGuidelinesInfoNeeded = true;
                    objPASMethods.Readpriorauths(objDoReadpriorauthorizationRequest, ref objDOAuthInfo, out Message);
                    if (Message == "Success")
                    {
                        if (objDOAuthInfo != null)
                        {
                            DOGetCaseNotesRequest objDOGetCaseNotesRequest = new DOGetCaseNotesRequest();
                            objDOGetCaseNotesRequest.CaseID = AuthDetails.CaseID;
                            objDOGetCaseNotesRequest.IncludeInboundCorrAttachments = "true";
                            RequestHeader requestheader = new RequestHeader();
                            requestheader.Application = AuthDetails.Application;
                            objDOGetCaseNotesRequest.RequestHeader = requestheader;
                            objPASMethods.GetCaseNotes(objDOGetCaseNotesRequest, ref objDOAuthInfo);
                            objPASMethods.GetCaseAttachments(objDOGetCaseNotesRequest, ref objDOAuthInfo);

                            //Second call to fetch reason code
                            if (!string.IsNullOrEmpty(objDOAuthInfo.MemberID) && objDOAuthInfo.MemberDOB != null)
                            {
                                try
                                {
                                    string SearchMessage = string.Empty;
                                    List<DOAuthInfo> LstDOAuthInfoMember = new List<DOAuthInfo>();
                                    AuthMember Member = new AuthMember();
                                    DOSearchPriorAuthorizationRequest AuthDetailsMember = new DOSearchPriorAuthorizationRequest();
                                    Member.MemberDOB = Convert.ToDateTime(objDOAuthInfo.MemberDOB).ToString("yyyyMMdd");
                                    Member.MemberID = objDOAuthInfo.MemberID;
                                    AuthDetailsMember.Member = Member;
                                    AuthDetailsMember.Application = AuthDetails.Application;
                                    objPASMethods.PriorAuthDetails(AuthDetailsMember, out LstDOAuthInfo, out SearchMessage);
                                    if (LstDOAuthInfo != null && LstDOAuthInfo.Count > 0)
                                    {
                                        DOAuthInfo SearchedAuth = new DOAuthInfo();
                                        SearchedAuth = LstDOAuthInfo.Where(Item => Item.AuthRefCaseID == AuthDetails.CaseID).FirstOrDefault();
                                        if (SearchedAuth != null)
                                        {
                                            objDOAuthInfo.ReasonCode = SearchedAuth.ReasonCode;
                                        }
                                    }
                                }
                                catch (Exception)
                                {
                                     
                                } 
                            }
                        }
                        LstDOAuthInfo = new List<DOAuthInfo>();
                        LstDOAuthInfo.Add(objDOAuthInfo);
                    }
                }
                _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Response", Message, _username);

                if (LstDOAuthInfo != null)
                {
                    return Ok(LstDOAuthInfo);
                }
                else
                {
                    return Ok(Message);
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
        [HttpGet]
        public ActionResult GetCaseNotes(string details = null)
        {
            DOGetCaseNotesRequest AuthDetails = null;
            DOAuthInfo objDOAuthInfo = null;
            source = BOCommon.GetRefererURI(Request);
            try
            {
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Request", details, _username);

                if (!string.IsNullOrEmpty(details))
                {
                    AuthDetails = new DOGetCaseNotesRequest();
                    AuthDetails = JsonConvert.DeserializeObject<DOGetCaseNotesRequest>(details);
                    PASMethods objPASMethods = new PASMethods(_memoryCacheHelper, _objConfiguration);
                    //DOGetCaseNotesRequest objDOGetCaseNotesRequest = new DOGetCaseNotesRequest();
                    // objDOGetCaseNotesRequest.CaseID = "PA-30128460";
                    //objDOGetCaseNotesRequest.CaseID = "PA-20127578";
                    //RequestHeader requestheader = new RequestHeader();
                    //requestheader.Application = "ABC";
                    //objDOGetCaseNotesRequest.RequestHeader = requestheader;
                    objPASMethods.GetCaseNotes(AuthDetails, ref objDOAuthInfo);
                    //objPASMethods.GetCaseAttachments(objDOGetCaseNotesRequest, out LstDOAuthInfo);
                    _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Response", BOCommon.JsonConvertObjectToString(objDOAuthInfo), _username);

                }
                return Ok(objDOAuthInfo);
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
        public ActionResult GetCaseAttachement(string details = null)
        {
            DOGetCaseNotesRequest AuthDetails = null;
            DOAuthInfo objDOAuthInfo = null;
            source = BOCommon.GetRefererURI(Request);
            try
            {
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Request", details, _username);

                if (!string.IsNullOrEmpty(details))
                {
                    AuthDetails = new DOGetCaseNotesRequest();
                    AuthDetails = JsonConvert.DeserializeObject<DOGetCaseNotesRequest>(details);
                    PASMethods objPASMethods = new PASMethods(_memoryCacheHelper, _objConfiguration);
                    //DOGetCaseNotesRequest objDOGetCaseNotesRequest = new DOGetCaseNotesRequest();
                    //objDOGetCaseNotesRequest.CaseID = "PA-20127578";
                    //RequestHeader requestheader = new RequestHeader();
                    //requestheader.Application = "ABC";
                    // objDOGetCaseNotesRequest.RequestHeader = requestheader;
                    objPASMethods.GetCaseAttachments(AuthDetails, ref objDOAuthInfo);
                    _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Response", BOCommon.JsonConvertObjectToString(objDOAuthInfo), _username);

                }
                return Ok(objDOAuthInfo);
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
        public ActionResult Readpriorauths(string details = null)
        {
            DoReadpriorauthorizationRequest AuthDetails = null;
            List<DOAuthInfo> LstDOAuthInfo = null;
            DOAuthInfo objDOAuthInfo = new DOAuthInfo();
            string Message = string.Empty;
            source = BOCommon.GetRefererURI(Request);
            try
            {
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Request", details, _username);

                if (!string.IsNullOrEmpty(details))
                {
                    AuthDetails = new DoReadpriorauthorizationRequest();
                    AuthDetails = JsonConvert.DeserializeObject<DoReadpriorauthorizationRequest>(details);
                    PASMethods objPASMethods = new PASMethods(_memoryCacheHelper, _objConfiguration);
                    // DoReadpriorauthorizationRequest objDoReadpriorauthorizationRequest = new DoReadpriorauthorizationRequest();
                    //objDOSearchPriorAuthorizationRequest.Provider.Npi = 1225229032;
                    //objDoReadpriorauthorizationRequest.Application = "ABC";
                    //objDoReadpriorauthorizationRequest.RequestId = "S";
                    //objDoReadpriorauthorizationRequest.CaseID = "PA-30128460";
                    AuthDetails.IsFormularyInfoNeeded = true;
                    AuthDetails.IsClinicalGuidelinesInfoNeeded = true;

                    objPASMethods.Readpriorauths(AuthDetails, ref objDOAuthInfo, out Message);
                }
                if (Message == "Success")
                {
                    return Ok(objDOAuthInfo);
                }
                else
                {
                    return Ok(Message);
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

    }
}
