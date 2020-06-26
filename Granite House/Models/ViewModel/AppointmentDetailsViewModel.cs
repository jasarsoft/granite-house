using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jasarsoft.GraniteHouse.Models.ViewModel
{
    public class AppointmentDetailsViewModel
    {
        public Appointments Appointment { get; set; }
        public List<ApplicationUser> SalesPersion { get; set; }
        public List<Products> Products { get; set; }
    }
}
