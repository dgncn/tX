using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using tX.Data;
using tX.Models;
using System.Linq;
using System.Linq.Dynamic.Core;
namespace tX.Controllers;

[Route("[controller]/[action]")]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly TxContext _context;

    public HomeController(ILogger<HomeController> logger, TxContext context)
    {
        _logger = logger;
        _context = context;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public IActionResult GetCustomers()
    {
        try
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;
            var customerData = (from tempcustomer in _context.Transactions select tempcustomer);
            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
            {
                customerData = customerData.OrderBy(sortColumn + " " + sortColumnDirection);
            }
            if (!string.IsNullOrEmpty(searchValue))
            {
                customerData = customerData.Where(m => m.From.Contains(searchValue)
                                            || m.To.Contains(searchValue));
            }
            recordsTotal = customerData.Count();
            var data = customerData.Skip(skip).Take(pageSize).ToList();
            var jsonData = new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data };
            return Ok(jsonData);
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
