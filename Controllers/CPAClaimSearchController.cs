using ANGDDEAPI.Common;
using ANGDDEAPIBO;
using ANGDDEAPIDO;
using ANGDDEAPIDO.Interface;
using ANGDDEAPIFoundation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using static ANGDDEAPIDO.DOProfessionalsPES;

namespace ANGDDEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class CPAClaimSearchController : ControllerBase
    {
        public string _username = string.Empty;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DOConfiguration _objConfiguration;
        private readonly IMemoryCacheHelper _memoryCacheHelper;
        private readonly BOCommon _objBOCommon;
        private readonly ICacheService _cache;
        ExceptionTypes _result;
        ExceptionTypes _resultDetails;
        string source = string.Empty;
        public static long LoggedInUserId = 0;

        public CPAClaimSearchController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _memoryCacheHelper = memoryCacheHelper;
            _objBOCommon = bOCommon;
            _cache = cache;
        }
        [HttpGet]
        public ActionResult GetAllClaim(string strClaimSearch)
        {

            string result = string.Empty;
            try
            {
                List<DOClaimInfo> lstObjDOClaimInfo = null;
                BOCPAClaimSearch objCPAClaimSearch = new BOCPAClaimSearch(_objConfiguration);
                DOClaimInfo objDOClaimInfo = null;
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;

                _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Request", strClaimSearch, _username);

                if (!string.IsNullOrEmpty(strClaimSearch))
                {
                    objDOClaimInfo = new DOClaimInfo();
                    objDOClaimInfo = JsonConvert.DeserializeObject<DOClaimInfo>(strClaimSearch);

                    //if (objDOClaimInfo.StartDate == null && objDOClaimInfo.EndDate == null)
                    //    return BadRequest(new { error = "Start & End Date must be provided." });

                    _result = objCPAClaimSearch.GetAllClaims(objDOClaimInfo, out lstObjDOClaimInfo);

                    if (_result != ExceptionTypes.Success && _result != ExceptionTypes.ZeroRecords) { return BadRequest(); }
                }
                _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Response", BOCommon.JsonConvertObjectToString(lstObjDOClaimInfo), _username);
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
        public ActionResult GetClaimDetails(string strClaimSearch)
        {
            try
            {
                List<DOClaimInfo> lstObjDOClaimInfo = null;
                DOClaimInfo objDOClaimInfo = null;
                BOCPAClaimSearch objCPAClaimSearch = new(_objConfiguration);

                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Request", strClaimSearch, _username);

                if (!string.IsNullOrEmpty(strClaimSearch))
                {
                    objDOClaimInfo = new DOClaimInfo();
                    objDOClaimInfo = JsonConvert.DeserializeObject<DOClaimInfo>(strClaimSearch);
                    lstObjDOClaimInfo = [];

                    _result = objCPAClaimSearch.GetAllClaims(objDOClaimInfo, out lstObjDOClaimInfo);
                    if (_result != ExceptionTypes.Success && _result != ExceptionTypes.ZeroRecords) { return BadRequest(); }

                    _resultDetails = objCPAClaimSearch.GetClaimDetails(objDOClaimInfo, ref lstObjDOClaimInfo);
                    if (_resultDetails != ExceptionTypes.Success && _resultDetails != ExceptionTypes.ZeroRecords) { return BadRequest(); }
                }
                _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Response", BOCommon.JsonConvertObjectToString(lstObjDOClaimInfo), _username);
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


