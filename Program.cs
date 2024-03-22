using ImplementCSVHealper_Read_Csv_Files.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddScoped<CsvFileServices>();
//builder.Services.AddLogging( logbuilder=> {
//    logbuilder.AddConsole();
//});
//builder.Services.AddScoped<ILogger, ILogger>();

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    
//}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
