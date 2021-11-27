Note: Related medium: https://medium.com/@niteshsinghal85/certificate-based-authentication-in-asp-net-core-web-api-aad37a33d448

# Certificate Base Authentication

## Create Certificate

`dotnet dev-certs https -ep dev_cert.pfx -p 1234`

## Create application

`dotnet new webapi -o CerificateAuth` 

## Add Certificate Authentication

`dotnet add package Microsoft.AspNetCore.Authentication.Certificate`

## Program.cs changes
```c#
public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.ConfigureKestrel(options => {
                        options.ConfigureHttpsDefaults(o => {
                            o.ClientCertificateMode = Microsoft.AspNetCore.Server.Kestrel.Https.ClientCertificateMode.RequireCertificate;
                        });
                    });
                });
```

## Certificate Validaton

```c#
using System.Security.Cryptography.X509Certificates;

public class CertificateValidationService
{
    public bool ValidateCertificate(X509Certificate2 clientCertificate)
    {
        //TODO: In production code use key vault to read this
        var cert = new X509Certificate2("dev_cert.pfx","1234");

        if(clientCertificate.Thumbprint == cert.Thumbprint)
        {
            return true;
        }

        return false;
    }
}
```

## Add CertificateValidation Method

```c#
public static class AuthenticationExtension
    {
        public static void ConfigureAuthetication(this IServiceCollection services)
        {
            services.AddAuthentication(CertificateAuthenticationDefaults.AuthenticationScheme)
                .AddCertificate(options =>
                {
                    options.RevocationMode = X509RevocationMode.NoCheck;
                    options.AllowedCertificateTypes = CertificateTypes.All;
                    options.Events = new CertificateAuthenticationEvents
                    {
                        OnCertificateValidated = context =>
                        {
                            var validationService = context.HttpContext.RequestServices.GetService<CertificateValidationService>();
                            if (validationService!= null && validationService.ValidateCertificate(context.ClientCertificate))
                            {
                                Console.WriteLine("Success");
                                context.Success();
                            }
                            else
                            {
                                Console.WriteLine("invalid cert");
                                context.Fail("invalid cert");
                            }

                            return Task.CompletedTask;
                        }
                    };
                });

            services.AddAuthorization();
        }
    }

```

Startup.cs configuration
```c#
public void ConfigureServices(IServiceCollection services)
{
    //Add required Validation Service for certificate based Auth
    services.AddTransient<CertificateValidationService>();
    //Add required Validation Service for certificate based Auth
    services.ConfigureAuthentication();
    //...
}


public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CerificateAuth v1"));
        }

        app.UseHttpsRedirection();

        app.UseRouting();

        //Add Authentication Middleware
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
}

```

Protect controller with `[Authorize]` attribute.

Client Side:

```c#

using System.Security.Crypthography.X509Certificates;
var cert = new X509Certificate2(@"dev_cert.pfx","1234");
handler.ClientCertificates.Add(cert);
var client = new HttpClient(handler);

var request = new HttpRequestMessage(){
    RequestUri = new Uri("...../weatherforecast"),
    Method=HttpMethod.Get,
}

```