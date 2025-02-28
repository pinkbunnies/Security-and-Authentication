using NUnit.Framework;
using System.Threading.Tasks;
using SafeVault.Services;

namespace SafeVault.Tests
{
    [TestFixture]
    public class TestAuthentication
    {
        [Test]
        public async Task TestInvalidLogin()
        {
            // Aquí se debería configurar un AuthService simulado utilizando un mock de UserManager,
            // y llamar a ValidateUserAsync con credenciales incorrectas para verificar que retorne false.
            // Por el momento, se utiliza await Task.CompletedTask para cumplir con la firma async.
            await Task.CompletedTask;
            Assert.Pass("Implementar pruebas unitarias de autenticación con un mock del UserManager.");
        }
    }
}
