using EcommerceAPI.Data;
using EcommerceAPI.Models;
using EcommerceAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using EcommerceAPI.Constants;
using Microsoft.OpenApi.Models;
using System.Text;
using EcommerceAPI.Middlewares;
// 👇 هذه المكتبة تأتي تلقائياً بعد تثبيت Swashbuckle

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

// 👇 5. إعداد Swagger بالطريقة المستقرة ليدعم زر إدخال التوكن
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Ecommerce API", Version = "v1" });

    // تعريف نوع الحماية
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "أدخل التوكن (Token) الخاص بك هنا مباشرة:"
    });

    // تطبيق الحماية
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
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
        }
    });
});

var app = builder.Build();
app.UseMiddleware<ExceptionMiddleware>();
// 👇 6. تشغيل واجهة Swagger
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

// 7. تفعيل الحماية (يجب أن يكون الترتيب Authentication ثم Authorization)
app.UseAuthentication(); 
app.UseAuthorization();

// 8. تشغيل الـ Controllers
app.MapControllers();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

// 9. زراعة البيانات الافتراضية (الصلاحيات ومدير النظام)
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    string[] roles = { AppRoles.SuperAdmin, AppRoles.Admin, AppRoles.User };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

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

        var result = await userManager.CreateAsync(superAdmin, "Super@Admin2026!"); 
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(superAdmin, AppRoles.SuperAdmin);
        }
    }
}

app.Run();