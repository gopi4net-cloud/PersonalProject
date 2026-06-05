using ANGDDEAPIBO;
using ANGDDEAPIDO;
using ANGDDEAPIFoundation;
using ANGDDEAPI.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using ANGDDEAPIDO.Interface;

namespace ANGDDEAPI.Controllers
{
    
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class ClaimNotesController : ControllerBase
    {
        public static long LoggedInUserId = 0;

        ExceptionTypes _result;
        public string _username = string.Empty;
        string source = string.Empty;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DOConfiguration _objConfiguration;
        private readonly IMemoryCacheHelper _memoryCacheHelper;
        private readonly BOCommon _objBOCommon;
        private readonly ICacheService _cache; 
         
        public ClaimNotesController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _memoryCacheHelper = memoryCacheHelper;
            _objBOCommon = bOCommon;
            _cache = cache;
        }

        [HttpGet]

        public ActionResult GetClaimNotesService(string claimid)
        {
            List<DOClaimNotesInfo> lstDOClaimNotesInfo = null;
            DOClaimNotesInfo objDOClaimNotesInfo = new DOClaimNotesInfo();
            source = BOCommon.GetRefererURI(Request);
            try
            {
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Request", claimid, _username);

                if (!string.IsNullOrEmpty(claimid))
                {
                    ClaimNotesMethods objClaimNotesMethods = new ClaimNotesMethods(_memoryCacheHelper, _objConfiguration);
                    _result = objClaimNotesMethods.ClaimNotesService(claimid, out lstDOClaimNotesInfo);
                    if (_result != ExceptionTypes.Success)
                    {
                        return StatusCode((int)HttpStatusCode.InternalServerError, "An Error occured");
                    }
                }
                _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Response", _result.ToString(), _username);
                return Ok(lstDOClaimNotesInfo);
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
                return StatusCode((int)HttpStatusCode.InternalServerError, "An Error occured");

            }
        }
    }
}