using E_commerce.Context;
using E_commerce.Services;
using E_commerce.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using E_commerce.Configurations;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
IConfiguration configuration = builder.Configuration;

builder.Configuration.AddEnvironmentVariables();


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



DotNetEnv.Env.Load();

//var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DBCon");

//if (string.IsNullOrEmpty(connectionString))
//{
//    throw new Exception("Connection string not found. Ensure the .env file is correctly configured and placed in the root directory.");
//}
//Add connection string to the applications configuration system

//builder.Configuration.AddInMemoryCollection(new Dictionary<string, string>
//{ {"ConnectionStrings:DBCon", connectionString }
//});



var dbServer = Environment.GetEnvironmentVariable("DB_SERVER");
var dbName = Environment.GetEnvironmentVariable("DB_NAME");
var connectionString = $"Server={dbServer};Database={dbName};Trusted_Connection=True;TrustServerCertificate=True;";


//var dbServer = Environment.GetEnvironmentVariable("DB_SERVER");
//var dbName = Environment.GetEnvironmentVariable("DB_NAME");

Console.WriteLine($"DB_SERVER: {dbServer}");
Console.WriteLine($"DB_NAME: {dbName}");


// Build the connection string
//var connectionString = $"Server={dbServer};Database={dbName};Trusted_Connection=True;";
Console.WriteLine($"server name {dbName} {dbServer} {connectionString}");

builder.Services.AddDbContext<DataContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Logging.ClearProviders(); // Optional: Clear default logging providers
builder.Logging.AddConsole(); // Optional: Add console logging
builder.Logging.SetMinimumLevel(LogLevel.Information); // Set minimum log level

var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
logger.LogInformation($"Server name: {dbServer}");
 logger.LogInformation($"Database name: {dbName}");
logger.LogInformation($"Connection string: {connectionString}");

builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddScoped<IUserServices, UserServices>();
builder.Services.AddScoped<IProductServices, ProductServices>();
builder.Services.AddScoped<ICartServices, CartServices>();
builder.Services.AddScoped<IOrderServices, OrderServices>();
builder.Services.AddScoped<IShippingAddressServices, ShippingAddressServices>();
builder.Services.AddScoped<IReviewServices, ReviewServices>();
builder.Services.AddScoped<IAuthServices, AuthServices>();
builder.Services.AddScoped<IWishListServices, WishListServices>();
builder.Services.AddScoped<ISalesService, SalesService>();
builder.Services.AddScoped<IRevenueService, RevenueService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IAdminHistoryService, AdminHistoryService>();
builder.Services.AddScoped<IRazorpayService, RazorpayService>();
builder.Services.AddSingleton<MQTTService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder.WithOrigins("http://localhost:3000", "http://localhost:5086")
                          .AllowAnyHeader()
                          .AllowAnyMethod());
});


var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

Console.WriteLine($"key {key}");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

//var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);
builder.Services.Configure<RazorpayConfig>(builder.Configuration.GetSection("RazorpayConfig"));

builder.Services.AddSingleton<RazorpayService>();


builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please insert JWT with Bearer into field",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement {
    {
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        },
        Array.Empty<string>()
    }});
});

Console.WriteLine($"Issuer: {jwtSettings["Issuer"]}");
Console.WriteLine($"Audience: {jwtSettings["Audience"]}");
Console.WriteLine($"Key: {Convert.ToBase64String(key)}");



builder.Services.AddAuthorization();



var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



app.UseHttpsRedirection();

app.UseCors("AllowSpecificOrigin");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.UseStaticFiles();


var mqttService = app.Services.GetRequiredService<MQTTService>();
await mqttService.ConnectAsync("ws://localhost:9001", 1883);
//await mqttService.ConnectAsync();

app.Run();



//{
//    "email": "shivesh@intimetec.com",
//  "password": "Test@123"
//}