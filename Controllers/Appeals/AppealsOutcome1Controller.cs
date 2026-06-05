using ANGDDEAPI.Common;
using ANGDDEAPIBO;
using ANGDDEAPIDO;
using ANGDDEAPIDO.Interface;
using ANGDDEAPIFoundation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Dynamic;
using System.Net.Http;
using System.Text;
namespace ANGDDEAPI.Controllers.Appeals
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AppealsOutcome1Controller : ControllerBase
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
        BOAppealsOutcome objBOCPMLRouter = null;
        BOStargateAuthentication objStargateAuth = null;
        HttpClient client = null;

        #region Constructor
        public AppealsOutcome1Controller(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
        {
            _cache = cache;
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _memoryCacheHelper = memoryCacheHelper;
            _objBOGPSMemberDetails = new BOGPSMemberDetails(objConfiguration, _cache);
            _objBOCommon = bOCommon;
            objBOCPMLRouter = new BOAppealsOutcome(_objConfiguration, _cache);
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
                    DOOutcome1UI objRouterRequestParams = new DOOutcome1UI();

                    objRouterRequestParams = JsonConvert.DeserializeObject<DOOutcome1UI>(strSearch);
                    Trn_CaseID = objRouterRequestParams?.case_id;

                    LoggedInUserId = !string.IsNullOrEmpty(objRouterRequestParams.UserId) ? Convert.ToInt64(objRouterRequestParams.UserId) : 0;
                    exResult = objStargateAuth.GetStargateConnection(ref client);

                    if (exResult == ExceptionTypes.Success)
                    {
                        string outcome1_cpml_router_req = _objConfiguration.AppSettings.outcome1_cpml_router_req;
                        string json = JsonConvert.SerializeObject(objRouterRequestParams, new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore
                        });
                        HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
                        CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, 0, json, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.outcome1_cpml_router_req);
                        HttpResponseMessage response = client.PostAsync(outcome1_cpml_router_req, content).Result;
                        result = response.Content.ReadAsStringAsync().Result;
                        objBOCPMLRouter.SaveOutcome1CPMLResponse(!string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, LoggedInUserId, _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.dynamic_pre_ui, strSearch, result);

                        if (!response.IsSuccessStatusCode)
                        {
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Error, result, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.outcome1_cpml_router_req);
                            return BadRequest(result);
                        }
                        else
                        {
                            this.objBOCPMLRouter.CPMLvalidationStageStarted(LoggedInUserId, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0);
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Response, result, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.outcome1_cpml_router_req);
                        }
                        return Ok(result);
                    }
                }

                return BadRequest();

            }
            catch (Exception ex)
            {
                CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Error, ex.Message, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.outcome1_cpml_router_req);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public ActionResult GetGlobalDocID(string Trn_CaseID)
        {
            try
            {
                var globalDocID = GetCPMLGlobalDocId(Trn_CaseID) as OkObjectResult;
                var globalDocIDValue = globalDocID?.Value?.ToString();

                if (!string.IsNullOrEmpty(globalDocIDValue))
                {
                    // Parse the JSON response
                    var jsonObject = JObject.Parse(globalDocIDValue);
                    var responseObject = jsonObject["response"];

                    return Ok(responseObject.ToString());
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
                // If JSON parsing fails, return empty values
                return Ok(new
                {
                    globaldocid = "",
                    classname = "",
                    contenttype = ""
                });
            }
            catch (Exception)
            {
                // For any other exceptions, return empty values
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
                        Reqobj.case_id = Trn_CaseID;

                        string json = JsonConvert.SerializeObject(Reqobj, new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore
                        });
                        HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

                        CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.GET, 0, Trn_CaseID, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.outcome1_globaldocapi);
                        HttpResponseMessage response = client.PostAsync(_objConfiguration.AppSettings.outcome1_globaldocapi, content).Result;
                        result = response.Content.ReadAsStringAsync().Result;

                        if (!response.IsSuccessStatusCode)
                        {
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.GET, (int)LogType.Error, result, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.outcome1_globaldocapi);
                        }
                        else
                        {
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.GET, (int)LogType.Response, result, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.outcome1_globaldocapi);
                        }
                        return Ok(result);
                    }
                }

                return BadRequest();

            }
            catch (Exception ex)
            {
                CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.GET, (int)LogType.Error, result, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.outcome1_globaldocapi);
                return BadRequest(ex.Message);
            }
        }
    }
}
