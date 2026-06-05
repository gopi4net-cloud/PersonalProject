using ANGDDEAPI.Common;
using ANGDDEAPIBO;
using ANGDDEAPIDO.Interface;
using ANGDDEAPIDO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System;
using ANGDDEAPIFoundation;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace ANGDDEAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CreateFolioController : ControllerBase
    {
        public static long LoggedInUserId = 0;
        public string _username = string.Empty;
        string source = string.Empty;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DOConfiguration _objConfiguration;
        private readonly IMemoryCacheHelper _memoryCacheHelper;
        public readonly BOCommon _objBOCommon;
        List<DOATSLookupMaster> lstATSLookupMaster = null;
        private readonly ICacheService _cache;

        public CreateFolioController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _memoryCacheHelper = memoryCacheHelper;
            _objBOCommon = bOCommon;
            _cache = cache;
        }
        [HttpPost]
        public DocFolioDocumentResponse CreateFolioDocument()
        {
            string RequestId = "";
            long fileSize = 0;
            string requestSource = "";
            DOC360Methods objDOC360Methods = new DOC360Methods(_memoryCacheHelper, _objConfiguration, _objBOCommon);
            var provider = new MultipartMemoryStreamProvider();
            EDMSServiceMethods objEDMSServiceMethods = new EDMSServiceMethods(_httpContextAccessor, _objConfiguration);            
            DocFolioDocumentResponse docFolioDocumentResponse = new DocFolioDocumentResponse();
            docFolioDocumentResponse.MessageMap = new messageMap();
            DocFolioDocument objDOEFolioDocument = null;
            string fileName = string.Empty;
            source = BOCommon.GetRefererURI(Request);
            byte[] fileBytes = null;
            try
            {
                try
                {
                    requestSource = Request.Host.Value;
                }
                catch
                {
                }
                _objBOCommon.GetRRTGPSAPiConfig(out lstATSLookupMaster);
                string FileSizeLimit = _objBOCommon.GetLookupValueBasedonCon(lstATSLookupMaster, (long)RRTGPSAPIConfig.FileSizeLimit, 0);
                RequestId = Guid.NewGuid().ToString();
                _objBOCommon.Trace("CreateFolioController-" + MethodBase.GetCurrentMethod().Name + " CreateFolioDocument - Request object of - " + RequestId, "CreateFolioController-request", requestSource, _httpContextAccessor.HttpContext.User.Identity.Name);

                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                string strObjDOECAADocument = _httpContextAccessor.HttpContext.Request.Form.ToArray()[0].Value;
                objDOEFolioDocument = JsonConvert.DeserializeObject<DocFolioDocument>(strObjDOECAADocument);
                docFolioDocumentResponse = objDOC360Methods.CreateFolioDocument(objDOEFolioDocument);

                if (docFolioDocumentResponse != null && (docFolioDocumentResponse.GlobalDocId != null || docFolioDocumentResponse.GlobalDocId != null))
                {
                    _objBOCommon.Trace("CreateFolioController-" + MethodBase.GetCurrentMethod().Name + " CreateFolioDocument - Response Success - " + RequestId, fileName + " - size-" + fileSize.ToString(), BOCommon.JsonConvertObjectToString(docFolioDocumentResponse), _httpContextAccessor.HttpContext.User.Identity.Name);
                }
                else
                {
                    _objBOCommon.Trace("CreateFolioController-" + MethodBase.GetCurrentMethod().Name + " CreateFolioDocument - Failed - " + RequestId, fileName + " - size-" + fileSize.ToString(), BOCommon.JsonConvertObjectToString(docFolioDocumentResponse), _httpContextAccessor.HttpContext.User.Identity.Name);
                }
            }
            catch (Exception ex)
            {
                _objBOCommon.Trace("CreateFolioController-" + MethodBase.GetCurrentMethod().Name + " Exception - " + RequestId, fileName + " - size- " + fileSize.ToString(), ex.Message, _httpContextAccessor.HttpContext.User.Identity.Name);
                docFolioDocumentResponse.Status = "500";
                List<string> uploaderrors = new List<string>();
                uploaderrors.Add("Bad Request");
                docFolioDocumentResponse.MessageMap.error = uploaderrors;
                if (source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(0, this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), ex.Message, ex.ToString());
                }
                else
                {
                    _objBOCommon.LogError(this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                }
            }

            return docFolioDocumentResponse;
        }

    }
}
