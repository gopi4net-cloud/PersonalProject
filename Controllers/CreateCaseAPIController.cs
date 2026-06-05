using ANGDDEAPIFoundation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System;
using ANGDDEAPIBO;
using ANGDDEAPIDO;
using ANGDDEAPIDO.Interface;
using static MongoDB.Bson.Serialization.Serializers.SerializerHelper;

namespace ANGDDEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class CreateCaseAPIController : ControllerBase
    {
        public string _username = string.Empty;

        BOCreateCase _objBOCreateCase;
        ExceptionTypes _resException;
        string errorMessage;
        long _currentUserId;
        private ICacheService _cacheService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly BOCommon _objBOCommon;
        string source = string.Empty;
       
        public CreateCaseAPIController(ICacheService cacheService)
        {
            _cacheService = cacheService;
            _objBOCreateCase = new BOCreateCase(_cacheService);

        }

        [HttpGet]
        public ActionResult GetDuplicateCases(string MemberId, string MBIorHICN, string HealthExchangeID = null)
        {
            var isDupCheckFromUI = false;
            List<DODuplicateCase> lstObjDOCaseStars = new List<DODuplicateCase>();

           // _username = _httpContextAccessor.HttpContext.User.Identity.Name;
            List<DODuplicateCase> lstDODuplicateCase = new List<DODuplicateCase>();
            //_objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Request", MemberId, _username);
            try
            {

                _resException = _objBOCreateCase.GetSuspectDuplicateCasesInSTAR(MemberId, MBIorHICN, out lstObjDOCaseStars);
                if (_resException != ExceptionTypes.Success && _resException != ExceptionTypes.ZeroRecords)
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok(lstDODuplicateCase);
        }

        [HttpGet]
        public ActionResult GetSuspectDuplicateCases(string MBIorHICN, string ProviderTaxID = null, long? ComplainantTypeLkup = 0)
        {
            List<DODuplicateCase> lstDODuplicateCase = new List<DODuplicateCase>();
            //  _logger.Trace(this.GetType().Name + " ", "CaseAPI Controller Information", new { MBIorHICN = MBIorHICN }, _objBOCreateCase.CurrentUserId);
            var isDupCheckFromUI = false;
            List<DODuplicateCase> lstObjDOCaseCitrus = new List<DODuplicateCase>();

            try
            {

                _resException = _objBOCreateCase.GetSuspectDuplicateCasesInCitrus(MBIorHICN, out lstDODuplicateCase,  ComplainantTypeLkup, ProviderTaxID);
                if (_resException != ExceptionTypes.Success && _resException != ExceptionTypes.ZeroRecords)
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok(lstDODuplicateCase);
        }

        [HttpGet]
        public ActionResult GetDuplicateCheck([FromQuery] VMDuplicateCheck objDuplicateCheck)
        {
            List<DODuplicateCase> lstDODuplicateCase = new List<DODuplicateCase>();
           // _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Request", objDuplicateCheck.ALJCaseID, _username);
            try
            {
                _resException = _objBOCreateCase.GetDuplicatesInCitrusForALJ(objDuplicateCheck.ALJCaseID, objDuplicateCheck.UniqueImageID, objDuplicateCheck.IntakeChannelLkup, out lstDODuplicateCase);
                if (_resException != ExceptionTypes.Success && _resException != ExceptionTypes.ZeroRecords)
                {
                    return BadRequest();
                }
            }
            catch (Exception ex) 
            {
                return BadRequest(ex.Message);  
            }

            return Ok(lstDODuplicateCase);
        }
        

        [HttpGet]
        public ActionResult AddStarsData(string StarsCaseID)
        {

            //_objBOCreateCase.CurrentTeamLkup = (long)_userService.CurrentUser.TeamLkup;


            //_objBOCreateCase.CurrentUserId = _userService.CurrentUser.ADM_UserInfoId;
            List<DOStarsCase> lstDOStarsCase = new List<DOStarsCase>();
            //_logger.Trace(this.GetType().Name + " ", "CaseAPI Controller Information", new { StarsCaseID = StarsCaseID }, _objBOCreateCase.CurrentUserId);
            _username = _httpContextAccessor.HttpContext.User.Identity.Name;
            _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Request", StarsCaseID, _username);

            try
            {

                _resException = _objBOCreateCase.GetStarCasesForALJ(StarsCaseID, out lstDOStarsCase);

                if (_resException != ExceptionTypes.Success && _resException != ExceptionTypes.ZeroRecords)
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok(lstDOStarsCase);
        }

        [HttpGet]
        public ActionResult GetSourceSysFromCaliber(string ALJCaseID)
        {

            //_objBOCreateCase.CurrentTeamLkup = (long)_userService.CurrentUser.TeamLkup;


            //_objBOCreateCase.CurrentUserId = _userService.CurrentUser.ADM_UserInfoId;
            DOCaliber objDOCaliber = new DOCaliber();
            //_logger.Trace(this.GetType().Name + " ", "CaseAPI Controller Information", new { ALJCaseID = ALJCaseID }, _objBOCreateCase.CurrentUserId);
            _username = _httpContextAccessor.HttpContext.User.Identity.Name;
            _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Request", ALJCaseID, _username);
            try
            {
                _resException = _objBOCreateCase.GetSourceSysFromCaliber(ALJCaseID, out objDOCaliber);
                if (_resException != ExceptionTypes.Success && _resException != ExceptionTypes.ZeroRecords)
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok(objDOCaliber);
        }
    }
}
