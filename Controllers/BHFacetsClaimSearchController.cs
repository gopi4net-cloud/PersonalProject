using ANGDDEAPI.Common;
using ANGDDEAPIBO;
using ANGDDEAPIDO;
using ANGDDEAPIDO.BHFacets;
using ANGDDEAPIDO.BHFacets.ClaimDetails;
using ANGDDEAPIDO.Interface;
using ANGDDEAPIDO.PSaaS;
using ANGDDEAPIFoundation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;


namespace ANGDDEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class BHFacetsClaimSearchController : ControllerBase
    {
        public static long LoggedInUserId = 0;
        BOClaimsSearch _objBOClaimsSearch;
        ExceptionTypes _result;
        public string _username = string.Empty;
        string source = string.Empty;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DOConfiguration _objConfiguration;
        private readonly IMemoryCacheHelper _memoryCacheHelper;
        public readonly BOCommon _objBOCommon;
        BHFacetsMethods _objboBHFacetsMethods;

        private readonly ICacheService _cache;

        public BHFacetsClaimSearchController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _memoryCacheHelper = memoryCacheHelper;
            _objBOCommon = bOCommon;
            _cache = cache;
        }

        [HttpGet]
        public IActionResult GetClaimDetailSearchByMemberID(string strClaimSearch)
        {
            source = BOCommon.GetRefererURI(Request);
            try
            {
                List<DOClaimInfo> lstObjDOClaimInfo = null;
                BHFacetsClaimInput objDOClaimListMemberInput = null;
                _objboBHFacetsMethods = new BHFacetsMethods(_memoryCacheHelper, _objConfiguration, _httpContextAccessor, _objBOCommon);
                if (!string.IsNullOrEmpty(strClaimSearch.ToString()))
                {
                    objDOClaimListMemberInput = new BHFacetsClaimInput();
                    objDOClaimListMemberInput = JsonConvert.DeserializeObject<BHFacetsClaimInput>(strClaimSearch);
                    _result = _objboBHFacetsMethods.ClaimListByMemberId(objDOClaimListMemberInput, out lstObjDOClaimInfo);

                }

                return Ok(lstObjDOClaimInfo);
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

        [HttpGet]
        public IActionResult GetClaimListByProviderID(string strClaimSearch)
        {
            source = BOCommon.GetRefererURI(Request);
            try
            {
                List<DOClaimInfo> lstObjDOClaimInfo = null;
                BHFacetsClaimInput objDOClaimListByClaimNumberRequestInput = null;
                _objboBHFacetsMethods = new BHFacetsMethods(_memoryCacheHelper, _objConfiguration, _httpContextAccessor, _objBOCommon);
                if (!string.IsNullOrEmpty(strClaimSearch.ToString()))
                {
                    objDOClaimListByClaimNumberRequestInput = new BHFacetsClaimInput();
                    objDOClaimListByClaimNumberRequestInput.providerID = strClaimSearch;
                    _result = _objboBHFacetsMethods.ClaimListByProviderID(objDOClaimListByClaimNumberRequestInput, out lstObjDOClaimInfo);
                }


                return Ok(lstObjDOClaimInfo);
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


        /// <summary>
        /// Get Claims information based on search criteria
        /// </summary>
        /// <param name="strClaimSearch"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetClaimListByClaimNumber(string strClaimSearch)
        {
            source = BOCommon.GetRefererURI(Request);
            try
            {
                List<DOClaimInfo> lstObjDOClaimInfo = null;
                BHFacetsClaimInput objDOClaimListByClaimNumberRequestInput = null;
                _objboBHFacetsMethods = new BHFacetsMethods(_memoryCacheHelper, _objConfiguration, _httpContextAccessor, _objBOCommon);
                if (!string.IsNullOrEmpty(strClaimSearch.ToString()))
                {
                    objDOClaimListByClaimNumberRequestInput = new BHFacetsClaimInput();
                    objDOClaimListByClaimNumberRequestInput.claimId = strClaimSearch;                  
                    _result = _objboBHFacetsMethods.ClaimListByClaimNumber(objDOClaimListByClaimNumberRequestInput, out lstObjDOClaimInfo);
                }


                return Ok(lstObjDOClaimInfo);
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

        [HttpGet]
        public IActionResult GetClaimDetailSearchByClaimNumber(string strClaimSearch)
        {
            source = BOCommon.GetRefererURI(Request);
            try
            {
                List<DOClaimInfo> lstObjDOClaimInfo = null;
                BHFacetsClaimInput objDOClaimDetailByClaimNumberInput = null;
                _objboBHFacetsMethods = new BHFacetsMethods(_memoryCacheHelper, _objConfiguration, _httpContextAccessor, _objBOCommon);
                if (!string.IsNullOrEmpty(strClaimSearch.ToString()))
                {
                    objDOClaimDetailByClaimNumberInput = new BHFacetsClaimInput();
                    objDOClaimDetailByClaimNumberInput.claimId = strClaimSearch;
                    //objDOClaimDetailByClaimNumberInput = JsonConvert.DeserializeObject<DOClaimDetailByClaimNumberRequest>(strClaimSearch);
                    _result = _objboBHFacetsMethods.ClaimDetailByClaimNumber(objDOClaimDetailByClaimNumberInput, out lstObjDOClaimInfo);
                }

                return Ok(lstObjDOClaimInfo);
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

       
        [HttpGet]
        public IActionResult GetClaimDetails(string strClaimSearch)
        {
            source = BOCommon.GetRefererURI(Request);
            try
            {
                List<DOClaimInfo> lstObjDOClaimInfo = null;
                BHFacetsClaimInput objDOClaimDetailsInput = null;
                _objboBHFacetsMethods = new BHFacetsMethods(_memoryCacheHelper, _objConfiguration, _httpContextAccessor, _objBOCommon);
                if (!string.IsNullOrEmpty(strClaimSearch.ToString()))
                {
                    objDOClaimDetailsInput = new BHFacetsClaimInput();
                    objDOClaimDetailsInput = JsonConvert.DeserializeObject<BHFacetsClaimInput>(strClaimSearch);
                    _result = _objboBHFacetsMethods.Claimdetails(objDOClaimDetailsInput, out lstObjDOClaimInfo);

                }

                return Ok(lstObjDOClaimInfo);
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
