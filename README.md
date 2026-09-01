# Cooperativa Financiera El Progreso - Management System

## Project Description
A robust backend console application developed in C# (.NET 10) designed to streamline operations for tellers at "Cooperativa Financiera El Progreso". The system manages single savings accounts per associated member, processes deposits and withdrawals with automated fee calculations, consumes the official open data TRM API asynchronously, and generates comprehensive executive management reports.

## Architecture
The application strictly follows a **3-Layer Architecture** pattern to maintain separation of concerns and avoid tightly coupling the user interface with data management:
- **Models (`/Models`)**: Domain entities (`Associated`, `Movement`, `TrmDto`) and system enums.
- **Data Access Layer (`/Data`)**: Dedicated repository classes (`AssociatedRepository`, `MovementRepository`) that manage in-memory collections and data persistence logic.
- **Services (`/Services`)**: Core business logic, validation rules, reporting algorithms, and asynchronous API communication (`BankingService`, `ExchangeRateService`).
- **Presentation Layer (`/Program.cs`)**: Interactive console user interface rendered in Spanish for end-users, communicating solely with the service layer.

## Technologies Used
- **Language**: C# (.NET 10.0)
- **HTTP Client**: `System.Net.Http` & `System.Text.Json` for asynchronous REST API integration.
- **Design Pattern**: Repository Pattern & Layered Architecture.

## Technical Decisions & Business Rules
1. **Strict Language Compliance**: All internal identifiers, classes, methods, variables, and code comments are fully written in English. User-facing strings (UI) remain in Spanish.
2. **Repository Isolation**: Data collections are safely encapsulated inside the `Data` layer rather than being exposed as local variables in the entry point, ensuring modularity and clean code principles.
3. **Resilient API Consumption**: The application queries the Colombian government's open data portal for the official TRM. If the connection fails, it gracefully notifies the teller and allows normal banking operations to continue without crashing.
4. **Dynamic Balance Calculation**: Balances are never hard-coded or manually updated; they are dynamically computed based on the complete ledger of historical transactions.
5. **Full CRUD Operations**: Complete lifecycle management for associates, including creation, reading, searching, updating, and safe logical deletion guarded by business constraints.

## Class Diagram
```text
[Associated] 1 -------- * [Movement]
  - DocumentNumber : string
  - FullName : string
  - Phone : string
  - Address : string
  - IsActive : bool

[Movement]
  - DocumentNumber : string
  - Type : MovementType (Deposit, Withdrawal)
  - Amount : decimal
  - Commission : decimal
  - Date : DateTime

[TrmDto]
  - Value : string
  - ValidFrom : string
  - ValidUntil : string

[ExchangeRateService]
  - FetchCurrentTrmAsync() : Task<(TrmDto?, string)>

[AssociatedRepository]
  - Insert(Associated)
  - GetByDocument(string)
  - GetByName(string)
  - GetAll()
  - Update(Associated)
  - Delete(string)

[MovementRepository]
  - Insert(Movement)
  - GetByDocument(string)
  - GetAll()

[BankingService]
  - RegisterAssociated(...)
  - CalculateBalance(string)
  - RegisterDeposit(...)
  - RegisterWithdrawal(...)
  - GetBalanceInDollarsAsync(string)
  - GetReport1TotalMoney()
  - GetReport2TopAssociateds()
  - GetReport3SleepingAssociateds()
  - GetReport4PeriodSummary(...)
  - GetReport5LargestMovements()
  - GetReport6MovementActivity()

[Program / UI]
  - Main(string[])
  - ShowReportsMenu(BankingService)
