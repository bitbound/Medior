using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

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

app.UseAuthorization();

app.MapControllers();

app.Run();


void ConfigureStaticFiles()
{
    app.UseStaticFiles();
    var downloadsDir = Path.Combine(app.Environment.WebRootPath, "downloads");
    Directory.CreateDirectory(downloadsDir);
    app.UseStaticFiles(new StaticFileOptions()
    {
        FileProvider = new PhysicalFileProvider(downloadsDir),
        ServeUnknownFileTypes = true,
        RequestPath = new PathString("/downloads"),
        ContentTypeProvider = new FileExtensionContentTypeProvider(),
        DefaultContentType = "application/octet-stream"
    });
}