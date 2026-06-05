using ANGDDEAPI.Common;
using ANGDDEAPIBO;
using ANGDDEAPIDO;
using ANGDDEAPIFoundation;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Net;
using System;
using OfficeOpenXml.FormulaParsing.Excel.Functions.RefAndLookup;
using ANGDDEAPIDO.BHLinxAuth;
using ANGDDEAPIDO.Interface;
using Microsoft.AspNetCore.Http;
using ANGDDEAPIDO.AORROI;
using ANGDDEAPIDO.MGRL;

namespace ANGDDEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class BHMGRLController : Controller
    {
        public static long LoggedInUserId = 0;
        BHMGRLMethods _ObjBHMGRL;
        ExceptionTypes _result;
        public string _username = string.Empty;
        string source = string.Empty;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DOConfiguration _objConfiguration;
        private readonly IMemoryCacheHelper _memoryCacheHelper;
        public readonly BOCommon _objBOCommon;

        private readonly ICacheService _cache;

        public BHMGRLController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _memoryCacheHelper = memoryCacheHelper;
            _objBOCommon = bOCommon;
            _cache = cache;
        }

        [HttpGet]
        public IActionResult GetBHMGRLDetails()
        {
            source = BOCommon.GetRefererURI(Request);
            try
            {
                DOMGRLResponse ObjDOMGRLResponse = null;
                _ObjBHMGRL = new BHMGRLMethods(_memoryCacheHelper, _objConfiguration, _httpContextAccessor, _objBOCommon);
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;

                _result = _ObjBHMGRL.MGRLDetails(out ObjDOMGRLResponse);
                
                return Ok(ObjDOMGRLResponse);
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

                return BadRequest();
            }

        }


        
    }
}
