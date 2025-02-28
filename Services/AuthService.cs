using Microsoft.AspNetCore.Identity;
using SafeVault.Models;
using System.Threading.Tasks;

namespace SafeVault.Services
{
    public class AuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<bool> RegisterUserAsync(string username, string email, string password)
        {
            var user = new ApplicationUser { UserName = username, Email = email };
            var result = await _userManager.CreateAsync(user, password);
            return result.Succeeded;
        }

        public async Task<bool> ValidateUserAsync(string username, string password)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null) return false;
            return await _userManager.CheckPasswordAsync(user, password);
        }
    }
}
