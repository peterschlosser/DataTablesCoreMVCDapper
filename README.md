# DataTablesCoreMVCDapper
jQuery DataTables Demo using ASP.NET Core MVC and Dapper ORM

A simple and concise demonstration of the [jQuery DataTables plugin](https://datatables.net/) within the ASP.NET Core MVC Framework using Dapper ORM.

A basic<sup name="a1">[1](#f1)</sup> demonstration including sample data and the references one needs to build, demonstrate and [integrate](DataTablesCoreMVCEntity/README.md) DataTables into your project.

## Key Components
  * A ModelBinder to parse the application/x-www-form-urlencoded input from the DataTables AJAX request into the appropriate DataTablesRequest Class Model (data structure) and greatly simplify the interface between Controller (to which the AJAX requests route) and the BLL (Business Logic Layer).
  * (SQLQuery)String and DataTables(Context) Extensions converting the DataTables request for paging, sorting and filtering into generic SQL operations for use by Dapper ORM.  As generics, these methods will function “right out of the box” with most database table models.

## Tested Framework, Tool and Bundle Versions
  * Visual Studio 2017
  * ASP.NET Core 1.1
  * ASP.NET Core 2.0
  * Dapper ORM 1.50.5
  * Bootstrap 3.3.7
  * jQuery 2.2.0
  * DataTables 1.10.16
  * DataTables.net 2.1.1

## Background
When facing the need to use jQuery DataTables in a Core MVC project, this author found it challenging to find examples applicable to ASP.NET Core MVC 1.1 and 2.0 Framework and Dapper ORM.
Some examples were too out of date to compile or function in the current (1.1-2.0) MVC modeling.  Others took the simple DataTables usage and morphed it into complex examples and projects that either failed to build or failed to function.
Efforts were made to keep the code lean, the project and examples simple, while providing a basic<sup>[1](#f1)</sup> demonstration that can be built, observed and tested with minimal effort to the reader.

<b id="f1">1</b> *basic* demonstration refers to the paging, sorting and filtering features of the [DataTables plugin](https://datatables.net/). [↩](#a1)