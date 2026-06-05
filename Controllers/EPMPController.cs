using ANGDDEAPI.Common;
using ANGDDEAPIBO;
using ANGDDEAPIDO;
using ANGDDEAPIDO.Interface;
using ANGDDEAPIFoundation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static ANGDDEAPI.Common.EPMPMethods;

namespace ANGDDEAPI.Controllers
{

    [ApiController]
    [Route("api/[controller]/[action]")]
    public class EPMPController : ControllerBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        ExceptionTypes _resException;
        public string _username = string.Empty;
        public string ModuleName = "EPMPController";
        private readonly DOConfiguration _objConfiguration;
        private readonly IMemoryCacheHelper _memoryCacheHelper;
        private readonly BOCommon _objBOCommon;
        private readonly ICacheService _cache;

        public EPMPController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _memoryCacheHelper = memoryCacheHelper;
            _objBOCommon = bOCommon;
            _cache = cache;
        }

        [HttpGet]
        public ActionResult GetEPMPDetails(string ePMPRequest = null)
        {
            DOEPMPResponse objDOEPMPResponse = new DOEPMPResponse();            
            DOEPMPRequest objDOEPMPRequest = JsonConvert.DeserializeObject<DOEPMPRequest>(ePMPRequest);
            
            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "GetEPMPDetails", _username);

            try
            {
                EPMPMethods objEPMPMethods = new EPMPMethods(_memoryCacheHelper, _objConfiguration);
                _resException = objEPMPMethods.GetEPMPSearch(objDOEPMPRequest, out objDOEPMPResponse);
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
            return Ok(objDOEPMPResponse);
        }

        [HttpGet]
        public ActionResult ATSEPMPDetails(string ePMPRequest = null)
        {
            DOATSEPMPDetailsResponse objDOATSEPMPDetailsResponse = null;
            DOEPMPResponse objDOEPMPResponse = new DOEPMPResponse();
            DOEPMPRequest objDOEPMPRequest = JsonConvert.DeserializeObject<DOEPMPRequest>(ePMPRequest);

            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "GetEPMPPreference", _username);

            try
            {
                EPMPMethods objEPMPMethods = new EPMPMethods(_memoryCacheHelper, _objConfiguration);
                _resException = objEPMPMethods.GetEPMPSearch(objDOEPMPRequest, out objDOEPMPResponse);
                if (objDOEPMPResponse.message == "No data found")
                {
                    _resException = objEPMPMethods.GetOBHEPMPSearch(objDOEPMPRequest, out objDOEPMPResponse);
                }
                if (_resException != ExceptionTypes.Success && _resException != ExceptionTypes.ZeroRecords)
                {
                    return BadRequest();
                }
                else if (_resException != ExceptionTypes.ZeroRecords)
                {
                    objEPMPMethods.MapToATSEPMPDetails(objDOEPMPResponse, out objDOATSEPMPDetailsResponse);
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
            return Ok(objDOATSEPMPDetailsResponse);
        }
        [HttpGet]
        public async Task<ActionResult> GetEPMPAuthRepDetails(string idType = "SCRCSP", string idValue = null)
        {
            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "GetEPMPAuthRepOnFileDetails", _username);

            try
            {
                EPMPMethods objEPMPMethods = new EPMPMethods(_memoryCacheHelper, _objConfiguration);

                var (result, responseList) = await objEPMPMethods.GetEPMPAuthRepOnFileDetailsAsync(idType, idValue);
                if (result != ExceptionTypes.Success)
                {
                    return BadRequest();
                }
                return Ok(responseList);
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
        }
    }
}
