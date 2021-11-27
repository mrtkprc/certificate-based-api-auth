# Certificate Base Authentication

## Create Certificate

`dotnet dev-certs https -ep dev_cert.pfx -p 1234`

## Create application

`dotnet new webapi -o CerificateAuth` 

## Add Certificate Authentication

`dotnet add package Microsoft.AspNetCore.Authentication.Certificate`

## Program.cs changes
```

```