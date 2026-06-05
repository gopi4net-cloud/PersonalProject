using ANGDDEAPI.Common;
using ANGDDEAPIBO;
using ANGDDEAPIDO;
using ANGDDEAPIDO.FDS;
using ANGDDEAPIDO.Interface;
using ANGDDEAPIDO.PSaaS;
using ANGDDEAPIFoundation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;


namespace ANGDDEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class FDSToDoc360DirectController : ControllerBase
    {
        public static long LoggedInUserId = 0;
        Doc360DirectFDSFileUploadMethods _objFdsDoc360Search;
        ExceptionTypes _result;
        public string _username = string.Empty;
        string source = string.Empty;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DOConfiguration _objConfiguration;
        private readonly IMemoryCacheHelper _memoryCacheHelper;
        public readonly BOCommon _objBOCommon;

        private readonly ICacheService _cache;

        public FDSToDoc360DirectController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _memoryCacheHelper = memoryCacheHelper;
            _objBOCommon = bOCommon;
            _cache = cache;
        }

        /// <summary>
        /// Get Claims information based on search criteria
        /// </summary>
        /// <param name="strClaimSearch"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult MoveDocumentFromFDSToDoc360(string strSearch)
        {
            source = BOCommon.GetRefererURI(Request);
            try
            {
                List<Docid_info> Doc360GlobalID = null;
                string log = string.Empty;
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                DOFdsToDoc360Input objDOFdsDoc360Input = null;
                bool isDirect = _objConfiguration.AppSettings.Doc360IsDrictBH;

                if (isDirect)
                {

                    _objFdsDoc360Search = new Doc360DirectFDSFileUploadMethods(_memoryCacheHelper, _objConfiguration, _httpContextAccessor, _objBOCommon);
                   
                    //log = log + _username;
                    if (!string.IsNullOrEmpty(strSearch.ToString()))
                    {
                        //log = log + " inif ";
                        objDOFdsDoc360Input = new DOFdsToDoc360Input();
                        objDOFdsDoc360Input = JsonConvert.DeserializeObject<DOFdsToDoc360Input>(strSearch);
                        //log = log + " jsonDeserialize ";

                        _result = _objFdsDoc360Search.MoveDocumentFromFDSToDoc360Direct(objDOFdsDoc360Input, out Doc360GlobalID, ref log);
                    }
                }
                else
                {
                    var _objFdsDoc360Search = new Doc360FDSFileUploadMethods(_memoryCacheHelper, _objConfiguration, _httpContextAccessor, _objBOCommon);
                   
                    if (!string.IsNullOrEmpty(strSearch.ToString()))
                    {
                        //log = log + " inif ";
                        objDOFdsDoc360Input = new DOFdsToDoc360Input();
                        objDOFdsDoc360Input = JsonConvert.DeserializeObject<DOFdsToDoc360Input>(strSearch);
                        //log = log + " jsonDeserialize ";

                        _result = _objFdsDoc360Search.MoveDocumentFromFDSToDoc360(objDOFdsDoc360Input, out Doc360GlobalID, ref log);

                        //log = log + Doc360GlobalID;

                    }
                }

                    //log = log + Doc360GlobalID;

                
                return Ok(Doc360GlobalID);
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
                // _logger.Error(this.GetType().Name + "", (long)ErrorModuleName.WebService, (long)500, ex.ToString, " ", ex.StackTrace.ToString(), new DO.DOLoginUserDetails { ADM_UserInfoId = 10001 });
                return BadRequest();
            }

        }

       
    }

    //public class DirectDocid_info
    //{
    //    public string documentId { get; set; }
    //    public string filename { get; set; }
    //    public long filesize { get; set; }


    //}
}
