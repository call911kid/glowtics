using System.Threading.Tasks;

namespace Glowtics.BLL.Interfaces
{
    public interface IEmailNotificationService
    {
        Task SendRegistrationEmailAsync(string toEmail, string otpCode);
        Task SendPasswordResetEmailAsync(string toEmail, string otpCode);
    }
}
