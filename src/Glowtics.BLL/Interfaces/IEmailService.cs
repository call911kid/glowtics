using System.Threading.Tasks;

namespace Glowtics.BLL.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string htmlBody);
    }
}
