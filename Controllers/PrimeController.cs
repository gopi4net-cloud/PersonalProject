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
using ANGDDEAPIDO.Prime;
using System.Collections.Generic;

namespace ANGDDEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class PrimeController : ControllerBase
    {

        private readonly IHttpContextAccessor _httpContextAccessor;

        ExceptionTypes _resException;
        public string _username = string.Empty;
        public string ModuleName = "UNETController";
        private DOConfiguration _objConfiguration;
        private IMemoryCacheHelper _memoryCacheHelper;
        private BOCommon _objBOCommon;
        private ICacheService _cache;

        public PrimeController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _memoryCacheHelper = memoryCacheHelper;
            _objBOCommon = bOCommon;
            _cache = cache;
        }

        [HttpGet]
        public ActionResult GetSBCDoc(string PolicyNumber = null)
        {
            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "GetSBCDoc", _username);
            LstPrimeDocInfo lstPrimeDocInfo = new LstPrimeDocInfo();

            try
            {
                PrimeMethods objPrime = new PrimeMethods(_memoryCacheHelper, _objConfiguration);
                _resException = objPrime.GetSBCDoc(PolicyNumber, out lstPrimeDocInfo);
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
            return Ok(lstPrimeDocInfo);
        }

        [HttpGet]
        public ActionResult GetPolicyDoc(string PolicyNumber = null)
        {
            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "GetPolicyDoc", _username);
            LstPrimeDocInfo lstPrimeDocInfo = new LstPrimeDocInfo();

            try
            {
                PrimeMethods objPrime = new PrimeMethods(_memoryCacheHelper, _objConfiguration);
                _resException = objPrime.GetPolicyDoc(PolicyNumber, out lstPrimeDocInfo);
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
            return Ok(lstPrimeDocInfo);
        }


        [HttpGet]
        public ActionResult GetPolicyPdf(string DocumentName = null)
        {
            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "GetPolicyPdf", _username);
            string data = string.Empty;

            try
            {
                PrimeMethods objPrime = new PrimeMethods(_memoryCacheHelper, _objConfiguration);
                _resException = objPrime.GetPolicyPdf(DocumentName, out data);
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
            return Ok(new { PdfBase64 = data });
        }
    }
}
