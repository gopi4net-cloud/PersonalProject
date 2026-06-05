using ANGDDEAPI.Common;
using ANGDDEAPIBO;
using ANGDDEAPIDO;
using ANGDDEAPIDO.Interface;
using ANGDDEAPIFoundation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using Newtonsoft.Json;
using ANGDDEAPIDO.UNET;
using log4net.Layout;
using System.Collections.Generic;
using NuGet.Protocol.Core.Types;
using NuGet.Common;
using ANGDDEAPIDO.ISET;
using System.Drawing;

namespace ANGDDEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class UNETController : ControllerBase
    {

        private readonly IHttpContextAccessor _httpContextAccessor;

        ExceptionTypes _resException;
        public string _username = string.Empty;
        public string ModuleName = "UNETController";
        private DOConfiguration _objConfiguration;
        private IMemoryCacheHelper _memoryCacheHelper;
        private BOCommon _objBOCommon;
        private ICacheService _cache;

        public UNETController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _memoryCacheHelper = memoryCacheHelper;
            _objBOCommon = bOCommon;
            _cache = cache;
        }

        [HttpGet]
        public ActionResult GetPMIDetails(string ProviderTin = null)
        {
            PMIInfo objDOPMIInfo = new PMIInfo();
            DOPMIRequest objDOPMIRequest = new DOPMIRequest()
            {

                Request = new PMIRequest()
                {
                    ApiConsumer = "TOPS",
                    reqRequiredFlds = new PMIReqRequiredFlds()
                    {
                        reqRespCode = "0000",
                        reqSystem = "ACW1",
                        reqPrvTin = ProviderTin,
                        reqViewName = "B5427PMI",
                        reqViewVersion = "01"
                    }
                }
            };

            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "GetPMIDetails", _username);

            try
            {
                UnetMethods objUnet = new UnetMethods(_memoryCacheHelper, _objConfiguration);
                _resException = objUnet.GetPMIDetails(objDOPMIRequest, ProviderTin, out objDOPMIInfo);
                if (_resException != ExceptionTypes.Success && _resException != ExceptionTypes.ZeroRecords)
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                if (source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(0, location, ex.Message, ex.ToString());
                }
                else
                {
                    _objBOCommon.LogError(location, source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                }
                return BadRequest(ex.Message);
            }
            return Ok(objDOPMIInfo);
        }

        [HttpGet]
        public ActionResult GetCMIDetails(string Policy = null, string Empid = null, string Empname = null, string Emprelation = null)
        {
            CMIInfo objDOCMIInfo = new CMIInfo();
            DOCMIRequest objDOCMIRequest = new DOCMIRequest()
            {

                Request = new CMIRequest()
                {
                    ApiConsumer = "TOPS",
                    reqRequiredFlds = new CMIReqRequiredFlds()
                    {
                        reqSearchPolicy = Policy,
                        reqSearchEmpid = Empid,
                        reqSearchEmpname = Empname,
                        reqSearchEmprelation = Emprelation
                    }
                }
            };

            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "GetCMIDetails", _username);

            try
            {
                UnetMethods objUnet = new UnetMethods(_memoryCacheHelper, _objConfiguration);
                _resException = objUnet.GetCMIDetails(objDOCMIRequest, out objDOCMIInfo);
                if (_resException != ExceptionTypes.Success && _resException != ExceptionTypes.ZeroRecords)
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                if (source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(0, location, ex.Message, ex.ToString());
                }
                else
                {
                    _objBOCommon.LogError(location, source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                }
                return BadRequest(ex.Message);
            }
            return Ok(objDOCMIInfo);
        }

        [HttpGet]
        public ActionResult GetMMIDetails(string Policy = null, string Plan = null, int page = 1)
        {
            List<MMIInfo> lstDOMMIInfo = new List<MMIInfo>();


            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "GetMMIDetails", _username);

            try
            {
                UnetMethods objUnet = new UnetMethods(_memoryCacheHelper, _objConfiguration);
                _resException = objUnet.GetMMIDetails(Policy, Plan, page, out lstDOMMIInfo);
                if (_resException != ExceptionTypes.Success && _resException != ExceptionTypes.ZeroRecords)
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                if (source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(0, location, ex.Message, ex.ToString());
                }
                else
                {
                    _objBOCommon.LogError(location, source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                }
                return BadRequest(ex.Message);
            }
            return Ok(lstDOMMIInfo);
        }

        [HttpGet]
        public ActionResult GetCEIDetails(string Policy = null, string Empid = null)
        {
            CEIInfo objDOCEIInfo = new CEIInfo();

            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "GetCEIDetails", _username);

            try
            {

                DOCEIRequest objDOCEIRequest = new DOCEIRequest()
                {

                    Request = new CEIRequest()
                    {
                        ApiConsumer = "TOPS",
                        reqRequiredFlds = new CEIReqrequiredflds()
                        {
                            reqSearchPolicy = Policy,
                            reqSearchEmpid = Empid
                        }
                    }
                };

                UnetMethods objUnet = new UnetMethods(_memoryCacheHelper, _objConfiguration);
                _resException = objUnet.GetCEIDetails(objDOCEIRequest, out objDOCEIInfo);
                if (_resException != ExceptionTypes.Success && _resException != ExceptionTypes.ZeroRecords)
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                if (source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(0, location, ex.Message, ex.ToString());
                }
                else
                {
                    _objBOCommon.LogError(location, source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                }
                return BadRequest(ex.Message);
            }
            return Ok(objDOCEIInfo);
        }

        [HttpGet]
        public ActionResult GetMRIDetails(string Policy = null, string Empid = null)
        {
            MRIInfo objDOMRIInfo = new MRIInfo();

            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "GetMRIDetails", _username);

            try
            {

                DOMRIRequest objDOMRIRequest = new DOMRIRequest()
                {

                    Request = new MRIRequest()
                    {
                        ApiConsumer = "TOPS",
                        reqRequiredFlds = new MRIReqrequiredflds()
                        {
                            reqSearchPolicy = Policy,
                            reqSearchEmpid = Empid
                        }
                    }
                };

                UnetMethods objUnet = new UnetMethods(_memoryCacheHelper, _objConfiguration);
                _resException = objUnet.GetMRIDetails(objDOMRIRequest, out objDOMRIInfo);
                if (_resException != ExceptionTypes.Success && _resException != ExceptionTypes.ZeroRecords)
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                if (source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(0, location, ex.Message, ex.ToString());
                }
                else
                {
                    _objBOCommon.LogError(location, source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                }
                return BadRequest(ex.Message);
            }
            return Ok(objDOMRIInfo);
        }

        [HttpGet]
        public ActionResult GetMHIDetails(string SearchIcn = null)
            {
            List<MHIInfo> objDOMHIInfo = new List<MHIInfo>();
            DOMHIResponse objDOMHIResponse = new DOMHIResponse();
            DOMHIRequest objDOMHIRequest = new DOMHIRequest()
            {

                Request = new MHIRequest()
                {
                    ApiConsumer = "TOPS",
                    reqRequiredFlds = new MHIReqRequiredflds()
                    {
                        reqSearchIcn = SearchIcn
                    }
                }
            };

            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "GetMHIDetails", _username);

            try
            {
                UnetMethods objUnet = new UnetMethods(_memoryCacheHelper, _objConfiguration);
                _resException = objUnet.GetMHIDetails(objDOMHIRequest, out objDOMHIInfo);

                if (_resException != ExceptionTypes.Success && _resException != ExceptionTypes.ZeroRecords)
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                if (source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(0, location, ex.Message, ex.ToString());
                }
                else
                {
                    _objBOCommon.LogError(location, source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                }
                return BadRequest(ex.Message);
            }
            return Ok(objDOMHIInfo);
        }
        [HttpGet]
        public ActionResult GetMHI_COMET(string SearchIcn = null)
        {
            MHIInfo mHIInfo = new MHIInfo();
            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "GetMHIComet", _username);
            string tToken = string.Empty;
            DOMHICometResponse objCometResponse = new DOMHICometResponse();
            DOMHIResponse objDOMHIResponse = new DOMHIResponse();
            MHIInfo objDOMHIInfo = new MHIInfo();   
            DOMHIRequest objDOMHIRequest = new DOMHIRequest()
            {

                Request = new MHIRequest()
                {
                    ApiConsumer = "TOPS",
                    reqRequiredFlds = new MHIReqRequiredflds()
                    {
                        reqSearchIcn = SearchIcn
                    }
                }
            };
            try
            {

                UnetMethods objUnet = new UnetMethods(_memoryCacheHelper, _objConfiguration);

                _resException = objUnet.GetMHIDirectDetails(objDOMHIRequest, out objDOMHIResponse);
                _resException = objUnet.GetTopsLogin(out tToken,objDOMHIResponse);
                _resException = objUnet.GetMHICometDetails(tToken, SearchIcn,objDOMHIResponse, out objDOMHIInfo);
               
                if (_resException != ExceptionTypes.Success && _resException != ExceptionTypes.ZeroRecords)
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                if (source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(0, location, ex.Message, ex.ToString());
                }
                else
                {
                    _objBOCommon.LogError(location, source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                }
                return BadRequest(ex.Message);
            }
            return Ok(objDOMHIInfo);
        }

        [HttpGet]
        public ActionResult GetNQIDetails(string ICN = null, string Draft = null)
        {
            NQIInfo objNQIInfo = new NQIInfo();
            DONQIResponse objNQIResponse = new DONQIResponse();
            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "GetNQIDetails", _username);

            try
            {
                UnetMethods objUnet = new UnetMethods(_memoryCacheHelper, _objConfiguration);
                _resException = objUnet.GetNQIDetails(ICN, Draft, out objNQIInfo);
                if (_resException != ExceptionTypes.Success && _resException != ExceptionTypes.ZeroRecords)
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                if (source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(0, location, ex.Message, ex.ToString());
                }
                else
                {
                    _objBOCommon.LogError(location, source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                }
                return BadRequest(ex.Message);
            }
            return Ok(objNQIInfo);
        }

        [HttpGet]
        public ActionResult GetIAIDetails(string Policy = null, string Empid = null, string DateReq = null)
        {

            DOIAIResponse objIAIResponse = new DOIAIResponse();
            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "GetIAIDetails", _username);

            try
            {
                UnetMethods objUnet = new UnetMethods(_memoryCacheHelper, _objConfiguration);
                _resException = objUnet.GetIAIDetails(Policy, Empid, DateReq, out objIAIResponse);
                if (_resException != ExceptionTypes.Success && _resException != ExceptionTypes.ZeroRecords)
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                if (source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(0, location, ex.Message, ex.ToString());
                }
                else
                {
                    _objBOCommon.LogError(location, source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                }
                return BadRequest(ex.Message);
            }
            return Ok(objIAIResponse);
        }

        [HttpGet]
        public ActionResult GetMXIDetails(string Policy = null, string Plan = null, string clss = null)
        {

            DOMXIResponse objMXIResponse = new DOMXIResponse();
            MXIInfo objMXIInfo = new MXIInfo();
            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "GetMXIDetails", _username);

            try
            {
                UnetMethods objUnet = new UnetMethods(_memoryCacheHelper, _objConfiguration);
                _resException = objUnet.GetMXIDetails(Policy, Plan, clss, out objMXIInfo);
                if (_resException != ExceptionTypes.Success && _resException != ExceptionTypes.ZeroRecords)
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                if (source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(0, location, ex.Message, ex.ToString());
                }
                else
                {
                    _objBOCommon.LogError(location, source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                }
                return BadRequest(ex.Message);
            }
            return Ok(objMXIInfo);
        }

        [HttpGet]
        public ActionResult GetMSIDetails(string Policy = null, string Plan = null, string Page = null)
        {

            DOMSIResponse objMSIResponse = new DOMSIResponse();
            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "GetMSIDetails", _username);

            try
            {
                UnetMethods objUnet = new UnetMethods(_memoryCacheHelper, _objConfiguration);
                _resException = objUnet.GetMSIDetails(Policy, Plan, Page, out objMSIResponse);
                if (_resException != ExceptionTypes.Success && _resException != ExceptionTypes.ZeroRecords)
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                if (source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(0, location, ex.Message, ex.ToString());
                }
                else
                {
                    _objBOCommon.LogError(location, source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                }
                return BadRequest(ex.Message);
            }
            return Ok(objMSIResponse);
        }

        [HttpGet]
        public ActionResult GetEPIDetails(string Empid = null)
        {
            EPIInfo objEPIInfo = new EPIInfo();
            DOEPIResponse objDOEPIResponse = new DOEPIResponse();
            DOEPIRequest objDOEPIRequest = new DOEPIRequest()
            {

                Request = new EPIRequest()
                {
                    ApiConsumer = "TOPS",
                    reqRequiredFlds = new EPIReqrequiredflds()
                    {
                        reqSearchEmpid = Empid
                    }
                }
            };

            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "GetEPIDetails", _username);

            try
            {
                UnetMethods objUnet = new UnetMethods(_memoryCacheHelper, _objConfiguration);
                _resException = objUnet.GetEPIDetails(objDOEPIRequest, out objEPIInfo);
                if (_resException != ExceptionTypes.Success && _resException != ExceptionTypes.ZeroRecords)
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                if (source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(0, location, ex.Message, ex.ToString());
                }
                else
                {
                    _objBOCommon.LogError(location, source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                }
                return BadRequest(ex.Message);
            }
            return Ok(objEPIInfo);
        }
        [HttpGet]
        public ActionResult GetPHIDetails(string SearchIcn = null, string SearchPolicy = null, string SearchEmpid = null,
            string SearchEmpname = null, string SearchEmprelation = null)
        {
            PHIInfo objDOPHIInfo = new PHIInfo();
            DOPHIResponse objDOPHIResponse = new DOPHIResponse();
            DOPHIRequest objDOPHIRequest = new DOPHIRequest()
            {

                Request = new PHIRequest()
                {
                    ApiConsumer = "TOPS",
                    reqRequiredFlds = new PHIReqrequiredflds()
                    {

                        reqSearchPolicy = SearchPolicy,
                        reqSearchEmpid = SearchEmpid,
                        reqSearchEmpname = SearchEmpname,
                        reqSearchEmprelation = SearchEmprelation
                    }
                }
            };

            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "GetPHIDetails", _username);

            try
            {
                UnetMethods objUnet = new UnetMethods(_memoryCacheHelper, _objConfiguration);
                _resException = objUnet.GetPHIDetails(objDOPHIRequest, out objDOPHIInfo);
                if (_resException != ExceptionTypes.Success && _resException != ExceptionTypes.ZeroRecords)
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                if (source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(0, location, ex.Message, ex.ToString());
                }
                else
                {
                    _objBOCommon.LogError(location, source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                }
                return BadRequest(ex.Message);
            }
            return Ok(objDOPHIInfo);
        }

        [HttpGet]
        public ActionResult GetPIIDetails(string SearchIcn = null, string SearchPolicy = null, string SearchEmpid = null,
           string SearchEmpname = null, string SearchEmprelation = null)
        {
            PIIInfo objDOPIIInfo = new PIIInfo();
            DOPIIResponse objDOPIIResponse = new DOPIIResponse();
            DOPIIRequest objDOPIIRequest = new DOPIIRequest()
            {

                Request = new PIIRequest()
                {
                    ApiConsumer = "TOPS",
                    reqRequiredFlds = new PIIReqrequiredflds()
                    {

                        reqSearchPolicy = SearchPolicy,
                        reqSearchEmpid = SearchEmpid,
                        reqSearchEmpname = SearchEmpname,
                        reqSearchEmpRelCd = SearchEmprelation
                    }
                }
            };

            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "GetPIIDetails", _username);

            try
            {
                UnetMethods objUnet = new UnetMethods(_memoryCacheHelper, _objConfiguration);
                _resException = objUnet.GetPIIDetails(objDOPIIRequest, out objDOPIIInfo);
                if (_resException != ExceptionTypes.Success && _resException != ExceptionTypes.ZeroRecords)
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                if (source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(0, location, ex.Message, ex.ToString());
                }
                else
                {
                    _objBOCommon.LogError(location, source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                }
                return BadRequest(ex.Message);
            }
            return Ok(objDOPIIInfo);
        }

        [HttpGet]
        public ActionResult GetOIIDetails(string SearchIcn = null, string SearchPolicy = null, string SearchEmpid = null,
           string SearchEmpname = null, string SearchEmprelation = null)
        {
            OIIInfo objDOOIIInfo = new OIIInfo();
            DOOIIResponse objDOOIIResponse = new DOOIIResponse();
            DOOIIRequest objDOOIIRequest = new DOOIIRequest()
            {

                Request = new OIIRequest()
                {
                    ApiConsumer = "TOPS",
                    reqRequiredFlds = new OIIReqrequiredflds()
                    {

                        reqSearchPolicy = SearchPolicy,
                        reqSearchEmpid = SearchEmpid,
                        reqSearchEmpname = SearchEmpname,
                        reqSearchEmprel = SearchEmprelation
                    }
                }
            };

            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "GetMHIDetails", _username);

            try
            {
                UnetMethods objUnet = new UnetMethods(_memoryCacheHelper, _objConfiguration);
                _resException = objUnet.GetOIIDetails(objDOOIIRequest, out objDOOIIInfo);
                if (_resException != ExceptionTypes.Success && _resException != ExceptionTypes.ZeroRecords)
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                if (source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(0, location, ex.Message, ex.ToString());
                }
                else
                {
                    _objBOCommon.LogError(location, source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                }
                return BadRequest(ex.Message);
            }
            return Ok(objDOOIIInfo);
        }

        [HttpGet]
        public ActionResult GetISIDetails(string SearchIcn = null, string SearchPolicy = null, string SearchEmpid = null,
          string SearchEmpname = null, string SearchEmprelation = null)
        {
            ISIInfo objDOISIInfo = new ISIInfo();
            DOISIResponse objDOISIResponse = new DOISIResponse();
            DOISIRequest objDOISIRequest = new DOISIRequest()
            {

                Request = new ISIRequest()
                {
                    ApiConsumer = "TOPS",
                    reqRequiredFlds = new ISIReqrequiredflds()
                    {

                        reqSearchPolicy = SearchPolicy,
                        reqSearchEmpid = SearchEmpid,
                        reqSearchEmpname = SearchEmpname,
                        reqSearchEmprelation = SearchEmprelation,
                        reqSearchICN = SearchIcn
                    }
                }
            };

            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "GetISIDetails", _username);

            try
            {
                UnetMethods objUnet = new UnetMethods(_memoryCacheHelper, _objConfiguration);
                _resException = objUnet.GetISIDetails(objDOISIRequest, out objDOISIInfo);
                if (_resException != ExceptionTypes.Success && _resException != ExceptionTypes.ZeroRecords)
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                if (source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(0, location, ex.Message, ex.ToString());
                }
                else
                {
                    _objBOCommon.LogError(location, source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                }
                return BadRequest(ex.Message);
            }
            return Ok(objDOISIInfo);
        }
        [HttpGet]
        public ActionResult GetBCIDetails(string SearchIcn = null, string SearchPolicy = null, string SearchEmpid = null,
       string SearchEmpname = null, string SearchEmpRelCd = null)
        {
            //MHIInfo objDOMHIInfo = new MHIInfo();
            DOBCIResponse objDOBCIResponse = new DOBCIResponse();
            DOBCIRequest objDOBCIRequest = new DOBCIRequest()
            {

                Request = new BCIRequest()
                {
                    ApiConsumer = "TOPS",
                    reqRequiredFlds = new BCIReqrequiredflds()
                    {

                        reqSearchPolicy = SearchPolicy,
                        reqSearchEmpid = SearchEmpid,
                        reqSearchEmpname = SearchEmpname,
                        reqSearchEmpRelCd = SearchEmpRelCd,
                    }
                }
            };

            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "GetBCIDetails", _username);

            try
            {
                UnetMethods objUnet = new UnetMethods(_memoryCacheHelper, _objConfiguration);
                _resException = objUnet.GetBCIDetails(objDOBCIRequest, out objDOBCIResponse);
                if (_resException != ExceptionTypes.Success && _resException != ExceptionTypes.ZeroRecords)
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                if (source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(0, location, ex.Message, ex.ToString());
                }
                else
                {
                    _objBOCommon.LogError(location, source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                }
                return BadRequest(ex.Message);
            }
            return Ok(objDOBCIResponse);
        }
        [HttpGet]
        public ActionResult GetCCIDetails(string SearchIcn = null, string SearchPolicy = null, string SearchEmpid = null,
       string SearchEmpname = null, string SearchEmprelation = null)
        {
           CCIInfo objDOCCIInfo = new CCIInfo();
            DOCCIResponse objDOCCIResponse = new DOCCIResponse();
            DOCCIRequest objDOCCIRequest = new DOCCIRequest()
            {

                Request = new CCIRequest()
                {
                    ApiConsumer = "TOPS",
                    reqRequiredFlds = new CCIReqrequiredflds()
                    {

                        reqSearchPolicy = SearchPolicy,
                        reqSearchEmpid = SearchEmpid,
                        reqSearchEmpname = SearchEmpname,
                        reqSearchEmprelation = SearchEmprelation,
                    }
                }
            };

            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "GetCCIDetails", _username);

            try
            {
                UnetMethods objUnet = new UnetMethods(_memoryCacheHelper, _objConfiguration);
                _resException = objUnet.GetCCIDetails(objDOCCIRequest, out objDOCCIInfo);
                if (_resException != ExceptionTypes.Success && _resException != ExceptionTypes.ZeroRecords)
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                if (source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(0, location, ex.Message, ex.ToString());
                }
                else
                {
                    _objBOCommon.LogError(location, source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                }
                return BadRequest(ex.Message);
            }
            return Ok(objDOCCIInfo);
        }
        [HttpGet]
        public ActionResult GetAHIDetails(string SearchIcn = null, string SearchPolicy = null, string SearchEmpid = null,
      string SearchEmpname = null, string SearchEmprelation = null,string reqSearchTin= null,string reqSearchLastDateOfService= null,string reqSearchFirstDateOfService= null)
        {
            AHIInfo objAHIInfo = new AHIInfo();
            DOAHIResponse objDOAHIResponse = new DOAHIResponse();
            DOAHIRequest objDOAHIRequest = new DOAHIRequest()
            {

                Request = new AHIRequest()
                {
                    ApiConsumer = "TOPS",
                    reqRequiredFlds = new AHIReqrequiredflds()
                    {

                        reqSearchPolicy = SearchPolicy,
                        reqSearchEmpid = SearchEmpid,
                        reqSearchEmpname = SearchEmpname,
                        reqSearchEmprelation = SearchEmprelation,
                    }
                }
            };
            if(reqSearchTin != null && reqSearchLastDateOfService!=null && reqSearchFirstDateOfService != null)
            {
                 objDOAHIRequest = new DOAHIRequest()
                {

                    Request = new AHIRequest()
                    {
                        ApiConsumer = "TOPS",
                        reqRequiredFlds = new AHIReqrequiredflds()
                        {

                            reqSearchPolicy = SearchPolicy,
                            reqSearchEmpid = SearchEmpid,
                            reqSearchEmpname = SearchEmpname,
                            reqSearchEmprelation = SearchEmprelation,
                            reqSearchTIN = reqSearchTin,
                            reqSearchLastDateOfService = reqSearchLastDateOfService,
                            reqSearchFirstDateOfService = reqSearchFirstDateOfService
                        }
                    }
                };
            }
            else if (reqSearchLastDateOfService != null && reqSearchFirstDateOfService != null)
            {
                objDOAHIRequest = new DOAHIRequest()
                {

                    Request = new AHIRequest()
                    {
                        ApiConsumer = "TOPS",
                        reqRequiredFlds = new AHIReqrequiredflds()
                        {

                            reqSearchPolicy = SearchPolicy,
                            reqSearchEmpid = SearchEmpid,
                            reqSearchEmpname = SearchEmpname,
                            reqSearchEmprelation = SearchEmprelation,
                            reqSearchLastDateOfService = reqSearchLastDateOfService,
                            reqSearchFirstDateOfService = reqSearchFirstDateOfService
                        }
                    }
                };
            }
            else if (reqSearchTin != null )
            {
                objDOAHIRequest = new DOAHIRequest()
                {

                    Request = new AHIRequest()
                    {
                        ApiConsumer = "TOPS",
                        reqRequiredFlds = new AHIReqrequiredflds()
                        {

                            reqSearchPolicy = SearchPolicy,
                            reqSearchEmpid = SearchEmpid,
                            reqSearchEmpname = SearchEmpname,
                            reqSearchEmprelation = SearchEmprelation,
                            reqSearchTIN = reqSearchTin,
                        }
                    }
                };
            }
            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "GetAHIDetails", _username);

            try
            {
                UnetMethods objUnet = new UnetMethods(_memoryCacheHelper, _objConfiguration);
                _resException = objUnet.GetAHIDetails(objDOAHIRequest, out objAHIInfo);
                if (_resException != ExceptionTypes.Success && _resException != ExceptionTypes.ZeroRecords)
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                if (source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(0, location, ex.Message, ex.ToString());
                }
                else
                {
                    _objBOCommon.LogError(location, source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                }
                return BadRequest(ex.Message);
            }
            return Ok(objAHIInfo);
        }
        [HttpGet]
        public ActionResult GetFCIDetails(string SearchIcn = null, string SearchPolicy = null, string SearchEmpid = null,
      string SearchEmpname = null, string SearchEmprelation = null)
        {
            //MHIInfo objDOMHIInfo = new MHIInfo();
            DOFCIResponse objDOFCIResponse = new DOFCIResponse();
            DOFCIRequest objDOFCIRequest = new DOFCIRequest()
            {

                Request = new FCIRequest()
                {
                    ApiConsumer = "TOPS",
                    reqRequiredFlds = new FCIReqrequiredflds()
                    {

                        reqSearchPolicy = SearchPolicy,
                        reqSearchEmpid = SearchEmpid,
                        reqSearchEmpname = SearchEmpname,
                        reqSearchEmprelation = SearchEmprelation,
                    }
                }
            };

            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "GetFCIDetails", _username);

            try
            {
                UnetMethods objUnet = new UnetMethods(_memoryCacheHelper, _objConfiguration);
                _resException = objUnet.GetFCIDetails(objDOFCIRequest, out objDOFCIResponse);
                if (_resException != ExceptionTypes.Success && _resException != ExceptionTypes.ZeroRecords)
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                if (source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(0, location, ex.Message, ex.ToString());
                }
                else
                {
                    _objBOCommon.LogError(location, source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                }
                return BadRequest(ex.Message);
            }
            return Ok(objDOFCIResponse);
        }
        [HttpGet]
        public ActionResult GetSCIDetails(string ICN = null, string Draft = null)
        {

            DOSCIResponse objSCIResponse = new DOSCIResponse();
            List<SCIInfo> objSCIInfo = new List<SCIInfo>(); 
            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "GetSCIDetails", _username);

            try
            {
                UnetMethods objUnet = new UnetMethods(_memoryCacheHelper, _objConfiguration);
                _resException = objUnet.GetSCIDetails(ICN, Draft, out objSCIInfo);
                if (_resException != ExceptionTypes.Success && _resException != ExceptionTypes.ZeroRecords)
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                if (source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(0, location, ex.Message, ex.ToString());
                }
                else
                {
                    _objBOCommon.LogError(location, source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                }
                return BadRequest(ex.Message);
            }
            return Ok(objSCIInfo);
        }

        [HttpGet]
        public ActionResult GetESIDetails(string SearchPolicy = null, string SearchEmpid = null)
        {
            ESIInfo objDOESIInfo = new ESIInfo();
            DOESIResponse objESIResponse = new DOESIResponse();
            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "GetESIDetails", _username);

            try
            {
                DOESIRequest objDOESIRequest = new DOESIRequest()
                {

                    Request = new ESIRequest()
                    {
                        ApiConsumer = "TOPS",
                        reqRequiredFlds = new ESIReqrequiredflds()
                        {

                            reqSearchPolicy = SearchPolicy,
                            reqSearchEmpid = SearchEmpid
                        }
                    }
                };
                UnetMethods objUnet = new UnetMethods(_memoryCacheHelper, _objConfiguration);
                _resException = objUnet.GetESIDetails(objDOESIRequest,  out objDOESIInfo);
                if (_resException != ExceptionTypes.Success && _resException != ExceptionTypes.ZeroRecords)
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                if (source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(0, location, ex.Message, ex.ToString());
                }
                else
                {
                    _objBOCommon.LogError(location, source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                }
                return BadRequest(ex.Message);
            }
            return Ok(objDOESIInfo);
        }
        [HttpGet]
        public ActionResult GetTOPSLogin()
        {

            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "GetTOPSLogin", _username);
            string tToken = string.Empty;

            try
            {

                UnetMethods objUnet = new UnetMethods(_memoryCacheHelper, _objConfiguration);
                _resException = objUnet.GetTopsLogin(out tToken,null);
                if (_resException != ExceptionTypes.Success && _resException != ExceptionTypes.ZeroRecords)
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                if (source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(0, location, ex.Message, ex.ToString());
                }
                else
                {
                    _objBOCommon.LogError(location, source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                }
                return BadRequest(ex.Message);
            }
            return Ok(tToken);
        }

        [HttpGet]
        public ActionResult GetTOPSLogout(string token)
        {

            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "GetTOPSLogout", _username);
            string tToken = string.Empty;

            try
            {

                UnetMethods objUnet = new UnetMethods(_memoryCacheHelper, _objConfiguration);
                _resException = objUnet.GetTopsLogout(token);
                if (_resException != ExceptionTypes.Success && _resException != ExceptionTypes.ZeroRecords)
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                if (source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(0, location, ex.Message, ex.ToString());
                }
                else
                {
                    _objBOCommon.LogError(location, source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                }
                return BadRequest(ex.Message);
            }
            return Ok(tToken);
        }
        [HttpGet]
        public ActionResult GetRHICOMET(string SearchIcn = null)
        {
            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "GetMHIComet", _username);
            string tToken = string.Empty;
            DOMHIResponse objDOMHIResponse = new DOMHIResponse();
            List<RHIInfo> objDORHIInfo = new List<RHIInfo>();
            DOMHIRequest objDOMHIRequest = new DOMHIRequest()
            {

                Request = new MHIRequest()
                {
                    ApiConsumer = "TOPS",
                    reqRequiredFlds = new MHIReqRequiredflds()
                    {
                        reqSearchIcn = SearchIcn?.Trim()
                    }
                }
            };
            try
            {

                UnetMethods objUnet = new UnetMethods(_memoryCacheHelper, _objConfiguration);

                _resException = objUnet.GetMHIDirectDetails(objDOMHIRequest, out objDOMHIResponse);
                _resException = objUnet.GetTopsLogin(out tToken, objDOMHIResponse);
                _resException = objUnet.GetRHICometDetails(tToken, SearchIcn?.Trim(), objDOMHIResponse, out objDORHIInfo);

                if (_resException != ExceptionTypes.Success && _resException != ExceptionTypes.ZeroRecords)
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                if (source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(0, location, ex.Message, ex.ToString());
                }
                else
                {
                    _objBOCommon.LogError(location, source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                }
                return BadRequest(ex.Message);
            }
            return Ok(objDORHIInfo);
        }
        [HttpGet]
        public ActionResult GetARICOMET(string SearchIcn = null)
        {
            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "GetMHIComet", _username);
            string tToken = string.Empty;
            DOMHIResponse objDOMHIResponse = new DOMHIResponse();
            ARIInfo objDOARIInfo = new ARIInfo();
            DOMHIRequest objDOMHIRequest = new DOMHIRequest()
            {

                Request = new MHIRequest()
                {
                    ApiConsumer = "TOPS",
                    reqRequiredFlds = new MHIReqRequiredflds()
                    {
                        reqSearchIcn = SearchIcn
                    }
                }
            };
            try
            {

                UnetMethods objUnet = new UnetMethods(_memoryCacheHelper, _objConfiguration);

                _resException = objUnet.GetMHIDirectDetails(objDOMHIRequest, out objDOMHIResponse);
                _resException = objUnet.GetTopsLogin(out tToken, objDOMHIResponse);
                _resException = objUnet.GetARICometDetails(tToken, SearchIcn, objDOMHIResponse, out objDOARIInfo);

                if (_resException != ExceptionTypes.Success && _resException != ExceptionTypes.ZeroRecords)
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                if (source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(0, location, ex.Message, ex.ToString());
                }
                else
                {
                    _objBOCommon.LogError(location, source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                }
                return BadRequest(ex.Message);
            }
            return Ok(objDOARIInfo);
        }
        [HttpGet]
        public ActionResult GetARIToPOICOMET(string authNbr = null, string authSrcInd = null,string cptCd=null, string EmpId=null)
        {
            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "GetARIToPOICOMET", _username);
            string tToken = string.Empty;
            DOARITOPOIResponse objDOAriToPoiResponse = new DOARITOPOIResponse();
            ARIPOIInfo aRIPOIInfo = new ARIPOIInfo();   
            try
            {

                UnetMethods objUnet = new UnetMethods(_memoryCacheHelper, _objConfiguration);
                _resException = objUnet.GetARIToPOICometDetails(authNbr, authSrcInd, cptCd, EmpId, out aRIPOIInfo);

                if (_resException != ExceptionTypes.Success && _resException != ExceptionTypes.ZeroRecords)
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                if (source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(0, location, ex.Message, ex.ToString());
                }
                else
                {
                    _objBOCommon.LogError(location, source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                }
                return BadRequest(ex.Message);
            }
            return Ok(aRIPOIInfo);
        }

        [HttpGet]
        public ActionResult GetARIToPSICOMET(string SearchIcn , string authNbr = null, string authSrcInd = null, string EmpId = null)
        {
            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "GetARIToPSICOMET", _username);
            string tToken = string.Empty;
            DOARITOPSIResponse objDOAriToPsiResponse = new DOARITOPSIResponse();
            ARIPSIInfo objARIPSIInfo=new ARIPSIInfo();
            DOMHIResponse objDOMHIResponse = new DOMHIResponse();
            DOMHIRequest objDOMHIRequest = new DOMHIRequest()
            {

                Request = new MHIRequest()
                {
                    ApiConsumer = "TOPS",
                    reqRequiredFlds = new MHIReqRequiredflds()
                    {
                        reqSearchIcn = SearchIcn
                    }
                }
            };
            try
            {
             
                UnetMethods objUnet = new UnetMethods(_memoryCacheHelper, _objConfiguration);
                _resException = objUnet.GetMHIDirectDetails(objDOMHIRequest, out objDOMHIResponse);
                _resException = objUnet.GetTopsLogin(out tToken, objDOMHIResponse);
                _resException = objUnet.GetARIToPSICometDetails(authNbr, authSrcInd, tToken, EmpId, out objARIPSIInfo);

                if (_resException != ExceptionTypes.Success && _resException != ExceptionTypes.ZeroRecords)
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                if (source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(0, location, ex.Message, ex.ToString());
                }
                else
                {
                    _objBOCommon.LogError(location, source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                }
                return BadRequest(ex.Message);
            }
            return Ok(objARIPSIInfo);
        }
        [HttpGet]
        public ActionResult GetRemarkCodeDetails(string RemarkCode = null)
        {
           
            DORemarkCodeResponse objRemarkCodeResponse = new DORemarkCodeResponse();
            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "GetRemarkCodeDetails", _username);

            try
            {
                UnetMethods objUnet = new UnetMethods(_memoryCacheHelper, _objConfiguration);
                _resException = objUnet.GetRemarkCodeDetails(RemarkCode, out objRemarkCodeResponse);
                if (_resException != ExceptionTypes.Success && _resException != ExceptionTypes.ZeroRecords)
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                if (source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(0, location, ex.Message, ex.ToString());
                }
                else
                {
                    _objBOCommon.LogError(location, source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                }
                return BadRequest(ex.Message);
            }
            return Ok(objRemarkCodeResponse);
        }
        [HttpGet]
        public ActionResult GetHPIDetails( string SearchPolicy = null, string SearchEmpid = null, string SearchIcn = null)
        {

            DOHPIResponse objHPIResponse = new DOHPIResponse();
            //SCIInfo objSCIInfo = new SCIInfo();
            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "GetHPIDetails", _username);

            try
            {
                UnetMethods objUnet = new UnetMethods(_memoryCacheHelper, _objConfiguration);
                _resException = objUnet.GetHPIDetails(SearchPolicy, SearchEmpid, SearchIcn, out objHPIResponse);
                if (_resException != ExceptionTypes.Success && _resException != ExceptionTypes.ZeroRecords)
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                if (source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(0, location, ex.Message, ex.ToString());
                }
                else
                {
                    _objBOCommon.LogError(location, source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                }
                return BadRequest(ex.Message);
            }
            return Ok(objHPIResponse);
        }
        [HttpGet]
        public ActionResult GetITIDetails(string SearchPolicy = null, string SearchEmpid = null, string SearchEmpName = null, string RelCd = null)
        {

            DOITIResponse objITIResponse = new DOITIResponse();
            //SCIInfo objSCIInfo = new SCIInfo();
            string location = ModuleName + System.Reflection.MethodBase.GetCurrentMethod().Name;
            string source = BOCommon.GetRefererURI(Request);
            _objBOCommon.LogError(location, source, 10001, "Controller Method", string.Empty, "GetITIDetails", _username);

            try
            {
                UnetMethods objUnet = new UnetMethods(_memoryCacheHelper, _objConfiguration);
                _resException = objUnet.GetITIDetails(SearchPolicy, SearchEmpid, SearchEmpName, RelCd, out objITIResponse);
                if (_resException != ExceptionTypes.Success && _resException != ExceptionTypes.ZeroRecords)
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                if (source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(0, location, ex.Message, ex.ToString());
                }
                else
                {
                    _objBOCommon.LogError(location, source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                }
                return BadRequest(ex.Message);
            }
            return Ok(objITIResponse);
        }


    }
}
