using ANGDDEAPI.Common;
using ANGDDEAPIBO;
using ANGDDEAPIDO;
using ANGDDEAPIDO.Interface;
using ANGDDEAPIFoundation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace ANGDDEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class BrokerComplaintsController : ControllerBase
    {
        public static long LoggedInUserId = 0;
        ExceptionTypes _result;
        public string _username = string.Empty;
        string source = string.Empty;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DOConfiguration _objConfiguration;
        private readonly ICacheService _cache;
        public readonly BOCommon _objBOCommon;
        //public readonly AuthorizeUser _authUser;
        public BrokerComplaintsController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, ICacheService cache, BOCommon bOCommon)
        {
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _cache = cache;
            _objBOCommon = bOCommon;
            //_authUser = authUser;
        }
        [HttpGet]
        public ActionResult GetByBrokerComplaints([FromQuery] DOBrokerComplaints objDOBrokerDetails)
        {
            List<DOBrokerComplaints> listdOBrokerDetails = new List<DOBrokerComplaints>();
            try
            {
                BOBrokerComplaints bOBrokerComplaints = new BOBrokerComplaints(_objConfiguration, _cache);
                _result = bOBrokerComplaints.GetBrokerComplaintsDetails(objDOBrokerDetails, out listdOBrokerDetails);

                if (listdOBrokerDetails == null || listdOBrokerDetails.Count == 0)
                {
                    //for submitted state ,new view
                    _result = bOBrokerComplaints.GetBrokerComplaintsDetailsForSubmittedStatus(objDOBrokerDetails, out listdOBrokerDetails);

                }
                if (_result != ExceptionTypes.Success && _result != ExceptionTypes.ZeroRecords)
                {
                    return BadRequest("Internal Server Error.");
                }

                if (_result == (long)ExceptionTypes.Success)
                {
                    return Ok(listdOBrokerDetails);
                }
                else
                {
                    return BadRequest("Internal Server Error.");
                }
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
}
