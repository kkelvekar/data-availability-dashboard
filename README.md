# System Design Document

## 1. Overview

This revised system design document outlines the updated architecture for the Data Availability Dashboard. In this release, GraphQL integration has been deprecated for Q2, and the design incorporates several new enhancements. Key improvements include the addition of detailed job statistics on the second (detailed) screen, the introduction of a RAG status indicator on the landing (summary) screen, and various refinements to configuration and data aggregation processes. The downstream Data Services API, the DaDashboard API for orchestrating data presentation, updated configuration tables, and the redesigned UI for both screens collectively improve data clarity, system performance, and the overall user experience for stakeholders monitoring daily data load metrics.

---

## 2. Data Services API (Downstream API)

This API exposes the JobStats table data and allows client systems to retrieve only the most relevant records. It accepts the following parameters:

- **Reference Date (Optional):**
    - Accepts a reference date (e.g., t‑1, t‑7) to specify the cutoff for data retrieval.
    - The API filters the data to include only JobStats records whose `RecordAsOfDate` is on or before the specified reference date.
    - If omitted, the API defaults to today’s date.
        
- **BusinessEntities List (Mandatory):**
    - Specifies the business entities for which records are to be retrieved.
    - Only the JobStats data belonging to these business entities will be returned.
        
Internally, the API applies additional logic to ensure that for each business entity only the records corresponding to its latest available date—as determined by the filtered data—are returned. This means that if a business entity does not have records for the reference date, the most recent records prior to that date are provided.
### Response Example (Job Stats JSON)

Below is an example JSON response representing a set of JobStats records (converted from the original Job Stats schema):

```json
[
  {
    "id": "7bbec91a-dfe0-46f7-a448-050b80112732",
    "businessEntity": "Account Static: Portfolios",
    "jobStart": "2025-04-06T13:29:00",
    "jobEnd": "2025-04-06T13:49:00",
    "jobStatus": "Failure",
    "recordAsOfDate": "2025-04-06T00:00:00+01:00",
    "qualityStatus": "Fail",
    "recordLoaded": 0,
    "recordFailed": 27
  },
  {
    "id": "998d2d97-cfcc-4b0a-8da6-a58f63336daa",
    "businessEntity": "Account Static: Portfolios",
    "jobStart": "2025-04-06T12:45:00",
    "jobEnd": "2025-04-06T13:36:00",
    "jobStatus": "Failure",
    "recordAsOfDate": "2025-04-06T00:00:00+01:00",
    "qualityStatus": "Fail",
    "recordLoaded": 0,
    "recordFailed": 69
  },
  {
    "id": "ea611353-c4c1-4762-8251-13db12512169",
    "businessEntity": "Account Static: Portfolios",
    "jobStart": "2025-04-06T14:52:00",
    "jobEnd": "2025-04-06T15:35:00",
    "jobStatus": "Success",
    "recordAsOfDate": "2025-04-06T00:00:00+01:00",
    "qualityStatus": "Pass",
    "recordLoaded": 263,
    "recordFailed": 0
  },
  {
    "id": "25d0f5a9-fa34-42cb-824d-46b4c72b4c64",
    "businessEntity": "Account Static: Portfolios",
    "jobStart": "2025-04-06T13:04:00",
    "jobEnd": "2025-04-06T13:53:00",
    "jobStatus": "Failure",
    "recordAsOfDate": "2025-04-06T00:00:00+01:00",
    "qualityStatus": "Fail",
    "recordLoaded": 0,
    "recordFailed": 48
  },
  {
    "id": "d3173145-3350-47fe-9f01-805e50442ac7",
    "businessEntity": "Account Static: Portfolios",
    "jobStart": "2025-04-06T13:34:00",
    "jobEnd": "2025-04-06T14:00:00",
    "jobStatus": "Failure",
    "recordAsOfDate": "2025-04-06T00:00:00+01:00",
    "qualityStatus": "Fail",
    "recordLoaded": 0,
    "recordFailed": 26
  },
  {
    "id": "e53f80c9-769a-472f-b754-e695ca5c0623",
    "businessEntity": "Account Static: GIM SCD Mapping",
    "jobStart": "2025-04-07T14:48:00",
    "jobEnd": "2025-04-07T15:13:00",
    "jobStatus": "Failure",
    "recordAsOfDate": "2025-04-07T00:00:00+01:00",
    "qualityStatus": "Fail",
    "recordLoaded": 0,
    "recordFailed": 59
  },
  {
    "id": "9d0cdff8-e0da-4370-9b29-6987699f25f8",
    "businessEntity": "Account Static: GIM SCD Mapping",
    "jobStart": "2025-04-07T13:38:00",
    "jobEnd": "2025-04-07T14:14:00",
    "jobStatus": "Failure",
    "recordAsOfDate": "2025-04-07T00:00:00+01:00",
    "qualityStatus": "Fail",
    "recordLoaded": 0,
    "recordFailed": 34
  },
  {
    "id": "431eeb7d-d006-4730-99ed-9d23546e3fb2",
    "businessEntity": "Account Static: GIM SCD Mapping",
    "jobStart": "2025-04-07T11:40:00",
    "jobEnd": "2025-04-07T12:16:00",
    "jobStatus": "Success",
    "recordAsOfDate": "2025-04-07T00:00:00+01:00",
    "qualityStatus": "Pass",
    "recordLoaded": 213,
    "recordFailed": 0
  },
  {
    "id": "78a670a7-11e3-4a09-b619-b1530f0a3b28",
    "businessEntity": "Account Static: GIM SCD Mapping",
    "jobStart": "2025-04-07T12:15:00",
    "jobEnd": "2025-04-07T13:01:00",
    "jobStatus": "Failure",
    "recordAsOfDate": "2025-04-07T00:00:00+01:00",
    "qualityStatus": "Fail",
    "recordLoaded": 0,
    "recordFailed": 42
  },
  {
    "id": "467eb0fa-f94a-43ab-ae99-68b80aef1aa8",
    "businessEntity": "Account Static: GIM SCD Mapping",
    "jobStart": "2025-04-07T09:24:00",
    "jobEnd": "2025-04-07T10:01:00",
    "jobStatus": "Success",
    "recordAsOfDate": "2025-04-07T00:00:00+01:00",
    "qualityStatus": "Pass",
    "recordLoaded": 236,
    "recordFailed": 0
  }
]
```

---

## 3. DaDashboard API (Orchestrator API)

The DaDashboard API provides two endpoints that aggregate and present data from the Data Services API.

### 3.1. Endpoint 1 – Dashboard Summary

- **Path:** `/api/Dashboard/GetDashboard`
- **Purpose:**
    - To retrieve summary data by calling the downstream API using a specified date (if provided) and a mandatory list of Business Entities.
    - The response includes:
        - **Application Owner**
        - **Business Entity**
        - **Latest Load Date**
        - **Total Records Loaded** (aggregated based on the BusinessEntity and RecordAsOfDate in the JobStats data)
        - **Dependent Functionalities** (mapped and stored in the DaDashboard database)
        - **RAG Status** (calculated based on business rules for each Business Entity)
- **Sequence Diagram:**
![[DaDashboard API Sequence.png]]

**Sequence Flow** (High-Level):

1. **UI** calls `GET /api/Dashboard/GetDashboard`.
    
2. **DaDashboard API** queries **BusinessEntity** to retrieve:
    
    - The **BusinessEntityConfig** for each Business Entity (where to fetch data).
        
    - The **BusinessEntityRAGConfigId** referencing the **BusinessEntityRAGConfig** table.
        
3. **DaDashboard API** calls **Data Services API** to fetch **JobStats** for these entities.
    
4. **DaDashboard API** merges config data with **JobStats**.
    
5. **DaDashboard API** evaluates the **RAG status**:
    
    - Retrieves the Red, Amber, Green expressions from **BusinessEntityRAGConfig**.
        
    - Uses **Dynamic Expresso** in C# to execute the expressions against the retrieved `JobStats`.
        
6. **DaDashboard API** returns the aggregated summary (including RAG status) to the **UI**.
    
7. **UI** displays the summary table with a color-coded RAG status indicator.

#### 3.1.1. **RAG Status Calculation**

To dynamically determine each Business Entity’s **Red/Amber/Green** status, we use:

1. **BusinessEntityRAGConfig**: Stores custom rule expressions in C#-like syntax (for Red, Amber, and Green).  
2. **Dynamic Expresso**: A lightweight C# expression interpreter that compiles the text-based expressions at runtime.  

##### 3.1.1.1 RAG Rules

Below are the core business rules used for RAG determination (stored in `BusinessEntityRAGConfig`):

- **Red** if:  
  ```csharp
  // "Red" if any job has failed status OR no job started after 9:00 AM
  jobStats.Any(j => j.JobStatus.ToLower() == "failed" || j.QualityStatus.ToLower() == "failed")
  || !jobStats.Any(j => j.JobStart >= currentDate.AddHours(9))
  ```

- **Amber** if:  
  ```csharp
  // "Amber" if all jobs succeeded, but at least one has a quality failure
  jobStats.All(j => j.JobStatus.ToLower() == "success")
  && jobStats.Any(j => j.QualityStatus.ToLower() == "failed")
  ```

- **Green** if:  
  ```csharp
  // "Green" if all jobs and quality checks succeeded
  jobStats.All(j => j.JobStatus.ToLower() == "success")
  && jobStats.All(j => j.QualityStatus.ToLower() == "success")
  ```

*(These expressions can be customized or extended in the `BusinessEntityRAGConfig` table as needed.)*

##### 3.1.1.2 Example C# Implementation

Below is a simplified example of how the DaDashboard API might implement the dynamic RAG evaluation using [Dynamic Expresso](https://github.com/dynamicexpresso/DynamicExpresso). The `EvaluateRAGStatus` method compiles and executes the text-based expressions from `BusinessEntityRAGConfig`.

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using DynamicExpresso;

public class JobStat
{
    public string JobStatus { get; set; }
    public string QualityStatus { get; set; }
    public DateTime JobStart { get; set; }
}

public class RAGStatusEvaluator
{
    private readonly Interpreter _interpreter;

    public RAGStatusEvaluator()
    {
        // Enable lambda expression support
        var options = InterpreterOptions.Default | InterpreterOptions.LambdaExpressions;
        _interpreter = new Interpreter(options);

        // Reference necessary assemblies/types for LINQ and your custom model
        _interpreter.Reference(typeof(Enumerable));
        _interpreter.Reference(typeof(JobStat));
    }

    /// <summary>
    /// Evaluates the RAG status by executing the provided rule expressions in priority order:
    /// Red > Amber > Green.
    /// </summary>
    public string EvaluateRAGStatus(
        IEnumerable<JobStat> jobStats,
        DateTime currentDate,
        string redExpression,
        string amberExpression,
        string greenExpression)
    {
        // 1) Check Red
        var redRule = _interpreter.ParseAsDelegate<Func<IEnumerable<JobStat>, DateTime, bool>>(
            redExpression, "jobStats", "currentDate");
        if (redRule(jobStats, currentDate))
            return "Red";

        // 2) Check Amber
        var amberRule = _interpreter.ParseAsDelegate<Func<IEnumerable<JobStat>, DateTime, bool>>(
            amberExpression, "jobStats", "currentDate");
        if (amberRule(jobStats, currentDate))
            return "Amber";

        // 3) Check Green
        var greenRule = _interpreter.ParseAsDelegate<Func<IEnumerable<JobStat>, DateTime, bool>>(
            greenExpression, "jobStats", "currentDate");
        if (greenRule(jobStats, currentDate))
            return "Green";

        // Fallback if none match
        return "Unknown";
    }
}
```

1. **Expressions Stored in SQL**:  
   The text-based logic for Red/Amber/Green (shown above) is saved in `BusinessEntityRAGConfig` as `RedExpression`, `AmberExpression`, and `GreenExpression`.

2. **Runtime Evaluation**:  
   - For each **Business Entity**, the DaDashboard API fetches the entity’s BusinessEntityRAGConfigId.  
   - Loads the relevant rule expressions.  
   - Invokes `EvaluateRAGStatus(...)` with the current day’s `JobStat` records.

3. **Priority**:  
   - **Red** is checked first. If any condition is met, we mark “Red” and skip further checks.  
   - If not Red, we test **Amber**.  
   - If neither Red nor Amber, we test **Green**.  

This ensures each entity is classified under the first matching rule.
### 3.2. Endpoint 2 – Detailed Job History

- **Path:** `/api/Dashboard/GetDashboard/{BusinessEntityName}/{LatestLoadDate}`
- **Purpose:**
    - To retrieve detailed job history for the selected Business Entity and load date.
    - The response includes:
        - **Business Entity**
        - **JobStartDateTime**
        - **JobEndDateTime**
        - **JobStatus**
        - **RecordAsOfDate**
        - **DataQualityStatus**
        - **RecordLoaded**
        - **RecordFailed**
- Sequence Diagram:
 ![[DaDashboard Detail API Sequence diagram 2.png]]

**Sequence Flow** (High-Level):

1. **UI** calls `/api/Dashboard/GetDashboard/{BusinessEntityName}/{LatestLoadDate}` after a user clicks on a row in the summary.
    
2. **DaDashboard API** fetches detailed job data from the **Data Services API** for that entity and date.
    
3. **DaDashboard API** returns the detailed job records to the **UI**.
    
4. **UI** displays the historical job data.

---

## 4. Revised Configuration Tables Schemas

### 4.1. BusinessEntity (Main Table)

| Column                            | Type             | Description                                                                          |
| --------------------------------- | ---------------- | ------------------------------------------------------------------------------------ |
| **Id** (PK)                       | uniqueidentifier | Unique ID for each Business Entity                                                   |
| **ApplicationOwner**              | nvarchar(100)    | Owner of the data (e.g., Data Services)                                              |
| **Name**                | nvarchar(100)    | Business Entity identifier in the system                                             |
| **DisplayName**                   | nvarchar(100)    | Display name for the Business Entity                                                 |
| **DependentFunctionalities**      | nvarchar(max)    | List (or JSON array) of functionalities impacted or dependent on the Business Entity |
| **DataDomainSourceConfigId** (FK) | uniqueidentifier | Foreign key referencing the BusinessEntityConfig                                   |
| **BusinessEntityRAGConfigId** (FK)                     | uniqueidentifier | Foreign key referencing the BusinessEntityRAGConfig                                            |
| **IsActive**                      | bit              | Indicates if the domain is active                                                    |
| **CreatedDate**                   | datetime2        | Record creation date (for auditing)                                                  |
| **UpdatedDate**                   | datetime2        | Last update date (for auditing)                                                      |

### 4.2. BusinessEntityConfig

| Column          | Type             | Description                                                                   |
| --------------- | ---------------- | ----------------------------------------------------------------------------- |
| **Id**          | uniqueidentifier | Unique ID for each domain configuration                                       |
| **Name**        | nvarchar(100)    | Configuration name (e.g., "Main REST Config", "Secondary Config")             |
| **Metadata**    | nvarchar(max)    | JSON formatted metadata (e.g., configuration settings from application files) |
| **CreatedDate** | datetime2        | Record creation date (for auditing)                                           |
| **UpdatedDate** | datetime2        | Last update date (for auditing)                                               |

### 4.3. BusinessEntityRAGConfig

| Column              | Type             | Description                                    |
| ------------------- | ---------------- | ---------------------------------------------- |
| **Id**              | uniqueidentifier | Unique ID for each RAG rule                    |
| **RedExpression**   | nvarchar(max)    | Text based logical expression for RED Status   |
| **AmberExpression** | nvarchar(max)    | Text based logical expression for AMBER Status |
| **GreenExpression** | nvarchar(max)    | Text based logical expression for GREEN Status |
| **Description**     | nvarchar(200)    | RAG rule description                           |
| **CreatedDate**     | datetime2        | Record creation date (for auditing)            |
| **UpdatedDate**     | datetime2        | Last update date (for auditing)                |

### 4.4. Sample Data for Configuration Tables

#### 4.4.1 BusinessEntityConfig Sample Data

|Id|Name|Metadata|CreatedDate|UpdatedDate|
|---|---|---|---|---|
|3F2504E0-4F89-11D3-9A0C-0305E82C3301|DataServices REST Config|{ "ServiceName": { "DevUrl": "[https://api-dev.example.com/data](https://api-dev.example.com/data)", "QaUrl": "[https://api-dev.example.com/data](https://api-dev.example.com/data)", "PreProdUrl": "[https://api-dev.example.com/data](https://api-dev.example.com/data)", "ProdUrl": "[https://api-dev.example.com/data](https://api-dev.example.com/data)" } }|2025-01-01T00:00:00|2025-02-01T00:00:00|
|9D4E1E23-3C8B-4C63-889D-8E0C23A12345|Poisoning Keeping Service Kafka Config|{ "KafkaConfig": { "BootstrapServers": "server1:9092,server2:9092", "Topic": "dataTopic", "GroupId": "group1", "SecurityProtocol": "SSL" } }|2025-01-15T00:00:00|2025-02-15T00:00:00|

#### 4.4.2 BusinessEntityRAGConfig Sample Data

| Id                                   | RedExpression                                                                                                            | AmberExpression                                                                                                           | GreenExpression                                                                                                            | Description                                                                                                                                                     | CreatedDate         | UpdatedDate         |
| ------------------------------------ | ------------------------------------------------------------------------------------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------- | ------------------- |
| 3F2504E0-4F89-11D3-9A0C-0305E82C3309 | jobStats.Any(j => j.JobStatus.ToLower() == "failed")                                                                     | jobStats.All(j => j.JobStatus.ToLower() == "success") && jobStats.Any(j => j.QualityStatus.ToLower() == "failed")         | jobStats.All(j => j.JobStatus.ToLower() == "success") && jobStats.All(j => j.QualityStatus.ToLower() == "success")         | Basic configuration: Red if any job fails; Amber if all jobs succeed but a quality check fails; Green if every job and quality check succeeds                   | 2025-01-01T00:00:00 | 2025-02-01T00:00:00 |
| 9D4E1E23-3C8B-4C63-889D-8E0C23A12347 | jobStats.Any(j => j.JobStatus.ToLower() == ""failed"")<br>\|\| !jobStats.Any(j => j.JobStart >= currentDate.AddHours(9)) | jobStats.All(j => j.JobStatus.ToLower() == ""success"")<br>&& jobStats.Any(j => j.QualityStatus.ToLower() == ""failed"")) | jobStats.All(j => j.JobStatus.ToLower() == ""success"")<br> && jobStats.All(j => j.QualityStatus.ToLower() == ""success"") | Extended configuration: Red if any job fails or if no job starts after 9 AM; Amber if all jobs succeed but at least one quality check fails; Green if all pass. | 2025-01-15T00:00:00 | 2025-02-15T00:00:00 |

#### 4.4.3 BusinessEntity Sample Data

| Id                                   | ApplicationOwner | Name                  | DisplayName     | DependentFunctionalities                     | DataDomainSourceConfigId             | RAGRuleConfigID                      | IsActive | CreatedDate         | UpdatedDate         |
| ------------------------------------ | ---------------- | ------------------------------- | --------------- | -------------------------------------------- | ------------------------------------ | ------------------------------------ | -------- | ------------------- | ------------------- |
| 5A1C4E2D-1234-5678-9ABC-DEF123456789 | Data Services    | Account Static: Portfolios      | Portfolios      | ["Portal", "Portfolio Services", "Currency"] | 3F2504E0-4F89-11D3-9A0C-0305E82C3301 | 3F2504E0-4F89-11D3-9A0C-0305E82C3309 | true     | 2025-03-01T00:00:00 | 2025-03-10T00:00:00 |
| 6B2D5F3E-2345-6789-ABCD-EF1234567890 | Data Services    | Account Static: GIM SCD Mapping | GIM SCD Mapping | ["Portal", "Portfolio Services", "Currency"] | 3F2504E0-4F89-11D3-9A0C-0305E82C3301 | 9D4E1E23-3C8B-4C63-889D-8E0C23A12347 | true     | 2025-03-01T00:00:00 | 2025-03-10T00:00:00 |
| 7C3E6G4F-3456-789A-BCDE-F12345678901 | Data Services    | Benchmark                       | Benchmark       | ["Strategy Manager"]                         | 9D4E1E23-3C8B-4C63-889D-8E0C23A12345 | 3F2504E0-4F89-11D3-9A0C-0305E82C3309 | true     | 2025-03-01T00:00:00 | 2025-03-10T00:00:00 |

---

## 5. UI Design

The UI consists of two screens: a landing (summary) screen and a detailed job history screen.

### 5.1. Landing Screen

- **Context:**
    - Displayed when the user clicks on the platform tile in the UBS Advantage Portal.
    - Retrieves data from the DaDashboard API endpoint: `/api/Dashboard/GetDashboard`.
    - Typically displays data based on the latest available load date.
- **Displayed Columns:**

|Application Owner|Business Entity|Latest Load Date|Total Records Loaded|Dependent Functionalities|RAG Status (Validation Checks)|
|---|---|---|---|---|---|
|Data Services|Account Static: Portfolios|14-03-2025|250|Portal, Portfolio Services, Currency|A|
|Data Services|Account Static: GIM SCD Mapping|14-03-2025|500|Portal, Portfolio Services, Currency|✓|
|Data Services|Account Static: Strategies|14-03-2025|0|Portal, Portfolio Services, Currency|X|
|Data Services|Account Static: Internal Contracts|14-03-2025|180|Portal, Portfolio Services, Currency|A|
|Data Services|Account Static Bank Accounts|14-03-2025|320|Portal, Portfolio Services, Currency|A|
|Data Services|Benchmark|14-03-2025|410|Strategy Manager|✓|
|Data Services|Benchmarks: Composition|14-03-2025|290|Strategy Manager|A|
|Data Services|Benchmarks: Weight Allocation|14-03-2025|360|Strategy Manager|X|
|Data Services|FI Analytics: Benchmark Holding|14-03-2025|275|Optimizer|A|
|Data Services|FI Analytics: Portfolio Holding|14-03-2025|310|Optimizer|✓|
|Data Services|FX Rate|14-03-2025|400|Portal, Portfolio Services, Currency|A|
|Data Services|Security Pricing|14-03-2025|450|Portal, Portfolio Services, Currency|A|
|Data Services|Securities|14-03-2025|500|Portal, Portfolio Services, Currency|✓|
|Data Services|Transactions|14-03-2025|370|Portal, Portfolio Services, Currency|A|

### 5.2. Detailed Screen

- **Context:**
    - Opened when the user selects a row on the landing screen.
    - Displays detailed job history for the selected Business Entity and load date.
    - Data is retrieved from the DaDashboard API endpoint:  
        `/api/Dashboard/GetDashboard/{BusinessEntityID}/{LatestLoadDate}`
- **Displayed Columns:**

|Job Start (Date & Time)|Job End (Date & Time)|Job Status|Record As-Of Date|Data Quality Status|Records Loaded|Records Failed Quality Check|
|---|---|---|---|---|---|---|
|13.03.2025 09:00:00 AM|13.03.2025 09:30:00 AM|Success|12.03.2025|Pass|100|5|
|13.03.2025 10:00:00 AM|13.03.2025 10:30:00 AM|Success|13.03.2025|Pass|150|3|
|13.03.2025 11:00:00 AM|13.03.2025 11:30:00 AM|Failure|13.03.2025|Fail|0|20|
|13.03.2025 11:45:00 AM|13.03.2025 12:45:00 PM|Success|11.03.2025|Fail|250|200|

---

## 6. Column Definitions

1. **Job Start (Date & Time):**
    - Specifies the exact date and time when the job started.
    - Format: `DD.MM.YYYY HH:MM:SS AM/PM`.
2. **Job End (Date & Time):**
    - Specifies the exact date and time when the job ended.
    - Format: `DD.MM.YYYY HH:MM:SS AM/PM`.
3. **Job Status:**
    - Indicates whether the job execution was successful or not.
    - Typical values include `Success` or `Failure`.
4. **Record As-Of Date:**
    - Represents the actual value date of the record.
    - This field may be applicable only for certain Business Entities.
5. **Data Quality Status:**
    - Indicates the outcome of the quality check performed on the loaded data.
    - A value of `Pass` indicates acceptable quality, while `Fail` denotes issues.
6. **Records Loaded:**
    - Denotes the count of records that passed the quality check and were loaded successfully.
7. **Records Failed Quality Check:**
    - Denotes the count of records that failed the quality check and were not loaded.
