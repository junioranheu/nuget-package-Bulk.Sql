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

Once you have installed the Bulk.Sql package, **make sure to import the proper using statement in your file**, then you can start using it in your .NET 6 project:

```
await Bulk.Insert(list, _context, "TableName");
```

1️⃣ The first parameter requires a list — commonly resulting from a LINQ query.

2️⃣ The second parameter allows you to pass whether the application's context, a SqlConnection (Microsoft.Data.SqlClient or System.Data.SqlClient), a MySqlConnection or a NpgsqlConnection data base connection.<br/>
E.g.: new SqlConnection(connectionString), new MySqlConnection(connectionString), or new NpgsqlConnection(connectionString).

3️⃣ The third parameter requires the aiming table's name.

*️⃣ There are other optional parameters. Be sure to check them out after downloading the package.

⚠️ Important note for PostgreSQL:
When using Bulk.Insert on PostgreSQL tables, columns that are Primary Keys with autoincrement (e.g., SERIAL, BIGSERIAL, IDENTITY) must be passed as null in the list. This way, the database itself will generate the values automatically during the insert. This behavior has only been confirmed for PostgreSQL. It has not been verified for SQL Server or MySQL.

### BulkDelete usage

```
await Bulk.Delete<ClassHere>(_context, condition: x => x.Status == true && x.Amount >= 22);
```

or

```
await Bulk.Delete<ClassHere>(_context);
```

1️⃣ The first parameter requires the application's context.

2️⃣ The second parameter is optional. You can pass a LINQ expression.

⚠️ Important note for PostgreSQL:
Bulk.Delete does not work with PostgreSQL.

## Compatibility

Bulk.Sql is compatible with .NET 6. It supports the following database providers: SQL Server (Microsoft.Data.SqlClient or System.Data.SqlClient), MySQL, and PostgreSQL.

## Support

If you need any assistance or have any questions, feel free to reach out to me at junioranheu@gmail.com.




