using ANGDDEAPI.Common;
using ANGDDEAPIBO;
using ANGDDEAPIDO;
using ANGDDEAPIDO.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ANGDDEAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class ProvidersController : ControllerBase
    {
        public static long LoggedInUserId = 0;
        public string _username = string.Empty;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DOConfiguration _objConfiguration;
        private readonly IMemoryCacheHelper _memoryCacheHelper;
        //public BOPASDetails _objBOPASDetails = null;
        public readonly BOCommon _objBOCommon;
        string source = string.Empty;
        private readonly ICacheService _cache; 
 
        public ProvidersController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _memoryCacheHelper = memoryCacheHelper;
            _objBOCommon = bOCommon;
            _cache = cache;
        }

        [HttpGet]
        public ActionResult Search(string providerObj, bool isProviderPref = false)
        {
            DOProviderOrFacilitySearchCriteria objDOPESProvider = null;
            String Message = string.Empty;
            DOProviderResponse objDOProviderResponse = null;
            source = BOCommon.GetRefererURI(Request);
            try
            {
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                if (!string.IsNullOrEmpty(providerObj))
                {
                    _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Request", providerObj, _username);
                    if (!isProviderPref)
                    {
                        objDOPESProvider = new DOProviderOrFacilitySearchCriteria();
                        objDOPESProvider = JsonConvert.DeserializeObject<DOProviderOrFacilitySearchCriteria>(providerObj);
                        if (string.IsNullOrEmpty(objDOPESProvider.MPIN) && string.IsNullOrEmpty(objDOPESProvider.TaxId))
                        {
                            return StatusCode((int)HttpStatusCode.BadRequest, "No valid request received!");
                        }

                        ProviderMethods objProviderMethods = new ProviderMethods(_memoryCacheHelper, _objConfiguration);
                        objProviderMethods.ProviderSearch(objDOPESProvider, out objDOProviderResponse, out Message);

                        return Ok(objDOProviderResponse);
                    }
                    else
                    {
                        Request objDOProviderPref = null;
                        
                        preferenceRequest objDOProviderPrefResponse = null;
                        
                        objDOProviderPref = new Request();
                        objDOProviderPref = JsonConvert.DeserializeObject<Request>(providerObj);
                        if (string.IsNullOrEmpty(objDOProviderPref.providerId) && string.IsNullOrEmpty(objDOProviderPref.providerTIN))
                        {
                            return StatusCode((int)HttpStatusCode.BadRequest, "No valid request received!");
                        }

                        ProviderMethods objProviderMethods = new ProviderMethods(_memoryCacheHelper, _objConfiguration);

                        objProviderMethods.FetchPreferenceFromNDBAndUpdateQueue(objDOProviderPref.providerId, objDOProviderPref.providerTIN, objDOProviderPref.letterTypeList[0].letterType, out objDOProviderPrefResponse);

                        return Ok(objDOProviderPrefResponse);
                    }
                }
                _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Response", Message, _username);

            
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
                return StatusCode((int)HttpStatusCode.InternalServerError, ex);
            }

            return Ok(objDOProviderResponse);

        }

        [Route("[action]")]
        [HttpGet]
        public ActionResult SearchNDBPreference(string providerObj)
        {
            
            String Message = string.Empty;
            DOProviderResponse objDOProviderResponse = null;
            source = BOCommon.GetRefererURI(Request);
            try
            {
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                if (!string.IsNullOrEmpty(providerObj))
                {
                    _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Request", providerObj, _username);
                   
                    
                        Request objDOProviderPref = null;

                        preferenceRequest objDOProviderPrefResponse = null;

                        objDOProviderPref = new Request();
                        objDOProviderPref = JsonConvert.DeserializeObject<Request>(providerObj);
                    if ((string.IsNullOrEmpty(objDOProviderPref.providerId) && string.IsNullOrEmpty(objDOProviderPref.providerTIN)) ||
                      objDOProviderPref.letterTypeList == null || !objDOProviderPref.letterTypeList.Any())
                    {
                        return StatusCode((int)HttpStatusCode.BadRequest, "No valid request received!");
                    }

                    ProviderMethods objProviderMethods = new ProviderMethods(_memoryCacheHelper, _objConfiguration);

                        objProviderMethods.FetchPreferenceFromNDB(objDOProviderPref.providerId, objDOProviderPref.providerTIN, objDOProviderPref.letterTypeList[0].letterType, out objDOProviderPrefResponse);

                        return Ok(objDOProviderPrefResponse);
                    
                }
                _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Response", Message, _username);


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
                return StatusCode((int)HttpStatusCode.InternalServerError, ex);
            }

            return Ok(objDOProviderResponse);

        }
    }
}
