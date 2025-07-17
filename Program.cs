using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using UnitedGrid.Data;
using UnitedGrid.Data.Providers;
using UnitedGrid.Hubs;
using UnitedGrid.Services;
using UnitedGrid.Services.Background;
using UnitedGrid.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

var sqlCconnectionString = builder.Configuration.GetConnectionString("SqlConnection") ?? throw new InvalidOperationException("Connection string 'SqlConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(sqlCconnectionString));

var redisCconnectionString = builder.Configuration.GetConnectionString("RedisConnection") ?? throw new InvalidOperationException("Connection string 'RedisConnection' not found.");
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(redisCconnectionString));
builder.Services.AddSingleton<IPresenceService, PresenceService>();

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

builder.Services.AddSingleton<IUserIdProvider, NameIdentifierUserIdProvider>();

builder.Services.AddControllersWithViews();

builder.Services.AddSignalR()
    .AddHubOptions<ChatHub>(options =>
        {
            options.EnableDetailedErrors = true;
        })
    //TODO Не запускал 2 экземпляра, надо будет проверять, как подключилось (Пока это писал, появилась мысль переписать все на .net9, там контракты поинтересней xD)
    .AddStackExchangeRedis(redisCconnectionString, options => 
    {
        options.Configuration.ChannelPrefix = "SignalRSync";
    });

builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        ["application/octet-stream"]);
});

builder.Services.AddSingleton<CompositeNotificationService>();
builder.Services.AddHostedService<UnreadNotificationService>();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseStaticFiles();
app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
app.MapHub<ChatHub>("/chatHub");

app.UseResponseCompression();

app.Run();
