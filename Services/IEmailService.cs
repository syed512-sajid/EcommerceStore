using EcommerceStore.Models;

namespace EcommerceStore.Services
{
    public interface IEmailService
    {
        // Existing methods (do NOT touch these)
        Task SendOrderConfirmationAsync(Order order, List<CartItem> cart);
        Task SendAdminNotificationAsync(Order order, List<CartItem> cart);

        // ✅ New method for OTP login
        Task SendOtpAsync(string email, string otp);
    }
}
