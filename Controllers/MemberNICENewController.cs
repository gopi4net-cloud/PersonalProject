using ANGDDEAPI.Common;
using ANGDDEAPIBO;
using ANGDDEAPIDO;
using ANGDDEAPIDO.Interface;
using ANGDDEAPIFoundation;
using Elastic.Apm.Api;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static ANGDDEAPIDO.DONICESearchRequest;

namespace ANGDDEAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class MemberNICENewController : ControllerBase
    {

        public static long LoggedInUserId = 0;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DOConfiguration _objConfiguration;
        private readonly IMemoryCacheHelper _memoryCacheHelper;
        private readonly BOCommon _objBOCommon;
        private readonly ICacheService _cache;

        public MemberNICENewController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _memoryCacheHelper = memoryCacheHelper;
            _objBOCommon = bOCommon;
            _cache = cache;
        }
        [HttpGet]
        public ActionResult GetMemberSearchDetails(string details = null)
        {
            string errLog = string.Empty;
            MemberSearchRequest searchRequest = null;
            MemberSearchRequest searchAdditionalFilterRequest = null;
            DOMemberSearchCriteria memberDetails = null;
            List<DOGPSMemberDetails> memberSearchDetails = null;
            BONICEMemberDetailsNew boNICEMemberDetails = new BONICEMemberDetailsNew(_objConfiguration, _cache);
            try
            {
                if (!string.IsNullOrEmpty(details))
                {
                    bool isValidRequest = false;

                    // Check if details contain DateOfBirth
                    var detailsJson = JsonConvert.DeserializeObject<Dictionary<string, string>>(details);
                    if (!detailsJson.ContainsKey("DateOfBirth") || string.IsNullOrEmpty(detailsJson["DateOfBirth"]))
                    {
                        detailsJson["DateOfBirth"] = DateTime.MinValue.ToString("MM/dd/yyyy");
                    }
                    // Re-serialize the updated detailsJson back to a JSON string
                    details = JsonConvert.SerializeObject(detailsJson);

                    memberDetails = new DOMemberSearchCriteria();
                    memberDetails = JsonConvert.DeserializeObject<DOMemberSearchCriteria>(details);
                   
                    searchRequest = new MemberSearchRequest
                    {
                        phsCompanyNumber = (memberDetails.phsCompanyNumber.Length < 2) ? memberDetails.phsCompanyNumber.PadLeft(2, '0') : memberDetails.phsCompanyNumber,
                        subscriberNumber = (memberDetails.MemberId.Length < 9) ? memberDetails.MemberId.PadLeft(9, '0') : memberDetails.MemberId,
                        firstName = memberDetails.FirstName,
                        lastName = memberDetails.LastName,
                        planTypeCode = memberDetails.planTypeCode                       
                    };
                    isValidRequest = true;

                    string dt=null;
                    if(memberDetails.DateOfBirth > DateTime.MinValue)
                    {
                        dt = memberDetails.DateOfBirth.ToString("MM/dd/yyyy");
                    }

                    searchAdditionalFilterRequest = new MemberSearchRequest
                    {
                        firstName = memberDetails.FirstName,
                        lastName = memberDetails.LastName,
                        dateOfBirth = dt,
                        planTypeCode = memberDetails.planTypeCode,
                        memberSuffix = null,
                        enableLog = memberDetails.enableLog ? memberDetails.enableLog : false
                    };


                    if (!isValidRequest)
                    {
                        return BadRequest(new { error = "bad request" });
                    }

                    boNICEMemberDetails.GetMemberSearchDetails(searchRequest, searchAdditionalFilterRequest, out memberSearchDetails, out errLog);
                }
                return Ok(memberSearchDetails);
            }
            catch (Exception ex)
            {
                _objBOCommon.ErrorLog(LoggedInUserId, this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), ex.Message, ex.ToString());
                errLog = errLog + " From GetMemberSearchDetails function " + ex.Message;
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = "An unexpected error occurred.", details = ex.Message + "" + errLog });
            }
        }

        [HttpGet]
        public ActionResult GetMemberDetails(string details = null)
        {
            string errLog = string.Empty;
            MemberSearchRequest searchRequest = null;
            MemberSearchRequest searchAdditionalRequest = null;
            DOMemberSearchCriteria memberDetails = null;
            List<DOGPSMemberDetails> memberSearchDetails = null;
            BONICEMemberDetailsNew boNICEMemberDetails = new BONICEMemberDetailsNew(_objConfiguration, _cache);
            try
            {
                if (!string.IsNullOrEmpty(details))
                {
                    // Check if details contain DateOfBirth
                    var detailsJson = JsonConvert.DeserializeObject<Dictionary<string, string>>(details);
                    if (!detailsJson.ContainsKey("DateOfBirth") || string.IsNullOrEmpty(detailsJson["DateOfBirth"]))
                    {
                        detailsJson["DateOfBirth"] = DateTime.MinValue.ToString("MM/dd/yyyy");
                    }
                    // Re-serialize the updated detailsJson back to a JSON string
                    details = JsonConvert.SerializeObject(detailsJson);

                    memberDetails = new DOMemberSearchCriteria();
                    memberDetails = JsonConvert.DeserializeObject<DOMemberSearchCriteria>(details);          

                    searchRequest = new MemberSearchRequest
                    {
                        memberSuffix = (memberDetails.MemberSuffix.Length < 2) ? memberDetails.MemberSuffix.PadLeft(2, '0') : memberDetails.MemberSuffix,
                        phsCompanyNumber = (memberDetails.phsCompanyNumber.Length < 2) ? memberDetails.phsCompanyNumber.PadLeft(2, '0') : memberDetails.phsCompanyNumber,
                        subscriberNumber = (memberDetails.MemberId.Length < 9) ? memberDetails.MemberId.PadLeft(9, '0') : memberDetails.MemberId
                    };

                    bool isValidRequest = false;
                    if (!string.IsNullOrEmpty(searchRequest.memberSuffix) || !string.IsNullOrEmpty(searchRequest.phsCompanyNumber)
                        || !string.IsNullOrEmpty(searchRequest.subscriberNumber))
                    {
                        isValidRequest = true;
                    }
                    if (!isValidRequest)
                    {
                        return BadRequest(new { error = "One of the mandatory params is missing, Please check MemberSuffix, PHSCompanyNumber and MemberId are provided." });
                    }

                    string dt = null;
                    if (memberDetails.DateOfBirth > DateTime.MinValue)
                    {
                        dt = memberDetails.DateOfBirth.ToString("MM/dd/yyyy");
                    }
                    searchAdditionalRequest = new MemberSearchRequest
                    {
                        firstName = memberDetails.FirstName,
                        lastName = memberDetails.LastName,
                        dateOfBirth = dt,
                        planTypeCode = memberDetails.planTypeCode,
                        memberSuffix = (memberDetails.MemberSuffix.Length < 2) ? memberDetails.MemberSuffix.PadLeft(2, '0') : memberDetails.MemberSuffix,
                    };
                    

                    boNICEMemberDetails.GetMemberDetails(searchRequest, searchAdditionalRequest, out memberSearchDetails, out errLog);
                }
                return Ok(memberSearchDetails);
            }
            catch (Exception ex)
            {
                _objBOCommon.ErrorLog(LoggedInUserId, this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), ex.Message, ex.ToString());
                errLog = errLog + " From GetMemberDetails function " + ex.Message;
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = "An unexpected error occurred.", details = ex.Message + "" + errLog });
            }
        }

        [HttpGet]
        public ActionResult GetMemberEligibility(string details = null)
        {
            string errLog = string.Empty;
            MemberSearchRequest searchRequest = null;
            DOMemberSearchCriteria memberDetails = null;
            List<DOMemberPlanDetails> memberSearchDetails = null;
            BONICEMemberDetailsNew boNICEMemberDetails = new BONICEMemberDetailsNew(_objConfiguration, _cache);
            try
            {
                if (!string.IsNullOrEmpty(details))
                {
                    memberDetails = new DOMemberSearchCriteria();
                    memberDetails = JsonConvert.DeserializeObject<DOMemberSearchCriteria>(details);
                    searchRequest = new MemberSearchRequest
                    {
                        memberSuffix = (memberDetails.MemberSuffix.Length < 2) ? memberDetails.MemberSuffix.PadLeft(2, '0') : memberDetails.MemberSuffix,
                        phsCompanyNumber = (memberDetails.phsCompanyNumber.Length < 2) ? memberDetails.phsCompanyNumber.PadLeft(2, '0') : memberDetails.phsCompanyNumber,
                        subscriberNumber = (memberDetails.MemberId.Length < 9) ? memberDetails.MemberId.PadLeft(9, '0') : memberDetails.MemberId
                    };

                    bool isValidRequest = false;
                    if (!string.IsNullOrEmpty(searchRequest.memberSuffix) || !string.IsNullOrEmpty(searchRequest.phsCompanyNumber)
                        || !string.IsNullOrEmpty(searchRequest.subscriberNumber))
                    {
                        isValidRequest = true;
                    }
                    if (!isValidRequest)
                    {
                        return BadRequest(new { error = "One of the mandatory params is missing, Please check MemberSuffix, PHSCompanyNumber and MemberId are provided." });
                    }

                    boNICEMemberDetails.GetMemberEligibility(searchRequest, out memberSearchDetails, out errLog);
                }
                return Ok(memberSearchDetails);
            }
            catch (Exception ex)
            {
                _objBOCommon.ErrorLog(LoggedInUserId, this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), ex.Message, ex.ToString());
                errLog = errLog + " From GetMemberEligibility function " + ex.Message;
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = "An unexpected error occurred.", details = ex.Message + "" + errLog });
            }
        }

        [HttpGet]
        public ActionResult GetMemberProviderHistory(string details = null)
        {
            string errLog = string.Empty;
            MemberSearchRequest searchRequest = null;
            DOMemberSearchCriteria memberDetails = null;
            List<DOMemberPlanDetails> memberProviderDetails = null;
            BONICEMemberDetailsNew boNICEMemberDetails = new BONICEMemberDetailsNew(_objConfiguration, _cache);
            try
            {
                if (!string.IsNullOrEmpty(details))
                {
                    memberDetails = new DOMemberSearchCriteria();
                    memberDetails = JsonConvert.DeserializeObject<DOMemberSearchCriteria>(details);
                    searchRequest = new MemberSearchRequest
                    {
                        memberSuffix = (memberDetails.MemberSuffix.Length < 2) ? memberDetails.MemberSuffix.PadLeft(2, '0') : memberDetails.MemberSuffix,
                        phsCompanyNumber = (memberDetails.phsCompanyNumber.Length < 2) ? memberDetails.phsCompanyNumber.PadLeft(2, '0') : memberDetails.phsCompanyNumber,
                        subscriberNumber = (memberDetails.MemberId.Length < 9) ? memberDetails.MemberId.PadLeft(9, '0') : memberDetails.MemberId
                    };

                    bool isValidRequest = false;
                    if (!string.IsNullOrEmpty(searchRequest.memberSuffix) || !string.IsNullOrEmpty(searchRequest.phsCompanyNumber)
                        || !string.IsNullOrEmpty(searchRequest.subscriberNumber))
                    {
                        isValidRequest = true;
                    }
                    if (!isValidRequest)
                    {
                        return BadRequest(new { error = "One of the mandatory params is missing, Please check MemberSuffix, PHSCompanyNumber and MemberId are provided." });
                    }

                    boNICEMemberDetails.GetMemberProviderHistory(searchRequest, out memberProviderDetails, out errLog);
                }
                return Ok(memberProviderDetails);
            }
            catch (Exception ex)
            {
                _objBOCommon.ErrorLog(LoggedInUserId, this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), ex.Message, ex.ToString());
                errLog = errLog + " From GetMemberProviderHistory function " + ex.Message;
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = "An unexpected error occurred.", details = ex.Message + "" + errLog });
            }
        }
    }
}
