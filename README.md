Hotel Reservations Manager
1. General Information
Hotel Reservations Manager is an information system for managing hotel reservations.
The system allows management of:
users (employees)
clients
hotel rooms
reservations
The main goal of the system is to facilitate hotel employees in:
creating and managing reservations
checking availability
storing customer information
automatic calculation of the amount due
The system is implemented as a web application developed in C#, which uses MS SQL Server for data storage.

2. System Architecture
The system is built on a three-layer architecture:

1. Presentation Layer (Presentation / UI)
This layer represents the web interface through which users work with the system.
Main functions:
information visualization
data entry
business logic invocation
form validation
Technologies:
C#
ASP.NET / ASP.NET MVC / ASP.NET Core
HTML
CSS
JavaScript

2. Business Logic Layer (BLL)
This layer contains the business logic of the application.
Main responsibilities:
request processing
data validation
calculation of the amount due
availability checking
reservation management
This layer communicates with both the Presentation Layer and the Data Access Layer.

3. Data Access Layer (DAL)
This layer provides communication with the database.
Main functions:
data extraction
data writing
SQL query execution
transaction management
Technologies:
C#
ADO.NET / Entity Framework
MS SQL Server

3. Database
The database is implemented in Microsoft SQL Server.
Main tables:
Users
Clients
Rooms
Reservations
ReservationClients

5. Main functionalities
5.1 User management
Only the administrator can:
create users
edit users
delete users
search users
The system provides:
pagination (10 / 25 / 50 records)
filtering by:
username
first name
last name
email
If an employee is dismissed:
the account becomes inactive
access to the system is blocked
the reservations made are saved

6. Customer management
The system supports CRUD operations for customers:
create
edit
delete
view
When creating a reservation, customers are selected from already existing records.
The system allows:
view all reservations of a client
filter by name
pagination

7. Room management
Only the administrator has the right to:
add rooms
edit rooms
delete rooms
All other users can:
view rooms
filter by:
capacity
type
free/occupied room

8. Reservation management
Each user can:
create reservations
edit reservations
delete reservations
view reservations
When creating a reservation:
A room is selected.
Clients are selected.
Dates are entered.
Type of meal is selected.
The system calculates the total price.

9. Price calculation
The total price is calculated based on:
number of nights
number of adults
number of children
price per bed
additional services

10. Data validation
Each form in the system contains validation.
Examples:
phone number cannot be longer than 10 characters
email must be valid
check-out date cannot be before check-in date
room must have sufficient capacity
reservation can only be made if there is a free room

11. Security
The system includes:
user authentication
role management (Admin / User)
restriction of access to certain functions
password protection

12. User interface
The system contains the following main pages:
Login
Dashboard
Users Management
Clients Management
Rooms Management
Reservations Management
Reservation Details

13. Conclusion
Hotel Reservations Manager provides an effective hotel reservation management system.
By using C# and MS SQL Server, the system provides:
reliable data storage
easy reservation management
fast request processing
convenient user interface
