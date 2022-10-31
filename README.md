# FileSystemAPI
[![.NET](https://github.com/zagorec92/FileSystemAPI/actions/workflows/dotnet.yml/badge.svg?branch=master)](https://github.com/zagorec92/FileSystemAPI/actions/workflows/dotnet.yml)

### Overview
FileSystemAPI is an attempt to create a fully RESTful API for browser based file system services.  
It is powered by ASP.NET Core 6.0 and EF Core 6 and it supports basic operations on files and directories:
* Read
* Write
* Update
* Move
* Delete  

### Implementation
#### Basics
Directories and files are treated as a content and, as such, stored in the same database table. But, to be able to differentiate between them, following rules are applied:
* **Type** value is different (**0 = directory / 1 = file**)
* The relative **Path** contains the extension if it is related to a file
* Directories can have descendants, files cannot  

And to isolate user's file system, each item has a reference to the **CustomerId**. This value is also a required component of each HTTP request as a prefix in resource URL's, e.g.:
```
https://[Domain]/[CustomerId]/...
```
#### Data 
As mentioned, there is only one table that stores both directories and files.
| Id  | CustomerId | Name | Path | Type | ParentId | Created | Modified | RowVersion |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| GUID  | GUID  | NVARCHAR(255) | NVARCHAR(450) | TINYINT  | GUID | BIGINT  | BIGINT  | ROWVERSION |

* **Id** content identifier
* **CustomerId** customer identifier
* **Name** content name (without the extension if it's a file)
* **Path** path relative to the user's root directory (with an extension if it's a file)
* **ParentId** content identifier which is an ancestor of the current item
* **Created** UNIX timestamp at which the content was created
* **Modified** UNIX timestamp pat which the content was last modified
* **[RowVersion](https://learn.microsoft.com/en-us/sql/t-sql/data-types/rowversion-transact-sql?view=sql-server-ver16)** unique binary value used in concurrency handling

File content will not be stored in a database because this is not a viable solution for large scale file system services (depending on the database, for example, SQL server [stores the data on a separate location on a disk](https://learn.microsoft.com/en-us/sql/relational-databases/blob/filestream-sql-server?view=sql-server-ver16), not in the database file). So, a file content should be physically stored in an actual file system location, preferably not on the same location where the database is found. To enable this, an actual file system interaction needs to be added.
