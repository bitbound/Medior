using Medior.Web.Server.Data;
using Medior.Web.Server.Services;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDb>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("SQLite"));
});

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddHostedService<FileCleanupService>();

builder.Services.AddSingleton<IAppSettings, AppSettings>();

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


app.MapRazorPages();
app.MapControllers();

app.MapWhen(
    context =>
    {
        return context.Request.Path.StartsWithSegments("/api/");
    },
    config =>
    {
        app.MapFallbackToFile("index.html", new StaticFileOptions()
        {
            OnPrepareResponse = context =>
            {
                context.Context.Response.Headers.TryAdd("Cache-Control", "no-store");
            }
        });
    });

using (var scope = app.Services.CreateScope())
{
    using var appDb = scope.ServiceProvider.GetRequiredService<AppDb>();
    await appDb.Database.MigrateAsync();
}
app.Run();
