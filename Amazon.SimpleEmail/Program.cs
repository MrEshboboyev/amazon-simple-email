using DotNetEnv;
using Amazon.Runtime;
using Amazon;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Microsoft.Extensions.Options;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection(EmailSettings.SectionName));

builder.Services.AddSingleton<IAmazonSimpleEmailService>(_ =>
{
    var accessKey = builder.Configuration["AWS:AccessKey"];
    var secretKey = builder.Configuration["AWS:SecretKey"];
    var region = builder.Configuration["AWS:Region"];

    var credentials = new BasicAWSCredentials(accessKey, secretKey);
    return new AmazonSimpleEmailServiceClient(credentials, RegionEndpoint.GetBySystemName(region));
});

var app = builder.Build();

app.MapPost("/email", async (
    string email,
    IAmazonSimpleEmailService ses,
    IOptions<EmailSettings> settings) =>
{
    var request = new SendEmailRequest
    {
        Source = settings.Value.SenderEmail,
        Destination = new Destination { ToAddresses = [email] },
        Message = new Message
        {
            Subject = new Content("Welcome!"),
            Body = new Body
            {
                Html = new Content("<p>You are now subscribed!</p>")
            }
        }
    };

    var response = await ses.SendEmailAsync(request);
    return Results.Ok(response.MessageId);
});

app.Run();
