using ANGDDEAPI.Common;
using ANGDDEAPIBO;
using ANGDDEAPIDO.Interface;
using ANGDDEAPIDO;
using ANGDDEAPIFoundation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.IO;
using System.Threading.Tasks;


namespace ANGDDEAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class GrievanceCPMLRouterController : ControllerBase
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
        public GrievanceCPMLRouterController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
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
                    DOGrievanceCPMLRouter objRouterRequestParams = new DOGrievanceCPMLRouter();

                    objRouterRequestParams = JsonConvert.DeserializeObject<DOGrievanceCPMLRouter>(strSearch);
                    Trn_CaseID = objRouterRequestParams.CaseDetails.TrnCaseID;

                    LoggedInUserId = !string.IsNullOrEmpty(objRouterRequestParams.UserId) ? Convert.ToInt64(objRouterRequestParams.UserId) : 0;
                    exResult = objStargateAuth.GetStargateConnection(ref client);

                    if (exResult == ExceptionTypes.Success)
                    {
                        string cpml_router_request = _objConfiguration.AppSettings.grv_cpml_router_req;
                        string json = JsonConvert.SerializeObject(objRouterRequestParams, new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore
                        });
                        HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
                        // Check
                        CpmlApilogID = this.objBOCPMLRouter.InsertUpdateGrievanceCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, 0, json,p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.ProdStargateAuthBaseURL + _objConfiguration.AppSettings.grv_cpml_router_req);
                        HttpResponseMessage response = client.PostAsync(cpml_router_request, content).Result;
                        result = response.Content.ReadAsStringAsync().Result;

                        if (!response.IsSuccessStatusCode)
                        {                           
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateGrievanceCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Error, result, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.ProdStargateAuthBaseURL + _objConfiguration.AppSettings.grv_cpml_router_req);
                            return BadRequest(result);
                        }
                        else
                        {
                           this.objBOCPMLRouter.CPMLvalidationStageStarted(LoggedInUserId, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0);
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateGrievanceCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Response, result, p_CpmlApiEndpoint: _objConfiguration.AppSettings.ProdStargateAuthBaseURL + _objConfiguration.AppSettings.grv_cpml_router_req);
                        }
                        return Ok(result);
                    }
                }

                return BadRequest();

            }
            catch (Exception ex)
            {
                CpmlApilogID = this.objBOCPMLRouter.InsertUpdateGrievanceCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Error, ex.Message, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.ProdStargateAuthBaseURL + _objConfiguration.AppSettings.grv_cpml_router_req);
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

                        CpmlApilogID = this.objBOCPMLRouter.InsertUpdateGrievanceCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, 0, json,p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.grv_cpml_router_res);
                        HttpResponseMessage response = client.PostAsync(_objConfiguration.AppSettings.grv_cpml_router_res, content).Result;
                        string result = response.Content.ReadAsStringAsync().Result;

                        if (!response.IsSuccessStatusCode)
                        {
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateGrievanceCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Error, result, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.grv_cpml_router_res);
                            return BadRequest(result);
                        }
                        else
                        {
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateGrievanceCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Response, result, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.grv_cpml_router_res);
                        }

                        return Ok(result);
                    }
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                this.objBOCPMLRouter.InsertUpdateGrievanceCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Error, ex.Message, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.grv_cpml_router_res);
                return BadRequest(ex);
            }

        }

    }


}
