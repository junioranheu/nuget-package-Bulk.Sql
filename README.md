## @junioranheu/Bulk.Sql

An easy-to-use and light NuGet package that simplifies the process of performing bulk inserts/deletes in .NET 6 applications. With Bulk.Sql you can efficiently insert and delete large amounts of data into your database tables, improving performance and reducing execution time.

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

### BulkInsert usage

Once you have installed the Bulk.Sql package, you can follow these steps to start using it in your .NET 6 project:

```
await Bulk.Helpers.BulkInsert(list, _context, "TableName");
```

or

```
await Bulk.Helpers.BulkInsert(list, con, "TableName");
```

The first parameter requires a list — commonly resulting from a LINQ query.

The second parameter allows you to pass whether the application's context, a SqlConnection (Microsoft.Data.SqlClient or System.Data.SqlClient), a MySqlConnection or a NpgsqlConnection data base connection.<br/>
E.g.: new SqlConnection(connectionString), new MySqlConnection(connectionString), or NpgsqlConnection(connectionString).

The third parameter requires the aiming table's name.

There are other optional parameters. Be sure to check them out after downloading the package.

### BulkDelete usage

```
await Bulk.Helpers.BulkDelete<ClassHere>(_context, condition: x => x.Status == true && x.Amount >= 22);
```

or

```
await Bulk.Helpers.BulkDelete<ClassHere>(_context);
```

The first parameter requires the application's context.

The second parameter is optional. You can pass a LINQ expression.

## Compatibility

Bulk.Sql is compatible with .NET 6. It supports the following database providers: SQL Server (Microsoft.Data.SqlClient or System.Data.SqlClient), MySQL, and PostgreSQL.

## Support

If you need any assistance or have any questions, feel free to reach out to me at junioranheu@gmail.com.

