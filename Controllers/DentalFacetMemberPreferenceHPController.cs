using Amazon.Runtime.Internal.Util;
using ANGDDEAPI.Common;
using ANGDDEAPIBO;
using ANGDDEAPIBO.Interface;
using ANGDDEAPIDO;
using ANGDDEAPIDO.Interface;
using ANGDDEAPIDO.ISET;
using ANGDDEAPIFoundation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ANGDDEAPI.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class DentalFacetMemberPreferenceHPController : ControllerBase
{
    private readonly DOConfiguration _objConfiguration;
    private readonly BOCommon _bOCommon;
    private readonly BOCommon _objBOCommon;
    private readonly ICacheService _cache;
    List<DOATSLookupMaster> lstATSLookupMaster = null;
    public ExceptionTypes exResult;
    public BOGPSMemberDetails _objBOGPSMemberDetails = null;
    public AccessAPI objAPICall = null;
    private readonly IMemoryCacheHelper _memoryCacheHelper;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DentalFacetMemberPreferenceHPController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
    {
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
    public IActionResult GetMemberPreference(string memberPreferenceRequest)
    {
        try
        {
            if (memberPreferenceRequest == null || memberPreferenceRequest.Trim().Length == 0)
            {
                return BadRequest();
            }

            DODentalFacetMemberPreferenceRequest memberPreferenceRequestObj = JsonConvert.DeserializeObject<DODentalFacetMemberPreferenceRequest>(memberPreferenceRequest);
            DentalFacetMemberPreferenceHP ObjBODentalFacetMemberPrefHP = new DentalFacetMemberPreferenceHP(_memoryCacheHelper, _objConfiguration, objAPICall);
            DODentalFacetMemberPreferenceResponse memeberPreferenceHPResponse = null;
            var result = ObjBODentalFacetMemberPrefHP.GetMemberPreference(memberPreferenceRequestObj, out memeberPreferenceHPResponse);
            if (result != ExceptionTypes.Success)
            {
                return BadRequest();
            }
            return Ok(memeberPreferenceHPResponse);

        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }

    }



}
