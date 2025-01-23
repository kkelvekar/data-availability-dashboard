# 1. Project Overview

## 1.1 Purpose

**Data Availability Dashboard** is a web application for UBS Advantage stakeholders. It will be **part of the UBS Advantage Portal** as a **new application**. The dashboard provides a **single view** of daily data load metrics (count, date, etc.) across various data domains (e.g., FxRates, Fixed Income, Security Prices, Account Master, etc.). The system is **configurable**: simply adding a new row in the **database** automatically enables retrieval and display of that domain’s metrics.

In **Phase One**, the system will:
1. **Display** the count of records and the latest load date for each domain.  
2. Potentially show a RAG (Red/Amber/Green) status based on data quality thresholds (future enhancement).

---

# 2. Flow Chart

![Project Screenshot](images/flow-chart.png)

### High-Level Steps

1. **Data Availability Dashboard** (a new React application in UBS Advantage) calls the **Orchestrator API** (`GET /api/data-availability`) with a valid Azure AD token.  
2. The **Orchestrator** reads domain configurations from the **`da_dashboard_db`** (e.g., `DataDomainConfig`, `DomainSourceGraphQL`) to determine each domain’s **SourceType** (GraphQL, REST, SQL, etc.).  
3. For each domain, the Orchestrator **calls** the configured data source asynchronously.  
4. The data source returns **count** of records and **latest load date**.  
5. The Orchestrator **aggregates** results into a **single JSON payload**.  
6. The frontend **renders** the JSON payload in a grid.  
7. **Security**: Both the Orchestrator and data sources enforce **Azure AD** tokens, ensuring no anonymous access.

---

# 3. Frontend (React – UBS Advantage Application)

The **UBS Advantage Portal** already hosts multiple business applications. The **Data Availability Dashboard** will be introduced as a **new application** within this portal, developed using **UBS NEO UI React** and **TypeScript**.

- **Reusability**: We leverage existing libraries for authentication, logging, etc.  
- **Access Control**: A new role, **`PLATFORM-BUSINESS-ANALYST`**, grants read-only access to business analysts monitoring data availability.  
- **UI Wireframe**:  

| Sr.No | Data Domain                | Load Date   | Count |
|-------|----------------------------|------------|-------|
|   1   | Account Master            | 13-01-2025 |   94  |
|   2   | FxRates                   | 13-01-2025 |   50  |
|   3   | Fixed Income (FI) Analy.. | 13-01-2025 |  120  |
|   4   | Security Prices           | 13-01-2025 |  900  |

  - The **frontend** retrieves this data from the **Orchestrator API** (`GET /api/data-availability`) and displays it in a grid.  
  - Future columns (e.g., **RAG status**) can be added.

---

# 4. Backend Configuration Table Structure

The **`da_dashboard_db`** is a **dedicated Azure SQL database** for our new backend API. It includes at least the following two tables:

## 4.1 DataDomainConfig (Main Table)

| Column (PK)    | Type               | Description                                                        |
|----------------|--------------------|--------------------------------------------------------------------|
| **Id**         | `uniqueidentifier` | Unique ID for each domain config                                   |
| **DomainName** | `nvarchar(100)`    | e.g., "FxRates," "Fixed Income (FI) Analytics"                     |
| **SourceType** | `nvarchar(50)`     | e.g., "GraphQL," "REST," "SQL," etc.                               |
| **IsActive**   | `bit`              | Indicates if the domain is active                                  |
| **CreatedDate**| `datetime2`        | For auditing                                                       |
| **UpdatedDate**| `datetime2`        | For auditing                                                       |

## 4.2 DomainSourceGraphQL (Child Table for GraphQL-specific fields)

| Column (PK)      | Type                  | Description                                                               |
|------------------|-----------------------|---------------------------------------------------------------------------|
| **DataDomainId** | `uniqueidentifier` FK | References `DataDomainConfig.Id`                                          |
| **DevBaseUrl**   | `nvarchar(2000)`      | e.g., `https://api.cedar-dev.azpriv-cloud.ubs.net/dataservices`           |
| **QaBaseUrl**    | `nvarchar(2000)`      | e.g., `https://api.cedar-qa.azpriv-cloud.ubs.net/dataservices`            |
| **PreProdBaseUrl**| `nvarchar(2000)`     | e.g., `https://api.cedar-preprod.azpriv-cloud.ubs.net/dataservices`       |
| **ProdBaseUrl**  | `nvarchar(2000)`      | e.g., `https://api.cedar-prod.azpriv-cloud.ubs.net/dataservices`          |
| **EndpointPath** | `nvarchar(500)`       | Often `/<DataDomain>/graphql/` or similar                                 |
| **EntityKey**    | `nvarchar(100)`       | Parameter for the GraphQL query, e.g. `fxrate`, `benchmarkholding`        |

> **Future Data Sources**: For REST or SQL integrations, you can create equivalent tables (e.g., `DomainSourceRest`, `DomainSourceSql`) referencing **DataDomainConfig** similarly. This approach expands **Infrastructure** without impacting the **Domain** or **Application** layers.

---

# 5. Domain Classes & Architecture

## 5.1 Domain Layer

Houses **core entities** that represent the business model:

### 5.1.1 DataDomain

```csharp
public class DataDomain
{
    public string Name { get; set; }  
    public List<DataMetric> Metrics { get; set; }
}
```

### 5.1.2 DataMetric

```csharp
public class DataMetric
{
    public int Count { get; set; }  
    public DateTime Date { get; set; }
}
```

These classes remain **unchanged** even if we introduce new data source types (REST, SQL, etc.).

---

# 6. Backend Orchestrator API (Clean Architecture)

We adopt **Clean Architecture** in the .NET 8 **Orchestrator**:

1. **Domain** (core entities, rules)  
2. **Application** (interfaces, services/use cases)  
3. **Infrastructure** (data source implementations, e.g., GraphQL calls)  
4. **WebApi** (exposes endpoints, handles Azure AD auth)

## 6.1 GraphQL Example

### 6.1.1 GraphQLService

- Uses **`HttpClient`** to call the domain’s GraphQL endpoint.  
- Builds the query using `EntityKey`.  
- Chooses **BaseUrl** based on environment (Dev, QA, PreProd, Prod), then appends **EndpointPath**.  
- Deserializes into `loadDate` and `count`, mapped to **DataMetric**.

### 6.1.2 WebApi Layer

- **Controllers**: e.g., `DataAvailabilityController` → `GET /api/data-availability`  
- **Azure AD**: No anonymous access; the Orchestrator requires valid tokens.  
- **Downstream Security**: If GraphQL also needs Azure AD, the Orchestrator uses On-Behalf-Of or service principal tokens.

### 6.1.3 Detailed Design & Flow

1. **Frontend** calls `GET /api/data-availability` with a valid Azure AD token.  
2. **DataAvailabilityController** → **DataDomainService** (Application layer).  
3. **DataDomainService** → **IConfigRepository** to get domain configs.  
4. For each config with `SourceType = 'GraphQL'`, call **IGraphQLService** (in **Infrastructure**) asynchronously.  
5. Aggregate the resulting `DataMetric` objects into a **DataDomain** list.  
6. Return JSON to the **Frontend**.

**Extensibility**: New data sources can be added by creating additional child tables (e.g., for REST) and corresponding Infrastructure implementations, without modifying the Domain or Application layers.

---

# 7. Security

1. **User Access via UBS Advantage Portal**  
   - Users log into **UBS Advantage** with their corporate credentials.  
   - The **Data Availability Dashboard** is accessible only to those with the **`PLATFORM-BUSINESS-ANALYST`** role, which grants **read-only** privileges for monitoring data availability.  

2. **Frontend → Orchestrator**  
   - The **React UI** in **UBS Advantage** calls the orchestrator API (`GET /api/data-availability`), sending tokens or headers established by the portal’s authentication flow.  
   - The **Orchestrator** leverages the **Pet.Core** authentication library to **validate** that each incoming request:  
     - Originates from a **Neo** user session.  
     - Contains the correct role/permission set (no anonymous access).  

3. **Orchestrator → Data Sources** (GraphQL, REST, SQL)  
   - If a particular data source also requires **Azure AD** (e.g., GraphQL endpoints), the orchestrator obtains tokens via **On-Behalf-Of** or **service principal** flows.  
   - This ensures **end-to-end** authenticated calls, avoiding any anonymous data retrieval.  

4. **Role & Permission Model**  
   - **`PLATFORM-BUSINESS-ANALYST`** is assigned within UBS Advantage to users who need visibility into data availability.  
   - This role ensures **read-only** access in the dashboard, enforced by the orchestrator.

---

# 8. Deployment

1. **Orchestrator (Backend)**  
   - Deployed on **UBS ADV AKS** (Azure Kubernetes Service) as a containerized .NET 8 application.  
   - Secured by Azure AD app registration.

2. **Frontend (UBS Neo)**  
   - Deployed via GitLab CI/CD or UBS NEO procedures.  
   - Integrated into UBS Advantage Portal.

3. **Configuration Database (`da_dashboard_db`)**  
   - Contains tables like `DataDomainConfig` and `DomainSourceGraphQL`.  
   - Managed by Flyway migrations or similar approach in a separate pipeline.

4. **Data Sources** (GraphQL, REST, SQL)  
   - Maintained by respective teams (e.g., Data PODs).  
   - Orchestrator calls them with valid tokens (On-Behalf-Of or service principal).

---

# 9. Conclusion

The **Data Availability Dashboard** provides a **configurable**, **extensible**, and **testable** solution for monitoring daily data load metrics across multiple domains. By adopting **Clean Architecture**:

- **Domain & Application layers** remain stable, even if we add new data sources (GraphQL, REST, SQL).  
- **Infrastructure** can be extended with additional implementations (e.g., child tables, REST clients) without impacting core logic.  
- **Security** is enforced via **Azure AD** tokens, with a new **`PLATFORM-BUSINESS-ANALYST`** role for read-only access.

As new requirements (e.g., additional columns or data domains) emerge, the architecture’s **separation of concerns** ensures minimal friction for ongoing development and maintenance.