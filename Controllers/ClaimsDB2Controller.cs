using ANGDDEAPIBO;
using ANGDDEAPIDA;
using ANGDDEAPIDO;
using ANGDDEAPIDO.Interface;
using ANGDDEAPIDO.RMTProvider;
using ANGDDEAPIFoundation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ANGDDEAPI.Controllers
{

    [ApiController]
    [Route("api/[controller]/[action]")]
    public class ClaimsDB2Controller : ControllerBase
    {
        public static long LoggedInUserId = 0;
        BOClaimSearchDB2 _objBOClaimsSearch;
        ExceptionTypes _result;
        public string _username = string.Empty;
        string source = string.Empty;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DOConfiguration _objConfiguration;
        private readonly BOCommon _objBOCommon;
        private readonly ICacheService _cache;
        public static string db2ConStr = string.Empty;
        public ClaimsDB2Controller(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, BOCommon bOCommon, ICacheService cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _objBOCommon = bOCommon;
            _cache = cache;
            db2ConStr = _objConfiguration.ConnectionStrings.DB2CONN;
        }
        /// <summary>
        /// Get Claims information based on search criteria
        /// </summary>
        /// <param name="strClaimSearch"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult GetClaimSearch(DOClaimInfo objDOClaimInfo)
        {
            source = BOCommon.GetRefererURI(Request);
            try
            {
                //DOClaimInfo objDOClaimInfo = new DOClaimInfo();
                //objDOClaimInfo.ClaimNumber = "086604830";
                //objDOClaimInfo.Div = "FLA";
                //objDOClaimInfo.RecId = "1";
                //objDOClaimInfo.AdjSeqNumber = "0";
                //objDOClaimInfo.ClaimType = "Hospital";
                _objBOClaimsSearch = new BOClaimSearchDB2(_objConfiguration, _cache);
                string query = string.Empty;
                StringBuilder claimQuery = new StringBuilder();
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                if (objDOClaimInfo != null)
                {
                    if (objDOClaimInfo.ClaimType == "Hospital")
                    {
                        query = ConstantTexts.HospitalHeaderSelectScript;
                        query = string.Format(query, objDOClaimInfo.ClaimNumber, objDOClaimInfo.Div, objDOClaimInfo.RecId, objDOClaimInfo.AdjSeqNumber);
                        claimQuery.Append(query);

                        query = ConstantTexts.HospitalReviewRsnCodeUserCodeSelectScript;
                        query = string.Format(query, objDOClaimInfo.ClaimNumber, objDOClaimInfo.Div, objDOClaimInfo.RecId, objDOClaimInfo.AdjSeqNumber);
                        claimQuery.Append(query);

                        query = ConstantTexts.HospitalUFEIDSelectScript;
                        query = string.Format(query, objDOClaimInfo.ClaimNumber, objDOClaimInfo.Div, objDOClaimInfo.AdjSeqNumber);
                        claimQuery.Append(query);

                        query = ConstantTexts.HospitalDiagnosisCodeSelectScript;
                        query = string.Format(query, objDOClaimInfo.ClaimNumber, objDOClaimInfo.Div, objDOClaimInfo.RecId, objDOClaimInfo.AdjSeqNumber);
                        claimQuery.Append(query);

                        query = ConstantTexts.HospitalInterestPaidSelectScript;
                        query = string.Format(query, objDOClaimInfo.ClaimNumber, objDOClaimInfo.Div, objDOClaimInfo.RecId, objDOClaimInfo.AdjSeqNumber);
                        claimQuery.Append(query);

                        query = ConstantTexts.HospitalRenderingReferringNPISelectScript;
                        query = string.Format(query, objDOClaimInfo.ClaimNumber, objDOClaimInfo.Div, objDOClaimInfo.RecId, objDOClaimInfo.AdjSeqNumber);
                        claimQuery.Append(query);

                        query = ConstantTexts.HospitalDetailSelectScript;
                        query = string.Format(query, objDOClaimInfo.ClaimNumber, objDOClaimInfo.Div, objDOClaimInfo.RecId, objDOClaimInfo.AdjSeqNumber);
                        claimQuery.Append(query);
                    }
                    else
                    {
                        query = ConstantTexts.PhysicianHeaderSelectScript;
                        query = string.Format(query, objDOClaimInfo.ClaimNumber, objDOClaimInfo.Div, objDOClaimInfo.RecId, objDOClaimInfo.ClaimSubAudit);
                        claimQuery.Append(query);

                        query = ConstantTexts.PhysicianReviewRsnCodeUserCodeSelectScript;
                        query = string.Format(query, objDOClaimInfo.ClaimNumber + objDOClaimInfo.ClaimSubAudit.ToString().PadLeft(2, '0'), objDOClaimInfo.Div, objDOClaimInfo.RecId);
                        claimQuery.Append(query);

                        query = ConstantTexts.PhysicianUFEIDSelectScript;
                        query = string.Format(query, objDOClaimInfo.ClaimNumber, objDOClaimInfo.Div, objDOClaimInfo.ClaimSubAudit);
                        claimQuery.Append(query);

                        query = ConstantTexts.PhysicianDiagnosisCodeSelectScript;
                        query = string.Format(query, objDOClaimInfo.ClaimNumber + objDOClaimInfo.ClaimSubAudit.ToString().PadLeft(2, '0'), objDOClaimInfo.Div, objDOClaimInfo.RecId);
                        claimQuery.Append(query);

                        query = ConstantTexts.PhysicianInterestPaidSelectScript;
                        query = string.Format(query, objDOClaimInfo.ClaimNumber + objDOClaimInfo.ClaimSubAudit.ToString().PadLeft(2, '0'), objDOClaimInfo.Div, objDOClaimInfo.RecId);
                        claimQuery.Append(query);

                        query = ConstantTexts.PhysicianRenderingReferringNPISelectScript;
                        query = string.Format(query, objDOClaimInfo.ClaimNumber + objDOClaimInfo.ClaimSubAudit.ToString().PadLeft(2, '0'), objDOClaimInfo.Div, objDOClaimInfo.RecId);
                        claimQuery.Append(query);

                        query = ConstantTexts.PhysicianDetailSelectScript;
                        query = string.Format(query, objDOClaimInfo.ClaimNumber, objDOClaimInfo.Div, objDOClaimInfo.RecId, objDOClaimInfo.ClaimSubAudit);
                        claimQuery.Append(query);

                        //query = ConstantTexts.PhysicianICESSelectScript;
                        //query = string.Format(query, objDOClaimInfo.ClaimNumber + objDOClaimInfo.ClaimSubAudit.ToString().PadLeft(2, '0'), objDOClaimInfo.Div);
                        //claimQuery.Append(query);
                    }

                    _result = _objBOClaimsSearch.GetClaimInformation(claimQuery.ToString(), out objDOClaimInfo);

                    if (_result != ExceptionTypes.Success && _result != ExceptionTypes.ZeroRecords)
                    {
                        return BadRequest();
                    }
                }
                return Ok(objDOClaimInfo);
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
                return BadRequest(); ;
            }
        }

        [HttpPost]
        public IActionResult GetClaimSearchFromDB2(DODB2ClaimHeader objDOClaimInfo)
        {
            source = BOCommon.GetRefererURI(Request);
            try
            {
                DataAcess data = new DataAcess(db2ConStr);
                _objBOClaimsSearch = new BOClaimSearchDB2(_objConfiguration, _cache);
                string query = string.Empty;
                StringBuilder claimQuery = new StringBuilder();
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Request", BOCommon.JsonConvertObjectToString(objDOClaimInfo), _username);
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                if (objDOClaimInfo != null)
                {
                    if (objDOClaimInfo.Type_of_Claim == "Hospital")
                    {
                        // SQL Injection Contrast 

                        parameters["HHC_AUDIT_NBR"] = objDOClaimInfo.Claim_Number;
                        parameters["HHC_SITE_ID"] = objDOClaimInfo.DIV;
                        parameters["HHC_REC_ID"] = objDOClaimInfo.REC_ID;
                        parameters["HHC_ADJ_SEQ_NBR"] = objDOClaimInfo.Adj_Seq_Number;
                        query = data.GetFormattedQueryFromResource(1, ConstantTexts.GetHospitalHeaderSelectScript, parameters);
                        // query = ConstantTexts.HospitalHeaderSelectScript;
                        // query = string.Format(query, objDOClaimInfo.Claim_Number, objDOClaimInfo.DIV, objDOClaimInfo.REC_ID, objDOClaimInfo.Adj_Seq_Number);
                        claimQuery.Append(query);

                        parameters["RVW_AUDIT"] = objDOClaimInfo.Claim_Number;
                        parameters["RVW_SITE_ID"] = objDOClaimInfo.DIV;
                        parameters["RVW_REC_ID"] = objDOClaimInfo.REC_ID;
                        parameters["RVW_ADJ_SEQ_NBR"] = objDOClaimInfo.Adj_Seq_Number;
                        query = data.GetFormattedQueryFromResource(1, ConstantTexts.GetHospitalReviewRsnCodeUserCodeSelectScript, parameters);
                        // query = ConstantTexts.HospitalReviewRsnCodeUserCodeSelectScript;
                        // query = string.Format(query, objDOClaimInfo.Claim_Number, objDOClaimInfo.DIV, objDOClaimInfo.REC_ID, objDOClaimInfo.Adj_Seq_Number);
                        claimQuery.Append(query);

                        parameters["BBD_AUDNBR"] = objDOClaimInfo.Claim_Number;
                        parameters["BBD_SITE_ID"] = objDOClaimInfo.DIV;
                        parameters["BBD_AUDSUB"] = objDOClaimInfo.Adj_Seq_Number;
                        query = data.GetFormattedQueryFromResource(1, ConstantTexts.GetHospitalUFEIDSelectScript, parameters);
                        // query = ConstantTexts.HospitalUFEIDSelectScript;
                        // query = string.Format(query, objDOClaimInfo.Claim_Number, objDOClaimInfo.DIV, objDOClaimInfo.Adj_Seq_Number);
                        claimQuery.Append(query);

                        parameters["HDC_AUDIT"] = objDOClaimInfo.Claim_Number;
                        parameters["HDC_SITE_ID"] = objDOClaimInfo.DIV;
                        parameters["HDC_REC_ID"] = objDOClaimInfo.REC_ID;
                        parameters["HDC_ADJ_SEQ_NBR"] = objDOClaimInfo.Adj_Seq_Number;
                        query = data.GetFormattedQueryFromResource(1, ConstantTexts.GetHospitalDiagnosisCodeSelectScript, parameters);
                        //query = ConstantTexts.HospitalDiagnosisCodeSelectScript;
                        //query = string.Format(query, objDOClaimInfo.Claim_Number, objDOClaimInfo.DIV, objDOClaimInfo.REC_ID, objDOClaimInfo.Adj_Seq_Number);
                        claimQuery.Append(query);

                        parameters["CAD_AUDIT"] = objDOClaimInfo.Claim_Number;
                        parameters["CAD_SITE_ID"] = objDOClaimInfo.DIV;
                        parameters["CAD_REC_ID"] = objDOClaimInfo.REC_ID;
                        parameters["CAD_ADJ_SEQ_NBR"] = objDOClaimInfo.Adj_Seq_Number;
                        query = data.GetFormattedQueryFromResource(1, ConstantTexts.GetHospitalInterestPaidSelectScript, parameters);
                        //query = ConstantTexts.HospitalInterestPaidSelectScript;
                        // query = string.Format(query, objDOClaimInfo.Claim_Number, objDOClaimInfo.DIV, objDOClaimInfo.REC_ID, objDOClaimInfo.Adj_Seq_Number);
                        claimQuery.Append(query);

                        parameters["HIP_AUDIT"] = objDOClaimInfo.Claim_Number;
                        parameters["HIP_SITE_ID"] = objDOClaimInfo.DIV;
                        parameters["HIP_REC_ID"] = objDOClaimInfo.REC_ID;
                        parameters["HIP_ADJ_SEQ_NBR"] = objDOClaimInfo.Adj_Seq_Number;
                        query = data.GetFormattedQueryFromResource(1, ConstantTexts.GetHospitalRenderingReferringNPISelectScript, parameters);
                        // query = ConstantTexts.HospitalRenderingReferringNPISelectScript;
                        // query = string.Format(query, objDOClaimInfo.Claim_Number, objDOClaimInfo.DIV, objDOClaimInfo.REC_ID, objDOClaimInfo.Adj_Seq_Number);
                        claimQuery.Append(query);

                        parameters["HDTL_AUDIT_NBR"] = objDOClaimInfo.Claim_Number;
                        parameters["HDTL_SITE_ID"] = objDOClaimInfo.DIV;
                        parameters["HDTL_REC_ID"] = objDOClaimInfo.REC_ID;
                        parameters["HDTL_ADJ_SEQ_NBR"] = objDOClaimInfo.Adj_Seq_Number;
                        query = data.GetFormattedQueryFromResource(1, ConstantTexts.GetHospitalDetailSelectScript, parameters);
                        //query = ConstantTexts.HospitalDetailSelectScript;
                        //query = string.Format(query, objDOClaimInfo.Claim_Number, objDOClaimInfo.DIV, objDOClaimInfo.REC_ID, objDOClaimInfo.Adj_Seq_Number);
                        claimQuery.Append(query);
                    }
                    else
                    {
                        parameters["CLM_AUDNBR"] = objDOClaimInfo.Claim_Number;
                        parameters["CLM_HMO_ID"] = objDOClaimInfo.DIV;
                        parameters["CLM_REC_ID"] = objDOClaimInfo.REC_ID;
                        parameters["CLM_AUDSUB"] = objDOClaimInfo.Claim_SubAudit;
                        query = data.GetFormattedQueryFromResource(1, ConstantTexts.GetPhysicianHeaderSelectScript, parameters);
                        //query = ConstantTexts.PhysicianHeaderSelectScript;
                        //query = string.Format(query, objDOClaimInfo.Claim_Number, objDOClaimInfo.DIV, objDOClaimInfo.REC_ID, objDOClaimInfo.Claim_SubAudit);
                        claimQuery.Append(query);

                        parameters["RVW_AUDIT"] = objDOClaimInfo.Claim_Number + objDOClaimInfo.Claim_SubAudit.ToString().PadLeft(2, '0');
                        parameters["RVW_SITE_ID"] = objDOClaimInfo.DIV;
                        parameters["RVW_REC_ID"] = objDOClaimInfo.REC_ID;
                        query = data.GetFormattedQueryFromResource(1, ConstantTexts.GetPhysicianReviewRsnCodeUserCodeSelectScript, parameters);
                        //query = ConstantTexts.PhysicianReviewRsnCodeUserCodeSelectScript;
                        //query = string.Format(query, objDOClaimInfo.Claim_Number + objDOClaimInfo.Claim_SubAudit.ToString().PadLeft(2, '0'), objDOClaimInfo.DIV, objDOClaimInfo.REC_ID);
                        claimQuery.Append(query);

                        parameters["BBD_AUDNBR"] = objDOClaimInfo.Claim_Number;
                        parameters["BBD_SITE_ID"] = objDOClaimInfo.DIV;
                        parameters["BBD_AUDSUB"] = objDOClaimInfo.Claim_SubAudit;
                        query = data.GetFormattedQueryFromResource(1, ConstantTexts.GetPhysicianUFEIDSelectScript, parameters);
                        //query = ConstantTexts.PhysicianUFEIDSelectScript;
                        //query = string.Format(query, objDOClaimInfo.Claim_Number, objDOClaimInfo.DIV, objDOClaimInfo.Claim_SubAudit);
                        claimQuery.Append(query);

                        parameters["HDC_AUDIT"] = objDOClaimInfo.Claim_Number + objDOClaimInfo.Claim_SubAudit.ToString().PadLeft(2, '0');
                        parameters["HDC_SITE_ID"] = objDOClaimInfo.DIV;
                        parameters["HDC_REC_ID"] = objDOClaimInfo.REC_ID;
                        query = data.GetFormattedQueryFromResource(1, ConstantTexts.GetPhysicianDiagnosisCodeSelectScript, parameters);
                        // query = ConstantTexts.PhysicianDiagnosisCodeSelectScript;
                        //query = string.Format(query, objDOClaimInfo.Claim_Number + objDOClaimInfo.Claim_SubAudit.ToString().PadLeft(2, '0'), objDOClaimInfo.DIV, objDOClaimInfo.REC_ID);
                        claimQuery.Append(query);

                        parameters["CAD_AUDIT"] = objDOClaimInfo.Claim_Number + objDOClaimInfo.Claim_SubAudit.ToString().PadLeft(2, '0');
                        parameters["CAD_SITE_ID"] = objDOClaimInfo.DIV;
                        parameters["CAD_REC_ID"] = objDOClaimInfo.REC_ID;
                        query = data.GetFormattedQueryFromResource(1, ConstantTexts.GetPhysicianInterestPaidSelectScript, parameters);
                        //query = ConstantTexts.PhysicianInterestPaidSelectScript;
                        //query = string.Format(query, objDOClaimInfo.Claim_Number + objDOClaimInfo.Claim_SubAudit.ToString().PadLeft(2, '0'), objDOClaimInfo.DIV, objDOClaimInfo.REC_ID);
                        claimQuery.Append(query);

                        parameters["HIP_AUDIT"] = objDOClaimInfo.Claim_Number + objDOClaimInfo.Claim_SubAudit.ToString().PadLeft(2, '0');
                        parameters["HIP_SITE_ID"] = objDOClaimInfo.DIV;
                        parameters["HIP_REC_ID"] = objDOClaimInfo.REC_ID;
                        query = data.GetFormattedQueryFromResource(1, ConstantTexts.GetPhysicianRenderingReferringNPISelectScript, parameters);
                        //query = ConstantTexts.PhysicianRenderingReferringNPISelectScript;
                        //query = string.Format(query, objDOClaimInfo.Claim_Number + objDOClaimInfo.Claim_SubAudit.ToString().PadLeft(2, '0'), objDOClaimInfo.DIV, objDOClaimInfo.REC_ID);
                        claimQuery.Append(query);

                        parameters["DRCD_AUDNBR"] = objDOClaimInfo.Claim_Number;
                        parameters["DRCD_SITE_ID"] = objDOClaimInfo.DIV;
                        parameters["DRCD_REC_ID"] = objDOClaimInfo.REC_ID;
                        parameters["DRCD_AUDSUB"] = objDOClaimInfo.Claim_SubAudit;
                        query = data.GetFormattedQueryFromResource(1, ConstantTexts.GetPhysicianDetailSelectScript, parameters);
                        //query = ConstantTexts.PhysicianDetailSelectScript;
                        //query = string.Format(query, objDOClaimInfo.Claim_Number, objDOClaimInfo.DIV, objDOClaimInfo.REC_ID, objDOClaimInfo.Claim_SubAudit);
                        claimQuery.Append(query);

                        parameters["ICE_AUDIT"] = objDOClaimInfo.Claim_Number + objDOClaimInfo.Claim_SubAudit.ToString().PadLeft(2, '0');
                        parameters["ICE_SITE_ID"] = objDOClaimInfo.DIV;
                        parameters["ICE_REC_SEQ"] = objDOClaimInfo.REC_ID;
                        query = data.GetFormattedQueryFromResource(1, ConstantTexts.GetPhysicianICESSelectScript, parameters);
                        //query = ConstantTexts.PhysicianICESSelectScript;
                        //query = string.Format(query, objDOClaimInfo.Claim_Number + objDOClaimInfo.Claim_SubAudit.ToString().PadLeft(2, '0'), objDOClaimInfo.DIV, objDOClaimInfo.REC_ID);
                        claimQuery.Append(query);
                    }

                    _result = _objBOClaimsSearch.GetClaimInformationFromDB2(claimQuery.ToString(), out objDOClaimInfo);

                    if (_result != ExceptionTypes.Success && _result != ExceptionTypes.ZeroRecords)
                    {
                        return BadRequest();
                    }
                }
                return Ok(objDOClaimInfo);
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
                return BadRequest(); ;
            }
        }

        [HttpPost]
        public IActionResult ProviderInformationFromDB2(DODB2ClaimHeader objDODB2ClaimHeaderRequest)
        {
             
            source = BOCommon.GetRefererURI(Request);
            DODB2ClaimHeader objDODB2ClaimHeaderResponse = new DODB2ClaimHeader();
            try
            { 
                _objBOClaimsSearch = new BOClaimSearchDB2(_objConfiguration, _cache);
                string query = string.Empty;
                StringBuilder claimQuery = new StringBuilder();
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                if (objDODB2ClaimHeaderRequest != null)
                {
                    BOClaimSearchDB2 objBOClaimSearchDB2 = new BOClaimSearchDB2(_objConfiguration, _cache);

                    objDODB2ClaimHeaderResponse = objBOClaimSearchDB2.GetProviderInformationFromDB2(objDODB2ClaimHeaderRequest);

                    if (_result != ExceptionTypes.Success && _result != ExceptionTypes.ZeroRecords)
                    {
                        return BadRequest();
                    }
                }
                
                return Ok(objDODB2ClaimHeaderResponse);
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
                return BadRequest(); ;
            }
        }

        /// <summary>
        /// Get Delegate address from DB2 based on UFEID (PMG Address)
        /// </summary>
        /// <param name="UFEID"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult DelegateAddressFromDB2(string UFEID, string div = null)
        {
            _username = _httpContextAccessor.HttpContext.User.Identity.Name;
            _objBOCommon.Trace("PMGAddressFromDB2", System.Reflection.MethodBase.GetCurrentMethod().Name + "- Request entry", null, _username);
            source = BOCommon.GetRefererURI(Request);
            DODelegateAddress objDODODelegateAddress = new DODelegateAddress();
            List<DODelegateAddress> lstDODelegateAddress = new List<DODelegateAddress>();
            try
            {
                _objBOCommon.Trace(source + "-" + this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod(), System.Reflection.MethodBase.GetCurrentMethod().Name + "-Request", BOCommon.JsonConvertObjectToString(UFEID), _username);

                _objBOClaimsSearch = new BOClaimSearchDB2(_objConfiguration, _cache);
                string query = string.Empty;
                StringBuilder claimQuery = new StringBuilder();
                
                if (UFEID != null && UFEID != string.Empty)
                {
                    BOClaimSearchDB2 objBOClaimSearchDB2 = new BOClaimSearchDB2(_objConfiguration, _cache);

                    objDODODelegateAddress = objBOClaimSearchDB2.GetDelegateAddress(UFEID, div, out lstDODelegateAddress);

                    if (_result != ExceptionTypes.Success && _result != ExceptionTypes.ZeroRecords)
                    {
                        return BadRequest();
                    }
                }
                else
                {
                    return BadRequest("UFEID is required!"); 
                }
                _objBOCommon.Trace("ProviderInformationFromDB2", System.Reflection.MethodBase.GetCurrentMethod().Name + "- Request end", null, _username);
                return Ok(lstDODelegateAddress);
                //return Ok(objDODODelegateAddress);
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
                return BadRequest(ex.Message); 
            }
        }
    }
}
