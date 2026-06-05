using ANGDDEAPIBO;
using ANGDDEAPIDO;
using Microsoft.AspNetCore.Mvc;

namespace ANGDDEAPI.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class DentalFacetHPController: ControllerBase
{
    private readonly DOConfiguration _objConfiguration;
    private readonly BOCommon _bOCommon;
    public DentalFacetHPController(DOConfiguration objConfiguration, BOCommon bOCommon)
    {
        _objConfiguration = objConfiguration;
        _bOCommon = bOCommon;
    }

    [HttpGet]
    public IActionResult GetMemberDetails()
    {
       return Ok();
    }
}
