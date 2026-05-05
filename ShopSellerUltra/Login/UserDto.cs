using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoSellerUltra.Login
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Login { get; set; } = "";
        public string Email { get; set; } = "";
        public string DateOfBirth { get; set; } = "";

        public int balance { get; set; }

        public int spendbalance { get; set; }

        public string Promocode { get; set; } = "";

        public int PromoDiscount { get; set; }
    }
}