using Amazon.Runtime.Internal.Util;
using ANGDDEAPI.Common;
using ANGDDEAPIBO;
using ANGDDEAPIBO.Interface;
using ANGDDEAPIDO;
using ANGDDEAPIDO.Interface;
using ANGDDEAPIFoundation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ANGDDEAPI.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class DentalFacetEligibilityHPController : ControllerBase
{
    private readonly DOConfiguration _objConfiguration;
    private readonly BOCommon _bOCommon;
    public AccessAPI objAPICall = null;
    private readonly IMemoryCacheHelper _memoryCacheHelper;


    public static long LoggedInUserId = 0;
    public string _username = string.Empty;
    string source = string.Empty;

    private readonly IHttpContextAccessor _httpContextAccessor;
    //private readonly DOConfiguration _objConfiguration;
    //private readonly IMemoryCacheHelper _memoryCacheHelper;
    private readonly BOCommon _objBOCommon;
    private readonly ICacheService _cache;
    List<DOATSLookupMaster> lstATSLookupMaster = null;
    public ExceptionTypes exResult;
    public BOGPSMemberDetails _objBOGPSMemberDetails = null;
    //public AccessAPI objAPICall = null;

    //public DentalFacetEligibilityHPController(DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon)
    public DentalFacetEligibilityHPController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
    {
        //_objConfiguration = objConfiguration;
        //_bOCommon = bOCommon;
        //_memoryCacheHelper = memoryCacheHelper;
        //objAPICall = new AccessAPI(_memoryCacheHelper, _objConfiguration);
        _httpContextAccessor = httpContextAccessor;
        _objConfiguration = objConfiguration;
        _memoryCacheHelper = memoryCacheHelper;
        _objBOCommon = bOCommon;
        _cache = cache;
        exResult = _objBOCommon.GetRRTGPSAPiConfig(out lstATSLookupMaster);
        _objBOGPSMemberDetails = new BOGPSMemberDetails(objConfiguration, _cache);
        objAPICall = new AccessAPI(_memoryCacheHelper, _objConfiguration);
    }

    [HttpGet]
    public IActionResult GetEligibilitySummary(string eligibilitySummaryRequest)
    {
        try
        {
            if (eligibilitySummaryRequest == null || eligibilitySummaryRequest.Trim().Length == 0)
            {
                return BadRequest();
            }
            DODentalFacetEligibilityHPSearchInputRequest searchInputRequestObj = JsonConvert.DeserializeObject<DODentalFacetEligibilityHPSearchInputRequest>(eligibilitySummaryRequest);
            DentalFacetEligibilityHP objDentalFacetEligibilityHP = new DentalFacetEligibilityHP(_memoryCacheHelper, _objConfiguration, objAPICall);
            DODentalFacetEligibilityHPResponse eligibilityHPResponse = null;
            var result = objDentalFacetEligibilityHP.GetEligibilitySummary(searchInputRequestObj, out eligibilityHPResponse);
            if (result != ExceptionTypes.Success)
            {
                return BadRequest();
            }
            return Ok(eligibilityHPResponse);
        }
        catch (Exception ex)
        {
            return BadRequest();
        }

    }
}
