using Microsoft.EntityFrameworkCore;
using FlowCare.Data;
using FlowCare.Services;
using FlowCare.Interfaces;
using FlowCare.Authentication;
using FlowCare.Seed;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication;
using System.Text;
using System.Text.Json.Serialization;

namespace FlowCare
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(
                    builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = null;
                    options.JsonSerializerOptions.Converters
                        .Add(new JsonStringEnumConverter());
                    options.JsonSerializerOptions.ReferenceHandler =
                        ReferenceHandler.IgnoreCycles; 
                });

            builder.Services.AddScoped<ISlotService, SlotService>();
            builder.Services.AddScoped<IAppointmentService, AppointmentService>();
            builder.Services.AddScoped<IAuditService, AuditService>();
            builder.Services.AddScoped<CsvService>();
            builder.Services.AddScoped<FileService>();
            builder.Services.AddScoped<SeedService>();
            builder.Services.AddHostedService<SlotCleanupService>();

            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            var key = Encoding.UTF8.GetBytes(
                jwtSettings["Key"] ?? throw new Exception("JWT Key is missing"));

            builder.Services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
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
                })
                .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>(
                    "Basic", null);

            builder.Services.AddAuthorization();

            var app = builder.Build();

            app.UseHttpsRedirection();
            app.UseMiddleware<ExceptionMiddleware>();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            //  Migration + Seeding
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await context.Database.MigrateAsync();

                try
                {
                    var seeder = scope.ServiceProvider.GetRequiredService<SeedService>();
                    await seeder.SeedAsync();
                }
                catch (Exception ex)
                {
                    var msg = ex.InnerException?.InnerException?.Message
                           ?? ex.InnerException?.Message
                           ?? ex.Message;
                    Console.WriteLine("===================");
                    Console.WriteLine("SEED ERROR: " + msg);
                    Console.WriteLine("===================");
                    throw;
                }
            }

            await app.RunAsync();
        }
    }
}