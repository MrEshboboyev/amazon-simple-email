using Amazon;
using Amazon.Runtime;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Load AWS configuration
var awsOptions = builder.Configuration.GetSection("AWS");
string region = awsOptions["Region"]!;
string accessKey = awsOptions["AccessKey"]!;
string secretKey = awsOptions["SecretKey"]!;

// Register AWS SES
builder.Services.AddSingleton<IAmazonSimpleEmailService>(_ =>
{
    var credentials = new BasicAWSCredentials(accessKey, secretKey);
    return new AmazonSimpleEmailServiceClient(credentials, RegionEndpoint.GetBySystemName(region));
});

// Load email settings
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection(EmailSettings.SectionName));

var app = builder.Build();

app.MapPost("/email", async (string email, IAmazonSimpleEmailService emailService, IOptions<EmailSettings> settings) =>
{
    var request = new SendEmailRequest
    {
        Source = settings.Value.SenderEmail,
        Destination = new Destination { ToAddresses = [email] },
        Message = new Message
        {
            Subject = new Content("Welcome to our Platform!"),
            Body = new Body
            {
                Html = new Content($"<h1>Welcome!</h1><p>You have successfully joined.</p>")
            }
        }
    };

    var response = await emailService.SendEmailAsync(request);
    return Results.Ok(response.MessageId);
});

app.UseHttpsRedirection();
app.Run();
