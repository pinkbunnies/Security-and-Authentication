using NUnit.Framework;
using SafeVault.Helpers;

namespace SafeVault.Tests
{
    [TestFixture]
    public class TestXSS
    {
        [Test]
        public void TestForXSS()
        {
            string maliciousInput = "<script>alert('XSS');</script>";
            bool isValid = SecurityHelpers.IsValidXSSInput(maliciousInput);
            // Se utiliza Assert.That con Is.False para comprobar que la validación detecta la inyección
            Assert.That(isValid, Is.False, "La validación debe detectar scripts maliciosos.");
        }
    }
}
