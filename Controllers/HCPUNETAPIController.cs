using ANGDDEAPI.Common;
using ANGDDEAPIBO;
using ANGDDEAPIDO;
using ANGDDEAPIDO.Interface;
using ANGDDEAPIFoundation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ANGDDEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class HCPUNETAPIController : ControllerBase
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
        private readonly BOHCPUNETClaimSearch _BOHCPUNETClaimSearch;
        public HCPUNETAPIController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _memoryCacheHelper = memoryCacheHelper;
            _objBOCommon = bOCommon;
            _cache = cache;
            _BOHCPUNETClaimSearch = new BOHCPUNETClaimSearch(_objConfiguration, _cache);
        }


        [HttpGet]
        public ActionResult GetClaimSummary(string strClaimSearch)
        {
            try
            {
                List<DOClaimInfo> lstObjDOClaimInfo = null;
                DOClaimInfo objDOClaimInfo = null;
                _objBOClaimsSearch = new BOClaimsSearch(_objConfiguration, _cache);
                HCPUNETClaimMethods _objBOHCPUNETClaimSearch = new(_memoryCacheHelper,_cache, _objConfiguration);

                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                if (!string.IsNullOrEmpty(strClaimSearch))
                {
                    objDOClaimInfo = new DOClaimInfo();
                    objDOClaimInfo = JsonConvert.DeserializeObject<DOClaimInfo>(strClaimSearch);
                    _result = _objBOHCPUNETClaimSearch.GetClaimSummary(objDOClaimInfo, out lstObjDOClaimInfo);

                    if (_result != ExceptionTypes.Success && _result != ExceptionTypes.ZeroRecords)
                    {
                        return BadRequest();
                    }
                }
                return Ok(lstObjDOClaimInfo);
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
                throw ex;
            }

        }

        [HttpGet]
        public ActionResult GetClaimDetail(string strClaimSearch)
        {
            try
            {
                List<DOClaimInfo> lstObjDOClaimInfo = null;
                DOClaimInfo objDOClaimInfo = null;
                _objBOClaimsSearch = new BOClaimsSearch(_objConfiguration, _cache);
                HCPUNETClaimMethods _objBOHCPUNETClaimSearch = new(_memoryCacheHelper, _cache, _objConfiguration);
              
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                if (!string.IsNullOrEmpty(strClaimSearch))
                {
                    objDOClaimInfo = new DOClaimInfo();
                    objDOClaimInfo = JsonConvert.DeserializeObject<DOClaimInfo>(strClaimSearch);
                    _result = _objBOHCPUNETClaimSearch.GetClaimDetail(objDOClaimInfo, out lstObjDOClaimInfo);

                    if (_result != ExceptionTypes.Success && _result != ExceptionTypes.ZeroRecords)
                    {
                        return BadRequest();
                    }
                }               
                    return Ok(lstObjDOClaimInfo);
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
                throw ex;
            }
        }

        [HttpGet]
        public ActionResult GetClaimServiceAddress(string strClaimSearch)
        {
            try
            {
                DOClaimInfo lstObjDOClaimInfo = null;
                DOClaimInfo objDOClaimInfo = null;
                _objBOClaimsSearch = new BOClaimsSearch(_objConfiguration, _cache);
                BHUNETAddressMethods _objBOHCPUNETServiceAddressSearch = new(_memoryCacheHelper, _objConfiguration);

                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                if (!string.IsNullOrEmpty(strClaimSearch))
                {
                    objDOClaimInfo = new DOClaimInfo();
                    objDOClaimInfo = JsonConvert.DeserializeObject<DOClaimInfo>(strClaimSearch);
                    _result = _objBOHCPUNETServiceAddressSearch.GetUNETPMIServiceAddressDetails(objDOClaimInfo.FullProviderTIN, out lstObjDOClaimInfo);

                    if (_result != ExceptionTypes.Success && _result != ExceptionTypes.ZeroRecords)
                    {
                        return BadRequest();
                    }
                }
                return Ok(lstObjDOClaimInfo);
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
                throw ex;
            }
        }
    }
}
