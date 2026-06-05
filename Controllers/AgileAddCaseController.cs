using ANGDDEAPIBO;
using ANGDDEAPIDO;
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
    public class AgileAddCaseController : ControllerBase
    {
        private readonly BOCommon _objBOCommon;

        public AgileAddCaseController(BOCommon bOCommon)
        {
            _objBOCommon = bOCommon;
        }
        /// <summary>
        /// // Insert Agile Case Entry Details
        /// </summary>
        /// <param name="memberOnlinePortal"></param>
        /// <returns></returns>
        //[AuthorizeUser]
        [HttpPost]
        public string PostAgileAddCaseDetails(DOAgileAddCaseDetails agileAddCaseDetails)
        {
            // DOMemberOnlinePortal memberOnlinePortal = null;

            _objBOCommon.ErrorLog(agileAddCaseDetails.LoginUserId, "PostAgileAddCaseDetails--Method Entry", "", "");
            try
            {
                bool result = BOAgileAddCaseDetails.InsertAgileAddCaseDetails(agileAddCaseDetails);
                if (result)
                {
                    agileAddCaseDetails.IsSucceeded = true;
                    agileAddCaseDetails.ResponseMessage = "Details has been inserted Successfully.";
                }
                else
                {
                    agileAddCaseDetails.IsSucceeded = false;
                    agileAddCaseDetails.ResponseMessage = "Failed to insert details data..!";
                }
            }
            catch (Exception ex)
            {
                _objBOCommon.ErrorLog(agileAddCaseDetails.LoginUserId, "PostAgileAddCaseDetails", ex.Message, ex.ToString());
            }
            _objBOCommon.ErrorLog(agileAddCaseDetails.LoginUserId, "PostAgileAddCaseDetails--Method End", "", "");

            return agileAddCaseDetails.ResponseMessage;
        }
    }
}
