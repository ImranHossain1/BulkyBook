
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Customer.Controllers;
[Area("Customer")]
public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;


        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> productList = _unitOfWork.Product.GetAll(includProperties:"Category,CoverType");
            return View(productList);
        }

    public IActionResult Details(int productId)
            {
                ShoppingCart cartObj = new()
                {
                    Count = 1,
                    ProductId = productId,
                    Product = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == productId, includProperties: "Category,CoverType")
                };
            return View(cartObj);
        }
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public IActionResult Details(ShoppingCart shoppingCart)
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
        shoppingCart.ApplicationUserId = claim.Value;

        ShoppingCart cartFromDb = _unitOfWork.ShoppingCart.GetFirstOrDefault( u => u.ApplicationUserId == claim.Value && u.ProductId == shoppingCart.ProductId);
        if(cartFromDb == null)
        {
            _unitOfWork.ShoppingCart.Add(shoppingCart);
        }
        else
        {
            _unitOfWork.ShoppingCart.IncrementCount(cartFromDb, shoppingCart.Count);
        }
        _unitOfWork.Save(); 
        return RedirectToAction(nameof(Index));
    }
     
    public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(model: new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
