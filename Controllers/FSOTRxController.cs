using ANGDDEAPI.Common;
using ANGDDEAPIBO;
using ANGDDEAPIDO;
using ANGDDEAPIDO.Interface;
using ANGDDEAPIFoundation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;


namespace ANGDDEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class FSOTRxController : ControllerBase
    {
        public static long LoggedInUserId = 0;
        public string _username = string.Empty;
        string source = string.Empty;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DOConfiguration _objConfiguration;
        private readonly IMemoryCacheHelper _memoryCacheHelper;
        private readonly BOCommon _objBOCommon;

        public FSOTRxController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _memoryCacheHelper = memoryCacheHelper;
            _objBOCommon = bOCommon;
        }

        [HttpPost]
        public ActionResult FormularyDrugDetail([FromBody] DOFormularyDrugDetails objDOFormularyDrugDetails)
        {
            try
            {
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                if (objDOFormularyDrugDetails != null)
                {
                    FSOTRxMethods objFSOTRxMethods = new(_memoryCacheHelper, _objConfiguration);
                    ExceptionTypes res = objFSOTRxMethods.GetCommFormularyDrugDetail(ref objDOFormularyDrugDetails);

                    if (res != ExceptionTypes.Success)
                        res = objFSOTRxMethods.GetPartDFormularyDrugDetail(ref objDOFormularyDrugDetails);

                    if(res != ExceptionTypes.Success)
                        return StatusCode((int)HttpStatusCode.NotFound, "");

                    return Ok(objDOFormularyDrugDetails);
                }
                return Ok(objDOFormularyDrugDetails);
            }
            catch (Exception ex)
            {
                if (source.Contains("DDE"))
                    _objBOCommon.ErrorLog(LoggedInUserId, this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), ex.Message, ex.ToString());
                else
                    _objBOCommon.LogError(this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                return StatusCode((int)HttpStatusCode.InternalServerError, ex);
            }
        }
    }
}
