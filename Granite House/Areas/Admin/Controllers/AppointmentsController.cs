﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
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
        public int PageSize = 5;

        public AppointmentsController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index(int productPage = 1, string searchName = null, string searchEmail = null, string searchPhone = null, string searchDate = null)
        {
            ClaimsPrincipal currentUser = this.User;
            var claimsIdentity = (ClaimsIdentity) this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            AppointmentViewModel appointmentVM = new AppointmentViewModel()
            {
                Appointmentses = new List<Appointments>()
            };
            
            StringBuilder param = new StringBuilder();

            param.Append("/Admin/Appointments?productPage=:");
            param.Append("&searchName");
            if (searchName != null)
            {
                param.Append(searchName);
            }
            param.Append("&searchEmail");
            if (searchEmail != null)
            {
                param.Append(searchEmail);
            }
            param.Append("&searchPhone");
            if (searchPhone != null)
            {
                param.Append(searchPhone);
            }
            param.Append("&searchDate");
            if (searchDate != null)
            {
                param.Append(searchDate);
            }

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

            var count = appointmentVM.Appointmentses.Count;

            appointmentVM.Appointmentses = appointmentVM.Appointmentses.OrderBy(p => p.AppointmentDate)
                .Skip((productPage - 1) * PageSize)
                .Take(PageSize).ToList();

            appointmentVM.PagingInfo = new PagingInfo
            {
                CurrentPage = productPage,
                ItemsPerPage = PageSize,
                TotalItems = count,
                urlParam = param.ToString()
            };

            return View(appointmentVM);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productList = (IEnumerable<Products>) (from p in _db.Products
                join a in _db.ProductsSelectedForAppointments
                    on p.Id equals a.ProductId
                where a.AppointmentId == id
                select p).Include("ProductTypes");

            var objAppointmentVM = new AppointmentDetailsViewModel()
            {
                Appointment = _db.Appointmentses.Include(a => a.SalesPersion).Where(a => a.Id == id).FirstOrDefault(),
                SalesPersion = _db.ApplicationUsers.ToList(),
                Products = productList.ToList()
            };

            return View(objAppointmentVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AppointmentDetailsViewModel objAppointmentVM)
        {
            if (ModelState.IsValid)
            {
                objAppointmentVM.Appointment.AppointmentDate = objAppointmentVM.Appointment.AppointmentDate
                    .AddHours(objAppointmentVM.Appointment.AppointmentDate.Hour)
                    .AddMinutes(objAppointmentVM.Appointment.AppointmentTime.Minute);

                var appointmentFromDb = _db.Appointmentses.Where(a => a.Id == objAppointmentVM.Appointment.Id)
                    .FirstOrDefault();

                appointmentFromDb.CustomerName = objAppointmentVM.Appointment.CustomerName;
                appointmentFromDb.CustomerEmail = objAppointmentVM.Appointment.CustomerEmail;
                appointmentFromDb.CustomerPhoneNumber = objAppointmentVM.Appointment.CustomerPhoneNumber;
                appointmentFromDb.AppointmentDate = objAppointmentVM.Appointment.AppointmentDate;
                appointmentFromDb.AppointmentTime = objAppointmentVM.Appointment.AppointmentTime;
                appointmentFromDb.isConfirmed = objAppointmentVM.Appointment.isConfirmed;

                if (User.IsInRole(SD.SuperAdminEndUser))
                {
                    appointmentFromDb.SalesPersonId = objAppointmentVM.Appointment.SalesPersonId;
                }

                _db.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            return View(objAppointmentVM);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productList = (IEnumerable<Products>)(from p in _db.Products
                join a in _db.ProductsSelectedForAppointments
                    on p.Id equals a.ProductId
                where a.AppointmentId == id
                select p).Include("ProductTypes");

            var objAppointmentVM = new AppointmentDetailsViewModel()
            {
                Appointment = _db.Appointmentses.Include(a => a.SalesPersion).Where(a => a.Id == id).FirstOrDefault(),
                SalesPersion = _db.ApplicationUsers.ToList(),
                Products = productList.ToList()
            };

            return View(objAppointmentVM);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productList = (IEnumerable<Products>)(from p in _db.Products
                join a in _db.ProductsSelectedForAppointments
                    on p.Id equals a.ProductId
                where a.AppointmentId == id
                select p).Include("ProductTypes");

            var objAppointmentVM = new AppointmentDetailsViewModel()
            {
                Appointment = _db.Appointmentses.Include(a => a.SalesPersion).Where(a => a.Id == id).FirstOrDefault(),
                SalesPersion = _db.ApplicationUsers.ToList(),
                Products = productList.ToList()
            };

            return View(objAppointmentVM);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appointment = await _db.Appointmentses.FindAsync(id);
            _db.Appointmentses.Remove(appointment);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}