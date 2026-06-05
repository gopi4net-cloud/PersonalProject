using ANGDDEAPIBO;
using ANGDDEAPIDO;
using ANGDDEAPIDO.Interface;
using ANGDDEAPIFoundation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ANGDDEAPI.Controllers
{

    [ApiController]
    public class ATSAPIController : ControllerBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        ExceptionTypes _resException;
        public string _username = string.Empty;
        public string ModuleName = "ATSAPIController";
        private readonly DOConfiguration _objConfiguration;
        private readonly BOCommon _objBOCommon;
        private readonly ICacheService _cache; 
        public ATSAPIController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, BOCommon bOCommon, ICacheService cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _username = httpContextAccessor.HttpContext.User.Identity.Name;
            _objBOCommon = bOCommon;
            _cache = cache;
        }

        [Route("api/ATSAPI")]
        [HttpGet]
        public ActionResult Get(string MemberId, string MBIorHICN)
        {
            List<DODuplicateCase> lstDODuplicateCase = new List<DODuplicateCase>();
            BODuplicateCheck objBODuplicateCheck = new BODuplicateCheck(_objConfiguration,_cache, _objBOCommon);
            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
          //  _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Request", "MemberId-" + MemberId + ", HICN-" + MBIorHICN, _username);
            try
            {
                //_username = System.Web.HttpContext.Current.User.Identity.Name;
                _resException = objBODuplicateCheck.GetSuspectDuplicateCases(MemberId, MBIorHICN, out lstDODuplicateCase);
               // _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Response", _resException.ToString(), _username);
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
            return Ok(lstDODuplicateCase);
        }
    }
}
