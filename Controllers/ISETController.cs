using ANGDDEAPI.Common;
using ANGDDEAPIBO;
using ANGDDEAPIDO;
using ANGDDEAPIDO.Interface;
using ANGDDEAPIDO.ISET;
using ANGDDEAPIFoundation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;

namespace ANGDDEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class ISETController : ControllerBase
    {
        public static long LoggedInUserId = 0;
        public string _username = string.Empty;
        string source = string.Empty;
        ExceptionTypes _resException;
        public string ModuleName = "ISETController";
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DOConfiguration _objConfiguration;
        private readonly IMemoryCacheHelper _memoryCacheHelper;
        public readonly BOCommon _objBOCommon;
        public ISETController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _memoryCacheHelper = memoryCacheHelper;
            _objBOCommon = bOCommon;

        }

        [HttpGet]
        public ActionResult GetISETEhealthEobDetails(string ISETRequest = null)
        {
            ResponseISET objDOISETResponse = new ResponseISET();
            DOISETEhealthEobRequest objDOISETRequest = JsonConvert.DeserializeObject<DOISETEhealthEobRequest>(ISETRequest);

            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "GetISETEhealthEobDetails", _username);

            try
            {
               ISETMethods objEPMPMethods = new ISETMethods(_memoryCacheHelper, _objConfiguration);
                _resException = objEPMPMethods.GetISETEhealthEobRead(objDOISETRequest, out objDOISETResponse);
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
            return Ok(objDOISETResponse);
        }


        [HttpGet]
        public ActionResult GetISETMedicalClaimDetails(string ISETMedicalClaimRequest = null)
        {
            DOMedicalClaimResponse  dOMedicalClaimResponse = new DOMedicalClaimResponse();
            DOMedicalClaimRequest  dOMedicalClaimRequest = JsonConvert.DeserializeObject<DOMedicalClaimRequest>(ISETMedicalClaimRequest);

            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "GetISETMedicalClaimDetails", _username);

            try
            {
                ISETMethods  iSETMethods = new ISETMethods(_memoryCacheHelper, _objConfiguration);
                _resException = iSETMethods.GetISETMedicalClaimRead(dOMedicalClaimRequest, out dOMedicalClaimResponse);
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
            return Ok(dOMedicalClaimResponse);
        }

        [HttpGet]
        public ActionResult GetISETAlertDetails(string ISETMedicalClaimAlertRequest = null)
        {
            DOMedicalClaimAlertResponse dOMedicalClaimAlertResponse = new DOMedicalClaimAlertResponse();
            DOMedicalClaimAlertRequest dOMedicalClaimAlertRequest = JsonConvert.DeserializeObject<DOMedicalClaimAlertRequest>(ISETMedicalClaimAlertRequest);

            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "GetISETAlertDetails", _username);

            try
            {
                ISETMethods iSETMethods = new ISETMethods(_memoryCacheHelper, _objConfiguration);
                _resException = iSETMethods.GetISETMedicalClaimAlert(dOMedicalClaimAlertRequest, out dOMedicalClaimAlertResponse);
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
            return Ok(dOMedicalClaimAlertResponse);
        }

        [HttpGet]
        public ActionResult GetISETEhealthEobSearchDetails(string ISETSearchRequest = null,string ClaimNumber = null)
        {
            DOEobSearchResponse objDOISETEobSearchResponse = new DOEobSearchResponse();
            EhealthEobSearch objDOISETSearchRequest = JsonConvert.DeserializeObject<EhealthEobSearch>(ISETSearchRequest);

            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "GetISETEhealthEobSearchDetails", _username);

            try
            {
                ISETMethods objEPMPMethods = new ISETMethods(_memoryCacheHelper, _objConfiguration);
                _resException = objEPMPMethods.GetISETEhealthEobSearch(objDOISETSearchRequest, out objDOISETEobSearchResponse, ClaimNumber);
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
            return Ok(objDOISETEobSearchResponse);
        }


        [HttpGet]
        public ActionResult GetISETMedicalClaimSearchDetails(string ISETMedicalClaimSearchRequest = null)
        {
            DOMedicalClaimSearchResponse dOMedicalClaimResponse = new DOMedicalClaimSearchResponse();
            RootMedicalSearch dOMedicalClaimSearchRequest = JsonConvert.DeserializeObject<RootMedicalSearch>(ISETMedicalClaimSearchRequest);

            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "GetISETMedicalClaimSearchDetails", _username);

            try
            {
                ISETMethods iSETMethods = new ISETMethods(_memoryCacheHelper, _objConfiguration);
                _resException = iSETMethods.GetISETMedicalClaimSearch(dOMedicalClaimSearchRequest, out dOMedicalClaimResponse);
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
            return Ok(dOMedicalClaimResponse);
        }

    }
}
