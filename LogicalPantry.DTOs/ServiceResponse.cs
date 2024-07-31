using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicalPantry.DTOs
{
    public class ServiceResponse<T>
    {
        public T Data { get; set; }
        public int Count { get; set; }
        public bool Success { get; set; } = true;
        public string Message { get; set; } = null;
    }
}
