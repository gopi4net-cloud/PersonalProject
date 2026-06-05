using ANGDDEAPI.Common;
using ANGDDEAPIBO;
using ANGDDEAPIDO;
using ANGDDEAPIDO.Interface;
using ANGDDEAPIFoundation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ANGDDEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class MedicalCodesController : ControllerBase
    {
        public static long LoggedInUserId = 0;        
        public string _username = string.Empty;
        string source = string.Empty;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DOConfiguration _objConfiguration;
        private readonly IMemoryCacheHelper _memoryCacheHelper;
        private readonly BOCommon _objBOCommon;
        private readonly ICacheService _cache;  
        public MedicalCodesController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _memoryCacheHelper = memoryCacheHelper;
            _objBOCommon = bOCommon;
            _cache = cache;
        }
        /// <summary>
        /// Get medical code description using code and code type
        /// </summary>
        /// <param name="objDOMedicalCodes"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult GetCodeDecription_Soap(DOMedicalCodes objDOMedicalCodes)
        {
            source = BOCommon.GetRefererURI(Request);
            try
            {
                //DOMedicalCodes objDOMedicalCodes = new DOMedicalCodes();
                //objDOMedicalCodes.Code = code;
                //objDOMedicalCodes.CodeType = codetype;

                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                if (objDOMedicalCodes != null && !string.IsNullOrEmpty(objDOMedicalCodes.Code) && !string.IsNullOrEmpty(objDOMedicalCodes.CodeType))
                {
                   // MedicalCodesMethods objMedicalCodesMethods = new MedicalCodesMethods();
                    objDOMedicalCodes.Code = objDOMedicalCodes.Code.Trim();
                    objDOMedicalCodes.CodeType = objDOMedicalCodes.CodeType.Trim();
                  //  objDOMedicalCodes = objMedicalCodesMethods.SelectMedicalCodes_Soap(objDOMedicalCodes);

                    if (!string.IsNullOrEmpty(objDOMedicalCodes.ErrorMessage))
                    {
                        throw new Exception(objDOMedicalCodes.ErrorMessage);
                    }
                }
                return Ok(objDOMedicalCodes);
            }
            catch (Exception ex)
            {
                if (source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(LoggedInUserId, this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), ex.Message, ex.ToString());
                }
                else
                {
                    _objBOCommon.LogError(this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                }
                return BadRequest();
            }
        }
        
        [HttpPost]
        public IActionResult GetCodeDecription([FromForm] DOMedicalCodes objDOMedicalCodes)
        {
            source = BOCommon.GetRefererURI(Request);
            try
            {
                //DOMedicalCodes objDOMedicalCodes = new DOMedicalCodes();
                //objDOMedicalCodes.Code = code;
                //objDOMedicalCodes.CodeType = codetype;
                DOMedicalCodesRequest objDOMedicalCodesRequest = new DOMedicalCodesRequest();
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Request", BOCommon.JsonConvertObjectToString(objDOMedicalCodes), _username);
                if (objDOMedicalCodes != null && !string.IsNullOrEmpty(objDOMedicalCodes.Code) && !string.IsNullOrEmpty(objDOMedicalCodes.CodeType))
                {
                    MedicalCodesMethods objMedicalCodesMethods = new MedicalCodesMethods(_memoryCacheHelper, _objConfiguration);
                    List<MedicalCodeType> lstmedicalcodetype = null;
                    lstmedicalcodetype = new List<MedicalCodeType>();
                    MedicalCodeType objMedicalCodeType = new MedicalCodeType();
                    objMedicalCodeType.code = objDOMedicalCodes.Code.Trim();
                    objMedicalCodeType.codeType = objDOMedicalCodes.CodeType.Trim();
                    lstmedicalcodetype.Add(objMedicalCodeType);
                    objDOMedicalCodesRequest.medicalCodeTypes = lstmedicalcodetype;
                    codingSolutionsSystemParameters objcodingSolutionsSystemParameters = new codingSolutionsSystemParameters();
                    objcodingSolutionsSystemParameters.username = "string";
                    objcodingSolutionsSystemParameters.password = "string";
                    controlModifier objcontrolModifier = new controlModifier();
                    objcontrolModifier.codingSolutionsSystemParameters = objcodingSolutionsSystemParameters;
                    objDOMedicalCodesRequest.controlModifier = objcontrolModifier;
                    objDOMedicalCodes = objMedicalCodesMethods.SelectMedicalCodes_Rest(objDOMedicalCodesRequest);

                    if (!string.IsNullOrEmpty(objDOMedicalCodes.ErrorMessage))
                    {
                        throw new Exception(objDOMedicalCodes.ErrorMessage);
                    }
                }
                _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Response", BOCommon.JsonConvertObjectToString(objDOMedicalCodes), _username);

                return Ok(objDOMedicalCodes);
            }
            catch (Exception ex)
            {
                if (source.Contains("DDE"))
                {
                    _objBOCommon.ErrorLog(LoggedInUserId, this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), ex.Message, ex.ToString());
                }
                else
                {
                    _objBOCommon.LogError(this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                }
                return BadRequest();
            }
        }
    }
}
