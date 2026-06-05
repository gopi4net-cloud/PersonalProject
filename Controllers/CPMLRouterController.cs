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
using System.Collections.Generic;


namespace ANGDDEAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CPMLRouterController : ControllerBase
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
        CPMLResponseController objcpmlResponseController = null;

        #region Constructor
        public CPMLRouterController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
        {
            _cache = cache;
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _memoryCacheHelper = memoryCacheHelper;
            _objBOGPSMemberDetails = new BOGPSMemberDetails(objConfiguration, _cache);
            _objBOCommon = bOCommon;
            objBOCPMLRouter = new BOCPMLRouter(_objConfiguration, _cache);
            objStargateAuth = new BOStargateAuthentication(_memoryCacheHelper, _objConfiguration, _httpContextAccessor, _objBOCommon);
            objcpmlResponseController = new CPMLResponseController(_httpContextAccessor, _objConfiguration, _memoryCacheHelper, _objBOCommon, _cache);
            client = new HttpClient();
        }
        #endregion

        [HttpGet]
        public ActionResult Get(string strSearch)
        {
            string result = string.Empty;
            string Trn_CaseID = string.Empty;
            string Trn_TaskFlowID = string.Empty;
            long CpmlApilogID = 0;
            try
            {
                if (!string.IsNullOrEmpty(strSearch))
                {
                    DOCPMLRouterRequest objRouterRequestParams = new DOCPMLRouterRequest();

                    objRouterRequestParams = JsonConvert.DeserializeObject<DOCPMLRouterRequest>(strSearch);
                    Trn_CaseID = objRouterRequestParams.CaseDetails.TrnCaseID;
                    Trn_TaskFlowID = objRouterRequestParams.CaseDetails.TrnTaskFlowID;

                    List<DOATSLookupMaster> lstATSLookupMaster = null;
                    _objBOCommon.GetATSLookupMaster(out lstATSLookupMaster);
                    if (objRouterRequestParams.IsCPMLPreTriage)
                    {
                        string _PreTriageScenarioId = _objBOCommon.GetLookupValueBasedonCon(lstATSLookupMaster, (long)CPMLScenarioId.PreTriageScenarioId, 0);
                        string _PreTriageScenarioVersionId = _objBOCommon.GetLookupValueBasedonCon(lstATSLookupMaster, (long)CPMLScenarioId.PreTriageScenarioVersionId, 0);
                        objRouterRequestParams.ScenarioID = _PreTriageScenarioId;
                        objRouterRequestParams.ScenarioVersionID = _PreTriageScenarioVersionId;

                    }
                    else if (objRouterRequestParams.IsPostServiceAppealReviewTask)
                    {
                        string PostServiceAppealReviewScenarioId = _objBOCommon.GetLookupValueBasedonCon(lstATSLookupMaster, (long)CPMLScenarioId.PerformPostServiceAppealReviewScenarioId, 0);
                        string PostServiceAppealReviewScenarioVersionId = _objBOCommon.GetLookupValueBasedonCon(lstATSLookupMaster, (long)CPMLScenarioId.PerformPostServiceAppealReviewScenarioVersion, 0);
                        objRouterRequestParams.ScenarioID = PostServiceAppealReviewScenarioId;
                        objRouterRequestParams.ScenarioVersionID = PostServiceAppealReviewScenarioVersionId;
                    }
                    else if (objRouterRequestParams.IsGrievanceReviewTask)
                    {
                        string GrievanceScenarioId = _objBOCommon.GetLookupValueBasedonCon(lstATSLookupMaster, (long)CPMLScenarioId.GrievanceScenarioId, 0);
                        string GrievanceScenarioVersionId = _objBOCommon.GetLookupValueBasedonCon(lstATSLookupMaster, (long)CPMLScenarioId.GrievanceScenarioVersion, 0);
                        objRouterRequestParams.ScenarioID = GrievanceScenarioId;
                        objRouterRequestParams.ScenarioVersionID = GrievanceScenarioVersionId;
                    }
                    else
                    {
                        if (objcpmlResponseController.isMemberOnBehalfStatus(objRouterRequestParams.OnBehalfOfLkup))
                        {
                            string MemberScenarioId = _objBOCommon.GetLookupValueBasedonCon(lstATSLookupMaster, (long)CPMLScenarioId.MemberScenarioId, 0);
                            string MemberScenarioVersionId = _objBOCommon.GetLookupValueBasedonCon(lstATSLookupMaster, (long)CPMLScenarioId.MemberScenarioVersionId, 0);

                            objRouterRequestParams.ScenarioID = MemberScenarioId;
                            objRouterRequestParams.ScenarioVersionID = MemberScenarioVersionId;
                        }
                        else
                        {
                            string ProviderScenarioId = _objBOCommon.GetLookupValueBasedonCon(lstATSLookupMaster, (long)CPMLScenarioId.ProviderScenarioId, 0);
                            string ProviderScenarioVersionId = _objBOCommon.GetLookupValueBasedonCon(lstATSLookupMaster, (long)CPMLScenarioId.ProviderScenarioVersionId, 0);

                            objRouterRequestParams.ScenarioID = ProviderScenarioId;
                            objRouterRequestParams.ScenarioVersionID = ProviderScenarioVersionId;
                        }

                    }


                    LoggedInUserId = !string.IsNullOrEmpty(objRouterRequestParams.UserId) ? Convert.ToInt64(objRouterRequestParams.UserId) : 0;
                    exResult = objStargateAuth.GetStargateConnection(ref client);

                    if (exResult == ExceptionTypes.Success)
                    {
                        string cpml_router_request = _objConfiguration.AppSettings.cpml_router_req;
                        var resolver = new JsonSerializePropertyIgnoreNested().Ignore(typeof(Casedetails), "TrnTaskFlowID");

                        string json = JsonConvert.SerializeObject(objRouterRequestParams, new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            ContractResolver = resolver
                        });
                        HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

                        CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, 0, json, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.cpml_router_req);
                        HttpResponseMessage response = client.PostAsync(cpml_router_request, content).Result;
                        result = response.Content.ReadAsStringAsync().Result;

                        if (!response.IsSuccessStatusCode)
                        {
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Error, result, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.cpml_router_req);
                            return BadRequest(result);
                        }
                        else
                        {
                            this.objBOCPMLRouter.CPMLvalidationStageStarted(LoggedInUserId, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, !string.IsNullOrEmpty(Trn_TaskFlowID) ? Convert.ToInt64(Trn_TaskFlowID) : 0);
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Response, result, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.cpml_router_req);

                            if (objRouterRequestParams.IsCPMLPreTriage)
                            {
                                this.objBOCPMLRouter.CPMLPreTriageStageStarted(LoggedInUserId, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, !string.IsNullOrEmpty(Trn_TaskFlowID) ? Convert.ToInt64(Trn_TaskFlowID) : 0);
                            }
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


        //remove below method not required after 31/5 prod release
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
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(_requestParam.Trn_CaseID) ? Convert.ToInt64(_requestParam.Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Error, ex.Message, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.cpml_router_res);
                return BadRequest(ex);
            }

        }


        [HttpPost]
        public IActionResult CPMLPost([FromForm] CPMLPOSTRequestParam _requestParam)
        {
            long CpmlApilogID = 0;
            try
            {
                if (_requestParam!=null)
                {
                    var objRouterRequestParams = JsonConvert.DeserializeObject(_requestParam.Data);

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
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(_requestParam.Trn_CaseID) ? Convert.ToInt64(_requestParam.Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Error, ex.Message, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.cpml_router_res);
                return BadRequest(ex);
            }

        }




    }


}
