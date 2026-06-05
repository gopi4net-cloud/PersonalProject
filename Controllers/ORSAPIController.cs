using ANGDDEAPI.Common;
using ANGDDEAPIBO;
using ANGDDEAPIDO;
using ANGDDEAPIDO.Interface;
using ANGDDEAPIDO.ORS;
using ANGDDEAPIFoundation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ANGDDEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class ORSAPIController : ControllerBase
    {
        ExceptionTypes _resException;
        public static long LoggedInUserId = 0;
        public string _username = string.Empty;
        public string source = string.Empty;
        public string ModuleName = "ORSAPIController";

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DOConfiguration _objConfiguration;
        private readonly IMemoryCacheHelper _memoryCacheHelper;
        private readonly BOCommon _objBOCommon;
        private readonly ICacheService _cache; 

        public ORSAPIController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _memoryCacheHelper = memoryCacheHelper;
            _objBOCommon = bOCommon;
            _cache = cache;
        }

        [HttpPost]
        public ActionResult ReOpenORSCase([FromForm] DOReOpenORSCase objDOReOpenORSCase)
        {
            //DOReOpenORSCase objDOReOpenORSCase = new DOReOpenORSCase(MailBox, IssueID);

            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "ReOpenORSCase", _username);

            try
            {
                ORSMethods objORS = new ORSMethods(_memoryCacheHelper, _objConfiguration, _cache);
                /*objDOReOpenORSCase.MailBox = MailBox;
                objDOReOpenORSCase.IssueID = IssueID;*/
                if (objDOReOpenORSCase.MailBox != "Close ORS")
                {
                    _resException = objORS.ReOpenORSCase(ref objDOReOpenORSCase);
                }
                else
                {
                    _resException = objORS.CloseOrsCase(ref objDOReOpenORSCase);
                }
                if (_resException != ExceptionTypes.Success && _resException != ExceptionTypes.ZeroRecords)
                {
                    return Ok(objDOReOpenORSCase);
                }
            }
            catch (Exception ex)
            {
                if (source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(0, location, ex.Message, ex.ToString());
                }
                else
                {
                    _objBOCommon.LogError(location, source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                }
                return BadRequest(ex.Message);
            }
            return Ok(objDOReOpenORSCase);
        }

        [HttpPost]
        public ActionResult CreateORSCase([FromForm] DOORSIssueRequest objDOORSIssueRequest)
        {           
            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "CreateORSCase", _username);
            try
            {
                if(objDOORSIssueRequest.MemberDOBStr != null)
                {
                    objDOORSIssueRequest.MemberDOB = Convert.ToDateTime(objDOORSIssueRequest.MemberDOBStr);
                }
            }
            catch(Exception ex)
            {
                _objBOCommon.LogError(location, source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                return BadRequest(ex.Message);
            }
            DOORSIssueResponse objCIRes = new DOORSIssueResponse();
            try
            {                
                ORSMethods objORS = new ORSMethods(_memoryCacheHelper, _objConfiguration, _cache);
                _resException = objORS.CreateORSCase(objDOORSIssueRequest, out objCIRes);
                if (_resException != ExceptionTypes.Success && _resException != ExceptionTypes.ZeroRecords)
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                if (source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(0, location, ex.Message, ex.ToString());
                }
                else
                {
                    _objBOCommon.LogError(location, source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                }
                return BadRequest(ex.Message);
            }
            return Ok(objCIRes);
        }

        [HttpGet]
        public ActionResult GetORSCase(string IssueID)
        {
            DOReOpenORSCase objDOReOpenORSCase = new DOReOpenORSCase();

            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "GetORSCase", _username);
            DOGetIssueResponse objGIRes = new DOGetIssueResponse();
            try
            {
                ORSMethods objORS = new ORSMethods(_memoryCacheHelper, _objConfiguration, _cache);
                objDOReOpenORSCase.IssueID = IssueID;

                _resException = objORS.GetORSCase(ref objDOReOpenORSCase, out objGIRes);
                if (_resException != ExceptionTypes.Success && _resException != ExceptionTypes.ZeroRecords)
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                if (source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(0, location, ex.Message, ex.ToString());
                }
                else
                {
                    _objBOCommon.LogError(location, source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                }
                return BadRequest(ex.Message);
            }
            return Ok(objGIRes);
        }

        [HttpGet]
        public ActionResult GetAllORSCasesInAOrsBox(string ORSBox = "261 TRI AH4",bool SendMinData = true)
        {
            DOReOpenORSCase objDOReOpenORSCase = new DOReOpenORSCase();
            List<DOORSReport> LstdOORSReport = new List<DOORSReport>();
            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "GetORSCase", _username);
            DOGetIssueResponse objGIRes = new DOGetIssueResponse();
            try
            {
                ORSMethods objORS = new ORSMethods(_memoryCacheHelper, _objConfiguration, _cache);
                DOGetAllIssuesResponse objGAllIRes = new DOGetAllIssuesResponse();
                _resException = objORS.getAllOrsCases(ref objDOReOpenORSCase, out LstdOORSReport,out objGAllIRes, SendMinData, ORSBox);
                if(SendMinData && _resException == ExceptionTypes.Success)
                {
                    return Ok(objGAllIRes);
                }
                if (_resException != ExceptionTypes.Success && _resException != ExceptionTypes.ZeroRecords)
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                if (source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(0, location, ex.Message, ex.ToString());
                }
                else
                {
                    _objBOCommon.LogError(location, source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                }
                return BadRequest(ex.Message);
            }
            return Ok(LstdOORSReport);
        }
        [HttpPost]
        public ActionResult CloseORSCase([FromForm] DOReOpenORSCase objDOReOpenORSCase)
        {           

            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "GetORSCase", _username);
            DOGetIssueResponse objGIRes = new DOGetIssueResponse();
            try
            {
                ORSMethods objORS = new ORSMethods(_memoryCacheHelper, _objConfiguration, _cache);

                _resException = objORS.CloseOrsCase(ref objDOReOpenORSCase);
                if (_resException != ExceptionTypes.Success && _resException != ExceptionTypes.ZeroRecords)
                {
                    return Ok(objDOReOpenORSCase);
                }
            }
            catch (Exception ex)
            {
                if (source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(0, location, ex.Message, ex.ToString());
                }
                else
                {
                    _objBOCommon.LogError(location, source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                }
                return BadRequest(ex.Message);
            }
            return Ok(objDOReOpenORSCase);
        }

        public ActionResult AssignORSCase(string IssueID)
        {
            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "GetORSCase", _username);
            DOGetIssueResponse objGIRes = new DOGetIssueResponse();
            DOReOpenORSCase objDOReOpenORSCase = new DOReOpenORSCase();
            try
            {
                
                ORSMethods objORS = new ORSMethods(_memoryCacheHelper, _objConfiguration, _cache);
                objDOReOpenORSCase.IssueID = IssueID;
                _resException = objORS.AssignOrsCase(ref objDOReOpenORSCase);
                if (_resException != ExceptionTypes.Success && _resException != ExceptionTypes.ZeroRecords)
                {
                    return Ok(objDOReOpenORSCase);
                }
            }
            catch (Exception ex)
            {
                if (source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(0, location, ex.Message, ex.ToString());
                }
                else
                {
                    _objBOCommon.LogError(location, source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                }
                return BadRequest(ex.Message);
            }
            return Ok(objDOReOpenORSCase);
        }
    }
}
