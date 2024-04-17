## @junioranheu/Bulk.Sql

An easy-to-use and light NuGet package that simplifies the process of performing bulk inserts in .NET 6 applications. With Bulk.Sql you can efficiently insert large amounts of data into your database tables, improving performance and reducing execution time.

👉 <b>Take a look at the usage section below to see how you can use the NuGet package correctly.</b>

## Deployment

[NuGet Gallery](https://www.nuget.org/packages/Bulk.Sql/).

## Getting Started
### Installation

You can install Bulk.Sql via NuGet Package Manager or by using the .NET CLI.

NuGet Package Manager:

```
Install-Package Bulk.Sql
```

.NET CLI:

```
dotnet add package Bulk.Sql
```

### Usage

Once you have installed the Bulk.Sql package, you can follow these steps to start using it in your .NET 6 project:

```
await BulkInsert(list, _context, "TableName");
```

The first parameter requires a list — commonly resulting from a LINQ query.

The second parameter allows you to pass whether the application's context or a direct SqlConnection/MySqlConnection data base connection.<br/>
E.g.: new SqlConnection(connectionString) or new MySqlConnection(connectionString).

The third parameter requires the aiming table's name.

The fourth parameter, which is both "hidden" and optional, specifies the time limit in seconds for the bulk copy process.

👉 Last but not least: a static using statement is also required to make it work:

```
using static Bulk.BulkCopy;
```

## Compatibility

Bulk.Sql is compatible with .NET 6. It supports the following database providers: SQL Server and MySQL.

## Support

If you need any assistance or have any questions, feel free to reach out to me at junioranheu@gmail.com.
