using Microsoft.AspNetCore.Mvc;
using System;
using ANGDDEAPIBO;
using ANGDDEAPIDO;
using System.Collections.Generic;
using API.Models;
using Amazon.Runtime.Internal.Util;
using ANGDDEAPI.Common;
using ANGDDEAPIDO.Interface;
using Microsoft.AspNetCore.Http;
using System.Net;
using Newtonsoft.Json;

namespace ANGDDEAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProviderInfoController : Controller
    {
        private readonly DOConfiguration _objConfiguration;
        public static long LoggedInUserId = 0;
        public string _username = string.Empty;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public readonly BOCommon _objBOCommon;
        string source = string.Empty; 

        public ProviderInfoController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, BOCommon bOCommon)
        {
            _objConfiguration = objConfiguration;
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _objBOCommon = bOCommon; 
        }

        [HttpGet]
        [Route("GetAllProvider")]
        public ActionResult GetAllProvider(string providerObj, string taxid = null)
        {
            DOProviderOrFacilitySearchCriteria objDOPESProvider = null;
            String Message = string.Empty;
            BOPractitionerSearch _objBOPractitionerSearch = new BOPractitionerSearch(_objConfiguration);
            DOProviderResponse objDOProviderResponse = null;
            source = BOCommon.GetRefererURI(Request);
            try
            {
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Request", providerObj, _username);
                objDOPESProvider = new DOProviderOrFacilitySearchCriteria();
                objDOPESProvider = JsonConvert.DeserializeObject<DOProviderOrFacilitySearchCriteria>(providerObj);
                if (string.IsNullOrEmpty(objDOPESProvider.MPIN) && string.IsNullOrEmpty(objDOPESProvider.TaxId))
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, "No valid request received!");
                }
                _objBOPractitionerSearch.GetAllPractitionerSeach(out objDOProviderResponse, objDOPESProvider);
                _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Response", Message, _username);
                return Ok(objDOProviderResponse);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }

            return NotFound();
        }

        [HttpGet]
        [Route("GetOrganization")]
        public ActionResult GetOrganization(string providerObj, string taxid = null)
        {
            BOPractitionerSearch _objBOPractitionerSearch = new BOPractitionerSearch(_objConfiguration);
            DOProviderResponse objDOProviderResponse = null;
            try
            {
                _objBOPractitionerSearch.GetOrganization(out objDOProviderResponse, providerObj, taxid);
                return Ok(objDOProviderResponse);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }

            return NotFound();
        }

        [HttpGet]
        [Route("ExportToExcel")]
        public ActionResult ExportToExcel()
        {
            try
            {
                List<DOPESProvider> _lstExportToExcelData = new List<DOPESProvider>() { new DOPESProvider() { ProviderFirstName = "A111", City = "C111", FacilityName = "F11", ProviderName = "P11" }, new DOPESProvider() { ProviderFirstName = "A222", City = "C222", FacilityName = "F22", ProviderName = "P22" } };
                return File(BOCommon.ExportExcel(_lstExportToExcelData, "Provider Info"), "application/vnd.ms-excel", "ProviderInfo.xlsx");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }

            return NotFound();
        }

        [HttpGet]
        [Route("GetProviderDetailsForSB")]
        public ActionResult GetProviderDetailsForSB(string taxid = null)
        {
            String Message = string.Empty;
            BOPractitionerSearch _objBOPractitionerSearch = new BOPractitionerSearch(_objConfiguration);
            DOPESProvider objDOProviderResponse = null;
            source = BOCommon.GetRefererURI(Request);
            try
            {
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Request", taxid, _username);
                if (string.IsNullOrEmpty(taxid))
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, "No valid request received!");
                }
                _objBOPractitionerSearch.GetProviderDetails(taxid, out objDOProviderResponse);
                _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Response", Message, _username);
                return Ok(objDOProviderResponse);
            }
            catch (Exception ex)
            {
                if (source != null && source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(LoggedInUserId, this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), ex.Message, ex.ToString());
                }
                else
                {
                    _objBOCommon.LogError(this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                }
                return StatusCode((int)HttpStatusCode.InternalServerError, ex);
            } 
        }
    }


}
