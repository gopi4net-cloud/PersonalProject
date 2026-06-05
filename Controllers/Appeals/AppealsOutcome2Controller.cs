using ANGDDEAPI.Common;
using ANGDDEAPIBO;
using ANGDDEAPIDO.Interface;
using ANGDDEAPIDO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using ANGDDEAPIFoundation;
using Newtonsoft.Json;
using System.Text;
using System;
using Newtonsoft.Json.Linq;
using System.Dynamic;

namespace ANGDDEAPI.Controllers
{

    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AppealsOutcome2Controller : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DOConfiguration _objConfiguration;
        private readonly IMemoryCacheHelper _memoryCacheHelper;
        private readonly ICacheService _cache;

        public static long LoggedInUserId = 0;
        BOStargateAuthentication objStargateAuth = null;
        BOAppealsOutcome objBOCPMLRouter = null;
        public readonly BOCommon _objBOCommon;
        HttpClient client = null;
        public ExceptionTypes exResult;
        public string hicsCPMLAPIURL = "";
        public AppealsOutcome2Controller(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
        {
            _cache = cache;
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _memoryCacheHelper = memoryCacheHelper;
            _objBOCommon = bOCommon;
            objStargateAuth = new BOStargateAuthentication(_memoryCacheHelper, _objConfiguration, _httpContextAccessor, _objBOCommon);
            objBOCPMLRouter = new BOAppealsOutcome(_objConfiguration, _cache);
            client = new HttpClient();
        }

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
                    DOOutCome2CPMLRouter objRouterRequestParams = new DOOutCome2CPMLRouter();

                    objRouterRequestParams = JsonConvert.DeserializeObject<DOOutCome2CPMLRouter>(strSearch);
                    Trn_CaseID = objRouterRequestParams?.CaseDetails?.TrnCaseID;

                    LoggedInUserId = !string.IsNullOrEmpty(objRouterRequestParams.UserId) ? Convert.ToInt64(objRouterRequestParams.UserId) : 0;
                    exResult = objStargateAuth.GetStargateConnection(ref client);

                    if (exResult == ExceptionTypes.Success)
                    {
                        string outcome2_cpml_router_request = _objConfiguration.AppSettings.outcome2_cpml_router_req;
                        string json = JsonConvert.SerializeObject(objRouterRequestParams, new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore
                        });
                        HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
                        // Check
                        CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, 0, json, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.outcome2_cpml_router_req);
                        HttpResponseMessage response = client.PostAsync(outcome2_cpml_router_request, content).Result;
                        result = response.Content.ReadAsStringAsync().Result;
                        objBOCPMLRouter.SaveOutcome2CPMLResponse(!string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, 0, LoggedInUserId, _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.outcome2_cpml_router_req, strSearch, result);

                        if (!response.IsSuccessStatusCode)
                        {
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Error, result, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.outcome2_cpml_router_req);
                            return BadRequest(result);
                        }
                        else
                        {
                            this.objBOCPMLRouter.CPMLvalidationStageStarted(LoggedInUserId, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0);
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Response, result, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.outcome2_cpml_router_req);
                        }
                        return Ok(result);
                    }
                }

                return BadRequest();

            }
            catch (Exception ex)
            {
                CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Error, ex.Message, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.outcome2_cpml_router_req);
                return BadRequest(ex.Message);
            }

        }

        [HttpGet]
        public IActionResult Post(string requestParam)
        {
            long CpmlApilogID = 0;
            OutCome2RequestParam _requestParam = new OutCome2RequestParam();
            try
            {
                if (!string.IsNullOrEmpty(requestParam))
                {
                    _requestParam = JsonConvert.DeserializeObject<OutCome2RequestParam>(requestParam);
                    var objRouterRequestParams = JsonConvert.DeserializeObject(_requestParam.Data);//JsonConvert.DeserializeObject<DOCPMLRouterRequest>(str);
                    LoggedInUserId = !string.IsNullOrEmpty(_requestParam.UserId) ? Convert.ToInt64(_requestParam.UserId) : 0;
                    exResult = objStargateAuth.GetStargateConnection(ref client);

                    if (exResult == ExceptionTypes.Success)
                    {
                        string json = JsonConvert.SerializeObject(objRouterRequestParams, new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore
                        });
                        HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

                        CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(_requestParam.Trn_CaseID) ? Convert.ToInt64(_requestParam.Trn_CaseID) : 0, (long)HttpRequestType.POST, 0, json, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.outcome2_cpml_router_res);
                        HttpResponseMessage response = client.PostAsync(_objConfiguration.AppSettings.outcome2_cpml_router_res, content).Result;
                        string result = response.Content.ReadAsStringAsync().Result;
                        objBOCPMLRouter.SaveCPMLResponse(!string.IsNullOrEmpty(_requestParam.Trn_CaseID) ? Convert.ToInt64(_requestParam.Trn_CaseID) : 0, 0, LoggedInUserId, _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.outcome2_cpml_router_res, requestParam, result);

                        if (!response.IsSuccessStatusCode)
                        {
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(_requestParam.Trn_CaseID) ? Convert.ToInt64(_requestParam.Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Error, result, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.outcome2_cpml_router_res);
                            return BadRequest(result);
                        }
                        else
                        {
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(_requestParam.Trn_CaseID) ? Convert.ToInt64(_requestParam.Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Response, result, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.outcome2_cpml_router_res);
                        }

                        return Ok(result);
                    }
                    else
                    {
                        objBOCPMLRouter.SaveCPMLResponse(!string.IsNullOrEmpty(_requestParam.Trn_CaseID) ? Convert.ToInt64(_requestParam.Trn_CaseID) : 0, 0, LoggedInUserId, _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.outcome2_cpml_router_res, requestParam, _requestParam.Trn_CaseID + " - HICSCPMLRouter.post  - else of exResult - " + exResult.ToString());
                    }
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(_requestParam.Trn_CaseID) ? Convert.ToInt64(_requestParam.Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Error, _requestParam.Trn_CaseID + " - HICSCPMLRouter.post - " + ex.Message, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.outcome2_cpml_router_res);
                objBOCPMLRouter.SaveCPMLResponse(!string.IsNullOrEmpty(_requestParam.Trn_CaseID) ? Convert.ToInt64(_requestParam.Trn_CaseID) : 0, 0, LoggedInUserId, _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.outcome2_cpml_router_res, requestParam, ex.Message);
                return BadRequest(ex);
            }

        }

        [HttpGet]
        public IActionResult GetAllDetails(string Trn_CaseID, string DeterminationLkup, string Comments = "", string UserId= "")
        {
            LoggedInUserId = !string.IsNullOrEmpty(UserId) ? Convert.ToInt64(UserId) : 0;
            var postDeterminationtoCPML = PostFeedback(Trn_CaseID, DeterminationLkup, Comments) as OkObjectResult;

            var caseResult = GetCase(Trn_CaseID) as OkObjectResult;
            
            if (caseResult == null)
            {
                return BadRequest("Failed to retrieve case details.");
            }

            return Ok(new
            {
                CaseDetails = caseResult?.Value               
            });
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

        public IActionResult GetCase(string Trn_CaseID)
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

                        hicsCPMLAPIURL = _objConfiguration.AppSettings.outcome2_cpml_response_NextAction;

                        CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.GET, 0, Trn_CaseID,p_userid:LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + hicsCPMLAPIURL);
                        HttpResponseMessage response = client.PostAsync(hicsCPMLAPIURL, content).Result;
                        result = response.Content.ReadAsStringAsync().Result;

                        //Response logging
                        objBOCPMLRouter.SaveCPMLResponse(!string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, 0, LoggedInUserId, hicsCPMLAPIURL, json, result);
                        if (!response.IsSuccessStatusCode)
                        {
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.GET, (int)LogType.Error, result, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + hicsCPMLAPIURL);
                        }
                        else
                        {
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.GET, (int)LogType.Response, result, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + hicsCPMLAPIURL);
                        }
                        return Ok(result);
                    }
                }

                return BadRequest();

            }
            catch (Exception ex)
            {
                CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.GET, (int)LogType.Error, ex.Message, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress +  hicsCPMLAPIURL);
                return BadRequest(ex.Message);
            }

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

                        hicsCPMLAPIURL = _objConfiguration.AppSettings.outcome2_cpml_response_DetermineLkupId;

                        CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, 0, json, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + hicsCPMLAPIURL);

                        HttpResponseMessage response = client.PostAsync(hicsCPMLAPIURL, content).Result;
                        result = response.Content.ReadAsStringAsync().Result;

                        //Response logging
                        objBOCPMLRouter.SaveCPMLResponse(!string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, 0, LoggedInUserId, hicsCPMLAPIURL, json, result);

                        if (!response.IsSuccessStatusCode)
                        {
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Error, result, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + hicsCPMLAPIURL);
                        }
                        else
                        {
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Response, result, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + hicsCPMLAPIURL);
                        }
                        return Ok(result);
                    }
                }

                return BadRequest();

            }
            catch (Exception ex)
            {
                CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Error, ex.Message, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + hicsCPMLAPIURL);
                return BadRequest(ex.Message);
            }
        }

        public IActionResult PostFeedback(string Trn_CaseID, string LookupValue, string Comments = "")
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
                        Reqobj.useraction = LookupValue;
                        Reqobj.scenario_id = "31"; // Assuming scenario_id is fixed as "15" based on the original code
                        if (!string.IsNullOrEmpty(Comments))
                        {
                            Reqobj.Comments = Comments;
                        }

                        string json = JsonConvert.SerializeObject(Reqobj, new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore
                        });

                        hicsCPMLAPIURL = _objConfiguration.AppSettings.outcome2_cpml_response_Feedback;

                        HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
                        CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, 0, json, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + hicsCPMLAPIURL);
                        HttpResponseMessage response = client.PostAsync(hicsCPMLAPIURL, content).Result;
                        result = response.Content.ReadAsStringAsync().Result;

                        //Response logging
                        objBOCPMLRouter.SaveCPMLResponse(!string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, 0, LoggedInUserId, hicsCPMLAPIURL, json, result);

                        if (!response.IsSuccessStatusCode)
                        {
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Error, result, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + hicsCPMLAPIURL);
                        }
                        else
                        {
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Response, result, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + hicsCPMLAPIURL);
                        }
                        return Ok(result);
                    }
                }

                return BadRequest();

            }
            catch (Exception ex)
            {
                CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Error, ex.Message, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + hicsCPMLAPIURL);
                return BadRequest(ex.Message);
            }
        }
      
    }
}
