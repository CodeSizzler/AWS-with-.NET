using Amazon.S3;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
//builder.Services.AddAWSService<IAmazonS3>();
 // Get AWS config from appsettings.json

 var awsOptions = builder.Configuration.GetSection("AWS");

 var accessKey = awsOptions["AccessKey"];

 var secretKey = awsOptions["SecretKey"];

 var region = awsOptions["Region"];

 // Register the AWS S3 client with credentials

 builder.Services.AddSingleton<IAmazonS3>(sp =>
     new AmazonS3Client(accessKey, secretKey, Amazon.RegionEndpoint.GetBySystemName(region))
 );
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
