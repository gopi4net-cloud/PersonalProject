using ANGDDEAPI.Common;
using ANGDDEAPIBO;
using ANGDDEAPIBO.Interface;
using ANGDDEAPIDO;
using ANGDDEAPIDO.BHFacets;
using ANGDDEAPIDO.Interface;
using ANGDDEAPIFoundation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.FlowAnalysis;
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
    public class CirrusProviderAddressSearchController : ControllerBase
    {
        public static long LoggedInUserId = 0;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DOConfiguration _objConfiguration;
        private readonly IMemoryCacheHelper _memoryCacheHelper;
        private readonly BOCommon _objBOCommon;
        private readonly ICacheService _cache;

        public CirrusProviderAddressSearchController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _memoryCacheHelper = memoryCacheHelper;
            _objBOCommon = bOCommon;
            _cache = cache;
        }

        [HttpGet]
        public ActionResult GetCirrusPMGAddressDetails(string details = null)
        {
            string errLog = string.Empty;
            DOCirrusPMGAddressRequestResponse.PMGAddressSearchRequest searchRequest = null;
            List<DONICEProviderDetails> addressResponseDetails = null;
            CirrusAddressDetails boCirrusPMGAddressDetails = new CirrusAddressDetails(_objConfiguration, _memoryCacheHelper);
            try
            {
                if (!string.IsNullOrEmpty(details))
                {
                    DOCirrusPMGAddressRequestResponse.PMGAddressSearchRequest memberDetails = JsonConvert.DeserializeObject<DOCirrusPMGAddressRequestResponse.PMGAddressSearchRequest>(details);

                    if (string.IsNullOrEmpty(memberDetails.networkContractDetailId))
                    {
                        return BadRequest(new { error = "One of the mandatory params is missing, Please check networkContractDetailId." });
                    }

                    searchRequest = new DOCirrusPMGAddressRequestResponse.PMGAddressSearchRequest
                    {
                        networkContractDetailId = memberDetails.networkContractDetailId,
                        enableLog = memberDetails.enableLog ? memberDetails.enableLog : false
                    };
                    Console.WriteLine("CirrusProviderAddressSearchController - GetCirrusPMGAddressDetails Starting");
                    addressResponseDetails = new List<DONICEProviderDetails>();
                    boCirrusPMGAddressDetails.GetCirrusPMGAddressDetails(searchRequest, out addressResponseDetails, out errLog);
                    if (!string.IsNullOrEmpty(errLog) && errLog.Length > 0)
                    {
                        _objBOCommon.ErrorLog(LoggedInUserId, this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), errLog, errLog);
                        return StatusCode((int)HttpStatusCode.FailedDependency, errLog);
                    }

                    //Filter based on Eligibility Dates
                    if (memberDetails.EligibilityFromDate.HasValue)
                    {
                        if (addressResponseDetails.Count > 0 && memberDetails.EligibilityFromDate.HasValue && memberDetails.EligibilityToDate.HasValue)
                        {
                            addressResponseDetails = addressResponseDetails.Where(x =>
                                    x.EffectiveDateFrom.HasValue &&
                                    x.EffectiveDateTo.HasValue &&
                                    memberDetails.EligibilityFromDate.Value >= x.EffectiveDateFrom.Value &&
                                    memberDetails.EligibilityToDate.Value <= x.EffectiveDateTo.Value
                                ).ToList();
                        }
                        else if (addressResponseDetails.Count > 0 && memberDetails.EligibilityFromDate.HasValue)
                        {
                            addressResponseDetails = addressResponseDetails.Where(x => x.EffectiveDateFrom.HasValue
                                                     && memberDetails.EligibilityFromDate.Value >= x.EffectiveDateFrom.Value).ToList();
                        }
                    }
                    Console.WriteLine("CirrusProviderAddressSearchController - GetCirrusPMGAddressDetails Ending");
                    return Ok(addressResponseDetails);
                }
                else
                {
                    return BadRequest(new { error = "bad request" });
                }
            }
            catch (Exception ex)
            {
                _objBOCommon.ErrorLog(LoggedInUserId, this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), ex.Message, ex.ToString());
                errLog = errLog + " From GetCirrusPMGAddressDetails function " + ex.Message;
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = "An unexpected error occurred.", details = ex.Message + "" + errLog });
            }
        }


        [HttpGet]
        public ActionResult GetCirrusPCPAddressDetails(string details = null)
        {
            string errLog = string.Empty;
            DOCirrusPCPAddressRequestResponse.PCPAddressSearchRequest searchRequest = null;
            List<DONICEProviderDetails> addressResponseDetails = null;
            CirrusAddressDetails boCirrusPCPAddressDetails = new CirrusAddressDetails(_objConfiguration, _memoryCacheHelper);
            try
            {
                if (!string.IsNullOrEmpty(details))
                {
                    DOCirrusPCPAddressRequestResponse.PCPAddressSearchRequest memberDetails = JsonConvert.DeserializeObject<DOCirrusPCPAddressRequestResponse.PCPAddressSearchRequest>(details);

                    if (string.IsNullOrEmpty(memberDetails.networkContractDetailId))
                    {
                        return BadRequest(new { error = "One of the mandatory params is missing, Please check networkContractDetailId." });
                    }

                    searchRequest = new DOCirrusPCPAddressRequestResponse.PCPAddressSearchRequest
                    {
                        recipientTypeLkup = memberDetails.recipientTypeLkup,
                        networkContractDetailId = memberDetails.networkContractDetailId,
                        enableLog = memberDetails.enableLog ? memberDetails.enableLog : false
                    };
                    Console.WriteLine("CirrusProviderAddressSearchController - GetCirrusPCPAddressDetails Starting");
                    addressResponseDetails = new List<DONICEProviderDetails>();
                    boCirrusPCPAddressDetails.GetCirrusPCPAddressDetails(searchRequest, out addressResponseDetails, out errLog);
                    if (!string.IsNullOrEmpty(errLog) && errLog.Length > 0)
                    {
                        _objBOCommon.ErrorLog(LoggedInUserId, this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), errLog, errLog);
                        return StatusCode((int)HttpStatusCode.FailedDependency, errLog);
                    }

                    //Filter based on Eligibility Dates
                    if (memberDetails.EligibilityFromDate.HasValue)
                    {
                        if (addressResponseDetails.Count > 0 && memberDetails.EligibilityFromDate.HasValue && memberDetails.EligibilityToDate.HasValue)
                        {
                            addressResponseDetails = addressResponseDetails.Where(x =>
                                    x.EffectiveDateFrom.HasValue &&
                                    x.EffectiveDateTo.HasValue &&
                                    memberDetails.EligibilityFromDate.Value >= x.EffectiveDateFrom.Value &&
                                    memberDetails.EligibilityToDate.Value <= x.EffectiveDateTo.Value
                                ).ToList();
                        }
                        else if (addressResponseDetails.Count > 0 && memberDetails.EligibilityFromDate.HasValue)
                        {
                            addressResponseDetails = addressResponseDetails.Where(x => x.EffectiveDateFrom.HasValue
                                                     && memberDetails.EligibilityFromDate.Value >= x.EffectiveDateFrom.Value).ToList();
                        }
                    }
                    Console.WriteLine("CirrusProviderAddressSearchController - GetCirrusPCPAddressDetails Ending");
                    return Ok(addressResponseDetails);
                }
                else
                {
                    return BadRequest(new { error = "bad request" });
                }
            }
            catch (Exception ex)
            {
                _objBOCommon.ErrorLog(LoggedInUserId, this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), ex.Message, ex.ToString());
                errLog = errLog + " From GetCirrusPCPAddressDetails function " + ex.Message;
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = "An unexpected error occurred.", details = ex.Message + "" + errLog });
            }
        }
    }
}
