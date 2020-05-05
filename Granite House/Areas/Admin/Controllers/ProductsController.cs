using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Jasarsoft.GraniteHouse.Data;
using Jasarsoft.GraniteHouse.Models;
using Jasarsoft.GraniteHouse.Models.ViewModel;
using Jasarsoft.GraniteHouse.Utility;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Granite_House.Controllers
{
    [Area("Admin")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly HostingEnvironment _hostingEnvironment;

        [BindProperty]
        public ProductsViewModel ProductsVM { get; set; }

        public ProductsController(ApplicationDbContext db, HostingEnvironment hostingEnvironment)
        {
            _db = db;
            _hostingEnvironment = hostingEnvironment;
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

        public IActionResult Create()
        {
            return View(ProductsVM);
        }

        //POST: Product Create
        [HttpPost, ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost()
        {
            if (!ModelState.IsValid)
            {
                return View(ProductsVM);
            }

            _db.Products.Add(ProductsVM.Products);
            await _db.SaveChangesAsync();

            //image being saved
            string webRootPath = _hostingEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;

            var productsFromDb = _db.Products.Find(ProductsVM.Products.Id);

            if (files.Count != 0)
            {
                // image has been uploaded
                var uploads = Path.Combine(webRootPath, SD.ImageFolder);
                var extension = Path.GetExtension(files[0].FileName);

                using (var filestream = new FileStream(Path.Combine(uploads, ProductsVM.Products.Id + extension),
                    FileMode.Create))
                {
                    files[0].CopyTo(filestream);
                }

                productsFromDb.Image = @"\" + SD.ImageFolder + @"\" + ProductsVM.Products.Id + extension;
            }
            else
            {
                // when user does not upload image
                var uploads = Path.Combine(webRootPath, SD.ImageFolder + @"\" + SD.DefaultProductImage);
                System.IO.File.Copy(uploads, webRootPath + @"\" + SD.ImageFolder + @"\" + ProductsVM.Products.Id + ".png");
                productsFromDb.Image = @"\" + SD.ImageFolder + @"\" + ProductsVM.Products.Id + ".png";
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ProductsVM.Products = await _db.Products.Include(m => m.SpecialTags).Include(m => m.ProductTypes)
                .SingleOrDefaultAsync(x => x.Id == id);
            if (ProductsVM.Products == null)
            {
                return NotFound();
            }

            return View(ProductsVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            if (ModelState.IsValid)
            {
                string webRootPath = _hostingEnvironment.WebRootPath;
                var files = HttpContext.Request.Form.Files;

                var productFromDb = _db.Products.Where(x => x.Id == ProductsVM.Products.Id).FirstOrDefault();

                if (files.Count > 0 && files[0] != null)
                {
                    // user upload a new image
                    var uploads = Path.Combine(webRootPath, SD.ImageFolder);
                    var extension_new = Path.GetExtension(files[0].FileName);
                    var extension_old = Path.GetExtension(productFromDb.Image);

                    if (System.IO.File.Exists(Path.Combine(uploads, ProductsVM.Products.Id + extension_old)))
                    {
                        System.IO.File.Delete(Path.Combine(uploads, ProductsVM.Products.Id + extension_old));
                    }

                    using (var filestream = new FileStream(Path.Combine(uploads, ProductsVM.Products.Id + extension_new),
                        FileMode.Create))
                    {
                        files[0].CopyTo(filestream);
                    }

                    ProductsVM.Products.Image = @"\" + SD.ImageFolder + @"\" + ProductsVM.Products.Id + extension_new;
                }

                if (ProductsVM.Products.Image != null)
                {
                    productFromDb.Image = ProductsVM.Products.Image;
                }

                productFromDb.Name = ProductsVM.Products.Name;
                productFromDb.Price = ProductsVM.Products.Price;
                productFromDb.Available = ProductsVM.Products.Available;
                productFromDb.ProductTypeId = ProductsVM.Products.ProductTypeId;
                productFromDb.SpecialTagsId = ProductsVM.Products.SpecialTagsId;
                productFromDb.ShadeColor = ProductsVM.Products.ShadeColor;

                await _db.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(ProductsVM);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ProductsVM.Products = await _db.Products.Include(m => m.SpecialTags).Include(m => m.ProductTypes)
                .SingleOrDefaultAsync(x => x.Id == id);
            if (ProductsVM.Products == null)
            {
                return NotFound();
            }

            return View(ProductsVM);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ProductsVM.Products = await _db.Products.Include(m => m.SpecialTags).Include(m => m.ProductTypes)
                .SingleOrDefaultAsync(x => x.Id == id);
            if (ProductsVM.Products == null)
            {
                return NotFound();
            }

            return View(ProductsVM);
        }
    }
}