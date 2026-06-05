using ANGDDEAPI.Common;
using ANGDDEAPIBO;
using ANGDDEAPIDO.Interface;
using ANGDDEAPIDO;
using ANGDDEAPIFoundation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace ANGDDEAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HCPOCMAuthController : ControllerBase
    {
        public static long LoggedInUserId = 0;
        BOClaimsSearch _objBOClaimsSearch;
        ExceptionTypes _result;
        public string _username = string.Empty;
        string source = string.Empty;


        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DOConfiguration _objConfiguration;
        private readonly IMemoryCacheHelper _memoryCacheHelper;
        private readonly BOCommon _objBOCommon;
        private readonly ICacheService _cache;


        public HCPOCMAuthController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _memoryCacheHelper = memoryCacheHelper;
            _objBOCommon = bOCommon;
            _cache = cache;
        }





        // GET: api/<HCPOCMAuthController>
        [HttpGet]
        public async Task<ActionResult> Get(string strAuthSearch)
        {
            List<DOAuthInfo> lstDOAuthInfo = null;
            DOAuthInfo objDOAuthInfo = null;
            HCPOCMAuthMethods objHCPOCMAuthMethods = new(_memoryCacheHelper, _objConfiguration);
            try
            {
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                if (!string.IsNullOrEmpty(strAuthSearch))
                {
                    objDOAuthInfo = new DOAuthInfo();
                    objDOAuthInfo = JsonConvert.DeserializeObject<DOAuthInfo>(strAuthSearch);

                    var (result, authInfoList) = await objHCPOCMAuthMethods.GetOCMAuthHeader(objDOAuthInfo);
                    _result = result;
                    lstDOAuthInfo = authInfoList;
                    if (_result != ExceptionTypes.Success && _result != ExceptionTypes.ZeroRecords)
                    {
                        return BadRequest();
                    }
                }
                return Ok(lstDOAuthInfo);
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
                throw;
            }
        }

        // GET api/<HCPOCMAuthController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult> GetById(string id)
        {
            List<DOAuthInfo> lstDOAuthInfo = null;
            DOAuthInfo objDOAuthInfo = null;
            HCPOCMAuthMethods objHCPOCMAuthMethods = new(_memoryCacheHelper, _objConfiguration);
            try
            {
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                if (!string.IsNullOrEmpty(id))
                {
                    objDOAuthInfo = new DOAuthInfo();
                    objDOAuthInfo.AuthRefCaseID = id;
                    var (result, authInfoList) = await objHCPOCMAuthMethods.GetOCMAuthDetails(objDOAuthInfo);
                    _result = result;
                    lstDOAuthInfo = authInfoList;

                    if (_result != ExceptionTypes.Success && _result != ExceptionTypes.ZeroRecords)
                    {
                        return BadRequest();
                    }
                }
                return Ok(lstDOAuthInfo);
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
                throw;
            }
        }

    }
}
