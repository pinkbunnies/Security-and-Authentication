using NUnit.Framework;
using SafeVault.Helpers;

namespace SafeVault.Tests
{
    [TestFixture]
    public class TestSQLInjection
    {
        [Test]
        public void TestForSQLInjection()
        {
            // Simular una inyección maliciosa en el username
            string maliciousInput = "admin'; DROP TABLE Users; --";
            bool isValid = ValidationHelpers.IsValidInput(maliciousInput);
            // Se utiliza Assert.That para verificar que la entrada maliciosa no es válida
            Assert.That(isValid, Is.False, "La validación no debe aceptar inyección SQL.");
        }
    }
}
