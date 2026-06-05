using ANGDDEAPI.Common;
using ANGDDEAPIBO;
using ANGDDEAPIDO;
using ANGDDEAPIDO.Interface;
using ANGDDEAPIDO.NDB;
using ANGDDEAPIDO.UNET;
using ANGDDEAPIFoundation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ANGDDEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class NDBController : Controller
    {

        private readonly IHttpContextAccessor _httpContextAccessor;

        ExceptionTypes _resException;
        public string _username = string.Empty;
        public string ModuleName = "NDBController";
        private IMemoryCacheHelper _memoryCacheHelper;
        private readonly DOConfiguration _objConfiguration;
        private readonly BOCommon _objBOCommon;
        private readonly ICacheService _cache;

        public NDBController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _username = httpContextAccessor.HttpContext.User.Identity.Name;
            _memoryCacheHelper = memoryCacheHelper;
            _objBOCommon = bOCommon;
            _cache = cache;
        }

        [HttpGet]
        public ActionResult GetPRPDetails(string ICN , string Suffix, string TopsEngineCd)
        {
            PRPInfo objDOPRPInfo = new PRPInfo();
            DOPRPRequest objDOPRPRequest = new DOPRPRequest()
            {
                icn = ICN,
                suffix = Suffix,
                topsEngineCd = TopsEngineCd
            };

            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "GetPRPDetails", _username);

            try
            {
                NDBMethods objNDB = new NDBMethods(_memoryCacheHelper, _objConfiguration);
                _resException = objNDB.GetPRPDetails(objDOPRPRequest, out objDOPRPInfo);
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
            return Ok(objDOPRPInfo);
        }

        [HttpGet]
        public ActionResult GetPESDetails(string Prefix, string TaxID, string Suffix, string ProviderType="")
        {
            DOPESInfo objDOPESInfo = new DOPESInfo();

            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "GetPESDetails", _username);

            try
            {
                NDBMethods objNDB = new NDBMethods(_memoryCacheHelper, _objConfiguration);
                _resException = objNDB.GetPESDetails(Prefix, TaxID, Suffix, ProviderType.ToUpper(), out objDOPESInfo);
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
            return Ok(objDOPESInfo);
        }

        [HttpGet]
        public ActionResult GetPESDetailInfo(string MPIN, string AddressSequence)
        {
            DOPESInfo objDOPESInfo = new DOPESInfo();

            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "GetPESDetailInfo", _username);

            try
            {
                NDBMethods objNDB = new NDBMethods(_memoryCacheHelper, _objConfiguration);
                _resException = objNDB.GetPESDetailInfo(MPIN, AddressSequence, out objDOPESInfo);
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
            return Ok(objDOPESInfo);
        }

        [HttpGet]
        public ActionResult GetContractDetails(string MPin, string TaxID, string ContractTypeCode= "H")
        {
            DOContractInfo objDOContractInfo = new DOContractInfo();            

            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "GetContractInfo", _username);

            try
            {
                NDBMethods objNDB = new NDBMethods(_memoryCacheHelper, _objConfiguration);
                _resException = objNDB.GetContractDetails(MPin, TaxID, ContractTypeCode, out objDOContractInfo);
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
            return Ok(objDOContractInfo);
        }

        [HttpGet]
        public async Task<ActionResult> GetPESContractDetails(string lob, string provId = "", string taxId = "", string additionalParams = "")
        {
            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "GetPESContractDetails", _username);

            try
            {
                // Validate required parameters
                if (string.IsNullOrEmpty(lob))
                {
                    return BadRequest("LOB parameter is required");
                }

                // Parse additional parameters if provided
                Dictionary<string, string> additionalParamsDict = null;
                if (!string.IsNullOrEmpty(additionalParams))
                {
                    try
                    {
                        additionalParamsDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(additionalParams);
                    }
                    catch (JsonException)
                    {
                        return BadRequest("Invalid format for additionalParams. Expected JSON object.");
                    }
                }

                NDBMethods objNDB = new(_memoryCacheHelper, _objConfiguration);
                var (exceptionResult, result) = await objNDB.GetContractDetailsAsync(lob, provId, taxId, additionalParamsDict);

                if (exceptionResult != ExceptionTypes.Success && exceptionResult != ExceptionTypes.ZeroRecords)
                {
                    return BadRequest(result?.ToString() ?? "An error occurred while retrieving contract details");
                }

                return Ok(result);
            }
            catch (ArgumentException argEx)
            {
                if (source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(0, location, argEx.Message, argEx.ToString());
                }
                else
                {
                    _objBOCommon.LogError(location, source, 10001, argEx.Message, " ", argEx.StackTrace?.ToString() ?? "", _username);
                }
                return BadRequest(argEx.Message);
            }
            catch (Exception ex)
            {
                if (source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(0, location, ex.Message, ex.ToString());
                }
                else
                {
                    _objBOCommon.LogError(location, source, 10001, ex.Message, " ", ex.StackTrace?.ToString() ?? "", _username);
                }
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public ActionResult GetCommentDetails(string MPin)
        {
            DOCommentInfo objDOCommentInfo = new DOCommentInfo();

            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "GetCommentInfo", _username);

            try
            {
                NDBMethods objNDB = new NDBMethods(_memoryCacheHelper, _objConfiguration);
                _resException = objNDB.GetCommentDetails(MPin, out objDOCommentInfo);
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
            return Ok(objDOCommentInfo);
        }

    }
}
