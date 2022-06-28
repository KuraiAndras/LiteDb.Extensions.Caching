# LiteDb.Extensions.Caching [![Nuget](https://img.shields.io/nuget/v/LiteDb.Extensions.Caching)](https://www.nuget.org/packages/LiteDb.Extensions.Caching) [![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=KuraiAndras_LiteDb.Extensions.Caching&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=KuraiAndras_LiteDb.Extensions.Caching) [![Coverage](https://sonarcloud.io/api/project_badges/measure?project=KuraiAndras_LiteDb.Extensions.Caching&metric=coverage)](https://sonarcloud.io/summary/new_code?id=KuraiAndras_LiteDb.Extensions.Caching)

An `IDistributedCache` implementation for [LiteDB](https://www.litedb.org/).

Usage:

```csharp

IServiceCollection services = //...

services.AddLiteDbCache("MyCache.db")
// or
services.AddLiteDbCache("MyCache.db", "MySuperStrongPassword1111!!!")
// or
services.AddLiteDbCache(options =>
{
    options.CachePath = "MyCache.db";
    options.Password = "MySuperStrongPassword1111!!!";
});

```