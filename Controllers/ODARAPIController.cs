using ANGDDEAPI.Common;
using ANGDDEAPIBO;
using ANGDDEAPIDO.Interface;
using ANGDDEAPIDO;
using ANGDDEAPIFoundation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ANGDDEAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ODARAPIController : ControllerBase
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

        public ODARAPIController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _memoryCacheHelper = memoryCacheHelper;
            _objBOCommon = bOCommon;
            _cache = cache;
        }

        // GET: api/<ODARAPIController>
        [HttpGet]
        public ActionResult GetOdarDetails([FromBody] DOODAR objODAR)
        {
            try
            {

                List<ODARResponse> lstObjDOODARDetails = null;
                BOODARAPI objBOODARAPI = new BOODARAPI(_objConfiguration);
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Request", string.Empty, _username);


                if (objODAR != null && !string.IsNullOrEmpty(objODAR.partialAuditNumberSearch.partialClaimAuditNumber) && objODAR.searchByAuditNumber)
                {
                    _result= objBOODARAPI.GetOdarDetails(objODAR,out lstObjDOODARDetails);

                    if (_result != ExceptionTypes.Success && _result != ExceptionTypes.ZeroRecords) { return BadRequest(); }
                }
                else
                {
                    _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Response", BOCommon.JsonConvertObjectToString(objODAR), _username);

                    if (objODAR.searchByAuditNumber==false)
                    {
                        return BadRequest(new { error = "Invalid request.searchByAuditNumber is True Always." });
                    }
                    else if(!string.IsNullOrEmpty(objODAR.partialAuditNumberSearch.partialClaimAuditNumber))
                    {
                        return BadRequest(new { error = "Invalid request.partialClaimAuditNumber is required." });
                    }
                        
                }


                if (lstObjDOODARDetails != null && lstObjDOODARDetails.Count > 0)
                {
                    return Ok(lstObjDOODARDetails);
                }
                else
                {
                    return NotFound(new { error = "No records found." });
                }

            }
            catch(Exception ex)
            {
                string location = "ODARAPIController.GetOdarDetails";
                string source = BOCommon.GetRefererURI(Request);
                _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "GetOdarDetails", _username);
                return BadRequest(new { error = ex.Message });
            }
        }

    }
   
}
