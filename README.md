
# 1. Project Overview

## 1.1 Purpose

**Data Availability Dashboard** is a web application for UBS Investment Bank. It provides a **single dashboard** showing daily data load metrics (count, date, etc.) across various data domains, for example:

- **FxRates**  
- **Fixed Income (FI) Analytics**  
- **Security Prices**  
- …and many more (the set of domains is not fixed)

This system is **fully configurable**: simply adding a new row in the **Data Catalog Config Table** (in MS SQL) automatically enables the retrieval and display of that new domain’s metrics.

In **Phase One**, the system will:
1. **Display** the count of data records and the latest load date for each domain.  
2. Potentially show a RAG (Red/Amber/Green) status in future enhancements based on data quality thresholds.

## 1.2 High-Level Data Flow

The **Data Availability Dashboard** solution has **three main components**:

1. **Front-End** (React + UBS NEO):  
   - The user interface displays a grid of domains, counts, dates, etc.  

2. **Orchestrator Backend** (this .NET 8 API):  
   - Reads from the Data Catalog Config Table (MS SQL) to get domain info (URLs, entity keys).  
   - Calls **domain-specific GraphQL endpoints** asynchronously for each domain to retrieve load metrics (count, date).  
   - Aggregates results into a single response.

3. **GraphQL Services** (provided by Data POD):  
   - Each domain (e.g., FxRates, Security Prices) has its own GraphQL endpoint.  
   - The orchestrator backend calls these endpoints, passing the appropriate entity key.

### Steps in the Data Flow

1. **React Front-End** calls our new **.NET 8** “Data Availability Dashboard” backend endpoint (`GET /api/data-availability`) with a valid Azure AD token.  
2. The backend retrieves the list of domains from the **Data Catalog Config Table**.  
3. For each domain, it calls the corresponding **GraphQL endpoint** asynchronously, using fields such as “Data Domain” (e.g., `pricing`, `fianalytics`) and “Entity Key” (e.g., `fxrate`, `benchmarkholding`).  
4. Each GraphQL endpoint returns a **count** and **load date** for the domain.  
5. The backend aggregates these responses into a single JSON payload.  
6. The JSON payload is returned to the **React** front-end, which displays it in the UBS NEO UI.  
7. Both the backend and GraphQL endpoints are **secured by Azure AD**.

---

# 2. Clean Architecture

We will implement the backend following the **Clean Architecture** approach. While this might appear to be an “over-engineered” pattern for a relatively simple orchestrator, the **key advantage** is **future extensibility**. For instance, certain domains may later require direct data retrieval from an MS SQL database or other external APIs. By keeping domain logic separate from infrastructure details, we can easily extend or change how data is fetched without impacting other layers.

The solution is split into four main projects (layers), plus a root solution file. Here is the general folder structure and an explanation of what each layer does:

```
DataAvailabilityDashboard
├── src
│   ├── DataAvailabilityDashboard.Domain
│   │   └── Entities
│   │   │    ├── DataDomain.cs
│   │   │    └── DataMetric.cs
│   ├── DataAvailabilityDashboard.Application
│   │   ├── Interfaces
│   │   ├── Services
│   │   └── DTOs  (optional if we need separate DTO structures)
│   ├── DataAvailabilityDashboard.Infrastructure
│   │   ├── Persistence
│   │   ├── Repositories
│   │   ├── GraphQL
│   └── DataAvailabilityDashboard.WebApi
│       ├── Controllers
│       ├── Filters (optional)
│       └── Program.cs
└── DataAvailabilityDashboard.sln
```

---

# 2.1 Domain Layer

- **Purpose**: Contains **core business entities** and logic that are independent of any external frameworks or technologies.  
- **Key Entities**:
  - **`DataDomain`**  
    ```csharp
    public class DataDomain
    {
        public string Name { get; set; }            // e.g. "FxRates", "Fixed Income (FI) Analytics", ...
        public List<DataMetric> Metrics { get; set; }
    }
    ```
  - **`DataMetric`**  
    ```csharp
    public class DataMetric
    {
        public int Count { get; set; }             // The data count
        public DateTime Date { get; set; }         // The latest load date
    }
    ```

> _Note_: The “Data Catalog Config Table” (which holds `Domain Name`, `Data Domain`, `Entity Key`) is effectively part of the domain concept, but in practical terms, it will be retrieved via a repository in the Infrastructure layer.

---

# 2.2 Application Layer

- **Purpose**:  
  1. Define **Interfaces** (e.g., `IGraphQLService`, `IDataDomainService`, `IConfigRepository`) that describe how the application interacts with external resources or data stores.  
  2. Coordinate application logic: might have **Services**, **Use Cases**, or **Handlers** that orchestrate tasks such as:
     - Retrieving a list of domain configurations from the DB.  
     - Looping through each configuration and calling GraphQL endpoints.  
     - Aggregating the results into domain objects (`DataDomain`).

- **Key Components**:
  1. **Interfaces**  
     - `IConfigRepository`: fetches the “Data Catalog Config” from MS SQL.  
     - `IGraphQLService`: calls the appropriate GraphQL endpoint, given a base URL segment (e.g., `pricing`, `fianalytics`) and an entity key (e.g. `fxrate`, `benchmarkholding`).  
     - `IDataDomainService`: orchestrates the entire flow, aggregating the data into a final response.  
  2. **Services**  
     - `DataDomainService`: A high-level application service (or use case handler).  
       - Calls `IConfigRepository` to get the config entries.  
       - For each entry, calls `IGraphQLService` asynchronously.  
       - Aggregates results into `List<DataDomain>`.  
       - Returns it to the Web API.

---

# 2.3 Infrastructure Layer

- **Purpose**: Provide **implementations** for the interfaces declared in the Application layer. This includes:
  - **Database Access** (e.g., `IConfigRepository` → `ConfigRepository` with EF Core or direct ADO.NET).  
  - **GraphQL Calls** (e.g., `IGraphQLService` → `GraphQLService` using `HttpClient`).  
  - **Security** (Azure AD token acquisition/validation logic could also live here or in the WebApi layer, depending on approach).

## 2.3.1 Persistence

- Contains logic to interact with **MS SQL** and fetch the “Data Catalog Config Table” entries, storing them in a list or an EF Core `DbContext`.
- Example table rows:

| Id (GUID) | Domain Name                | Data Domain   | Entity Key           |
|-----------|----------------------------|---------------|----------------------|
| ...       | FxRates                    | pricing       | fxrate              |
| ...       | Fixed Income (FI) Analytics| fianalytics   | benchmarkholding    |
| ...       | Security Prices           | pricing       | security            |
| ...       | [Your New Domain Here]    | ???           | ???                 |

Adding a new row will cause the orchestrator to retrieve metrics for that domain automatically.

## 2.3.2 GraphQL

- Implements the actual GraphQL POST call.  
- Builds a query string with the needed variables (`entityName`) and the URL path segment from the “Data Domain” column.  
- Deserializes the response into `loadDate` and `count` (returned as `DataMetric`).

---

# 2.4 WebApi (Presentation) Layer

- **Purpose**:  
  - Exposes **HTTP** endpoints (e.g., minimal APIs or controllers) for the React UI.  
  - Receives the request, calls the Application Service to do the necessary orchestration, and returns the aggregated result as JSON.  
  - Handles **Azure AD** authentication/authorization configuration.

- **Key Components**:
  - **Controllers**: E.g., `DataAvailabilityController` with an endpoint `/api/data-availability`.
  - **Security**:  
    - **Azure AD**: The Web API is protected via Azure AD. This means any request to the orchestrator requires a Bearer token from Azure AD.  
    - For the downstream GraphQL endpoints, the WebApi might request **On-Behalf-Of** tokens, or simply forward the existing user token, depending on how the GraphQL services are protected.

---

# 3. Detailed Design

## 3.1 Data Catalog Config Table

Stores the config that maps **Domain Name** → **Data Domain** (URL segment) + **Entity Key** (GraphQL variable). The orchestrator retrieves these rows to know which GraphQL calls to make.

| Id (GUID)  | Domain Name               | Data Domain  | Entity Key        |
|------------|---------------------------|--------------|-------------------|
| <new guid> | FxRates                   | pricing      | fxrate            |
| <new guid> | Fixed Income (FI) Analytics | fianalytics | benchmarkholding  |
| <new guid> | Security Prices           | pricing      | security          |

## 3.2 GraphQL Endpoints

The typical endpoints (DEV, QA, UAT, PROD) look like:
```
DEV:  https://api.cedar-dev.azpriv-cloud.ubs.net/dataservices/<DataDomain>/graphql/
QA:   https://api.cedar-test.azpriv-cloud.ubs.net/dataservices/<DataDomain>/graphql/
UAT:  https://api.cedar-preprod.azpriv-cloud.ubs.net/dataservices/<DataDomain>/graphql/
PROD: https://api.cedar-prod.azpriv-cloud.ubs.net/dataservices/<DataDomain>/graphql/
```
With a query like:
```graphql
query {
  monitoringCounts(entityName: "fxrate") {
    loadDate
    count
  }
}
```

## 3.3 Async Orchestration Flow

1. **Frontend** calls `GET /api/data-availability` with a valid token.  
2. **`DataAvailabilityController`** calls **`DataDomainService`**.  
3. **`DataDomainService`**:
   - Queries **`IConfigRepository`** for config rows.  
   - For each config, calls **`IGraphQLService`** asynchronously.  
   - Aggregates results into `List<DataDomain>`.  
4. **Controller** returns the aggregated list as JSON.

---

# 4. Security Considerations (Azure AD)

1. **API Protection**: The orchestrator WebApi is protected by Azure AD (via `AddMicrosoftIdentityWebApi(...)`, for example). The React UI obtains an **access token** from Azure AD using MSAL in the browser.  
2. **Downstream GraphQL Calls**: If the GraphQL endpoints require their own Azure AD tokens, an **On-Behalf-Of** flow may be used. Alternatively, they may accept the same token or a separate client credential token.  
3. **Configuration**:  
   - Azure AD application registration for the orchestrator.  
   - Possibly separate registration for the GraphQL resource.  
   - Connection details stored securely (e.g., in Key Vault).

---

# 5. Layer-by-Layer Explanation

### 5.1 Domain Layer

**Folder**: `DataAvailabilityDashboard.Domain`  
- **Purpose**: Holds core entities (`DataDomain`, `DataMetric`).  
- **Why**: Keeps business concepts decoupled from external frameworks.

### 5.2 Application Layer

**Folder**: `DataAvailabilityDashboard.Application`  
- **Purpose**: Orchestrates logic. Defines interfaces for data access (Config DB, GraphQL).  
- **Service**: `DataDomainService` uses these interfaces to fetch domain configs, call GraphQL, and build domain objects.

### 5.3 Infrastructure Layer

**Folder**: `DataAvailabilityDashboard.Infrastructure`  
- **Persistence** (`ConfigRepository`): Fetches config from MS SQL.  
- **GraphQL** (`GraphQLService`): Calls domain-specific GraphQL endpoints with `HttpClient`.  
- **Security**: May store Azure AD logic (if needed).

### 5.4 WebApi Layer

**Folder**: `DataAvailabilityDashboard.WebApi`  
- Hosts ASP.NET 8 controllers or minimal APIs.  
- Configures **Azure AD** authentication.  
- Example: `DataAvailabilityController` → returns aggregated data to the front-end.

---

# 6. Example Folder Structure

Below is a reference structure for **.NET 8** and Clean Architecture:

```
DataAvailabilityDashboard.sln
└── src
    ├── DataAvailabilityDashboard.Domain
    │   └── Entities
    │       ├── DataDomain.cs
    │       └── DataMetric.cs
    ├── DataAvailabilityDashboard.Application
    │   ├── Interfaces
    │   │   ├── IConfigRepository.cs
    │   │   ├── IGraphQLService.cs
    │   │   └── IDataDomainService.cs
    │   ├── Services
    │   │   └── DataDomainService.cs
    │   └── DTOs (optional)
    ├── DataAvailabilityDashboard.Infrastructure
    │   ├── Persistence
    │   │   ├── ConfigRepository.cs
    │   │   └── DataCatalogConfig.cs // EF entity or mapping for DB table
    │   ├── GraphQL
    │   │   └── GraphQLService.cs
    │   └── Security
    │       └── (Optional Azure AD OBO logic)
    └── DataAvailabilityDashboard.WebApi
        ├── Controllers
        │   └── DataAvailabilityController.cs
        ├── Filters (optional)
        └── Program.cs
```

---

# 7. Front-End Integration

1. **React UI (UBS NEO)**:  
   - A page “Data Availability Dashboard” calls `GET /api/data-availability` with an Azure AD token.  
   - Receives JSON data and displays it in a grid (domain name, count, date, etc.).  

2. **Expanding**:  
   - If we add more columns in the future (e.g., RAG status, load time), the same architecture supports it with minimal changes.

---

# 8. Deployment

1. **Backend (Orchestrator)**:  
   - Deployed on **UBS ADV AKS** (Azure Kubernetes Service) cluster as a containerized .NET 8 application.  
   - Azure AD app registration is configured to secure the API.

2. **Front-End (React)**:  
   - Hosted according to **UBS NEO** standard deployment procedures and integrated into NEO UI.

3. **GraphQL Services (Data POD)**:  
   - Maintained by other POD teams (FxRates, Security, etc.).  
   - Our orchestrator calls these endpoints with the necessary token or credentials.

---

# 9. Conclusion

The **Data Availability Dashboard** solution uses **.NET 8**, **Clean Architecture**, and **Azure AD** security to create a **configurable**, **extensible**, and **testable** orchestrator API. The three main components—**React Front-End**, **.NET Orchestrator**, and **GraphQL Services**—work together as follows:

- **Front-End**: Displays data to UBS NEO users.  
- **Orchestrator**: Retrieves config from MS SQL, calls GraphQL services asynchronously, aggregates results.  
- **GraphQL Services**: Provide domain-specific data counts and load dates.

**Clean Architecture** provides a future-proof design, allowing us to:
- Easily add new data domains by simply updating the config table.  
- Introduce new data sources (e.g., direct MS SQL queries) without impacting other layers.  
- Maintain clear separation of concerns (Domain, Application, Infrastructure, WebApi).

As new requirements (e.g., additional columns or data domains) appear, the architecture’s flexibility ensures minimal friction for ongoing development.
