using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jasarsoft.GraniteHouse.Data;
using Jasarsoft.GraniteHouse.Extensions;
using Jasarsoft.GraniteHouse.Models;
using Jasarsoft.GraniteHouse.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Jasarsoft.GraniteHouse.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class ShoppingCartController : Controller
    {
        private readonly ApplicationDbContext _db;
        [BindProperty] 
        public ShoppingCartViewModel ShoppingCartVM { get; set; }


        public ShoppingCartController(ApplicationDbContext db)
        {
            _db = db;
            ShoppingCartVM = new ShoppingCartViewModel()
            {
                Products = new List<Products>()
            };
        }
        public async Task<IActionResult> Index()
        {
            List<int> lstShoppingCart = HttpContext.Session.Get<List<int>>("ssShoppingCart");
            if (lstShoppingCart.Count > 0)
            {
                foreach (var cartItem in lstShoppingCart)
                {
                    Products prod = _db.Products.Include(p => p.SpecialTags).Include(x => x.ProductTypes)
                        .Where(x => x.Id == cartItem).FirstOrDefault();
                    ShoppingCartVM.Products.Add(prod);
                }
            }
            return View(ShoppingCartVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Index")]
        public IActionResult IndexPost()
        {
            List<int> lstCartItems = HttpContext.Session.Get<List<int>>("ssShoppingCart");

            ShoppingCartVM.Appointments.AppointmentDate = ShoppingCartVM.Appointments.AppointmentDate
                .AddHours(ShoppingCartVM.Appointments.AppointmentTime.Hour)
                .AddMinutes(ShoppingCartVM.Appointments.AppointmentTime.Minute);

            Appointments appointments = ShoppingCartVM.Appointments;
            _db.Appointmentses.Add(appointments);
            _db.SaveChanges();

            int appointmentId = appointments.Id;

            foreach (var productId in lstCartItems)
            {
                ProductsSelectedForAppointment productsSelectedForAppointment = new ProductsSelectedForAppointment()
                {
                    AppointmentId = appointmentId,
                    ProductId = productId
                };
                _db.ProductsSelectedForAppointments.Add(productsSelectedForAppointment);
            }

            _db.SaveChanges();
            lstCartItems = new List<int>();
            HttpContext.Session.Set("ssShoppingCart", lstCartItems);

            return RedirectToAction("Index");
        }
    }
}