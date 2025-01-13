# 1. Project Overview

## 1.1 Purpose

**Data Availability Dashboard** is a web application for UBS Advantage Stakeholders. It provides a **single dashboard** showing daily data load metrics (count, date, etc.) across various data domains, for example:

- **FxRates**  
- **Fixed Income (FI) Analytics**  
- **Security Prices**  
- **Account Master**  
- …and many more

This system is **fully configurable**: simply adding a new row in the **DataDomainConfig** (in MS SQL) automatically enables the retrieval and display of that new domain’s metrics.

In **Phase One**, the system will:
1. **Display** the count of data records and the latest load date for each domain.  
2. Potentially show a RAG (Red/Amber/Green) status in future enhancements based on data quality thresholds.

## 1.2 High-Level Data Flow

The **Data Availability Dashboard** solution has **three main components**:

1. **Front-End** (React + UBS NEO):  
   - The user interface displays a grid of domains, counts, dates, etc.

2. **Orchestrator Backend** (this .NET 8 API):  
   - Reads from the **DataDomainConfig** (MS SQL) plus any child configuration table (e.g., **DomainSourceGraphQL**) to get domain info (URLs, entity keys).  
   - Calls **domain-specific GraphQL endpoints** asynchronously for each domain to retrieve load metrics (count, date).  
   - Aggregates results into a single response.

3. **GraphQL Services** (provided by Data POD):  
   - Each domain (e.g., FxRates, Security Prices, etc.) has its own GraphQL endpoint.  
   - The orchestrator backend calls these endpoints, passing the appropriate entity key.

### Steps in the Data Flow

1. **React Front-End** calls our new **.NET 8** “Data Availability Dashboard” backend endpoint (`GET /api/data-availability`) with a valid Azure AD token.  
2. The backend retrieves the list of domains from **DataDomainConfig** (and, for each domain, the child table if `SourceType = 'GraphQL'`).  
3. For each domain, it calls the corresponding **GraphQL endpoint** asynchronously, using fields such as “BaseUrl,” “EndpointPath,” and “EntityKey” from the config tables.  
4. Each GraphQL endpoint returns a **count** and **load date** for that domain.  
5. The backend aggregates these responses into a single JSON payload.  
6. The JSON payload is returned to the **React** front-end, which displays it in the UBS NEO UI.  
7. Both the backend and GraphQL endpoints are **secured by Azure AD**.

---

# 2. (Placeholder) System Design Diagram

*(A diagram would go here, illustrating the React UI → Backend Orchestrator API → GraphQL flow, with Azure AD security on both sides.)*

---

# 3. Folder Structure

Below is the folder structure, featuring **three main folders** (Front-End, Back-End, Database) plus a **.devops** folder at the root for GitLab CI/CD. Each folder has its own pipeline as needed.

```
DaDashboard
├── .devops
│   └── gitlab-ci.yml            // or other DevOps scripts/config for top-level automation
├── Front-End
│   ├── .devops
│   │   └── gitlab-ci.yml        // pipeline config for the React front-end
│   ├── package.json
│   ├── yarn.lock (or package-lock.json)
│   ├── src
│   │   └── (React app source files)
│   └── public
│       └── (Static assets, index.html, etc.)
├── Back-End
│   ├── .devops
│   │   └── gitlab-ci.yml        // pipeline config for .NET solution
│   ├── src
│   │   ├── DaDashboard.sln
│   │   ├── DaDashboard.Domain
│   │   │   └── Entities
│   │   │       ├── DataDomain.cs
│   │   │       └── DataMetric.cs
│   │   ├── DaDashboard.Application
│   │   │   ├── Interfaces
│   │   │   │   ├── IConfigRepository.cs
│   │   │   │   ├── IGraphQLService.cs
│   │   │   │   └── IDataDomainService.cs
│   │   │   ├── Services
│   │   │   │   └── DataDomainService.cs
│   │   │   └── DTOs (optional)
│   │   ├── DaDashboard.Infrastructure
│   │   │   ├── Persistence
│   │   │   │   ├── ConfigRepository.cs
│   │   │   │   ├── DataDomainConfig.cs      // EF/ORM mapping for main config
│   │   │   │   └── DomainSourceGraphQL.cs   // EF/ORM mapping for GraphQL config
│   │   │   ├── GraphQL
│   │   │   │   └── GraphQLService.cs
│   │   │   └── Security
│   │   │       └── (Optional Azure AD OBO logic)
│   │   └── DaDashboard.WebApi
│   │       ├── Controllers
│   │       │   └── DataAvailabilityController.cs
│   │       ├── Filters (optional)
│   │       ├── Middlewares (optional)
│   │       │   └── (Your custom middleware classes)
│   │       └── Program.cs
│   └── tests
│       ├── DaDashboard.Application.Tests
│       ├── DaDashboard.Infrastructure.Tests
│       └── DaDashboard.WebApi.Tests
└── Database
    ├── .devops
    │   └── gitlab-ci.yml        // pipeline config for DB migrations (Flyway, etc.)
    ├── Flyway
    │   └── (Migration scripts)
    └── (Other DB artifacts if needed)
```

---

# 4. (Front-End) UI Sketch

Though the final UI discussion under development, **it will be developed using UBS NEO UI React + TypeScript**. A **rough wireframe** for the daily data load metrics might look like this:

| Sr. No | Data Domain                  | Load Date   | Total Records Loaded |
|--------|------------------------------|------------|----------------------|
| 1      | Account Master              | 13-01-2025 | 94                   |
| 2      | FxRates                     | 13-01-2025 | 50                   |
| 3      | Fixed Income (FI) Analytics | 13-01-2025 | 120                  |
| 4      | Security Prices             | 13-01-2025 | 900                  |

- The front-end retrieves this data from the orchestrator API (`GET /api/data-availability`) and displays it in a grid.  
- Future columns (e.g., RAG status) can be easily added.

---

# 5. Backend Orchestrator API

Below we describe the .NET 8 **Backend Orchestrator API**, including how **Clean Architecture** is implemented.

## 5.1 Clean Architecture & Detailed Explanation

We will implement the backend following the **Clean Architecture** approach. While this might appear to be an “over-engineered” pattern for a relatively simple orchestrator, the key advantage is **future extensibility**. For instance, certain domains may later require direct data retrieval from an MS SQL database or other external APIs. By keeping domain logic separate from infrastructure details, we can easily extend or change how data is fetched without impacting other layers.

### 5.1.1 Domain Layer

**Folder**: `DaDashboard.Domain`

**Purpose**  
Contains **core business entities** and logic that are independent of any external frameworks or technologies.

**Key Entities**  
1. **`DataDomain`**  
   ```csharp
   public class DataDomain
   {
       public string Name { get; set; }            // e.g. "FxRates", "Fixed Income (FI) Analytics", ...
       public List<DataMetric> Metrics { get; set; }
   }
   ```
2. **`DataMetric`**  
   ```csharp
   public class DataMetric
   {
       public int Count { get; set; }             // The data count
       public DateTime Date { get; set; }         // The latest load date
   }
   ```

> **Note**: While `DataDomainConfig` and `DomainSourceGraphQL` are conceptually domain-related, they physically reside in the **Infrastructure** layer as EF/database entities.

### 5.1.2 Application Layer

**Folder**: `DaDashboard.Application`

**Purpose**  
1. Define **Interfaces** (e.g., `IGraphQLService`, `IDataDomainService`, `IConfigRepository`) describing how the application interacts with external resources or data stores.  
2. **Services**, **Use Cases**, or **Handlers** that orchestrate tasks such as:
   - Retrieving domain configurations from the DB.  
   - Looping over each configuration and calling GraphQL endpoints.  
   - Aggregating the results into domain objects (`DataDomain`).

**Key Components**  

- **Interfaces**  
  1. **`IConfigRepository`**: fetches `DataDomainConfig` (and child config) from MS SQL.  
  2. **`IGraphQLService`**: calls the appropriate GraphQL endpoint, given a base URL segment and an entity key.  
  3. **`IDataDomainService`**: orchestrates the entire flow, aggregating the data into a final response.

- **Services**  
  - **`DataDomainService`**  
    1. Calls `IConfigRepository` to get the config entries.  
    2. For each entry, calls `IGraphQLService` asynchronously.  
    3. Aggregates results into `List<DataDomain>`.  
    4. Returns them to the **WebApi** layer.

### 5.1.3 Infrastructure Layer

**Folder**: `DaDashboard.Infrastructure`

**Purpose**  
Provide **implementations** for the interfaces declared in the Application layer. This includes:

- **Database Access** (e.g., `IConfigRepository` → `ConfigRepository` via EF Core or direct ADO.NET).  
- **GraphQL Calls** (e.g., `IGraphQLService` → `GraphQLService` using `HttpClient`).  
- **Security** (Azure AD token acquisition/validation logic, if not placed in WebApi).

#### 5.1.3.1 Persistence

We store domain configurations in **two tables**:

1. **`DataDomainConfig`** (main table)  
   | Column (PK)  | Type               | Description                                                          |
   |--------------|--------------------|----------------------------------------------------------------------|
   | **Id**       | `uniqueidentifier` | Unique identifier for this domain’s config.                         |
   | **DomainName** | `nvarchar(100)`   | e.g. `"FxRates"`, `"Fixed Income (FI) Analytics"`                   |
   | **SourceType** | `nvarchar(50)`    | e.g. `"GraphQL"`, `"REST"`, `"SQL"`, etc.                           |
   | **IsActive** | `bit`             | Indicates if this domain is active                                   |
   | **CreatedDate** | `datetime2`     | Audit field                                                          |
   | **UpdatedDate** | `datetime2`     | Audit field                                                          |

2. **`DomainSourceGraphQL`** (child table for GraphQL-specific fields)  
   | Column (PK)      | Type                  | Description                                                                   |
   |------------------|-----------------------|-------------------------------------------------------------------------------|
   | **DataDomainId** | `uniqueidentifier` FK | References `DataDomainConfig.Id`                                              |
   | **DevBaseUrl**   | `nvarchar(2000)`      | e.g. `https://api.cedar-dev.azpriv-cloud.ubs.net/dataservices`               |
   | **QaBaseUrl**    | `nvarchar(2000)`      | e.g. `https://api.cedar-qa.azpriv-cloud.ubs.net/dataservices` (example)      |
   | **PreProdBaseUrl** | `nvarchar(2000)`    | e.g. `https://api.cedar-preprod.azpriv-cloud.ubs.net/dataservices`           |
   | **ProdBaseUrl**  | `nvarchar(2000)`      | e.g. `https://api.cedar-prod.azpriv-cloud.ubs.net/dataservices`              |
   | **EndpointPath** | `nvarchar(500)`       | Often `/<DataDomain>/graphql/` or similar                                     |
   | **EntityKey**    | `nvarchar(100)`       | Parameter used in the GraphQL query (e.g. `fxrate`, `benchmarkholding`)       |

If `SourceType = 'GraphQL'`, we look up **DomainSourceGraphQL** to get environment-specific Base URLs, `EndpointPath`, and `EntityKey`. Future sources (REST, SQL) can have analogous child tables (e.g. `DomainSourceRest`, `DomainSourceSql`).

#### 5.1.3.2 GraphQL

- **`GraphQLService`** uses `HttpClient` to call the domain’s GraphQL endpoint.  
- Builds the query using `EntityKey`.  
- Chooses the correct **BaseUrl** depending on environment (Dev, QA, PreProd, Prod), appends **EndpointPath**.  
- Deserializes into `loadDate` and `count` (mapped to `DataMetric`).

### 5.1.4 WebApi Layer

**Folder**: `DaDashboard.WebApi`

**Purpose**  
- **Exposes HTTP endpoints** for the React UI.  
- Returns aggregated JSON after orchestrating calls to the Application layer.  
- Handles **Azure AD** auth (no anonymous access).

**Key Components**  
- **Controllers**: e.g., `DataAvailabilityController` → `GET /api/data-availability`  
- **Security**: Orchestrator is secured by Azure AD.  
  - Downstream calls to GraphQL might use On-Behalf-Of tokens or a service principal.

### 5.1.5 Detailed Design & Flow

#### Data Catalog Config Table

**DataDomainConfig** + **DomainSourceGraphQL** store the config in MS SQL.

#### GraphQL Endpoints

Each environment’s **BaseUrl** + **EndpointPath** yields a final call like:

```
DEV:  https://api.cedar-dev.azpriv-cloud.ubs.net/dataservices/pricing/graphql/
QA:   https://api.cedar-qa.azpriv-cloud.ubs.net/dataservices/fianalytics/graphql/
...
```

The orchestrator passes `EntityKey` to the query, for example:

```graphql
query {
  monitoringCounts(entityName: "fxrate") {
    loadDate
    count
  }
}
```

#### Async Orchestration Flow

1. Front-End calls `GET /api/data-availability` with Azure AD token.  
2. `DataAvailabilityController` → `DataDomainService`.  
3. `DataDomainService` → `IConfigRepository` → `IGraphQLService` (async).  
4. Aggregates results → returns `List<DataDomain>` to the controller.  
5. Controller returns JSON.

---

# 6. Deployment

1. **Orchestrator**:  
   - Deployed on **UBS ADV AKS** (Azure Kubernetes Service) as a containerized .NET 8 application.  
   - Azure AD app registration secures the API, ensuring no anonymous access.

2. **Front-End** (React):  
   - Packaged and deployed via GitLab CI/CD or UBS NEO standard procedures.  
   - Integrated into the NEO UI.

3. **Database**:  
   - The **Flyway** migrations (in `Database/.devops`) run in a separate pipeline.  
   - Ensures the `DataDomainConfig` and any child tables (e.g., `DomainSourceGraphQL`) are up to date.

4. **GraphQL Services** (Data POD):  
   - Maintained separately by domain teams.  
   - Our orchestrator calls these endpoints with a valid Azure AD token (service principal or On-Behalf-Of flow).

---

# 7. Security Considerations

1. **Front-End → Orchestrator**  
   - Secured via **Azure AD**; no anonymous access.  
   - The React UI obtains an Azure AD token (MSAL).

2. **Orchestrator → GraphQL**  
   - Uses **service principal** or On-Behalf-Of flow.  
   - All calls are **authenticated**; no domain calls are anonymous.

3. **Configuration**  
   - Azure AD registration for the orchestrator.  
   - Potential separate registration for GraphQL resource.  
   - Secrets stored securely in Key Vault or environment variables.

---

# 8. Conclusion

The **Data Availability Dashboard** solution uses **.NET 8**, **Clean Architecture**, and **Azure AD** security to create a **configurable**, **extensible**, and **testable** orchestrator API. The three main components—**React Front-End**, **.NET Orchestrator**, and **GraphQL Services**—work together as follows:

- **Front-End**: Displays data to UBS NEO users, calls the API with Azure AD tokens.  
- **Orchestrator**: Fetches domain configs from MS SQL, calls GraphQL services asynchronously, aggregates results.  
- **GraphQL Services**: Provide domain-specific data counts and load dates (also Azure AD-protected).

**Clean Architecture** provides a future-proof design, allowing us to:
- Easily add new data domains by inserting rows in **DataDomainConfig** / **DomainSourceGraphQL**.  
- Introduce new data sources (REST, SQL, etc.) by creating additional child tables.  
- Maintain separation of concerns (Domain, Application, Infrastructure, WebApi).  

As new requirements (e.g., additional columns or data domains) appear, the architecture’s flexibility ensures minimal friction for ongoing development.
