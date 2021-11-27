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