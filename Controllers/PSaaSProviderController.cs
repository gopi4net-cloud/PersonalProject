using ANGDDEAPI.Common;
using ANGDDEAPIBO;
using ANGDDEAPIDO;
using ANGDDEAPIDO.Interface;
using ANGDDEAPIDO.PSaaS;
using ANGDDEAPIFoundation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using static ANGDDEAPI.Controllers.PSaaSProviderController;


namespace ANGDDEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class PSaaSProviderController : ControllerBase
    {
        public static long LoggedInUserId = 0;
        PSaSProviderMethods _objBOProviderSearch;
        ExceptionTypes _result;
        public string _username = string.Empty;
        string source = string.Empty;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DOConfiguration _objConfiguration;
        private readonly IMemoryCacheHelper _memoryCacheHelper;
        public readonly BOCommon _objBOCommon;

        private readonly ICacheService _cache;

        public PSaaSProviderController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _memoryCacheHelper = memoryCacheHelper;
            _objBOCommon = bOCommon;
            _cache = cache;
        }

        /// <summary>
        /// Get Claims information based on search criteria
        /// </summary>
        /// <param name="strClaimSearch"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetProviderSearch(string strProviderSearch)
        {
            source = BOCommon.GetRefererURI(Request);
            try
            {
                List<DOPSasSProviderInfo> lstObjDOProviderResponse = null;

                DOPSasProviderRequest objDOProviderInput = null;
                _objBOProviderSearch = new PSaSProviderMethods(_memoryCacheHelper, _objConfiguration, _httpContextAccessor, _objBOCommon);
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                if (!string.IsNullOrEmpty(strProviderSearch.ToString()))
                {
                    objDOProviderInput = new DOPSasProviderRequest();
                    objDOProviderInput = JsonConvert.DeserializeObject<DOPSasProviderRequest>(strProviderSearch);
                    //var practiceId = objDOProviderInput.practiceId;
                    //var taxId = objDOProviderInput.taxId;
                    if (objDOProviderInput.entityType != "G")
                    {
                        _result = _objBOProviderSearch.PSaSproviderDetails(objDOProviderInput, out lstObjDOProviderResponse); //P/F
                    }
                    else
                    {
                        _result = _objBOProviderSearch.PSaSGroupProviderDetails(objDOProviderInput, out lstObjDOProviderResponse);//G
                    }

                    if (objDOProviderInput.providerType != "P" && lstObjDOProviderResponse != null && lstObjDOProviderResponse.Count > 0)
                    {
                        foreach (var provider in lstObjDOProviderResponse)
                        {
                            provider.ProviderNetwork = null;
                            provider.ProviderNetworkList = null;
                        }
                    }

                }
                return Ok(lstObjDOProviderResponse);
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
                // _logger.Error(this.GetType().Name + "", (long)ErrorModuleName.WebService, (long)500, ex.ToString, " ", ex.StackTrace.ToString(), new DO.DOLoginUserDetails { ADM_UserInfoId = 10001 });
                return BadRequest();
            }
        }



        //[HttpGet]
        //public IActionResult GetLinxProviderSearch(string strProviderSearch)
        //{
        //    if (string.IsNullOrEmpty(strProviderSearch))
        //    {
        //        return BadRequest("Request body cannot be null or empty.");
        //    }

        //    try
        //    {
        //        string facilityType = null;
        //        string clinicianType = null;
        //        string type = string.Empty;
        //        //string type = string.Empty;
        //        // Deserialize the JSON string into the DOPSasFacility object

        //        var facilityData = JsonConvert.DeserializeObject<DOPSasFacility>(strProviderSearch);

        //        facilityData.networks = new List<string>()
        //        {
        //            "AKMEDICAID",
        //            "AKTRIBALHO",
        //            "ALABAEXCHANG",
        //            "ALEXCHANGE",
        //            "ALWYSCOMPREF",
        //            "APIPARHBA",
        //            "ATTSELECT",
        //            "AVMEDABAXCHG",
        //            "AVMEDFLXCHG",
        //            "AVMEDJHS",
        //            "AZABA",
        //            "AZABAEXCHGN",
        //            "AZBORLTD",
        //            "AZCRSMEDCAID",
        //            "AZEXCHANGE",
        //            "AZGMHSACAID",
        //            "AZIDD",
        //            "AZLTC",
        //            "BAYLORLTD",
        //            "BCBANETWORK",
        //            "BCBS",
        //            "BCBSAZCARE",
        //            "BCBSBPP",
        //            "BCBSCAPLAB",
        //            "BCBSINTEG",
        //            "BCBSMA",
        //            "BCBSMHPPO",
        //            "BCBSMHSA",
        //            "BCBSMIEAP",
        //            "BCBSPPMH",
        //            "BCBSTRAD",
        //            "BCBSTRUST",
        //            "BCN",
        //            "BCNA",
        //            "CAABAEXCHANG",
        //            "CAABAMEDICAL",
        //            "CAEXCHANGE",
        //            "CAMEDICAL",
        //            "CARDINALCUST",
        //            "CCI EXCHANGE",
        //            "CENTCOASTASO",
        //            "CLIENTOPTION",
        //            "COCHIP",
        //            "CODSNP",
        //            "COEXCHANGEDP",
        //            "COEXCHANGEHO",
        //            "COEXCHANGEMH",
        //            "COEXCHANGERM",
        //            "COEXCHANGESK",
        //            "COMEDICAID",
        //            "COMEDICARE",
        //            "CORAE",
        //            "DARTMOUTHOOA",
        //            "DARTMOUTHTR1",
        //            "DCDSNP",
        //            "DCMEDICAID",
        //            "EVERCAREHI",
        //            "FIDELITYCUST",
        //            "FLABAEXCHANG",
        //            "FLCMMA",
        //            "FLDSNP",
        //            "FLEXCHANGE",
        //            "FLHEALTHKIDS",
        //            "FLLTC",
        //            "FLMEDICAID",
        //            "GAABACAID",
        //            "GAABAEXCHANG",
        //            "GACHIP",
        //            "GAEXCHANGE",
        //            "GAMEDICAID",
        //            "GEHAAETNA",
        //            "GLHPMCAID",
        //            "HAWAIIQUEST",
        //            "HCAGAMHP",
        //            "HCAPREFLVLA",
        //            "HCAPREFLVLB",
        //            "HCAPREFLVLC",
        //            "HEALTHYKIDCA",
        //            "HEALTHYPA",
        //            "HIABAMCAID",
        //            "HP BOSTMEDCT",
        //            "HP BROCKTON",
        //            "HP CAREGROUP",
        //            "HP FALLONCLN",
        //            "HP SOSHORBNF",
        //            "IAABA",
        //            "IAMEDICAID",
        //            "IDABACAID",
        //            "IDMEDICAID",
        //            "IHCP",
        //            "ILABAEXCHANG",
        //            "ILEXCHANGE",
        //            "INABACAID",
        //            "INMEDICAID",
        //            "KSABA",
        //            "KSABAEXCHANG",
        //            "KSCHIP",
        //            "KSEXCHANGE",
        //            "KSLTC/AU/SED",
        //            "KSMEDICAID",
        //            "KYABACAID",
        //            "KYMCAID",
        //            "LAABACAID",
        //            "LAABAEXCHANG",
        //            "LABAYOU",
        //            "LAEXCHANGE",
        //            "LEGACY005",
        //            "LEGACYEMP",
        //            "LIFESOLUTION",
        //            "LOA",
        //            "LYRACUSTOM",
        //            "LYRAMEDICA",
        //            "MAABACAID",
        //            "MAMEDICAID",
        //            "MAMSIGATED",
        //            "MAMSIONENET",
        //            "MAMSIPPO",
        //            "MAMSIWORKCMP",
        //            "MAONECARE",
        //            "MDABAEXCHGN",
        //            "MDEXCHANGE",
        //            "MEDFAIRVIEW",
        //            "MEDICABACAID",
        //            "MEDICACAID",
        //            "MEDICACARE",
        //            "MEDICAID",
        //            "MEDICALABOR",
        //            "MEDICAMNCARE",
        //            "MEDICAMNPREF",
        //            "MEDICARE",
        //            "MEDICASELECT",
        //            "MEMORIALPREF",
        //            "MIABAEXCHANG",
        //            "MIEXCHANGE",
        //            "MNABACAID",
        //            "MNDSNP",
        //            "MNFIREEAP",
        //            "MNMEDICAID",
        //            "MOABAEXCHANG",
        //            "MOEXCHANGE",
        //            "MOMEDICAID",
        //            "MSABACAID",
        //            "MSABAEXCHANG",
        //            "MSCHIP",
        //            "MSEXCHANGE",
        //            "MSMEDICAID",
        //            "NCABACAID",
        //            "NCABAEXCHANG",
        //            "NCEXCHANGE",
        //            "NCFOSTERCARE",
        //            "NCMEDICAID",
        //            "NEABACAID",
        //            "NEDSNP",
        //            "NEMEDICAID",
        //            "NHPEXCHANGE",
        //            "NHPFQHC",
        //            "NJABACAID",
        //            "NJEXCHANGE",
        //            "NJLTC",
        //            "NJMEDICAID",
        //            "NMABAEXCHANG",
        //            "NMEXCHANGE",
        //            "NYABAEXCHANG",
        //            "NYCAIDABA",
        //            "NYCHIPABA",
        //            "NYEMPIREPLAN",
        //            "NYEPPABA",
        //            "NYMEDICAID",
        //            "NYMME",
        //            "OHABACAID",
        //            "OHABAEXCHANG",
        //            "OHEXCHANGE",
        //            "OHMEDICAID",
        //            "OHMME",
        //            "OKABAEXCHANG",
        //            "OKABASOONER",
        //            "OKEXCHANGE",
        //            "OKSOONERCAID",
        //            "OSCARCIRPLUS",
        //            "OSCARHLTHABA",
        //            "OSCARHLTHPLN",
        //            "OSCARMEDICAR",
        //            "OSCARSMALLGR",
        //            "OXFORDMCARE",
        //            "PAABACHIP",
        //            "PACHIP",
        //            "PARTNERSABA",
        //            "PARTNERSPREF",
        //            "PASTORALCOUN",
        //            "PBHCAOVERLAP",
        //            "PHPAK",
        //            "PHPMIDMICH",
        //            "PHPPREFFERED",
        //            "PSSANDIEGO",
        //            "RICAID",
        //            "RIMEDICAID",
        //            "SCABAEXCHANG",
        //            "SCEXCHANGE",
        //            "SLCOSA",
        //            "SPRINGHLTH",
        //            "STANFORDLTD",
        //            "SYSCOSPRGHIL",
        //            "TBICON",
        //            "TESTNETWORK",
        //            "TNABACAID",
        //            "TNABACHP",
        //            "TNABAEXCHGN",
        //            "TNCHIP",
        //            "TNEXCHANGE",
        //            "TNMEDICAID",
        //            "TRIBALNATION",
        //            "TRICARE",
        //            "TX CHIP",
        //            "TXABACAID",
        //            "TXABAEXCHANG",
        //            "TXEXCHANGE",
        //            "TXMMP",
        //            "TXSTAR",
        //            "TXSTARKIDS",
        //            "TXSTARPLUS",
        //            "UBH GENERAL",
        //            "UNIVAMER",
        //            "VAABACAID",
        //            "VAABAEXCHANG",
        //            "VACCC",
        //            "VACCN1",
        //            "VACCN2",
        //            "VACCN3",
        //            "VAEXCHANGE",
        //            "VAMDLCAID",
        //            "VHANETWORK",
        //            "WAABACAID",
        //            "WAABAEXCHGN",
        //            "WAAHECAID",
        //            "WAEXCHANGE",
        //            "WAHEALTHYOPT",
        //            "WAIMCCAID",
        //            "WAKINGBHO",
        //            "WAMME",
        //            "WIABAEXCHANG",
        //            "WICHAD",
        //            "WIEXCHANGE",
        //            "WIMEDICAID"

        //        };// Assuming this method fetches the networks from cache
        //          // Use switch expression to determine type

        //        facilityData.nonPar = facilityData.providerType == "NP";
        //        // Skip setting id/idType when searching by FirstName + LastName
        //        if (string.IsNullOrWhiteSpace(facilityData.firstName) || string.IsNullOrWhiteSpace(facilityData.lastName))
        //        {
        //            facilityData.id =
        //                !string.IsNullOrEmpty(facilityData.taxId) ? facilityData.taxId :
        //                !string.IsNullOrEmpty(facilityData.practiceId) ? facilityData.practiceId :
        //                !string.IsNullOrEmpty(facilityData.npi) ? facilityData.npi :
        //                facilityData.id;

        //            facilityData.idType = GetIdType(facilityData.taxId, facilityData.practiceId, facilityData.facilityName, facilityData.npi).ToString();
        //        }
        //        else
        //        {
        //            facilityData.id = null;
        //            facilityData.idType = null;
        //        }

        //        if (facilityData.entityType == "P")
        //        {
        //            type = "clinician";
        //        }
        //        if (facilityData.entityType == "G")
        //        {
        //            type = "clinician";
        //        }
        //        if (facilityData.entityType == "F")
        //        {
        //            type = "facility";
        //        }

        //        type = facilityData.practiceId switch
        //        {
        //            string pid when !string.IsNullOrEmpty(pid) && pid.StartsWith("FAC", StringComparison.OrdinalIgnoreCase) => "facility",
        //            string pid when !string.IsNullOrEmpty(pid) && pid.StartsWith("GRP", StringComparison.OrdinalIgnoreCase) => "clinician",
        //            string pid when !string.IsNullOrEmpty(pid) => "clinician",
        //            _ => "clinician"
        //        };

        //        if (type == "facility")
        //        {

        //            if (facilityData == null)
        //            {
        //                return BadRequest("Invalid input: Unable to parse the request body.");
        //            }

        //            // Determine the facility type based on the input data

        //            if (!string.IsNullOrEmpty(facilityData.id))
        //            {
        //                facilityType = "id";
        //            }
        //            else if (!string.IsNullOrEmpty(facilityData.city) || !string.IsNullOrEmpty(facilityData.state))
        //            {
        //                facilityType = "location";
        //            }
        //            else if (!string.IsNullOrEmpty(facilityData.zip) || !string.IsNullOrEmpty(facilityData.city))
        //            {
        //                facilityType = "address";
        //            }
        //            else if (!string.IsNullOrEmpty(facilityData.facilityName))
        //            {
        //                facilityType = "name";
        //            }
        //            else
        //            {
        //                return BadRequest("Invalid input: Unable to determine the required operation.");
        //            }

        //        }
        //        else
        //        {
        //            if (facilityData == null)
        //            {
        //                return BadRequest("Invalid input: Unable to parse the request body.");
        //            }

        //            // Determine the facility type based on the input data

        //            if (!string.IsNullOrEmpty(facilityData.id))
        //            {
        //                clinicianType = "id";
        //                //facilityData.idType = GetIdType(facilityData.id);
        //            }
        //            else if (!string.IsNullOrEmpty(facilityData.zip) || (!string.IsNullOrEmpty(facilityData.city) & !string.IsNullOrEmpty(facilityData.state)))
        //            {
        //                clinicianType = "location";
        //            }
        //            else if (!string.IsNullOrEmpty(facilityData.zip) || !string.IsNullOrEmpty(facilityData.state))
        //            {
        //                clinicianType = "address";
        //            }
        //            else if (!string.IsNullOrEmpty(facilityData.firstName) && !string.IsNullOrEmpty(facilityData.lastName))
        //            {
        //                clinicianType = "name";
        //            }
        //            else
        //            {
        //                return BadRequest("Invalid input: Unable to determine the required operation.");
        //            }
        //        }



        //        // Initialize the BO class
        //        var psasProviderMethods = new LinxProviderMethods(_memoryCacheHelper, _objConfiguration, _httpContextAccessor, _objBOCommon);

        //        // Call the consolidated GetFacilityData method
        //        List<DOPSasSProviderInfo> responseList = null;
        //        ExceptionTypes result;

        //        if (facilityData.entityType == "F")
        //        {
        //            result = psasProviderMethods.GetFacilityData(facilityData, facilityType, out responseList); // entiry Type F
        //        }
        //        //if (facilityData.entityType == "G")
        //        //{
        //        //    result = psasProviderMethods.GetGroupClinicianData(facilityData, clinicianType, out responseList); // entiry Type G
        //        //}
        //        else
        //        {
        //            result = psasProviderMethods.GetClinicianData(facilityData, clinicianType, out responseList); // entiry Type P
        //        }

        //        // Handle the result
        //        if (result == ExceptionTypes.Success)
        //        {
        //            return Ok(responseList);
        //        }
        //        else if (result == ExceptionTypes.ArgumentNullException)
        //        {
        //            return BadRequest("Invalid input: Required fields are missing.");
        //        }
        //        else if (result == ExceptionTypes.NullReferenceException)
        //        {
        //            return StatusCode((int)HttpStatusCode.InternalServerError, "No response from the external API.");
        //        }
        //        else
        //        {
        //            return BadRequest();
        //        }
        //    }
        //    catch (JsonException jsonEx)
        //    {
        //        // Handle JSON deserialization errors
        //        _objBOCommon.LogError(this.GetType().Name + "." + nameof(GetLinxProviderSearch), source, 10001, jsonEx.Message, " ", jsonEx.StackTrace.ToString(), _username);
        //        return BadRequest("Invalid JSON format in the request body.");
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the exception
        //        _objBOCommon.LogError(this.GetType().Name + "." + nameof(GetLinxProviderSearch), source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
        //        return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
        //    }
        //}
        [HttpGet]
        public IActionResult GetLinxProviderSearch(string strProviderSearch)
        {
            if (string.IsNullOrEmpty(strProviderSearch))
            {
                return BadRequest("Request body cannot be null or empty.");
            }

            try
            {
                string facilityType = null;
                string clinicianType = null;
                string type = string.Empty;

                var facilityData = JsonConvert.DeserializeObject<DOPSasFacility>(strProviderSearch);

                facilityData.networks = new List<string>()
                {
                    "AKMEDICAID","AKTRIBALHO","ALABAEXCHANG","ALEXCHANGE","ALWYSCOMPREF",
                    "APIPARHBA","ATTSELECT","AVMEDABAXCHG","AVMEDFLXCHG","AVMEDJHS",
                    "AZABA","AZABAEXCHGN","AZBORLTD","AZCRSMEDCAID","AZEXCHANGE",
                    "AZGMHSACAID","AZIDD","AZLTC","BAYLORLTD","BCBANETWORK",
                    "BCBS","BCBSAZCARE","BCBSBPP","BCBSCAPLAB","BCBSINTEG",
                    "BCBSMA","BCBSMHPPO","BCBSMHSA","BCBSMIEAP","BCBSPPMH",
                    "BCBSTRAD","BCBSTRUST","BCN","BCNA","CAABAEXCHANG",
                    "CAABAMEDICAL","CAEXCHANGE","CAMEDICAL","CARDINALCUST","CCI EXCHANGE",
                    "CENTCOASTASO","CLIENTOPTION","COCHIP","CODSNP","COEXCHANGEDP",
                    "COEXCHANGEHO","COEXCHANGEMH","COEXCHANGERM","COEXCHANGESK","COMEDICAID",
                    "COMEDICARE","CORAE","DARTMOUTHOOA","DARTMOUTHTR1","DCDSNP",
                    "DCMEDICAID","EVERCAREHI","FIDELITYCUST","FLABAEXCHANG","FLCMMA",
                    "FLDSNP","FLEXCHANGE","FLHEALTHKIDS","FLLTC","FLMEDICAID",
                    "GAABACAID","GAABAEXCHANG","GACHIP","GAEXCHANGE","GAMEDICAID",
                    "GEHAAETNA","GLHPMCAID","HAWAIIQUEST","HCAGAMHP","HCAPREFLVLA",
                    "HCAPREFLVLB","HCAPREFLVLC","HEALTHYKIDCA","HEALTHYPA","HIABAMCAID",
                    "HP BOSTMEDCT","HP BROCKTON","HP CAREGROUP","HP FALLONCLN","HP SOSHORBNF",
                    "IAABA","IAMEDICAID","IDABACAID","IDMEDICAID","IHCP",
                    "ILABAEXCHANG","ILEXCHANGE","INABACAID","INMEDICAID","KSABA",
                    "KSABAEXCHANG","KSCHIP","KSEXCHANGE","KSLTC/AU/SED","KSMEDICAID",
                    "KYABACAID","KYMCAID","LAABACAID","LAABAEXCHANG","LABAYOU",
                    "LAEXCHANGE","LEGACY005","LEGACYEMP","LIFESOLUTION","LOA",
                    "LYRACUSTOM","LYRAMEDICA","MAABACAID","MAMEDICAID","MAMSIGATED",
                    "MAMSIONENET","MAMSIPPO","MAMSIWORKCMP","MAONECARE","MDABAEXCHGN",
                    "MDEXCHANGE","MEDFAIRVIEW","MEDICABACAID","MEDICACAID","MEDICACARE",
                    "MEDICAID","MEDICALABOR","MEDICAMNCARE","MEDICAMNPREF","MEDICARE",
                    "MEDICASELECT","MEMORIALPREF","MIABAEXCHANG","MIEXCHANGE","MNABACAID",
                    "MNDSNP","MNFIREEAP","MNMEDICAID","MOABAEXCHANG","MOEXCHANGE",
                    "MOMEDICAID","MSABACAID","MSABAEXCHANG","MSCHIP","MSEXCHANGE",
                    "MSMEDICAID","NCABACAID","NCABAEXCHANG","NCEXCHANGE","NCFOSTERCARE",
                    "NCMEDICAID","NEABACAID","NEDSNP","NEMEDICAID","NHPEXCHANGE",
                    "NHPFQHC","NJABACAID","NJEXCHANGE","NJLTC","NJMEDICAID",
                    "NMABAEXCHANG","NMEXCHANGE","NYABAEXCHANG","NYCAIDABA","NYCHIPABA",
                    "NYEMPIREPLAN","NYEPPABA","NYMEDICAID","NYMME","OHABACAID",
                    "OHABAEXCHANG","OHEXCHANGE","OHMEDICAID","OHMME","OKABAEXCHANG",
                    "OKABASOONER","OKEXCHANGE","OKSOONERCAID","OSCARCIRPLUS","OSCARHLTHABA",
                    "OSCARHLTHPLN","OSCARMEDICAR","OSCARSMALLGR","OXFORDMCARE","PAABACHIP",
                    "PACHIP","PARTNERSABA","PARTNERSPREF","PASTORALCOUN","PBHCAOVERLAP",
                    "PHPAK","PHPMIDMICH","PHPPREFFERED","PSSANDIEGO","RICAID",
                    "RIMEDICAID","SCABAEXCHANG","SCEXCHANGE","SLCOSA","SPRINGHLTH",
                    "STANFORDLTD","SYSCOSPRGHIL","TBICON","TESTNETWORK","TNABACAID",
                    "TNABACHP","TNABAEXCHGN","TNCHIP","TNEXCHANGE","TNMEDICAID",
                    "TRIBALNATION","TRICARE","TX CHIP","TXABACAID","TXABAEXCHANG",
                    "TXEXCHANGE","TXMMP","TXSTAR","TXSTARKIDS","TXSTARPLUS",
                    "UBH GENERAL","UNIVAMER","VAABACAID","VAABAEXCHANG","VACCC",
                    "VACCN1","VACCN2","VACCN3","VAEXCHANGE","VAMDLCAID",
                    "VHANETWORK","WAABACAID","WAABAEXCHGN","WAAHECAID","WAEXCHANGE",
                    "WAHEALTHYOPT","WAIMCCAID","WAKINGBHO","WAMME","WIABAEXCHANG",
                    "WICHAD","WIEXCHANGE","WIMEDICAID"
                };

                facilityData.nonPar = facilityData.providerType == "NP";

                // Detect search scenarios
                bool isClinicianNameSearch = !string.IsNullOrWhiteSpace(facilityData.firstName)
                                          && !string.IsNullOrWhiteSpace(facilityData.lastName) 
                                          && facilityData.entityType == "P";
                bool isGroupNameSearch = !string.IsNullOrWhiteSpace(facilityData.groupName)
                                       && facilityData.entityType == "P";
                bool isFacilityNameSearch = !string.IsNullOrWhiteSpace(facilityData.facilityName)
                                          && facilityData.entityType == "F";
                bool hasStreet = !string.IsNullOrWhiteSpace(facilityData.street);
                bool hasZip = !string.IsNullOrWhiteSpace(facilityData.zip);
                bool hasCityAndState = !string.IsNullOrWhiteSpace(facilityData.city)
                                     && !string.IsNullOrWhiteSpace(facilityData.state);
                bool isLocationSearch = hasCityAndState || hasZip;
                bool isAddressSearch = hasStreet && (hasCityAndState || hasZip);
                bool isNameOrAddressSearch = isClinicianNameSearch || isGroupNameSearch
                                          || isFacilityNameSearch || isLocationSearch || isAddressSearch;

                // Set id/idType only for ID-based searches
                if (!isNameOrAddressSearch)
                {
                    facilityData.id =
                        !string.IsNullOrEmpty(facilityData.taxId) ? facilityData.taxId :
                        !string.IsNullOrEmpty(facilityData.practiceId) ? facilityData.practiceId :
                        !string.IsNullOrEmpty(facilityData.npi) ? facilityData.npi :
                        facilityData.id;

                    facilityData.idType = GetIdType(facilityData.taxId, facilityData.practiceId, facilityData.facilityName, facilityData.npi).ToString();
                }
                else
                {
                    facilityData.id = null;
                    facilityData.idType = null;
                }

                // Determine type: facility vs clinician
                if (!string.IsNullOrEmpty(facilityData.practiceId))
                {
                    type = facilityData.practiceId switch
                    {
                        string pid when pid.StartsWith("FAC", StringComparison.OrdinalIgnoreCase) => "facility",
                        string pid when pid.StartsWith("GRP", StringComparison.OrdinalIgnoreCase) => "clinician",
                        _ => "clinician"
                    };
                }
                else if (facilityData.entityType == "F" || isFacilityNameSearch)
                {
                    type = "facility";
                }
                else
                {
                    type = "clinician";
                }

                // Determine sub-type (id / location / address / name)
                if (type == "facility")
                {
                    if (!string.IsNullOrEmpty(facilityData.id))
                    {
                        facilityType = "id";
                    }
                    else if (isAddressSearch)
                    {
                        facilityType = "address";
                    }
                    else if (isLocationSearch)
                    {
                        facilityType = "location";
                    }
                    else if (isFacilityNameSearch)
                    {
                        facilityType = "name";
                    }
                    else
                    {
                        return BadRequest("Invalid input: Unable to determine the required operation.");
                    }
                }
                else
                {
                    if (isClinicianNameSearch)
                    {
                        clinicianType = "name";
                    }
                    else if (isGroupNameSearch)
                    {
                        clinicianType = "groupName";
                    }
                    else if (!string.IsNullOrEmpty(facilityData.id))
                    {
                        clinicianType = "id";
                    }
                    else if (isAddressSearch)
                    {
                        clinicianType = "address";
                    }
                    else if (isLocationSearch)
                    {
                        clinicianType = "location";
                    }
                    else
                    {
                        return BadRequest("Invalid input: Unable to determine the required operation.");
                    }
                }

                // Initialize the BO class
                var psasProviderMethods = new LinxProviderMethods(_memoryCacheHelper, _objConfiguration, _httpContextAccessor, _objBOCommon);

                List<DOPSasSProviderInfo> responseList = null;
                ExceptionTypes result;

                if (facilityData.entityType == "F")
                {
                    result = psasProviderMethods.GetFacilityData(facilityData, facilityType, out responseList);
                }
                else if (facilityData.entityType == "G")
                {
                    result = psasProviderMethods.GetGroupClinicianData(facilityData, clinicianType, out responseList);
                }
                else
                {
                    result = psasProviderMethods.GetClinicianData(facilityData, clinicianType, out responseList);
                }

                if (result == ExceptionTypes.Success)
                {
                    return Ok(responseList);
                }
                else if (result == ExceptionTypes.ArgumentNullException)
                {
                    return BadRequest("Invalid input: Required fields are missing.");
                }
                else if (result == ExceptionTypes.NullReferenceException)
                {
                    return StatusCode((int)HttpStatusCode.InternalServerError, "No response from the external API.");
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (JsonException jsonEx)
            {
                _objBOCommon.LogError(this.GetType().Name + "." + nameof(GetLinxProviderSearch), source, 10001, jsonEx.Message, " ", jsonEx.StackTrace.ToString(), _username);
                return BadRequest("Invalid JSON format in the request body.");
            }
            catch (Exception ex)
            {
                _objBOCommon.LogError(this.GetType().Name + "." + nameof(GetLinxProviderSearch), source, 10001, ex.Message, " ", ex.StackTrace.ToString(), _username);
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public IActionResult GetLinxproviderDetailsByClinicianNumber(string strProviderSearch)
        {
            source = BOCommon.GetRefererURI(Request);
            try
            {
                List<DOPSasSProviderInfo> lstObjDOProviderResponse = null;

                ClinicianDetailRequestInput objDOProviderInput = null;
                var _objBOProviderSearch = new LinxProviderMethods(_memoryCacheHelper, _objConfiguration, _httpContextAccessor, _objBOCommon);
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                if (!string.IsNullOrEmpty(strProviderSearch.ToString()))
                {
                    objDOProviderInput = new ClinicianDetailRequestInput();
                    objDOProviderInput = JsonConvert.DeserializeObject<ClinicianDetailRequestInput>(strProviderSearch);

                    objDOProviderInput.eJProviderCartItemArray = new List<int>()
                    {
                        1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20
                    };
                    objDOProviderInput.noteHistDays = 180;

                    //var practiceId = objDOProviderInput.practiceId;
                    //var taxId = objDOProviderInput.taxId;

                    _result = _objBOProviderSearch.ClinicianDetailList(objDOProviderInput, out lstObjDOProviderResponse); //P/F


                }
                return Ok(lstObjDOProviderResponse);
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
                // _logger.Error(this.GetType().Name + "", (long)ErrorModuleName.WebService, (long)500, ex.ToString, " ", ex.StackTrace.ToString(), new DO.DOLoginUserDetails { ADM_UserInfoId = 10001 });
                return BadRequest();
            }
        }

        public IActionResult GetLinxGroupClinicianDetails(string strProviderSearch)
        {
            source = BOCommon.GetRefererURI(Request);
            try
            {
                List<DOPSasSProviderInfo> lstObjDOProviderResponse = null;

                GroupClinicianRequestInput objDOProviderInput = null;
                var _objBOProviderSearch = new LinxProviderMethods(_memoryCacheHelper, _objConfiguration, _httpContextAccessor, _objBOCommon);
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                if (!string.IsNullOrEmpty(strProviderSearch.ToString()))
                {
                    objDOProviderInput = new GroupClinicianRequestInput();
                    objDOProviderInput = JsonConvert.DeserializeObject<GroupClinicianRequestInput>(strProviderSearch);
                    objDOProviderInput.rostersThreshold = 200;
                    objDOProviderInput.rostersToReturn = 0;
                    //var practiceId = objDOProviderInput.practiceId;
                    //var taxId = objDOProviderInput.taxId;

                    _result = _objBOProviderSearch.ProviderGroupRoasterViewDetails(objDOProviderInput, out lstObjDOProviderResponse); //P/F


                }
                return Ok(lstObjDOProviderResponse);
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
                // _logger.Error(this.GetType().Name + "", (long)ErrorModuleName.WebService, (long)500, ex.ToString, " ", ex.StackTrace.ToString(), new DO.DOLoginUserDetails { ADM_UserInfoId = 10001 });
                return BadRequest();
            }
        }

        public IActionResult GetGroupNameSearch(string strProviderSearch)
        {
            source = BOCommon.GetRefererURI(Request);
            try
            {
                string facilityType = null;
                string clinicianType = null;
                string type = string.Empty;

                

                var facilityData = JsonConvert.DeserializeObject<DOPSasFacility>(strProviderSearch);
                facilityData.networks = new List<string>()
                {
                    "AKMEDICAID","AKTRIBALHO","ALABAEXCHANG","ALEXCHANGE","ALWYSCOMPREF",
                    "APIPARHBA","ATTSELECT","AVMEDABAXCHG","AVMEDFLXCHG","AVMEDJHS",
                    "AZABA","AZABAEXCHGN","AZBORLTD","AZCRSMEDCAID","AZEXCHANGE",
                    "AZGMHSACAID","AZIDD","AZLTC","BAYLORLTD","BCBANETWORK",
                    "BCBS","BCBSAZCARE","BCBSBPP","BCBSCAPLAB","BCBSINTEG",
                    "BCBSMA","BCBSMHPPO","BCBSMHSA","BCBSMIEAP","BCBSPPMH",
                    "BCBSTRAD","BCBSTRUST","BCN","BCNA","CAABAEXCHANG",
                    "CAABAMEDICAL","CAEXCHANGE","CAMEDICAL","CARDINALCUST","CCI EXCHANGE",
                    "CENTCOASTASO","CLIENTOPTION","COCHIP","CODSNP","COEXCHANGEDP",
                    "COEXCHANGEHO","COEXCHANGEMH","COEXCHANGERM","COEXCHANGESK","COMEDICAID",
                    "COMEDICARE","CORAE","DARTMOUTHOOA","DARTMOUTHTR1","DCDSNP",
                    "DCMEDICAID","EVERCAREHI","FIDELITYCUST","FLABAEXCHANG","FLCMMA",
                    "FLDSNP","FLEXCHANGE","FLHEALTHKIDS","FLLTC","FLMEDICAID",
                    "GAABACAID","GAABAEXCHANG","GACHIP","GAEXCHANGE","GAMEDICAID",
                    "GEHAAETNA","GLHPMCAID","HAWAIIQUEST","HCAGAMHP","HCAPREFLVLA",
                    "HCAPREFLVLB","HCAPREFLVLC","HEALTHYKIDCA","HEALTHYPA","HIABAMCAID",
                    "HP BOSTMEDCT","HP BROCKTON","HP CAREGROUP","HP FALLONCLN","HP SOSHORBNF",
                    "IAABA","IAMEDICAID","IDABACAID","IDMEDICAID","IHCP",
                    "ILABAEXCHANG","ILEXCHANGE","INABACAID","INMEDICAID","KSABA",
                    "KSABAEXCHANG","KSCHIP","KSEXCHANGE","KSLTC/AU/SED","KSMEDICAID",
                    "KYABACAID","KYMCAID","LAABACAID","LAABAEXCHANG","LABAYOU",
                    "LAEXCHANGE","LEGACY005","LEGACYEMP","LIFESOLUTION","LOA",
                    "LYRACUSTOM","LYRAMEDICA","MAABACAID","MAMEDICAID","MAMSIGATED",
                    "MAMSIONENET","MAMSIPPO","MAMSIWORKCMP","MAONECARE","MDABAEXCHGN",
                    "MDEXCHANGE","MEDFAIRVIEW","MEDICABACAID","MEDICACAID","MEDICACARE",
                    "MEDICAID","MEDICALABOR","MEDICAMNCARE","MEDICAMNPREF","MEDICARE",
                    "MEDICASELECT","MEMORIALPREF","MIABAEXCHANG","MIEXCHANGE","MNABACAID",
                    "MNDSNP","MNFIREEAP","MNMEDICAID","MOABAEXCHANG","MOEXCHANGE",
                    "MOMEDICAID","MSABACAID","MSABAEXCHANG","MSCHIP","MSEXCHANGE",
                    "MSMEDICAID","NCABACAID","NCABAEXCHANG","NCEXCHANGE","NCFOSTERCARE",
                    "NCMEDICAID","NEABACAID","NEDSNP","NEMEDICAID","NHPEXCHANGE",
                    "NHPFQHC","NJABACAID","NJEXCHANGE","NJLTC","NJMEDICAID",
                    "NMABAEXCHANG","NMEXCHANGE","NYABAEXCHANG","NYCAIDABA","NYCHIPABA",
                    "NYEMPIREPLAN","NYEPPABA","NYMEDICAID","NYMME","OHABACAID",
                    "OHABAEXCHANG","OHEXCHANGE","OHMEDICAID","OHMME","OKABAEXCHANG",
                    "OKABASOONER","OKEXCHANGE","OKSOONERCAID","OSCARCIRPLUS","OSCARHLTHABA",
                    "OSCARHLTHPLN","OSCARMEDICAR","OSCARSMALLGR","OXFORDMCARE","PAABACHIP",
                    "PACHIP","PARTNERSABA","PARTNERSPREF","PASTORALCOUN","PBHCAOVERLAP",
                    "PHPAK","PHPMIDMICH","PHPPREFFERED","PSSANDIEGO","RICAID",
                    "RIMEDICAID","SCABAEXCHANG","SCEXCHANGE","SLCOSA","SPRINGHLTH",
                    "STANFORDLTD","SYSCOSPRGHIL","TBICON","TESTNETWORK","TNABACAID",
                    "TNABACHP","TNABAEXCHGN","TNCHIP","TNEXCHANGE","TNMEDICAID",
                    "TRIBALNATION","TRICARE","TX CHIP","TXABACAID","TXABAEXCHANG",
                    "TXEXCHANGE","TXMMP","TXSTAR","TXSTARKIDS","TXSTARPLUS",
                    "UBH GENERAL","UNIVAMER","VAABACAID","VAABAEXCHANG","VACCC",
                    "VACCN1","VACCN2","VACCN3","VAEXCHANGE","VAMDLCAID",
                    "VHANETWORK","WAABACAID","WAABAEXCHGN","WAAHECAID","WAEXCHANGE",
                    "WAHEALTHYOPT","WAIMCCAID","WAKINGBHO","WAMME","WIABAEXCHANG",
                    "WICHAD","WIEXCHANGE","WIMEDICAID"
                };
                List<DOPSasSProviderInfo> lstObjDOProviderResponse = null;

                GroupClinicianRequestInput objDOProviderInput = null;
                var _objBOProviderSearch = new LinxProviderMethods(_memoryCacheHelper, _objConfiguration, _httpContextAccessor, _objBOCommon);
                _username = _httpContextAccessor.HttpContext.User.Identity.Name;
                if (!string.IsNullOrEmpty(strProviderSearch.ToString()))
                {
                    objDOProviderInput = new GroupClinicianRequestInput();
                    objDOProviderInput = JsonConvert.DeserializeObject<GroupClinicianRequestInput>(strProviderSearch);
                    objDOProviderInput.rostersThreshold = 200;
                    objDOProviderInput.rostersToReturn = 0;
                    //var practiceId = objDOProviderInput.practiceId;
                    //var taxId = objDOProviderInput.taxId;

                    _result = _objBOProviderSearch.GroupDetailsByName(facilityData, out lstObjDOProviderResponse); //P/F


                }
                return Ok(lstObjDOProviderResponse);
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
                // _logger.Error(this.GetType().Name + "", (long)ErrorModuleName.WebService, (long)500, ex.ToString, " ", ex.StackTrace.ToString(), new DO.DOLoginUserDetails { ADM_UserInfoId = 10001 });
                return BadRequest();
            }
        }

        public enum IdType
        {
            TAXID = 0,
            SSN = 1,
            CLINICIAN_ID = 2,
            PRACTICE_ID = 3,
            NAME_ONLY = 4,
            TELEPHONE = 5,
            NPI = 6,
            MPIN = 7
        }

        public string GetIdType(string taxId, string practiceId, string name,string npi)
        {
            if (!string.IsNullOrWhiteSpace(taxId))
                return "0";
            if (!string.IsNullOrWhiteSpace(practiceId))
                return "3";
            if (!string.IsNullOrWhiteSpace(name))
                return "4";
            if (!string.IsNullOrWhiteSpace(npi))
                return "6";

            throw new ArgumentException("No valid identifier provided.");
        }




    }
}
