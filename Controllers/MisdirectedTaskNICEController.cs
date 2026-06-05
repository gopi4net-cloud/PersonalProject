using ANGDDEAPIBO;
using ANGDDEAPIDO;
using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.Extensions.Configuration;
using ANGDDEAPI.Common;
using ANGDDEAPIDO.Interface;
using Microsoft.AspNetCore.Http;
using Amazon.Runtime.Internal.Util;
using Newtonsoft.Json;
using static ANGDDEAPIDO.DONICESearchRequest;
using System.DirectoryServices.Protocols;
using ANGDDEAPIFoundation;
using System.Collections.Generic;

namespace ANGDDEAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class MisdirectedTaskNICEController : ControllerBase
    {
        private readonly BONICEMisdirectedTask _bo;
        public static long LoggedInUserId = 0;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DOConfiguration _objConfiguration;
        private readonly IMemoryCacheHelper _memoryCacheHelper;
        private readonly BOCommon _objBOCommon;
        private readonly ICacheService _cache;
        private ExceptionTypes _result;



        public MisdirectedTaskNICEController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache, IConfiguration config)
        {
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _memoryCacheHelper = memoryCacheHelper;
            _objBOCommon = bOCommon;
            _cache = cache;
        }

       
        [HttpGet]
        public IActionResult BusinessExceptionCheck1(string details =null)
        {

            BONICEMisdirectedTask boNICEMisdirectedTask = new BONICEMisdirectedTask(_objConfiguration, _cache,_objBOCommon);
            DONICEMisdirectedTask taskdetails = null;
            try
            {
                if (!string.IsNullOrEmpty(details))
                {
                    taskdetails = new DONICEMisdirectedTask();
                    taskdetails = JsonConvert.DeserializeObject<DONICEMisdirectedTask>(details);

                    DOClaimInfoOutput output = boNICEMisdirectedTask.BusinessExceptionCheck1Query(taskdetails);

                    if (output != null)
                    {
                        return Ok(output);
                    }
                    else
                    {
                        return BadRequest();
                    }
                }
                return BadRequest("Details parameter is required.");
            }
            catch (Exception ex)
            {
                _objBOCommon.ErrorLog(LoggedInUserId, this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), ex.Message, ex.ToString());
                return BadRequest(ex.Message + "  Full Error:" + ex.ToString());
            }

        }

        [HttpGet]
        public IActionResult BusinessExceptionCheck2(string details = null)
        {

            BONICEMisdirectedTask boNICEMisdirectedTask = new BONICEMisdirectedTask(_objConfiguration, _cache,_objBOCommon);
            DONICEMisdirectedTask taskdetails = null;
            _result = new ExceptionTypes();

            try
            {
                if (!string.IsNullOrEmpty(details))
                {
                    taskdetails = new DONICEMisdirectedTask();
                    taskdetails = JsonConvert.DeserializeObject<DONICEMisdirectedTask>(details);

                    DOAuthorizationInfoOutput output = boNICEMisdirectedTask.BusinessExceptionCheck2Query(taskdetails);

                    if (output != null)
                    {
                        return Ok(output);
                    }
                    else
                    {
                        return BadRequest();
                    }
                }
                return BadRequest("Details parameter is required.");
            }
            catch (Exception ex)
            {
                _objBOCommon.ErrorLog(LoggedInUserId, this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), ex.Message, ex.ToString());
                return BadRequest(ex.Message + "  Full Error:" + ex.ToString());
            }

        }

        [HttpGet]
        public IActionResult BusinessExceptionCheck3(string details = null)
        {

            BONICEMisdirectedTask boNICEMisdirectedTask = new BONICEMisdirectedTask(_objConfiguration, _cache, _objBOCommon);
            DONICEMisdirectedTask taskdetails = null;
            _result = new ExceptionTypes();

            try
            {
                if (!string.IsNullOrEmpty(details))
                {
                    taskdetails = new DONICEMisdirectedTask();
                    taskdetails = JsonConvert.DeserializeObject<DONICEMisdirectedTask>(details);

                    DOContractUpdateCheckOutput output = boNICEMisdirectedTask.BusinessExceptionCheck3Query(taskdetails);

                    if (output != null)
                    {
                        return Ok(output);
                    }
                    else
                    {
                        return BadRequest();
                    }
                }
                return BadRequest("Details parameter is required.");
            }
            catch (Exception ex)
            {
                _objBOCommon.ErrorLog(LoggedInUserId, this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), ex.Message, ex.ToString());
                return BadRequest(ex.Message + "  Full Error:" + ex.ToString());
            }

        }
    }
}