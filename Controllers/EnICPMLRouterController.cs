using ANGDDEAPI.Common;
using ANGDDEAPIBO;
using ANGDDEAPIDO.Interface;
using ANGDDEAPIDO;
using ANGDDEAPIFoundation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;
using System;
using Newtonsoft.Json.Linq;
using System.Net;

namespace ANGDDEAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class EnICPMLRouterController : Controller
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
        public EnICPMLRouterController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
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
            string Trn_TaskFlowID = string.Empty;
            bool IsCompleteCommand = false;
            long CpmlApilogID = 0;
            try
            {
                if (!string.IsNullOrEmpty(strSearch))
                {
                    DOCPMLRouterRequest objRouterRequestParams = new DOCPMLRouterRequest();

                    objRouterRequestParams = JsonConvert.DeserializeObject<DOCPMLRouterRequest>(strSearch);

                    var ParsedUpdateIssue = JObject.Parse(JsonConvert.SerializeObject(objRouterRequestParams));

                    // Remove the TrnTaskFlowID property from the CaseDetails object
                    var caseDetails = ParsedUpdateIssue["CaseDetails"] as JObject;
                    caseDetails?.Remove("TrnTaskFlowID");

                    string json = JsonConvert.SerializeObject(ParsedUpdateIssue);
                    string safeJson = WebUtility.HtmlEncode(json);

                    Trn_CaseID = objRouterRequestParams.CaseDetails.TrnCaseID;
                    Trn_TaskFlowID = objRouterRequestParams.CaseDetails.TrnTaskFlowID;

                    exResult = objStargateAuth.GetStargateConnection(ref client);

                    if (exResult == ExceptionTypes.Success)
                    {
                        string cpml_router_request = _objConfiguration.AppSettings.enicpml_router_req;
                        HttpContent content = new StringContent(safeJson, Encoding.UTF8, "application/json");

                        CpmlApilogID = this.objBOCPMLRouter.InsertUpdateENICpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, !string.IsNullOrEmpty(Trn_TaskFlowID) ? Convert.ToInt64(Trn_TaskFlowID) : 0, (long)HttpRequestType.POST, 0, safeJson, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.enicpml_router_req);
                        HttpResponseMessage response = client.PostAsync(cpml_router_request, content).Result;
                        result = response.Content.ReadAsStringAsync().Result;

                        // Check if the subcommand property is "completed"
                        var jsonResponse = JsonConvert.DeserializeObject<dynamic>(result);
                        if (jsonResponse != null && jsonResponse.command == "completed")
                        {
                           IsCompleteCommand = true;
                            this.objBOCPMLRouter.InsertENICpmlFactsLog(!string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, !string.IsNullOrEmpty(Trn_TaskFlowID) ? Convert.ToInt64(Trn_TaskFlowID) : 0, Convert.ToString(jsonResponse));
                        }

                        if (!response.IsSuccessStatusCode)
                        {
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateENICpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, !string.IsNullOrEmpty(Trn_TaskFlowID) ? Convert.ToInt64(Trn_TaskFlowID) : 0, (long)HttpRequestType.POST, (int)LogType.Error, result, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.enicpml_router_req);
                            return BadRequest(result);
                        }
                        else
                        {
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateENICpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, !string.IsNullOrEmpty(Trn_TaskFlowID) ? Convert.ToInt64(Trn_TaskFlowID) : 0, (long)HttpRequestType.POST, (int)LogType.Response, result, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.enicpml_router_req, IsCompleteCommand : IsCompleteCommand);
                        }
                        return Ok(result);
                    }
                }

                return BadRequest();

            }
            catch (Exception ex)
            {
                CpmlApilogID = this.objBOCPMLRouter.InsertUpdateENICpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, !string.IsNullOrEmpty(Trn_TaskFlowID) ? Convert.ToInt64(Trn_TaskFlowID) : 0, (long)HttpRequestType.POST, (int)LogType.Error, ex.Message, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.enicpml_router_req);
                return BadRequest("An error occurred while processing the request.");
            }

        }

        [HttpGet]
        public IActionResult Post(string requestParam)
        {
            long CpmlApilogID = 0;
            RequestParam _requestParam = new RequestParam();
            try
            {
                if (!string.IsNullOrEmpty(requestParam))
                {
                    _requestParam = JsonConvert.DeserializeObject<RequestParam>(requestParam);
                    var objRouterRequestParams = JsonConvert.DeserializeObject(_requestParam.Data);

                    exResult = objStargateAuth.GetStargateConnection(ref client);

                    if (exResult == ExceptionTypes.Success)
                    {
                        string json = JsonConvert.SerializeObject(objRouterRequestParams, new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore
                        });
                        HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

                        CpmlApilogID = this.objBOCPMLRouter.InsertUpdateENICpmlLog(CpmlApilogID, !string.IsNullOrEmpty(_requestParam.Trn_CaseID) ? Convert.ToInt64(_requestParam.Trn_CaseID) : 0, !string.IsNullOrEmpty(_requestParam.Trn_TaskFlowID) ? Convert.ToInt64(_requestParam.Trn_TaskFlowID) : 0, (long)HttpRequestType.POST, 0, json, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.enicpml_router_req);
                        HttpResponseMessage response = client.PostAsync(_objConfiguration.AppSettings.enicpml_router_res, content).Result;
                        string result = response.Content.ReadAsStringAsync().Result;
                        string encodedResult = WebUtility.HtmlEncode(result);

                        if (!response.IsSuccessStatusCode)
                        {
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateENICpmlLog(CpmlApilogID, !string.IsNullOrEmpty(_requestParam.Trn_CaseID) ? Convert.ToInt64(_requestParam.Trn_CaseID) : 0, !string.IsNullOrEmpty(_requestParam.Trn_TaskFlowID) ? Convert.ToInt64(_requestParam.Trn_TaskFlowID) : 0, (long)HttpRequestType.POST, (int)LogType.Error, result, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.enicpml_router_req);
                            return BadRequest(encodedResult);
                        }
                        else
                        {
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateENICpmlLog(CpmlApilogID, !string.IsNullOrEmpty(_requestParam.Trn_CaseID) ? Convert.ToInt64(_requestParam.Trn_CaseID) : 0, !string.IsNullOrEmpty(_requestParam.Trn_TaskFlowID) ? Convert.ToInt64(_requestParam.Trn_TaskFlowID) : 0, (long)HttpRequestType.POST, (int)LogType.Response, result, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.enicpml_router_req);
                        }

                        return Ok(encodedResult);
                    }
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                string encodedErrorMessage = WebUtility.HtmlEncode(ex.Message);
                this.objBOCPMLRouter.InsertUpdateENICpmlLog(CpmlApilogID, !string.IsNullOrEmpty(_requestParam.Trn_CaseID) ? Convert.ToInt64(_requestParam.Trn_CaseID) : 0, !string.IsNullOrEmpty(_requestParam.Trn_TaskFlowID) ? Convert.ToInt64(_requestParam.Trn_TaskFlowID) : 0, (long)HttpRequestType.POST, (int)LogType.Error, encodedErrorMessage, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.enicpml_router_req);
                return BadRequest(encodedErrorMessage);
            }

        }
    }
}
