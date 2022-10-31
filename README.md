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

### How to run
#### Things to note before
There are 5 launch profiles available:
* **FileSystem** standard launch profile
* **FileSystem_WithGeneratedData** profile that runs the parallel [Task](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task?view=net-6.0) which generates and saves random data
    * Number of randomly generated content can be configured by editing the launch profile's command line args (_[minCustomerCount] [maxCustomerCount] [minContentCount] [maxContentCount]_)  
    ```"commandLineArgs": "100 150 100 1000"```
      * the above example ensures there will be _100-150_ customers and _100-1000_ content items for each one of them (so, worst case, this will generate _150 * 1000_ content items)
* **Docker Compose** (docker-compose project)
  1. Update the connection
* **Docker** profile
* **WSL** profile
#### Running with FileSystem/FileSystem_WithGeneratedData
1. Open up an IDE of your liking (Visual Studio >= 2022 or VS Code with all the packages needed for building and running the app)
2. Run the app (_FileSystem_ launch profile is selected by default)
#### Running with Docker-Compose
  1. Switch to docker-compose project
  2. Update the connection string in [appsettings.Development.json](https://github.com/zagorec92/FileSystemAPI/blob/documentation/FileSystemAPI/FileSystem.API/appsettings.Development.json#L9)
#### Running with Docker
  1. Ensure Docker Desktop is installed
  2. Create and run [SQL server container instance](https://learn.microsoft.com/en-us/sql/linux/quickstart-install-connect-docker?view=sql-server-ver16&pivots=cs1-cmd)
      1. Update the connection string in [appsettings.Development.json](https://github.com/zagorec92/FileSystemAPI/blob/documentation/FileSystemAPI/FileSystem.API/appsettings.Development.json#L9)
#### Running with WSL
  1. Ensure you have WSL version 2 enabled and Ubuntu 20.04 distro installed
  2. Start the distro and
      1. Install ASP.NET Core 6 runtime
      2. Install [VSDebugger](https://vsdebugger.azureedge.net/vsdbg-17-4-11017-1/vsdbg-linux-x64.tar.gz) tool
      3. Install [docker](https://docs.docker.com/engine/install/ubuntu/)
      4. Create and run [SQL server container instance](https://learn.microsoft.com/en-us/sql/linux/quickstart-install-connect-docker?view=sql-server-ver16&pivots=cs1-cmd)
          1. Update the connection string in [appsettings.Development.json](https://github.com/zagorec92/FileSystemAPI/blob/documentation/FileSystemAPI/FileSystem.API/appsettings.Development.json#L9)

### Implementation
#### Basics
Directories and files are treated as a content and, as such, stored in the same database table. But, to be able to differentiate between them, following rules are applied:
* **Type** value is different (**0 = directory / 1 = file**)
* The relative **Path** contains the extension if it is related to a file
* Directories can have descendants, files cannot  

And to _"isolate"_ user's file system, each item has a reference to the **CustomerId**. This value is also a required component of each HTTP request as a prefix in resource URL's, e.g.:
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

File content will not be stored in a database because this is not a viable solution for large scale file system services (depending on the database, for example, SQL server [stores the data on a separate disk location](https://learn.microsoft.com/en-us/sql/relational-databases/blob/filestream-sql-server?view=sql-server-ver16), not in the database file). So, a file content should be physically stored in an actual file system location, preferably not on the same location where the database is found. To enable this, an actual file system interaction needs to be added.  
Directory structure is what you would expect, classic top-down hierarchy with a root directory:
```
└── c145b03e-2597-4ad6-8a2f-d331905658ef
    └── Directory_1
        ├── Directory_1_1
        ├── File_1_1.txt     
    └── Directory_2
└── dbd9168b-da2a-4390-8113-e7096baa78a6
    └── Directory_1   
    └── Directory_2
```
