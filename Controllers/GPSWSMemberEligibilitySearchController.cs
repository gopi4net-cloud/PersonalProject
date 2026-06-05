using ANGDDEAPI.Common;
using ANGDDEAPIBO;
using ANGDDEAPIDO;
using ANGDDEAPIDO.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net;
using static ANGDDEAPIDO.DOPersonSearchWSRequestResponse;
using static ANGDDEAPIDO.DOHouseHoldRequestResponse;
using System;
using Newtonsoft.Json;
using static ANGDDEAPIDO.DOPlanProfileRequestResponse;

namespace ANGDDEAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class GPSWSMemberEligibilitySearchController : ControllerBase
    {
        public static long LoggedInUserId = 0;
        public string _username = string.Empty;
        string source = string.Empty;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DOConfiguration _objConfiguration;
        private readonly IMemoryCacheHelper _memoryCacheHelper;
        private readonly BOCommon _objBOCommon;
        private readonly ICacheService _cache;
        private static List<DOMemberPlanDetails> planSearchDetails;
        public BOGPSMemberDetails _objBOGPSMemberDetails = null;

        public GPSWSMemberEligibilitySearchController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _memoryCacheHelper = memoryCacheHelper;
            _objBOCommon = bOCommon;
            _cache = cache;
            _objBOGPSMemberDetails = new BOGPSMemberDetails(objConfiguration, _cache);
        }
        [HttpGet]
        public ActionResult GetMemberSearchDetails(string details = null)
        {
            PersonSearchRequest searchRequest = null;
            DOMemberSearchCriteria memberDetails = null;
            List<DOGPSMemberDetails> memberSearchDetails = null;
            BOGPSWSMemberDetails boGPSWSMemberDetails = new BOGPSWSMemberDetails(_objConfiguration);

            source = BOCommon.GetRefererURI(Request);
            try
            {
                if (!string.IsNullOrEmpty(details))
                {
                    memberDetails = new DOMemberSearchCriteria();
                    memberDetails = JsonConvert.DeserializeObject<DOMemberSearchCriteria>(details);
                    //var phone = new { PhoneNumber = memberDetails.PhoneNumber };
                    searchRequest = new PersonSearchRequest
                    {
                        memberNumber = memberDetails.MemberId,
                        medicareClaimNumber = memberDetails.HICNumber
                    };

                    #region MemberEligibilty Empty Request Handling
                    bool isValidRequest = false;
                    if (!string.IsNullOrEmpty(searchRequest.memberNumber)
                        || !string.IsNullOrEmpty(searchRequest.medicareClaimNumber)
                        || !string.IsNullOrEmpty(searchRequest.phone?.phoneNumber))
                    {
                        isValidRequest = true;
                    }
                    if (!isValidRequest)
                    {
                        return BadRequest(new { error = "bad request" });
                    }
                    #endregion
                    boGPSWSMemberDetails.GetMemberSearchDetails(searchRequest, out memberSearchDetails);
                }
                return Ok(memberSearchDetails);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet]
        public ActionResult GetHouseHoldAndPlanDetails(string details = null)
        {
            HouseholdSummaryRequest summaryRequest;
            BOGPSWSMemberDetails boGPSWSMemberDetails = new BOGPSWSMemberDetails(_objConfiguration);
            source = BOCommon.GetRefererURI(Request);
            //List<DOMemberPlanDetails> planSearchDetails = null;
            try
            {
                summaryRequest = new HouseholdSummaryRequest();
                summaryRequest = JsonConvert.DeserializeObject<HouseholdSummaryRequest>(details);

                boGPSWSMemberDetails.GetHouseHoldAndPlanDetails(summaryRequest, out planSearchDetails);

                #region SOAP PARAM CONSTRUCTION
                //string _soapEnvelope = @"<soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/' xmlns:get='http://gps.uhc.com/2011/04/15/gethouseholdsummary'>" +
                //"<soapenv:Header><gps:Security xmlns:gps='http://gps.uhc.com/2008/08/01/common/security'></gps:Security></soapenv:Header>" +
                //"<soapenv:Body></soapenv:Body></soapenv:Envelope>";

                //var client = new RestClient(ServiceReferenceModel.HouseHoldServiceUrl());
                //var request = new RestRequest()
                //{
                //    Method = Method.POST
                //};

                //request.AddHeader("Content-Type", "text/xml");

                //StringBuilder _soapParam = new StringBuilder(_soapEnvelope);

                //var _soapHeader = @"<gps:UserId>" + ServiceReferenceModel.UserId() + "</gps:UserId>" +
                //    "<gps:ClientId>" + ServiceReferenceModel.ClientId() + "</gps:ClientId>";

                //_soapParam.Insert(_soapParam.ToString().IndexOf("</gps:Security>"), _soapHeader);

                //var _soapBody = new StringBuilder("<get:getHouseholdSummaryRequest>");

                //foreach (PropertyInfo objProp in summaryRequest.GetType().GetProperties())
                //{
                //    if (objProp.CanRead)
                //    {
                //        object val = objProp.GetValue(summaryRequest, null);
                //        if (val != null)
                //        {
                //            _soapBody.Append("<get:" + objProp.Name + ">" + val + "</get:" + objProp.Name + ">");
                //        }
                //    }
                //}

                //_soapBody.Append("</get:getHouseholdSummaryRequest>");

                //_soapParam.Insert(_soapParam.ToString().IndexOf("</soapenv:Body>"), _soapBody);
                //request.AddParameter("text/xml", _soapParam.ToString(), ParameterType.RequestBody);
                //var response = client.Execute(request);
                #endregion

                return Ok(planSearchDetails);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet]
        public ActionResult GetMemberSearchByIDFromGPSWS(string details = null)
        {

            List<DOGPSMemberDetails> memberSearchDetails = null;
            BOGPSWSMemberDetails boGPSWSMemberDetails = new BOGPSWSMemberDetails(_objConfiguration);
            try
            {
                if (!string.IsNullOrEmpty(details))
                {
                    DOMemberSearchCriteria memberDetails = new DOMemberSearchCriteria();
                    memberDetails = JsonConvert.DeserializeObject<DOMemberSearchCriteria>(details);

                    PersonSearchRequest searchRequest = new PersonSearchRequest
                    {
                        memberNumber = memberDetails.MemberId,
                        medicareClaimNumber = memberDetails.HICNumber
                    };

                    #region MemberEligibilty Empty Request Handling
                    bool isValidRequest = false;
                    if (!string.IsNullOrEmpty(searchRequest.memberNumber)
                        && !string.IsNullOrEmpty(searchRequest.medicareClaimNumber))
                    {
                        isValidRequest = true;
                    }
                    if (!isValidRequest)
                    {
                        return BadRequest(new { error = "Bad Request" });
                    }
                    #endregion
                    boGPSWSMemberDetails.GetMemberSearchDetails(searchRequest, out memberSearchDetails);
                }
                // Internal Mapping
                // As these field values are available in HouseholdDetails
                memberSearchDetails[0].MemberAge = planSearchDetails[0].MemberAge;
                memberSearchDetails[0].DateOfDeath = planSearchDetails[0].DateOfDeath;
                memberSearchDetails[0].MailingAddressLine1 = planSearchDetails[0].MailingAddressLine1;
                memberSearchDetails[0].MailingAddressLine2 = planSearchDetails[0].MailingAddressLine2;
                memberSearchDetails[0].MailingCity = planSearchDetails[0].MailingCity;
                memberSearchDetails[0].MailingZIP = planSearchDetails[0].MailingZIP;
                memberSearchDetails[0].MailingState = planSearchDetails[0].MailingState;
                memberSearchDetails[0].PermanentAddrStartDate = planSearchDetails[0].PermanentAddrStartDate;
                memberSearchDetails[0].PermanentAddrStopDate = DateTime.MaxValue;
                memberSearchDetails[0].AlternatePhone = planSearchDetails[0].AlternatePhone;
                memberSearchDetails[0].POAOnFile = planSearchDetails[0].POAOnFile;
                memberSearchDetails[0].MedicaidMemberId = planSearchDetails[0].MedicaidMemberId;

                if (!string.IsNullOrEmpty(memberSearchDetails[0].MailingAddressLine1))
                {
                    memberSearchDetails[0].MailingAddrStartDate = memberSearchDetails[0].PermanentAddrStartDate;
                    memberSearchDetails[0].MailingAddrStopDate = DateTime.MaxValue;
                }

                memberSearchDetails[0].MemberEmailAddress = planSearchDetails[0].MemberEmailAddress;
                return Ok(memberSearchDetails);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex);
            }
        }
    }
}
