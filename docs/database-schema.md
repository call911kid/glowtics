# Glowtics — Database Schema

> This document details the SQL Server schema for the Glowtics backend.
> Note: Product vector embeddings are stored in MongoDB. A local copy of product metadata is stored in SQL Server for dashboard analytics.

---

## 1. `AspNetUsers` (GlowticsUser)
Managed by ASP.NET Core Identity. Handles all authentication and security for retailers.

| Column | Type | Constraints | Description |
|---|---|---|---|
| `Id` | `uniqueidentifier` | Primary Key | Standard Identity GUID. |
| `Email` | `nvarchar(256)` | Unique | The retailer's login email. |
| `UserName` | `nvarchar(256)` | Unique | Maps to the Email. |
| `PasswordHash` | `nvarchar(max)` | | Hashed password. |
| *(Other Identity Columns)* | | | Default ASP.NET Identity columns (e.g., SecurityStamp, PhoneNumber). |

---

## 2. `Retailers`
Holds the Glowtics-specific tenant profile and business logic configurations.

| Column | Type | Constraints | Description |
|---|---|---|---|
| `Id` | `uniqueidentifier` | Primary Key | GUID for the Retailer. |
| `UserId` | `uniqueidentifier` | Foreign Key | Maps 1:1 to `AspNetUsers.Id`. |
| `Domain` | `nvarchar(256)` | Unique | Case-insensitive storefront domain (e.g., skinstore.com). |
| `Status` | `nvarchar(50)` | | State machine value (`Pending`, `Active`, `Suspended`, `Deactivated`). |
| `ApiKeyHash` | `nvarchar(max)` | Nullable | Hashed API key for request validation. |
| `ApiKeyHint` | `nvarchar(16)` | Nullable | Masked key for UI display (e.g., `glk_••••••a1b2`). |
| `ProductEndpoint` | `nvarchar(2048)`| Nullable | HTTPS URL to fetch live product info. |
| `CartRedirectUrl` | `nvarchar(2048)`| Nullable | HTTPS URL for cart handoff. |
| `MongoCollectionName`| `nvarchar(255)` | | System-generated name (e.g., `catalog_<guid>`). |
| `CreatedAt` | `datetime2` | | Timestamp of registration. |
| `UpdatedAt` | `datetime2` | | Timestamp of last profile update. |
| `IsDeleted` | `bit` | | Global soft-delete flag. |

---

## 3. `Products`
A relational copy of the retailer's product catalog used exclusively for fast dashboard analytics. It is kept in sync with MongoDB but does not contain embeddings.

| Column | Type | Constraints | Description |
|---|---|---|---|
| `Id` | `uniqueidentifier` | Primary Key | GUID for the Product in SQL Server. |
| `RetailerId` | `uniqueidentifier` | Foreign Key | Maps to `Retailers.Id`. |
| `ExternalProductId`| `nvarchar(256)` | | The ID the retailer uses natively. |
| `Name` | `nvarchar(256)` | | The normalized product name. |
| `IsAvailable` | `bit` | | Stock status. |
| `IsDeleted` | `bit` | | Soft-delete flag. Kept true instead of hard deleting to preserve dashboard history. |

---

## 4. `DiagnosticSessions`
Tracks the history of end-user diagnoses to power retailer analytics dashboards.

| Column | Type | Constraints | Description |
|---|---|---|---|
| `Id` | `uniqueidentifier` | Primary Key | Session identifier. |
| `RetailerId` | `uniqueidentifier` | Foreign Key | Maps to `Retailers.Id`. Identifies the storefront. |
| `SkinProfileResult` | `nvarchar(max)` | JSON | Structured output from the AI's photo analysis (e.g., conditions, acne severity). |
| `CreatedAt` | `datetime2` | | Timestamp of the diagnosis. |

---

## 5. `DiagnosticSessionProduct` (Implicit Junction)
Managed automatically by EF Core to represent the many-to-many skip navigation between `DiagnosticSessions` and `Products`.

| Column | Type | Constraints | Description |
|---|---|---|---|
| `DiagnosticSessionsId` | `uniqueidentifier` | Foreign Key | Maps to `DiagnosticSessions.Id` |
| `ProductsId` | `uniqueidentifier` | Foreign Key | Maps to `Products.Id` |

---

## MongoDB Boundary
The backend interacts with MongoDB dynamically based on the Retailer.
*   **Database:** `GlowticsVectorDb` (or similar shared DB).
*   **Collection:** Dynamically resolved via `Retailers.MongoCollectionName` (`catalog_<guid>`).
*   **Documents:** Store `productId`, `isAvailable`, and vector arrays. No standard relational schema is enforced.
