using Authantication.CustomAuthorization;
using Authantication.Data.Context;
using Authantication.Data.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace Authantication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            // For Entity Framework  
            var ConnectionString = builder.Configuration.GetConnectionString("Connstr");
            builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(ConnectionString));


            // For Identity 
            builder.Services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            //Adding Authentication
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "JWT";
                options.DefaultChallengeScheme = "JWT";
            })
                // Adding Jwt Bearer 
                .AddJwtBearer("JWT", options =>
                {
                    var secretKey = builder.Configuration.GetValue<string>("SecretKey");
                    var secretKeyinByte = Encoding.ASCII.GetBytes(secretKey);
                    var key = new SymmetricSecurityKey(secretKeyinByte);

                    options.SaveToken = true;
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = key,
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = false,
                        RequireExpirationTime = false,
                        ClockSkew = TimeSpan.Zero
                    };
                });

            //Adding Authorization
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireManagers", policy => policy.RequireClaim(ClaimTypes.Role, "Manager"));
                options.AddPolicy("Adults", policy => policy.AddRequirements(new AgeRequirments(18)));
                       
                       
            });

            builder.Services.AddSingleton< IAuthorizationHandler, AgeRequirmentsHandller >();

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();

        }
    }
}
