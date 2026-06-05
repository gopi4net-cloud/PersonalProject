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
using System.Collections.Generic;
using System.Dynamic;
using System.Net.Http;
using System.Text;

namespace ANGDDEAPI.Controllers
{

    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CPMLResponseController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DOConfiguration _objConfiguration;
        private readonly IMemoryCacheHelper _memoryCacheHelper;
        private readonly ICacheService _cache;

        public static long LoggedInUserId = 0;
        BOStargateAuthentication objStargateAuth = null;
        BOCPMLRouter objBOCPMLRouter = null;
        HttpClient client = null;
        public ExceptionTypes exResult;
        public readonly BOCommon _objBOCommon;
        public CPMLResponseController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
        {
            _cache = cache;
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _memoryCacheHelper = memoryCacheHelper;
            objStargateAuth = new BOStargateAuthentication(_memoryCacheHelper, _objConfiguration, _httpContextAccessor, bOCommon);
            objBOCPMLRouter = new BOCPMLRouter(_objConfiguration, _cache);
            _objBOCommon = bOCommon;
            client = new HttpClient();
        }

        [HttpGet]
        public IActionResult GetAllDetails(string Trn_CaseID, string DeterminationLkup, string Comments = "", bool fetchClaimDetails = false, string UserId = "", string OnBehalfOfLkup = "", bool IsPostServiceAppealReviewTask = false, bool IsCPMLPreTriage = false, bool IsGrievanceReviewTask = false)
        {
            LoggedInUserId = !string.IsNullOrEmpty(UserId) ? Convert.ToInt64(UserId) : 0;
            var postDeterminationtoCPML = PostFeedback(Trn_CaseID, DeterminationLkup, Comments) as OkObjectResult;
            if (!IsGrievanceReviewTask && !IsCPMLPreTriage)
            {
                var caseLogDataResult = GetCaseLogData(Trn_CaseID, OnBehalfOfLkup) as OkObjectResult;
            }

            var caseResult = GetCase(Trn_CaseID, OnBehalfOfLkup, IsPostServiceAppealReviewTask, IsCPMLPreTriage, IsGrievanceReviewTask) as OkObjectResult;

            if (!string.IsNullOrEmpty(DeterminationLkup) && DeterminationLkup == "18693161")
            {
                OkObjectResult clearCPMLCache = ClearCache(Trn_CaseID, IsPostServiceAppealReviewTask, IsGrievanceReviewTask) as OkObjectResult;
            }

            if (caseResult == null)
            {
                return BadRequest("Failed to retrieve case details.");
            }

            return Ok(new
            {
                CaseDetails = caseResult?.Value
            });
        }

        public IActionResult ClearCache(string Trn_CaseID, bool IsPostServiceAppealReviewTask = false, bool IsGrievanceReviewTask = false)
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
                        List<DOATSLookupMaster> lstATSLookupMaster = null;
                        _objBOCommon.GetATSLookupMaster(out lstATSLookupMaster);

                        var obj = new
                        {
                            CaseDetails = new { TrnCaseID = Trn_CaseID },
                            ScenarioID = _objBOCommon.GetLookupValueBasedonCon(lstATSLookupMaster, (long)CPMLScenarioId.ClearCacheCPML, 0),
                            isTestRun = _objBOCommon.GetLookupValueBasedonCon(lstATSLookupMaster, (long)CPMLScenarioId.ClearCacheCPML, 1).Equals("true", StringComparison.OrdinalIgnoreCase)
                        };

                        if (IsPostServiceAppealReviewTask)
                        {
                            obj = new
                            {
                                CaseDetails = new { TrnCaseID = Trn_CaseID },
                                ScenarioID = _objBOCommon.GetLookupValueBasedonCon(lstATSLookupMaster, (long)CPMLScenarioId.ClearProviderCacheCPML, 0),
                                isTestRun = _objBOCommon.GetLookupValueBasedonCon(lstATSLookupMaster, (long)CPMLScenarioId.ClearProviderCacheCPML, 1).Equals("true", StringComparison.OrdinalIgnoreCase)
                            };
                        }
                        else if (IsGrievanceReviewTask)
                        {
                            obj = new
                            {
                                CaseDetails = new { TrnCaseID = Trn_CaseID },
                                ScenarioID = _objBOCommon.GetLookupValueBasedonCon(lstATSLookupMaster, (long)CPMLScenarioId.ClearGrievanceCacheCPML, 0),
                                isTestRun = _objBOCommon.GetLookupValueBasedonCon(lstATSLookupMaster, (long)CPMLScenarioId.ClearGrievanceCacheCPML, 1).Equals("true", StringComparison.OrdinalIgnoreCase)
                            };
                        }

                        string json = JsonConvert.SerializeObject(obj, new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore
                        });
                        HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
                        CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, 0, json, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.cpml_clear_cache);
                        HttpResponseMessage response = client.PostAsync(_objConfiguration.AppSettings.cpml_clear_cache, content).Result;
                        result = response.Content.ReadAsStringAsync().Result;

                        if (!response.IsSuccessStatusCode)
                        {
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Error, result, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.cpml_clear_cache);
                        }
                        else
                        {
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Response, result, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.cpml_clear_cache);
                        }
                        return Ok(result);
                    }
                }

                return BadRequest();

            }
            catch (Exception ex)
            {
                CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Error, ex.Message, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.cpml_clear_cache);
                return BadRequest(ex.Message);
            }
        }




        [HttpGet]
        public IActionResult GetInitialCPMLDetails(string Trn_CaseID, string UserId = "", string OnBehalfOfLkup = "", bool IsPostServiceAppealReviewTask = false, bool IsCPMLPreTriage = false, bool IsGrievanceReviewTask = false)
        {
            LoggedInUserId = !string.IsNullOrEmpty(UserId) ? Convert.ToInt64(UserId) : 0;
            var _document = GetGlobleDocId(Trn_CaseID, OnBehalfOfLkup, IsPostServiceAppealReviewTask, IsCPMLPreTriage, IsGrievanceReviewTask) as OkObjectResult;
            var _getDeterminationLkup = GetDetermineLkupId(Trn_CaseID, OnBehalfOfLkup, IsPostServiceAppealReviewTask, IsGrievanceReviewTask) as OkObjectResult;
            var determinationLkupValue = _getDeterminationLkup?.Value?.ToString();

            return Ok(new
            {
                DeterminationLkup = long.TryParse(determinationLkupValue, out var result) ? result : 0,
                Doc360Details = _document?.Value
            });
        }

        public IActionResult GetCase(string Trn_CaseID, string OnbehalfLkup = "", bool IsPostServiceAppealReviewTask = false, bool IsCPMLPreTriage = false, bool IsGrievanceReviewTask = false)
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

                        string cpml_case_url = _objConfiguration.AppSettings.cpml_response_case;
                        if (IsCPMLPreTriage)
                        {
                            cpml_case_url = _objConfiguration.AppSettings.cpml_pretriage_response_case;
                        }
                        else if (IsPostServiceAppealReviewTask)
                        {
                            cpml_case_url = _objConfiguration.AppSettings.cpml_providerRA_response_case;
                        }
                        else if (IsGrievanceReviewTask)
                        {
                            cpml_case_url = _objConfiguration.AppSettings.cpml_grievanceRA_response_case;
                        }
                        else if (isMemberOnBehalfStatus(OnbehalfLkup))
                        {
                            cpml_case_url = _objConfiguration.AppSettings.member_cpml_response_case;
                        }

                        string cpml_response_caseRequestUri = cpml_case_url + "?case_id=" + Trn_CaseID;
                        string _LogMessage = _objConfiguration.AppSettings.StarGatewayCoreAddress + cpml_case_url;
                        CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.GET, 0, Trn_CaseID, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _LogMessage);
                        HttpResponseMessage response = client.GetAsync(cpml_response_caseRequestUri).Result;
                        result = response.Content.ReadAsStringAsync().Result;

                        if (!response.IsSuccessStatusCode)
                        {
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.GET, (int)LogType.Error, result, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _LogMessage);
                        }
                        else
                        {
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.GET, (int)LogType.Response, result, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _LogMessage);
                        }
                        return Ok(result);
                    }
                }

                return BadRequest();

            }
            catch (Exception ex)
            {
                CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.GET, (int)LogType.Error, ex.Message, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.cpml_response_case);
                return BadRequest(ex.Message);
            }

        }

        public IActionResult GetDetermineLkupId(string Trn_CaseID, string OnBehalfOfLkup = "", bool IsPostServiceAppealReviewTask = false, bool IsGrievanceReviewTask = false)
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

                        CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, 0, json, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.cpml_response_DetermineLkupId);

                        string _cpmldeterminationUrl = _objConfiguration.AppSettings.cpml_response_DetermineLkupId;


                        if (IsPostServiceAppealReviewTask)
                        {
                            _cpmldeterminationUrl = _objConfiguration.AppSettings.cpml_providerRA_response_DetermineLkupId;
                        }
                        else if (IsGrievanceReviewTask)
                        {
                            _cpmldeterminationUrl = _objConfiguration.AppSettings.cpml_grievance_response_DetermineLkupId;
                        }
                        else if (isMemberOnBehalfStatus(OnBehalfOfLkup))
                        {
                            _cpmldeterminationUrl = _objConfiguration.AppSettings.member_response_cpml_response_DetermineLkupId;
                        }

                        string _LogMessage = _objConfiguration.AppSettings.StarGatewayCoreAddress + _cpmldeterminationUrl;
                        HttpResponseMessage response = client.PostAsync(_cpmldeterminationUrl, content).Result;
                        result = response.Content.ReadAsStringAsync().Result;

                        if (!response.IsSuccessStatusCode)
                        {
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Error, result, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _LogMessage);
                        }
                        else
                        {
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Response, result, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _LogMessage);
                        }
                        return Ok(result);
                    }
                }

                return BadRequest();

            }
            catch (Exception ex)
            {
                CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Error, ex.Message, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.cpml_response_DetermineLkupId);
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
                        Reqobj.LookupValue = LookupValue;
                        if (!string.IsNullOrEmpty(Comments))
                        {
                            Reqobj.Comments = Comments;
                        }

                        string json = JsonConvert.SerializeObject(Reqobj, new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore
                        });
                        HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
                        CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, 0, json, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.cpml_response_Feedback);
                        HttpResponseMessage response = client.PostAsync(_objConfiguration.AppSettings.cpml_response_Feedback, content).Result;
                        result = response.Content.ReadAsStringAsync().Result;

                        if (!response.IsSuccessStatusCode)
                        {
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Error, result, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.cpml_response_Feedback);
                        }
                        else
                        {
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Response, result, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.cpml_response_Feedback);
                        }
                        return Ok(result);
                    }
                }

                return BadRequest();

            }
            catch (Exception ex)
            {
                CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Error, ex.Message, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.cpml_response_Feedback);
                return BadRequest(ex.Message);
            }
        }

        public IActionResult GetGlobleDocId(string Trn_CaseID, string OnBehalfOfLkup = "", bool IsPostServiceAppealReviewTask = false, bool IsCPMLPreTriage = false, bool IsGrievanceReviewTask = false)
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

                        string _cpmlGlobalDocUrl = _objConfiguration.AppSettings.cpml_response_GetGlobleDoc;

                        if (IsCPMLPreTriage)
                        {
                            _cpmlGlobalDocUrl = _objConfiguration.AppSettings.cpml_pretriage_globaldocref;
                        }
                        else
                        {
                            if (isMemberOnBehalfStatus(OnBehalfOfLkup))
                            {
                                _cpmlGlobalDocUrl = _objConfiguration.AppSettings.member_cpml_globaldocref;
                            }

                            if (IsPostServiceAppealReviewTask)
                            {
                                _cpmlGlobalDocUrl = _objConfiguration.AppSettings.cpml_providerRA_response_GetGlobleDoc;
                            }

                            if (IsGrievanceReviewTask)
                            {
                                _cpmlGlobalDocUrl = _objConfiguration.AppSettings.cpml_grievance_response_GetGlobleDoc;
                            }
                        }


                        CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, 0, json, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _cpmlGlobalDocUrl);
                        HttpResponseMessage response = client.PostAsync(_cpmlGlobalDocUrl, content).Result;
                        result = response.Content.ReadAsStringAsync().Result;

                        if (!response.IsSuccessStatusCode)
                        {
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Error, result, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _cpmlGlobalDocUrl);
                        }
                        else
                        {
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Response, result, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _cpmlGlobalDocUrl);
                        }
                        return Ok(result);
                    }
                }

                return BadRequest();

            }
            catch (Exception ex)
            {
                CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Error, ex.Message, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.cpml_response_GetGlobleDoc);
                return BadRequest(ex.Message);
            }
        }


        public bool isMemberOnBehalfStatus(string OnBehalfOfLkup)
        {
            string _onBehalfValue = OnBehalfOfLkup;

            bool isMemberOnBehalf = false;

            if (_onBehalfValue != null && _onBehalfValue != "")
            {
                string Providervalue = ((int)OnBehalfOfLkupEnum.Provider).ToString();
                string Membervalue = ((int)OnBehalfOfLkupEnum.Member).ToString();

                if (_onBehalfValue.ToString().Equals(Providervalue))
                {
                    isMemberOnBehalf = false;

                }
                else if (_onBehalfValue.ToString().Equals(Membervalue))
                {
                    isMemberOnBehalf = true;
                }
            }

            return isMemberOnBehalf;
        }





        public IActionResult GetCaseLogData(string Trn_CaseID, string OnbehalfLkup = "")
        {
            string result = string.Empty;
            long CpmlApilogID = 0;
            string cpml_case_url = _objConfiguration.AppSettings.cpml_response_getcaselogdata;
            string _LogMessage = string.Empty;
            bool isMember = false;
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

                        if (isMemberOnBehalfStatus(OnbehalfLkup))
                        {
                            isMember = true;
                            cpml_case_url = _objConfiguration.AppSettings.cpml_response_get_member_caselogdata;
                        }

                        string cpml_response_caseRequestUri = cpml_case_url + "?case_id=" + Trn_CaseID;
                        _LogMessage = _objConfiguration.AppSettings.StarGatewayCoreAddress + cpml_response_caseRequestUri;

                        CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, 0, Trn_CaseID, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _LogMessage);
                        HttpResponseMessage response = client.GetAsync(cpml_response_caseRequestUri).Result;
                        result = response.Content.ReadAsStringAsync().Result;
                        if (isMember)
                        {
                            var responseObject = JsonConvert.DeserializeObject<DOCPMLMemberCaseData>(result);
                            this.objBOCPMLRouter.CPMLLogMemberCaseData(!string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, responseObject);
                            if (!response.IsSuccessStatusCode)
                            {
                                CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Error, result, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _LogMessage);
                            }
                            else
                            {
                                CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Response, result, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _LogMessage);
                            }
                            return Ok(responseObject);
                        }
                        else
                        {

                            var responseObject = JsonConvert.DeserializeObject<DOCPMLCaseData>(result);
                            this.objBOCPMLRouter.CPMLLogCaseData(!string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, responseObject);
                            if (!response.IsSuccessStatusCode)
                            {
                                CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Error, result, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _LogMessage);
                            }
                            else
                            {
                                CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Response, result, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _LogMessage);
                            }
                            return Ok(responseObject);
                        }

                    }
                }

                return BadRequest();

            }
            catch (Exception ex)
            {
                CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Error, ex.Message, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _LogMessage);
                return BadRequest(ex.Message);
            }

        }


    }
}
