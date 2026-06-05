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
using System.Net.WebSockets;


namespace ANGDDEAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class EnICPMLResponseController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DOConfiguration _objConfiguration;
        private readonly IMemoryCacheHelper _memoryCacheHelper;
        private readonly ICacheService _cache;

        BOStargateAuthentication objStargateAuth = null;
        BOCPMLRouter objBOCPMLRouter = null;
        HttpClient client = null;
        public ExceptionTypes exResult;
        public EnICPMLResponseController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
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
        public IActionResult GetAllDetails(string Trn_CaseID, string DeterminationLkup, string Comments = "", bool fetchClaimDetails = false, bool IsMember = false)
        {
            var postDeterminationtoCPML = PostFeedback(Trn_CaseID, DeterminationLkup, Comments) as OkObjectResult;

            var caseResult = GetCase(Trn_CaseID, IsMember) as OkObjectResult;

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
        public IActionResult GetInitialCPMLDetails(string Trn_CaseID, bool IsMember = false)
        {
            var _document = GetGlobleDocId(Trn_CaseID) as OkObjectResult;
            var _getDeterminationLkup = GetDetermineLkupId(Trn_CaseID, IsMember) as OkObjectResult;
            var determinationLkupValue = _getDeterminationLkup?.Value?.ToString();
            var DeterminationFloat = decimal.TryParse(determinationLkupValue,out var resp )?resp:0;

            var DeterminationLkup = DeterminationFloat!= null && DeterminationFloat>0 ? Convert.ToInt64(DeterminationFloat):0;
            return Ok(new
            {
                DeterminationLkup = DeterminationLkup,
                Doc360Details = _document?.Value
            });
        }

        public IActionResult GetCase(string Trn_CaseID,bool IsMember)
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
                        string cpml_response_caseReuestUri = IsMember? _objConfiguration.AppSettings.enicpmlmember_response_case + "?TrnCaseID=" + Trn_CaseID : _objConfiguration.AppSettings.enicpml_response_case + "?TrnCaseID=" + Trn_CaseID;

                        CpmlApilogID = this.objBOCPMLRouter.InsertUpdateENICpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0,0, (long)HttpRequestType.GET, 0, Trn_CaseID, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + (IsMember ? _objConfiguration.AppSettings.enicpmlmember_response_case : _objConfiguration.AppSettings.enicpml_response_case));
                        HttpResponseMessage response = client.GetAsync(cpml_response_caseReuestUri).Result;
                        result = response.Content.ReadAsStringAsync().Result;

                        if (!response.IsSuccessStatusCode)
                        {
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateENICpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0,0, (long)HttpRequestType.GET, (int)LogType.Error, result, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + (IsMember ? _objConfiguration.AppSettings.enicpmlmember_response_case : _objConfiguration.AppSettings.enicpml_response_case));
                        }
                        else
                        {
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateENICpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0,0, (long)HttpRequestType.GET, (int)LogType.Response, result, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + (IsMember ? _objConfiguration.AppSettings.enicpmlmember_response_case : _objConfiguration.AppSettings.enicpml_response_case));
                        }
                        return Ok(result);
                    }
                }

                return BadRequest();

            }
            catch (Exception ex)
            {
                CpmlApilogID = this.objBOCPMLRouter.InsertUpdateENICpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0,0, (long)HttpRequestType.GET, (int)LogType.Error, ex.Message, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + (IsMember ? _objConfiguration.AppSettings.enicpmlmember_response_case : _objConfiguration.AppSettings.enicpml_response_case));
                return BadRequest(ex.Message);
            }

        }

        public IActionResult GetDetermineLkupId(string Trn_CaseID, bool IsMember)
        {
            string result = string.Empty;
            long CpmlApilogID = 0;
            try
            {
                if (!string.IsNullOrEmpty(Trn_CaseID))
                {
                    client = new HttpClient();
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

                        CpmlApilogID = this.objBOCPMLRouter.InsertUpdateENICpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0,0, (long)HttpRequestType.POST, 0, json, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + (IsMember ? _objConfiguration.AppSettings.enicpmlmember_response_DetermineLkupId : _objConfiguration.AppSettings.enicpml_response_DetermineLkupId));

                        HttpResponseMessage response = client.PostAsync(IsMember? _objConfiguration.AppSettings.enicpmlmember_response_DetermineLkupId : _objConfiguration.AppSettings.enicpml_response_DetermineLkupId, content).Result;
                        
                        result = response.Content.ReadAsStringAsync().Result;

                        if (!response.IsSuccessStatusCode)
                        {
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateENICpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0,0, (long)HttpRequestType.POST, (int)LogType.Error, result, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + (IsMember ? _objConfiguration.AppSettings.enicpmlmember_response_DetermineLkupId : _objConfiguration.AppSettings.enicpml_response_DetermineLkupId));
                        }
                        else
                        {
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateENICpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0,0, (long)HttpRequestType.POST, (int)LogType.Response, result, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + (IsMember ? _objConfiguration.AppSettings.enicpmlmember_response_DetermineLkupId : _objConfiguration.AppSettings.enicpml_response_DetermineLkupId));
                        }
                        return Ok(result);
                    }
                }

                return BadRequest();

            }
            catch (Exception ex)
            {
                CpmlApilogID = this.objBOCPMLRouter.InsertUpdateENICpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0,0, (long)HttpRequestType.POST, (int)LogType.Error, ex.Message, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + (IsMember ? _objConfiguration.AppSettings.enicpmlmember_response_DetermineLkupId : _objConfiguration.AppSettings.enicpml_response_DetermineLkupId));
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
                        CpmlApilogID = this.objBOCPMLRouter.InsertUpdateENICpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0,0, (long)HttpRequestType.POST, 0, json, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.enicpml_response_Feedback);
                        HttpResponseMessage response = client.PostAsync(_objConfiguration.AppSettings.enicpml_response_Feedback, content).Result;
                        result = response.Content.ReadAsStringAsync().Result;

                        if (!response.IsSuccessStatusCode)
                        {
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateENICpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0,0, (long)HttpRequestType.POST, (int)LogType.Error, result, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.enicpml_response_Feedback);
                        }
                        else
                        {
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateENICpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0,0, (long)HttpRequestType.POST, (int)LogType.Response, result, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.enicpml_response_Feedback);
                        }
                        return Ok(result);
                    }
                }

                return BadRequest();

            }
            catch (Exception ex)
            {
                CpmlApilogID = this.objBOCPMLRouter.InsertUpdateENICpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0,0, (long)HttpRequestType.POST, (int)LogType.Error, ex.Message, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.enicpml_response_Feedback);
                return BadRequest(ex.Message);
            }
        }

        public IActionResult GetGlobleDocId(string Trn_CaseID)
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

                        CpmlApilogID = this.objBOCPMLRouter.InsertUpdateENICpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0,0, (long)HttpRequestType.POST, 0, json, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.enicpml_response_GetGlobleDoc);
                        HttpResponseMessage response = client.PostAsync(_objConfiguration.AppSettings.enicpml_response_GetGlobleDoc, content).Result;
                        result = response.Content.ReadAsStringAsync().Result;

                        if (!response.IsSuccessStatusCode)
                        {
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateENICpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0,0, (long)HttpRequestType.POST, (int)LogType.Error, result, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.enicpml_response_GetGlobleDoc);
                        }
                        else
                        {
                            CpmlApilogID = this.objBOCPMLRouter.InsertUpdateENICpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0,0, (long)HttpRequestType.POST, (int)LogType.Response, result, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.enicpml_response_GetGlobleDoc);
                        }
                        return Ok(result);
                    }
                }

                return BadRequest();

            }
            catch (Exception ex)
            {
                CpmlApilogID = this.objBOCPMLRouter.InsertUpdateENICpmlLog(CpmlApilogID, !string.IsNullOrEmpty(Trn_CaseID) ? Convert.ToInt64(Trn_CaseID) : 0,0, (long)HttpRequestType.POST, (int)LogType.Error, ex.Message, p_CpmlApiEndpoint: _objConfiguration.AppSettings.StarGatewayCoreAddress + _objConfiguration.AppSettings.enicpml_response_GetGlobleDoc);
                return BadRequest(ex.Message);
            }
        }
    }
}
