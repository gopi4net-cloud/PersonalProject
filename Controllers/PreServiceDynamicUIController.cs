using ANGDDEAPI.Common;
using ANGDDEAPIBO;
using ANGDDEAPIDO.Interface;
using ANGDDEAPIDO;
using ANGDDEAPIFoundation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Dynamic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
namespace ANGDDEAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PreServiceDynamicUIController : ControllerBase
    {
        public static long LoggedInUserId = 0;
        public string _username = string.Empty;
        string source = string.Empty;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DOConfiguration _objConfiguration;
        private readonly IMemoryCacheHelper _memoryCacheHelper;
        public BOGPSMemberDetails _objBOGPSMemberDetails = null;
        public readonly BOCommon _objBOCommon;
        private readonly ICacheService _cache;
        public ExceptionTypes exResult;
        BOCPMLRouter objBOCPMLRouter = null;
        BOStargateAuthentication objStargateAuth = null;
        HttpClient client = null;

        #region Constructor
        public PreServiceDynamicUIController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
        {
            _cache = cache;
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _memoryCacheHelper = memoryCacheHelper;
            _objBOGPSMemberDetails = new BOGPSMemberDetails(objConfiguration, _cache);
            _objBOCommon = bOCommon;
            objBOCPMLRouter = new BOCPMLRouter(_objConfiguration, _cache);
            objStargateAuth = new BOStargateAuthentication(_memoryCacheHelper, _objConfiguration, _httpContextAccessor, _objBOCommon);
            client = new HttpClient();
        }
        #endregion

        [HttpGet]
        public ActionResult Get(string strSearch)
        {
            string result = string.Empty;
            string Trn_CaseID = string.Empty;
            long CpmlApilogID = 0;
            try
            {
                if (!string.IsNullOrEmpty(strSearch))
                {
                    DOPreserviceDynamicUI objRouterRequestParams = new DOPreserviceDynamicUI();

                    objRouterRequestParams = JsonConvert.DeserializeObject<DOPreserviceDynamicUI>(strSearch);
                    Trn_CaseID = objRouterRequestParams?.CaseDetails.TrnCaseID;

                    LoggedInUserId = !string.IsNullOrEmpty(objRouterRequestParams.UserId) ? Convert.ToInt64(objRouterRequestParams.UserId) : 0;
                    exResult = objStargateAuth.GetStargateConnection(ref client);

                    if (exResult == ExceptionTypes.Success)
                    {
                        string dynamic_pre_ui_request = _objConfiguration.AppSettings.dynamic_pre_ui;
                        string json = JsonConvert.SerializeObject(objRouterRequestParams, new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore
                        });
                        HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
                        CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, 0, json, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.dynamic_pre_ui);
                        HttpResponseMessage response = client.PostAsync(dynamic_pre_ui_request, content).Result;
                        result = response.Content.ReadAsStringAsync().Result;
                        objBOCPMLRouter.SavePreServiceCPMLResponse(!string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, LoggedInUserId, _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.dynamic_pre_ui, strSearch, result);

                        if (!response.IsSuccessStatusCode)
                        {
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Error, result, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.dynamic_pre_ui);
                            return BadRequest(result);
                        }
                        else
                        {
                            this.objBOCPMLRouter.CPMLvalidationStageStarted(LoggedInUserId, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0);
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Response, result, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.dynamic_pre_ui);
                        }
                        return Ok(result);
                    }
                }

                return BadRequest();

            }
            catch (Exception ex)
            {
                CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Error, ex.Message, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.dynamic_pre_ui);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public ActionResult GetOutCome2(string strSearch)
        {
            string result = string.Empty;
            string Trn_CaseID = string.Empty;
            long CpmlApilogID = 0;
            try
            {
                if (!string.IsNullOrEmpty(strSearch))
                {
                    DOPreserviceDynamicUI objRouterRequestParams = new DOPreserviceDynamicUI();

                    objRouterRequestParams = JsonConvert.DeserializeObject<DOPreserviceDynamicUI>(strSearch);
                    Trn_CaseID = objRouterRequestParams?.CaseDetails.TrnCaseID;

                    LoggedInUserId = !string.IsNullOrEmpty(objRouterRequestParams.UserId) ? Convert.ToInt64(objRouterRequestParams.UserId) : 0;
                    exResult = objStargateAuth.GetStargateConnection(ref client);

                    if (exResult == ExceptionTypes.Success)
                    {
                        string dynamic_pre_ui_request_2 = _objConfiguration.AppSettings.dynamic_pre_ui_2;
                        string json = JsonConvert.SerializeObject(objRouterRequestParams, new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore
                        });
                        HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
                        CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, 0, json, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.dynamic_pre_ui_2);
                        HttpResponseMessage response = client.PostAsync(dynamic_pre_ui_request_2, content).Result;
                        result = response.Content.ReadAsStringAsync().Result;
                        objBOCPMLRouter.SavePreServiceCPMLResponse(!string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, LoggedInUserId, _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.dynamic_pre_ui_2, strSearch, result);

                        if (!response.IsSuccessStatusCode)
                        {
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Error, result, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.dynamic_pre_ui_2);
                            return BadRequest(result);
                        }
                        else
                        {
                            this.objBOCPMLRouter.CPMLvalidationStageStarted(LoggedInUserId, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0);
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Response, result, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.dynamic_pre_ui_2);
                        }
                        return Ok(result);
                    }
                }

                return BadRequest();

            }
            catch (Exception ex)
            {
                CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Error, ex.Message, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.dynamic_pre_ui_2);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public IActionResult GetInitialCPMLDetails(string Trn_CaseID, string UserId = "")
        {
            LoggedInUserId = !string.IsNullOrEmpty(UserId) ? Convert.ToInt64(UserId) : 0;
            var _getDeterminationLkup = GetDetermineLkupId(Trn_CaseID) as OkObjectResult;
            var determinationLkupValue = _getDeterminationLkup?.Value?.ToString();

            return Ok(new
            {
                DeterminationLkup = long.TryParse(determinationLkupValue, out var result) ? result : 0
            });
        }

        public IActionResult GetDetermineLkupId(string Trn_CaseID)
        {
            string result = string.Empty;
            long CpmlApilogID = 0;
            try
            {
                if (!string.IsNullOrEmpty(Trn_CaseID))
                {
                    exResult = objStargateAuth.GetStargateConnection(ref client);
                    if (exResult == ExceptionTypes.Success)
                    {
                        dynamic Reqobj = new ExpandoObject();
                        Reqobj.TrnCaseID = Trn_CaseID;

                        string json = JsonConvert.SerializeObject(Reqobj, new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore
                        });
                        HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

                        string dynamic_pre_ui2_DetermineUrl = _objConfiguration.AppSettings.dynamic_pre_ui2_DetermineUrl;

                        CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, 0, json, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + dynamic_pre_ui2_DetermineUrl);

                        HttpResponseMessage response = client.PostAsync(dynamic_pre_ui2_DetermineUrl, content).Result;
                        result = response.Content.ReadAsStringAsync().Result;

                        //Response logging
                        objBOCPMLRouter.SavePreServiceCPMLResponse(!string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, LoggedInUserId, _objConfiguration.AppSettings.StarGatewayCoreAddress + dynamic_pre_ui2_DetermineUrl, "", result);

                        if (!response.IsSuccessStatusCode)
                        {
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Error, result, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + dynamic_pre_ui2_DetermineUrl);
                        }
                        else
                        {
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Response, result, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + dynamic_pre_ui2_DetermineUrl);
                        }
                        return Ok(result);
                    }
                }

                return BadRequest();

            }
            catch (Exception ex)
            {
                CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Error, ex.Message, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.dynamic_pre_ui2_DetermineUrl);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Post(string Data, string Trn_CaseID, string UserId)
        {
            long CpmlApilogID = 0;
            try
            {

                if (!string.IsNullOrEmpty(Data) && !string.IsNullOrEmpty(Trn_CaseID) && !string.IsNullOrEmpty(UserId))
                {
                    var objRouterRequestParams = JsonConvert.DeserializeObject(Data);
                    LoggedInUserId = !string.IsNullOrEmpty(UserId) ? Convert.ToInt64(UserId) : 0;
                    exResult = objStargateAuth.GetStargateConnection(ref client);

                    if (exResult == ExceptionTypes.Success)
                    {
                        string json = JsonConvert.SerializeObject(objRouterRequestParams, new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore
                        });
                        HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

                        CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, 0, json, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.dynamic_pre_put_res);
                        HttpResponseMessage response = client.PostAsync(_objConfiguration.AppSettings.dynamic_pre_put_res, content).Result;
                        string result = response.Content.ReadAsStringAsync().Result;

                        if (!response.IsSuccessStatusCode)
                        {
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Error, result, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.dynamic_pre_put_res);
                            return BadRequest(result);
                        }
                        else
                        {
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Response, result, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.dynamic_pre_put_res);
                        }

                        return Ok(result);
                    }
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Error, ex.Message, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.dynamic_pre_put_res);
                return BadRequest(ex);
            }

        }
        [HttpGet]
        public async Task<IActionResult> PostO1(string Data, string Trn_CaseID, string UserId)
        {
            long CpmlApilogID = 0;
            try
            {

                if (!string.IsNullOrEmpty(Data) && !string.IsNullOrEmpty(Trn_CaseID) && !string.IsNullOrEmpty(UserId))
                {
                    var objRouterRequestParams = JsonConvert.DeserializeObject(Data);
                    LoggedInUserId = !string.IsNullOrEmpty(UserId) ? Convert.ToInt64(UserId) : 0;
                    exResult = objStargateAuth.GetStargateConnection(ref client);

                    if (exResult == ExceptionTypes.Success)
                    {
                        string json = JsonConvert.SerializeObject(objRouterRequestParams, new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore
                        });
                        HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

                        CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, 0, json, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.dynamic_pre_put_res);
                        HttpResponseMessage response = client.PostAsync(_objConfiguration.AppSettings.dynamic_pre_put_res, content).Result;
                        string result = response.Content.ReadAsStringAsync().Result;

                        if (!response.IsSuccessStatusCode)
                        {
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Error, result, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.dynamic_pre_put_res);
                            return BadRequest(result);
                        }
                        else
                        {
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Response, result, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.dynamic_pre_put_res);
                        }

                        return Ok(result);
                    }
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Error, ex.Message, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.dynamic_pre_put_res);
                return BadRequest(ex);
            }

        }


        [HttpGet]
        public IActionResult GetGlobalDocID(string case_id)
        {
            try
            {
                var globalDocID = GetCPMLGlobalDocIdOutCome1(case_id) as OkObjectResult;
                var globalDocIDValue = globalDocID?.Value?.ToString();

                if (!string.IsNullOrEmpty(globalDocIDValue))
                {
                    var jsonObject = JObject.Parse(globalDocIDValue);

                    var docRef = jsonObject["doc_ref"];

                    if (docRef != null)
                    {
                        var objectId = docRef["object_id"]?.ToString() ?? "";
                        var globalDocId = objectId.Split('|')[0];

                        return Ok(new
                        {
                            globaldocid = globalDocId,
                            classname = docRef["type_name"]?.ToString() ?? "",
                            contenttype = jsonObject["content_type"]?.ToString() ?? "",
                            isProd = jsonObject["model"]?.ToString() ?? "0",
                        });
                    }
                    else
                    {
                        return Ok(new
                        {
                            globaldocid = jsonObject["global_doc_id"]?.ToString() ?? "",
                            classname = jsonObject["class_name"]?.ToString() ?? "",
                            contenttype = jsonObject["content_type"]?.ToString() ?? "",
                            isProd = jsonObject["is_prod"]?.ToString() ?? "0",
                        });
                    }
                }

                return Ok(new
                {
                    globaldocid = "",
                    classname = "",
                    contenttype = ""
                });
            }
            catch (JsonReaderException)
            {
                return Ok(new
                {
                    globaldocid = "",
                    classname = "",
                    contenttype = ""
                });
            }
            catch (Exception)
            {
                return Ok(new
                {
                    globaldocid = "",
                    classname = "",
                    contenttype = ""
                });
            }
        }
        public IActionResult GetCPMLGlobalDocId(string Trn_CaseID)
        {
            string result = string.Empty;
            long CpmlApilogID = 0;
            try
            {
                if (!string.IsNullOrEmpty(Trn_CaseID))
                {
                    exResult = objStargateAuth.GetStargateConnection(ref client);

                    if (exResult == ExceptionTypes.Success)
                    {
                        dynamic Reqobj = new ExpandoObject();
                        Reqobj.TrnCaseID = Trn_CaseID;

                        string json = JsonConvert.SerializeObject(Reqobj, new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore
                        });
                        HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

                        CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.GET, 0, Trn_CaseID, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.dynamic_pre_ui2_documenturl);
                        HttpResponseMessage response = client.PostAsync(_objConfiguration.AppSettings.grv_cpml_documenturl, content).Result;
                        result = response.Content.ReadAsStringAsync().Result;

                        if (!response.IsSuccessStatusCode)
                        {
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.GET, (int)LogType.Error, result, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.dynamic_pre_ui2_documenturl);
                        }
                        else
                        {
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.GET, (int)LogType.Response, result, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.dynamic_pre_ui2_documenturl);
                        }
                        return Ok(result);
                    }
                }

                return BadRequest();

            }
            catch (Exception ex)
            {
                CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.GET, (int)LogType.Error, result, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.dynamic_pre_ui2_documenturl);
                return BadRequest(ex.Message);
            }
        }

        public IActionResult GetCPMLGlobalDocIdOutCome1(string case_id)
        {
            string result = string.Empty;
            long CpmlApilogID = 0;
            try
            {
                if (!string.IsNullOrEmpty(case_id))
                {
                    exResult = objStargateAuth.GetStargateConnection(ref client);

                    if (exResult == ExceptionTypes.Success)
                    {
                        dynamic Reqobj = new ExpandoObject();
                        Reqobj.case_id = case_id;

                        string dynamic_pre_document_ui_request = _objConfiguration.AppSettings.dynamic_pre_ui_documenturl;
                        string requestUrl = $"{dynamic_pre_document_ui_request}case_id={Reqobj.case_id}";
                        string json = JsonConvert.SerializeObject(Reqobj, new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore
                        });
                        HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

                        CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(case_id) ? Convert.ToInt64(case_id) : 0, (long)HttpRequestType.GET, 0, case_id, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.dynamic_pre_ui_documenturl);
                        HttpResponseMessage response = client.GetAsync(requestUrl).Result;
                        result = response.Content.ReadAsStringAsync().Result;

                        if (!response.IsSuccessStatusCode)
                        {
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(case_id) ? Convert.ToInt64(case_id) : 0, (long)HttpRequestType.GET, (int)LogType.Error, result, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.dynamic_pre_ui_documenturl);
                        }
                        else
                        {
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(case_id) ? Convert.ToInt64(case_id) : 0, (long)HttpRequestType.GET, (int)LogType.Response, result, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.dynamic_pre_ui_documenturl);
                        }
                        return Ok(result);
                    }
                }

                return BadRequest();

            }
            catch (Exception ex)
            {
                CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(case_id) ? Convert.ToInt64(case_id) : 0, (long)HttpRequestType.GET, (int)LogType.Error, result, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.dynamic_pre_ui2_documenturl);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public IActionResult GetAction(string case_id)
        {
            string result = string.Empty;
            string Trn_CaseID = string.Empty;
            long CpmlApilogID = 0;
            try
            {
                if (!string.IsNullOrEmpty(case_id))
                {
                    DOPreserviceDynamicUI objRouterRequestParams = new DOPreserviceDynamicUI();
                    Trn_CaseID = case_id;

                    LoggedInUserId = !string.IsNullOrEmpty(objRouterRequestParams.UserId) ? Convert.ToInt64(objRouterRequestParams.UserId) : 0;
                    exResult = objStargateAuth.GetStargateConnection(ref client);

                    if (exResult == ExceptionTypes.Success)
                    {
                        string dynamic_pre_action_ui_request = _objConfiguration.AppSettings.dynamic_pre_action_ui;
                        string requestUrl = $"{dynamic_pre_action_ui_request}case_id={Trn_CaseID}";
                        string json = JsonConvert.SerializeObject(objRouterRequestParams, new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore
                        });
                        HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
                        CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, 0, json, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.dynamic_pre_action_ui);
                        HttpResponseMessage response = client.GetAsync(requestUrl).Result;
                        result = response.Content.ReadAsStringAsync().Result;
                        objBOCPMLRouter.SavePreServiceCPMLResponse(!string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, LoggedInUserId, _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.dynamic_pre_action_ui, case_id, result);

                        if (!response.IsSuccessStatusCode)
                        {
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Error, result, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.dynamic_pre_action_ui);
                            return BadRequest(result);
                        }
                        else
                        {
                            this.objBOCPMLRouter.CPMLvalidationStageStarted(LoggedInUserId, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0);
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Response, result, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.dynamic_pre_action_ui);
                        }
                        return Ok(result);
                    }
                }

                return BadRequest();

            }
            catch (Exception ex)
            {
                CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Error, ex.Message, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.dynamic_pre_action_ui);
                return BadRequest(ex.Message);
            }
        }
    }
}
