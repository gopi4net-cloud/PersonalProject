using ANGDDEAPI.Common;
using ANGDDEAPIBO;
using ANGDDEAPIDO.Interface;
using ANGDDEAPIDO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Amazon.Runtime.Internal.Util;
using System.Collections.Generic;
using System.Net;
using System;
using ANGDDEAPIDO.MAPS;
using Newtonsoft.Json;

namespace ANGDDEAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class MAPSController : ControllerBase
    {
        public static long LoggedInUserId = 0;
        public string _username = string.Empty;
        string source = string.Empty;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DOConfiguration _objConfiguration;
        private readonly IMemoryCacheHelper _memoryCacheHelper;
        private readonly BOCommon _objBOCommon;
        private readonly ICacheService _cache;

        public MAPSController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _memoryCacheHelper = memoryCacheHelper;
            _objBOCommon = bOCommon;
            _cache = cache;
        }

        [HttpGet]
        public ActionResult mapsinformation(string details = null)
        {
            List<DOMPASResult> lstMapsDetails = null;
            DOMPASSearchCriteria objDOMPASSearchCriteria = null;
            source = BOCommon.GetRefererURI(Request);
            try
            {
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                //_httpContextAccessor.HttpContext.User.Identity.Name.Split(new string[] { @"\" }, StringSplitOptions.RemoveEmptyEntries);

                if (!string.IsNullOrEmpty(details))
                {
                    objDOMPASSearchCriteria = new DOMPASSearchCriteria();
                    objDOMPASSearchCriteria = JsonConvert.DeserializeObject<DOMPASSearchCriteria>(details);

                    lstMapsDetails = new List<DOMPASResult>();
                    HCPMAPSDetails objBOHCPMemberDetails = new(_memoryCacheHelper, _objConfiguration);
                    lstMapsDetails = objBOHCPMemberDetails.GetMAPSDetails(objDOMPASSearchCriteria);
                }
                //return Ok(lstMapsDetails);
                var fileName = "NGS GRID " + DateTime.Now.ToString("MM-dd-yyyy") + ".xlsx";
                return File(BOCommon.ExportExcel(lstMapsDetails, "GPSEmployerID n Group(Current)"), "application/vnd.ms-excel", fileName);
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
                return StatusCode((int)HttpStatusCode.InternalServerError, ex);
            }
        }
    }
}
