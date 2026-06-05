using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;       // ✅ for CallerMemberName
using ANGDDEAPI.Common;
using ANGDDEAPIBO;
using ANGDDEAPIDO;
using ANGDDEAPIFoundation;
using ANGDDEAPIDO.Interface;
using ANGDDEAPIDO.BHFacets;

namespace ANGDDEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class HCPClaimSearchController : ControllerBase
    {
        public static long LoggedInUserId = 0;
        BOClaimsSearch _objBOClaimsSearch;
        ExceptionTypes _result;
        public string _username = string.Empty;
        string source = string.Empty;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DOConfiguration _objConfiguration;
        private readonly IMemoryCacheHelper _memoryCacheHelper;
        private readonly BOCommon _objBOCommon;
        private readonly ICacheService _cache;
        private readonly BOHCPClaimSearch _BOHCPClaimSearch;

        public HCPClaimSearchController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration,
            IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _memoryCacheHelper = memoryCacheHelper;
            _objBOCommon = bOCommon;
            _cache = cache;
            _BOHCPClaimSearch = new BOHCPClaimSearch(_objConfiguration, _cache);
        }

        // ✅ Helper: safe username resolution
        private string GetUsername() =>
            _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? string.Empty;

        // ✅ Helper: replaces MethodBase.GetCurrentMethod() — serialization-safe, trim-safe
        private string CallerInfo([CallerMemberName] string memberName = "") =>
            $"{GetType().Name}.{memberName}";

        // ✅ Helper: centralized error logging — never returns MethodBase in response
        private void LogException(Exception ex, [CallerMemberName] string memberName = "")
        {
            string caller = $"{GetType().Name}.{memberName}";
            if (source.Contains("DDE"))
                _objBOCommon.ErrorLog(LoggedInUserId, caller, ex.Message, ex.ToString());
            else
                _objBOCommon.LogError(caller, source, 10001, ex.Message, " ", ex.StackTrace ?? string.Empty, _username);
        }

        [HttpGet]
        public ActionResult GetAllClaim(string strClaimSearch)
        {
            try
            {
                List<DOClaimInfo> lstObjDOClaimInfo = null;
                HCPClaimMethods _objBOHCPClaimSearch = new(_memoryCacheHelper, _objConfiguration);
                _username = GetUsername();

                if (!string.IsNullOrEmpty(strClaimSearch))
                {
                    // ✅ Guard against null deserialization
                    var objDOClaimInfo = JsonConvert.DeserializeObject<DOClaimInfo>(strClaimSearch);
                    if (objDOClaimInfo == null)
                        return BadRequest("Invalid claim search payload.");

                    if (objDOClaimInfo.ClaimSystemLkup == (int)SourceSystemType.FACETS)
                    {
                        var _objCSPFacetsMethods = new CSPFacetsMethods(_memoryCacheHelper, _objConfiguration, _httpContextAccessor, _objBOCommon);
                        var objDOClaimSummaryRequest = new DOClaimSummaryRequest
                        {
                            claimId           = string.IsNullOrEmpty(objDOClaimInfo.ClaimNumber) ? null : objDOClaimInfo.ClaimNumber,
                            dateOfServiceFrom = objDOClaimInfo.StartDate,
                            dateOfServiceTo   = objDOClaimInfo.EndDate,
                            limit             = 150,
                            subscriberId      = string.IsNullOrEmpty(objDOClaimInfo.MemberID) ? null : objDOClaimInfo.MemberID,
                            offset            = 1,
                            providerTin       = string.IsNullOrEmpty(objDOClaimInfo.TaxID) ? null : objDOClaimInfo.TaxID,
                            sortOrder         = "D"
                        };

                        bool showAllClaim = !string.IsNullOrEmpty(objDOClaimInfo.StatusCode)
                            && objDOClaimInfo.StatusCode.Equals("showallclaim", StringComparison.OrdinalIgnoreCase);

                        _result = _objCSPFacetsMethods.ClaimSummary(objDOClaimSummaryRequest, out lstObjDOClaimInfo, showAllClaim);
                        lstObjDOClaimInfo = lstObjDOClaimInfo?.Where(l => l.MemberID == objDOClaimInfo.MemberID).ToList();

                        if (_result != ExceptionTypes.Success && _result != ExceptionTypes.ZeroRecords)
                            return BadRequest();
                    }

                    if (objDOClaimInfo.ClaimSystemLkup == (int)SourceSystemType.OBHFACETS)
                    {
                        var _objBHFacetsMethods = new BHFacetsMethods(_memoryCacheHelper, _objConfiguration, _httpContextAccessor, _objBOCommon);
                        var objDOClaimSummaryRequest = new BHFacetsClaimInput
                        {
                            claimId          = string.IsNullOrEmpty(objDOClaimInfo.ClaimNumber)      ? null : objDOClaimInfo.ClaimNumber,
                            dateOfServiceFrom = objDOClaimInfo.StartDate,
                            dateOfServiceTo   = objDOClaimInfo.EndDate,
                            subscriberId      = string.IsNullOrEmpty(objDOClaimInfo.MemberID)        ? null : objDOClaimInfo.MemberID,
                            providerTin       = string.IsNullOrEmpty(objDOClaimInfo.TaxID)           ? null : objDOClaimInfo.TaxID,
                            firstName         = string.IsNullOrEmpty(objDOClaimInfo.PatientFirstName)? null : objDOClaimInfo.PatientFirstName,
                            lastName          = string.IsNullOrEmpty(objDOClaimInfo.PatientLastName) ? null : objDOClaimInfo.PatientLastName,
                            relationshipCode  = string.IsNullOrEmpty(objDOClaimInfo.RelationshipCode)? null : objDOClaimInfo.RelationshipCode,
                            groupId           = string.IsNullOrEmpty(objDOClaimInfo.groupId)         ? null : objDOClaimInfo.groupId,
                            dob               = string.IsNullOrEmpty(objDOClaimInfo.MemberDOB)       ? null : objDOClaimInfo.MemberDOB,
                            providerTaxId     = string.IsNullOrEmpty(objDOClaimInfo.ProviderTaxID)   ? null : objDOClaimInfo.ProviderTaxID
                        };

                        bool allFieldsPresent = objDOClaimSummaryRequest.subscriberId != null
                            && objDOClaimSummaryRequest.relationshipCode != null
                            && objDOClaimSummaryRequest.groupId != null
                            && objDOClaimSummaryRequest.firstName != null
                            && objDOClaimSummaryRequest.lastName != null
                            && objDOClaimSummaryRequest.dob != null
                            && objDOClaimSummaryRequest.dateOfServiceFrom != null
                            && objDOClaimSummaryRequest.dateOfServiceTo != null;

                        if (allFieldsPresent)
                        {
                            _result = _objBHFacetsMethods.ClaimListByMemberId(objDOClaimSummaryRequest, out lstObjDOClaimInfo);
                            lstObjDOClaimInfo = lstObjDOClaimInfo?.Where(l => l.MemberID == objDOClaimInfo.MemberID).ToList();
                        }
                        else
                        {
                            _result = _objBHFacetsMethods.ClaimListByClaimNumber(objDOClaimSummaryRequest, out lstObjDOClaimInfo);
                            lstObjDOClaimInfo = lstObjDOClaimInfo?.Where(l => l.MemberID == objDOClaimInfo.MemberID).ToList();

                            if (_result != ExceptionTypes.Success && _result != ExceptionTypes.ZeroRecords)
                                return BadRequest();
                        }
                    }
                    else if (objDOClaimInfo.ClaimSystemLkup == (int)SourceSystemType.COSMOS
                          || objDOClaimInfo.ClaimSystemLkup == (int)SourceSystemType.NICE)
                    {
                        _result = _objBOHCPClaimSearch.GetAllClaims(objDOClaimInfo, out lstObjDOClaimInfo);
                    }

                    if (_result != ExceptionTypes.Success && _result != ExceptionTypes.ZeroRecords)
                        return BadRequest();
                }

                return Ok(lstObjDOClaimInfo);
            }
            catch (Exception ex)
            {
                LogException(ex);
                // ✅ Return 500 — never rethrow raw exception (prevents MethodBase serialization)
                return StatusCode(500, "An internal server error occurred.");
            }
        }

        [HttpGet]
        public ActionResult GetClaimDetail(string strClaimSearch)
        {
            try
            {
                List<DOClaimInfo> lstObjDOClaimInfo = null;
                DOClaimInfo objDOClaimInfo = null;
                _objBOClaimsSearch = new BOClaimsSearch(_objConfiguration, _cache);
                HCPClaimMethods _objBOHCPClaimSearch = new(_memoryCacheHelper, _objConfiguration);
                long? requestID = null;
                _username = GetUsername();

                if (!string.IsNullOrEmpty(strClaimSearch))
                {
                    // ✅ Guard against null deserialization
                    objDOClaimInfo = JsonConvert.DeserializeObject<DOClaimInfo>(strClaimSearch);
                    if (objDOClaimInfo == null)
                        return BadRequest("Invalid claim search payload.");

                    if (objDOClaimInfo?.ClaimSystemLkup == (int)SourceSystemType.FACETS)
                    {
                        var _objCSPFacetsMethods = new CSPFacetsMethods(_memoryCacheHelper, _objConfiguration, _httpContextAccessor, _objBOCommon);
                        bool showAllClaim = !string.IsNullOrEmpty(objDOClaimInfo.StatusCode)
                            && objDOClaimInfo.StatusCode.Equals("showallclaim", StringComparison.OrdinalIgnoreCase);

                        _result = _objCSPFacetsMethods.ClaimDetails(objDOClaimInfo, out lstObjDOClaimInfo, showAllClaim);

                        if (_result != ExceptionTypes.Success && _result != ExceptionTypes.ZeroRecords)
                            return BadRequest();
                    }
                    else if (objDOClaimInfo?.ClaimSystemLkup == (int)SourceSystemType.OBHFACETS)
                    {
                        var _objBHFacetsMethods = new BHFacetsMethods(_memoryCacheHelper, _objConfiguration, _httpContextAccessor, _objBOCommon);
                        var objDOClaimSummaryRequest = new BHFacetsClaimInput
                        {
                            claimId           = string.IsNullOrEmpty(objDOClaimInfo.ClaimNumber)       ? null : objDOClaimInfo.ClaimNumber,
                            billingProviderId  = string.IsNullOrEmpty(objDOClaimInfo.BillingProviderId) ? null : objDOClaimInfo.BillingProviderId,
                            servicePradType    = string.IsNullOrEmpty(objDOClaimInfo.ServicePradType)   ? null : objDOClaimInfo.ServicePradType,
                            sortOrder          = "D"
                        };

                        _result = _objBHFacetsMethods.ClaimDetailByClaimNumber(objDOClaimSummaryRequest, out lstObjDOClaimInfo);

                        if (_result != ExceptionTypes.Success && _result != ExceptionTypes.ZeroRecords)
                            return BadRequest();
                    }
                    else if (objDOClaimInfo?.ClaimSystemLkup == (long)SourceSystemType.COSMOS
                          || objDOClaimInfo?.ClaimSystemLkup == (long)SourceSystemType.NICE)
                    {
                        _result = _objBOHCPClaimSearch.GetClaimDetail(objDOClaimInfo, out lstObjDOClaimInfo, out requestID);
                    }

                    if (_result != ExceptionTypes.Success && _result != ExceptionTypes.ZeroRecords)
                        return BadRequest();

                    // ✅ Trace moved inside if block — objDOClaimInfo guaranteed non-null here
                    // ✅ CallerInfo() replaces MethodBase.GetCurrentMethod() — no reflection objects
                    _objBOCommon.Trace(
                        CallerInfo(),
                        $"Single claim search with detail for - {objDOClaimInfo.ClaimSystemLkup} is - {_result}",
                        strClaimSearch,
                        _username);
                }

                return requestID != null ? Ok(requestID) : Ok(lstObjDOClaimInfo);
            }
            catch (Exception ex)
            {
                LogException(ex);
                // ✅ Return 500 — never rethrow raw exception (prevents MethodBase serialization)
                return StatusCode(500, "An internal server error occurred.");
            }
        }

        [HttpGet]
        public ActionResult GetAllClaimForMES(string strClaimSearch)
        {
            try
            {
                List<DOClaimInfo> lstObjDOClaimInfo = null;
                HCPClaimMethods _objBOHCPClaimSearch = new(_memoryCacheHelper, _objConfiguration);
                _username = GetUsername();

                if (!string.IsNullOrEmpty(strClaimSearch))
                {
                    var objDOClaimInfo = JsonConvert.DeserializeObject<DOClaimInfo>(strClaimSearch);
                    if (objDOClaimInfo == null)
                        return BadRequest("Invalid claim search payload.");

                    _result = _objBOHCPClaimSearch.GetAllClaimsForMES(objDOClaimInfo, out lstObjDOClaimInfo);

                    if (_result != ExceptionTypes.Success && _result != ExceptionTypes.ZeroRecords)
                        return BadRequest();
                }

                return Ok(lstObjDOClaimInfo);
            }
            catch (Exception ex)
            {
                LogException(ex);
                return StatusCode(500, "An internal server error occurred.");
            }
        }

        [HttpGet]
        public ActionResult GetClaimDetailForMES(string strClaimSearch)
        {
            try
            {
                List<DOClaimInfo> lstObjDOClaimInfo = null;
                _objBOClaimsSearch = new BOClaimsSearch(_objConfiguration, _cache);
                HCPClaimMethods _objBOHCPClaimSearch = new(_memoryCacheHelper, _objConfiguration);
                long? requestID = null;
                _username = GetUsername();

                if (!string.IsNullOrEmpty(strClaimSearch))
                {
                    var objDOClaimInfo = JsonConvert.DeserializeObject<DOClaimInfo>(strClaimSearch);
                    if (objDOClaimInfo == null)
                        return BadRequest("Invalid claim search payload.");

                    _result = _objBOHCPClaimSearch.GetClaimDetailForMES(objDOClaimInfo, out lstObjDOClaimInfo, out requestID);

                    if (_result != ExceptionTypes.Success && _result != ExceptionTypes.ZeroRecords)
                        return BadRequest();
                }

                return requestID != null ? Ok(requestID) : Ok(lstObjDOClaimInfo);
            }
            catch (Exception ex)
            {
                LogException(ex);
                return StatusCode(500, "An internal server error occurred.");
            }
        }

        [HttpGet]
        public ActionResult GetAllClaimForTOPS(string strClaimSearch)
        {
            try
            {
                List<DOClaimInfo> lstObjDOClaimInfo = null;
                HCPClaimMethods _objBOHCPClaimSearch = new(_memoryCacheHelper, _objConfiguration);
                _username = GetUsername();

                if (!string.IsNullOrEmpty(strClaimSearch))
                {
                    var objDOClaimInfo = JsonConvert.DeserializeObject<DOClaimInfo>(strClaimSearch);
                    if (objDOClaimInfo == null)
                        return BadRequest("Invalid claim search payload.");

                    _result = _objBOHCPClaimSearch.GetAllClaimsForTOPS(objDOClaimInfo, out lstObjDOClaimInfo);

                    if (_result != ExceptionTypes.Success && _result != ExceptionTypes.ZeroRecords)
                        return BadRequest();
                }

                return Ok(lstObjDOClaimInfo);
            }
            catch (Exception ex)
            {
                LogException(ex);
                return StatusCode(500, "An internal server error occurred.");
            }
        }

        [HttpGet]
        public ActionResult GetClaimDetailForTOPS(string strClaimSearch)
        {
            try
            {
                List<DOClaimInfo> lstObjDOClaimInfo = null;
                _objBOClaimsSearch = new BOClaimsSearch(_objConfiguration, _cache);
                HCPClaimMethods _objBOHCPClaimSearch = new(_memoryCacheHelper, _objConfiguration);
                long? requestID = null;
                _username = GetUsername();

                if (!string.IsNullOrEmpty(strClaimSearch))
                {
                    var objDOClaimInfo = JsonConvert.DeserializeObject<DOClaimInfo>(strClaimSearch);
                    if (objDOClaimInfo == null)
                        return BadRequest("Invalid claim search payload.");

                    _result = _objBOHCPClaimSearch.GetClaimDetailForTOPS(objDOClaimInfo, out lstObjDOClaimInfo, out requestID);

                    if (_result != ExceptionTypes.Success && _result != ExceptionTypes.ZeroRecords)
                        return BadRequest();
                }

                return requestID != null ? Ok(requestID) : Ok(lstObjDOClaimInfo);
            }
            catch (Exception ex)
            {
                LogException(ex);
                return StatusCode(500, "An internal server error occurred.");
            }
        }

        [HttpGet]
        public ActionResult GetClaimSummaryForTOPS(string strClaimSearch)
        {
            try
            {
                List<DOClaimInfo> lstObjDOClaimInfo = null;
                HCPClaimMethods _objBOHCPClaimSearch = new(_memoryCacheHelper, _objConfiguration);
                _username = GetUsername();

                if (!string.IsNullOrEmpty(strClaimSearch))
                {
                    var objDOClaimInfo = JsonConvert.DeserializeObject<DOClaimInfo>(strClaimSearch);
                    if (objDOClaimInfo == null)
                        return BadRequest("Invalid claim search payload.");

                    _result = _objBOHCPClaimSearch.GetClaimsSummaryTOPS(objDOClaimInfo, out lstObjDOClaimInfo);

                    if (_result != ExceptionTypes.Success && _result != ExceptionTypes.ZeroRecords)
                        return BadRequest();
                }

                return Ok(lstObjDOClaimInfo);
            }
            catch (Exception ex)
            {
                LogException(ex);
                return StatusCode(500, "An internal server error occurred.");
            }
        }
    }
}