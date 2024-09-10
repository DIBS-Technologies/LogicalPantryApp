using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicalPantry.DTOs.TimeSlotDtos
{
    public class Event
    {
        // Unix timestamps
        public long Start { get; set; }
        public long End { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }  

    }

}
