using ANGDDEAPIBO;
using ANGDDEAPIDO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ANGDDEAPI.Controllers
{
    
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class AgileCaseSearchController : ControllerBase
    {
        private readonly BOCommon _objBOCommon;
        public AgileCaseSearchController(BOCommon bOCommon)
        {
            _objBOCommon = bOCommon;
        }
        /// <summary>
        /// GET: Agile Case Search details
        /// </summary>
        /// <param name="details"></param>
        /// <returns></returns>
       // [AuthorizeUser]
        [HttpGet]
        public ActionResult GetCaseSearchDetails(string details)
        {
            DOCaseSearchCriteria caseCriteriaDetails = null;
            List<DOCaseSearchDetails> caseSearchDetails = null;
            try
            {
                if (!string.IsNullOrEmpty(details))
                {
                    caseCriteriaDetails = new DOCaseSearchCriteria();
                    caseCriteriaDetails = JsonConvert.DeserializeObject<DOCaseSearchCriteria>(details);
                }
                caseSearchDetails = BOAgileCaseSearchDetails.GetCaseSearchDetails(caseCriteriaDetails);
                if (caseSearchDetails.Count > 0)
                {
                    //return Request.CreateResponse(HttpStatusCode.OK, caseSearchDetails);
                    return Ok(caseSearchDetails);
                }
                else
                {
                    //return Request.CreateResponse(HttpStatusCode.NotFound, caseSearchDetails);
                    return NotFound(caseSearchDetails);
                }
            }
            catch (Exception ex)
            {

                _objBOCommon.ErrorLog(caseCriteriaDetails.LoginUserId, "AgileCaseSearchAPI:GetCaseSearchDetails", ex.Message, ex.ToString());
                //return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }

        }
    }
}
