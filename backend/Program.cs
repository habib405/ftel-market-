using Microsoft.EntityFrameworkCore;
using backend;
using backend.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
  options.AddPolicy("AllowFrontend", policy =>
  {
    policy.WithOrigins("http://localhost:3000")
          .AllowAnyHeader()
          .AllowAnyMethod();
  });
});

builder.Services.AddOpenApi();
builder.Services.AddControllers();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
  var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
  db.Database.Migrate();

  if (!await db.Categories.AnyAsync())
  {
    await db.Categories.AddRangeAsync(
      new[]
      {
        new Category { Name = "PlayStation 5" },
        new Category { Name = "Nintendo Switch" },
        new Category { Name = "Xbox Series X" },
        new Category { Name = "PC" }
      }
    );

    await db.SaveChangesAsync();
  }
}

if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.MapControllers();

app.Run();
