namespace SafeVault.Helpers
{
    public static class SecurityHelpers
    {
        public static bool IsValidXSSInput(string input)
        {
            if (string.IsNullOrEmpty(input))
                return true;

            string lowerInput = input.ToLower();
            // No permitir etiquetas <script> o <iframe>
            if (lowerInput.Contains("<script") || lowerInput.Contains("<iframe"))
                return false;

            return true;
        }
        
        // Función auxiliar para sanear la entrada (opcional)
        public static string SanitizeInput(string input)
        {
            // Se puede implementar una lógica más robusta
            return input.Replace("<", "&lt;").Replace(">", "&gt;");
        }
    }
}
