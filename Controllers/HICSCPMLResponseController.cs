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
    public class HICSCPMLResponseController : Controller
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
        public string hicsCPMLAPIURL = "";
        public HICSCPMLResponseController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
        {
            _cache = cache;
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _memoryCacheHelper = memoryCacheHelper;
            objStargateAuth = new BOStargateAuthentication(_memoryCacheHelper, _objConfiguration, _httpContextAccessor, bOCommon);
            objBOCPMLRouter = new BOCPMLRouter(_objConfiguration, _cache);
            client = new HttpClient();
        }

        [HttpGet]
        public IActionResult GetAllDetails(string Trn_CaseID, string DeterminationLkup, string Comments = "", string UserId= "", bool HICSUPSFlag = false)
        {
            LoggedInUserId = !string.IsNullOrEmpty(UserId) ? Convert.ToInt64(UserId) : 0;
            var postDeterminationtoCPML = PostFeedback(Trn_CaseID, DeterminationLkup, Comments, HICSUPSFlag) as OkObjectResult;

            var caseResult = GetCase(Trn_CaseID, HICSUPSFlag) as OkObjectResult;
            
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
        public IActionResult GetInitialCPMLDetails(string Trn_CaseID, string UserId = "", bool HICSUPSFlag = false)
        {
            LoggedInUserId = !string.IsNullOrEmpty(UserId) ? Convert.ToInt64(UserId) : 0;            
            var _getDeterminationLkup = GetDetermineLkupId(Trn_CaseID, HICSUPSFlag) as OkObjectResult;
            var determinationLkupValue = _getDeterminationLkup?.Value?.ToString();

            return Ok(new
            {
                DeterminationLkup = long.TryParse(determinationLkupValue, out var result) ? result : 0                
            });
        }

        public IActionResult GetCase(string Trn_CaseID, bool HICSUPSFlag)
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

                        if (HICSUPSFlag)
                        {
                            hicsCPMLAPIURL = _objConfiguration.AppSettings.hics_ups_cpml_response_NextAction;
                        }
                        else
                        {
                            hicsCPMLAPIURL = _objConfiguration.AppSettings.hics_cpml_response_NextAction;
                        }

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
                CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.GET, (int)LogType.Error, ex.Message, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.cpml_response_case);
                return BadRequest(ex.Message);
            }

        }
      
        public IActionResult GetDetermineLkupId(string Trn_CaseID, bool HICSUPSFlag)
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

                        if (HICSUPSFlag)
                        {
                            hicsCPMLAPIURL = _objConfiguration.AppSettings.hics_ups_cpml_response_DetermineLkupId;
                        }
                        else
                        {
                            hicsCPMLAPIURL = _objConfiguration.AppSettings.hics_cpml_response_DetermineLkupId;
                        }

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
                CpmlApilogID = this.objBOCPMLRouter.InsertUpdateCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Error, ex.Message, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.cpml_response_DetermineLkupId);
                return BadRequest(ex.Message);
            }
        }

        public IActionResult PostFeedback(string Trn_CaseID, string LookupValue, string Comments = "", bool HICSUPSFlag=false)
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
                        Reqobj.scenario_id = "15"; // Assuming scenario_id is fixed as "15" based on the original code
                        if (!string.IsNullOrEmpty(Comments))
                        {
                            Reqobj.Comments = Comments;
                        }

                        string json = JsonConvert.SerializeObject(Reqobj, new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore
                        });

                        if (HICSUPSFlag)
                        {
                            hicsCPMLAPIURL = _objConfiguration.AppSettings.hics_ups_cpml_response_feedback;
                        }
                        else
                        {
                            hicsCPMLAPIURL = _objConfiguration.AppSettings.hics_cpml_response_feedback;
                        }

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
