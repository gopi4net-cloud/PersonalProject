using ANGDDEAPI.Common;
using ANGDDEAPIBO;
using ANGDDEAPIDO;
using ANGDDEAPIDO.Interface;
using ANGDDEAPIFoundation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;

namespace ANGDDEAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class DOC360DirectController : ControllerBase
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


        public DOC360DirectController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _memoryCacheHelper = memoryCacheHelper;
            _objBOCommon = bOCommon;
            _cache = cache;
        }

        /// <summary>
        /// Retrieves the content stream of a document based on its global document ID.
        /// </summary>
        /// <param name="gbldocid">The global document ID of the document to retrieve.</param>
        /// <param name="docclassTypeName">The document class type name (default: "u_obhats_clinical_docs").</param>
        /// <returns>A file result containing the document's content stream, content type, and file name.</returns>
        public ActionResult GetContentStream(string gbldocid, string docclassTypeName = "u_obhats_clinical_docs")
        {
            bool isDirect = _objConfiguration.AppSettings.Doc360IsDrictBH;
            bool isHCPEnabled = _objConfiguration.AppSettings.Doc360IsHCP;
            if (isHCPEnabled)
            {
                return GetContentStreamForHCP(gbldocid, docclassTypeName);
            }
            else
            {
                if (isDirect)
                {

                    return GetContentDirectStream(gbldocid, docclassTypeName);
                }
                else
                {
                    return GetContentStargateStream(gbldocid, docclassTypeName);
                }
            }
        }

        private ActionResult GetContentDirectStream(string gbldocid, string docclassTypeName)
        {
            DOC360DirectMethods objDOC360Methods = new DOC360DirectMethods(_memoryCacheHelper, _objConfiguration, _objBOCommon);
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

                string docId = gbldocid.Split("?")[0];
                response = objDOC360Methods.GetDirectDocuments(docId, out Message, docclassTypeName);
                if (response != null && response.Content != null)
                {
                    docRes = response.Content.ReadAsByteArrayAsync().Result;

                    ////Set the Response Content Length.
                    response.Content.Headers.ContentLength = Convert.ToInt64(docRes.Length);

                    //string extension = objDOC360Methods.GetFileExtension(response.Content.Headers.ContentType.MediaType);

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

        private ActionResult GetContentStargateStream(string gbldocid, string docclassTypeName = "u_obhats_clinical_docs", bool pointToProd = false, bool CPMLStartPageAndEndPageRange = false, int startPage = 0, int endPage = 0, bool IsCPMLDoc360 = false)
        {
            DOC360DirectMethods objDOC360Methods = new DOC360DirectMethods(_memoryCacheHelper, _objConfiguration, _objBOCommon);
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
                objFilterClaus.value = gbldocid.Split("|")[0];// "U0291244003";
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

                response = objDOC360Methods.GetDocumentContentWithStargate(objDocsRequest, out objDOECAAResponse, out Message);
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

        /// <summary>
        /// Uploads a document to DOC360 using multi-form data from the request.
        /// Handles metadata and file content.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <returns>A DOC360Response object containing the status and any error messages.</returns>
        [HttpPost]
        [RequestSizeLimit(2L * 1024 * 1024 * 1024)] // 2 GB  // 200 MB
        public DOC360Response CreateDocumentWithDirectClient()
        {
            bool isDirect = _objConfiguration.AppSettings.Doc360IsDrictBH;
            bool isHCPEnabled = _objConfiguration.AppSettings.Doc360IsHCP;
            if (isHCPEnabled)
            {
                return CreateDocumentForHCP(string.Empty);
            }
            else
            {
                if (isDirect)
                {
                    return UploadDocumentWithDirectClient(string.Empty);

                }
                else
                {
                    return CreateDocument(string.Empty);
                }
            }
        }
        [HttpPost]
        [RequestSizeLimit(2L * 1024 * 1024 * 1024)] // 2 GB 
        public DOC360Response CreateDocWithDirectClient(string Docclass)
        {
            bool isDirect = _objConfiguration.AppSettings.Doc360IsDrictBH;
            bool isHCPEnabled = _objConfiguration.AppSettings.Doc360IsHCP;
            if (isHCPEnabled)
            {
                return CreateDocumentForHCP(Docclass);
            }
            else
            {
                if (isDirect)
                {

                    return UploadDocumentWithDirectClient(Docclass);
                }
                else
                {
                    return CreateDocument(Docclass);
                }
            }


        }
        [RequestSizeLimit(2L * 1024 * 1024 * 1024)] // 2 GB 
        private DOC360Response UploadDocumentWithDirectClient(string Docclass)
        {
            DOC360DirectMethods objDOC360Methods = new DOC360DirectMethods(_memoryCacheHelper, _objConfiguration, _objBOCommon);
            var provider = new MultipartMemoryStreamProvider();
            EDMSServiceMethods objEDMSServiceMethods = new EDMSServiceMethods(_httpContextAccessor, _objConfiguration);
            DOC360Response objDOC360Response = new DOC360Response();
            objDOC360Response.messageMap = new messageMap();
            DOECAADocument objDOECAADocument = new DOECAADocument();
            string fileName = string.Empty;
            string DocClassname = Docclass;
            source = BOCommon.GetRefererURI(Request);
            byte[] fileBytes = null;
            try
            {
                string RequestId = Guid.NewGuid().ToString();
                _objBOCommon.Trace("DOC360DirectController-" + MethodBase.GetCurrentMethod().Name + " CreateDocument - Request object of-" + RequestId, "", BOCommon.JsonConvertObjectToString(new { searchId = Request }), _httpContextAccessor.HttpContext.User.Identity.Name);

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
                objDOC360Response = objDOC360Methods.UploadDocumentDirectClient(objDOECAADocument, DocClassname);

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
        [HttpPost]
        [RequestSizeLimit(2L * 1024 * 1024 * 1024)] // 2 GB 
        public DOC360Response CreateDocument(string Docclass)
        {
            string RequestId = "";
            long fileSize = 0;
            string requestSource = "";
            DOC360DirectMethods objDOC360Methods = new DOC360DirectMethods(_memoryCacheHelper, _objConfiguration, _objBOCommon);
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
                string FileSizeLimit = _objBOCommon.GetLookupValueBasedonCon(lstATSLookupMaster, (long)RRTGPSAPIConfig.FileSizeLimit, 0);
                string Toenablenewstargate = _objBOCommon.GetLookupValueBasedonCon(lstATSLookupMaster, (long)RRTGPSAPIConfig.ToEnableNewStarGate, 0);
                RequestId = Guid.NewGuid().ToString();

                _objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name + " CreateDocument - Request object of - " + RequestId, "DOC360Controller-request", requestSource, _httpContextAccessor.HttpContext.User.Identity.Name);
                if (string.IsNullOrEmpty(Docclass))
                {
                    Docclass = "u_obhats_clinical_docs";
                }

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
                objDOECAADocument.DocClassToUpload = Docclass;
                fileSize = fileBytes.Length;
                if (fileBytes.Length > (Convert.ToInt64(FileSizeLimit) * 2))
                {

                    _objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name + " CreateDocumentWithStargate - Request object of-" + RequestId, fileSize.ToString(), source, _httpContextAccessor.HttpContext.User.Identity.Name);
                    objDOC360Response = objDOC360Methods.CreateDocumentWithStargate(objDOECAADocument);

                }
                else
                {
                    objDOC360Response = objDOC360Methods.CreateDocument(objDOECAADocument);
                }

                if (objDOC360Response != null && (objDOC360Response.globalDocId != null || objDOC360Response.global_doc_id != null))
                {
                    _objBOCommon.Trace("DOC360DirectController-" + MethodBase.GetCurrentMethod().Name + " CreateDocument - Response Success - " + RequestId, fileName + " - size-" + fileSize.ToString(), BOCommon.JsonConvertObjectToString(objDOC360Response), _httpContextAccessor.HttpContext.User.Identity.Name);
                }
                else
                {
                    _objBOCommon.Trace("DOC360DirectController-" + MethodBase.GetCurrentMethod().Name + " CreateDocument - Failed - " + RequestId, fileName + " - size-" + fileSize.ToString(), BOCommon.JsonConvertObjectToString(objDOC360Response), _httpContextAccessor.HttpContext.User.Identity.Name);
                }
            }
            catch (Exception ex)
            {
                _objBOCommon.Trace("DOC360DirectController-" + MethodBase.GetCurrentMethod().Name + " Exception - " + RequestId, fileName + " - size- " + fileSize.ToString(), ex.Message, _httpContextAccessor.HttpContext.User.Identity.Name);

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
        /// Updates a single document in DOC360 based on the provided attributes and global document ID.
        /// </summary>
        /// <param name="listDoc360Request">A list of attributes to update in the document.</param>
        /// <param name="GlobalDocId">The global document ID of the document to update.</param>
        /// <param name="docclassTypeName">The document class type name (default: "u_obhats_clinical_docs").</param>
        /// <returns>A DOC360Response object containing the status and any error messages.</returns>
        [HttpPost]
        public DOC360Response UpdateSingleDocumentWithDirectClient(List<AttributeList> listDoc360Request, string GlobalDocId, string docclassTypeName = "u_obhats_clinical_docs")
        {
            bool isDirect = _objConfiguration.AppSettings.Doc360IsDrictBH;
            bool isHCPEnabled = _objConfiguration.AppSettings.Doc360IsHCP;
            if (isHCPEnabled)
            {
                return UpdateSingleDocumentWithHCP(listDoc360Request, GlobalDocId, docclassTypeName);
            }
            else
            {
                if (isDirect)
                {

                    return UpdateSingleDocumentWithDirect(listDoc360Request, GlobalDocId, docclassTypeName);
                }
                else
                {
                    return UpdateSingleDocumentWithStargate(listDoc360Request, GlobalDocId, docclassTypeName);
                }
            }
        }
        [HttpPost]
        public DOC360Response UpdateSingleDocumentWithDirect(List<AttributeList> listDoc360Request, string GlobalDocId, string docclassTypeName = "u_obhats_clinical_docs")
        {
            DOC360DirectMethods objDOC360Methods = new DOC360DirectMethods(_memoryCacheHelper, _objConfiguration, _objBOCommon);
            var provider = new MultipartMemoryStreamProvider();
            EDMSServiceMethods objEDMSServiceMethods = new EDMSServiceMethods(_httpContextAccessor, _objConfiguration);
            DOC360Response objDOC360Response = new DOC360Response();
            objDOC360Response.messageMap = new messageMap();
            DOECAADocument objDOECAADocument = new DOECAADocument();

            string fileName = string.Empty;
            //string DocClassname = string.Empty;
            source = BOCommon.GetRefererURI(Request);

            try
            {
                if (string.IsNullOrEmpty(docclassTypeName))
                {
                    docclassTypeName = "u_obhats_clinical_docs";
                }
                string RequestId = Guid.NewGuid().ToString();
                _objBOCommon.Trace("DOC360DirectController-" + MethodBase.GetCurrentMethod().Name + " CreateDocument - Request object of-" + RequestId, "", BOCommon.JsonConvertObjectToString(new { searchId = Request }), _httpContextAccessor.HttpContext.User.Identity.Name);


                //objDOECAADocument = JsonConvert.DeserializeObject<List<Doc360InputRequest>>(doc360DirectRequests.);

                objDOC360Response = objDOC360Methods.UpdateDocumentDirectClient(listDoc360Request, GlobalDocId, docclassTypeName);

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
        /// Updates multiple documents in DOC360 in bulk based on the provided requests.
        /// </summary>
        /// <param name="listDoc360Request">A list of document update requests.</param>
        /// <param name="GlobalDocId">The global document ID of the documents to update.</param>
        /// <param name="docclassTypeName">The document class type name (default: "u_obhats_clinical_docs").</param>
        /// <returns>A DOC360Response object containing the status and any error messages.</returns>
        [HttpPost]
        public ActionResult UpdateBulkDocumentWithDirectClient(List<Doc360DirectRequest> listDoc360Request, string GlobalDocId, string docclassTypeName = "u_obhats_clinical_docs")
        {
            bool isDirect = _objConfiguration.AppSettings.Doc360IsDrictBH;
            bool isHCPEnabled = _objConfiguration.AppSettings.Doc360IsHCP;
            if (isHCPEnabled)
            {
                return UpdateBulkDocumentWithHCP(listDoc360Request, GlobalDocId, docclassTypeName);
            }
            else
            {
                if (isDirect)
                {

                    return UpdateBulkDocumentWithDirect(listDoc360Request, GlobalDocId, docclassTypeName);
                }
                else
                {
                    return UpdateBulkDocumentWithStargate(listDoc360Request, GlobalDocId, docclassTypeName);
                }
            }
        }
        [HttpPost]
        public ActionResult UpdateBulkDocumentWithDirect(List<Doc360DirectRequest> listDoc360Request, string GlobalDocId, string docclassTypeName = "u_obhats_clinical_docs")
        {
            DOC360DirectMethods objDOC360Methods = new DOC360DirectMethods(_memoryCacheHelper, _objConfiguration, _objBOCommon);
            var provider = new MultipartMemoryStreamProvider();
            EDMSServiceMethods objEDMSServiceMethods = new EDMSServiceMethods(_httpContextAccessor, _objConfiguration);
            List<Doc360BulkUpdateResponse> objDOC360Response = new List<Doc360BulkUpdateResponse>();
            //objDOC360Response.messageMap = new messageMap();
            DOECAADocument objDOECAADocument = new DOECAADocument();

            string fileName = string.Empty;
            string DocClassname = string.Empty;
            source = BOCommon.GetRefererURI(Request);

            try
            {
                string RequestId = Guid.NewGuid().ToString();
                _objBOCommon.Trace("DOC360DirectController-" + MethodBase.GetCurrentMethod().Name + " CreateDocument - Request object of-" + RequestId, "", BOCommon.JsonConvertObjectToString(new { searchId = Request }), _httpContextAccessor.HttpContext.User.Identity.Name);

                objDOC360Response = objDOC360Methods.UpdateBulkDocument(listDoc360Request, DocClassname);

                _objBOCommon.Trace("DOC360DirectController-" + MethodBase.GetCurrentMethod().Name + " CreateDocument - Response object of-" + RequestId, "", BOCommon.JsonConvertObjectToString(objDOC360Response), _httpContextAccessor.HttpContext.User.Identity.Name);
            }
            catch (Exception ex)
            {

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
        /// Deletes a document from DOC360 based on its global document ID and document class name.
        /// </summary>
        /// <param name="globalId">The global document ID of the document to delete.</param>
        /// <param name="docClassName">The document class name of the document to delete.</param>
        /// <returns>A DOC360DeleteResponse object containing the deletion status and any error messages.</returns>
        public DOC360DeleteResponse DeleteDocument(string globalId, string docClassName)
        {
            bool isDirect = _objConfiguration.AppSettings.Doc360IsDrictBH;
            bool isHCPEnabled = _objConfiguration.AppSettings.Doc360IsHCP;
            if (isHCPEnabled)
            {
                return DeleteDocumentWithHCP(globalId, docClassName);
            }
            else
            {
                if (isDirect)
                {

                    return DeleteDocumentWithDirect(globalId, docClassName);
                }
                else
                {
                    return DeleteDocumentWithStargate(globalId, docClassName);
                }
            }
        }
        public DOC360DeleteResponse DeleteDocumentWithDirect(string globalId, string docClassName)
        {
            DOC360DirectMethods objDOC360Methods = new DOC360DirectMethods(_memoryCacheHelper, _objConfiguration, _objBOCommon);
            DOC360DeleteResponse objDOECAAResponse = null;
            String Message = string.Empty;
            source = BOCommon.GetRefererURI(Request);
            try
            {
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Request", globalId, _username);

                objDOECAAResponse = objDOC360Methods.DeleteDocument(globalId, docClassName);

            }
            catch (Exception ex)
            {
                objDOECAAResponse.deletionStatusMessage = "Failed";

                if (source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(0, this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), ex.Message, ex.ToString());
                }
                else
                {
                    _objBOCommon.LogError(this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                }
            }
            _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Response", BOCommon.JsonConvertObjectToString(objDOECAAResponse), _username);

            return objDOECAAResponse;
        }

        /// <summary>
        /// Retrieves documents from DOC360 based on a case ID and document class type name.
        /// </summary>
        /// <param name="caseId">The case ID to search for documents.</param>
        /// <param name="docclassTypeName">The document class type name (default: "u_obhats_clinical_docs").</param>
        /// <returns>An ActionResult containing the retrieved documents or an error message.</returns>
        public ActionResult GetQuery(string caseId, string docclassTypeName = "u_obhats_clinical_docs")
        {
            bool isDirect = _objConfiguration.AppSettings.Doc360IsDrictBH;
            bool isHCPEnabled = _objConfiguration.AppSettings.Doc360IsHCP;
            if (isHCPEnabled)
            {
                return GetHCPQuery(caseId, docclassTypeName);
            }
            else
            {
                if (isDirect)
                {
                    return GetDirectQuery(caseId, docclassTypeName);
                }
                else
                {
                    return GetStargateQuery(caseId, docclassTypeName);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="docclassTypeName"></param>
        /// <returns></returns>
        private ActionResult GetDirectQuery(string caseId, string docclassTypeName)
        {
            DOC360DirectMethods objDOC360Methods = new DOC360DirectMethods(_memoryCacheHelper, _objConfiguration, _objBOCommon);
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

                ExceptionTypes exceptionResult = objDOC360Methods.GetDocuments(objDocsRequest, out objDOECAAResponse, out Message, docclassTypeName);
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


        public ActionResult GetStargateQuery(string caseId, string docclassTypeName = "u_clinical_docs")
        {
            DOC360DirectMethods objDOC360Methods = new DOC360DirectMethods(_memoryCacheHelper, _objConfiguration, _objBOCommon);
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

                ExceptionTypes exceptionResult = objDOC360Methods.GetDocumentsWithStartgate(objDocsRequest, out objDOECAAResponse, out Message);
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
        /// Copies a document from a claim to a clinical classification in DOC360.
        /// </summary>
        /// <param name="caseId">The case ID to associate with the copied document.</param>
        /// <param name="propName">The property name to filter documents.</param>
        /// <param name="value">The value of the property to filter documents.</param>
        /// <param name="docclassTypeName">The document class type name (default: "u_obhats_clinical_docs").</param>
        /// <returns>An ActionResult containing the copied document or an error message.</returns>
        public ActionResult CopyDocumentFromClaimToClinical(string caseId, string propName, string value, string keyAccountInd, string EmployeeRestrictInd, bool OnshoreInd, string docclassTypeName = "u_obhats_clinical_docs", string destinationDocClass = "u_obhats_clinical_docs")
        {
            bool isDirect = _objConfiguration.AppSettings.Doc360IsDrictBH;
            bool isHCPEnabled = _objConfiguration.AppSettings.Doc360IsHCP;
            if (isHCPEnabled)
            {
                return CopyDocumentFromClaimToClinicalWithHCP(caseId, propName, value, keyAccountInd, EmployeeRestrictInd, OnshoreInd, docclassTypeName, destinationDocClass);
            }
            else
            {
                if (isDirect)
                {
                    return CopyDocumentFromClaimToClinicalWithDirectClient(caseId, propName, value, keyAccountInd, EmployeeRestrictInd, OnshoreInd, docclassTypeName, destinationDocClass);

                }
                else
                {
                    return CopyDocumentFromClaimToClinicalWithStargate(caseId, propName, value, keyAccountInd, EmployeeRestrictInd, OnshoreInd, docclassTypeName, destinationDocClass);
                }
            }
        }
        public ActionResult CopyDocumentFromClaimToClinicalWithDirectClient(string caseId, string propName, string value, string keyAccountInd, string EmployeeRestrictInd, bool OnshoreInd, string sourceDocclassTypeName = "u_obhats_clinical_docs", string destinationDocClass = "u_obhats_clinical_docs")
        {
            var objDOC360Methods = new DOC360DirectMethods(_memoryCacheHelper, _objConfiguration, _objBOCommon);
            var source = BOCommon.GetRefererURI(Request);
            var requestId = Guid.NewGuid().ToString();
            var message = string.Empty;
            var username = _httpContextAccessor.HttpContext.User.Identity.Name;

            try
            {
                _objBOCommon.Trace($"DOC360DirectController-{MethodBase.GetCurrentMethod().Name} - Start - Request object of-{requestId}", "", BOCommon.JsonConvertObjectToString(new { searchId = value }), username);


                var objDocsRequest = new DOC360Request
                {
                    indexName = sourceDocclassTypeName,
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

                if (objDOC360Methods.GetDocuments(objDocsRequest, out var objDOECAAResponse, out message, sourceDocclassTypeName) == ExceptionTypes.Success && objDOECAAResponse.lstDOECAADocument.Any())
                {
                    var doc = objDOECAAResponse.lstDOECAADocument.First();
                    if (doc.GlobalDocId != null)
                    {
                        var doc360GlobalDocId = doc.GlobalDocId.Split('|')[0];
                        _objBOCommon.Trace($"DOC360DirectController-{MethodBase.GetCurrentMethod().Name} - Before GetContentStream - {requestId}", "", BOCommon.JsonConvertObjectToString(new { doc360GlobalDocId }), username);

                        var result = GetContentStream(doc360GlobalDocId, sourceDocclassTypeName);
                        _objBOCommon.Trace($"DOC360DirectController-{MethodBase.GetCurrentMethod().Name} - After GetContentStream - {requestId}", "", BOCommon.JsonConvertObjectToString(new { doc360GlobalDocId }), username);

                        if (result is FileContentResult fileResult)
                        {
                            var fileBytes = fileResult.FileContents;
                            var fileName = fileResult.FileDownloadName;
                            doc.CaseID = caseId;
                            doc.DocType = "INIT CORR";
                            doc.Title = doc.CaseID + " " + "INIT CORR";
                            doc.ContentStream = fileBytes;
                            doc.ContentStreamFileName = fileName;
                            doc.FileContentSize = fileBytes.Length;
                            doc.KeyAccountIndicator = keyAccountInd;
                            doc.EmployeeRestrictedInd = EmployeeRestrictInd;
                            doc.OnshoreIndicator = OnshoreInd;
                            doc.UploadedOn = DateTime.Now.ToString("MM-dd-yyyy");
                            string docclsTypeName = destinationDocClass;

                            _objBOCommon.Trace($"DOC360DirectController-{MethodBase.GetCurrentMethod().Name} - Before CreateDocument - {requestId}", "", BOCommon.JsonConvertObjectToString(new { doc }), username);
                            var objDOC360Response = objDOC360Methods.UploadDocumentDirectClient(doc, docclsTypeName);
                            _objBOCommon.Trace($"DOC360DirectController-{MethodBase.GetCurrentMethod().Name} - After CreateDocument - {requestId}", "", BOCommon.JsonConvertObjectToString(new { objDOC360Response }), username);

                            if (objDOC360Response != null && (objDOC360Response.globalDocId != null || objDOC360Response.global_doc_id != null))
                            {
                                doc.ContentStream = null;
                                doc.GlobalDocId = objDOC360Response.globalDocId ?? objDOC360Response.global_doc_id;
                                doc.CreationDate = DateTime.UtcNow;
                                _objBOCommon.Trace($"DOC360DirectController-{MethodBase.GetCurrentMethod().Name} CreateDocument - Response Success - {requestId}", $"{fileName} - size-{doc.FileContentSize}", BOCommon.JsonConvertObjectToString(objDOC360Response), username);
                                return Ok(doc);
                            }
                            else
                            {
                                doc.ContentStream = null;
                                _objBOCommon.Trace($"DOC360DirectController-{MethodBase.GetCurrentMethod().Name} CreateDocument - Failed - {requestId}", $"{fileName} - size-{doc.FileContentSize}", BOCommon.JsonConvertObjectToString(objDOC360Response), username);
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

        /// <summary>
        /// Copies document content from a source document and updates metadata
        /// </summary>
        /// <param name="sourceDocClass">Source document class from which to copy</param>
        /// <param name="sourceDocId">Source document ID to copy from</param>
        /// <param name="destinationDocClass">Destination document class to copy to</param>
        /// <param name="documentData">Document metadata for the new document</param>
        /// <returns>Response indicating success or failure of the operation</returns>
        [HttpPost]
        public DOC360Response CopyDocContentAndUpdateMeta(
       [FromQuery] string sourceDocClass,
       [FromQuery] string sourceDocId,
       [FromQuery] string destinationDocClass,
       [FromBody] DOECAADocument documentData)
        {
            bool isHCPEnabled = _objConfiguration.AppSettings.Doc360IsHCP;
            if (isHCPEnabled)
            {
                return CopyDocContentAndUpdateMetaWithHCP(sourceDocClass, sourceDocId, destinationDocClass, documentData);
            }
            else
            {
                return CopyDocContentAndUpdateMetaWithStargate(sourceDocClass, sourceDocId, destinationDocClass, documentData);

            }
        }
        

            [HttpPost]
        public DOC360Response CopyDocContentAndUpdateMetaWithStargate(
        [FromQuery] string sourceDocClass,
        [FromQuery] string sourceDocId,
        [FromQuery] string destinationDocClass,
        [FromBody] DOECAADocument documentData)
        {
            string RequestId = Guid.NewGuid().ToString();
            // Get username and source
            _username = _httpContextAccessor.HttpContext.User.Identity.Name;
            source = BOCommon.GetRefererURI(Request);

            var objDOC360Response = new DOC360Response
            {
                messageMap = new messageMap
                {
                    error = new List<string>()
                },
                Status = "400" // Default to bad request, will be updated on success
            };

            try
            {
                // Input validation
                var validationErrors = new List<string>();

                if (string.IsNullOrEmpty(sourceDocClass))
                {
                    validationErrors.Add("Source document class is required");
                }

                if (string.IsNullOrEmpty(sourceDocId))
                {
                    validationErrors.Add("Source document ID is required");
                }

                if (string.IsNullOrEmpty(destinationDocClass))
                {
                    validationErrors.Add("Destination document class is required");
                }

                if (documentData == null)
                {
                    validationErrors.Add("Document metadata is required");
                }
                else
                {
                    // Document data validations - only check if documentData is not null
                    if (string.IsNullOrEmpty(documentData.CaseID))
                    {
                        validationErrors.Add("Document CaseID is required");
                    }

                    if (string.IsNullOrEmpty(documentData.Title))
                    {
                        validationErrors.Add("Document title is required");
                    }
                }

                // If there are any validation errors, add them to the response and return
                if (validationErrors.Any())
                {
                    objDOC360Response.Status = "400";
                    objDOC360Response.messageMap.error.AddRange(validationErrors);
                    return objDOC360Response;
                }

                // Get username and source
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                source = BOCommon.GetRefererURI(Request);

                // Log request
                _objBOCommon.Trace(
                    $"DOC360DirectController-{MethodBase.GetCurrentMethod().Name} - Start - {RequestId}",
                    "",
                    BOCommon.JsonConvertObjectToString(new { sourceDocClass, sourceDocId, destinationDocClass }),
                    _username);

                // Prepare document data
                documentData.DocClassToUpload = destinationDocClass;
                documentData.SourceDocClass = sourceDocClass;
                documentData.SourceGlobalDocId = sourceDocId;
                documentData.CreatedBy = _username;
                documentData.SecurityGroup ??= "AG";

                // Create instances and process
                var objDOC360Methods = new DOC360DirectMethods(_memoryCacheHelper, _objConfiguration, _objBOCommon);
                objDOC360Response = objDOC360Methods.CopyDocContentAndUpdateMeta(documentData);

                // Handle response
                if (objDOC360Response != null &&
                    (objDOC360Response.globalDocId != null || objDOC360Response.global_doc_id != null))
                {
                    objDOC360Response.Status = "Success";
                    _objBOCommon.Trace(
                        $"DOC360DirectController-{MethodBase.GetCurrentMethod().Name} - Success - {RequestId}",
                        "",
                        BOCommon.JsonConvertObjectToString(objDOC360Response),
                        _username);
                }
                else
                {
                    objDOC360Response.Status = "500";
                    _objBOCommon.Trace(
                        $"DOC360DirectController-{MethodBase.GetCurrentMethod().Name} - Failed - {RequestId}",
                        "",
                        BOCommon.JsonConvertObjectToString(objDOC360Response),
                        _username);
                }
            }
            catch (Exception ex)
            {
                objDOC360Response.Status = "500";
                objDOC360Response.messageMap.error.Add($"Internal server error: {ex.Message}");

                _objBOCommon.Trace(
                    $"DOC360DirectController-{MethodBase.GetCurrentMethod().Name} - Exception - {RequestId}",
                    "",
                    ex.Message,
                    _username);

                _objBOCommon.LogError(
                    GetType().Name + MethodBase.GetCurrentMethod(),
                    source,
                    10001,
                    ex.Message,
                    " ",
                    ex.StackTrace.ToString(),
                    _username);
            }

            return objDOC360Response;
        }
        /// <summary>
        /// Bulk updates metadata for multiple documents in DOC360
        /// </summary>
        /// <param name="docClassName">The document class name (defaults to "u_obhats_clinical_docs" if not provided)</param>
        /// <param name="documentDataList">List of documents with updated metadata</param>
        /// <returns>ActionResult containing list of bulk update responses or error details</returns>
        [HttpPost]
        public ActionResult DocumentMetadataBulkUpdater([FromQuery] string docClassName, [FromBody] List<DOECAADocument> documentDataList)
        {
            bool isDirect = _objConfiguration.AppSettings.Doc360IsDrictBH;
            bool isHCPEnabled = _objConfiguration.AppSettings.Doc360IsHCP;
            if (isHCPEnabled)
            {
                return DocumentMetadataBulkUpdaterWithHCP(docClassName, documentDataList);
            }
            else
            {
                if (isDirect)
                {

                    return DocumentMetadataBulkUpdaterWithDirect(docClassName, documentDataList);
                }
                else
                {
                    return DocumentMetadataBulkUpdaterWithStargate(docClassName, documentDataList);
                }
            }
        }
        [HttpPost]
        public ActionResult DocumentMetadataBulkUpdaterWithDirect([FromQuery] string docClassName, [FromBody] List<DOECAADocument> documentDataList)
        {
            string RequestId = Guid.NewGuid().ToString();
            _username = _httpContextAccessor.HttpContext.User.Identity.Name;
            source = BOCommon.GetRefererURI(Request);
            List<Doc360BulkUpdateResponse> objDOC360Response = new List<Doc360BulkUpdateResponse>();

            try
            {
                // Validate input parameters
                if (documentDataList == null || !documentDataList.Any())
                {
                    _objBOCommon.Trace("DOC360DirectController-" + MethodBase.GetCurrentMethod().Name +
                        " DocumentMetadataBulkUpdater - Invalid Request - " + RequestId, "",
                        "Document data list is null or empty", _username);
                    return BadRequest("Document data list cannot be null or empty");
                }

                // Validate that all documents have GlobalDocId
                var documentsWithoutGlobalId = documentDataList
                    .Where(doc => string.IsNullOrEmpty(doc.GlobalDocId))
                    .ToList();

                if (documentsWithoutGlobalId.Any())
                {
                    var errorMessage = "All documents must have a GlobalDocId value. Missing GlobalDocId for documents at positions: " +
                        string.Join(", ", documentDataList.Select((doc, index) =>
                            string.IsNullOrEmpty(doc.GlobalDocId) ? (index + 1).ToString() : null)
                            .Where(pos => pos != null));

                    _objBOCommon.Trace("DOC360DirectController-" + MethodBase.GetCurrentMethod().Name +
                        " DocumentMetadataBulkUpdater - Invalid Request - " + RequestId, "",
                        errorMessage, _username);

                    return BadRequest(errorMessage);
                }

                // Set default document class if not provided
                if (string.IsNullOrEmpty(docClassName))
                {
                    docClassName = "u_obhats_clinical_docs";
                }

                DOC360DirectMethods objDOC360Methods = new DOC360DirectMethods(_memoryCacheHelper, _objConfiguration, _objBOCommon);

                // Log incoming request
                _objBOCommon.Trace("DOC360DirectController-" + MethodBase.GetCurrentMethod().Name +
                    " DocumentMetadataBulkUpdater - Request object of-" + RequestId, "",
                    BOCommon.JsonConvertObjectToString(new { docClassName, documentCount = documentDataList.Count }),
                    _username);

                // Call service method with the correct parameters
                objDOC360Response = objDOC360Methods.DocumentMetadataBulkUpdater(documentDataList, docClassName);

                // Check response
                if (objDOC360Response == null)
                {
                    _objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name +
                        " DocumentMetadataBulkUpdater - Null Response - " + RequestId, "",
                        "Service returned null response", _username);
                    return StatusCode(500, "Service returned null response");
                }

                // Log success response
                _objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name +
                    " DocumentMetadataBulkUpdater - Response object of-" + RequestId, "",
                    BOCommon.JsonConvertObjectToString(objDOC360Response), _username);

                return Ok(objDOC360Response);
            }
            catch (Exception ex)
            {
                // Log error
                _objBOCommon.LogError(
                    this.GetType().Name + MethodBase.GetCurrentMethod(),
                    source,
                    10001,
                    ex.Message,
                    " ",
                    ex.StackTrace.ToString(),
                    _username);

                // Log error trace
                _objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name +
                    " DocumentMetadataBulkUpdater - Exception - " + RequestId, "",
                    ex.ToString(), _username);

                return StatusCode(500, new { message = "An error occurred while processing the request", error = ex.Message });
            }
        }
        [HttpPut]
        public ActionResult DocumentMetadataBulkUpdaterWithStargate([FromQuery] string docClassName, [FromBody] List<DOECAADocument> documentDataList)
        {
            string RequestId = Guid.NewGuid().ToString();
            _username = _httpContextAccessor.HttpContext.User.Identity.Name;
            source = BOCommon.GetRefererURI(Request);
            List<Doc360BulkUpdateResponse> objDOC360Response = new List<Doc360BulkUpdateResponse>();

            try
            {
                // Validate input parameters
                if (documentDataList == null || !documentDataList.Any())
                {
                    _objBOCommon.Trace("DOC360DirectController-" + MethodBase.GetCurrentMethod().Name +
                        " DocumentMetadataBulkUpdater - Invalid Request - " + RequestId, "",
                        "Document data list is null or empty", _username);
                    return BadRequest("Document data list cannot be null or empty");
                }

                // Validate that all documents have GlobalDocId
                var documentsWithoutGlobalId = documentDataList
                    .Where(doc => string.IsNullOrEmpty(doc.GlobalDocId))
                    .ToList();

                if (documentsWithoutGlobalId.Any())
                {
                    var errorMessage = "All documents must have a GlobalDocId value. Missing GlobalDocId for documents at positions: " +
                        string.Join(", ", documentDataList.Select((doc, index) =>
                            string.IsNullOrEmpty(doc.GlobalDocId) ? (index + 1).ToString() : null)
                            .Where(pos => pos != null));

                    _objBOCommon.Trace("DOC360DirectController-" + MethodBase.GetCurrentMethod().Name +
                        " DocumentMetadataBulkUpdater - Invalid Request - " + RequestId, "",
                        errorMessage, _username);

                    return BadRequest(errorMessage);
                }

                // Set default document class if not provided
                if (string.IsNullOrEmpty(docClassName))
                {
                    docClassName = "u_obhats_clinical_docs";
                }

                DOC360DirectMethods objDOC360Methods = new DOC360DirectMethods(_memoryCacheHelper, _objConfiguration, _objBOCommon);

                // Log incoming request
                _objBOCommon.Trace("DOC360DirectController-" + MethodBase.GetCurrentMethod().Name +
                    " DocumentMetadataBulkUpdater - Request object of-" + RequestId, "",
                    BOCommon.JsonConvertObjectToString(new { docClassName, documentCount = documentDataList.Count }),
                    _username);

                // Call service method with the correct parameters
                objDOC360Response = objDOC360Methods.DocumentMetadataBulkUpdaterWithStargate(documentDataList, docClassName);

                // Check response
                if (objDOC360Response == null)
                {
                    _objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name +
                        " DocumentMetadataBulkUpdater - Null Response - " + RequestId, "",
                        "Service returned null response", _username);
                    return StatusCode(500, "Service returned null response");
                }

                // Log success response
                _objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name +
                    " DocumentMetadataBulkUpdater - Response object of-" + RequestId, "",
                    BOCommon.JsonConvertObjectToString(objDOC360Response), _username);

                return Ok(objDOC360Response);
            }
            catch (Exception ex)
            {
                // Log error
                _objBOCommon.LogError(
                    this.GetType().Name + MethodBase.GetCurrentMethod(),
                    source,
                    10001,
                    ex.Message,
                    " ",
                    ex.StackTrace.ToString(),
                    _username);

                // Log error trace
                _objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name +
                    " DocumentMetadataBulkUpdater - Exception - " + RequestId, "",
                    ex.ToString(), _username);

                return StatusCode(500, new { message = "An error occurred while processing the request", error = ex.Message });
            }
        }

        [HttpPut]
        public DOC360Response UpdateSingleDocumentWithStargate(List<AttributeList> listDoc360Request, string GlobalDocId, string docclassTypeName = "u_obhats_clinical_docs")
        {
            DOC360DirectMethods objDOC360Methods = new DOC360DirectMethods(_memoryCacheHelper, _objConfiguration, _objBOCommon);
            var provider = new MultipartMemoryStreamProvider();
            EDMSServiceMethods objEDMSServiceMethods = new EDMSServiceMethods(_httpContextAccessor, _objConfiguration);
            DOC360Response objDOC360Response = new DOC360Response();
            objDOC360Response.messageMap = new messageMap();
            DOECAADocument objDOECAADocument = new DOECAADocument();

            string fileName = string.Empty;
            source = BOCommon.GetRefererURI(Request);

            try
            {
                if (string.IsNullOrEmpty(docclassTypeName))
                {
                    docclassTypeName = "u_obhats_clinical_docs";
                }
                string RequestId = Guid.NewGuid().ToString();
                _objBOCommon.Trace("DOC360DirectController-" + MethodBase.GetCurrentMethod().Name + " CreateDocument - Request object of-" + RequestId, "", BOCommon.JsonConvertObjectToString(new { searchId = Request }), _httpContextAccessor.HttpContext.User.Identity.Name);

                objDOC360Response = objDOC360Methods.UpdateSingleDocumentWithStargate(listDoc360Request, GlobalDocId, docclassTypeName);

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

        [HttpPut]
        public ActionResult UpdateBulkDocumentWithStargate(List<Doc360DirectRequest> listDoc360Request, string GlobalDocId, string docclassTypeName = "u_obhats_clinical_docs")
        {
            DOC360DirectMethods objDOC360Methods = new DOC360DirectMethods(_memoryCacheHelper, _objConfiguration, _objBOCommon);
            var provider = new MultipartMemoryStreamProvider();
            EDMSServiceMethods objEDMSServiceMethods = new EDMSServiceMethods(_httpContextAccessor, _objConfiguration);
            List<Doc360BulkUpdateResponse> objDOC360Response = new List<Doc360BulkUpdateResponse>();
            DOECAADocument objDOECAADocument = new DOECAADocument();

            string fileName = string.Empty;
            string DocClassname = string.Empty;
            source = BOCommon.GetRefererURI(Request);

            try
            {
                string RequestId = Guid.NewGuid().ToString();
                _objBOCommon.Trace("DOC360DirectController-" + MethodBase.GetCurrentMethod().Name + " CreateDocument - Request object of-" + RequestId, "", BOCommon.JsonConvertObjectToString(new { searchId = Request }), _httpContextAccessor.HttpContext.User.Identity.Name);

                objDOC360Response = objDOC360Methods.UpdateBulkDocumentWithStargate(listDoc360Request);

                _objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name + " CreateDocument - Response object of-" + RequestId, "", BOCommon.JsonConvertObjectToString(objDOC360Response), _httpContextAccessor.HttpContext.User.Identity.Name);
            }
            catch (Exception ex)
            {

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
        public ActionResult CopyDocumentFromClaimToClinicalWithStargate(string caseId, string propName, string value, string keyAccountInd, string EmployeeRestrictInd, bool OnshoreInd, string sourceDocclassTypeName = "u_obhats_clinical_docs", string destinationDocClass = "u_obhats_clinical_docs")
        {
            DOC360Response objDOC360Response = new DOC360Response();
            objDOC360Response.messageMap = new messageMap();
            var objDOC360Methods = new DOC360DirectMethods(_memoryCacheHelper, _objConfiguration, _objBOCommon);
            var source = BOCommon.GetRefererURI(Request);
            var requestId = Guid.NewGuid().ToString();
            var message = string.Empty;
            var username = _httpContextAccessor.HttpContext.User.Identity.Name;

            try
            {
                _objBOCommon.Trace($"DOC360DirectController-{MethodBase.GetCurrentMethod().Name} - Start - Request object of-{requestId}", "", BOCommon.JsonConvertObjectToString(new { searchId = value }), username);


                var objDocsRequest = new DOC360Request
                {
                    indexName = sourceDocclassTypeName,
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

                if (objDOC360Methods.GetDocumentsWithStartgate(objDocsRequest, out var objDOECAAResponse, out message) == ExceptionTypes.Success && objDOECAAResponse.lstDOECAADocument.Any())
                {
                    var doc = objDOECAAResponse.lstDOECAADocument.First();
                    if (doc.GlobalDocId != null)
                    {
                        var doc360GlobalDocId = doc.GlobalDocId.Split('|')[0];
                        _objBOCommon.Trace($"DOC360DirectController-{MethodBase.GetCurrentMethod().Name} - Before GetContentStream - {requestId}", "", BOCommon.JsonConvertObjectToString(new { doc360GlobalDocId }), username);

                        var result = GetContentStargateStream(doc360GlobalDocId, sourceDocclassTypeName);
                        _objBOCommon.Trace($"DOC360DirectController-{MethodBase.GetCurrentMethod().Name} - After GetContentStream - {requestId}", "", BOCommon.JsonConvertObjectToString(new { doc360GlobalDocId }), username);

                        _objBOCommon.GetRRTGPSAPiConfig(out lstATSLookupMaster);
                        string FileSizeLimit = _objBOCommon.GetLookupValueBasedonCon(lstATSLookupMaster, (long)RRTGPSAPIConfig.FileSizeLimit, 0);

                        if (result is FileContentResult fileResult)
                        {
                            var fileBytes = fileResult.FileContents;
                            var fileName = fileResult.FileDownloadName;
                            doc.CaseID = caseId;
                            doc.DocType = "INIT CORR";
                            doc.Title = doc.CaseID + " " + "INIT CORR";
                            doc.ContentStream = fileBytes;
                            doc.ContentStreamFileName = fileName;
                            doc.FileContentSize = fileBytes.Length;
                            doc.KeyAccountIndicator = keyAccountInd;
                            doc.EmployeeRestrictedInd = EmployeeRestrictInd;
                            doc.OnshoreIndicator = OnshoreInd;
                            doc.UploadedOn = DateTime.Now.ToString("MM-dd-yyyy");
                            doc.DocClassToUpload = destinationDocClass;

                            _objBOCommon.Trace($"DOC360DirectController-{MethodBase.GetCurrentMethod().Name} - Before CreateDocument - {requestId}", "", BOCommon.JsonConvertObjectToString(new { doc }), username);
                            if (fileBytes.Length > (Convert.ToInt64(FileSizeLimit) * 2))
                            {

                                _objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name + " CreateDocumentWithStargate - Request object of-" + requestId, fileBytes.Length.ToString(), source, _httpContextAccessor.HttpContext.User.Identity.Name);
                                objDOC360Response = objDOC360Methods.CreateDocumentWithStargate(doc);

                            }
                            else
                            {
                                objDOC360Response = objDOC360Methods.CreateDocument(doc);
                            }
                            _objBOCommon.Trace($"DOC360DirectController-{MethodBase.GetCurrentMethod().Name} - After CreateDocument - {requestId}", "", BOCommon.JsonConvertObjectToString(new { objDOC360Response }), username);

                            if (objDOC360Response != null && (objDOC360Response.globalDocId != null || objDOC360Response.global_doc_id != null))
                            {
                                doc.ContentStream = null;
                                doc.GlobalDocId = objDOC360Response.globalDocId ?? objDOC360Response.global_doc_id;
                                doc.CreationDate = DateTime.UtcNow;
                                _objBOCommon.Trace($"DOC360DirectController-{MethodBase.GetCurrentMethod().Name} CreateDocument - Response Success - {requestId}", $"{fileName} - size-{doc.FileContentSize}", BOCommon.JsonConvertObjectToString(objDOC360Response), username);
                                return Ok(doc);
                            }
                            else
                            {
                                doc.ContentStream = null;
                                _objBOCommon.Trace($"DOC360DirectController-{MethodBase.GetCurrentMethod().Name} CreateDocument - Failed - {requestId}", $"{fileName} - size-{doc.FileContentSize}", BOCommon.JsonConvertObjectToString(objDOC360Response), username);
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
        public DOC360DeleteResponse DeleteDocumentWithStargate(string globalId, string docClassName)
        {
            DOC360DirectMethods objDOC360DirectMethods = new DOC360DirectMethods(_memoryCacheHelper, _objConfiguration, _objBOCommon);
            DOC360DeleteResponse objDOECAAResponse = null;
            String Message = string.Empty;
            source = BOCommon.GetRefererURI(Request);
            try
            {
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Request", globalId, _username);
                if (string.IsNullOrEmpty(docClassName))
                {
                    docClassName = "u_obhats_clinical_docs";
                }
                objDOECAAResponse = objDOC360DirectMethods.DeleteDocumentWithStarGate(globalId, docClassName);

            }
            catch (Exception ex)
            {
                objDOECAAResponse.deletionStatusMessage = "Failed";

                if (source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(0, this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), ex.Message, ex.ToString());
                }
                else
                {
                    _objBOCommon.LogError(this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                }
            }
            _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Response", BOCommon.JsonConvertObjectToString(objDOECAAResponse), _username);

            return objDOECAAResponse;
        }

        #region [--DOC360 HCP--]
        private ActionResult GetContentStreamForHCP(string gbldocid, string docclassTypeName = "u_obhats_clinical_docs", bool pointToProd = false, bool CPMLStartPageAndEndPageRange = false, int startPage = 0, int endPage = 0, bool IsCPMLDoc360 = false)
        {
            DOC360DirectMethods objDOC360Methods = new DOC360DirectMethods(_memoryCacheHelper, _objConfiguration, _objBOCommon);
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
                objFilterClaus.value = gbldocid.Split("|")[0];// "U0291244003";
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

                response = objDOC360Methods.GetDocumentContentWithHCP(objDocsRequest, out objDOECAAResponse, out Message);
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

        [HttpPost]
        [RequestSizeLimit(2L * 1024 * 1024 * 1024)] // 2 GB 
        public DOC360Response CreateDocumentForHCP(string Docclass)
        {
            string RequestId = "";
            long fileSize = 0;
            string requestSource = "";
            DOC360DirectMethods objDOC360Methods = new DOC360DirectMethods(_memoryCacheHelper, _objConfiguration, _objBOCommon);
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
                string hcpLargeFileBoundaryStr = _objBOCommon.GetLookupValueBasedonCon(lstATSLookupMaster, (long)RRTGPSAPIConfig.HcpLargeFileBoundaryBytes, 0);
                long Doc360HCPLargeFileSizeLimit = long.TryParse(hcpLargeFileBoundaryStr, out long parsedBoundary) && parsedBoundary > 0
                    ? parsedBoundary : 99L * 1024L * 1024L; // default: 103,809,024 bytes = 99 MB
                RequestId = Guid.NewGuid().ToString();

                _objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name + " CreateDocument - Request object of - " + RequestId, "DOC360Controller-request", requestSource, _httpContextAccessor.HttpContext.User.Identity.Name);
                if (string.IsNullOrEmpty(Docclass))
                {
                    Docclass = "u_obhats_clinical_docs";
                }

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
                objDOECAADocument.DocClassToUpload = Docclass;
                fileSize = fileBytes.Length;
                if (fileBytes.Length > Doc360HCPLargeFileSizeLimit)
                {

                    _objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name + " CreateDocumentWithHCPLargeFile - Request object of-" + RequestId, fileSize.ToString(), source, _httpContextAccessor.HttpContext.User.Identity.Name);
                    objDOC360Response = objDOC360Methods.CreateDocumentWithHCPLargeFile(objDOECAADocument);

                }
                else
                {
                    objDOC360Response = objDOC360Methods.CreateDocumentWithHCP(objDOECAADocument);
                }

                if (objDOC360Response != null && (objDOC360Response.globalDocId != null || objDOC360Response.global_doc_id != null))
                {
                    _objBOCommon.Trace("DOC360DirectController-" + MethodBase.GetCurrentMethod().Name + " CreateDocument - Response Success - " + RequestId, fileName + " - size-" + fileSize.ToString(), BOCommon.JsonConvertObjectToString(objDOC360Response), _httpContextAccessor.HttpContext.User.Identity.Name);
                }
                else
                {
                    _objBOCommon.Trace("DOC360DirectController-" + MethodBase.GetCurrentMethod().Name + " CreateDocument - Failed - " + RequestId, fileName + " - size-" + fileSize.ToString(), BOCommon.JsonConvertObjectToString(objDOC360Response), _httpContextAccessor.HttpContext.User.Identity.Name);
                }
            }
            catch (Exception ex)
            {
                _objBOCommon.Trace("DOC360DirectController-" + MethodBase.GetCurrentMethod().Name + " Exception - " + RequestId, fileName + " - size- " + fileSize.ToString(), ex.Message, _httpContextAccessor.HttpContext.User.Identity.Name);

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

        public DOC360DeleteResponse DeleteDocumentWithHCP(string globalId, string docClassName)
        {
            DOC360DirectMethods objDOC360DirectMethods = new DOC360DirectMethods(_memoryCacheHelper, _objConfiguration, _objBOCommon);
            DOC360DeleteResponse objDOECAAResponse = null;
            String Message = string.Empty;
            source = BOCommon.GetRefererURI(Request);
            try
            {
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Request", globalId, _username);
                if (string.IsNullOrEmpty(docClassName))
                {
                    docClassName = "u_obhats_clinical_docs";
                }
                objDOECAAResponse = objDOC360DirectMethods.DeleteDocumentWithHCP(globalId, docClassName);

            }
            catch (Exception ex)
            {
                objDOECAAResponse.deletionStatusMessage = "Failed";

                if (source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(0, this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), ex.Message, ex.ToString());
                }
                else
                {
                    _objBOCommon.LogError(this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                }
            }
            _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Response", BOCommon.JsonConvertObjectToString(objDOECAAResponse), _username);

            return objDOECAAResponse;
        }

        public ActionResult GetHCPQuery(string caseId, string docclassTypeName = "u_clinical_docs")
        {
            DOC360DirectMethods objDOC360Methods = new DOC360DirectMethods(_memoryCacheHelper, _objConfiguration, _objBOCommon);
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
                objFilterClaus.value = caseId;
                objFilterClaus.type = "equal";
                List<FilterClaus> lstfilterClauses = new List<FilterClaus>();
                lstfilterClauses.Add(objFilterClaus);
                objCriteria.filterClauses = lstfilterClauses;
                objDocsRequest.criteria = objCriteria;

                ExceptionTypes exceptionResult = objDOC360Methods.GetDocumentsWithHCP(objDocsRequest, out objDOECAAResponse, out Message);
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
        public ActionResult CopyDocumentFromClaimToClinicalWithHCP(string caseId, string propName, string value, string keyAccountInd, string EmployeeRestrictInd, bool OnshoreInd, string sourceDocclassTypeName = "u_obhats_clinical_docs", string destinationDocClass = "u_obhats_clinical_docs")
        {
            DOC360Response objDOC360Response = new DOC360Response();
            objDOC360Response.messageMap = new messageMap();
            var objDOC360Methods = new DOC360DirectMethods(_memoryCacheHelper, _objConfiguration, _objBOCommon);
            var source = BOCommon.GetRefererURI(Request);
            var requestId = Guid.NewGuid().ToString();
            var message = string.Empty;
            var username = _httpContextAccessor.HttpContext.User.Identity.Name;

            try
            {
                _objBOCommon.Trace($"DOC360DirectController-{MethodBase.GetCurrentMethod().Name} - Start - Request object of-{requestId}", "", BOCommon.JsonConvertObjectToString(new { searchId = value }), username);


                var objDocsRequest = new DOC360Request
                {
                    indexName = sourceDocclassTypeName,
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

                if (objDOC360Methods.GetDocumentsWithHCP(objDocsRequest, out var objDOECAAResponse, out message) == ExceptionTypes.Success && objDOECAAResponse.lstDOECAADocument.Any())
                {
                    var doc = objDOECAAResponse.lstDOECAADocument.First();
                    if (doc.GlobalDocId != null)
                    {
                        var doc360GlobalDocId = doc.GlobalDocId.Split('|')[0];
                        _objBOCommon.Trace($"DOC360DirectController-{MethodBase.GetCurrentMethod().Name} - Before GetContentStream - {requestId}", "", BOCommon.JsonConvertObjectToString(new { doc360GlobalDocId }), username);

                        var result = GetContentStreamForHCP(doc360GlobalDocId, sourceDocclassTypeName);
                        _objBOCommon.Trace($"DOC360DirectController-{MethodBase.GetCurrentMethod().Name} - After GetContentStream - {requestId}", "", BOCommon.JsonConvertObjectToString(new { doc360GlobalDocId }), username);

                        _objBOCommon.GetRRTGPSAPiConfig(out lstATSLookupMaster);
                        string FileSizeLimit = _objBOCommon.GetLookupValueBasedonCon(lstATSLookupMaster, (long)RRTGPSAPIConfig.FileSizeLimit, 0);

                        if (result is FileContentResult fileResult)
                        {
                            var fileBytes = fileResult.FileContents;
                            var fileName = fileResult.FileDownloadName;
                            doc.CaseID = caseId;
                            doc.DocType = "INIT CORR";
                            doc.Title = doc.CaseID + " " + "INIT CORR";
                            doc.ContentStream = fileBytes;
                            doc.ContentStreamFileName = fileName;
                            doc.FileContentSize = fileBytes.Length;
                            doc.KeyAccountIndicator = keyAccountInd;
                            doc.EmployeeRestrictedInd = EmployeeRestrictInd;
                            doc.OnshoreIndicator = OnshoreInd;
                            doc.UploadedOn = DateTime.Now.ToString("MM-dd-yyyy");
                            doc.DocClassToUpload = destinationDocClass;

                            _objBOCommon.Trace($"DOC360DirectController-{MethodBase.GetCurrentMethod().Name} - Before CreateDocument - {requestId}", "", BOCommon.JsonConvertObjectToString(new { doc }), username);
                            if (fileBytes.Length > (Convert.ToInt64(FileSizeLimit) * 2))
                            {

                                _objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name + " CreateDocumentWithStargate - Request object of-" + requestId, fileBytes.Length.ToString(), source, _httpContextAccessor.HttpContext.User.Identity.Name);
                                objDOC360Response = objDOC360Methods.CreateDocumentWithHCPLargeFile(doc);

                            }
                            else
                            {
                                objDOC360Response = objDOC360Methods.CreateDocumentWithHCP(doc);
                            }
                            _objBOCommon.Trace($"DOC360DirectController-{MethodBase.GetCurrentMethod().Name} - After CreateDocument - {requestId}", "", BOCommon.JsonConvertObjectToString(new { objDOC360Response }), username);

                            if (objDOC360Response != null && (objDOC360Response.globalDocId != null || objDOC360Response.global_doc_id != null))
                            {
                                doc.ContentStream = null;
                                doc.GlobalDocId = objDOC360Response.globalDocId ?? objDOC360Response.global_doc_id;
                                doc.CreationDate = DateTime.UtcNow;
                                _objBOCommon.Trace($"DOC360DirectController-{MethodBase.GetCurrentMethod().Name} CreateDocument - Response Success - {requestId}", $"{fileName} - size-{doc.FileContentSize}", BOCommon.JsonConvertObjectToString(objDOC360Response), username);
                                return Ok(doc);
                            }
                            else
                            {
                                doc.ContentStream = null;
                                _objBOCommon.Trace($"DOC360DirectController-{MethodBase.GetCurrentMethod().Name} CreateDocument - Failed - {requestId}", $"{fileName} - size-{doc.FileContentSize}", BOCommon.JsonConvertObjectToString(objDOC360Response), username);
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

        [HttpPut]
        public ActionResult DocumentMetadataBulkUpdaterWithHCP([FromQuery] string docClassName, [FromBody] List<DOECAADocument> documentDataList)
        {
            string RequestId = Guid.NewGuid().ToString();
            _username = _httpContextAccessor.HttpContext.User.Identity.Name;
            source = BOCommon.GetRefererURI(Request);
            List<Doc360BulkUpdateResponse> objDOC360Response = new List<Doc360BulkUpdateResponse>();

            try
            {
                // Validate input parameters
                if (documentDataList == null || !documentDataList.Any())
                {
                    _objBOCommon.Trace("DOC360DirectController-" + MethodBase.GetCurrentMethod().Name +
                        " DocumentMetadataBulkUpdaterWithHCP - Invalid Request - " + RequestId, "",
                        "Document data list is null or empty", _username);
                    return BadRequest("Document data list cannot be null or empty");
                }

                // Validate that all documents have GlobalDocId
                var documentsWithoutGlobalId = documentDataList
                    .Where(doc => string.IsNullOrEmpty(doc.GlobalDocId))
                    .ToList();

                if (documentsWithoutGlobalId.Any())
                {
                    var errorMessage = "All documents must have a GlobalDocId value. Missing GlobalDocId for documents at positions: " +
                        string.Join(", ", documentDataList.Select((doc, index) =>
                            string.IsNullOrEmpty(doc.GlobalDocId) ? (index + 1).ToString() : null)
                            .Where(pos => pos != null));

                    _objBOCommon.Trace("DOC360DirectController-" + MethodBase.GetCurrentMethod().Name +
                        " DocumentMetadataBulkUpdaterWithHCP - Invalid Request - " + RequestId, "",
                        errorMessage, _username);

                    return BadRequest(errorMessage);
                }

                // Set default document class if not provided
                if (string.IsNullOrEmpty(docClassName))
                {
                    docClassName = "u_obhats_clinical_docs";
                }

                DOC360DirectMethods objDOC360Methods = new DOC360DirectMethods(_memoryCacheHelper, _objConfiguration, _objBOCommon);

                // Log incoming request
                _objBOCommon.Trace("DOC360DirectController-" + MethodBase.GetCurrentMethod().Name +
                    " DocumentMetadataBulkUpdaterWithHCP - Request object of-" + RequestId, "",
                    BOCommon.JsonConvertObjectToString(new { docClassName, documentCount = documentDataList.Count }),
                    _username);

                // Call service method with the correct parameters
                objDOC360Response = objDOC360Methods.DocumentMetadataBulkUpdaterWithHCP(documentDataList, docClassName);

                // Check response
                if (objDOC360Response == null)
                {
                    _objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name +
                        " DocumentMetadataBulkUpdaterWithHCP - Null Response - " + RequestId, "",
                        "Service returned null response", _username);
                    return StatusCode(500, "Service returned null response");
                }

                // Log success response
                _objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name +
                    " DocumentMetadataBulkUpdaterWithHCP - Response object of-" + RequestId, "",
                    BOCommon.JsonConvertObjectToString(objDOC360Response), _username);

                return Ok(objDOC360Response);
            }
            catch (Exception ex)
            {
                // Log error
                _objBOCommon.LogError(
                    this.GetType().Name + MethodBase.GetCurrentMethod(),
                    source,
                    10001,
                    ex.Message,
                    " ",
                    ex.StackTrace.ToString(),
                    _username);

                // Log error trace
                _objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name +
                    " DocumentMetadataBulkUpdaterWithHCP - Exception - " + RequestId, "",
                    ex.ToString(), _username);

                return StatusCode(500, new { message = "An error occurred while processing the request", error = ex.Message });
            }
        }
        [HttpPost]
        public DOC360Response CopyDocContentAndUpdateMetaWithHCP(
       [FromQuery] string sourceDocClass,
       [FromQuery] string sourceDocId,
       [FromQuery] string destinationDocClass,
       [FromBody] DOECAADocument documentData)
        {
            string RequestId = Guid.NewGuid().ToString();
            // Get username and source
            _username = _httpContextAccessor.HttpContext.User.Identity.Name;
            source = BOCommon.GetRefererURI(Request);

            var objDOC360Response = new DOC360Response
            {
                messageMap = new messageMap
                {
                    error = new List<string>()
                },
                Status = "400" // Default to bad request, will be updated on success
            };

            try
            {
                // Input validation
                var validationErrors = new List<string>();

                if (string.IsNullOrEmpty(sourceDocClass))
                {
                    validationErrors.Add("Source document class is required");
                }

                if (string.IsNullOrEmpty(sourceDocId))
                {
                    validationErrors.Add("Source document ID is required");
                }

                if (string.IsNullOrEmpty(destinationDocClass))
                {
                    validationErrors.Add("Destination document class is required");
                }

                if (documentData == null)
                {
                    validationErrors.Add("Document metadata is required");
                }
                else
                {
                    // Document data validations - only check if documentData is not null
                    if (string.IsNullOrEmpty(documentData.CaseID))
                    {
                        validationErrors.Add("Document CaseID is required");
                    }

                    if (string.IsNullOrEmpty(documentData.Title))
                    {
                        validationErrors.Add("Document title is required");
                    }
                }

                // If there are any validation errors, add them to the response and return
                if (validationErrors.Any())
                {
                    objDOC360Response.Status = "400";
                    objDOC360Response.messageMap.error.AddRange(validationErrors);
                    return objDOC360Response;
                }

                // Get username and source
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                source = BOCommon.GetRefererURI(Request);

                // Log request
                _objBOCommon.Trace(
                    $"DOC360DirectController-{MethodBase.GetCurrentMethod().Name} - Start - {RequestId}",
                    "",
                    BOCommon.JsonConvertObjectToString(new { sourceDocClass, sourceDocId, destinationDocClass }),
                    _username);

                // Prepare document data
                documentData.DocClassToUpload = destinationDocClass;
                documentData.SourceDocClass = sourceDocClass;
                documentData.SourceGlobalDocId = sourceDocId;
                documentData.CreatedBy = _username;
                documentData.SecurityGroup ??= "AG";

                // Create instances and process
                var objDOC360Methods = new DOC360DirectMethods(_memoryCacheHelper, _objConfiguration, _objBOCommon);
                objDOC360Response = objDOC360Methods.CopyDocContentAndUpdateMetaWithHCP(documentData);

                // Handle response
                if (objDOC360Response != null &&
                    (objDOC360Response.globalDocId != null || objDOC360Response.global_doc_id != null))
                {
                    objDOC360Response.Status = "Success";
                    _objBOCommon.Trace(
                        $"DOC360DirectController-{MethodBase.GetCurrentMethod().Name} - Success - {RequestId}",
                        "",
                        BOCommon.JsonConvertObjectToString(objDOC360Response),
                        _username);
                }
                else
                {
                    objDOC360Response.Status = "500";
                    _objBOCommon.Trace(
                        $"DOC360DirectController-{MethodBase.GetCurrentMethod().Name} - Failed - {RequestId}",
                        "",
                        BOCommon.JsonConvertObjectToString(objDOC360Response),
                        _username);
                }
            }
            catch (Exception ex)
            {
                objDOC360Response.Status = "500";
                objDOC360Response.messageMap.error.Add($"Internal server error: {ex.Message}");

                _objBOCommon.Trace(
                    $"DOC360DirectController-{MethodBase.GetCurrentMethod().Name} - Exception - {RequestId}",
                    "",
                    ex.Message,
                    _username);

                _objBOCommon.LogError(
                    GetType().Name + MethodBase.GetCurrentMethod(),
                    source,
                    10001,
                    ex.Message,
                    " ",
                    ex.StackTrace.ToString(),
                    _username);
            }

            return objDOC360Response;
        }

        [HttpPut]
        public DOC360Response UpdateSingleDocumentWithHCP(List<AttributeList> listDoc360Request, string GlobalDocId, string docclassTypeName = "u_obhats_clinical_docs")
        {
            DOC360DirectMethods objDOC360Methods = new DOC360DirectMethods(_memoryCacheHelper, _objConfiguration, _objBOCommon);
            var provider = new MultipartMemoryStreamProvider();
            EDMSServiceMethods objEDMSServiceMethods = new EDMSServiceMethods(_httpContextAccessor, _objConfiguration);
            DOC360Response objDOC360Response = new DOC360Response();
            objDOC360Response.messageMap = new messageMap();
            DOECAADocument objDOECAADocument = new DOECAADocument();

            string fileName = string.Empty;
            source = BOCommon.GetRefererURI(Request);

            try
            {
                if (string.IsNullOrEmpty(docclassTypeName))
                {
                    docclassTypeName = "u_obhats_clinical_docs";
                }
                string RequestId = Guid.NewGuid().ToString();
                _objBOCommon.Trace("DOC360DirectController-" + MethodBase.GetCurrentMethod().Name + " UpdateSingleDocumentWithHCP - Request object of-" + RequestId, "", BOCommon.JsonConvertObjectToString(new { searchId = Request }), _httpContextAccessor.HttpContext.User.Identity.Name);

                objDOC360Response = objDOC360Methods.UpdateSingleDocumentWithHCP(listDoc360Request, GlobalDocId, docclassTypeName);

                _objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name + " UpdateSingleDocumentWithHCP - Response object of-" + RequestId, "", BOCommon.JsonConvertObjectToString(objDOC360Response), _httpContextAccessor.HttpContext.User.Identity.Name);
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
        [HttpPut]
        public ActionResult UpdateBulkDocumentWithHCP(List<Doc360DirectRequest> listDoc360Request, string GlobalDocId, string docclassTypeName = "u_obhats_clinical_docs")
        {
            DOC360DirectMethods objDOC360Methods = new DOC360DirectMethods(_memoryCacheHelper, _objConfiguration, _objBOCommon);
            var provider = new MultipartMemoryStreamProvider();
            EDMSServiceMethods objEDMSServiceMethods = new EDMSServiceMethods(_httpContextAccessor, _objConfiguration);
            List<Doc360BulkUpdateResponse> objDOC360Response = new List<Doc360BulkUpdateResponse>();
            DOECAADocument objDOECAADocument = new DOECAADocument();

            string fileName = string.Empty;
            string DocClassname = string.Empty;
            source = BOCommon.GetRefererURI(Request);

            try
            {
                string RequestId = Guid.NewGuid().ToString();
                _objBOCommon.Trace("DOC360DirectController-" + MethodBase.GetCurrentMethod().Name + " CreateDocument - Request object of-" + RequestId, "", BOCommon.JsonConvertObjectToString(new { searchId = Request }), _httpContextAccessor.HttpContext.User.Identity.Name);

                objDOC360Response = objDOC360Methods.UpdateBulkDocumentWithHCP(listDoc360Request);

                _objBOCommon.Trace("DOC360Controller-" + MethodBase.GetCurrentMethod().Name + " CreateDocument - Response object of-" + RequestId, "", BOCommon.JsonConvertObjectToString(objDOC360Response), _httpContextAccessor.HttpContext.User.Identity.Name);
            }
            catch (Exception ex)
            {

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
        #endregion

    }

}
