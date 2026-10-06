using Microsoft.AspNetCore.Mvc;
namespace CrownBarbers;

public class HomeController : Controller
{
    public IActionResult Index() => View();
    [Route("Services")] public IActionResult Services() => View();
    [Route("About")] public IActionResult About() => View();
    [Route("Terms")] public IActionResult Terms() => View();
}