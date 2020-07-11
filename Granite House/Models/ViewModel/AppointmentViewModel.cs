using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jasarsoft.GraniteHouse.Models.ViewModel
{
    public class AppointmentViewModel
    {
        public List<Appointments> Appointmentses { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}
