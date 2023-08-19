using join.Models;
using Microsoft.AspNetCore.Mvc;
using MyDapperMvcApp.Data;


public class HomeController : Controller
{
    private readonly DapperContext _dapperContext;
    [Route("home/getcountry")]
    public IActionResult GetAllCountries()
    {
        var countries = _dapperContext.GetAllCountries();
        return Json(countries);
    }
    [Route("home/GetStates/{countryId}")]
    public IActionResult GetStates(int countryId)
    {
        var states = _dapperContext.GetStatesByCountry(countryId);
        return Json(states);
    }
    [Route("home/GetCities/{stateId}")]
    public IActionResult GetCities(int stateId)
    {
        var cities = _dapperContext.GetCitiesByState(stateId);
        return Json(cities);
    }
    private bool IsAllowedFileType(string contentType)
    {
        // List of allowed content types for PDF and images
        string[] allowedTypes = { "application/pdf", "image/jpeg", "image/png", "image/gif" };

        return allowedTypes.Contains(contentType);
    }

    public HomeController(DapperContext dapperContext)
    {
        _dapperContext = dapperContext;
    }



    //[HttpPost]
    //public JsonResult AjaxMethod()
    //{
    //    List<Customer> customers = (from customer in entities.Customers
    //                                select customer).ToList();
    //    return Json(customers);
    //}

    public IActionResult Index()
    {
        var products = _dapperContext.GetAllProducts();
        return View(products);
    }
    public IActionResult GetAllData()
    {
        var products = _dapperContext.GetAllProducts();
        return Json(products);
    }

    public IActionResult Details(int id)
    {
        var product = _dapperContext.GetProductById(id);

        if (product == null)
        {
            return NotFound();
        }

        return View(product);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(product product, IFormFile productFile)
    {
        if (ModelState.IsValid)
        {   

            product.file_name = productFile?.FileName;
            product.file_content_type = productFile?.ContentType;
            if (productFile != null && productFile.Length > 0)
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", productFile.FileName);
                if (IsAllowedFileType(productFile.ContentType))
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        productFile.CopyTo(stream);
                    }
                    _dapperContext.InsertProduct(product, productFile);
                }
                else
                {
                    ModelState.AddModelError("ProductFile", "Only PDF and image files are allowed.");
                    return View(product);
                }
            }
            return RedirectToAction(nameof(Index));
        }

        return View(product);
    }

    public IActionResult Edit(int id)
    {
        var product = _dapperContext.GetProductById(id);

        if (product == null)
        {
            return NotFound();
        }
        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, product updatedProduct, IFormFile productFile)
    {
        var product = _dapperContext.GetProductById(id);

        if (product == null)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            product.name = updatedProduct.name;
            product.phone = updatedProduct.phone;
            product.address = updatedProduct.address;
            product.country = updatedProduct.country;
            product.file_name = updatedProduct.file_name;
            product.file_content_type = updatedProduct.file_content_type;


            _dapperContext.UpdateProduct(product, productFile);
            return RedirectToAction(nameof(Index));
        }

        return View(updatedProduct);
    }

    public IActionResult Delete(int id)
    {
        var product = _dapperContext.GetProductById(id);

        if (product == null)
        {
            return NotFound();
        }

        // Delete the product using the DapperContext
        _dapperContext.DeleteProduct(id);

        return RedirectToAction(nameof(Index));
    }

    public IActionResult DataTable()
    {
        return View();
    }

}
