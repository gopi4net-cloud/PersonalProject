using ANGDDEAPIBO;
using ANGDDEAPIDO;
using ANGDDEAPIDO.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;

namespace ANGDDEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class MaestroMemberDetailsController : ControllerBase
    {
        public static long LoggedInUserId = 0;
        public string _username = string.Empty;
        public string source = string.Empty;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DOConfiguration _objConfiguration;
        public BOMaestroMemberDetails _objBOMaestroMemberDetails = null;
        public readonly BOCommon _objBOCommon;
        private readonly ICacheService _cache;
        public MaestroMemberDetailsController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, BOCommon bOCommon, ICacheService cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _objBOMaestroMemberDetails = new BOMaestroMemberDetails(objConfiguration, _cache);
            _objBOCommon = bOCommon;
            _cache = cache;
        }

        [HttpGet]
        public ActionResult GetMaestroMemberDetails(string details = null)
        {
            DOMemberSearchCriteria memberDetails = null;
            List<DOMaestroMemberDetails> maestroMemberDetails = null;
            source = BOCommon.GetRefererURI(Request);
            try
            {
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                _objBOCommon.ErrorLog(LoggedInUserId, "MaestroAPI:GetMaestroMemberDetails", "Maestro Member Details API working", "Maestro Member Details API working");
                if (!string.IsNullOrEmpty(details))
                {
                    memberDetails = new DOMemberSearchCriteria();
                    memberDetails = JsonConvert.DeserializeObject<DOMemberSearchCriteria>(details);
                    if (memberDetails != null)
                    {
                        LoggedInUserId = memberDetails.UserId;
                    }
                }
                string query = @"SELECT * FROM all_tables WHERE table_name like '%MIIM_MEMBER_PHONE_NUMBER%'"; ///oracle query string
                maestroMemberDetails = new List<DOMaestroMemberDetails>();
                if (!string.IsNullOrWhiteSpace(query))
                {
                    maestroMemberDetails = BOMaestroMemberDetails.GetMemberDetails(query, LoggedInUserId);
                    if (maestroMemberDetails != null && maestroMemberDetails.Count > 0)
                    {
                        _objBOCommon.ErrorLog(LoggedInUserId, "MaestroAPI:GetMaestroMemberDetails", "Maestro Member Details API working:Count>0", "Maestro Member Details API working");
                    }
                }
                else
                {
                    _objBOCommon.ErrorLog(LoggedInUserId, "MaestroAPI:GetMaestroMemberDetails", "no input found", "GetMaestroMemberDetails API found no input params");
                }
                return Ok(maestroMemberDetails);
            }
            catch (Exception ex)
            {
                _objBOCommon.LogError(this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                return StatusCode((int)HttpStatusCode.InternalServerError, ex);
            }
        }
    }
}
