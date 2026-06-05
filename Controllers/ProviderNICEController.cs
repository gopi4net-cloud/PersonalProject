using ANGDDEAPI.Common;
using ANGDDEAPIBO;
using ANGDDEAPIDO;
using ANGDDEAPIDO.BHFacets;
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
    public class ProviderNICEController : Controller
    {
        public static long LoggedInUserId = 0;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DOConfiguration _objConfiguration;
        private readonly IMemoryCacheHelper _memoryCacheHelper;
        private readonly BOCommon _objBOCommon;
        private readonly ICacheService _cache;

        public ProviderNICEController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
        {
            Console.WriteLine("ProviderNICEController - Initialising");
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _memoryCacheHelper = memoryCacheHelper;
            _objBOCommon = bOCommon;
            _cache = cache;
            Console.WriteLine("ProviderNICEController - Initialising completed");
        }
        [HttpGet]
        public ActionResult GetProviderSearchDetails(string details = null)
        {
            string errMsg = string.Empty;
            ProviderSearchRequest searchRequest = null;
            DOProviderSearchCriteria providerDetails = null;
            List<DONICEProviderDetails> providerSearchDetails = null;
            BONICEProviderDetails boNICEProviderDetails = new BONICEProviderDetails(_objConfiguration, _cache);
            try
            {
                if (!string.IsNullOrEmpty(details))
                {
                    bool isValidRequest = false;
                    providerDetails = new DOProviderSearchCriteria();
                    providerDetails = JsonConvert.DeserializeObject<DOProviderSearchCriteria>(details);
                    if (providerDetails != null && !string.IsNullOrEmpty(providerDetails.NPI)
                        && !string.IsNullOrEmpty(providerDetails.PHSCompanyNumber) && !string.IsNullOrEmpty(providerDetails.ContractPlanType))
                    {
                        searchRequest = new ProviderSearchRequest
                        {
                            NPI = providerDetails.NPI,
                            phsCompanyNumber = (providerDetails.PHSCompanyNumber.Length < 2) ? providerDetails.PHSCompanyNumber.PadLeft(2, '0') : providerDetails.PHSCompanyNumber,
                            contractPlanType = providerDetails.ContractPlanType,
                            providerSearchType = string.IsNullOrEmpty(providerDetails.ProviderSearchType)? "O" : providerDetails.ProviderSearchType,
                            searchCategory = string.IsNullOrEmpty(providerDetails.SearchCategory) ? "N" : providerDetails.SearchCategory,
                            enableLog = providerDetails.enableLog ? providerDetails.enableLog : false
                        };
                        isValidRequest = true;
                    }
                    else if (providerDetails != null && !string.IsNullOrEmpty(providerDetails.ProviderGroupFacility) && !string.IsNullOrEmpty(providerDetails.ProviderGroupNumber) 
                        && !string.IsNullOrEmpty(providerDetails.PHSCompanyNumber) && !string.IsNullOrEmpty(providerDetails.ContractPlanType))
                    {
                        searchRequest = new ProviderSearchRequest
                        {                            
                            providerGroupFacility = (providerDetails.ProviderGroupFacility.Length < 4) ? providerDetails.ProviderGroupFacility.PadLeft(4, '0') : providerDetails.ProviderGroupFacility,
                            providerGroupNumber = (providerDetails.ProviderGroupNumber.Length < 7)? providerDetails.ProviderGroupNumber.PadLeft(7, '0') : providerDetails.ProviderGroupNumber,
                            phsCompanyNumber = (providerDetails.PHSCompanyNumber.Length < 2) ? providerDetails.PHSCompanyNumber.PadLeft(2, '0') : providerDetails.PHSCompanyNumber,
                            contractPlanType = providerDetails.ContractPlanType,
                            providerSearchType = string.IsNullOrEmpty(providerDetails.ProviderSearchType) ? "P" : providerDetails.ProviderSearchType,
                            searchCategory = string.IsNullOrEmpty(providerDetails.SearchCategory) ? "D" : providerDetails.SearchCategory,
                            enableLog = providerDetails.enableLog ? providerDetails.enableLog : false
                        };
                        isValidRequest = true;
                    }

                    if (!isValidRequest)
                    {
                        return BadRequest(new { error = "One of the mandatory params is missing, Please check either ProviderGroupFacility, ProviderGroupNumber, PHSCompanyNumber and ContractPlanType are provided." });
                    }

                    boNICEProviderDetails.GetProviderSearchDetails(searchRequest, out providerSearchDetails, out errMsg);
                    if (!string.IsNullOrEmpty(errMsg) && errMsg.Length > 0)
                    {
                        _objBOCommon.ErrorLog(LoggedInUserId, this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), errMsg, errMsg);
                        return StatusCode((int)HttpStatusCode.FailedDependency, errMsg);
                    }
                }
                return Ok(providerSearchDetails);
            }
            catch (Exception ex)
            {
                _objBOCommon.ErrorLog(LoggedInUserId, this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), ex.Message, ex.ToString());
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = "An unexpected error occurred.", details = ex.Message + "" + errMsg});
            }
        }

        [HttpGet]
        public ActionResult GetProviderAddress(string details = null)
        {
            string errMsg = string.Empty;
            Console.WriteLine("ProviderNICEController - GetProviderAddress");
            DOProviderSearchCriteria providerDetails = null;
            List<DONICEProviderDetails> providerDetailsList = null;
            BONICEProviderAddress boNICEProviderAddress = new BONICEProviderAddress(_objConfiguration, _cache, _objBOCommon);
            try
            {
                if (!string.IsNullOrEmpty(details))
                {
                    providerDetails = JsonConvert.DeserializeObject<DOProviderSearchCriteria>(details);
                    if (providerDetails != null && !string.IsNullOrEmpty(providerDetails.ProviderGroupFacility)
                        && !string.IsNullOrEmpty(providerDetails.ProviderGroupNumber) && !string.IsNullOrEmpty(providerDetails.ProviderAddressType))
                    {
                        ProviderSearchRequest searchRequest = new ProviderSearchRequest
                        {
                            providerGroupFacility = (providerDetails.ProviderGroupFacility.Length < 4) ? providerDetails.ProviderGroupFacility.PadLeft(4, '0') : providerDetails.ProviderGroupFacility,
                            providerGroupNumber = (providerDetails.ProviderGroupNumber.Length < 7) ? providerDetails.ProviderGroupNumber.PadLeft(7, '0') : providerDetails.ProviderGroupNumber,
                            providerAddressType = providerDetails.ProviderAddressType
                        };
                        Console.WriteLine("ProviderNICEController - GetProviderAddress starting");
                        boNICEProviderAddress.GetProviderAddress(searchRequest, out providerDetailsList, out errMsg);
                        if (!string.IsNullOrEmpty(errMsg) && errMsg.Length > 0)
                        {
                            _objBOCommon.ErrorLog(LoggedInUserId, this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), errMsg, errMsg);
                            return StatusCode((int)HttpStatusCode.FailedDependency, errMsg);
                        }
                        Console.WriteLine("ProviderNICEController - GetProviderAddress completed");
                    }
                    else
                    {
                        return BadRequest(new { error = "One of the mandatory params is missing, Please check either ProviderGroupFacility, ProviderGroupNumber and ProviderAddressType are provided." });
                    }
                }
                Console.WriteLine("ProviderNICEController - GetProviderAddress");
                return Ok(providerDetailsList);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ProviderNICEController - Error: ", ex.Message);
                _objBOCommon.ErrorLog(LoggedInUserId, this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), ex.Message, ex.ToString());
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = "An unexpected error occurred.", details = ex.Message + "" + errMsg });
            }
        }

        [HttpGet]
        public ActionResult GetServiceProviderBillingAddress(string details = null)
        {
            string errMsg = string.Empty;
            Console.WriteLine("ProviderNICEController - GetServiceProviderBillingAddress");
            DOProviderSearchCriteria providerDetails = null;
            List<DONICEProviderDetails> providerDetailsList = null;
            BONICEProviderAddress boNICEProviderAddress = new BONICEProviderAddress(_objConfiguration, _cache, _objBOCommon);
            try
            {
                if (!string.IsNullOrEmpty(details))
                {
                    providerDetails = JsonConvert.DeserializeObject<DOProviderSearchCriteria>(details);
                    if (providerDetails != null && !string.IsNullOrEmpty(providerDetails.ProviderGroupFacility)
                        && !string.IsNullOrEmpty(providerDetails.ProviderGroupNumber) && !string.IsNullOrEmpty(providerDetails.ProviderAddressType))
                    {
                        ProviderSearchRequest searchRequest = new ProviderSearchRequest
                        {
                            providerGroupFacility = (providerDetails.ProviderGroupFacility.Length < 4) ? providerDetails.ProviderGroupFacility.PadLeft(4, '0') : providerDetails.ProviderGroupFacility,
                            providerGroupNumber = (providerDetails.ProviderGroupNumber.Length < 7) ? providerDetails.ProviderGroupNumber.PadLeft(7, '0') : providerDetails.ProviderGroupNumber,
                            providerAddressType = providerDetails.ProviderAddressType
                        };
                        Console.WriteLine("ProviderNICEController - GetServiceProviderBillingAddress starting");
                        boNICEProviderAddress.GetServiceProviderBillingAddress(searchRequest, out providerDetailsList, out errMsg);
                        if (!string.IsNullOrEmpty(errMsg) && errMsg.Length > 0)
                        {
                            _objBOCommon.ErrorLog(LoggedInUserId, this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), errMsg, errMsg);
                            return StatusCode((int)HttpStatusCode.FailedDependency, errMsg);
                        }
                        Console.WriteLine("ProviderNICEController - GetServiceProviderBillingAddress completed");
                    }
                    else
                    {
                        return BadRequest(new { error = "One of the mandatory params is missing, Please check either ProviderGroupFacility, ProviderGroupNumber and ProviderAddressType are provided." });
                    }
                }
                Console.WriteLine("ProviderNICEController - GetServiceProviderBillingAddress");
                return Ok(providerDetailsList);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ProviderNICEController - Error: ", ex.Message);
                _objBOCommon.ErrorLog(LoggedInUserId, this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), ex.Message, ex.ToString());
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = "An unexpected error occurred.", details = ex.Message + "" + errMsg });
            }
        }

        [HttpGet]
        public ActionResult GetCapHospitalBillingAddress(string details = null)
        {
            string errMsg = string.Empty;
            Console.WriteLine("ProviderNICEController - GetCapHospitalBillingAddress");
            DOProviderSearchCriteria providerDetails = null;
            List<DONICEProviderDetails> providerDetailsList = null;
            BONICEProviderAddress boNICEProviderAddress = new BONICEProviderAddress(_objConfiguration, _cache, _objBOCommon);
            try
            {
                if (!string.IsNullOrEmpty(details))
                {
                    providerDetails = JsonConvert.DeserializeObject<DOProviderSearchCriteria>(details);
                    if (providerDetails != null && !string.IsNullOrEmpty(providerDetails.ProviderGroupFacility)
                        && !string.IsNullOrEmpty(providerDetails.ProviderGroupNumber) && !string.IsNullOrEmpty(providerDetails.ProviderAddressType))
                    {
                        ProviderSearchRequest searchRequest = new ProviderSearchRequest
                        {
                            providerGroupFacility = (providerDetails.ProviderGroupFacility.Length < 4) ? providerDetails.ProviderGroupFacility.PadLeft(4, '0') : providerDetails.ProviderGroupFacility,
                            providerGroupNumber = (providerDetails.ProviderGroupNumber.Length < 7) ? providerDetails.ProviderGroupNumber.PadLeft(7, '0') : providerDetails.ProviderGroupNumber,
                            providerAddressType = providerDetails.ProviderAddressType
                        };
                        Console.WriteLine("ProviderNICEController - GetCapHospitalBillingAddress starting");
                        boNICEProviderAddress.GetCapHospitalBillingAddress(searchRequest, out providerDetailsList, out errMsg);
                        if (!string.IsNullOrEmpty(errMsg) && errMsg.Length > 0)
                        {
                            _objBOCommon.ErrorLog(LoggedInUserId, this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), errMsg, errMsg);
                            return StatusCode((int)HttpStatusCode.FailedDependency, errMsg);
                        }
                        Console.WriteLine("ProviderNICEController - GetCapHospitalBillingAddress completed");
                    }
                    else
                    {
                        return BadRequest(new { error = "One of the mandatory params is missing, Please check either ProviderGroupFacility, ProviderGroupNumber and ProviderAddressType are provided." });
                    }
                }
                Console.WriteLine("ProviderNICEController - GetCapHospitalBillingAddress");
                return Ok(providerDetailsList);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ProviderNICEController - Error: ", ex.Message);
                _objBOCommon.ErrorLog(LoggedInUserId, this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), ex.Message, ex.ToString());
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = "An unexpected error occurred.", details = ex.Message + "" + errMsg });
            }
        }

        [HttpGet]
        public ActionResult GetClaimContactAddress(string details = null)
        {
            string errMsg = string.Empty;
            Console.WriteLine("ProviderNICEController - GetClaimContactAddress");
            DOProviderSearchCriteria providerDetails = null;
            List<DONICEProviderDetails> providerDetailsList = null;
            BONICEProviderAddress boNICEProviderAddress = new BONICEProviderAddress(_objConfiguration, _cache, _objBOCommon);
            try
            {
                if (!string.IsNullOrEmpty(details))
                {
                    providerDetails = JsonConvert.DeserializeObject<DOProviderSearchCriteria>(details);
                    if (providerDetails != null && !string.IsNullOrEmpty(providerDetails.PMGGroupFacilityNumber)
                        && !string.IsNullOrEmpty(providerDetails.PCPGroupFacilityNumber))
                    {
                        ProviderSearchRequest searchRequest = new ProviderSearchRequest
                        {
                            pmgGroupFacilityNumber = providerDetails.PMGGroupFacilityNumber,
                            pcpGroupFacilityNumber = providerDetails.PCPGroupFacilityNumber
                        };
                        Console.WriteLine("ProviderNICEController - GetClaimContactAddress starting");
                        boNICEProviderAddress.GetClaimContactAddress(searchRequest, out providerDetailsList, out errMsg);
                        if (!string.IsNullOrEmpty(errMsg) && errMsg.Length > 0)
                        {
                            _objBOCommon.ErrorLog(LoggedInUserId, this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), errMsg, errMsg);
                            return StatusCode((int)HttpStatusCode.FailedDependency, errMsg);
                        }
                        Console.WriteLine("ProviderNICEController - GetClaimContactAddress completed");
                    }
                    else
                    {
                        return BadRequest(new { error = "One of the mandatory params is missing, Please check either PMGGroupFacility, PMGGroupNumber,PCPGroupFacility and PCPGroupNumber are provided." });
                    }
                }
                Console.WriteLine("ProviderNICEController - GetClaimContactAddress");
                return Ok(providerDetailsList);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ProviderNICEController - Error: ", ex.Message);
                _objBOCommon.ErrorLog(LoggedInUserId, this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), ex.Message, ex.ToString());
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = "An unexpected error occurred.", details = ex.Message + "" + errMsg });
            }
        }

        [HttpGet]
        public ActionResult GetNICEPMGName(string details = null)
        {
            string errMsg = string.Empty;
            Console.WriteLine("ProviderNICEController - GetNICEPMGName");
            DOProviderSearchCriteria providerDetails = null;
            string pmgName = string.Empty;
            List<DONICEProviderDetails> providerDetailsList = new List<DONICEProviderDetails>();
            BONICEProviderAddress boNICEProviderAddress = new BONICEProviderAddress(_objConfiguration, _cache, _objBOCommon);
            try
            {
                if (!string.IsNullOrEmpty(details))
                {
                    providerDetails = JsonConvert.DeserializeObject<DOProviderSearchCriteria>(details);
                    if (providerDetails != null && !string.IsNullOrEmpty(providerDetails.ProviderGroupNumber) && !providerDetails.ProviderGroupNumber.All(c => c == '0'))
                    {
                        ProviderSearchRequest searchRequest = new ProviderSearchRequest
                        {
                            providerGroupNumber = (providerDetails.ProviderGroupNumber.Length < 7) ? providerDetails.ProviderGroupNumber.PadLeft(7, '0') : providerDetails.ProviderGroupNumber,
                            providerAddressType = "20000216"
                        };
                        Console.WriteLine("ProviderNICEController - GetNICEPMGName starting");
                        boNICEProviderAddress.GetNICEPMGName(searchRequest, out pmgName, out errMsg);
                        DONICEProviderDetails obj = new DONICEProviderDetails();
                        obj.ContactName = pmgName;
                        providerDetailsList.Add(obj);
                        if (!string.IsNullOrEmpty(errMsg) && errMsg.Length > 0)
                        {
                            _objBOCommon.ErrorLog(LoggedInUserId, this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), errMsg, errMsg);
                            return StatusCode((int)HttpStatusCode.FailedDependency, errMsg);
                        }
                        Console.WriteLine("ProviderNICEController - GetNICEPMGName completed");
                    }
                    else if(providerDetails != null && !string.IsNullOrEmpty(providerDetails.ProviderGroupNumber) && providerDetails.ProviderGroupNumber.All(c => c == '0'))
                    {
                        DONICEProviderDetails obj = new DONICEProviderDetails();
                        obj.ContactName = "";
                        providerDetailsList.Add(obj);
                    }
                    else
                    {
                        return BadRequest(new { error = "One of the mandatory params is missing, Please check ProviderGroupNumber" });
                    }
                }
                Console.WriteLine("ProviderNICEController - GetNICEPMGName");
                return Ok(providerDetailsList);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ProviderNICEController - Error: ", ex.Message);
                _objBOCommon.ErrorLog(LoggedInUserId, this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), ex.Message, ex.ToString());
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = "An unexpected error occurred.", details = ex.Message + "" + errMsg });
            }
        }
    }
}