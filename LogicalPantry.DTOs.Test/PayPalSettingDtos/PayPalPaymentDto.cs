using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicalPantry.DTOs.Test.PayPalSettingDtos
{
    public class PayPalPaymentDto
    {
        public string OrderId { get; set; }
        public string PayerId { get; set; }
        public string PaymentId { get; set; }
    }
}
