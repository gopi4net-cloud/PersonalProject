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

namespace ANGDDEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class ETSATSAPIController : ControllerBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        ExceptionTypes _resException;
        public string _username = string.Empty;
        public string ModuleName = "ETSATSAPIController";
        private readonly DOConfiguration _objConfiguration;
        private readonly BOCommon _objBOCommon;
        private readonly ICacheService _cache;

        public ETSATSAPIController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, BOCommon bOCommon, ICacheService cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _username = httpContextAccessor.HttpContext.User.Identity.Name;
            _objBOCommon = bOCommon;
            _cache = cache;
        }

        [HttpPost]
        public ActionResult GetDuplicateCases(DOETSCaseRequest objDOETSCaseRequest)
        {
            List<DODuplicateCase> lstObjDOCaseETS = new List<DODuplicateCase>();
            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", " ", "GetDuplicateCases", _username);

            try
            {
                BODuplicateCheck objBODuplicateCheck = new BODuplicateCheck(_objConfiguration, _cache, _objBOCommon);
                _resException = objBODuplicateCheck.GetSuspectDuplicateCasesInETS(objDOETSCaseRequest, out lstObjDOCaseETS);
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
            return Ok(lstObjDOCaseETS);
        }

        [HttpPost]
        public ActionResult GetDuplicateCasesSB(DOETSCaseRequest objDOETSCaseRequest)
        {
            List<DODuplicateCase> lstObjDOCaseETS = new List<DODuplicateCase>();
            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", " ", "GetDuplicateCases", _username);

            try
            {
                BODuplicateCheck objBODuplicateCheck = new BODuplicateCheck(_objConfiguration, _cache, _objBOCommon);
                _resException = objBODuplicateCheck.GetSuspectDuplicateCasesInETSForSB(objDOETSCaseRequest, out lstObjDOCaseETS);
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
            return Ok(lstObjDOCaseETS);
        }

        [HttpPost]
        public ActionResult ETSCreateCase(DOETSCreateCaseRequest objDOETSCreateCaseRequest)
        {

            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", " ", "ETSCreateCase", _username);

            DOETSCreateCaseResponse objDOETSCreateCaseResponse = new DOETSCreateCaseResponse();

            try
            {
                BOCreateCase objBOCreateCase = new BOCreateCase(_objConfiguration);
                _resException = objBOCreateCase.ETSCreateCase(objDOETSCreateCaseRequest, out objDOETSCreateCaseResponse);
                if (_resException != ExceptionTypes.Success)
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

            return Ok(objDOETSCreateCaseResponse);
        }

        [HttpPost]
        public ActionResult ETSCreateCase1()
        {
            DOETSCreateCaseRequest objDOETSCreateCaseRequest = null;
            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request)??string.Empty;
            _objBOCommon.LogError(location, source, 10001, "Controller Method", " ", "ETSCreateCase", _username);

            DOETSCreateCaseResponse objDOETSCreateCaseResponse = new DOETSCreateCaseResponse();

            try
            {


                var formData = _httpContextAccessor?.HttpContext?.Request?.HasFormContentType == true
                    ? _httpContextAccessor.HttpContext.Request.Form
                    : null;

                var formArray = formData?.ToArray() ?? Array.Empty<KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>>();

                if (formArray.Length == 0)
                {
                    return BadRequest("No form data received.");
                }

                string strObjDOETSCreateCaseRequest = formArray[0].Value;
                if (string.IsNullOrWhiteSpace(strObjDOETSCreateCaseRequest))
                {
                    return BadRequest("Form data is empty.");
                }

                objDOETSCreateCaseRequest = JsonConvert.DeserializeObject<DOETSCreateCaseRequest>(strObjDOETSCreateCaseRequest);
                if (objDOETSCreateCaseRequest?.oRequest == null)
                {
                    return BadRequest("Invalid request: oRequest is null.");
                }

                BOCreateCase objBOCreateCase = new BOCreateCase(_objConfiguration);
                _resException = objBOCreateCase.ETSCreateCase(objDOETSCreateCaseRequest, out objDOETSCreateCaseResponse);
                if (_resException != ExceptionTypes.Success)
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
                    _objBOCommon.LogError(location, source, 10001, ex.Message, " ", ex.StackTrace?.ToString() ?? string.Empty, _username);
                }
                return BadRequest(ex.Message);
            }

            return Ok(objDOETSCreateCaseResponse);
        }
        [HttpPost]
        public ActionResult GetDuplicateCases1()           
        {
            DOETSCaseRequest objDOETSCaseRequest = null;
            var formData = _httpContextAccessor.HttpContext.Request.Form;
            string strObjDOETSCaseRequest = _httpContextAccessor.HttpContext.Request.Form.ToArray()[0].Value;
            objDOETSCaseRequest = JsonConvert.DeserializeObject<DOETSCaseRequest>(strObjDOETSCaseRequest);

            List<DODuplicateCase> lstObjDOCaseETS = new List<DODuplicateCase>();
            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", " ", "GetDuplicateCases", _username);

            try
            {
                BODuplicateCheck objBODuplicateCheck = new BODuplicateCheck(_objConfiguration, _cache, _objBOCommon);
                _resException = objBODuplicateCheck.GetSuspectDuplicateCasesInETS(objDOETSCaseRequest, out lstObjDOCaseETS);
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
            return Ok(lstObjDOCaseETS);
        }



        [HttpGet]
        public ActionResult getdata()
        {
            return Ok(_objConfiguration.AppSettings.ETS_PP_CreateCaseURL);
        }
    }
}
