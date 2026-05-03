using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoSellerUltra.Login
{
    public static class UserSession
    {
        public static UserDto? CurrentUser { get; set; }

        public static event Action? SessionChanged;

        public static void SetUser(UserDto user)
        {
            CurrentUser = user;
            IsUserLogin = true;
            SessionChanged?.Invoke();
        }

        public static UserDto getUser()
        {
            return CurrentUser;
        }

        public static void SetPromoDiscount(int discount)
        {
            if (CurrentUser == null)
            {
                return;
            }

            CurrentUser.PromoDiscount = discount;
            SessionChanged?.Invoke();
        }

        public static bool IsUserLogin { get; set; }
    
        public static bool getIsUser()
        {
            return IsUserLogin;
        }

        public static void Logout()
        {
            CurrentUser = null;
            SessionChanged?.Invoke();
        }

    }
}
