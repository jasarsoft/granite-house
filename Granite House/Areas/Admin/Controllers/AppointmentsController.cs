using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Jasarsoft.GraniteHouse.Data;
using Jasarsoft.GraniteHouse.Models;
using Jasarsoft.GraniteHouse.Models.ViewModel;
using Jasarsoft.GraniteHouse.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Jasarsoft.GraniteHouse.Areas.Admin.Controllers
{
    [Authorize(Roles = SD.AdminEndUser + "," + SD.SuperAdminEndUser)]
    [Area("Admin")]
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AppointmentsController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index(string searchName = null, string searchEmail = null, string searchPhone = null, string searchDate = null)
        {
            ClaimsPrincipal currentUser = this.User;
            var claimsIdentity = (ClaimsIdentity) this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            AppointmentViewModel appointmentVM = new AppointmentViewModel()
            {
                Appointmentses = new List<Appointments>()
            };

            appointmentVM.Appointmentses = _db.Appointmentses.Include(a => a.SalesPersion).ToList();
            if (User.IsInRole(SD.AdminEndUser))
            {
                appointmentVM.Appointmentses =
                    appointmentVM.Appointmentses.Where(a => a.SalesPersonId == claim.Value).ToList();

            }

            if (searchName != null)
            {
                appointmentVM.Appointmentses = appointmentVM.Appointmentses
                    .Where(a => a.CustomerName.ToLower().Contains(searchName.ToLower())).ToList();
            }
            if (searchEmail != null)
            {
                appointmentVM.Appointmentses = appointmentVM.Appointmentses
                    .Where(a => a.CustomerEmail.ToLower().Contains(searchEmail.ToLower())).ToList();
            }
            if (searchPhone != null)
            {
                appointmentVM.Appointmentses = appointmentVM.Appointmentses
                    .Where(a => a.CustomerPhoneNumber.ToLower().Contains(searchPhone.ToLower())).ToList();
            }
            if (searchDate != null)
            {
                try
                {
                    DateTime appDate = Convert.ToDateTime(searchDate);
                    appointmentVM.Appointmentses = appointmentVM.Appointmentses
                        .Where(a => a.AppointmentDate.ToShortDateString().Equals(appDate.ToShortDateString())).ToList();
                }
                catch (Exception e)
                {
                    
                }
            }

            return View(appointmentVM);
        }
    }
}