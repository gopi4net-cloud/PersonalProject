using ANGDDEAPI.Common;
using ANGDDEAPIBO;
using ANGDDEAPIDO;
using ANGDDEAPIDO.Interface;
using ANGDDEAPIFoundation;
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
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class ClaimNICEController : Controller
    {
        public static long LoggedInUserId = 0;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DOConfiguration _objConfiguration;
        private readonly IMemoryCacheHelper _memoryCacheHelper;
        private readonly BOCommon _objBOCommon;
        private readonly ICacheService _cache;

        public ClaimNICEController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _memoryCacheHelper = memoryCacheHelper;
            _objBOCommon = bOCommon;
            _cache = cache;
        }

        [HttpGet]
        public ActionResult GetClaimDetails(string strClaimSearch = null)
        {
            string errLog = string.Empty;
            ClaimSearchRequest searchRequest = null;
            DOClaimInfo claimDetails = null;
            List<DOClaimInfo> claimSearchDetails = null;
            BONICEClaimDetails boNICEClaimDetails = new BONICEClaimDetails(_objConfiguration, _cache, _objBOCommon);
            try
            {
                if (!string.IsNullOrEmpty(strClaimSearch))
                {
                    claimDetails = new DOClaimInfo();
                    claimDetails = JsonConvert.DeserializeObject<DOClaimInfo>(strClaimSearch);

                    bool isValidRequest = false;
                    if (!string.IsNullOrEmpty(claimDetails.ClaimNumber) && !string.IsNullOrEmpty(claimDetails.phsCompanyNumber) 
                            && !string.IsNullOrEmpty(claimDetails.MemberID))
                    {
                        searchRequest = new ClaimSearchRequest
                        {
                            claimNumber = (claimDetails.ClaimNumber.Length < 4) ? claimDetails.ClaimNumber.PadLeft(4, '0') : claimDetails.ClaimNumber,
                            //matchType = claimDetails.matchType,
                            memberSuffix = (claimDetails.MemberSuffix.Length < 2) ? claimDetails.MemberSuffix.PadLeft(2, '0') : claimDetails.MemberSuffix,
                            phsCompanyNumber = (claimDetails.phsCompanyNumber.Length < 2) ? claimDetails.phsCompanyNumber.PadLeft(2, '0') : claimDetails.phsCompanyNumber,
                            subscriberNumber = (claimDetails.MemberID.Length < 9) ? claimDetails.MemberID.PadLeft(9, '0') : claimDetails.MemberID
                        };
                        isValidRequest = true;
                        boNICEClaimDetails.GetClaimMatchDetails(searchRequest, true, out claimSearchDetails, out errLog);
                        if (claimSearchDetails.Count > 0 && claimDetails.StartDate.HasValue && claimDetails.EndDate.HasValue && claimDetails.TotalBilled.HasValue && claimDetails.TotalBilled > 0)
                        {
                            claimSearchDetails = claimSearchDetails.Where(x => x.StartDate >= claimDetails.StartDate && x.EndDate <= claimDetails.EndDate && x.TotalBilled == claimDetails.TotalBilled).ToList();
                        }
                        else if (claimSearchDetails.Count > 0 && claimDetails.StartDate.HasValue && claimDetails.EndDate.HasValue)
                        {
                            claimSearchDetails = claimSearchDetails.Where(x => x.StartDate >= claimDetails.StartDate && x.EndDate <= claimDetails.EndDate).ToList();
                        }
                        else if (claimSearchDetails.Count > 0 && claimDetails.StartDate.HasValue && claimDetails.TotalBilled.HasValue && claimDetails.TotalBilled>0)
                        {
                            claimSearchDetails = claimSearchDetails.Where(x => x.StartDate >= claimDetails.StartDate && x.TotalBilled == claimDetails.TotalBilled).ToList();
                        }
                        else if (claimSearchDetails.Count > 0 && claimDetails.StartDate.HasValue)
                        {
                            claimSearchDetails = claimSearchDetails.Where(x => x.StartDate >= claimDetails.StartDate).ToList();
                        }
                    }
                    else if (claimDetails.StartDate.HasValue && claimDetails.EndDate.HasValue
                        && !string.IsNullOrEmpty(claimDetails.phsCompanyNumber) && !string.IsNullOrEmpty(claimDetails.MemberID))
                    {
                        searchRequest = new ClaimSearchRequest
                        {
                            firstDateOfService = claimDetails.StartDate.Value.ToString("yyyyMMdd"),
                            lastDateOfService = claimDetails.EndDate.Value.ToString("yyyyMMdd"),
                            memberSuffix = (claimDetails.MemberSuffix.Length < 2) ? claimDetails.MemberSuffix.PadLeft(2, '0') : claimDetails.MemberSuffix,
                            phsCompanyNumber = (claimDetails.phsCompanyNumber.Length < 2) ? claimDetails.phsCompanyNumber.PadLeft(2, '0') : claimDetails.phsCompanyNumber,
                            subscriberNumber = (claimDetails.MemberID.Length < 9) ? claimDetails.MemberID.PadLeft(9, '0') : claimDetails.MemberID
                        };
                        isValidRequest = true;
                        boNICEClaimDetails.GetClaimSearchDetails(searchRequest, true, out claimSearchDetails, out errLog);

                        if (claimSearchDetails.Count > 0 && claimDetails.TotalBilled.HasValue && claimDetails.TotalBilled > 0)
                        {
                            claimSearchDetails = claimSearchDetails.Where(x => x.TotalBilled == claimDetails.TotalBilled).OrderBy(x => x.ClaimReceivedDate).ToList();
                        }
                    }
                    else if (claimDetails.StartDate.HasValue && claimDetails.TotalBilled.HasValue && claimDetails.TotalBilled > 0
                        && !string.IsNullOrEmpty(claimDetails.phsCompanyNumber) && !string.IsNullOrEmpty(claimDetails.MemberID))
                    {
                        searchRequest = new ClaimSearchRequest
                        {
                            firstDateOfService = claimDetails.StartDate.Value.ToString("yyyyMMdd"),
                            lastDateOfService = claimDetails.StartDate.Value.ToString("yyyyMMdd"),
                            memberSuffix = (claimDetails.MemberSuffix.Length < 2) ? claimDetails.MemberSuffix.PadLeft(2, '0') : claimDetails.MemberSuffix,
                            phsCompanyNumber = (claimDetails.phsCompanyNumber.Length < 2) ? claimDetails.phsCompanyNumber.PadLeft(2, '0') : claimDetails.phsCompanyNumber,
                            subscriberNumber = (claimDetails.MemberID.Length < 9) ? claimDetails.MemberID.PadLeft(9, '0') : claimDetails.MemberID
                        };
                        isValidRequest = true;
                        boNICEClaimDetails.GetClaimSearchDetails(searchRequest, true, out claimSearchDetails, out errLog);

                        if (claimSearchDetails.Count > 0 && claimDetails.TotalBilled.HasValue && claimDetails.TotalBilled > 0)
                        {
                            claimSearchDetails = claimSearchDetails.Where(x => x.TotalBilled == claimDetails.TotalBilled).OrderBy(x=>x.ClaimReceivedDate).ToList();
                        }
                    }
                    else
                    {
                        return BadRequest(new { error = "One of the mandatory params is missing, Please check either ClaimNumber, MemberSuffix, PHSCompanyNumber and MemberId or FirstDateOfService, LastDateOfService, MemberSuffix, PHSCompanyNumber and MemberId are provided." });
                    }

                    if (!isValidRequest)
                    {
                        return BadRequest(new { error = "One of the mandatory params is missing, Please check either ClaimNumber, MemberSuffix, PHSCompanyNumber and MemberId or FirstDateOfService, LastDateOfService, MemberSuffix, PHSCompanyNumber and MemberId are provided." });
                    }
                }
                return Ok(claimSearchDetails);
            }
            catch (Exception ex)
            {
                errLog = errLog + " From GetClaimDetails function " + ex.Message;
                _objBOCommon.ErrorLog(LoggedInUserId, this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), ex.Message, ex.ToString());
                return StatusCode((int)HttpStatusCode.InternalServerError,new { error = "An unexpected error occurred.", details = ex.Message + "" + errLog });
            }
        }

        [HttpGet]
        public ActionResult GetClaimLineDetails(string strClaimSearch = null)
        {
            string errLog = string.Empty;
            ClaimSearchRequest searchRequest = null;
            DOClaimInfo claimDetails = null;
            List<DOClaimInfo> claimSearchDetails = null;
            BONICEClaimDetails boNICEClaimDetails = new BONICEClaimDetails(_objConfiguration, _cache, _objBOCommon);
            try
            {
                if (!string.IsNullOrEmpty(strClaimSearch))
                {
                    claimDetails = new DOClaimInfo();
                    claimDetails = JsonConvert.DeserializeObject<DOClaimInfo>(strClaimSearch);

                    bool isValidRequest = false;
                    if (!string.IsNullOrEmpty(claimDetails.AdmissionDate) && !string.IsNullOrEmpty(claimDetails.ClaimNumber) && !string.IsNullOrEmpty(claimDetails.MemberSuffix)
                        && !string.IsNullOrEmpty(claimDetails.DateOfBirth) && !string.IsNullOrEmpty(claimDetails.phsCompanyNumber) && !string.IsNullOrEmpty(claimDetails.MemberID))
                    {
                        //To trim member id 
                        try
                        {
                            if (!string.IsNullOrEmpty(claimDetails.MemberID))
                            {
                                int tempMemberId = Convert.ToInt32(claimDetails.MemberID);
                                claimDetails.MemberID = tempMemberId.ToString();
                            }
                        }
                        catch
                        {
                            // Ignore parse errors
                        }

                        searchRequest = new ClaimSearchRequest
                        {
                            admissionDate = claimDetails.AdmissionDate,
                            batchOrRealtimeFlag = "R",
                            claimNumber = (claimDetails.ClaimNumber.Length < 4) ? claimDetails.ClaimNumber.PadLeft(4, '0') : claimDetails.ClaimNumber,
                            memberSuffix = (claimDetails.MemberSuffix.Length < 2) ? claimDetails.MemberSuffix.PadLeft(2, '0') : claimDetails.MemberSuffix,
                            phsCompanyNumber = (claimDetails.phsCompanyNumber.Length < 2) ? claimDetails.phsCompanyNumber.PadLeft(2, '0') : claimDetails.phsCompanyNumber,
                            subscriberNumber = (claimDetails.MemberID.Length < 7) ? claimDetails.MemberID.PadLeft(7, '0') : claimDetails.MemberID,
                            dateOfBirth = claimDetails.DateOfBirth,
                            providerTaxId = claimDetails.ProviderTaxID
                        };
                        isValidRequest = true;
                        boNICEClaimDetails.GetClaimLineDetails(searchRequest, out claimSearchDetails,out errLog);
                    }
                    if (!isValidRequest)
                    {
                        return BadRequest(new { error = "One of the mandatory params is missing, Please check either AdmissionDate, ClaimNumber, MemberSuffix, PHSCompanyNumber, MemberId, DateOfBirth, ProviderTaxID are provided." });
                    }
                }
                return Ok(claimSearchDetails);
            }
            catch (Exception ex)
            {
                _objBOCommon.ErrorLog(LoggedInUserId, this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message, ex.ToString());
                errLog = errLog + " From GetClaimLineDetails function " + ex.Message;
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = "An unexpected error occurred.", details = ex.Message + "" + errLog });
            }
        }

        [HttpGet]
        public ActionResult GetClaimStatusDetails(string strClaimSearch = null)
        {
            string errLog = string.Empty;
            ClaimSearchRequest searchRequest = null;
            DOClaimInfo claimDetails = null;
            List<DOClaimLine> claimStatusDetails = null;
            BONICEClaimDetails boNICEClaimDetails = new BONICEClaimDetails(_objConfiguration, _cache, _objBOCommon);
            try
            {
                if (!string.IsNullOrEmpty(strClaimSearch))
                {
                    claimDetails = new DOClaimInfo();
                    claimDetails = JsonConvert.DeserializeObject<DOClaimInfo>(strClaimSearch);

                    bool isValidRequest = false;
                    if (!string.IsNullOrEmpty(claimDetails.ClaimNumber) && !string.IsNullOrEmpty(claimDetails.MemberSuffix)
                        && !string.IsNullOrEmpty(claimDetails.phsCompanyNumber) && !string.IsNullOrEmpty(claimDetails.MemberID))
                    {
                        searchRequest = new ClaimSearchRequest
                        {
                            claimNumber = (claimDetails.ClaimNumber.Length < 4) ? claimDetails.ClaimNumber.PadLeft(4, '0') : claimDetails.ClaimNumber,
                            memberSuffix = (claimDetails.MemberSuffix.Length < 2) ? claimDetails.MemberSuffix.PadLeft(2, '0') : claimDetails.MemberSuffix,
                            phsCompanyNumber = (claimDetails.phsCompanyNumber.Length < 2) ? claimDetails.phsCompanyNumber.PadLeft(2, '0') : claimDetails.phsCompanyNumber,
                            subscriberNumber = (claimDetails.MemberID.Length < 9) ? claimDetails.MemberID.PadLeft(9, '0') : claimDetails.MemberID
                        };
                        isValidRequest = true;
                        boNICEClaimDetails.GetClaimStatusDetails(searchRequest, out claimStatusDetails, out errLog);
                    }
                    if (!isValidRequest)
                    {
                        return BadRequest(new { error = "One of the mandatory params is missing, Please check either ClaimNumber, MemberSuffix, PHSCompanyNumber and MemberId are provided." });
                        }
                }
                return Ok(claimStatusDetails);
            }
            catch (Exception ex)
            {
                _objBOCommon.ErrorLog(LoggedInUserId, this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), ex.Message, ex.ToString());
                errLog = errLog + " From GetClaimStatusDetails function " + ex.Message;
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = "An unexpected error occurred.", details = ex.Message + "" + errLog });
            }
        }

        [HttpGet]
        public ActionResult GetClaimProviderDetails(string strClaimSearch = null)
        {
            string errLog = string.Empty;
            ClaimSearchRequest searchRequest = null;
            DOClaimInfo claimDetails = null;
            DOClaimInfo claimSearchDetails = null;
            BONICEClaimDetails boNICEClaimDetails = new BONICEClaimDetails(_objConfiguration, _cache, _objBOCommon);
            try
            {
                if (!string.IsNullOrEmpty(strClaimSearch))
                {
                    claimDetails = new DOClaimInfo();
                    claimDetails = JsonConvert.DeserializeObject<DOClaimInfo>(strClaimSearch);

                    bool isValidRequest = false;
                    if (!string.IsNullOrEmpty(claimDetails.ClaimNumber))
                    {
                        searchRequest = new ClaimSearchRequest
                        {
                            claimNumber = (claimDetails.ClaimNumber.Length < 4) ? claimDetails.ClaimNumber.PadLeft(4, '0') : claimDetails.ClaimNumber,
                            //matchType = claimDetails.matchType,
                            memberSuffix = (claimDetails.MemberSuffix.Length < 2) ? claimDetails.MemberSuffix.PadLeft(2, '0') : claimDetails.MemberSuffix,
                            phsCompanyNumber = (claimDetails.phsCompanyNumber.Length < 2) ? claimDetails.phsCompanyNumber.PadLeft(2, '0') : claimDetails.phsCompanyNumber,
                            subscriberNumber = (claimDetails.MemberID.Length < 9) ? claimDetails.MemberID.PadLeft(9, '0') : claimDetails.MemberID
                        };
                        isValidRequest = true;
                        boNICEClaimDetails.GetClaimProviderHistory(searchRequest, out claimSearchDetails, out errLog);
                    }
                    else
                    {
                        return BadRequest(new { error = "One of the mandatory params is missing, Please check either ClaimNumber, MemberSuffix, PHSCompanyNumber and MemberId are provided." });
                    }

                    if (!isValidRequest)
                    {
                        return BadRequest(new { error = "One of the mandatory params is missing, Please check either ClaimNumber, MemberSuffix, PHSCompanyNumber and MemberId are provided." });
                    }
                }
                return Ok(claimSearchDetails);
            }
            catch (Exception ex)
            {
                _objBOCommon.ErrorLog(LoggedInUserId, this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), ex.Message, ex.ToString());
                errLog = errLog + " From GetClaimProviderDetails function " + ex.Message;
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = "An unexpected error occurred.", details = ex.Message + "" + errLog });
            }
        }

        [HttpPost]
        public async Task<ClaimSearchResponse> UpdateNiceComments(string strClaimSearch = null)
        {
            string status = string.Empty;
            ClaimSearchResponse objDOClaimResponse = new ClaimSearchResponse();
            ClaimSearchRequest searchRequest = null;
            DOClaimInfo claimDetails = null;
            BONICEClaimDetails boNICEClaimDetails = new BONICEClaimDetails(_objConfiguration, _cache, _objBOCommon);
            try
            {
                if (!string.IsNullOrEmpty(strClaimSearch))
                {
                    claimDetails = new DOClaimInfo();
                    claimDetails = JsonConvert.DeserializeObject<DOClaimInfo>(strClaimSearch);
                    bool isValidRequest = false;
                    if (!string.IsNullOrEmpty(claimDetails.ClaimNumber) && !string.IsNullOrEmpty(claimDetails.MemberSuffix)
                        && !string.IsNullOrEmpty(claimDetails.phsCompanyNumber) && !string.IsNullOrEmpty(claimDetails.MemberID) && !string.IsNullOrEmpty(claimDetails.ClaimComment))
                    {
                        searchRequest = new ClaimSearchRequest
                        {
                            claimComment = claimDetails.ClaimComment,
                            claimNumber = (claimDetails.ClaimNumber.Length < 4) ? claimDetails.ClaimNumber.PadLeft(4, '0') : claimDetails.ClaimNumber,
                            commentType = "OB",
                            memberSuffix = (claimDetails.MemberSuffix.Length < 2) ? claimDetails.MemberSuffix.PadLeft(2, '0') : claimDetails.MemberSuffix,
                            phsCompanyNumber = (claimDetails.phsCompanyNumber.Length < 2) ? claimDetails.phsCompanyNumber.PadLeft(2, '0') : claimDetails.phsCompanyNumber,
                            subscriberNumber = (claimDetails.MemberID.Length < 9) ? claimDetails.MemberID.PadLeft(9, '0') : claimDetails.MemberID
                        };
                        isValidRequest = true;
                        objDOClaimResponse.Status = await boNICEClaimDetails.UpdateNiceComments(searchRequest);
                        if (objDOClaimResponse.Status == "S") {
                            objDOClaimResponse.ErrorMessage = string.Empty;
                        }
                        else
                        {
                            objDOClaimResponse.ErrorMessage = "Failed to update comment";
                        }
                    }
                    else
                    {
                        objDOClaimResponse.Status = "Failed";
                        objDOClaimResponse.ErrorMessage = "One of the mandatory params is missing, Please check either Comments, ClaimNumber, MemberSuffix, PHSCompanyNumber and MemberId are provided.";
                    }
                    if (!isValidRequest)
                    {
                        objDOClaimResponse.Status = "Failed";
                        objDOClaimResponse.ErrorMessage = "One of the mandatory params is missing, Please check either Comments, ClaimNumber, MemberSuffix, PHSCompanyNumber and MemberId are provided.";
                    }
                }
            }
            catch (Exception ex)
            {
                objDOClaimResponse.Status = "Failed";
                objDOClaimResponse.ErrorMessage = ex.Message;
                _objBOCommon.ErrorLog(LoggedInUserId, this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), ex.Message, ex.ToString());
            }
            return objDOClaimResponse;
        }
    }
}