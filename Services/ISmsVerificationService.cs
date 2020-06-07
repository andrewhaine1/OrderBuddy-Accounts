namespace Ord.Accounts.Services
{
    public interface ISmsVerificationService
    {
        System.Threading.Tasks.Task<bool> SendVerificationCode(string mobileNumber, string verificationCode);
    }
}
