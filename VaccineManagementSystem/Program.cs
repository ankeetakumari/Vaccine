using DNTCaptcha.Core;
using Serilog;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDNTCaptcha(options => options.UseCookieStorageProvider().ShowThousandsSeparators(false));

builder.Services.AddSession();

  builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews();


Log.Logger = new LoggerConfiguration()
.WriteTo.Console()
.WriteTo.File("Logs/log-.txt")
    .Filter.ByIncludingOnly(evt => evt.Level == Serilog.Events.LogEventLevel.Error)
    .CreateLogger();

builder.Host.UseSerilog();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseSession();
app.UseHttpsRedirection();
app.UseStaticFiles();


app.UseRouting();  

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Default}/{id?}");

app.Run();
