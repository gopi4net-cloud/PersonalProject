using ANGDDEAPI.Common;
using ANGDDEAPIBO;
using ANGDDEAPIDO;
using ANGDDEAPIFoundation;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Net;
using System;
using OfficeOpenXml.FormulaParsing.Excel.Functions.RefAndLookup;
using ANGDDEAPIDO.BHLinxAuth;
using ANGDDEAPIDO.Interface;
using Microsoft.AspNetCore.Http;
using ANGDDEAPIDO.AORROI;

namespace ANGDDEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class LinxRelatedROISearchController : Controller
    {
        public static long LoggedInUserId = 0;
        BHROIMethods _ObjBHROIsearch;
        ExceptionTypes _result;
        public string _username = string.Empty;
        string source = string.Empty;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DOConfiguration _objConfiguration;
        private readonly IMemoryCacheHelper _memoryCacheHelper;
        public readonly BOCommon _objBOCommon;

        private readonly ICacheService _cache;

        public LinxRelatedROISearchController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _memoryCacheHelper = memoryCacheHelper;
            _objBOCommon = bOCommon;
            _cache = cache;
        }



        [HttpGet]
        public IActionResult GetBHROISearch(string strSearch)
        {
            source = BOCommon.GetRefererURI(Request);
            try
            {
                List<DOROIDetailInfo> lstObjDOROIDetailInfo = null;

                DOROAROIInputRequest DOROAROIInputRequestInput = null;
                _ObjBHROIsearch = new BHROIMethods(_memoryCacheHelper, _objConfiguration, _httpContextAccessor, _objBOCommon);
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                if (!string.IsNullOrEmpty(strSearch.ToString()))
                {
                    DOROAROIInputRequestInput = new DOROAROIInputRequest();
                    DOROAROIInputRequestInput = JsonConvert.DeserializeObject<DOROAROIInputRequest>(strSearch);

                    _result = _ObjBHROIsearch.ROIDetails(DOROAROIInputRequestInput, out lstObjDOROIDetailInfo);

                }
                return Ok(lstObjDOROIDetailInfo);
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


        //[HttpGet]
        //public ActionResult RelatedROISearch(string details = null)
        //{//TODO Sample Code For Development.In Future it will updated with actual End point
        //    List<RelatedROISearch> lstROISearch = new List<RelatedROISearch>();
        //    try
        //    {
        //        List<RelatedROISearch> result = new List<RelatedROISearch>();
        //        var LinxDetails = JsonConvert.DeserializeObject<DOMemberSearchCriteria>(details);
        //        if (!string.IsNullOrEmpty(details))
        //        {
        //            lstROISearch.Add(new ANGDDEAPIDO.RelatedROISearch()
        //            {
        //                ROIAddress = "Test Bangalore1",
        //                ROIAddress2 = "Test Bangalore2",
        //                ROICity = "Dallas",
        //                ROIPhoneNum = "5453535353",
        //                ROIEffectiveDate = "04/01/2024",
        //                ROIEventDescription = "Test Data",
        //                ROIExpirationDate = "04/01/2025",
        //                ROIExpirationTypeLkup = 18683028,
        //                ROIReasonsLkup = 18682912,
        //                ROIReceivedDate = "04/01/2024",
        //                RelationToMember = "Test Data1",
        //                RepresentativeName = "Jason Bouch",
        //                ROIStateLkup = 1826,
        //                ROIZip = "12345-1234",
        //                AuthenticationInformation = "MSLogin",
        //                ROIEventCompletionDate = "04/01/2024",
        //                ROINotes = "Test ROINotes",
        //                ROIReceivedDateTime = "03/01/2024",
        //                ROIRecordType = "test-RecordType1",
        //                ROIStatus = "Completed",
        //                ROICategories = "Test-Category1",
        //                MemberId = "4567823456"


        //            });
        //            lstROISearch.Add(new ANGDDEAPIDO.RelatedROISearch()
        //            {
        //                RepresentativeName = "TOM Harry",
        //                ROIAddress = "Test Bangalore3",
        //                ROIAddress2 = "Test Line1",
        //                ROICity = "Austin",
        //                ROIPhoneNum = "9853535353",
        //                ROIEffectiveDate = "04/01/2024",
        //                ROIEventDescription = "Test Data1",
        //                ROIExpirationDate = "04/01/2025",
        //                ROIExpirationTypeLkup = 18683029,
        //                ROIReasonsLkup = 18682912,
        //                ROIReceivedDate = "04/01/2024",
        //                RelationToMember = "Test Data1",
        //                ROIStateLkup = 1826,
        //                ROIZip = "12345-1234",
        //                AuthenticationInformation = "LDAP",
        //                ROIEventCompletionDate = "04/01/2025",
        //                ROINotes = "Test ROINotes",
        //                ROIReceivedDateTime = "04/01/2024",
        //                ROIRecordType = "test-RecordType",
        //                ROIStatus = "Inprogress",
        //                ROICategories = "Test-Category",
        //                MemberId = "8856432567"
        //            });

        //            lstROISearch.Add(new ANGDDEAPIDO.RelatedROISearch()
        //            {
        //                RepresentativeName = "John Cena",
        //                ROIAddress = "Test Hyd",
        //                ROIAddress2 = "Test Line2",
        //                ROICity = "Chicago",
        //                ROIPhoneNum = "9853535353",
        //                ROIEffectiveDate = "04/01/2024",
        //                ROIEventDescription = "Test Data2",
        //                ROIExpirationDate = "04/01/2025",
        //                ROIExpirationTypeLkup = 18683028,
        //                ROIReasonsLkup = 18682912,
        //                ROIReceivedDate = "05/01/2024",
        //                RelationToMember = "Test Data1",
        //                ROIStateLkup = 1826,
        //                ROIZip = "12345-1234",
        //                AuthenticationInformation = "SSO",
        //                ROIEventCompletionDate = "04/01/2024",
        //                ROINotes = "Test ROINotes",
        //                ROIReceivedDateTime = "02/01/2024",
        //                ROIRecordType = "test-RecordType2",
        //                ROIStatus = "Incomplete",
        //                ROICategories = "Test-Category2",
        //                MemberId = "9956432566"
        //            });



        //            if (!string.IsNullOrEmpty(LinxDetails.MemberId))
        //            {
        //                result = lstROISearch.FindAll(c => c.MemberId == LinxDetails.MemberId);
        //            }
        //            else if (!string.IsNullOrEmpty(LinxDetails.State))
        //            {
        //                result = lstROISearch.FindAll(c => c.ROIStateLkup == 1826);
        //            }
        //            else if (!string.IsNullOrEmpty(LinxDetails.ZIP))
        //            {
        //                result = lstROISearch.FindAll(c => c.ROIZip == LinxDetails.ZIP);
        //            }
        //            else
        //            {
        //                result = lstROISearch;
        //            }


        //        }
        //        return Ok(result);

        //    }
        //    catch (Exception ex)
        //    {

        //        return StatusCode((int)HttpStatusCode.InternalServerError, ex);
        //    }
        //}
    }
}
