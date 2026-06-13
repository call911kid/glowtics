# Glowtics — Database Schema

> This document details the SQL Server schema for the Glowtics backend.
> Note: Product catalogs and embeddings are stored in MongoDB and are omitted from this SQL schema.

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

## 3. `DiagnosticSessions`
Tracks the history of end-user diagnoses to power retailer analytics dashboards.

| Column | Type | Constraints | Description |
|---|---|---|---|
| `Id` | `uniqueidentifier` | Primary Key | Session identifier. |
| `RetailerId` | `uniqueidentifier` | Foreign Key | Maps to `Retailers.Id`. Identifies the storefront. |
| `SkinProfileResult` | `nvarchar(max)` | JSON | Structured output from the AI's photo analysis (e.g., conditions, acne severity). |
| `RecommendedProducts`| `nvarchar(max)` | JSON | Array of `productId`s and the LLM's rationale for recommending them. |
| `CreatedAt` | `datetime2` | | Timestamp of the diagnosis. |

---

## MongoDB Boundary
The backend interacts with MongoDB dynamically based on the Retailer.
*   **Database:** `GlowticsVectorDb` (or similar shared DB).
*   **Collection:** Dynamically resolved via `Retailers.MongoCollectionName` (`catalog_<guid>`).
*   **Documents:** Store `productId`, `isAvailable`, and vector arrays. No standard relational schema is enforced.
