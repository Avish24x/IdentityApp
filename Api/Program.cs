using Api.Data;
using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<Context>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// be able to inject JWTservice class inside our controllers
builder.Services.AddScoped<JWTService>();

//defining our identity core service
builder.Services.AddIdentityCore<User>(options =>
{
    //password configuration
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit  = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;

    //for email confirmation
    options.SignIn.RequireConfirmedEmail = true;
})
    .AddRoles<IdentityRole>() // be able to add roles
    .AddRoleManager<RoleManager<IdentityRole>>() // be able to make use of RoleManager
    .AddEntityFrameworkStores<Context>() // Providing our context
    .AddSignInManager<SignInManager<User>>() // make use of signin manager
    .AddUserManager<UserManager<User>>() // make user   of user manager to create user
    .AddDefaultTokenProviders(); // be able to create tokens for email confirmation 


//be able to authenticate user using jwt token

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // validate the token based on the key we have provided inside appsettings.development.json JWT:Key
            ValidateIssuerSigningKey = true,
            //the issuer signing keuy based on JWT:KEy
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
            // the issuer which in here is the api project url we are using
            ValidIssuer = builder.Configuration["JWT:Issuer"],
            // validate the issuer (who ever is issuing the JWT)
            ValidateIssuer = true,
            // dont validate audience (angular side)
            ValidateAudience = false
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


// adding userAuthentication into our pipeline and this should come before useAuthorization
// Authentication verifies the indentity of a user or servivce, and authorization determines their access rights.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
