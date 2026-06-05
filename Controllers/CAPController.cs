using ANGDDEAPI.Common;
using ANGDDEAPIBO;
using ANGDDEAPIDO;
using ANGDDEAPIDO.CAPS;
using ANGDDEAPIDO.Interface;
using ANGDDEAPIFoundation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
namespace ANGDDEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class CAPController : ControllerBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        ExceptionTypes _resException;
        public string _username = string.Empty;
        public string ModuleName = "CAPController";
        private DOConfiguration _objConfiguration;
        private IMemoryCacheHelper _memoryCacheHelper;
        private BOCommon _objBOCommon;
        private ICacheService _cache;
        public CAPController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _memoryCacheHelper = memoryCacheHelper;
            _objBOCommon = bOCommon;
            _cache = cache;
        }

        [HttpGet]
        public ActionResult GetClaimCases(string org, string claimid)
        {
            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            List<string> caseid;
            try
            {
                CAPMethods objCap = new CAPMethods(_memoryCacheHelper, _objConfiguration);
                _resException = objCap.GetClaimCases(org?.ToUpper(), claimid, out caseid);
                if (_resException != ExceptionTypes.Success && _resException != ExceptionTypes.ZeroRecords)
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                _objBOCommon.ErrorLog(0, location, ex.Message, ex.ToString());
                return BadRequest(ex.Message);
            }
            return Ok(caseid);
        }

        [HttpPost]
        public ActionResult UpdateCAPCaseStatus(string org, string caseid, DOUpdateCaseStatusRequest objDOUpdateCaseStatusRequest)
        {
            DOCAPSCaseStatusResponse objDOCAPSCaseStatusResponse = new DOCAPSCaseStatusResponse();
            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            string message;
            try
            {
                CAPMethods objCap = new CAPMethods(_memoryCacheHelper, _objConfiguration);
                _resException = objCap.UpdateCAPCaseStatus(org?.ToUpper(), caseid, objDOUpdateCaseStatusRequest, out objDOCAPSCaseStatusResponse, out message);
                if (_resException != ExceptionTypes.Success)
                {
                    return BadRequest(message);
                }
            }
            catch (Exception ex)
            {
                _objBOCommon.ErrorLog(0, location, ex.Message, ex.ToString());
                return BadRequest(ex.Message);
            }
            return Ok(objDOCAPSCaseStatusResponse);
        }

        [HttpPost]
        public ActionResult AddCAPSNotes(string org, string caseid, DOAddCapsNotesRequest objDOAddCapsNotesRequest)
        {
            DOCAPSCaseStatusResponse objDOCAPSCaseStatusResponse = new DOCAPSCaseStatusResponse();
            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            string message;
            // _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "GetCaseSummary", _username);

            try
            {
                CAPMethods objCap = new CAPMethods(_memoryCacheHelper, _objConfiguration); 
                _resException = objCap.AddCAPSNotes(org?.ToUpper(), caseid, objDOAddCapsNotesRequest, out message);
                if (_resException != ExceptionTypes.Success && _resException != ExceptionTypes.ZeroRecords)
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                _objBOCommon.ErrorLog(0, location, ex.Message, ex.ToString());
                return BadRequest(ex.Message);
            }
            return Ok(message);
        }

        [HttpPost]
        public ActionResult AddCAPSDocument(string org, string caseid, DOAddCapsDocRequest objDOAddCapsDocRequest)
        {
            string message;
            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            // _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "GetCaseSummary", _username);

            try
            {
                CAPMethods objCap = new CAPMethods(_memoryCacheHelper, _objConfiguration);
                _resException = objCap.AddCAPSDocument(org?.ToUpper(), caseid, objDOAddCapsDocRequest, out message);
                if (_resException != ExceptionTypes.Success && _resException != ExceptionTypes.ZeroRecords)
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                _objBOCommon.ErrorLog(0, location, ex.Message, ex.ToString());
                return BadRequest(ex.Message);
            }
            return Ok(message);
        }


        [HttpPost]
        public ActionResult ReportCaseStatus(string org, string caseid, DOReportCaseStatusRequest objDOUpdateCaseStatusRequest)
        {
            DOCAPSCaseStatusResponse objDOCAPSCaseStatusResponse = new DOCAPSCaseStatusResponse();
            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            string message;
            try
            {
                CAPMethods objCap = new CAPMethods(_memoryCacheHelper, _objConfiguration);
                _resException = objCap.ReportCAPCaseStatus(org?.ToUpper(), caseid, objDOUpdateCaseStatusRequest, out message);
                if (_resException != ExceptionTypes.Success)
                {
                    return BadRequest(message);
                }
            }
            catch (Exception ex)
            {
                _objBOCommon.ErrorLog(0, location, ex.Message, ex.ToString());
                return BadRequest(ex.Message);
            }
            return Ok(objDOCAPSCaseStatusResponse);
        }

        [HttpGet]
        public ActionResult GetCaseById(string org, string caseid)
        {
            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            List<DOCAPSCaseDetailResponse> lstDOCAPSCaseDetailResponse;
            try
            {
                CAPMethods objCap = new CAPMethods(_memoryCacheHelper, _objConfiguration);
                _resException = objCap.GetCaseById(org?.ToUpper(), caseid, out lstDOCAPSCaseDetailResponse);
                if (_resException != ExceptionTypes.Success && _resException != ExceptionTypes.ZeroRecords)
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                _objBOCommon.ErrorLog(0, location, ex.Message, ex.ToString());
                return BadRequest(ex.Message);
            }
            return Ok(lstDOCAPSCaseDetailResponse);
        }
    }
}