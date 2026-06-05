using ANGDDEAPI.Common;
using ANGDDEAPIBO;
using ANGDDEAPIDO;
using ANGDDEAPIDO.CDB;
using ANGDDEAPIDO.Interface;
using ANGDDEAPIFoundation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ANGDDEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class MemberErisaInfoController : Controller
    {
        public static long LoggedInUserId = 0;
        MemberErisaMethods _ObjMemberErisa;
        ExceptionTypes _result;
        public string _username = string.Empty;
        string source = string.Empty;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DOConfiguration _objConfiguration;
        private readonly IMemoryCacheHelper _memoryCacheHelper;
        public readonly BOCommon _objBOCommon;

        private readonly ICacheService _cache;

        public MemberErisaInfoController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _memoryCacheHelper = memoryCacheHelper;
            _objBOCommon = bOCommon;
            _cache = cache;
        }



        [HttpGet]
        public IActionResult GetMemberErisa(string strSearch)
        {
            source = BOCommon.GetRefererURI(Request);
            try
            {
                List<ErisaDetails> lstObjErisaDetailInfo = null;

                DOErisaRequestInput _objErisaRequestInput = null;
                _ObjMemberErisa = new MemberErisaMethods(_memoryCacheHelper, _objConfiguration, _httpContextAccessor, _objBOCommon);
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                if (!string.IsNullOrEmpty(strSearch.ToString()))
                {
                    _objErisaRequestInput = new DOErisaRequestInput();
                    _objErisaRequestInput = JsonConvert.DeserializeObject<DOErisaRequestInput>(strSearch);

                    _result = _ObjMemberErisa.GetErisaData(_objErisaRequestInput, out lstObjErisaDetailInfo);

                }
                return Ok(lstObjErisaDetailInfo);
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
