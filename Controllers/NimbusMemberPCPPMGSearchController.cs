using ANGDDEAPI.Common;
using ANGDDEAPIBO;
using ANGDDEAPIBO.Interface;
using ANGDDEAPIDO;
using ANGDDEAPIDO.BHFacets;
using ANGDDEAPIDO.Interface;
using ANGDDEAPIFoundation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using static ANGDDEAPIDO.DONICESearchRequest;

namespace ANGDDEAPI.Controllers
{
    /// <summary>
    /// NEW DotNet core
    /// </summary>
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class NimbusMemberPCPPMGSearchController : ControllerBase
    {
        public static long LoggedInUserId = 0;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DOConfiguration _objConfiguration;
        private readonly IMemoryCacheHelper _memoryCacheHelper;
        private readonly BOCommon _objBOCommon;
        private readonly ICacheService _cache;

        public NimbusMemberPCPPMGSearchController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _memoryCacheHelper = memoryCacheHelper;
            _objBOCommon = bOCommon;
            _cache = cache;
        }

        [HttpGet]
        public ActionResult GetNimbusMemberPCPPMGDetails(string details = null)
        {
            string errLog = string.Empty;
            DONimbusRequestResponse.PCPPMGSearchRequest searchRequest = null;
            List<DONimbusRequestResponse.PCPPMGSearchResponse> memberResponseDetails = null;
            NimbusMemberPCPPMGDetails boNimbusMemberDetails = new NimbusMemberPCPPMGDetails(_objConfiguration, _memoryCacheHelper);
            try
            {
                if (!string.IsNullOrEmpty(details))
                {
                    DONimbusRequestResponse.PCPPMGSearchRequest memberDetails = JsonConvert.DeserializeObject<DONimbusRequestResponse.PCPPMGSearchRequest>(details);

                    if (string.IsNullOrEmpty(memberDetails.memberId) || string.IsNullOrEmpty(memberDetails.memberSuffix) || string.IsNullOrEmpty(memberDetails.groupNumber))
                    {
                        return BadRequest(new { error = "One of the mandatory params is missing, Please check memberId, memberSuffix and GroupNumber are provided." });
                    }

                    searchRequest = new DONimbusRequestResponse.PCPPMGSearchRequest
                    {
                        memberId = (memberDetails.memberId.Length < 9) ? memberDetails.memberId.PadLeft(9, '0') : memberDetails.memberId,
                        memberSuffix = (memberDetails.memberSuffix.Length < 2) ? memberDetails.memberSuffix.PadLeft(2, '0') : memberDetails.memberSuffix,
                        groupNumber = memberDetails.groupNumber,
                    };

                    memberResponseDetails = new List<DONimbusRequestResponse.PCPPMGSearchResponse>();
                    boNimbusMemberDetails.GetCirrusPMGPCPDetails(searchRequest, out memberResponseDetails, out errLog);

                    //Filter based on Eligibility Dates
                    if (memberDetails.EligibilityFromDate.HasValue)
                    {
                        if (memberResponseDetails.Count > 0 && memberDetails.EligibilityFromDate.HasValue && memberDetails.EligibilityToDate.HasValue)
                        {
                            memberResponseDetails = memberResponseDetails.Where(x => x.PCPEffectiveDateFrom >= memberDetails.EligibilityFromDate && x.PCPEffectiveDateTo <= memberDetails.EligibilityToDate).ToList();
                        }
                        else if (memberResponseDetails.Count > 0 && memberDetails.EligibilityFromDate.HasValue)
                        {
                            memberResponseDetails = memberResponseDetails.Where(x => x.PCPEffectiveDateFrom >= memberDetails.EligibilityFromDate).ToList();
                        }
                    }

                    return Ok(memberResponseDetails);
                }
                else
                {
                    return BadRequest(new { error = "bad request" });
                }
            }
            catch (Exception ex)
            {
                _objBOCommon.ErrorLog(LoggedInUserId, this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), ex.Message, ex.ToString());
                errLog = errLog + " From GetMemberPCPPMGDetails function " + ex.Message;
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = "An unexpected error occurred.", details = ex.Message + "" + errLog });
            }
        }
        [HttpGet]
        public ActionResult GetNimbusQmSCOGDetails(string details = null)
        {
            string errLog = string.Empty;
            DONimbusRequestResponse.PCPPMGSearchRequest searchRequest = null;
            DONimbusRequestResponse.CirrusQMSCOResponse cirrusQMSCOResponse = null;
            NimbusMemberPCPPMGDetails boNimbusMemberDetails = new NimbusMemberPCPPMGDetails(_objConfiguration, _memoryCacheHelper);
            try
            {
                if (!string.IsNullOrEmpty(details))
                {
                    DONimbusRequestResponse.PCPPMGSearchRequest memberDetails = JsonConvert.DeserializeObject<DONimbusRequestResponse.PCPPMGSearchRequest>(details);

                    if (string.IsNullOrEmpty(memberDetails.memberId) || string.IsNullOrEmpty(memberDetails.memberSuffix) || string.IsNullOrEmpty(memberDetails.groupNumber))
                    {
                        return BadRequest(new { error = "One of the mandatory params is missing, Please check memberId, memberSuffix and GroupNumber are provided." });
                    }

                    searchRequest = new DONimbusRequestResponse.PCPPMGSearchRequest
                    {
                        memberId = (memberDetails.memberId.Length < 9) ? memberDetails.memberId.PadLeft(9, '0') : memberDetails.memberId,
                        //memberSuffix = (memberDetails.memberSuffix.Length < 2) ? memberDetails.memberSuffix.PadLeft(2, '0') : memberDetails.memberSuffix,
                        memberSuffix = int.TryParse(memberDetails.memberSuffix, out var suffixNum) ? suffixNum.ToString("D2") : memberDetails.memberSuffix?.PadLeft(2, '0'),
                        groupNumber = memberDetails.groupNumber,
                        EligibilityFromDate = memberDetails.EligibilityFromDate,
                        EligibilityToDate = memberDetails.EligibilityToDate,
                        setNumber = memberDetails.setNumber
                    };

                    cirrusQMSCOResponse = new DONimbusRequestResponse.CirrusQMSCOResponse();
                    cirrusQMSCOResponse = boNimbusMemberDetails.GetCirrusQMSCODetails(searchRequest, "M", searchRequest.setNumber, searchRequest.EligibilityFromDate, searchRequest.EligibilityToDate);



                    return Ok(cirrusQMSCOResponse);
                }
                else
                {
                    return BadRequest(new { error = "bad request" });
                }
            }
            catch (Exception ex)
            {
                _objBOCommon.ErrorLog(LoggedInUserId, this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), ex.Message, ex.ToString());
                errLog = errLog + " From GetMemberPCPPMGDetails function " + ex.Message;
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = "An unexpected error occurred.", details = ex.Message + "" + errLog });
            }
        }
    }
}
