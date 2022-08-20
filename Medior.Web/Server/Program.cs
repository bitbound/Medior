using Medior.Shared.Auth;
using Medior.Shared.Services;
using Medior.Web.Server.Auth;
using Medior.Web.Server.Data;
using Medior.Web.Server.Hubs;
using Medior.Web.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDb>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("SQLite"));
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = AuthSchemes.DigitalSignature;
    options.AddScheme(AuthSchemes.DigitalSignature, builder =>
    {
        builder.DisplayName = "Digital Signature";
        builder.HandlerType = typeof(DigitalSignatureAuthenticationHandler);
    });
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(PolicyNames.DigitalSignaturePolicy, builder =>
    {
        builder.AddAuthenticationSchemes(AuthSchemes.DigitalSignature);
        builder.RequireAssertion(x =>
        {
            return x.User?.Identity?.IsAuthenticated == true;
        });
        builder.RequireClaim(ClaimNames.PublicKey);
        builder.RequireClaim(ClaimNames.Username);
    });
});

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddSignalR(options =>
    {
        options.MaximumParallelInvocationsPerClient = 5;
    })
    .AddMessagePackProtocol();

builder.Services.AddHostedService<DbCleanupService>();
builder.Services.AddSingleton<IAppSettings, AppSettings>();
builder.Services.AddSingleton<ISystemTime, SystemTime>();
builder.Services.AddSingleton<IHubSessionCache, HubSessionCache>();
builder.Services.AddSingleton<IDesktopStreamCache, DesktopStreamCache>();
builder.Services.AddScoped<IClipboardSyncService, ClipboardSyncService>();
builder.Services.AddScoped<IUploadedFileManager, UploadedFileManager>();
builder.Services.AddScoped<IEncryptionService, EncryptionService>();
builder.Services.AddScoped<DigitalSignatureFilterAttribute>();
builder.Host.UseSystemd();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.UseEndpoints(config =>
{
    config.MapHub<DesktopHub>("/hubs/desktop");
});

app.MapFallbackToFile("index.html", new StaticFileOptions()
{
    OnPrepareResponse = context =>
    {
        context.Context.Response.Headers.TryAdd("Cache-Control", "no-store");
    }
});


using (var scope = app.Services.CreateScope())
{
    using var appDb = scope.ServiceProvider.GetRequiredService<AppDb>();
    await appDb.Database.MigrateAsync();
}

app.Run();
