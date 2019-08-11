using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jasarsoft.GraniteHouse.Data;
using Jasarsoft.GraniteHouse.Models;
using Jasarsoft.GraniteHouse.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Granite_House.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _db { get; set; }

        [BindProperty]
        public ProductsViewModel ProductsVM { get; set; }

        public ProductsController(ApplicationDbContext db)
        {
            _db = db;
            ProductsVM = new ProductsViewModel
            {
                Products = new Products(),
                ProductTypes = _db.ProductTypes.ToList(),
                SpecialTags = _db.SpecialTags.ToList(),
            };
        }

        public async Task<IActionResult> Index()
        {
            var products = _db.Products.Include(x => x.ProductTypes).Include(x => x.SpecialTags);

            return View(await products.ToListAsync());
        }
    }
}