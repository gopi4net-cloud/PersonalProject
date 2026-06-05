using ANGDDEAPI.Common;
using ANGDDEAPIBO;
using ANGDDEAPIDO;
using ANGDDEAPIDO.Interface;
using EDMSServiceReference;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;


namespace ANGDDEAPI.Controllers
{

    [ApiController]
    [Route("api/[controller]/[action]")]
    public class EDMSController : ControllerBase
    {
        public string _username = string.Empty;
        string source = string.Empty;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DOConfiguration _objConfiguration;
        private readonly BOCommon _objBOCommon;
        private readonly ICacheService _cache; 
         
        public EDMSController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, BOCommon bOCommon, ICacheService cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _objBOCommon = bOCommon;
            _cache = cache;
        }

        /// <summary>
        /// Method to get the list of documents which are created in ECAA for the specific Case ID
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>

        //[HttpGet]
        //public async Task<IActionResult> GetQuery(string caseId)
        //{
        //    List<DOECAADocument> lstDOECAADocument = null;
        //    _username = _httpContextAccessor.HttpContext.User.Identity.Name;
        //    DOECAAResponse objDOECAAResponse = new DOECAAResponse();
        //    source = BOCommon.GetRefererURI(Request);
        //    EDMSServiceMethods objEDMSServiceMethods = new EDMSServiceMethods(_httpContextAccessor, _objConfiguration);
        //    try
        //    {
        //        lstDOECAADocument = await objEDMSServiceMethods.GetQuery(caseId);
        //        objDOECAAResponse.Status = "Success";
        //        objDOECAAResponse.lstDOECAADocument = lstDOECAADocument;
        //    }
        //    catch (Exception ex)
        //    {
        //        objDOECAAResponse.Status = "Failed";
        //        objDOECAAResponse.ErrorMessage = ex.Message;
        //        //Checking Referer is DDE or ATS.. as per the Source we are logging the error details
        //        if (source.Contains("DDE"))
        //        {
        //            _objBOCommon.ErrorLog(0, this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), ex.Message, ex.ToString());
        //        }
        //        else
        //        {
        //            BOCommon.LogError(this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
        //        }
        //    }
        //    return Ok(objDOECAAResponse);
        //}
        /// <summary>
        /// Method to get the list of documents which are created in ECAA for the specific Case ID
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>

        [HttpGet]
        public async Task<IActionResult> GetQuery(string caseId, string msid)
        {
            List<DOECAADocument> lstDOECAADocument = null;
            _username = _httpContextAccessor.HttpContext.User.Identity.Name;
            DOECAAResponse objDOECAAResponse = new DOECAAResponse();
            source = BOCommon.GetRefererURI(Request);
           // _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Request", caseId+"' "+ msid, _username);

            EDMSServiceMethods objEDMSServiceMethods = new EDMSServiceMethods(_httpContextAccessor, _objConfiguration);
            try
            {
                _httpContextAccessor.HttpContext.Items["UserId"] = string.IsNullOrEmpty(msid) ? null : msid;
                lstDOECAADocument = await objEDMSServiceMethods.GetQuery(caseId);
                objDOECAAResponse.Status = "Success";
                objDOECAAResponse.lstDOECAADocument = lstDOECAADocument;
            }
            catch (Exception ex)
            {
                objDOECAAResponse.Status = "Failed";
                objDOECAAResponse.ErrorMessage = ex.Message;
                //Checking Referer is DDE or ATS.. as per the Source we are logging the error details
                if (source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(0, this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), ex.Message, ex.ToString());
                }
                else
                {
                    _objBOCommon.LogError(this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                }
            }
            //_objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Response", BOCommon.JsonConvertObjectToString(objDOECAAResponse), _username);

            return Ok(objDOECAAResponse);
        }

        ///// <summary>
        ///// Method to get the list of documents which are created in ECAA for the specific Object Id
        ///// </summary>
        ///// <param name="objectId"></param>
        ///// <returns></returns>

        [HttpGet]
        public async Task<IActionResult> GetDocumentFromObjectId(string objectId)
        {
            List<DOECAADocument> lstDOECAADocument = null;
            _username = _httpContextAccessor.HttpContext.User.Identity.Name;
            DOECAAResponse objDOECAAResponse = new DOECAAResponse();
            source = BOCommon.GetRefererURI(Request);
            EDMSServiceMethods objEDMSServiceMethods = new EDMSServiceMethods(_httpContextAccessor, _objConfiguration);
          //  _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Request", objectId, _username);
            try
            {
                lstDOECAADocument = await objEDMSServiceMethods.GetDocumentFromObjectId(objectId);
                objDOECAAResponse.Status = "Success";
                objDOECAAResponse.lstDOECAADocument = lstDOECAADocument;
            }
            catch (Exception ex)
            {
                objDOECAAResponse.Status = "Failed";
                objDOECAAResponse.ErrorMessage = ex.Message;
                //Checking Referer is DDE or ATS.. as per the Source we are logging the error details
                if (source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(0, this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), ex.Message, ex.ToString());
                }
                else
                {
                    _objBOCommon.LogError(this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                }
            }
           // _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Response", BOCommon.JsonConvertObjectToString(objDOECAAResponse), _username);

            return Ok(objDOECAAResponse);
        }

        ///// <summary>
        ///// Method to Create the document in ECAA
        ///// </summary>
        ///// <returns></returns>

        [HttpPost]
        public async Task<DOECAAResponse> CreateDocument()
        {
            var provider = new MultipartMemoryStreamProvider();
            EDMSServiceMethods objEDMSServiceMethods = new EDMSServiceMethods(_httpContextAccessor, _objConfiguration);
            DOECAAResponse objDOECAAResponse = new DOECAAResponse();
            DOECAADocument objDOECAADocument = null;
            string objectId = string.Empty;
            source = BOCommon.GetRefererURI(Request);
            byte[] fileBytes = null;
            try
            {
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                var formData = _httpContextAccessor.HttpContext.Request.Form;
                if (formData != null && formData.Count > 0)
                {
                    foreach (IFormFile formFile in formData.Files)
                    {
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
               // _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Request", _httpContextAccessor.HttpContext.Request.Form.ToArray()[0].Value, _username);

                //  await Request.HttpContext.Content.ReadAsMultipartAsync(provider);
                string strObjDOECAADocument = _httpContextAccessor.HttpContext.Request.Form.ToArray()[0].Value;
                // byte[] file = await provider.Contents[0].ReadAsByteArrayAsync();
                // string strObjDOECAADocument = provider.Contents[1].ReadAsStringAsync().Result;
                objDOECAADocument = JsonConvert.DeserializeObject<DOECAADocument>(strObjDOECAADocument);
                objDOECAADocument.ContentStream = fileBytes;
                objDOECAAResponse.objDOECAADocument = await objEDMSServiceMethods.CreateDocument(objDOECAADocument);
                objDOECAAResponse.Status = "Success";
            }
            catch (Exception ex)
            {
                objDOECAAResponse.Status = "Failed";
                objDOECAAResponse.ErrorMessage = ex.Message;
                if (source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(0, this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), ex.Message, ex.ToString());
                }
                else
                {
                    _objBOCommon.LogError(this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                }
            }
           // _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Response", BOCommon.JsonConvertObjectToString(objDOECAAResponse), _username);

            return objDOECAAResponse;
        }
        ///// <summary>
        ///// Method to Delete the document in ECAA
        ///// </summary>
        ///// <returns></returns>

        [HttpPost]
        public async Task<DOECAAResponse> DeleteDocument()
        {
            var provider = new MultipartMemoryStreamProvider();
            EDMSServiceMethods objEDMSServiceMethods = new EDMSServiceMethods(_httpContextAccessor, _objConfiguration);
            DOECAAResponse objDOECAAResponse = new DOECAAResponse();
            DOECAADocument objDOECAADocument = null;
            string objectId = string.Empty;
            source = BOCommon.GetRefererURI(Request);
            try
            {
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                var formData = _httpContextAccessor.HttpContext.Request.Form;
               // _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Request", objectId, _username);

                if (formData != null && formData.Count > 0)
                {
                    string strObjDOECAADocument = _httpContextAccessor.HttpContext.Request.Form.ToArray()[0].Value;
                    objDOECAADocument = JsonConvert.DeserializeObject<DOECAADocument>(strObjDOECAADocument);
                    objDOECAAResponse.objDOECAADocument = await objEDMSServiceMethods.DeleteDocument(objDOECAADocument);
                    objDOECAAResponse.Status = "Success";
                }              
                
            }
            catch (Exception ex)
            {
                objDOECAAResponse.Status = "Failed";
                objDOECAAResponse.ErrorMessage = ex.Message;
                if (source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(0, this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), ex.Message, ex.ToString());
                }
                else
                {
                    _objBOCommon.LogError(this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                }
            }
           // _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Response", BOCommon.JsonConvertObjectToString(objDOECAAResponse), _username);

            return objDOECAAResponse;
        }
        /// <summary>
        /// Method to get the file data to download
        /// </summary>
        /// <param name="objectId"></param>
        /// <returns></returns>

        [HttpGet]
        public async Task<ActionResult> GetContentStream(string objectId)
        {
            EDMSServiceMethods objEDMSServiceMethods = new EDMSServiceMethods(_httpContextAccessor, _objConfiguration);
            cmisContentStreamType objcmisContentStreamType = null;
            string extension = string.Empty;
            var stream = new MemoryStream();
            source = BOCommon.GetRefererURI(Request);
            string contentType;
            //Create HTTP Response.

            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                //_objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Request", objectId, _username);
                objcmisContentStreamType = await objEDMSServiceMethods.GetContentStream(objectId);
                //Set the Response Content.
                response.Content = new ByteArrayContent(objcmisContentStreamType.stream);

                //Set the Response Content Length.
                response.Content.Headers.ContentLength = Convert.ToInt64(objcmisContentStreamType.length);

                extension = GetFileExtension(objcmisContentStreamType.mimeType);

                //Set the Content Disposition Header Value and FileName.
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                response.Content.Headers.ContentDisposition.FileName = objcmisContentStreamType.filename + extension;

               

                new FileExtensionContentTypeProvider().TryGetContentType(response.Content.Headers.ContentDisposition.FileName, out contentType);
                //Set the File Content Type.
                response.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
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
                throw ex;
            }
            //_objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Response", BOCommon.JsonConvertObjectToString(response), _username);

            return File(objcmisContentStreamType.stream, contentType);
        }
        /// <summary>
        /// Method to get the file extention
        /// </summary>
        /// <param name="mimeType"></param>
        /// <returns></returns>
        private string GetFileExtension(string mimeType)
        {
            string extension = string.Empty;
            switch (mimeType)
            {
                case "image/bmp":
                    extension = ".bmp";
                    break;
                case "text/csv":
                    extension = ".csv";
                    break;
                case "application/msword":
                    extension = ".doc";
                    break;
                case "application/vnd.openxmlformats-officedocument.wordprocessingml.document":
                    extension = ".docx";
                    break;
                case "image/jpeg":
                    extension = ".jpeg";
                    break;
                case "image/png":
                    extension = ".png";
                    break;
                case "application/pdf":
                    extension = ".pdf";
                    break;
                case "application/rtf":
                    extension = ".rtf";
                    break;
                case "image/tiff":
                    extension = ".tiff";
                    break;
                case "text/plain":
                    extension = ".txt";
                    break;
                case "application/vnd.ms-excel":
                    extension = ".xls";
                    break;
                case "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet":
                    extension = ".xlsx";
                    break;
                case "application/zip":
                    extension = ".zip";
                    break;
                case "application/x-zip-compressed":
                    extension = ".zip";
                    break;
                case "application/x-rar-compressed":
                    extension = ".rar";
                    break;
                case "application/rar":
                    extension = ".rar";
                    break;
                case "application/octet-stream":
                    extension = ".rar";
                    break;
                default:
                    break;
            }
            return extension;
        }
        /// <summary>
        /// Method to check-out the document based on objectid
        /// </summary>
        /// <param name="objectId"></param>
        /// <returns></returns>

        //[HttpGet]
        //public IActionResult Checkout(string objectId)
        //{
        //    EDMSServiceMethods objEDMSServiceMethods = new EDMSServiceMethods();
        //    DOECAAResponse objDOECAAResponse = new DOECAAResponse();
        //    try
        //    {
        //        objDOECAAResponse.objDOECAADocument = objEDMSServiceMethods.Checkout(objectId);
        //        objDOECAAResponse.Status = "Success";
        //    }
        //    catch (Exception ex)
        //    {
        //        objDOECAAResponse.Status = "Failed";
        //        objDOECAAResponse.ErrorMessage = ex.Message;
        //    }
        //    return Ok(objDOECAAResponse);
        //}

        ///// <summary>
        ///// Method to Check-In the document to update the document
        ///// </summary>
        ///// <returns></returns>

        //[HttpPost]
        //public async Task<DOECAAResponse> Checkin()
        //{
        //    var provider = new MultipartMemoryStreamProvider();
        //    DOECAAResponse objDOECAAResponse = new DOECAAResponse();
        //    EDMSServiceMethods objEDMSServiceMethods = new EDMSServiceMethods();
        //    DOECAADocument objDOECAADocument = null;
        //    source = BOCommon.GetRefererURI(Request);
        //    try
        //    {
        //        await Request.Content.ReadAsMultipartAsync(provider);
        //        byte[] file = await provider.Contents[0].ReadAsByteArrayAsync();
        //        string strObjDOECAADocument = provider.Contents[1].ReadAsStringAsync().Result;
        //        objDOECAADocument = JsonConvert.DeserializeObject<DOECAADocument>(strObjDOECAADocument);

        //        objDOECAADocument.ContentStream = file;
        //        objDOECAAResponse.objDOECAADocument = objEDMSServiceMethods.Checkin(objDOECAADocument);
        //        objDOECAAResponse.Status = "Success";
        //    }
        //    catch (Exception ex)
        //    {
        //        objDOECAAResponse.Status = "Failed";
        //        objDOECAAResponse.ErrorMessage = ex.Message;
        //        if (source.Contains("DDE"))
        //        {
        //            _objBOCommon.ErrorLog(0, this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), ex.Message, ex.ToString());
        //        }
        //        else
        //        {
        //            BOCommon.LogError(this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
        //        }
        //    }
        //    return objDOECAAResponse;
        //}
        ///// <summary>
        ///// Method to cancel the check-out based on object id
        ///// </summary>
        ///// <param name="objectId"></param>
        ///// <returns></returns>


        //[HttpGet]
        //public IActionResult CancelCheckOut(string objectId)
        //{
        //    EDMSServiceMethods objEDMSServiceMethods = new EDMSServiceMethods();
        //    DOECAAResponse objDOECAAResponse = new DOECAAResponse();
        //    source = BOCommon.GetRefererURI(Request);
        //    try
        //    {
        //        _username = _httpContextAccessor.HttpContext.User.Identity.Name;
        //        string[] strLoginName = _username.Split(new string[] { @"\" }, StringSplitOptions.RemoveEmptyEntries);
        //        _username = strLoginName.Length > 1 ? strLoginName[1] : "";
        //        objDOECAAResponse.Status = objEDMSServiceMethods.CancelCheckOut(objectId);
        //    }
        //    catch (Exception ex)
        //    {
        //        objDOECAAResponse.Status = "Failed";
        //        objDOECAAResponse.ErrorMessage = ex.Message;
        //        if (source.Contains("DDE"))
        //        {
        //            _objBOCommon.ErrorLog(0, this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), ex.Message, ex.ToString());
        //        }
        //        else
        //        {
        //            BOCommon.LogError(this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
        //        }
        //    }
        //    return Ok(objDOECAAResponse);
        //}
    }
}
