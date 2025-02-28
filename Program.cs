using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models; // Agregado para Swagger
using Microsoft.AspNetCore.HttpsPolicy; // Agregado para configurar opciones de redirección HTTPS
using SafeVault.Data;
using SafeVault.Models;
using System.Text;

// Se crea el constructor de la aplicación Web. Aquí se configuran los servicios que utilizará la aplicación.
var builder = WebApplication.CreateBuilder(args);

/////////////////////////////////////////////////////////////////////////////////////////
// 1. CONFIGURACIÓN DE LA CADENA DE CONEXIÓN
/////////////////////////////////////////////////////////////////////////////////////////
// Se obtiene la cadena de conexión definida en el archivo appsettings.json, en la clave "DefaultConnection".
// Si no se encuentra (por ejemplo, en ambiente de desarrollo), se utiliza una cadena de conexión por defecto.
// Esta cadena es necesaria para que Entity Framework Core se conecte a la base de datos.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? "Server=(localdb)\\mssqllocaldb;Database=SafeVaultDb;Trusted_Connection=True;MultipleActiveResultSets=true";

/////////////////////////////////////////////////////////////////////////////////////////
// 2. CONFIGURACIÓN DE ENTITY FRAMEWORK CORE Y ASP.NET IDENTITY
/////////////////////////////////////////////////////////////////////////////////////////
// Se añade el servicio del DbContext (ApplicationDbContext) a la colección de servicios de la aplicación.
// Aquí se configura EF Core para que use SQL Server con la cadena de conexión especificada.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Se configura ASP.NET Identity, que permite gestionar usuarios, roles y la autenticación de manera sencilla.
// Se establecen opciones de seguridad para las contraseñas, como la obligatoriedad de incluir dígitos, letras minúsculas y mayúsculas, entre otras.
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;              // Requiere que la contraseña contenga al menos un dígito.
        options.Password.RequireLowercase = true;            // Requiere que la contraseña tenga letras minúsculas.
        options.Password.RequireUppercase = true;            // Requiere que la contraseña tenga letras mayúsculas.
        options.Password.RequireNonAlphanumeric = false;     // No requiere caracteres especiales (no alfanuméricos).
        options.Password.RequiredLength = 6;                 // Define la longitud mínima de la contraseña.
    })
    // Se indica que se usará EF Core para almacenar la información de Identity.
    .AddEntityFrameworkStores<ApplicationDbContext>()
    // Se añaden los proveedores de tokens por defecto, útiles para operaciones como restablecer contraseñas.
    .AddDefaultTokenProviders();

/////////////////////////////////////////////////////////////////////////////////////////
// 3. CONFIGURACIÓN DE LA AUTENTICACIÓN CON JWT (JSON WEB TOKENS)
/////////////////////////////////////////////////////////////////////////////////////////
// Se extraen los parámetros para JWT desde la sección "Jwt" del archivo appsettings.json.
var jwtSettings = builder.Configuration.GetSection("Jwt");

// Se obtiene la clave secreta que se usará para firmar los tokens JWT. La clave se convierte a un arreglo de bytes.
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"] ?? "DefaultSecretKey12345");

// Se configura el esquema de autenticación para que se utilicen tokens JWT.
// Se establecen los esquemas de autenticación y desafío, de modo que se use JwtBearer para validar cada solicitud.
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; // Esquema por defecto para autenticar solicitudes.
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;    // Esquema por defecto para manejar desafíos de autenticación.
})
.AddJwtBearer(options =>
{
    // Se requiere HTTPS para la transmisión segura de los tokens.
    options.RequireHttpsMetadata = true;
    // Permite almacenar el token en el contexto de la solicitud.
    options.SaveToken = true;
    // Se definen los parámetros que se usarán para validar el token:
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,                // Se valida que el emisor del token coincida con el configurado.
        ValidateAudience = true,              // Se valida que la audiencia del token sea la esperada.
        ValidateLifetime = true,              // Se verifica que el token no haya expirado.
        ValidateIssuerSigningKey = true,      // Se valida la clave con la que se firmó el token.
        ValidIssuer = jwtSettings["Issuer"],  // Emisor válido, configurado en appsettings.json.
        ValidAudience = jwtSettings["Audience"], // Audiencia válida, configurada en appsettings.json.
        IssuerSigningKey = new SymmetricSecurityKey(key) // Clave simétrica para verificar la firma del token.
    };
});

/////////////////////////////////////////////////////////////////////////////////////////
// 3.1. CONFIGURACIÓN DE LA REDIRECCIÓN HTTPS
/////////////////////////////////////////////////////////////////////////////////////////
// Se configura explícitamente el puerto HTTPS para que el middleware de redirección lo utilice.
// Esto soluciona el warning "Failed to determine the https port for redirect."
builder.Services.Configure<HttpsRedirectionOptions>(options =>
{
    // Establece el puerto HTTPS, por ejemplo 7265, que debe coincidir con el configurado en launchSettings.json
    options.HttpsPort = 7265;
});

/////////////////////////////////////////////////////////////////////////////////////////
// 4. REGISTRO DE SERVICIOS ADICIONALES: CONTROLADORES Y SWAGGER
/////////////////////////////////////////////////////////////////////////////////////////
// Se añade el soporte para controladores, lo que permite definir endpoints mediante clases que hereden de ControllerBase.
builder.Services.AddControllers();

// Se añade soporte para explorar los endpoints de la API. Esto es útil para herramientas de documentación.
builder.Services.AddEndpointsApiExplorer();

// Se configura Swagger, que es una herramienta de documentación interactiva para APIs.
// Permite probar los endpoints directamente desde una interfaz web.
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SafeVault API", Version = "v1" });
});

/////////////////////////////////////////////////////////////////////////////////////////
// 5. CONSTRUCCIÓN DE LA APLICACIÓN Y CONFIGURACIÓN DEL PIPELINE HTTP
/////////////////////////////////////////////////////////////////////////////////////////
// Se construye la aplicación con todas las configuraciones y servicios registrados.
var app = builder.Build();

// Si la aplicación se ejecuta en ambiente de desarrollo, se habilita Swagger para facilitar la prueba y documentación de la API.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SafeVault API V1");
    });
}

// Se fuerza la redirección de todas las solicitudes HTTP a HTTPS para garantizar la seguridad en la transmisión de datos.
app.UseHttpsRedirection();

// Se añade el middleware de autenticación, que intercepta las solicitudes y valida el token JWT en las cabeceras.
app.UseAuthentication();

// Se añade el middleware de autorización, que determina si el usuario autenticado tiene permisos para acceder a determinados recursos.
app.UseAuthorization();

// Se mapean los controladores definidos en la aplicación para que puedan responder a las solicitudes.
app.MapControllers();

/////////////////////////////////////////////////////////////////////////////////////////
// 6. EJECUCIÓN DE LA APLICACIÓN
/////////////////////////////////////////////////////////////////////////////////////////
// Se inicia la aplicación y comienza a escuchar solicitudes en el puerto configurado.
app.Run();
