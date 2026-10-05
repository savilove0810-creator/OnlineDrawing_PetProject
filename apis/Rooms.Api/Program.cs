using Drawing.Shared.Web;
using Microsoft.EntityFrameworkCore;
using Rooms.Api.Data;
using Rooms.Api.Services.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddRoomServices(builder.Configuration);
builder.Services.AddOpenApi();

builder.Services.AddJwtAuthentication(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var database = scope.ServiceProvider.GetRequiredService<AppDbContext>().Database;
    if (database.IsNpgsql())
        database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseApiExceptionHandling();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();