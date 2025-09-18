using Amazon.SQS;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
//builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
//builder.Services.AddAWSService<IAmazonSQS>();
var awsOptions = builder.Configuration.GetSection("AWS");

var accessKey = awsOptions["AccessKey"];

var secretKey = awsOptions["SecretKey"];

var region = awsOptions["Region"];
// Register the AWS S3 client with credentials

builder.Services.AddSingleton<IAmazonSQS>(sp =>
    new AmazonSQSClient(accessKey, secretKey, Amazon.RegionEndpoint.GetBySystemName(region))
);
var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
