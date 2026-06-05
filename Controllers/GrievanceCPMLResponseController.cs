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
    public class GrievanceCPMLResponseController : Controller
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
        public GrievanceCPMLResponseController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
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
        public IActionResult GetAllDetails(string Trn_CaseID, string DeterminationLkup, string Comments = "", string UserId = "", long? action = null)
          {
            LoggedInUserId = !string.IsNullOrEmpty(UserId) ? Convert.ToInt64(UserId) : 0;
            if (action == 1)
            {
                var postDeterminationtoCPML = PostFeedback(Trn_CaseID, DeterminationLkup, Comments) as OkObjectResult;
            }

            var caseResult = GetCase(Trn_CaseID, action, LoggedInUserId) as OkObjectResult;

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

        [HttpGet]
        public IActionResult GetGlobalDocID(string Trn_CaseID)
        {
            try
            {
                var globalDocID = GetCPMLGlobalDocId(Trn_CaseID) as OkObjectResult;
                var globalDocIDValue = globalDocID?.Value?.ToString();

                if (!string.IsNullOrEmpty(globalDocIDValue))
                {
                    // Parse the JSON response
                    var jsonObject = JObject.Parse(globalDocIDValue);
                    
                    return Ok(new
                    {
                        globaldocid = jsonObject["global_doc_id"]?.ToString() ?? "",
                        classname = jsonObject["class_name"]?.ToString() ?? "",
                        contenttype = jsonObject["content_type"]?.ToString() ?? "",
                        isProd = jsonObject["is_prod"]?.ToString() ?? "0",
                    });
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
                        Reqobj.TrnCaseID = Trn_CaseID;

                        string json = JsonConvert.SerializeObject(Reqobj, new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore
                        });
                        HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

                        CpmlApilogID = this.objBOCPMLRouter.InsertUpdateGrievanceCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.GET, 0, Trn_CaseID, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.grv_cpml_documenturl);
                        HttpResponseMessage response = client.PostAsync(_objConfiguration.AppSettings.grv_cpml_documenturl, content).Result;
                        result = response.Content.ReadAsStringAsync().Result;

                        if (!response.IsSuccessStatusCode)
                        {
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateGrievanceCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.GET, (int)LogType.Error, result, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.grv_cpml_documenturl);
                        }
                        else
                        {
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateGrievanceCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.GET, (int)LogType.Response, result, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.grv_cpml_documenturl);
                        }
                        return Ok(result);
                    }
                }

                return BadRequest();

            }
            catch (Exception ex)
            {
                CpmlApilogID = this.objBOCPMLRouter.InsertUpdateGrievanceCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.GET, (int)LogType.Error, result, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.grv_cpml_documenturl);
                return BadRequest(ex.Message);
            }
        }

        public IActionResult GetCase(string Trn_CaseID, long? action, long UserID)
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
                        
                        CpmlApilogID = this.objBOCPMLRouter.InsertUpdateGrievanceCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.GET, 0, Trn_CaseID,p_userid:LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.grv_cpml_response_case);                       
                        HttpResponseMessage response = client.PostAsync(_objConfiguration.AppSettings.grv_cpml_response_NextAction, content).Result;
                        result = response.Content.ReadAsStringAsync().Result;
                        if (action == 1)
                        {
                            objBOCPMLRouter.SaveFinalOutcome(result, Convert.ToInt64(Trn_CaseID), UserID);
                        }
                        
                        if (!response.IsSuccessStatusCode)
                        {
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateGrievanceCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.GET, (int)LogType.Error, result, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.grv_cpml_response_case);
                        }
                        else
                        {
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateGrievanceCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.GET, (int)LogType.Response, result, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.grv_cpml_response_case);
                        }
                        return Ok(result);
                    }
                }

                return BadRequest();

            }
            catch (Exception ex)
            {
                CpmlApilogID = this.objBOCPMLRouter.InsertUpdateGrievanceCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.GET, (int)LogType.Error, ex.Message, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.grv_cpml_response_case);
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

                        CpmlApilogID = this.objBOCPMLRouter.InsertUpdateGrievanceCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, 0, json, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.grv_cpml_response_DetermineLkupId);
                        HttpResponseMessage response = client.PostAsync(_objConfiguration.AppSettings.grv_cpml_response_DetermineLkupId, content).Result;
                        result = response.Content.ReadAsStringAsync().Result;

                        if (!response.IsSuccessStatusCode)
                        {
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateGrievanceCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Error, result, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.grv_cpml_response_DetermineLkupId);
                        }
                        else
                        {
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateGrievanceCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Response, result, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.grv_cpml_response_DetermineLkupId);
                        }
                        return Ok(result);
                    }
                }

                return BadRequest();

            }
            catch (Exception ex)
            {
                CpmlApilogID = this.objBOCPMLRouter.InsertUpdateGrievanceCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Error, ex.Message, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.grv_cpml_response_DetermineLkupId);
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
                        Reqobj.Comments = Comments;                       
                        string json = JsonConvert.SerializeObject(Reqobj, new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore
                        });
                        HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
                        CpmlApilogID = this.objBOCPMLRouter.InsertUpdateGrievanceCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, 0, json, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.grv_cpml_response_feedback);
                        HttpResponseMessage response = client.PostAsync(_objConfiguration.AppSettings.grv_cpml_response_feedback, content).Result;
                        result = response.Content.ReadAsStringAsync().Result;

                        if (!response.IsSuccessStatusCode)
                        {
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateGrievanceCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Error, result, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.grv_cpml_response_feedback);
                        }
                        else
                        {
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateGrievanceCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Response, result, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.grv_cpml_response_feedback);
                        }
                        return Ok(result);
                    }
                }

                return BadRequest();

            }
            catch (Exception ex)
            {
                CpmlApilogID = this.objBOCPMLRouter.InsertUpdateGrievanceCpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0, (long)HttpRequestType.POST, (int)LogType.Error, ex.Message, p_userid: LoggedInUserId, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.grv_cpml_response_feedback);
                return BadRequest(ex.Message);
            }
        }

    }
}
