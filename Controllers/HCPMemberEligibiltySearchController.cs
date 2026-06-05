using ANGDDEAPI.Common;
using ANGDDEAPIBO;
using ANGDDEAPIBO.Interface;
using ANGDDEAPIDO;
using ANGDDEAPIDO.Interface;
using ANGDDEAPIFoundation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;

namespace ANGDDEAPI.Controllers
{
    /// <summary>
    /// NEW DotNet core
    /// </summary>
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class HCPMemberEligibiltySearchController : ControllerBase
    {
        public static long LoggedInUserId = 0;
        public string _username = string.Empty;
        string source = string.Empty;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DOConfiguration _objConfiguration;
        private readonly IMemoryCacheHelper _memoryCacheHelper;
        private readonly BOCommon _objBOCommon;
        private readonly ICacheService _cache;
        List<DOATSLookupMaster> lstATSLookupMaster = null;
        public ExceptionTypes exResult;
        public BOGPSMemberDetails _objBOGPSMemberDetails = null;
        public AccessAPI objAPICall = null;

        public HCPMemberEligibiltySearchController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _memoryCacheHelper = memoryCacheHelper;
            _objBOCommon = bOCommon;
            _cache = cache;
            exResult = _objBOCommon.GetRRTGPSAPiConfig(out lstATSLookupMaster);
            _objBOGPSMemberDetails = new BOGPSMemberDetails(objConfiguration, _cache);
            objAPICall = new AccessAPI(_memoryCacheHelper, _objConfiguration);
        }


        [HttpGet]
        public ActionResult GetMemberSearchDetails(string details = null) 
        {
            DOMemberSearchCriteria memberDetails = null;
            List<DOGPSMemberDetails> memberSearchDetails = null;
            source = BOCommon.GetRefererURI(Request);
            try
            {
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                //_httpContextAccessor.HttpContext.User.Identity.Name.Split(new string[] { @"\" }, StringSplitOptions.RemoveEmptyEntries);


                if (!string.IsNullOrEmpty(details))
                {
                    memberDetails = new DOMemberSearchCriteria();
                    memberDetails = JsonConvert.DeserializeObject<DOMemberSearchCriteria>(details);
                    #region MemberEligibilty empty request handling on 02/15/2021 by Sunil/Naseer/Pallavi
                    bool isValidRequest = false;
                    if (!string.IsNullOrEmpty(memberDetails.MemberId)
                        || !string.IsNullOrEmpty(memberDetails.HICNumber) || !string.IsNullOrEmpty(memberDetails.FirstName)
                        || !string.IsNullOrEmpty(memberDetails.LastName) || !string.IsNullOrEmpty(memberDetails.PhoneNumber)
                        || !string.IsNullOrEmpty(memberDetails.State) || !string.IsNullOrEmpty(memberDetails.ZIP)
                        || !string.IsNullOrEmpty(memberDetails.MemberSuffix))
                    {
                        isValidRequest = true;
                    }
                    if (!isValidRequest)
                    {
                        return BadRequest(new { error = "bad request" });
                    }
                    #endregion
                    if (memberDetails != null)
                    {
                        LoggedInUserId = memberDetails.UserId;
                    }
                    memberSearchDetails = new List<DOGPSMemberDetails>();
                    //BOHCPMemberDetails objBOHCPMemberDetails = new BOHCPMemberDetails(_objConfiguration);
                    HCPMemberMethods objHCPMemberMethods = new HCPMemberMethods(_memoryCacheHelper, _objConfiguration);
                    memberSearchDetails = objHCPMemberMethods.GetMemberDetails(memberDetails);
                    var isfilterenabled = _objBOCommon.GetLookupValueBasedonCon(lstATSLookupMaster, (long)RRTGPSAPIConfig.FilterOffshoreMember, 0);
                    if (memberSearchDetails != null && memberDetails?.IsOnshoreUser != true && !String.IsNullOrEmpty(isfilterenabled) && isfilterenabled == "1")
                    {
                        memberSearchDetails = memberSearchDetails?.Where(x => x.IsRestrictedMember == false).ToList();
                    }
                    if (memberSearchDetails == null || memberSearchDetails.Count == 0)
                    {
                        CSPEligibilityMethods objCSPEligibilityMethods = new CSPEligibilityMethods(_memoryCacheHelper, _objConfiguration, objAPICall);
                        DOCSPEligMemberDemographicsRequest objDOCSPEligMemberDemographicsRequest = new DOCSPEligMemberDemographicsRequest();
                        if (!string.IsNullOrEmpty(memberDetails.MemberId))
                        {
                            objDOCSPEligMemberDemographicsRequest.subscriberId = memberDetails.MemberId;
                            if (!string.IsNullOrEmpty(memberDetails.MemberSuffix))
                            {
                                objDOCSPEligMemberDemographicsRequest.memberSuffix = memberDetails.MemberSuffix;
                            }
                        }
                        objCSPEligibilityMethods.CSPMemberDemgraphicsDetailsV3(objDOCSPEligMemberDemographicsRequest, out memberSearchDetails);
                    }

                }
                return Ok(memberSearchDetails);
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
                return StatusCode((int)HttpStatusCode.InternalServerError, ex);
            }
        }
        public ActionResult GetPlanDetails(string details = null)
        {
            DOMemberSearchCriteria planDetails = null;
            List<DOMemberPlanDetails> planSearchDetails = null;
            source = BOCommon.GetRefererURI(Request);
            try
            {
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                if (!string.IsNullOrEmpty(details))
                {

                    planDetails = new DOMemberSearchCriteria();
                    planDetails = JsonConvert.DeserializeObject<DOMemberSearchCriteria>(details);
                    if (planDetails != null)
                    {
                        LoggedInUserId = planDetails.UserId;
                    }

                    if (planDetails.MemberRecordsource == "CDB")
                    {
                        HCPCDBMemberMethods objHCPMemberMethods = new HCPCDBMemberMethods(_memoryCacheHelper, _objConfiguration);
                        planSearchDetails = objHCPMemberMethods.GetPlanCDBMemberDeatils(planDetails);
                    }
                    else if (planDetails.MemberRecordsource != "CSP FACETS")
                    {
                        //BOHCPMemberDetails objBOHCPMemberDetails = new BOHCPMemberDetails(_objConfiguration);
                        HCPMemberMethods objHCPMemberMethods = new(_memoryCacheHelper, _objConfiguration);
                        planSearchDetails = objHCPMemberMethods.GetPlanMemberDeatils(planDetails);
                    }
                    else
                    {
                        CSPEligibilityMethods objCSPEligibilityMethods = new(_memoryCacheHelper, _objConfiguration, objAPICall);
                        planSearchDetails = objCSPEligibilityMethods.GetCSPPlanDetailsWithPCPAndDemographics(planDetails, _objBOCommon, lstATSLookupMaster);
                    }
                }
                return Ok(planSearchDetails);
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
                return StatusCode((int)HttpStatusCode.InternalServerError, ex);
            }
        }
        public ActionResult GetMemberSearchByID(string details = null)
        {
            DOCSPMemberData objDOCSPMemberData = null;
            DOMemberSearchCriteria memberDetails = null;
            List<DOGPSMemberDetails> memberSearchDetails = null;
            source = BOCommon.GetRefererURI(Request);
            try
            {
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                if (!string.IsNullOrEmpty(details))
                {
                    memberDetails = new DOMemberSearchCriteria();
                    memberDetails = JsonConvert.DeserializeObject<DOMemberSearchCriteria>(details);
                    if (memberDetails != null)
                    {
                        LoggedInUserId = memberDetails.UserId;
                    }
                    if (memberDetails.MemberRecordsource == "CDB")
                    {

                        HCPCDBMemberMethods objHCPMemberMethods = new HCPCDBMemberMethods(_memoryCacheHelper, _objConfiguration);
                        memberSearchDetails = objHCPMemberMethods.GetMemberCDBdetailsID(memberDetails);
                    }

                    else if (memberDetails.MemberRecordsource != "CSP FACETS")
                    {
                        //BOHCPMemberDetails objBOHCPMemberDetails = new BOHCPMemberDetails(_objConfiguration);
                        HCPMemberMethods objBOHCPMemberDetails = new(_memoryCacheHelper, _objConfiguration);
                        memberSearchDetails = objBOHCPMemberDetails.GetMemberdetailsID(memberDetails);
                    }
                    else
                    {
                        CSPEligibilityMethods objCSPEligibilityMethods = new CSPEligibilityMethods(_memoryCacheHelper, _objConfiguration, objAPICall);
                        DOCSPEligMemberDemographicsRequest objDOCSPEligMemberDemographicsRequest = new DOCSPEligMemberDemographicsRequest();
                        if (!string.IsNullOrEmpty(memberDetails.MemberId))
                        {
                            objDOCSPEligMemberDemographicsRequest.subscriberId = memberDetails.MemberId;
                        }
                        else if (!string.IsNullOrEmpty(memberDetails.HICNumber))
                        {
                            objDOCSPEligMemberDemographicsRequest.medicareId = memberDetails.HICNumber;
                        }
                        if (!string.IsNullOrEmpty(memberDetails.MemberSuffix))
                            objDOCSPEligMemberDemographicsRequest.memberSuffix = memberDetails.MemberSuffix;

                        objCSPEligibilityMethods.CSPMemberDemgraphicsDetailsV3(objDOCSPEligMemberDemographicsRequest, out memberSearchDetails);

                        try
                        {
                            var flagforMbrServiceAPI = _objBOCommon.GetLookupValueBasedonCon(lstATSLookupMaster, 18690065, 0);
                            if (flagforMbrServiceAPI == "1")
                            {
                                DOCSPMemberReq request = new DOCSPMemberReq();
                                CSPMemberBilling request1 = new CSPMemberBilling();
                                foreach (var mbr in memberSearchDetails)
                                {
                                    if (mbr.LstHospiceData is null)
                                    {
                                        mbr.LstHospiceData = new List<MedicareAndRetirementIndividualProfile>();
                                    }
                                    if (memberDetails != null && !string.IsNullOrEmpty(memberDetails.MemberId) && !string.IsNullOrEmpty(mbr.EmployerGroupNumber))
                                    {
                                        request.memberInquiry.subscriberId = memberDetails.MemberId;//111399356
                                        request.memberInquiry.groupId = mbr.EmployerGroupNumber;//AZMCARE
                                        request.memberInquiry.memberSuffix = mbr.MemberSuffix;//1
                                        if (request.memberInquiry.groupId != null && request.memberInquiry.groupId.ToLower().EndsWith("ex"))
                                        {
                                            request.requestType = "Exchange";
                                        }
                                        else
                                        {
                                            request.requestType = "Medicaid";
                                        }

                                        objCSPEligibilityMethods.CSPGetMemberV6(request, out objDOCSPMemberData);

                                        if (objDOCSPMemberData != null && objDOCSPMemberData.data != null && objDOCSPMemberData.data.Count > 0 && objDOCSPMemberData.data[0] != null)
                                        {
                                            if (objDOCSPMemberData.data[0].attributes?.premiumRateDetails?.Count > 0
                                                && objDOCSPMemberData.data[0].attributes.premiumRateDetails.Any(x => x.ratingEffDate.Date == Convert.ToDateTime(memberDetails.EligibilityFrom).Date))
                                            {
                                                mbr.TotalPremiumAmount = objDOCSPMemberData.data[0].attributes.premiumRateDetails.Where(x => x.ratingEffDate.Date == Convert.ToDateTime(memberDetails.EligibilityFrom).Date).Select(x => x.subsOvrridePremium).FirstOrDefault().ToString();
                                            }
                                            if (objDOCSPMemberData.data[0].attributes?.member?.memberEnrollSources?.Count > 0 && objDOCSPMemberData.data[0].attributes.member.memberEnrollSources.Any(x => x.effectiveDate.Date == Convert.ToDateTime(memberDetails.EligibilityFrom).Date))
                                            {
                                                mbr.PolicyExchangeId = objDOCSPMemberData.data[0].attributes?.member?.memberEnrollSources?.Where(x => x.effectiveDate.Date == Convert.ToDateTime(memberDetails.EligibilityFrom).Date).Select(x => x.exchMemberId).FirstOrDefault().ToString();
                                            }
                                            if (objDOCSPMemberData.data[0].attributes?.member?.memberSubsidyDetails?.Count > 0 && objDOCSPMemberData.data[0].attributes.member.memberSubsidyDetails.Any(x => x.subsidyEffectiveDate.Date == Convert.ToDateTime(memberDetails.EligibilityFrom).Date && x.subsidyType == "APTC"))
                                            {
                                                mbr.TotalAPTCAmount = objDOCSPMemberData.data[0].attributes?.member?.memberSubsidyDetails?.Where(x => x.subsidyEffectiveDate.Date == Convert.ToDateTime(memberDetails.EligibilityFrom).Date && x.subsidyType == "APTC").Select(x => x.subsidyAmount).FirstOrDefault().ToString();
                                            }
                                            if (objDOCSPMemberData.data[0].attributes?.member?.memberSubsidyDetails?.Count > 0 && objDOCSPMemberData.data[0].attributes.member.memberSubsidyDetails.Any(x => x.subsidyEffectiveDate.Date == Convert.ToDateTime(memberDetails.EligibilityFrom).Date && x.subsidyType == "CSR"))
                                            {
                                                mbr.CSRAmount = objDOCSPMemberData.data[0].attributes?.member?.memberSubsidyDetails?.Where(x => x.subsidyEffectiveDate.Date == Convert.ToDateTime(memberDetails.EligibilityFrom).Date && x.subsidyType == "CSR").Select(x => x.subsidyAmount).FirstOrDefault().ToString();
                                            }
                                            if (objDOCSPMemberData.data[0].attributes?.member?.comprehensiveMemEligs?.Count > 0 && objDOCSPMemberData.data[0].attributes.member.comprehensiveMemEligs.Any(x => Convert.ToDateTime(x.effDate).Date == Convert.ToDateTime(memberDetails.EligibilityFrom).Date && x.eligInd == "Y"))
                                            {
                                                mbr.PlanTermedThru = objDOCSPMemberData.data[0].attributes?.member?.comprehensiveMemEligs?.Where(x => Convert.ToDateTime(x.effDate).Date == Convert.ToDateTime(memberDetails.EligibilityFrom).Date && x.eligInd == "Y").Select(x => x.effDate).FirstOrDefault().ToString();
                                            }
                                            if (objDOCSPMemberData.data[0].attributes?.member?.memberEnrollSources?.Count > 0 && objDOCSPMemberData.data[0].attributes.member.memberEnrollSources.Any(x => x.effectiveDate.Date == Convert.ToDateTime(memberDetails.EligibilityFrom).Date))
                                            {
                                                mbr.ExchPolicyId = objDOCSPMemberData.data[0].attributes?.member?.memberEnrollSources?.Where(x => x.effectiveDate.Date == Convert.ToDateTime(memberDetails.EligibilityFrom).Date).Select(x => x.exchPolicyId).FirstOrDefault().ToString();
                                            }
                                            if (objDOCSPMemberData.data[0].attributes?.member?.MedicareDetails?.Count > 0)
                                            {
                                                if ((bool)objDOCSPMemberData.data[0].attributes?.member?.MedicareDetails.Any(x => x.MedicareInfos.Count > 0 && x.MedicareInfos.Any(s => s.EventCode == "HSPC")))
                                                {
                                                    List<MedicareInfo> LstMedicareInfo = objDOCSPMemberData.data[0].attributes?.member?.MedicareDetails[0]?.MedicareInfos?.Where(x => x.EventCode == "HSPC").ToList();

                                                    foreach (var medicareInfo in LstMedicareInfo)
                                                    {
                                                        MedicareAndRetirementIndividualProfile objMedicareAndRetirementIndividualProfile = new MedicareAndRetirementIndividualProfile();
                                                        objMedicareAndRetirementIndividualProfile.hospiceIndicator = "Y";
                                                        objMedicareAndRetirementIndividualProfile.MedicaidNumber = medicareInfo.MedicareID;
                                                        objMedicareAndRetirementIndividualProfile.profileDates = new()
                                                        {
                                                            startDate = medicareInfo.EffDate,
                                                            endDate = medicareInfo.TermDate
                                                        };

                                                        mbr.LstHospiceData.Add(objMedicareAndRetirementIndividualProfile);
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    if (memberDetails != null && !string.IsNullOrEmpty(memberDetails.MemberId))
                                    {
                                        DOCSPMemberBillingSummary objDOCSPMemberBillingSummary = null;
                                        request1.subscriberId = memberDetails.MemberId;//120951360
                                        request1.startDate = memberDetails.EligibilityFrom;//2024-01-01
                                        request1.endDate = memberDetails.EligibilityTo;//2024-12-01

                                        objCSPEligibilityMethods.GetMemberBilling(request, out objDOCSPMemberBillingSummary);

                                        if (objDOCSPMemberBillingSummary is not null && objDOCSPMemberBillingSummary.Data is not null && objDOCSPMemberBillingSummary.Data.Count > 0)
                                        {
                                            var billingInvoice = objDOCSPMemberBillingSummary.Data.OrderByDescending(x => x.Attributes?.BillInvoice?.InvoiceCreateDate).FirstOrDefault();
                                            if (billingInvoice != null)
                                            {
                                                mbr.NetAmountDue = billingInvoice.Attributes?.BillInvoice?.NetDueAmount;
                                            }
                                        }
                                    }
                                    if (memberDetails != null && !string.IsNullOrEmpty(memberDetails.MemberId))
                                    {
                                        DOCSPMemberReceiptHistoryResp objDOCSPMemberReceiptHistoryResp = null;
                                        request1.subscriberId = memberDetails.MemberId;//120951360
                                        request1.startDate = memberDetails.EligibilityFrom;//2024-01-01
                                        request1.endDate = memberDetails.EligibilityTo;//2024-12-01
                                        request1.consumerData = new ConsumerDataRequest();
                                        request1.consumerData.clientCode = "ATS";
                                        request1.consumerData.instance = "CSP";

                                        objCSPEligibilityMethods.GetMemberReceiptHistory(request, out objDOCSPMemberReceiptHistoryResp);

                                        if (objDOCSPMemberReceiptHistoryResp is not null && objDOCSPMemberReceiptHistoryResp.receipts is not null && objDOCSPMemberReceiptHistoryResp.receipts.Count > 0)
                                        {
                                            Receipt receiptHistory = objDOCSPMemberReceiptHistoryResp.receipts.OrderByDescending(x => Convert.ToDateTime(x.receivedDate)).FirstOrDefault();
                                            mbr.LastPaymentRcvdAmt = receiptHistory.receiptAmount;
                                            mbr.LastPaymentRcvdOn = receiptHistory.receivedDate;
                                        }
                                    }
                                    var flagforMbrAlvarAPI = _objBOCommon.GetLookupValueBasedonCon(lstATSLookupMaster, 18691459, 0);
                                    if (flagforMbrAlvarAPI == "1")
                                    {
                                        if (memberDetails != null && !string.IsNullOrEmpty(memberDetails.MemberId) && !string.IsNullOrEmpty(mbr.PermanentState) && !string.IsNullOrEmpty(mbr.ExchPolicyId))
                                        {
                                            try
                                            {
                                                mbr.LstDOEnrollmentSeedDetails = new List<DOEnrollmentSeedDetail>();
                                                DOAlvarSegmentRequest objDOAlvarSegmentRequest = new DOAlvarSegmentRequest();
                                                objDOAlvarSegmentRequest.state = mbr.PermanentState;
                                                objDOAlvarSegmentRequest.subsID = mbr.HealthExchangeID;
                                                AlvarMethods objAlvarMethods = new AlvarMethods(_memoryCacheHelper, _objConfiguration);
                                                DOAlvarSegmentResponse objDOAlvarSegmentResponse = new DOAlvarSegmentResponse();
                                                objAlvarMethods.GetAlvarSegmentDetails(objDOAlvarSegmentRequest, out objDOAlvarSegmentResponse);
                                                int index = 0;
                                                foreach (memberLists alrsegment in objDOAlvarSegmentResponse.memberList)
                                                {
                                                    DOEnrollmentSeedDetail alvarDetail = new DOEnrollmentSeedDetail();
                                                    alvarDetail.ExchangeAssignedPolicyId = Convert.ToString(alrsegment.policyNum);
                                                    alvarDetail.SegmentId = Convert.ToString(alrsegment.segmentId);
                                                    alvarDetail.AppliedAPTCAmount = Convert.ToDecimal(alrsegment.aptc);
                                                    alvarDetail.CSRAmount = Convert.ToDecimal(alrsegment.csr);
                                                    alvarDetail.TotalPremiumAmount = Convert.ToDecimal(alrsegment.tpa);
                                                    alvarDetail.NPN = Convert.ToString(alrsegment.npn);
                                                    alvarDetail.AssistorName = Convert.ToString(alrsegment.assistorName);
                                                    alvarDetail.TermOrCancelReason = !string.IsNullOrEmpty(alrsegment.termRsnCode) ? Convert.ToString(alrsegment.termRsnCode) : Convert.ToString(alrsegment.cancelRsnCode);
                                                    alvarDetail.QHPID = Convert.ToString(alrsegment.qhp);
                                                    
                                                    if (!string.IsNullOrEmpty(alrsegment.aptcEffDate) &&  DateTime.TryParseExact(alrsegment.aptcEffDate, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var res1))
                                                    {
                                                        alvarDetail.APTCEffDate = res1;
                                                    }
                                                    else
                                                    {
                                                        alvarDetail.APTCEffDate = null; // or default value if needed
                                                    }

                                                    DOAlvarRequest objDOAlvarRequest = new DOAlvarRequest();
                                                    objDOAlvarRequest.state = mbr.PermanentState;
                                                    objDOAlvarRequest.policyNum = long.Parse(alvarDetail.ExchangeAssignedPolicyId);
                                                    DOAlvarResponse objDOAlvarResponse = new DOAlvarResponse();
                                                    objAlvarMethods.GetAlvarRecords(objDOAlvarRequest, out objDOAlvarResponse);

                                                    DateTime? tempCovBeginDate = null;
                                                    DateTime? tempCovEndDate = null;

                                                    if (objDOAlvarResponse.memberList != null)
                                                    {
                                                        foreach (DataMember alr in objDOAlvarResponse.memberList)
                                                        {
                                                            alvarDetail.ExchangeAssignedPolicyId = Convert.ToString(objDOAlvarResponse.policyNumber);
                                                            alvarDetail.NPN = alr.brokerId_1;
                                                            alvarDetail.AssistorName = alr.brokerName_1;
                                                            alvarDetail.IndexToEditDel = index;
                                                            alvarDetail.MaintenanceReason = alr.maintenanceReasonCode;
                                                            if (tempCovBeginDate == null)
                                                            {
                                                                if (!string.IsNullOrEmpty(alr.CoverageBeginDate) && DateTime.TryParseExact(alr.CoverageBeginDate, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var covBegindate))
                                                                {
                                                                    alvarDetail.CoverageBeginDate = covBegindate;
                                                                }
                                                                else
                                                                {
                                                                    alvarDetail.CoverageBeginDate = null; // or default value if needed
                                                                }
                                                            }

                                                            if (tempCovEndDate == null)
                                                            {

                                                                if (!string.IsNullOrEmpty(alr.CoverageEndDate) && DateTime.TryParseExact(alr.CoverageEndDate, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var covEnddate))
                                                                {
                                                                    alvarDetail.CoverageEndDate = covEnddate;
                                                                }
                                                                else
                                                                {
                                                                    alvarDetail.CoverageEndDate = null; // or default value if needed
                                                                }
                                                            }
                                                            tempCovBeginDate=alvarDetail.CoverageBeginDate;
                                                            tempCovEndDate=alvarDetail.CoverageEndDate;
                                                            
                                                            //if (alvarDetail.NPN != null)
                                                            //    break;
                                                        }
                                                    }
                                                    mbr.LstDOEnrollmentSeedDetails.Add(alvarDetail);
                                                    index++;
                                                }
                                            }
                                            catch (Exception)
                                            {

                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
                return Ok(memberSearchDetails);
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
                return StatusCode((int)HttpStatusCode.InternalServerError, ex);
            }
        }
        public ActionResult GetAllMemberDetails(string details = null)
        {
            List<DOGPSMemberDetails> memberSearchDetails = null;
            List<DOMemberPlanDetails> memberPlanDetails = null;
            source = BOCommon.GetRefererURI(Request);
            try
            {
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                if (!string.IsNullOrEmpty(details))
                {
                    DOMemberSearchCriteria memberDetails = new();
                    memberDetails = JsonConvert.DeserializeObject<DOMemberSearchCriteria>(details);

                    if (memberDetails != null)
                        LoggedInUserId = memberDetails.UserId;

                    if (memberDetails.MemberRecordsource != "CSP FACETS")
                    {
                        HCPMemberMethods objBOHCPMemberDetails = new(_memoryCacheHelper, _objConfiguration);
                        (memberSearchDetails, memberPlanDetails) = objBOHCPMemberDetails.GetAllMemberDetails(memberDetails);
                    }
                    if (memberDetails.MemberRecordsource == "CSP FACETS" || (memberSearchDetails?.Count == 0 && memberPlanDetails?.Count == 0))
                    {
                        CSPEligibilityMethods objCSPEligibilityMethods = new(_memoryCacheHelper, _objConfiguration, objAPICall);
                        (memberSearchDetails, memberPlanDetails) = objCSPEligibilityMethods.GetAllCSPMemberDetails(memberDetails);
                    }
                    _objBOCommon.Trace(this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), "Single member search with detail for - " + memberDetails.MemberRecordsource, details, _username);
                }

                return Ok(new { memberSearchDetails, memberPlanDetails });
            }
            catch (Exception ex)
            {
                if (source.Contains("DDE"))
                    _objBOCommon.ErrorLog(LoggedInUserId, this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), ex.Message, ex.ToString());
                else
                    _objBOCommon.LogError(this.GetType().Name + System.Reflection.MethodBase.GetCurrentMethod(), source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                return StatusCode((int)HttpStatusCode.InternalServerError, ex);
            }
        }
        public ActionResult GetMemberSearchDetailsForIEX(string details = null)
        {
            DOMemberSearchCriteria memberDetails = null;
            List<DOGPSMemberDetails> memberSearchDetails = null;
            source = BOCommon.GetRefererURI(Request);
            try
            {
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                if (!string.IsNullOrEmpty(details))
                {

                    memberDetails = new DOMemberSearchCriteria();
                    memberDetails = JsonConvert.DeserializeObject<DOMemberSearchCriteria>(details);
                    #region MemberEligibilty empty request handling on 02/15/2021 by Sunil/Naseer/Pallavi
                    bool isValidRequest = false;
                    if (!string.IsNullOrEmpty(memberDetails.MemberId)
                        || !string.IsNullOrEmpty(memberDetails.HICNumber) || !string.IsNullOrEmpty(memberDetails.FirstName)
                        || !string.IsNullOrEmpty(memberDetails.LastName) || !string.IsNullOrEmpty(memberDetails.PhoneNumber)
                        || !string.IsNullOrEmpty(memberDetails.State) || !string.IsNullOrEmpty(memberDetails.ZIP)
                        || !string.IsNullOrEmpty(memberDetails.MemberSuffix))
                    {
                        isValidRequest = true;
                    }
                    if (!isValidRequest)
                    {
                        return BadRequest(new { error = "bad request" });
                    }
                    #endregion
                    if (memberDetails != null)
                    {
                        LoggedInUserId = memberDetails.UserId;
                    }
                    CSPEligibilityMethods objCSPEligibilityMethods = new CSPEligibilityMethods(_memoryCacheHelper, _objConfiguration, objAPICall);
                    DOCSPEligMemberDemographicsRequest objDOCSPEligMemberDemographicsRequest = new DOCSPEligMemberDemographicsRequest();
                    if (memberDetails != null && !string.IsNullOrEmpty(memberDetails.MemberId))
                    {
                        objDOCSPEligMemberDemographicsRequest.subscriberId = memberDetails.MemberId;
                        if (!string.IsNullOrEmpty(memberDetails.MemberSuffix))
                        {
                            objDOCSPEligMemberDemographicsRequest.memberSuffix = memberDetails.MemberSuffix;
                        }
                        objCSPEligibilityMethods.CSPMemberDemgraphicsDetailsV3(objDOCSPEligMemberDemographicsRequest, out memberSearchDetails);
                    }
                    if (memberSearchDetails == null || memberSearchDetails.Count == 0)
                    {
                        //BOHCPMemberDetails objBOHCPMemberDetails = new BOHCPMemberDetails(_objConfiguration);
                        HCPMemberMethods objBOHCPMemberDetails = new(_memoryCacheHelper, _objConfiguration);
                        memberSearchDetails = objBOHCPMemberDetails.GetMemberDetails(memberDetails);
                    }
                }
                return Ok(memberSearchDetails);
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
                return StatusCode((int)HttpStatusCode.InternalServerError, ex);
            }
        }        
    }
}
