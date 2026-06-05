using ANGDDEAPI.Common;
using ANGDDEAPIBO;
using ANGDDEAPIDO;
using ANGDDEAPIDO.Interface;
using ANGDDEAPIFoundation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using Newtonsoft.Json;
using ANGDDEAPIDO.UNET;
using log4net.Layout;
using System.Collections.Generic;
using NuGet.Protocol.Core.Types;
using ANGDDEAPIDO.PICTS;
namespace ANGDDEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class PICTSController : ControllerBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        ExceptionTypes _resException;
        public string _username = string.Empty;
        public string ModuleName = "PICTSController";
        private DOConfiguration _objConfiguration;
        private IMemoryCacheHelper _memoryCacheHelper;
        private BOCommon _objBOCommon;
        private ICacheService _cache;
        public PICTSController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _memoryCacheHelper = memoryCacheHelper;
            _objBOCommon = bOCommon;
            _cache = cache;
        }

        [HttpGet]
        public ActionResult GetCaseSummary(string caseId = null)
        {
            DOCaseSummaryResponse objDOCaseSummaryResponse = new DOCaseSummaryResponse();
            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "GetCaseSummary", _username);

            try
            {
                PICTSMethods objPicts = new PICTSMethods(_memoryCacheHelper, _objConfiguration);
                _resException = objPicts.GetCaseSummaryDetails(caseId, out objDOCaseSummaryResponse);
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
            return Ok(objDOCaseSummaryResponse);
        }
        [HttpGet]
        public ActionResult GetNotesFoaCase(string caseId = null)
        {
            DOPICTSNotesResponse objDONotesResponse = new DOPICTSNotesResponse();
            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "GetNotesFoaCase", _username);

            try
            {
                PICTSMethods objPicts = new PICTSMethods(_memoryCacheHelper, _objConfiguration);
                _resException = objPicts.GetNotesForACaseDetails(caseId, out objDONotesResponse);
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
            return Ok(objDONotesResponse);
        }

        [HttpGet]
        public ActionResult GetContactsBasedOnCaseId(string caseId = null)
        {
            DOPICTSContactsResponse objDOContactsResponse = new DOPICTSContactsResponse();
            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "GetContactsBasedOnCaseId", _username);

            try
            {
                PICTSMethods objPicts = new PICTSMethods(_memoryCacheHelper, _objConfiguration);
                _resException = objPicts.GetContractBasedOnCaseID(caseId, out objDOContactsResponse);
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
            return Ok(objDOContactsResponse);
        }
        [HttpGet]
        public ActionResult GetEDROfCase(string caseId = null)
        {
            DOPICTSEDRResponse objDOEDRResponse = new DOPICTSEDRResponse();
            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "GetEDROfCase", _username);

            try
            {
                PICTSMethods objPicts = new PICTSMethods(_memoryCacheHelper, _objConfiguration);
                _resException = objPicts.GetEDR(caseId, out objDOEDRResponse);
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
            return Ok(objDOEDRResponse);
        }
        [HttpGet]
        public ActionResult GetTasksOfCase(string caseId = null)
        {
            DOPICTSTaskResponse objDOTasksResponse = new DOPICTSTaskResponse();
            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "GetTasksOfCase", _username);

            try
            {
                PICTSMethods objPicts = new PICTSMethods(_memoryCacheHelper, _objConfiguration);
                _resException = objPicts.GetTaskForaCase(caseId, out objDOTasksResponse);
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
            return Ok(objDOTasksResponse);
        }
        [HttpGet]
        public ActionResult GetAllClaimsDataForCase(string caseId = null)
        {
            DOPICTSClaimsResponse objDOClaimsResponse = new DOPICTSClaimsResponse();
            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "GetAllClaimsDataForCase", _username);

            try
            {
                PICTSMethods objPicts = new PICTSMethods(_memoryCacheHelper, _objConfiguration);
                _resException = objPicts.GetAllClaimDataBasedOnCaseID(caseId, out objDOClaimsResponse);
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
            return Ok(objDOClaimsResponse);
        }
        [HttpGet]
        public ActionResult GetInventoryForCase(string caseId = null)
        {
            DOPICTSInventoryResponse objDOInventoryResponse = new DOPICTSInventoryResponse();
            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "GetInventoryForCase", _username);

            try
            {
                PICTSMethods objPicts = new PICTSMethods(_memoryCacheHelper, _objConfiguration);
                _resException = objPicts.GetCaseInventoryBasedOnCaseID(caseId, out objDOInventoryResponse);
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
            return Ok(objDOInventoryResponse);
        }

        [HttpGet]
        public ActionResult GetSearchCase(string memberId = null, string dateOfServiceStartDate = null, string dateOfServiceEndDate = null,
               string tin = null, string claimid = null)
        {

            DOPICTSSearchCaseResponse objDOCaseSearchResponse = new DOPICTSSearchCaseResponse();
            DOPICTSSearchCaseRequest objDOCaseRequest = new DOPICTSSearchCaseRequest()
            {
                msId = null,
                memberId = memberId,
                tin = tin,
                dateOfServiceStartDate = dateOfServiceStartDate,
                dateOfServiceEndDate = dateOfServiceEndDate,
                claimNumber = claimid,
                caseId = 0,
                workflowType = null,
                page = 1

            };

            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "GetSearchCase", _username);

            try
            {
                PICTSMethods objPicts = new PICTSMethods(_memoryCacheHelper, _objConfiguration);
                _resException = objPicts.GetSearchCaseDetails(objDOCaseRequest, out objDOCaseSearchResponse);
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
            return Ok(objDOCaseSearchResponse);
        }

    }
}
