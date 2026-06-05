using ANGDDEAPI.Common;
using ANGDDEAPIBO;
using ANGDDEAPIDO;
using ANGDDEAPIDO.Interface;
using ANGDDEAPIFoundation;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;


namespace ANGDDEAPI.Controllers
{
    [EnableCors("RRTGPSAPIPolicy")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class HICSCPMLRouterController : ControllerBase
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
        public HICSCPMLRouterController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
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
                    DOHICSCPMLRouter objRouterRequestParams = new DOHICSCPMLRouter();

                    objRouterRequestParams = JsonConvert.DeserializeObject<DOHICSCPMLRouter>(strSearch);
                    Trn_CaseID = objRouterRequestParams?.CaseDetails?.TrnCaseID;

                    LoggedInUserId = !string.IsNullOrEmpty(objRouterRequestParams.UserId) ? Convert.ToInt64(objRouterRequestParams.UserId) : 0;
                    exResult = objStargateAuth.GetStargateConnection(ref client);

                    if (exResult == ExceptionTypes.Success)
                    {
                        string cpml_router_request = _objConfiguration.AppSettings.cpml_router_req;
                        string json = JsonConvert.SerializeObject(objRouterRequestParams, new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore
                        });
                        HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
                        // Check
                        CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, 0, json, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.cpml_router_req);
                        HttpResponseMessage response = client.PostAsync(cpml_router_request, content).Result;
                        result = response.Content.ReadAsStringAsync().Result;
                        objBOCPMLRouter.SaveCPMLResponse(!string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, 0, LoggedInUserId, _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.cpml_router_req, strSearch, result);

                        if (!response.IsSuccessStatusCode)
                        {
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Error, result, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.cpml_router_req);
                            return BadRequest(result);
                        }
                        else
                        {
                            this.objBOCPMLRouter.CPMLvalidationStageStarted(LoggedInUserId, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0);
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Response, result, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.cpml_router_req);
                        }
                        return Ok(result);
                    }
                }

                return BadRequest();

            }
            catch (Exception ex)
            {
                CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Error, ex.Message, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.cpml_router_req);
                return BadRequest(ex.Message);
            }

        }

        [HttpPost]
        public IActionResult Postdata([FromBody] HICSRequestParam requestParam)
        {
            long CpmlApilogID = 0;
            HICSRequestParam _requestParam = new HICSRequestParam();
            try
            {
                if (requestParam?.Trn_CaseID != null)
                {
                    _requestParam = requestParam;
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

                        CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(_requestParam.Trn_CaseID) ? Convert.ToInt64(_requestParam.Trn_CaseID) : 0, (long)HttpRequestType.POST, 0, json, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.cpml_router_res);
                        HttpResponseMessage response = client.PostAsync(_objConfiguration.AppSettings.cpml_router_res, content).Result;
                        string result = response.Content.ReadAsStringAsync().Result;
                        objBOCPMLRouter.SaveCPMLResponse(!string.IsNullOrEmpty(_requestParam.Trn_CaseID) ? Convert.ToInt64(_requestParam.Trn_CaseID) : 0, 0, LoggedInUserId, _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.cpml_router_res, json, result);

                        if (!response.IsSuccessStatusCode)
                        {
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(_requestParam.Trn_CaseID) ? Convert.ToInt64(_requestParam.Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Error, result, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.cpml_router_res);
                            return BadRequest(result);
                        }
                        else
                        {
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(_requestParam.Trn_CaseID) ? Convert.ToInt64(_requestParam.Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Response, result, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.cpml_router_res);
                        }

                        return Ok(result);
                    }
                    else
                    {
                        objBOCPMLRouter.SaveCPMLResponse(!string.IsNullOrEmpty(_requestParam.Trn_CaseID) ? Convert.ToInt64(_requestParam.Trn_CaseID) : 0, 0, LoggedInUserId, _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.cpml_router_req, requestParam?.Data.ToString(), _requestParam.Trn_CaseID + " - HICSCPMLRouter.post  - else of exResult - " + exResult.ToString());
                    }
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(_requestParam.Trn_CaseID) ? Convert.ToInt64(_requestParam.Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Error, _requestParam.Trn_CaseID + " - HICSCPMLRouter.post - " + ex.Message, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.cpml_router_res);
                objBOCPMLRouter.SaveCPMLResponse(!string.IsNullOrEmpty(_requestParam.Trn_CaseID) ? Convert.ToInt64(_requestParam.Trn_CaseID) : 0, 0, LoggedInUserId, _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.cpml_router_req, requestParam?.Data.ToString(), ex.Message);
                return BadRequest(ex);
            }

        }

        [HttpGet]
        public async Task<IActionResult> Post(string Data, string Trn_CaseID, string UserId)
        {
            long CpmlApilogID = 0;
            try
            {
                if (!string.IsNullOrEmpty(Data) && !string.IsNullOrEmpty(Trn_CaseID))
                {
                    var objRouterRequestParams = JsonConvert.DeserializeObject(Data);//JsonConvert.DeserializeObject<DOCPMLRouterRequest>(str);
                    LoggedInUserId = !string.IsNullOrEmpty(UserId) ? Convert.ToInt64(UserId) : 0;
                    exResult = objStargateAuth.GetStargateConnection(ref client);

                    if (exResult == ExceptionTypes.Success)
                    {
                        string json = JsonConvert.SerializeObject(objRouterRequestParams, new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore
                        });
                        HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

                        CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, 0, json, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.cpml_router_res);
                        HttpResponseMessage response = client.PostAsync(_objConfiguration.AppSettings.cpml_router_res, content).Result;
                        string result = response.Content.ReadAsStringAsync().Result;
                        objBOCPMLRouter.SaveCPMLResponse(!string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, 0, LoggedInUserId, _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.cpml_router_res, json, result);

                        if (!response.IsSuccessStatusCode)
                        {
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Error, result, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.cpml_router_res);
                            return BadRequest(result);
                        }
                        else
                        {
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Response, result, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.cpml_router_res);
                        }

                        return Ok(result);
                    }
                    else
                    {
                        objBOCPMLRouter.SaveCPMLResponse(!string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, 0, LoggedInUserId, _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.cpml_router_req, Data, Trn_CaseID + " - HICSCPMLRouter.post  - else of exResult - " + exResult.ToString());
                    }
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Error, Trn_CaseID + " - HICSCPMLRouter.post - " + ex.Message, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.cpml_router_res);
                objBOCPMLRouter.SaveCPMLResponse(!string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, 0, LoggedInUserId, _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.cpml_router_req, Data, ex.Message);
                return BadRequest(ex);
            }

        }
    }


}
