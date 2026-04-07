using EcommerceAPI.Data;
using EcommerceAPI.Models;
using EcommerceAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using EcommerceAPI.Constants;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. الاتصال بقاعدة البيانات
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. إعداد Identity (نظام المستخدمين والصلاحيات)
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// 3. إعداد JWT (التوكن وحماية الـ APIs)
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JWT:ValidAudience"],
        ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"]!))
    };
});

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICategoryService, CategoryService>(); 
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();

// 4. تفعيل استخدام الـ Controllers
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

// 👇 التعديل الجديد: إعداد Swagger ليدعم إدخال الـ Token
builder.Services.AddSwaggerGen();
var app = builder.Build();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }


    app.UseSwagger();
    app.UseSwaggerUI();









//   "ConnectionStrings": {
//     "DefaultConnection": "Server=db46915.databaseasp.net; Database=db46915; User Id=db46915; Password=o?7GKj#34Fr=; Encrypt=False; MultipleActiveResultSets=True"
//   },



 {/*
 "ConnectionStrings": {
"DefaultConnection": "Server=.;Database=EcommerceDB;Trusted_Connection=True;TrustServerCertificate=True;"
},
 */}

app.UseHttpsRedirection();

// 5. تفعيل الحماية (يجب أن يكون الترتيب Authentication ثم Authorization)
app.UseAuthentication(); 
app.UseAuthorization();

// 6. تشغيل الـ Controllers
app.MapControllers();

// زراعة البيانات الافتراضية (الصلاحيات ومدير النظام)
// كود تقريبي لوضعه في Program.cs بعد app.Run() أو قبلها
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    // 1. التأكد من وجود الأدوار (Roles)
    string[] roles = { AppRoles.SuperAdmin, AppRoles.Admin, AppRoles.User };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    // 2. إنشاء السوبر أدمن اليدوي
    var superAdminEmail = "superadmin@ecommerce.com";
    var user = await userManager.FindByEmailAsync(superAdminEmail);

    if (user == null)
    {
        var superAdmin = new ApplicationUser
        {
            UserName = "superadmin",
            Email = superAdminEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(superAdmin, "Super@Admin2026!"); // الباسورد اليدوي
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(superAdmin, AppRoles.SuperAdmin);
        }
    }
}


app.Run();