
namespace TaskManager.Core.Interfaces
{
    public interface IPasswordService
    {
        public string SecurePassword(string password);
        public bool VerifyPassword(string password, string hashedPassword);
    }
}