using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using ANGDDEAPI.Common;
using ANGDDEAPIBO;
using ANGDDEAPIDO;
using ANGDDEAPIDO.Interface;
using ANGDDEAPIFoundation;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml.FormulaParsing.Excel.Functions.RefAndLookup;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;

namespace ANGDDEAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class DOC360Controller : ControllerBase
    {
        public static long LoggedInUserId = 0;
        public string _username = string.Empty;
        string source = string.Empty;
        public string ModuleName = "DOC360Controller";
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DOConfiguration _objConfiguration;
        private readonly IMemoryCacheHelper _memoryCacheHelper;
        public readonly BOCommon _objBOCommon;
        List<DOATSLookupMaster> lstATSLookupMaster = null;
        private readonly ICacheService _cache;
        private readonly DOC360Methods _objDOC360Methods;
        private readonly DOC360HCPMethod _objDOC360HCPMethods;

        private string _fileSizeLimit = null;
        private string _toEnableNewStargate = null;
        private string _EnableHCPUpload = null;
       
        public DOC360Controller(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _memoryCacheHelper = memoryCacheHelper;
            _objBOCommon = bOCommon;
            _cache = cache;

            // Initialized once — reused across all action methods in this controller
            _objDOC360Methods = new DOC360Methods(_memoryCacheHelper, _objConfiguration, _objBOCommon);
            _objDOC360HCPMethods = new DOC360HCPMethod(_memoryCacheHelper, _objConfiguration, _objBOCommon);
        }

        private void EnsureLookupConfig()
        {
            if (lstATSLookupMaster == null)
            {
                _objBOCommon.GetRRTGPSAPiConfig(out lstATSLookupMaster);
            }
            _fileSizeLimit = _objBOCommon.GetLookupValueBasedonCon(lstATSLookupMaster, (long)RRTGPSAPIConfig.FileSizeLimit, 0);
            _toEnableNewStargate = _objBOCommon.GetLookupValueBasedonCon(lstATSLookupMaster, (long)RRTGPSAPIConfig.ToEnableNewStarGate, 0);
            _EnableHCPUpload = _objBOCommon.GetLookupValueBasedonCon(lstATSLookupMaster, (long)RRTGPSAPIConfig.EnableHCPUpload, 0);
        }
        /// <summary>
        /// Get documents by case id
        /// </summary>
        /// <param name="caseId">Case id</param>
        /// <returns>Response object with list of documents </returns>
        public ActionResult GetQuery(string caseId, string docclassTypeName = "u_clinical_docs")
        {
            DOC360Methods objDOC360Methods = new DOC360Methods(_memoryCacheHelper, _objConfiguration, _objBOCommon);
            DOC360HCPMethod objDOC360HCPMethods = new DOC360HCPMethod(_memoryCacheHelper, _objConfiguration, _objBOCommon);
            DOECAAResponse objDOECAAResponse = null;
            String Message = string.Empty;
            source = BOCommon.GetRefererURI(Request);
            EnsureLookupConfig();
            try
            {
                _objBOCommon.GetRRTGPSAPiConfig(out lstATSLookupMaster);

                string TotalRecords = "50";
                try
                {
                    string lookupValue = _objBOCommon.GetLookupValueBasedonCon(lstATSLookupMaster, (long)RRTGPSAPIConfig.TotalRecords, 0);
                    if (!string.IsNullOrEmpty(lookupValue))
                    {
                        TotalRecords = lookupValue;
                    }
                }
                catch (Exception)
                {
                    // Log the exception if necessary
                    // TotalRecords remains "50" as default
                }

                string RequestId = Guid.NewGuid().ToString();
                Console.WriteLine("Get Query entered - updated - " + RequestId);
                //_objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name + " GetDocuments - Request object of-" + RequestId, "", BOCommon.JsonConvertObjectToString(new { searchId = caseId }), _httpContextAccessor.HttpContext.User.Identity.Name);
                if (string.IsNullOrEmpty(caseId))
                {
                    return Ok("Not a valid case");
                }
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                DOC360Request objDocsRequest = new DOC360Request();
                Criteria objCriteria = new Criteria();
                FilterClaus objFilterClaus = new FilterClaus();

                objDocsRequest.indexName = docclassTypeName;
                objDocsRequest.totalRecords = Convert.ToInt32(TotalRecords);
                objDocsRequest.scrollId = "";
                objFilterClaus.name = "u_cse_id";
                objFilterClaus.value = caseId;// "U0291244003";
                objFilterClaus.type = "equal";
                List<FilterClaus> lstfilterClauses = new List<FilterClaus>();
                lstfilterClauses.Add(objFilterClaus);
                objCriteria.filterClauses = lstfilterClauses;
                objDocsRequest.criteria = objCriteria;

                ExceptionTypes exceptionResult;

                if (_EnableHCPUpload == "1")
                {
                    exceptionResult = objDOC360HCPMethods.GetDocuments(objDocsRequest, out objDOECAAResponse, out Message);
                }
                else
                {
                    exceptionResult = objDOC360Methods.GetDocuments(objDocsRequest, out objDOECAAResponse, out Message);
                }

                //_objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name + " GetDocuments - Response object of-" + RequestId, "", exceptionResult.ToString() + "-" + Message, _httpContextAccessor.HttpContext.User.Identity.Name);
                Console.WriteLine("Get Query completed");
                if (exceptionResult == ExceptionTypes.Success)
                {
                    Console.WriteLine("Get Query Success");
                    return Ok(objDOECAAResponse);
                }
                else
                {
                    return Ok(Message);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Get Query Exception-" + ex.Message);
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
        }

        /// <summary>
        /// Get document details of global id to check doc available or not
        /// </summary>
        /// <param name="objectId">This is global doc id</param>
        /// <returns>Response object with list of documents</returns>
        public ActionResult GetDocumentFromObjectId(string objectId, string docclassTypeName = "u_clinical_docs")
        {
            DOC360Methods objDOC360Methods = new DOC360Methods(_memoryCacheHelper, _objConfiguration, _objBOCommon);
            DOC360HCPMethod objDOC360HCPMethods = new DOC360HCPMethod(_memoryCacheHelper, _objConfiguration, _objBOCommon);
            DOECAAResponse objDOECAAResponse = null;
            String Message = string.Empty;
            source = BOCommon.GetRefererURI(Request);
            EnsureLookupConfig();
            try
            {
                string RequestId = Guid.NewGuid().ToString();
                _objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name + " GetDocumentFromObjectId - Request object of-" + RequestId, "", BOCommon.JsonConvertObjectToString(new { searchId = objectId }), _httpContextAccessor.HttpContext.User.Identity.Name);
                if (string.IsNullOrEmpty(objectId))
                {
                    return Ok("No global doc id sent!");
                }
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                DOC360Request objDocsRequest = new DOC360Request();
                Criteria objCriteria = new Criteria();
                FilterClaus objFilterClaus = new FilterClaus();

                objDocsRequest.indexName = docclassTypeName;
                objFilterClaus.name = "u_gbl_doc_id";
                objFilterClaus.value = objectId.Split("|")[0];// "3aaf0b0f-2080-4e9d-a25d-b48239c36e99";
                objFilterClaus.type = "equal";
                List<FilterClaus> lstfilterClauses = new List<FilterClaus>();
                lstfilterClauses.Add(objFilterClaus);
                objCriteria.filterClauses = lstfilterClauses;
                objDocsRequest.criteria = objCriteria;

                ExceptionTypes exceptionResult;

                if (_EnableHCPUpload == "1")
                {
                    exceptionResult = objDOC360HCPMethods.GetDocuments(objDocsRequest, out objDOECAAResponse, out Message);
                }
                else
                {
                    exceptionResult = objDOC360Methods.GetDocuments(objDocsRequest, out objDOECAAResponse, out Message);
                }

                _objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name + " GetDocumentFromObjectId - Response object of-" + RequestId, "", exceptionResult.ToString() + "-" + Message, _httpContextAccessor.HttpContext.User.Identity.Name);

                if (exceptionResult == ExceptionTypes.Success)
                {
                    return Ok(objDOECAAResponse);
                }
                else
                {
                    return Ok(Message);
                }
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
        }

        /// <summary>
        /// Get documents by filter vlause based on property passed by user
        /// </summary>
        /// <param name="propName">Name of the property to filter in filter clause</param>
        /// <param name="value">Value of the property </param>
        /// <returns>Response object with list of documents</returns>
        public ActionResult GetDocumentsByCriteria(string propName, string value, string docclassTypeName = "u_clinical_docs", bool SetToProd = false)
        {
            DOC360Methods objDOC360Methods = new DOC360Methods(_memoryCacheHelper, _objConfiguration, _objBOCommon);
            DOC360HCPMethod objDOC360HCPMethods = new DOC360HCPMethod(_memoryCacheHelper, _objConfiguration, _objBOCommon);
            DOECAAResponse objDOECAAResponse = null;
            String Message = string.Empty;
            source = BOCommon.GetRefererURI(Request);
            EnsureLookupConfig();
            try
            {
                string RequestId = Guid.NewGuid().ToString();
                _objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name + " GetDocumentFromObjectId - Request object of-" + RequestId, "", BOCommon.JsonConvertObjectToString(new { name = propName, valueInput = value }), _httpContextAccessor.HttpContext.User.Identity.Name);

                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                DOC360Request objDocsRequest = new DOC360Request();
                Criteria objCriteria = new Criteria();
                FilterClaus objFilterClaus = new FilterClaus();

                objDocsRequest.indexName = docclassTypeName;
                objFilterClaus.name = propName;
                objFilterClaus.value = value;
                objFilterClaus.type = "equal";
                List<FilterClaus> lstfilterClauses = new List<FilterClaus>();
                lstfilterClauses.Add(objFilterClaus);
                objCriteria.filterClauses = lstfilterClauses;
                objDocsRequest.criteria = objCriteria;

                ExceptionTypes exceptionResult;

                if (_EnableHCPUpload == "1")
                {
                    exceptionResult = objDOC360HCPMethods.GetDocuments(objDocsRequest, out objDOECAAResponse, out Message, SetToProd);
                }
                else
                {
                    exceptionResult = objDOC360Methods.GetDocuments(objDocsRequest, out objDOECAAResponse, out Message, SetToProd);
                }

                _objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name + " GetDocumentFromObjectId - Response object of-" + RequestId, "", exceptionResult.ToString() + "-" + Message, _httpContextAccessor.HttpContext.User.Identity.Name);

                if (exceptionResult == ExceptionTypes.Success)
                {
                    return Ok(objDOECAAResponse);
                }
                else
                {
                    return Ok(Message);
                }
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
        }

        /// <summary>
        /// Uploading document to DOC360 getting frile as multiform data from request and file and metadata
        /// </summary>
        /// <returns></returns>   
        [HttpPost]
        [RequestSizeLimit(200 * 1024 * 1024)] // 200 MB
        public DOC360Response CreateDocument()
        {
            string RequestId = "";
            long fileSize = 0;
            string requestSource = "";
            DOC360Methods objDOC360Methods = new DOC360Methods(_memoryCacheHelper, _objConfiguration, _objBOCommon);
            var provider = new MultipartMemoryStreamProvider();
            EDMSServiceMethods objEDMSServiceMethods = new EDMSServiceMethods(_httpContextAccessor, _objConfiguration);
            DOC360Response objDOC360Response = new DOC360Response();
            objDOC360Response.messageMap = new messageMap();
            DOECAADocument objDOECAADocument = null;
            string fileName = string.Empty;
            source = BOCommon.GetRefererURI(Request);
            byte[] fileBytes = null;

            try
            {
                try { requestSource = Request.Host.Value; } catch { }
                EnsureLookupConfig();
                RequestId = Guid.NewGuid().ToString();
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                var formData = _httpContextAccessor.HttpContext.Request.Form;

                if (formData != null && formData.Count > 0)
                {
                    foreach (IFormFile formFile in formData.Files)
                    {
                        fileName = formFile.FileName;
                        if (formFile.Length > 0)
                        {
                            using (var ms = new MemoryStream())
                            {
                                formFile.CopyTo(ms);
                                fileBytes = ms.ToArray();
                            }
                        }
                    }
                }

                string strObjDOECAADocument = _httpContextAccessor.HttpContext.Request.Form.ToArray()[0].Value;
                objDOECAADocument = JsonConvert.DeserializeObject<DOECAADocument>(strObjDOECAADocument);
                objDOECAADocument.ContentStream = fileBytes;
                objDOECAADocument.CreatedBy = _username;
                objDOECAADocument.ContentStreamFileName = fileName;
                fileSize = fileBytes.Length;

                if (_EnableHCPUpload == "1")
                {
                    // HCP unified method — size branching handled internally
                    objDOC360Response = _objDOC360HCPMethods.CreateDocument(objDOECAADocument);
                }
                else
                {
                    if (fileBytes.Length > Convert.ToInt64(_fileSizeLimit))
                    {
                        if (int.TryParse(_toEnableNewStargate, out int enableFlag) && enableFlag == 1)
                        {
                            _objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name + " CreateDocumentWithStargate - Request object of-" + RequestId, fileSize.ToString(), source, _httpContextAccessor.HttpContext.User.Identity.Name);
                            objDOC360Response = objDOC360Methods.CreateDocumentWithStargate(objDOECAADocument);
                        }
                        else
                        {
                            _objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name + " CreateDocumentWithDirectClient - Request object of-" + RequestId, fileSize.ToString(), source, _httpContextAccessor.HttpContext.User.Identity.Name);
                            objDOC360Response = objDOC360Methods.CreateDocumentWithDirectClient(objDOECAADocument);
                        }
                    }
                    else
                    {
                        objDOC360Response = objDOC360Methods.CreateDocument(objDOECAADocument);
                    }
                }

                if (objDOC360Response != null && (objDOC360Response.globalDocId != null || objDOC360Response.global_doc_id != null))
                {
                    _objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name + " Response Success - " + RequestId,
                        fileName + " - size-" + fileSize.ToString(), BOCommon.JsonConvertObjectToString(objDOC360Response),
                        _httpContextAccessor.HttpContext.User.Identity.Name);
                }
                else
                {
                    _objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name + " Failed - " + RequestId,
                        fileName + " - size-" + fileSize.ToString(), BOCommon.JsonConvertObjectToString(objDOC360Response),
                        _httpContextAccessor.HttpContext.User.Identity.Name);
                }
            }
            catch (Exception ex)
            {
                _objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name + " Exception - " + RequestId,
                    fileName + " - size- " + fileSize.ToString(), ex.Message, _httpContextAccessor.HttpContext.User.Identity.Name);

                objDOC360Response.Status = "500";
                List<string> uploaderrors = new List<string>();
                uploaderrors.Add(ex.Message);
                objDOC360Response.messageMap.error = uploaderrors;
                if (source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(0, this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), ex.Message, ex.ToString());
                }
                else
                {
                    _objBOCommon.LogError(this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                }
            }

            return objDOC360Response;
        }

        [HttpPost]
        public DOC360Response CreateDocumentBySourceId(string documentType, string sourceDocumentType, string SourceGlobalDocId)
        {
            string RequestId = "";
            long fileSize = 0;
            string requestSource = "";
            DOC360Methods objDOC360Methods = new DOC360Methods(_memoryCacheHelper, _objConfiguration, _objBOCommon);
            var provider = new MultipartMemoryStreamProvider();
            EDMSServiceMethods objEDMSServiceMethods = new EDMSServiceMethods(_httpContextAccessor, _objConfiguration);
            DOC360Response objDOC360Response = new DOC360Response();
            objDOC360Response.messageMap = new messageMap();
            DOECAADocument objDOECAADocument = null;
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
                RequestId = Guid.NewGuid().ToString();
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                EnsureLookupConfig();

                string strObjDOECAADocument = _httpContextAccessor.HttpContext.Request.Form.ToArray()[0].Value;
                objDOECAADocument = JsonConvert.DeserializeObject<DOECAADocument>(strObjDOECAADocument);
                objDOECAADocument.DocClassToUpload = documentType;
                objDOECAADocument.SourceDocClass = sourceDocumentType;
                objDOECAADocument.SourceGlobalDocId = SourceGlobalDocId;

                if (_EnableHCPUpload == "1")
                {
                    objDOC360Response = _objDOC360HCPMethods.CreateDocumentFromSourceWithHcp(objDOECAADocument);
                }
                else
                {
                    objDOC360Response = objDOC360Methods.CreateDocumentFromSource(objDOECAADocument);
                }

                if (objDOC360Response != null && (objDOC360Response.globalDocId != null || objDOC360Response.global_doc_id != null))
                {
                    _objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name + " CreateDocument - Response Success - " + RequestId, fileName + " - size-" + fileSize.ToString(), BOCommon.JsonConvertObjectToString(objDOC360Response), _httpContextAccessor.HttpContext.User.Identity.Name);
                }
                else
                {
                    _objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name + " CreateDocument - Failed - " + RequestId, fileName + " - size-" + fileSize.ToString(), BOCommon.JsonConvertObjectToString(objDOC360Response), _httpContextAccessor.HttpContext.User.Identity.Name);
                }
            }
            catch (Exception ex)
            {
                _objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name + " Exception - " + RequestId, fileName + " - size- " + fileSize.ToString(), ex.Message, _httpContextAccessor.HttpContext.User.Identity.Name);

                objDOC360Response.Status = "500";
                List<string> uploaderrors = new List<string>();
                uploaderrors.Add("Bad Request");
                objDOC360Response.messageMap.error = uploaderrors;
                if (source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(0, this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), ex.Message, ex.ToString());
                }
                else
                {
                    _objBOCommon.LogError(this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                }
            }

            return objDOC360Response;
        }
        /// <summary>
        /// Download document
        /// </summary>
        /// <param name="gbldocid"></param>
        /// <returns></returns>
        public ActionResult GetContentStream(string gbldocid, string docclassTypeName = "u_clinical_docs", bool pointToProd = false, bool CPMLStartPageAndEndPageRange = false, int startPage = 0, int endPage = 0, bool IsCPMLDoc360 = false, bool IsEnIPointToProd = false, bool IsCnSdoc360ProdValue = false)
        {
            DOC360Methods objDOC360Methods = new DOC360Methods(_memoryCacheHelper, _objConfiguration, _objBOCommon);
            DOECAAResponse objDOECAAResponse = null;
            String Message = string.Empty;
            source = BOCommon.GetRefererURI(Request);
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            string contentType;
            byte[] docRes = null;
            var fileName = "doc360File";
            try
            {
                string RequestId = Guid.NewGuid().ToString();
                _objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name + " GetDocumentContent - Request object of-" + RequestId, "", BOCommon.JsonConvertObjectToString(new { searchId = gbldocid }), _httpContextAccessor.HttpContext.User.Identity.Name);

                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                DOC360Request objDocsRequest = new DOC360Request();
                Criteria objCriteria = new Criteria();
                FilterClaus objFilterClaus = new FilterClaus();

                objDocsRequest.typeName = docclassTypeName;
                objFilterClaus.name = "u_gbl_doc_id";
                objFilterClaus.value = gbldocid.Split("|")[0];
                objFilterClaus.type = "equal";
                List<FilterClaus> lstfilterClauses = new List<FilterClaus>();
                lstfilterClauses.Add(objFilterClaus);
                objCriteria.filterClauses = lstfilterClauses;
                objDocsRequest.criteria = objCriteria;

                if (CPMLStartPageAndEndPageRange)
                {
                    objDocsRequest.CPMLDoc360Request.startPage = startPage;
                    objDocsRequest.CPMLDoc360Request.endPage = endPage;
                    objDocsRequest.CPMLDoc360Request.IsCPMLCasePaginationEnable = CPMLStartPageAndEndPageRange;
                }

                if (IsCPMLDoc360)
                {
                    objDocsRequest.CPMLDoc360Request.IsCPMLCase = true;
                }

                _objBOCommon.GetRRTGPSAPiConfig(out lstATSLookupMaster);
                EnsureLookupConfig();
                if (_EnableHCPUpload == "1")
                {
                    // HCP — primary endpoint
                    response = _objDOC360HCPMethods.GetDocumentContentWithHcp(objDocsRequest, out objDOECAAResponse, out Message, pointToProd, IsEnIPointToProd, IsCnSdoc360ProdValue: IsCnSdoc360ProdValue);
                }
                else
                {
                    // Mode 0 (default) — legacy Stargate
                    response = objDOC360Methods.GetDocumentContent(objDocsRequest, out objDOECAAResponse, out Message, pointToProd, IsEnIPointToProd, IsCnSdoc360ProdValue: IsCnSdoc360ProdValue);
                }

                if (response != null && response.Content != null)
                {
                    docRes = response.Content.ReadAsByteArrayAsync().Result;

                    ////Set the Response Content Length.
                    response.Content.Headers.ContentLength = Convert.ToInt64(docRes.Length);

                    string extension = objDOC360Methods.GetFileExtension(response.Content.Headers.ContentType.MediaType);

                    if (response.Content.Headers != null && response.Content.Headers.ContentDisposition != null
                        && !string.IsNullOrEmpty(response.Content.Headers.ContentDisposition.FileName))
                    {
                        fileName = response.Content.Headers.ContentDisposition.FileName;
                        fileName = fileName.Replace("\"", "");
                    }

                    contentType = response.Content.Headers.ContentType.MediaType;
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.NotFound, "No response content found/Bad request");
                }

                _objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name + " GetDocumentContent - Response object of-" + RequestId, "", response.Content.Headers.ToString() + "-" + Message, _httpContextAccessor.HttpContext.User.Identity.Name);
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
            return File(docRes, contentType, fileName);
        }

        public DOC360Response DeleteDocument(string gbldocid)
        {
            DOC360Methods objDOC360Methods = new DOC360Methods(_memoryCacheHelper, _objConfiguration, _objBOCommon);
            DOC360Response objDOECAAResponse = null;
            String Message = string.Empty;
            source = BOCommon.GetRefererURI(Request);
            try
            {
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;

                EnsureLookupConfig(); 
                
                if (_EnableHCPUpload == "1")
                {
                    // HCP only — primary endpoint
                    objDOECAAResponse = _objDOC360HCPMethods.DeleteDocument(gbldocid);
                }
                else
                {
                    if (int.TryParse(_toEnableNewStargate, out int enableFlag) && enableFlag == 1)
                    {
                        // Stargate only
                        objDOECAAResponse = objDOC360Methods.DeleteDocumentWithStarGate(gbldocid);
                    }
                    else
                    {
                        // Mode 0 — legacy Direct Client only
                        objDOECAAResponse = objDOC360Methods.DeleteDocument(gbldocid);
                    }
                }
            }
            catch (Exception ex)
            {
                objDOECAAResponse = new DOC360Response { Status = "Failed" };

                if (source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(0, this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), ex.Message, ex.ToString());
                }
                else
                {
                    _objBOCommon.LogError(this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                }
            }
            _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(),
                System.Reflection.MethodBase.GetCurrentMethod().Name + "-Response",
                BOCommon.JsonConvertObjectToString(objDOECAAResponse), _username);

            return objDOECAAResponse;
        }

        public ActionResult GetTotalPageCount(string gbldocid, string docclassTypeName = "u_clinical_docs", bool pointToProd = false, bool IsEnIPointToProd = false,bool IsCnSdoc360ProdValue=false)
        {
            DOC360Methods objDOC360Methods = new DOC360Methods(_memoryCacheHelper, _objConfiguration, _objBOCommon);
            String Message = string.Empty;
            source = BOCommon.GetRefererURI(Request);
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            string contentType;

            try
            {
                string RequestId = Guid.NewGuid().ToString();
                _objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name + " GetTotalPageCount - Request object of-" + RequestId, "", BOCommon.JsonConvertObjectToString(new { searchId = gbldocid }), _httpContextAccessor.HttpContext.User.Identity.Name);

                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                DOC360Request objDocsRequest = new DOC360Request();
                Criteria objCriteria = new Criteria();
                FilterClaus objFilterClaus = new FilterClaus();

                objDocsRequest.typeName = docclassTypeName;
                objFilterClaus.name = "u_gbl_doc_id";
                objFilterClaus.value = gbldocid.Split("|")[0];// "U0291244003";
                objFilterClaus.type = "equal";
                objDocsRequest.indexName = objDocsRequest.typeName;

                List<FilterClaus> lstfilterClauses = new List<FilterClaus>();
                lstfilterClauses.Add(objFilterClaus);


                objCriteria.filterClauses = lstfilterClauses;
                objDocsRequest.criteria = objCriteria;


                response = objDOC360Methods.GetTotalPageCount(pointToProd, objDocsRequest, IsEnIPointToProd, IsCnSdoc360ProdValue);
                var result = string.Empty;
                if (response != null && response.Content != null && response.StatusCode == HttpStatusCode.OK)
                {
                    result = response.Content.ReadAsStringAsync().Result;

                    DOC360Response objDO360Response = JsonConvert.DeserializeObject<DOC360Response>(result);
                    if (objDO360Response != null && objDO360Response.recordsList != null && objDO360Response.recordsList.Count > 0)
                    {

                        if (objDO360Response.recordsList[0].contentType != null && objDO360Response.recordsList[0].contentType.ToLower().Contains("tiff"))
                        {
                            objDocsRequest.CPMLDoc360Request.IsCPMLCase = true;
                            var _getTiFFPageCount = GetCPMLContentStreamTiffTotalCount(objDocsRequest, pointToProd, IsCnSdoc360ProdValue) as OkObjectResult;
                            var GetTiFFPageCount = _getTiFFPageCount?.Value?.ToString();
                            return Ok(new
                            {
                                TotalPages = long.TryParse(GetTiFFPageCount, out var _result) ? _result : 0,
                                DocumentType = "TIFF"
                            });
                        }
                        else
                        {
                            return Ok(new
                            {
                                TotalPages = objDO360Response.recordsList[0].totalPages,
                                DocumentType = objDO360Response.recordsList[0].contentType
                            });
                        }

                    }
                    else
                    {
                        return Ok("No records found");
                    }
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.NotFound, "No response content found/Bad request");
                }
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
        }

        /// <summary>
        /// Uploading document to DOC360 getting frile as multiform data from request and file and metadata
        /// If file size is large (>28.6MB), use GetAuthorizationForLargeUpload before calling this method to handle 401 unauthorized error
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public DOC360Response CreateDocumentWithDirectClient()
        {
            DOC360Methods objDOC360Methods = new DOC360Methods(_memoryCacheHelper, _objConfiguration, _objBOCommon);
            var provider = new MultipartMemoryStreamProvider();
            EDMSServiceMethods objEDMSServiceMethods = new EDMSServiceMethods(_httpContextAccessor, _objConfiguration);
            DOC360Response objDOC360Response = new DOC360Response();
            objDOC360Response.messageMap = new messageMap();
            DOECAADocument objDOECAADocument = null;
            string fileName = string.Empty;
            source = BOCommon.GetRefererURI(Request);
            byte[] fileBytes = null;
            try
            {
                string RequestId = Guid.NewGuid().ToString();
                _objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name + " CreateDocument - Request object of-" + RequestId, "", BOCommon.JsonConvertObjectToString(new { searchId = Request }), _httpContextAccessor.HttpContext.User.Identity.Name);

                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                var formData = _httpContextAccessor.HttpContext.Request.Form;
                if (formData != null && formData.Count > 0)
                {
                    foreach (IFormFile formFile in formData.Files)
                    {
                        fileName = formFile.FileName;
                        if (formFile.Length > 0)
                        {
                            using (var ms = new MemoryStream())
                            {
                                formFile.CopyTo(ms);
                                fileBytes = ms.ToArray();
                                // string s = Convert.ToBase64String(fileBytes);
                            }
                        }
                    }
                }
                string strObjDOECAADocument = _httpContextAccessor.HttpContext.Request.Form.ToArray()[0].Value;
                objDOECAADocument = JsonConvert.DeserializeObject<DOECAADocument>(strObjDOECAADocument);
                objDOECAADocument.ContentStream = fileBytes;
                objDOECAADocument.CreatedBy = _username;
                objDOECAADocument.ContentStreamFileName = fileName;
                objDOC360Response = objDOC360Methods.CreateDocumentWithDirectClient(objDOECAADocument);

                _objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name + " CreateDocument - Response object of-" + RequestId, "", BOCommon.JsonConvertObjectToString(objDOC360Response), _httpContextAccessor.HttpContext.User.Identity.Name);
            }
            catch (Exception ex)
            {
                objDOC360Response.Status = "500";
                List<string> uploaderrors = new List<string>();
                uploaderrors.Add("Bad Request");
                objDOC360Response.messageMap.error = uploaderrors;
                if (source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(0, this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), ex.Message, ex.ToString());
                }
                else
                {
                    _objBOCommon.LogError(this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                }
            }

            return objDOC360Response;
        }

        
        /// <summary>
        /// sprint 26(US4940015) - get the IFP SBC and COC documents
        /// </summary>
        /// <param name="planId"></param>
        /// <param name="planYear"></param>
        /// <returns></returns>
        public ActionResult GetSbcCocQuery(string planId, string planYear)
        {
            DOC360Methods objDOC360Methods = new DOC360Methods(_memoryCacheHelper, _objConfiguration, _objBOCommon);
            DOC360HCPMethod objDOC360HCPMethods = new DOC360HCPMethod(_memoryCacheHelper, _objConfiguration, _objBOCommon);
            DOECAAResponse objDOECAAResponse = null;
            String Message = string.Empty;
            source = BOCommon.GetRefererURI(Request);
            EnsureLookupConfig();
            try
            {
                _objBOCommon.GetRRTGPSAPiConfig(out lstATSLookupMaster);
                string TotalRecords = "10";

                string RequestId = Guid.NewGuid().ToString();
                Console.WriteLine("Get Query entered - updated - " + RequestId);
                //_objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name + " GetDocuments - Request object of-" + RequestId, "", BOCommon.JsonConvertObjectToString(new { searchId = caseId }), _httpContextAccessor.HttpContext.User.Identity.Name);
                if (string.IsNullOrEmpty(planId) || string.IsNullOrEmpty(planYear))
                {
                    return Ok("Not a valid case");
                }
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                DOC360Request objDocsRequest = new DOC360Request();
                Criteria objCriteria = new Criteria();
                objDocsRequest.indexName = "u_exchange_sbc_coc";
                objDocsRequest.totalRecords = Convert.ToInt32(TotalRecords);
                objDocsRequest.scrollId = "";

                List<FilterClaus> lstfilterClauses = new List<FilterClaus>()
                {
                    new FilterClaus {name= "u_plan_id", value= planId,type="equal"},
                    new FilterClaus{name="u_plan_year",value= planYear,type="equal"},
                    new FilterClaus{name="u_lang",value = ConstantTexts.DOC360EmglishFiltertext, type = "equal"}
                };

                objCriteria.filterClauses = lstfilterClauses;
                objDocsRequest.criteria = objCriteria;

                ExceptionTypes exceptionResult;

                if (_EnableHCPUpload == "1")
                {
                    exceptionResult = objDOC360HCPMethods.GetDocuments(objDocsRequest, out objDOECAAResponse, out Message);
                }
                else
                {
                    exceptionResult = objDOC360Methods.GetDocuments(objDocsRequest, out objDOECAAResponse, out Message);
                }
                //_objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name + " GetDocuments - Response object of-" + RequestId, "", exceptionResult.ToString() + "-" + Message, _httpContextAccessor.HttpContext.User.Identity.Name);
                Console.WriteLine("Get Query completed");
                if (exceptionResult == ExceptionTypes.Success)
                {
                    Console.WriteLine("Get Query Success");
                    return Ok(objDOECAAResponse);
                }
                else
                {
                    return Ok(Message);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Get Query Exception-" + ex.Message);
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
        }

        /// <summary>
        /// get the IFP SBC and COC file and upload to the case
        /// </summary>
        /// <param name="gbldocid"></param>
        /// <param name="CaseID"></param>
        /// <param name="Title"></param>
        /// <param name="docType"></param>
        /// <returns></returns>
        public ActionResult GetSbcFileByIdAndUploadToCase(string gbldocid, string CaseID, string Title, string docType)
        {
            DOC360Methods objDOC360Methods = new DOC360Methods(_memoryCacheHelper, _objConfiguration, _objBOCommon);
            DOC360HCPMethod objDOC360HCPMethods = new DOC360HCPMethod(_memoryCacheHelper, _objConfiguration, _objBOCommon);
            DOECAAResponse objDOECAAResponse = null;
            String Message = string.Empty;
            source = BOCommon.GetRefererURI(Request);
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            string RequestId = "";
            byte[] fileBytes = null;
            var fileName = "doc360File";
            DOC360Response objDOC360Response = new DOC360Response();
            objDOC360Response.messageMap = new messageMap();
            long fileSize = 0;
            EnsureLookupConfig();
            try
            {
                RequestId = Guid.NewGuid().ToString();
                _objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name + " GetDocumentContent - Request object of-" + RequestId, "", BOCommon.JsonConvertObjectToString(new { searchId = gbldocid }), _httpContextAccessor.HttpContext.User.Identity.Name);
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                DOC360Request objDocsRequest = new DOC360Request();
                Criteria objCriteria = new Criteria();
                FilterClaus objFilterClaus = new FilterClaus();
                objDocsRequest.typeName = docType;
                objFilterClaus.name = "u_gbl_doc_id";
                objFilterClaus.value = gbldocid.Split("|")[0];// "U0291244003";
                objFilterClaus.type = "equal";
                List<FilterClaus> lstfilterClauses = new List<FilterClaus>();
                lstfilterClauses.Add(objFilterClaus);
                objCriteria.filterClauses = lstfilterClauses;
                objDocsRequest.criteria = objCriteria;
                response = objDOC360Methods.GetDocumentContent(objDocsRequest, out objDOECAAResponse, out Message);
                if (response != null && response.Content != null)
                {
                    fileBytes = response.Content.ReadAsByteArrayAsync().Result;

                    if (response.Content.Headers != null && response.Content.Headers.ContentDisposition != null
                           && !string.IsNullOrEmpty(response.Content.Headers.ContentDisposition.FileName))
                    {
                        fileName = response.Content.Headers.ContentDisposition.FileName;
                        fileName = fileName.Replace("\"", "");
                    }

                    _objBOCommon.GetRRTGPSAPiConfig(out lstATSLookupMaster);

                    DOECAADocument objDOECAADocument = new DOECAADocument()
                    {
                        CaseID = CaseID,
                        ContentStream = fileBytes,
                        CreatedBy = _username,
                        ContentStreamFileName = fileName,
                        Title = Title
                    };

                    string FileSizeLimit = _objBOCommon.GetLookupValueBasedonCon(lstATSLookupMaster, (long)RRTGPSAPIConfig.FileSizeLimit, 0);
                    fileSize = fileBytes.Length;

                    if (_EnableHCPUpload == "1")
                    {
                        // HCP unified method — size branching handled internally
                        objDOC360Response = objDOC360HCPMethods.CreateDocument(objDOECAADocument);
                    }
                    else
                    {
                        if (fileBytes.Length > Convert.ToInt64(_fileSizeLimit))
                        {
                            _objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name + " CreateDocumentWithDirectClient - Request object of-" + RequestId, fileSize.ToString(), source, _httpContextAccessor.HttpContext.User.Identity.Name);
                            objDOC360Response = objDOC360Methods.CreateDocumentWithDirectClient(objDOECAADocument);
                        }
                        else
                        {
                            objDOC360Response = objDOC360Methods.CreateDocument(objDOECAADocument);
                        }
                    }

                    if (objDOC360Response != null && (objDOC360Response.globalDocId != null || objDOC360Response.global_doc_id != null))
                    {
                        _objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name + " CreateDocument - Response Success - " + RequestId, fileName + " - size-" + fileSize.ToString(), BOCommon.JsonConvertObjectToString(objDOC360Response), _httpContextAccessor.HttpContext.User.Identity.Name);
                    }
                    else
                    {
                        _objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name + " CreateDocument - Failed - " + RequestId, fileName + " - size-" + fileSize.ToString(), BOCommon.JsonConvertObjectToString(objDOC360Response), _httpContextAccessor.HttpContext.User.Identity.Name);
                    }

                }
                else
                {
                    objDOC360Response.Status = "404";
                    objDOC360Response.responseMesage = "No response content found/Bad request";
                }
                _objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name + " GetDocumentContent - Response object of-" + RequestId, "", response.Content.Headers.ToString() + "-" + Message, _httpContextAccessor.HttpContext.User.Identity.Name);

            }
            catch (Exception ex)
            {
                _objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name + " Exception - " + RequestId, fileName + " - size- " + fileSize.ToString(), ex.Message, _httpContextAccessor.HttpContext.User.Identity.Name);

                objDOC360Response.Status = "500";
                List<string> uploaderrors = new List<string>();
                uploaderrors.Add("Bad Request");
                objDOC360Response.messageMap.error = uploaderrors;
                objDOC360Response.responseMesage = ex.Message;
                if (source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(0, this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), ex.Message, ex.ToString());
                }
                else
                {
                    _objBOCommon.LogError(this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                }
            }

            return Ok(objDOC360Response);
        }
        /// <summary>
        /// get the IFP COC documents for the year 2023 onwards
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="planId"></param>
        /// <param name="planYear"></param>
        /// <returns></returns>
        public ActionResult GetCocQuery(string memberId, string planId, string planYear)
        {
            DOC360Methods objDOC360Methods = new DOC360Methods(_memoryCacheHelper, _objConfiguration, _objBOCommon);
            DOC360HCPMethod objDOC360HCPMethods = new DOC360HCPMethod(_memoryCacheHelper, _objConfiguration, _objBOCommon);
            DOECAAResponse objDOECAAResponse = null;
            String Message = string.Empty;
            source = BOCommon.GetRefererURI(Request);
            EnsureLookupConfig();
            try
            {
                _objBOCommon.GetRRTGPSAPiConfig(out lstATSLookupMaster);
                string TotalRecords = "10";

                string RequestId = Guid.NewGuid().ToString();
                Console.WriteLine("Get Query entered - updated - " + RequestId);
                //_objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name + " GetDocuments - Request object of-" + RequestId, "", BOCommon.JsonConvertObjectToString(new { searchId = caseId }), _httpContextAccessor.HttpContext.User.Identity.Name);

                if (string.IsNullOrEmpty(planId) || string.IsNullOrEmpty(planYear))
                {
                    return Ok("Not a valid case");
                }

                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                DOC360Request objDocsRequest = new DOC360Request();
                Criteria objCriteria = new Criteria();
                objDocsRequest.indexName = "u_ifp_sbscr_coc";
                objDocsRequest.totalRecords = Convert.ToInt32(TotalRecords);
                objDocsRequest.scrollId = "";

                List<FilterClaus> lstfilterClauses = new List<FilterClaus>()
                {
                    new FilterClaus{name="u_mbr_id", value=memberId,type="equal"},
                    new FilterClaus {name= "u_plan_id", value= planId,type="equal" },
                    new FilterClaus{name="u_plan_year",value= planYear,type="equal"},
                    new FilterClaus{name="u_lang",value = "ENGLISH", type = "equal"}
                };

                objCriteria.filterClauses = lstfilterClauses;
                objDocsRequest.criteria = objCriteria;

                ExceptionTypes exceptionResult;

                if (_EnableHCPUpload == "1")
                {
                    exceptionResult = objDOC360HCPMethods.GetDocuments(objDocsRequest, out objDOECAAResponse, out Message);
                }
                else
                {
                    exceptionResult = objDOC360Methods.GetDocuments(objDocsRequest, out objDOECAAResponse, out Message);
                }
                //_objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name + " GetDocuments - Response object of-" + RequestId, "", exceptionResult.ToString() + "-" + Message, _httpContextAccessor.HttpContext.User.Identity.Name);
                Console.WriteLine("Get Query completed");
                if (exceptionResult == ExceptionTypes.Success)
                {
                    Console.WriteLine("Get Query Success");
                    return Ok(objDOECAAResponse);
                }
                else
                {
                    return Ok(Message);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Get Query Exception-" + ex.Message);
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
        }
        [HttpPost]
        public DOC360Response UploadDocument()
        {
            string RequestId = "";
            long fileSize = 0;
            string requestSource = "";
            DOC360Methods objDOC360Methods = new DOC360Methods(_memoryCacheHelper, _objConfiguration, _objBOCommon);
            var provider = new MultipartMemoryStreamProvider();
            DOC360Response objDOC360Response = new DOC360Response();
            objDOC360Response.messageMap = new messageMap();
            DOECAADocument objDOECAADocument = null;
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

                _objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name + " CreateDocument - Request object of - " + RequestId, "DOC360Controller-request", requestSource, _httpContextAccessor.HttpContext.User.Identity.Name);


                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                var formData = _httpContextAccessor.HttpContext.Request.Form;

                if (formData != null && formData.Count > 0)
                {
                    foreach (IFormFile formFile in formData.Files)
                    {
                        fileName = formFile.FileName;
                        if (formFile.Length > 0)
                        {
                            using (var ms = new MemoryStream())
                            {
                                formFile.CopyTo(ms);
                                fileBytes = ms.ToArray();
                                //string s = Convert.ToBase64String(fileBytes);
                            }
                        }
                    }
                }
                string strObjDOECAADocument = _httpContextAccessor.HttpContext.Request.Form.ToArray()[0].Value;
                objDOECAADocument = JsonConvert.DeserializeObject<DOECAADocument>(strObjDOECAADocument);
                objDOECAADocument.ContentStream = fileBytes;
                objDOECAADocument.CreatedBy = _username;
                objDOECAADocument.ContentStreamFileName = fileName;
                fileSize = fileBytes.Length;
                objDOC360Response = objDOC360Methods.UploadDocument(objDOECAADocument);
                if (objDOC360Response != null && (objDOC360Response.globalDocId != null || objDOC360Response.global_doc_id != null))
                {
                    _objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name + " CreateDocument - Response Success - " + RequestId, fileName + " - size-" + fileSize.ToString(), BOCommon.JsonConvertObjectToString(objDOC360Response), _httpContextAccessor.HttpContext.User.Identity.Name);
                }
                else
                {
                    _objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name + " CreateDocument - Failed - " + RequestId, fileName + " - size-" + fileSize.ToString(), BOCommon.JsonConvertObjectToString(objDOC360Response), _httpContextAccessor.HttpContext.User.Identity.Name);
                }

            }
            catch (Exception ex)
            {
                _objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name + " Exception - " + RequestId, fileName + " - size- " + fileSize.ToString(), ex.Message, _httpContextAccessor.HttpContext.User.Identity.Name);

                objDOC360Response.Status = "500";
                List<string> uploaderrors = new List<string>();
                uploaderrors.Add("Bad Request");
                objDOC360Response.messageMap.error = uploaderrors;
                if (source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(0, this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), ex.Message, ex.ToString());
                }
                else
                {
                    _objBOCommon.LogError(this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                }
            }
            return objDOC360Response;
        }
        /// <summary>
        /// Get documents by case id for EnI
        /// </summary>
        /// <param name="caseId">Case id</param>
        /// <returns>Response object with list of documents </returns>
        [HttpGet]
        public ActionResult SearchDocument(string caseId, string docclassTypeName = "u_apl_grv_ltr")
        {
            DOC360Methods objDOC360Methods = new DOC360Methods(_memoryCacheHelper, _objConfiguration, _objBOCommon);
            DOC360HCPMethod objDOC360HCPMethods = new DOC360HCPMethod(_memoryCacheHelper, _objConfiguration, _objBOCommon);
            DOECAAResponse objDOECAAResponse = null;
            String Message = string.Empty;
            source = BOCommon.GetRefererURI(Request);
            EnsureLookupConfig();
            try
            {
                _objBOCommon.GetRRTGPSAPiConfig(out lstATSLookupMaster);
                string TotalRecords = "50";
                try
                {
                    TotalRecords = _objBOCommon.GetLookupValueBasedonCon(lstATSLookupMaster, (long)RRTGPSAPIConfig.TotalRecords, 0);
                }
                catch (Exception)
                {
                    TotalRecords = "50";
                }

                if (TotalRecords == null || TotalRecords == "")
                {
                    TotalRecords = "50";
                }
                string RequestId = Guid.NewGuid().ToString();
                Console.WriteLine("Get Query entered - updated - " + RequestId);
                //_objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name + " GetDocuments - Request object of-" + RequestId, "", BOCommon.JsonConvertObjectToString(new { searchId = caseId }), _httpContextAccessor.HttpContext.User.Identity.Name);
                if (string.IsNullOrEmpty(caseId))
                {
                    return Ok("Not a valid case");
                }
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                DOC360Request objDocsRequest = new DOC360Request();
                Criteria objCriteria = new Criteria();
                FilterClaus objFilterClaus = new FilterClaus();

                objDocsRequest.indexName = docclassTypeName;
                objDocsRequest.totalRecords = Convert.ToInt32(TotalRecords);
                objDocsRequest.scrollId = "";
                objFilterClaus.name = "u_case_id";
                objFilterClaus.value = caseId;// "U0291244003";
                objFilterClaus.type = "equal";
                List<FilterClaus> lstfilterClauses = new List<FilterClaus>();
                lstfilterClauses.Add(objFilterClaus);
                objCriteria.filterClauses = lstfilterClauses;
                objDocsRequest.criteria = objCriteria;

                ExceptionTypes exceptionResult;

                if (_EnableHCPUpload == "1")
                {
                    exceptionResult = objDOC360HCPMethods.GetDocuments(objDocsRequest, out objDOECAAResponse, out Message);
                }
                else
                {
                    exceptionResult = objDOC360Methods.GetDocuments(objDocsRequest, out objDOECAAResponse, out Message);
                }
                //_objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name + " GetDocuments - Response object of-" + RequestId, "", exceptionResult.ToString() + "-" + Message, _httpContextAccessor.HttpContext.User.Identity.Name);
                Console.WriteLine("Get Query completed");
                if (exceptionResult == ExceptionTypes.Success)
                {
                    Console.WriteLine("Get Query Success");
                    return Ok(objDOECAAResponse);
                }
                else
                {
                    return Ok(Message);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Get Query Exception-" + ex.Message);
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
        }
        public ActionResult GetDocumentsBy3Criteria(string tin, string claimNumber, string MemberId, string docClassCategoryId, string fedSearchCategoryId, string lineOfBusinessId, string PaymentTraceNumber = null, string InitialClaimDenailDate = null)
        {
            DOC360Methods objDOC360Methods = new DOC360Methods(_memoryCacheHelper, _objConfiguration, _objBOCommon);
            DOC360HCPMethod objDOC360HCPMethods = new DOC360HCPMethod(_memoryCacheHelper, _objConfiguration, _objBOCommon);
            DOFedSearch objDOECAAResponse = null;
            String Message = string.Empty;
            source = BOCommon.GetRefererURI(Request);
            EnsureLookupConfig();
            try
            {
                string RequestId = Guid.NewGuid().ToString();
                _objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name + " GetDocumentFromObjectId - Request object of-" + RequestId, "", BOCommon.JsonConvertObjectToString(new { name = "tin", valueInput = tin }), _httpContextAccessor.HttpContext.User.Identity.Name);

                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                ProviderFedSearch objDocsRequest = new ProviderFedSearch();
                Criteria2 objCriteria = new Criteria2();


                objDocsRequest.documentCategoryId = docClassCategoryId;
                objDocsRequest.fedratedSearchCategoryId = fedSearchCategoryId;
                objDocsRequest.lineOfBusinessId = lineOfBusinessId;
                List<FilterClaus> lstfilterClauses = new List<FilterClaus>() { };
                if (docClassCategoryId == "7")
                {


                    lstfilterClauses = new List<FilterClaus>()
                {
                    new FilterClaus {name= "u_tin", value= tin,type="equal"},
                    new FilterClaus{name="u_clm_nbr",value= claimNumber,type="equal"},
                    new FilterClaus{name="u_member_id",value= MemberId,type="equal"},
                    new FilterClaus{name="u_chk_nbr",value= PaymentTraceNumber==null?"":PaymentTraceNumber,type="equal"}
                };


                }

                if (docClassCategoryId == "1" && !string.IsNullOrEmpty(InitialClaimDenailDate))
                {
                    //DateTime InitialClaimDenailDateFrom = DateTime.Parse(InitialClaimDenailDate);
                    //DateTime InitialClaimDenailDateTo = InitialClaimDenailDateFrom.AddDays(14);
                    FilterClaus objFilterClaus = new FilterClaus();
                    objFilterClaus.name = "u_eob_dt";
                    objFilterClaus.range = new ANGDDEAPIDO.Range();
                    try
                    {
                        DateTime dateTime;
                        //dateTime = Convert.ToDateTime(InitialClaimDenailDate).ToUniversalTime();
                        dateTime = Convert.ToDateTime(InitialClaimDenailDate, CultureInfo.InvariantCulture).ToUniversalTime();
                        objFilterClaus.range.gte = dateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
                        dateTime = dateTime.AddDays(14);
                        objFilterClaus.range.lte = dateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
                    }
                    catch (Exception)
                    {

                    }
                    objFilterClaus.type = "range";
                    lstfilterClauses.Add(objFilterClaus);
                    objFilterClaus = new FilterClaus();
                    objFilterClaus.name = "u_member_id";
                    objFilterClaus.value = MemberId;
                    objFilterClaus.type = "equal";
                    lstfilterClauses.Add(objFilterClaus);
                    /*lstfilterClauses = new List<FilterClaus>()
                {
                    new FilterClaus{name="u_member_id",value= MemberId,type="equal"},
                };*/
                }
                objCriteria.clause = lstfilterClauses;
                objDocsRequest.criteria = objCriteria;

                ExceptionTypes exceptionResult;

                if (_EnableHCPUpload == "1")
                {
                    exceptionResult = objDOC360HCPMethods.GetProviderFedSearch(objDocsRequest, out objDOECAAResponse, out Message, PaymentTraceNumber);
                }
                else
                {
                    exceptionResult = objDOC360Methods.GetProviderFedSearch(objDocsRequest, out objDOECAAResponse, out Message, PaymentTraceNumber);
                }

                _objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name + " GetDocumentFromObjectId - Response object of-" + RequestId, "", exceptionResult.ToString() + "-" + Message, _httpContextAccessor.HttpContext.User.Identity.Name);

                if (exceptionResult == ExceptionTypes.Success)
                {
                    return Ok(objDOECAAResponse);
                }
                else
                {
                    return Ok(Message);
                }
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
        }
        public ActionResult GetPlaceOfServiceState(string FlnNumber, long claimType, string docclassTypeName = "u_edi_claim", bool isDosRequest = false)
        {
            //var res = GetDocumentsByCriteria("u_fln_dcc", FlnNumber, docclassTypeName);

            string gbldocid = "";

            DOC360Methods objDOC360Methods = new DOC360Methods(_memoryCacheHelper, _objConfiguration, _objBOCommon);
            DOC360HCPMethod objDOC360HCPMethods = new DOC360HCPMethod(_memoryCacheHelper, _objConfiguration, _objBOCommon);
            DOECAAResponse objDOECAAResponse = null;
            String Message = string.Empty;
            source = BOCommon.GetRefererURI(Request);
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            string StateCode = string.Empty;
            EnsureLookupConfig();
            try
            {

                string RequestId = Guid.NewGuid().ToString();
                _objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name + " GetDocumentContent - Request object of-" + RequestId, "", BOCommon.JsonConvertObjectToString(new { searchId = gbldocid }), _httpContextAccessor.HttpContext.User.Identity.Name);

                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                DOC360Request objDocsRequest = new DOC360Request();
                Criteria objCriteria = new Criteria();
                FilterClaus objFilterClaus = new FilterClaus();

                objDocsRequest.indexName = docclassTypeName;
                objFilterClaus.name = "u_fln_dcc";
                objFilterClaus.value = FlnNumber;
                objFilterClaus.type = "equal";
                List<FilterClaus> lstfilterClauses = new List<FilterClaus>();
                lstfilterClauses.Add(objFilterClaus);
                objCriteria.filterClauses = lstfilterClauses;
                objDocsRequest.criteria = objCriteria;

                ExceptionTypes exceptionResult;

                if (_EnableHCPUpload == "1")
                {
                    exceptionResult = objDOC360HCPMethods.GetDocuments(objDocsRequest, out objDOECAAResponse, out Message, true);
                }
                else
                {
                    exceptionResult = objDOC360Methods.GetDocuments(objDocsRequest, out objDOECAAResponse, out Message, true);
                }
                if (objDOECAAResponse.lstDOECAADocument != null && objDOECAAResponse.lstDOECAADocument.Count > 0)
                {
                    gbldocid = objDOECAAResponse.lstDOECAADocument[0].DOC360ObjectId.Split('|')[0];
                    DOC360Request objDocsRequest1 = new DOC360Request();
                    Criteria objCriteria1 = new Criteria();
                    FilterClaus objFilterClaus1 = new FilterClaus();
                    objDocsRequest1.typeName = docclassTypeName;
                    objFilterClaus1.name = "u_gbl_doc_id";
                    objFilterClaus1.value = gbldocid.Split("|")[0];// "U0291244003";
                    objFilterClaus1.type = "equal";
                    List<FilterClaus> lstfilterClauses1 = new List<FilterClaus>();
                    lstfilterClauses1.Add(objFilterClaus1);
                    objCriteria1.filterClauses = lstfilterClauses1;
                    objDocsRequest1.criteria = objCriteria1;

                    response = objDOC360Methods.GetDocumentContent(objDocsRequest1, out objDOECAAResponse, out Message, true);
                    if (response != null && response.Content != null)
                    {
                        Stream stream = response.Content.ReadAsStreamAsync().Result;
                        StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                        string content = reader.ReadToEndAsync().Result;
                        string[] lines = content.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                        if (claimType == (long)ClaimTypeCode.Physician)
                        {
                            _objBOCommon.GetATSLookupMaster(out lstATSLookupMaster);
                            List<DOATSLookupMaster> StateCodeList = new List<DOATSLookupMaster>();
                            StateCodeList = _objBOCommon.GetLookupListBasedonType(lstATSLookupMaster, (long)LookupType.StateCode);
                            List<string> StateCodeListValues = new List<string>();
                            StateCodeListValues = StateCodeList.Select(x => " " + x.LookupValue + " ").ToList();
                            if (!isDosRequest)
                            {
                                int indexbox32 = -1;
                                int index32 = -1;
                                int indexend32 = -1;
                                int indexbox32end = -1;
                                for (int i = 0; i < lines.Length; i++)
                                {
                                    if (lines[i].Contains("|32"))
                                    {
                                        indexbox32 = i;
                                        index32 = lines[i].IndexOf("|32");
                                        indexend32 = lines[i].IndexOf("|", index32 + 1);
                                        //break;
                                    }
                                    if (indexbox32 != -1 && lines[i].Contains("+--"))
                                    {
                                        indexbox32end = i;
                                        break;
                                    }
                                }
                                //_objBOCommon.GetATSLookupMaster(out lstATSLookupMaster);
                                //List<DOATSLookupMaster> StateCodeList = new List<DOATSLookupMaster>();
                                //StateCodeList = _objBOCommon.GetLookupListBasedonType(lstATSLookupMaster, (long)LookupType.StateCode);
                                //List<string> StateCodeListValues = new List<string>();
                                //StateCodeListValues = StateCodeList.Select(x => " " + x.LookupValue + " ").ToList();
                                for (int i = indexbox32 + 1; i < indexbox32end; i++)
                                {
                                    foreach (var item in StateCodeListValues)
                                    {
                                        if (lines[i].Substring(index32, indexend32 - index32 + 1).Contains(item))
                                        {
                                            StateCode = item.Substring(1, 2);
                                            
                                            //break;
                                        }
                                    }
                                }
                            }
                            if (isDosRequest)
                            {
                                string datePart1 = "", datePart2 = "";

                                int indexbox24 = -1;
                                int index24 = -1;
                                int indexend24 = -1;
                                int firstLineIndex = -1;
                                int indexend28 = -1;
                                int index28 = -1;
                                int indexbox28end = -1;
                                int indexbox28 = -1;

                                for (int i = 0; i < lines.Length; i++)
                                {
                                    if (lines[i].Contains("|24") || index24 > -1)
                                    {
                                        indexbox24 = i;

                                        index24 = index24 == -1 ? lines[i].IndexOf("|24") : index24;
                                        indexend24 = lines[i].IndexOf("|", index24 + 1);

                                        if (index24 > -1)
                                        {

                                            if (lines[i].StartsWith(" +--") || firstLineIndex > -1)
                                            {
                                                firstLineIndex = firstLineIndex == -1 ? i : firstLineIndex;
                                                int test = 0;
                                                //break;

                                                if (lines[i].StartsWith(" |"))
                                                {
                                                    //StartEndDateIndex = k;

                                                    string startDate;
                                                    string endDate;
                                                    string[] dateParts = lines[i].Split('|');
                                                    if (dateParts.Length > 0)
                                                    {
                                                        //startDate = dateParts[1].Trim();
                                                        startDate = dateParts?.Length > 1 ? dateParts[1].Trim() : null;
                                                        //break;

                                                        if (string.IsNullOrEmpty(datePart1))
                                                        {
                                                            if (test == 0)
                                                            {
                                                                test++;
                                                                datePart1 = startDate.Split(' ')[0];
                                                            }

                                                        }
                                                        datePart2 = startDate.Split(' ')[2];


                                                    }


                                                }
                                                if (lines[i].StartsWith(" +--") && firstLineIndex > -1 && firstLineIndex != i)
                                                {
                                                    break;
                                                }


                                            }
                                        }

                                    }
                                    
                                }
                                for (int i = 0; i < lines.Length; i++)
                                {
                                    
                                    if (lines[i].Contains("|28"))
                                    {
                                        indexbox28 = i;
                                        index28 = lines[i].IndexOf("|28");
                                        indexend28 = lines[i].IndexOf("|", index28 + 1);
                                        //break;
                                    }
                                    if (indexbox28 != -1 && lines[i].Contains("+--"))
                                    {
                                        indexbox28end = i;
                                        break;
                                    }
                                }
                                string TotalBilled = "";
                                for (int i = indexbox28 + 1; i < indexbox28end; i++)
                                {
                                    for(int j = index28; j < indexend28; j++)
                                    {
                                        if (lines[i][j] != ' ' && (Char.IsNumber(lines[i][j]) || lines[i][j] == '.') )
                                        {
                                            TotalBilled = TotalBilled+ lines[i][j];
                                        }
                                    }
                                }
                                datePart1 = !string.IsNullOrEmpty(datePart1) ? datePart1.Substring(0, 2) + "/" + datePart1.Substring(2, 2) + "/" + datePart1.Substring(4, 2) : "";
                                datePart2 = !string.IsNullOrEmpty(datePart2) ? datePart2.Substring(0, 2) + "/" + datePart2.Substring(2, 2) + "/" + datePart2.Substring(4, 2) : "";

                                string formattedDatePart1 = datePart1;
                                DateTime parsedDatePart1;
                                if (!string.IsNullOrEmpty(datePart1) && DateTime.TryParseExact(datePart1, "MM/dd/yy", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDatePart1))
                                {
                                    formattedDatePart1 = parsedDatePart1.ToString("MM/dd/yyyy");
                                }

                                // Convert datePart2 to MM/dd/yyyy if possible
                                string formattedDatePart2 = datePart2;
                                DateTime parsedDatePart2;
                                if (!string.IsNullOrEmpty(datePart2) && DateTime.TryParseExact(datePart2, "MM/dd/yy", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDatePart2))
                                {
                                    formattedDatePart2 = parsedDatePart2.ToString("MM/dd/yyyy");
                                }

                                StartEndDate date = new StartEndDate();
                                date.startDate = formattedDatePart1;
                                date.endDate = formattedDatePart2;
                                date.TotalBilled = TotalBilled;
                                return Ok(date);
                            }
                            else if (string.IsNullOrEmpty(StateCode))
                            {
                                int indexbox33 = -1;
                                int index33 = -1;
                                int indexend33 = -1;
                                int indexbox33end = -1;
                                for (int i = 0; i < lines.Length; i++)
                                {
                                    if (lines[i].Contains("|33"))
                                    {
                                        indexbox33 = i;
                                        index33 = lines[i].IndexOf("|33");
                                        indexend33 = lines[i].IndexOf("|", index33 + 1);
                                        //break;
                                    }
                                    if (indexbox33 != -1 && lines[i].Contains("+--"))
                                    {
                                        indexbox33end = i;
                                        break;
                                    }
                                }
                                
                                for (int i = indexbox33 + 1; i < indexbox33end; i++)
                                {
                                    foreach (var item in StateCodeListValues)
                                    {
                                        if (lines[i].Substring(index33, indexend33 - index33 + 1).Contains(item))
                                        {
                                            StateCode = item.Substring(1, 2);
                                            //break;
                                        }
                                    }
                                }
                            }
                            return Ok(StateCode);
                        }
                        else if (claimType == (long)ClaimTypeCode.Hospital)
                        {
                            if (!isDosRequest)
                            {
                                if (lines.Length > 6)
                                {
                                    int STindex = lines[5].IndexOf("ST:");
                                    StateCode = lines[5].Substring(STindex + 4, 2);
                                    return Ok(StateCode);
                                }
                                else
                                {
                                    return StatusCode((int)HttpStatusCode.NotFound, "No response content found/Bad request 1");
                                }
                            }
                            else
                            {
                                StartEndDate date = new StartEndDate();
                                for (int i = 0; i < lines.Length; i++)
                                {
                                    int indexk = lines[i].IndexOf("COV PER FROM");
                                    
                                    if (indexk > -1)
                                    {
                                        string startDateRaw = lines[i].Substring(indexk + 14, 8);
                                        DateTime parsedStartDate;
                                        string startDate = DateTime.TryParseExact(startDateRaw, "MM/dd/yy", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedStartDate)
                                            ? parsedStartDate.ToString("MM/dd/yyyy")
                                            : startDateRaw;
                                        int indexthru = lines[i].IndexOf("THRU");
                                        string endDateRaw = lines[i].Substring(indexthru + 6, 8);
                                        DateTime parsedEndDate;
                                        string endDate = DateTime.TryParseExact(endDateRaw, "MM/dd/yy", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedEndDate)
                                            ? parsedEndDate.ToString("MM/dd/yyyy")
                                            : endDateRaw;

                                        
                                        date.startDate = startDate;
                                        date.endDate = endDate;

                                        break;
                                    }
                                    int indexf = lines[i].IndexOf("TOT CHARGE");
                                    if (indexf > -1)
                                    {
                                        for (int f = i + 1; f < i+3 ; f++)
                                        {
                                            string totalbilledline = lines[f].ToString();
                                            string TotalBilled = "";
                                            var ArrofStrings = totalbilledline.Split(' ');
                                            var lstStrings = ArrofStrings.Where(x => x != "" && x != "|").ToList();
                                            if (lstStrings.Count > 2)
                                            {
                                                TotalBilled = lstStrings[2];
                                                date.TotalBilled = TotalBilled;
                                                break;
                                            }
                                        }
                                    }
                                    
                                }
                                return Ok(date);
                                
                            }
                            
                        }

                        
                    }
                    else
                    {
                        return StatusCode((int)HttpStatusCode.NotFound, "No response content found/Bad request 2");
                    }
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.NotFound, "No response content found/Bad request 3");
                }



                _objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name + " GetDocumentContent - Response object of-" + RequestId, "", response.Content.Headers.ToString() + "-" + Message, _httpContextAccessor.HttpContext.User.Identity.Name);

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

            return Ok(StateCode);
        }
        class StartEndDate
        {
            public string startDate { get; set; }
            public string endDate { get; set; }
            public string TotalBilled { get; set; }
        }
        /// <summary>
        /// If file size is large (>28.6MB), use this method before calling CreateDocumentWithDirectClient to handle 401 unauthorized error
        /// </summary>
        /// <returns>true</returns>
        [HttpGet]
        public ActionResult GetAuthorizationForLargeUpload()
        {
            return Ok(true);
        }
        class Doc360JsonIntake
        {
            public string propName { get; set; }
            public string value { get; set; }
            public Boolean isDate { get; set; }
            public string value2 { get; set; }
            public Boolean isMultiple { get; set; }
        }
        public ActionResult GetDocumentsByListCriteria(string searchCriteria, string docclassTypeName = "u_clinical_docs")
        {
            DOC360Methods objDOC360Methods = new DOC360Methods(_memoryCacheHelper, _objConfiguration, _objBOCommon);
            DOECAAResponse objDOECAAResponse = null;
            String Message = string.Empty;
            source = BOCommon.GetRefererURI(Request);

            try
            {
                List<Doc360JsonIntake> objJsonInfoValues = JsonConvert.DeserializeObject<List<Doc360JsonIntake>>(searchCriteria);
                // var Jsondata = JsonConvert.DeserializeObject<Dictionary<string, string>>(searchCriteria);
                string RequestId = Guid.NewGuid().ToString();
                _objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name + " GetDocumentFromObjectId - Request object of-" + RequestId, "", BOCommon.JsonConvertObjectToString(new { name = "propName", valueInput = "value" }), _httpContextAccessor.HttpContext.User.Identity.Name);

                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                DOC360Request objDocsRequest = new DOC360Request();
                Criteria objCriteria = new Criteria();


                objDocsRequest.indexName = docclassTypeName;
                List<FilterClaus> lstfilterClauses = new List<FilterClaus>();
                for (int i = 0; i < objJsonInfoValues.Count; i++)
                {
                    if (objJsonInfoValues[i].value != null && objJsonInfoValues[i].value != "" && objJsonInfoValues[i].isDate != true)
                    {
                        FilterClaus objFilterClaus = new FilterClaus();
                        objFilterClaus.name = objJsonInfoValues[i].propName;
                        objFilterClaus.value = objJsonInfoValues[i].value;
                        if (objJsonInfoValues[i].isMultiple == true)
                        {
                            objFilterClaus.type = "in";
                        }
                        else
                        {
                            objFilterClaus.type = "equal";
                        }
                        lstfilterClauses.Add(objFilterClaus);
                    }
                    if (objJsonInfoValues[i].value != null && objJsonInfoValues[i].value != "" && objJsonInfoValues[i].isDate == true)
                    {
                        FilterClaus objFilterClaus = new FilterClaus();
                        objFilterClaus.name = objJsonInfoValues[i].propName;
                        objFilterClaus.range = new ANGDDEAPIDO.Range();
                        try
                        {
                            DateTime dateTime;
                            dateTime = Convert.ToDateTime(objJsonInfoValues[i].value).ToUniversalTime();
                            dateTime = Convert.ToDateTime(objJsonInfoValues[i].value, CultureInfo.InvariantCulture).ToUniversalTime();
                            objFilterClaus.range.gte = dateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
                            dateTime = Convert.ToDateTime(objJsonInfoValues[i].value2).ToUniversalTime();
                            dateTime = Convert.ToDateTime(objJsonInfoValues[i].value2, CultureInfo.InvariantCulture).ToUniversalTime();
                            objFilterClaus.range.lte = dateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
                        }
                        catch (Exception)
                        {

                        }
                        objFilterClaus.type = "range";

                        lstfilterClauses.Add(objFilterClaus);
                    }
                }

                objCriteria.filterClauses = lstfilterClauses;
                objDocsRequest.criteria = objCriteria;

                EnsureLookupConfig();
                ExceptionTypes exceptionResult;
                if (_EnableHCPUpload == "1")
                {
                    // HCP — primary endpoint
                    exceptionResult = _objDOC360HCPMethods.GetDocuments(objDocsRequest, out objDOECAAResponse, out Message);                                  
                }
                else
                {
                    // Mode 0 (default) — legacy Stargate
                    exceptionResult = objDOC360Methods.GetDocuments(objDocsRequest, out objDOECAAResponse, out Message, true);
                }

                _objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name + " GetDocumentFromObjectId - Response object of-" + RequestId, "", exceptionResult.ToString() + "-" + Message, _httpContextAccessor.HttpContext.User.Identity.Name);

                if (exceptionResult == ExceptionTypes.Success)
                {
                    return Ok(objDOECAAResponse);
                }
                else
                {
                    return Ok(Message);
                }
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
        }
        public ActionResult GetDocumentsForPRA(string searchCriteria, string docClassCategoryId, string fedSearchCategoryId, string lineOfBusinessId)
        {
            DOC360Methods objDOC360Methods = new DOC360Methods(_memoryCacheHelper, _objConfiguration, _objBOCommon);
            DOFedSearch objDOECAAResponse = null;
            String Message = string.Empty;
            source = BOCommon.GetRefererURI(Request);
            try
            {
                List<Doc360JsonIntake> objJsonInfoValues = JsonConvert.DeserializeObject<List<Doc360JsonIntake>>(searchCriteria);
                string RequestId = Guid.NewGuid().ToString();
                _objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name + " GetDocumentForPRA - Request object of-" + RequestId, "", BOCommon.JsonConvertObjectToString(new { name = "propName", valueInput = "value" }), _httpContextAccessor.HttpContext.User.Identity.Name);

                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                ProviderFedSearch objDocsRequest = new ProviderFedSearch();
                Criteria2 objCriteria = new Criteria2();
                //FilterClaus objFilterClaus = new FilterClaus();

                objDocsRequest.documentCategoryId = docClassCategoryId;
                objDocsRequest.fedratedSearchCategoryId = fedSearchCategoryId;
                objDocsRequest.lineOfBusinessId = lineOfBusinessId;
                List<FilterClaus> lstfilterClauses = new List<FilterClaus>();
                for (int i = 0; i < objJsonInfoValues.Count; i++)
                {
                    if (objJsonInfoValues[i].value != null && objJsonInfoValues[i].value != "")
                    {
                        FilterClaus objFilterClaus = new FilterClaus();
                        objFilterClaus.name = objJsonInfoValues[i].propName;
                        objFilterClaus.value = objJsonInfoValues[i].value;
                        objFilterClaus.type = "equal";

                        lstfilterClauses.Add(objFilterClaus);
                    }

                }

                objCriteria.clause = lstfilterClauses;
                objDocsRequest.criteria = objCriteria;

                ExceptionTypes exceptionResult = objDOC360Methods.GetProviderListFedSearch(objDocsRequest, out objDOECAAResponse, out Message);
                _objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name + " GetDocumentFromObjectId - Response object of-" + RequestId, "", exceptionResult.ToString() + "-" + Message, _httpContextAccessor.HttpContext.User.Identity.Name);

                if (exceptionResult == ExceptionTypes.Success)
                {
                    return Ok(objDOECAAResponse);
                }
                else
                {
                    return Ok(Message);
                }
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
        }

        public ActionResult GetContentStreamTiff(string gbldocid, string docclassTypeName = "u_clinical_docs", bool pointToProd = false, bool SendLength = false, int currentPicCount = 0, bool IsEnIPointToProd = false)
        {
            DOC360Methods objDOC360Methods = new DOC360Methods(_memoryCacheHelper, _objConfiguration, _objBOCommon);
            DOECAAResponse objDOECAAResponse = null;
            String Message = string.Empty;
            source = BOCommon.GetRefererURI(Request);
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            string contentType;
            byte[] docRes = null;
            var fileName = "doc360File";
            List<ActionResult> tiffImages = new List<ActionResult>();
            string jsonString = "";
            try
            {
                string RequestId = Guid.NewGuid().ToString();
                _objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name + " GetDocumentContent - Request object of-" + RequestId, "", BOCommon.JsonConvertObjectToString(new { searchId = gbldocid }), _httpContextAccessor.HttpContext.User.Identity.Name);

                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                DOC360Request objDocsRequest = new DOC360Request();
                Criteria objCriteria = new Criteria();
                FilterClaus objFilterClaus = new FilterClaus();

                objDocsRequest.typeName = docclassTypeName;
                objFilterClaus.name = "u_gbl_doc_id";
                objFilterClaus.value = gbldocid.Split("|")[0];// "U0291244003";
                objFilterClaus.type = "equal";
                List<FilterClaus> lstfilterClauses = new List<FilterClaus>();
                lstfilterClauses.Add(objFilterClaus);
                objCriteria.filterClauses = lstfilterClauses;
                objDocsRequest.criteria = objCriteria;

                EnsureLookupConfig();
                if (_EnableHCPUpload == "1")
                {
                    // HCP — primary endpoint
                    response = _objDOC360HCPMethods.GetDocumentContentWithHcp(objDocsRequest, out objDOECAAResponse, out Message, pointToProd, IsEnIPointToProd);
                }
                else
                {
                    // Mode 0 (default) — legacy Stargate
                    response = objDOC360Methods.GetDocumentContent(objDocsRequest, out objDOECAAResponse, out Message, pointToProd, IsEnIPointToProd);
                }       
                if (response != null && response.Content != null)
                {
                    docRes = response.Content.ReadAsByteArrayAsync().Result;
                    if (docRes == null || docRes.Length == 0)
                    {
                        return StatusCode((int)HttpStatusCode.NotFound, "Document content is empty or invalid");
                    }
                    using (MemoryStream ms = new MemoryStream(docRes))
                    {
                        Image img = Image.FromStream(ms);
                        int frameCount = img.GetFrameCount(FrameDimension.Page);
                        for (int i = 0; i < frameCount; i++)
                        {
                            img.SelectActiveFrame(FrameDimension.Page, i);

                            using (MemoryStream ms1 = new MemoryStream())
                            {
                                img.Save(ms1, ImageFormat.Png);
                                tiffImages.Add(File(ms1.ToArray(), "image/png"));
                                if (!SendLength && currentPicCount == i)
                                {
                                    return File(ms1.ToArray(), "image/png");
                                }

                            }
                        }
                    }
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.NotFound, "No response content found/Bad request");
                }


                _objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name + " GetDocumentContent - Response object of-" + RequestId, "", response.Content.Headers.ToString() + "-" + Message, _httpContextAccessor.HttpContext.User.Identity.Name);

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
            return Ok(tiffImages.Count());
        }

        public ActionResult GetCPMLContentStreamTiff(string gbldocid, string docclassTypeName = "u_clinical_docs", bool pointToProd = false, int startPage = 0, int endPage = 0, bool IsCnSdoc360ProdValue=false)
        {
            DOC360Methods objDOC360Methods = new DOC360Methods(_memoryCacheHelper, _objConfiguration, _objBOCommon);
            DOECAAResponse objDOECAAResponse = null;
            String Message = string.Empty;
            source = BOCommon.GetRefererURI(Request);
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            byte[] docRes = null;
            List<FileContentResult> tiffImages = new List<FileContentResult>();
            try
            {
                string RequestId = Guid.NewGuid().ToString();
                _objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name + " GetDocumentContent - Request object of-" + RequestId, "", BOCommon.JsonConvertObjectToString(new { searchId = gbldocid }), _httpContextAccessor.HttpContext.User.Identity.Name);

                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                DOC360Request objDocsRequest = new DOC360Request();
                Criteria objCriteria = new Criteria();
                FilterClaus objFilterClaus = new FilterClaus();

                objDocsRequest.typeName = docclassTypeName;
                objFilterClaus.name = "u_gbl_doc_id";
                objFilterClaus.value = gbldocid.Split("|")[0];// "U0291244003";
                objFilterClaus.type = "equal";
                List<FilterClaus> lstfilterClauses = new List<FilterClaus>();
                lstfilterClauses.Add(objFilterClaus);
                objCriteria.filterClauses = lstfilterClauses;
                objDocsRequest.criteria = objCriteria;
                objDocsRequest.CPMLDoc360Request.IsCPMLCase = true;
                startPage--;

                EnsureLookupConfig();
                if (_EnableHCPUpload == "1")
                {
                    // HCP — primary endpoint
                    response = _objDOC360HCPMethods.GetDocumentContentWithHcp(objDocsRequest, out objDOECAAResponse, out Message, pointToProd, IsCnSdoc360ProdValue: IsCnSdoc360ProdValue);
                }
                else
                {
                    // Mode 0 (default) — legacy Stargate
                    response = objDOC360Methods.GetDocumentContent(objDocsRequest, out objDOECAAResponse, out Message, pointToProd, IsCnSdoc360ProdValue: IsCnSdoc360ProdValue);
                }
               
                if (response != null && response.Content != null)
                {
                    docRes = response.Content.ReadAsByteArrayAsync().Result;
                    if (docRes == null || docRes.Length == 0)
                    {
                        return StatusCode((int)HttpStatusCode.BadRequest, "Document stream is empty or null.");
                    }

                    using (MemoryStream ms = new MemoryStream(docRes))
                    {
                        Image img = Image.FromStream(ms);
                        int frameCount = img.GetFrameCount(FrameDimension.Page);

                        for (int i = startPage; i < endPage; i++)
                        {
                            img.SelectActiveFrame(FrameDimension.Page, i);

                            using (MemoryStream ms1 = new MemoryStream())
                            {
                                img.Save(ms1, ImageFormat.Png);
                                tiffImages.Add(File(ms1.ToArray(), "image/png"));

                            }
                        }
                    }
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.NotFound, "No response content found/Bad request");
                }


                _objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name + " GetDocumentContent - Response object of-" + RequestId, "", response.Content.Headers.ToString() + "-" + Message, _httpContextAccessor.HttpContext.User.Identity.Name);

            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(source) && source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(LoggedInUserId, this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), ex.Message, ex.ToString());
                }
                else
                {
                    _objBOCommon.LogError(this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                }
                return StatusCode((int)HttpStatusCode.InternalServerError, ex);
            }

            return Ok(tiffImages);
        }

        public ActionResult GetCPMLContentStreamTiffTotalCount(DOC360Request objDocsRequest, bool pointToProd = true,bool IsCnSdoc360ProdValue=false)
        {
            DOC360Methods objDOC360Methods = new DOC360Methods(_memoryCacheHelper, _objConfiguration, _objBOCommon);
            DOECAAResponse objDOECAAResponse = null;
            String Message = string.Empty;
            source = BOCommon.GetRefererURI(Request);
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);

            byte[] docRes = null;

            int frameCount = 0;
            try
            {
                string RequestId = Guid.NewGuid().ToString();
                EnsureLookupConfig();
                if (_EnableHCPUpload == "1")
                {
                    // HCP — primary endpoint
                    response = _objDOC360HCPMethods.GetDocumentContentWithHcp(objDocsRequest, out objDOECAAResponse, out Message, pointToProd, IsCnSdoc360ProdValue: IsCnSdoc360ProdValue);
                }
                else
                {
                    // Mode 0 (default) — legacy Stargate
                    response = objDOC360Methods.GetDocumentContent(objDocsRequest, out objDOECAAResponse, out Message, pointToProd, IsCnSdoc360ProdValue: IsCnSdoc360ProdValue);
                }
           
                if (response != null && response.Content != null)
                {
                    docRes = response.Content.ReadAsByteArrayAsync().Result;
                    using (MemoryStream ms = new MemoryStream(docRes))
                    {
                        Image img = Image.FromStream(ms);
                        frameCount = img.GetFrameCount(FrameDimension.Page);
                    }
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.NotFound, "No response content found/Bad request");
                }


                _objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name + " GetDocumentContent - Response object of-" + RequestId, "", response.Content.Headers.ToString() + "-" + Message, _httpContextAccessor.HttpContext.User.Identity.Name);

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

            return Ok(frameCount);
        }

        static void CreateTextFile(string filePath, byte[] byteArray)
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                fileStream.Write(byteArray, 0, byteArray.Length);
            }
        }
        public ActionResult GetContentStreamByCriteria(string searchPropName, string searchval, string docclassTypeName = "u_clinical_docs", bool pointToProd = false)
        {
            DOC360Methods objDOC360Methods = new DOC360Methods(_memoryCacheHelper, _objConfiguration, _objBOCommon);
            DOECAAResponse objDOECAAResponse = null;
            String Message = string.Empty;
            source = BOCommon.GetRefererURI(Request);
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            string contentType;
            byte[] docRes = null;
            var fileName = "doc360File";
            try
            {
                string RequestId = Guid.NewGuid().ToString();
                _objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name + " GetContentStreamByCriteria - Request object of-" + RequestId, "", BOCommon.JsonConvertObjectToString(new { searchId = searchval, searchprop = searchPropName }), _httpContextAccessor.HttpContext.User.Identity.Name);

                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                DOC360Request objDocsRequest = new DOC360Request();
                Criteria objCriteria = new Criteria();
                FilterClaus objFilterClaus = new FilterClaus();

                objDocsRequest.typeName = docclassTypeName;
                objFilterClaus.name = searchPropName;
                objFilterClaus.value = searchval;// "U0291244003";
                objFilterClaus.type = "equal";
                List<FilterClaus> lstfilterClauses = new List<FilterClaus>();
                lstfilterClauses.Add(objFilterClaus);
                objCriteria.filterClauses = lstfilterClauses;
                objDocsRequest.criteria = objCriteria;
                EnsureLookupConfig();

                if (_EnableHCPUpload == "1")
                {
                    // HCP — primary endpoint
                    response = _objDOC360HCPMethods.GetDocumentContentWithHcp(objDocsRequest, out objDOECAAResponse, out Message, pointToProd);
                }
                else
                {
                    // Mode 0 (default) — legacy Stargate
                    response = objDOC360Methods.GetDocumentContent(objDocsRequest, out objDOECAAResponse, out Message, pointToProd);
                }

                if (response != null && response.Content != null)
                {
                    docRes = response.Content.ReadAsByteArrayAsync().Result;

                    ////Set the Response Content Length.
                    response.Content.Headers.ContentLength = Convert.ToInt64(docRes.Length);

                    string extension = objDOC360Methods.GetFileExtension(response.Content.Headers.ContentType.MediaType);

                    //response.Content.Headers.ContentDisposition.FileName = response.Content.Headers.ContentDisposition.FileName + extension;
                    if (response.Content.Headers != null && response.Content.Headers.ContentDisposition != null
                        && !string.IsNullOrEmpty(response.Content.Headers.ContentDisposition.FileName))
                    {
                        fileName = response.Content.Headers.ContentDisposition.FileName;
                        fileName = fileName.Replace("\"", "");
                    }

                    //new FileExtensionContentTypeProvider().TryGetContentType(response.Content.Headers.ContentDisposition.FileName, out contentType);
                    contentType = response.Content.Headers.ContentType.MediaType;
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.NotFound, "No response content found/Bad request");
                }


                _objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name + " GetDocumentContent - Response object of-" + RequestId, "", response.Content.Headers.ToString() + "-" + Message, _httpContextAccessor.HttpContext.User.Identity.Name);

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
            return File(docRes, contentType, fileName);
        }
        public ActionResult CopyDocumentFromClaimToClinical(string caseId, string propName, string value, string docclassTypeName = "u_clinical_docs")
        {
            var objDOC360Methods = new DOC360Methods(_memoryCacheHelper, _objConfiguration, _objBOCommon);
            var source = BOCommon.GetRefererURI(Request);
            var requestId = Guid.NewGuid().ToString();
            var username = _httpContextAccessor.HttpContext.User.Identity.Name;

            try
            {
                _objBOCommon.Trace($"DOC360Controller-{MethodBase.GetCurrentMethod().Name} - Start - Request object of-{requestId}", "", BOCommon.JsonConvertObjectToString(new { searchId = value }), username);

                var objDocsRequest = new DOC360Request
                {
                    indexName = docclassTypeName,
                    criteria = new Criteria
                    {
                        filterClauses = new List<FilterClaus>
                 {
                     new FilterClaus
                     {
                         name = propName,
                         value = value,
                         type = "equal"
                     }
                 }
                    }
                };

                if (objDOC360Methods.GetDocuments(objDocsRequest, out var objDOECAAResponse, out var message, false) == ExceptionTypes.Success && objDOECAAResponse.lstDOECAADocument.Any())
                {
                    var doc = objDOECAAResponse.lstDOECAADocument.First();
                    if (doc.GlobalDocId != null)
                    {
                        var doc360GlobalDocId = doc.GlobalDocId.Split('|')[0];
                        _objBOCommon.Trace($"DOC360Controller-{MethodBase.GetCurrentMethod().Name} - Before GetContentStream - {requestId}", "", BOCommon.JsonConvertObjectToString(new { doc360GlobalDocId }), username);

                        var result = GetContentStream(doc360GlobalDocId, docclassTypeName);
                        _objBOCommon.Trace($"DOC360Controller-{MethodBase.GetCurrentMethod().Name} - After GetContentStream - {requestId}", "", BOCommon.JsonConvertObjectToString(new { doc360GlobalDocId }), username);

                        if (result is FileContentResult fileResult)
                        {
                            var fileBytes = fileResult.FileContents;
                            var fileName = fileResult.FileDownloadName;
                            //doc.SecurityGroup = "OBH";
                            doc.CaseID = caseId;
                            doc.DocType = "INIT CORR";
                            doc.Title = doc.CaseID + " " + "INIT CORR";
                            doc.ContentStream = fileBytes;
                            doc.ContentStreamFileName = fileName;
                            doc.FileContentSize = fileBytes.Length;
                            _objBOCommon.Trace($"DOC360Controller-{MethodBase.GetCurrentMethod().Name} - Before CreateDocument - {requestId}", "", BOCommon.JsonConvertObjectToString(new { doc }), username);
                            var objDOC360Response = objDOC360Methods.CreateDocument(doc);
                            _objBOCommon.Trace($"DOC360Controller-{MethodBase.GetCurrentMethod().Name} - After CreateDocument - {requestId}", "", BOCommon.JsonConvertObjectToString(new { objDOC360Response }), username);

                            if (objDOC360Response != null && (objDOC360Response.globalDocId != null || objDOC360Response.global_doc_id != null))
                            {
                                doc.ContentStream = null;
                                doc.GlobalDocId = objDOC360Response.globalDocId ?? objDOC360Response.global_doc_id;
                                doc.CreationDate = DateTime.UtcNow;
                                _objBOCommon.Trace($"DOC360Controller-{MethodBase.GetCurrentMethod().Name} CreateDocument - Response Success - {requestId}", $"{fileName} - size-{doc.FileContentSize}", BOCommon.JsonConvertObjectToString(objDOC360Response), username);
                                return Ok(doc);
                            }
                            else
                            {
                                doc.ContentStream = null;
                                _objBOCommon.Trace($"DOC360Controller-{MethodBase.GetCurrentMethod().Name} CreateDocument - Failed - {requestId}", $"{fileName} - size-{doc.FileContentSize}", BOCommon.JsonConvertObjectToString(objDOC360Response), username);
                                return Ok(message);
                            }
                        }
                    }
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.NotFound, "No documents found");
                }
            }
            catch (Exception ex)
            {
                if (source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(LoggedInUserId, $"{GetType().Name}.{MethodBase.GetCurrentMethod().Name}", ex.Message, ex.ToString());
                }
                else
                {
                    _objBOCommon.LogError($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name}", source, 10001, ex.Message, " ", ex.StackTrace, username);
                }
                return StatusCode((int)HttpStatusCode.InternalServerError, ex);
            }
            return StatusCode((int)HttpStatusCode.NoContent, "No content available");
        }

        public ActionResult GetQueryByPropName(string propName, string value, string docclassTypeName = "u_clinical_docs")
        {
            DOC360Methods objDOC360Methods = new DOC360Methods(_memoryCacheHelper, _objConfiguration, _objBOCommon);
            DOECAAResponse objDOECAAResponse = null;
            String Message = string.Empty;
            source = BOCommon.GetRefererURI(Request);
            try
            {
                _objBOCommon.GetRRTGPSAPiConfig(out lstATSLookupMaster);

                string TotalRecords = "50";
                try
                {
                    string lookupValue = _objBOCommon.GetLookupValueBasedonCon(lstATSLookupMaster, (long)RRTGPSAPIConfig.TotalRecords, 0);
                    if (!string.IsNullOrEmpty(lookupValue))
                    {
                        TotalRecords = lookupValue;
                    }
                }
                catch (Exception)
                {
                    // Log the exception if necessary
                    // TotalRecords remains "50" as default
                }

                string RequestId = Guid.NewGuid().ToString();
                Console.WriteLine("Get Query entered - updated - " + RequestId);
                //_objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name + " GetDocuments - Request object of-" + RequestId, "", BOCommon.JsonConvertObjectToString(new { searchId = caseId }), _httpContextAccessor.HttpContext.User.Identity.Name);

                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                DOC360Request objDocsRequest = new DOC360Request();
                Criteria objCriteria = new Criteria();
                FilterClaus objFilterClaus = new FilterClaus();

                objDocsRequest.indexName = docclassTypeName;
                objDocsRequest.totalRecords = Convert.ToInt32(TotalRecords);
                objDocsRequest.scrollId = "";
                objFilterClaus.name = propName;
                objFilterClaus.value = value;// "U0291244003";
                objFilterClaus.type = "equal";
                List<FilterClaus> lstfilterClauses = new List<FilterClaus>();
                lstfilterClauses.Add(objFilterClaus);
                objCriteria.filterClauses = lstfilterClauses;
                objDocsRequest.criteria = objCriteria;
                EnsureLookupConfig();

                ExceptionTypes exceptionResult;
                if (_EnableHCPUpload == "1")
                {
                    // HCP — primary endpoint
                    exceptionResult = _objDOC360HCPMethods.GetDocuments(objDocsRequest, out objDOECAAResponse, out Message);
                }
                else
                {
                    // Mode 0 (default) — legacy Stargate
                    exceptionResult = objDOC360Methods.GetDocuments(objDocsRequest, out objDOECAAResponse, out Message);
                }
               
                Console.WriteLine("Get Query completed");
                if (exceptionResult == ExceptionTypes.Success)
                {
                    Console.WriteLine("Get Query Success");
                    return Ok(objDOECAAResponse);
                }
                else
                {
                    return Ok(Message);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Get Query Exception-" + ex.Message);
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
        }
        [HttpPost]
        public ActionResult MergeDocDocuments([FromBody] DOMergeDocuments objDOMergeDocuments, [FromQuery] string docclassType)
        {
            DOMergeDocResponse objDOMergeDocResponse = new DOMergeDocResponse();
            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "MergeDocDocuments", _username);

            try
            {
                MergeDocuemnts objMergeDocuemnts = new MergeDocuemnts(_memoryCacheHelper, _objConfiguration, _objBOCommon);
                bool IsModelValid = objMergeDocuemnts.CheckValidation(objDOMergeDocuments, docclassType, out objDOMergeDocResponse);
                if (IsModelValid)
                {
                    ExceptionTypes exceptionResult = objMergeDocuemnts.MergeDocDocuments(objDOMergeDocuments, docclassType, out objDOMergeDocResponse);
                    if (exceptionResult != ExceptionTypes.Success && exceptionResult != ExceptionTypes.ZeroRecords)
                    {
                        return BadRequest();
                    }
                }
                else
                {
                    return Ok(objDOMergeDocResponse);
                }
            }
            catch (Exception ex)
            {
                if (source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(0, location, ex.Message, ex.ToString());
                }
                else
                {
                    _objBOCommon.LogError(location, source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                }
                return BadRequest(ex.Message);
            }
            return Ok(objDOMergeDocResponse);
        }
    }
}
