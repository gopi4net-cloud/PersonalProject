using ANGDDEAPI.Common;
using ANGDDEAPIBO;
using ANGDDEAPIDO;
using ANGDDEAPIDO.Interface;
using ANGDDEAPIFoundation;
using MacessServiceReference;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ANGDDEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class MacessController : ControllerBase
    {
        public string _username = string.Empty;
        string source = string.Empty;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DOConfiguration _objConfiguration;
        private readonly BOCommon _objBOCommon;
        private readonly ICacheService _cache;
        private readonly IMemoryCacheHelper _memoryCacheHelper;

        public MacessController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, BOCommon bOCommon, ICacheService cache, IMemoryCacheHelper memoryCacheHelper)
        {
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _objBOCommon = bOCommon;
            _cache = cache;
            _memoryCacheHelper = memoryCacheHelper;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<SessionInfo> Authenticate()
        {
            MacessServiceMethods objMacessServiceMethods = new MacessServiceMethods(_httpContextAccessor, _objConfiguration, _objBOCommon, _memoryCacheHelper);
            SessionInfo objSessionInfo = new SessionInfo();

            source = BOCommon.GetRefererURI(Request);
            try
            {
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                objSessionInfo = await objMacessServiceMethods.Authenticate();
                

            }
            catch (Exception ex)
            {
                throw;
            }          

            return objSessionInfo;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<DOMacessResponse> CreateDocument(DOMacessRequest MacessRequest)
        {

            MacessServiceMethods objMacessServiceMethods = new MacessServiceMethods(_httpContextAccessor, _objConfiguration, _objBOCommon, _memoryCacheHelper);
            DOMacessResponse objDOMacessResponse = new DOMacessResponse();
            DOMacessCaseInfo dOMacessCaseInfo = new DOMacessCaseInfo();

            source = BOCommon.GetRefererURI(Request);
            try
            {
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;

                if(MacessRequest != null)
                {
                    dOMacessCaseInfo = JsonConvert.DeserializeObject<DOMacessCaseInfo>(MacessRequest.request);

                    objDOMacessResponse = await objMacessServiceMethods.CreateDocument(dOMacessCaseInfo);
                }
                      
            }
            catch (Exception ex)
            {
                objDOMacessResponse.Status = "Failed";
                objDOMacessResponse.ErrorMessage = ex.Message;

             _objBOCommon.LogError(this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
            }
            //_objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Response", BOCommon.JsonConvertObjectToString(objDOMacessResponse), _username);

            return objDOMacessResponse;
        }
        /// <summary>
        /// not using - for local test through postman
        /// </summary>
        /// <returns></returns>

        [HttpPost]
        public async Task<DOMacessResponse> CreateDocumentinLocal()
        {

            MacessServiceMethods objMacessServiceMethods = new MacessServiceMethods(_httpContextAccessor, _objConfiguration, _objBOCommon, _memoryCacheHelper);
            DOMacessResponse objDOMacessResponse = new DOMacessResponse();
            DOMacessCaseInfo dOMacessCaseInfo = new DOMacessCaseInfo();

            source = BOCommon.GetRefererURI(Request);
            try
            {
                string a = JsonConvert.SerializeObject(dOMacessCaseInfo);
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;

                using var reader = new StreamReader(HttpContext.Request.Body);
                var MacessRequest = await reader.ReadToEndAsync();

                if (MacessRequest != null)
                {
                    dOMacessCaseInfo = JsonConvert.DeserializeObject<DOMacessCaseInfo>(MacessRequest);

                    objDOMacessResponse = await objMacessServiceMethods.CreateDocument(dOMacessCaseInfo);
                }

            }
            catch (Exception ex)
            {
                objDOMacessResponse.Status = "Failed";
                objDOMacessResponse.ErrorMessage = ex.Message;

                _objBOCommon.LogError(this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
            }
            //_objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Response", BOCommon.JsonConvertObjectToString(objDOMacessResponse), _username);

            return objDOMacessResponse;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="documentID"></param>
        /// <returns></returns>
        /// 
        [HttpPost]
        public ActionResult GetServiceForm(string documentID)
        {
            if (string.IsNullOrWhiteSpace(documentID))
                return BadRequest("documentID is required.");

            var objMacessServiceMethods = new MacessServiceMethods(_httpContextAccessor, _objConfiguration, _objBOCommon, _memoryCacheHelper);
            var objServiceFormView = new ServiceFormView();
            source = BOCommon.GetRefererURI(Request);

            try
            {
                objMacessServiceMethods.GetServiceForm(documentID, out objServiceFormView);
                return Ok(objServiceFormView);
            }
            catch (Exception ex)
            {
                _objBOCommon.LogError(this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), source, 10001, ex.Message, " ", ex.StackTrace, _username);
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="documentID"></param>
        /// <returns></returns>
        /// 
        [HttpPost]
        public ActionResult GetDocumentNotes(string documentID)
        {
            if (string.IsNullOrWhiteSpace(documentID))
                return BadRequest("documentID is required.");

            var objMacessServiceMethods = new MacessServiceMethods(_httpContextAccessor, _objConfiguration, _objBOCommon, _memoryCacheHelper);
            source = BOCommon.GetRefererURI(Request);
            List<DocumentNotes> lstdocumentNotes = new List<DocumentNotes>();

            try
            {
                objMacessServiceMethods.GetDocumentNotes(documentID, out lstdocumentNotes);
                return Ok(lstdocumentNotes);
            }
            catch (Exception ex)
            {
                _objBOCommon.LogError(this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), source, 10001, ex.Message, " ", ex.StackTrace, _username);
                return StatusCode(500, ex.Message);
            }
        }
    }
}
