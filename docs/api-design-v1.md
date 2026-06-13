# Glowtics — API Specification v1

> Official API contract for the Glowtics skincare recommendation platform.
> This is the authoritative specification for all API endpoints, request/response contracts, and error handling.

---

## Table of Contents

- [Conventions](#conventions)
- [Authentication](#authentication)
- [Standard Error Envelope](#standard-error-envelope)
- [Pagination](#pagination)
- [1. Auth Endpoints](#1-auth-endpoints)
- [2. Profile Endpoints](#2-profile-endpoints)
- [3. Catalog Endpoints](#3-catalog-endpoints)
- [4. Shopper Endpoints](#4-shopper-endpoints)
- [Status Code Reference](#status-code-reference)
- [Error Code Reference](#error-code-reference)

---

## Conventions

| Item | Convention |
|---|---|
| **Base URL** | `https://api.glowtics.com/v1` |
| **Protocol** | HTTPS only |
| **Content-Type** | `application/json` unless otherwise noted |
| **Date format** | ISO 8601 (`2026-06-12T05:37:00Z`) |
| **ID format** | Strings (retailer-defined for products, GUID for internal entities) |
| **Case sensitivity** | Domain lookups are case-insensitive. All other fields are case-sensitive. |
| **Versioning** | URI path prefix `/v1/`. `GET /analyze` is a page route and is unversioned. |

---

## Authentication

Glowtics uses **two authentication mechanisms**:

### 1. JWT Bearer Token — Dashboard & Profile Operations

Obtained via `POST /v1/auth/login`. Sent in the `Authorization` header.

```
Authorization: Bearer <jwt_token>
```

| Claim | Description |
|---|---|
| `sub` | User GUID |
| `email` | User email |
| `iat` | Issued-at timestamp |
| `exp` | Expiration timestamp (24 hours from issue) |


### 2. API Key — Catalog Operations

Generated on demand via `POST /v1/auth/rotate-key`. The full key is shown **only once** — in the rotation response. `GET /v1/profile` returns a masked version (last 4 characters only), or `null` if no key has been generated yet. If the retailer loses the key, they must rotate to generate a new one. Sent in the `X-Glowtics-Key` header.

```
X-Glowtics-Key: <api_key>
```

### Authentication Matrix

| Endpoint Group | Mechanism | Header |
|---|---|---|
| `POST /v1/auth/register` | None | — |
| `POST /v1/auth/login` | None | — |
| `POST /v1/auth/rotate-key` | JWT | `Authorization: Bearer <token>` |
| `GET /v1/profile` | JWT | `Authorization: Bearer <token>` |
| `PATCH /v1/profile` | JWT | `Authorization: Bearer <token>` |
| `* /v1/catalog/**` | API Key | `X-Glowtics-Key: <key>` |
| `GET /analyze` | None | — |
| `POST /v1/analyze/photo` | None | — |

---

## Standard Error Envelope

All error responses follow this structure:

```json
{
  "type": "ERROR_CODE",
  "message": "Human-readable description of the error.",
  "errors": [
    {
      "field": "fieldName",
      "message": "Field-level error description."
    }
  ]
}
```

| Field | Type | Required | Description |
|---|---|---|---|
| `type` | `string` | yes | Machine-readable error code (see [Error Code Reference](#error-code-reference)). |
| `message` | `string` | yes | Human-readable summary. |
| `errors` | `array` | no | Present only for validation errors. Contains per-field details. |

---

## Pagination

Paginated endpoints use **offset-based pagination** via query parameters.

### Request Parameters

| Parameter | Type | Default | Max | Description |
|---|---|---|---|---|
| `page` | `integer` | `1` | — | 1-indexed page number. |
| `pageSize` | `integer` | `25` | `100` | Number of items per page. |

### Response Wrapper

All paginated responses are wrapped in:

```json
{
  "data": [ ... ],
  "pagination": {
    "page": 1,
    "pageSize": 25,
    "totalCount": 142,
    "totalPages": 6
  }
}
```

| Field | Type | Description |
|---|---|---|
| `data` | `array` | The page of results. |
| `pagination.page` | `integer` | Current page number. |
| `pagination.pageSize` | `integer` | Items per page (as applied). |
| `pagination.totalCount` | `integer` | Total number of items across all pages. |
| `pagination.totalPages` | `integer` | Computed: `⌈totalCount / pageSize⌉`. |

---

## 1. Auth Endpoints

### `POST /v1/auth/register`

Creates a new retailer account and provisions a dedicated catalog collection. No API key is generated at this stage — the retailer must call `POST /v1/auth/rotate-key` after login to generate their first API key.

#### Request

```json
{
  "email": "string (required) — valid email format",
  "password": "string (required) — minimum 8 characters",
  "domain": "string (required) — retailer's storefront domain (e.g., skinstore.com)"
}
```

#### Response — `201 Created`

```json
{
  "email": "admin@skinstore.com",
  "domain": "skinstore.com"
}
```

#### Errors

| Status | Type | Condition |
|---|---|---|
| `400` | `VALIDATION_ERROR` | Missing/invalid fields (malformed email, password too short, invalid domain format). |
| `409` | `REGISTRATION_FAILED` | Email or domain already in use. **Message is generic** — does not reveal which field caused the conflict. |

#### Error Examples

**400 — Validation failure:**
```json
{
  "type": "VALIDATION_ERROR",
  "message": "One or more fields failed validation.",
  "errors": [
    { "field": "email", "message": "Must be a valid email address." },
    { "field": "password", "message": "Must be at least 8 characters." }
  ]
}
```

**409 — Duplicate email or domain:**
```json
{
  "type": "REGISTRATION_FAILED",
  "message": "Registration could not be completed. Please verify your information and try again."
}
```

---

### `POST /v1/auth/login`

Authenticates a retailer and returns a JWT access token.

#### Request

```json
{
  "email": "string (required)",
  "password": "string (required)"
}
```

#### Response — `200 OK`

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "expiresIn": 86400
}
```

| Field | Type | Description |
|---|---|---|
| `accessToken` | `string` | JWT token. |
| `expiresIn` | `integer` | Token lifetime in seconds (24 hours = `86400`). |

#### Errors

| Status | Type | Condition |
|---|---|---|
| `400` | `VALIDATION_ERROR` | Missing email or password. |
| `401` | `INVALID_CREDENTIALS` | Email not found or password mismatch. **Generic message** — does not reveal which field is wrong. |

**401 — Invalid credentials:**
```json
{
  "type": "INVALID_CREDENTIALS",
  "message": "The email or password you entered is incorrect."
}
```

---

### `POST /v1/auth/rotate-key`

Generates a new API key for the authenticated retailer. If a key already exists, it is immediately invalidated and replaced. This is the **only way** to obtain an API key — the full key is shown **only in this response**.

#### Auth

`Authorization: Bearer <jwt_token>`

#### Request

No body required.

#### Response — `200 OK`

> [!CAUTION]
> The `apiKey` value is displayed **only in this response**. The retailer must store it securely. It will never be shown in full again.

```json
{
  "apiKey": "glk_a1b2c3d4e5f6..."
}
```

#### Errors

| Status | Type | Condition |
|---|---|---|
| `401` | `UNAUTHORIZED` | Missing, expired, or invalid JWT. |

---

## 2. Profile Endpoints

### `GET /v1/profile`

Returns the authenticated retailer's full profile, including the API key.

#### Auth

`Authorization: Bearer <jwt_token>`

#### Response — `200 OK`

```json
{
  "email": "admin@skinstore.com",
  "domain": "skinstore.com",
  "status": "Pending",
  "apiKeyHint": "glk_••••••a1b2",
  "productEndpoint": null,
  "cartRedirectUrl": null,
  "createdAt": "2026-06-12T05:00:00Z",
  "updatedAt": "2026-06-12T05:00:00Z"
}
```

| Field | Type | Nullable | Description |
|---|---|---|---|
| `email` | `string` | No | Retailer's registered email. |
| `domain` | `string` | No | Retailer's storefront domain. |
| `status` | `string` | No | One of: `Pending`, `Active`, `Suspended`, `Deactivated`. |
| `apiKeyHint` | `string` | Yes | Masked API key — last 4 characters only (e.g., `glk_••••••a1b2`). `null` if no key has been generated yet. Full key is only shown in the `rotate-key` response. |
| `productEndpoint` | `string` | Yes | Retailer's `GET Product By ID` endpoint URL. `null` until set. |
| `cartRedirectUrl` | `string` | Yes | Retailer's cart redirect URL. `null` until set. |
| `createdAt` | `string` | No | ISO 8601 timestamp. |
| `updatedAt` | `string` | No | ISO 8601 timestamp. |

#### Errors

| Status | Type | Condition |
|---|---|---|
| `401` | `UNAUTHORIZED` | Missing, expired, or invalid JWT. |

---

### `PATCH /v1/profile`

Updates the retailer's profile. Can be called multiple times. If both `productEndpoint` and `cartRedirectUrl` are set (either in this request or from previous requests), the retailer's status transitions from `Pending` → `Active`.

An already `Active` retailer can call this endpoint to update their URLs without affecting their status.

#### Auth

`Authorization: Bearer <jwt_token>`

#### Request

At least one field is required. Both must be valid HTTPS URLs.

```json
{
  "productEndpoint": "string (optional) — HTTPS URL",
  "cartRedirectUrl": "string (optional) — HTTPS URL"
}
```

| Field | Type | Required | Description |
|---|---|---|---|
| `productEndpoint` | `string` | ❌ | Retailer's endpoint for fetching product details by ID. Must be HTTPS. |
| `cartRedirectUrl` | `string` | ❌ | URL to redirect shoppers to after product selection. Must be HTTPS. |

> [!IMPORTANT]
> At least one field must be provided. Sending an empty body or a body with no recognized fields returns `400`.

#### Response — `200 OK`

Returns the full updated profile (same shape as `GET /v1/profile`).

```json
{
  "email": "admin@skinstore.com",
  "domain": "skinstore.com",
  "status": "Active",
  "apiKeyHint": "glk_••••••a1b2",
  "productEndpoint": "https://skinstore.com/api/products",
  "cartRedirectUrl": "https://skinstore.com/cart",
  "createdAt": "2026-06-12T05:00:00Z",
  "updatedAt": "2026-06-12T05:10:00Z"
}
```

#### Errors

| Status | Type | Condition |
|---|---|---|
| `400` | `VALIDATION_ERROR` | No fields provided, or URLs are not valid HTTPS. |
| `401` | `UNAUTHORIZED` | Missing, expired, or invalid JWT. |

**400 — Invalid URL:**
```json
{
  "type": "VALIDATION_ERROR",
  "message": "One or more fields failed validation.",
  "errors": [
    { "field": "productEndpoint", "message": "Must be a valid HTTPS URL." }
  ]
}
```

---

## 3. Catalog Endpoints

> [!NOTE]
> All catalog endpoints authenticate via the `X-Glowtics-Key` header.
> Catalog operations are available to `Pending` and `Active` retailers.
> `Suspended` and `Deactivated` retailers receive `403 Forbidden`.

> [!IMPORTANT]
> Glowtics does **not** store product metadata. MongoDB holds only `productId`, `isAvailable`, and the embedding vector. The normalized product data (name, category, ingredients, etc.) is used transiently to generate embeddings and is returned in write responses (POST/PUT) as confirmation, but is not persisted or queryable.

---

### `GET /v1/catalog/products`

Returns a paginated list of all indexed products for the authenticated retailer.

#### Auth

`X-Glowtics-Key: <api_key>`

#### Query Parameters

| Parameter | Type | Default | Max | Description |
|---|---|---|---|---|
| `page` | `integer` | `1` | — | Page number (1-indexed). |
| `pageSize` | `integer` | `25` | `100` | Items per page. |

#### Response — `200 OK`

```json
{
  "data": [
    {
      "productId": "SKU-1042",
      "isAvailable": true
    },
    {
      "productId": "SKU-3321",
      "isAvailable": false
    }
  ],
  "pagination": {
    "page": 1,
    "pageSize": 25,
    "totalCount": 142,
    "totalPages": 6
  }
}
```

#### Catalog Index Item

| Field | Type | Description |
|---|---|---|
| `productId` | `string` | Retailer-defined product identifier. |
| `isAvailable` | `boolean` | Current stock availability. |

#### Errors

| Status | Type | Condition |
|---|---|---|
| `400` | `VALIDATION_ERROR` | Invalid `page` or `pageSize` values. |
| `401` | `UNAUTHORIZED` | Missing or invalid API key. |
| `403` | `FORBIDDEN` | Retailer is `Suspended` or `Deactivated`. |

---

### `GET /v1/catalog/products/{productId}`

Returns a product's availability status by its ID.

#### Auth

`X-Glowtics-Key: <api_key>`

#### Path Parameters

| Parameter | Type | Description |
|---|---|---|
| `productId` | `string` | The retailer-defined product ID. |

#### Response — `200 OK`

```json
{
  "productId": "SKU-1042",
  "isAvailable": true
}
```

#### Errors

| Status | Type | Condition |
|---|---|---|
| `401` | `UNAUTHORIZED` | Missing or invalid API key. |
| `403` | `FORBIDDEN` | Retailer is `Suspended` or `Deactivated`. |
| `404` | `PRODUCT_NOT_FOUND` | No product with this ID exists in the retailer's catalog. |

---

### `POST /v1/catalog/products`

Adds one or more products to the catalog. The request body is a **freeform JSON array** in the retailer's native data format. The AI middleware normalizes each product into the Glowtics canonical schema. Results are reported per-product.

#### Auth

`X-Glowtics-Key: <api_key>`

#### Request

- **Content-Type:** `application/json`
- **Body:** A JSON array of product objects in the retailer's native format.
- **Constraints:** Minimum 1 item, maximum **100** items per request.

```json
[
  {
    "sku": "SKU-1042",
    "title": "Hydrating Cleanser",
    "type": "face wash",
    "description": "A gentle cleanser for dry and sensitive skin...",
    "ingredients": "Water, Hyaluronic Acid, Ceramide NP, ..."
  },
  {
    "sku": "SKU-3321",
    "title": "Retinol Night Serum",
    "type": "serum",
    "description": "...",
    "ingredients": "..."
  }
]
```

> [!NOTE]
> The schema above is an **example** of one retailer's native format. There is no enforced structure — the AI model handles arbitrary JSON. The only requirement is that it must be a JSON array of objects.

#### Response — `207 Multi-Status`

```json
{
  "totalReceived": 2,
  "succeeded": 1,
  "failed": 1,
  "results": [
    {
      "productId": "SKU-1042",
      "status": "created",
      "product": {
        "productId": "SKU-1042",
        "name": "Hydrating Cleanser",
        "category": "Cleanser",
        "targetConditions": ["dryness", "sensitivity"],
        "activeIngredients": ["hyaluronic acid", "ceramides"],
        "conflicts": [],
        "isAvailable": true
      }
    },
    {
      "productId": "SKU-3321",
      "status": "failed",
      "error": {
        "type": "CONFLICT",
        "message": "A product with ID 'SKU-3321' already exists."
      }
    }
  ]
}
```

#### Result Item Schema

| Field | Type | Nullable | Description |
|---|---|---|---|
| `productId` | `string` | Yes | Extracted product ID. `null` if the AI couldn't extract an ID. |
| `status` | `string` | No | One of: `created`, `failed`. |
| `product` | `object` | Yes | The normalized product (canonical schema). Present only when `status` is `created`. |
| `error` | `object` | Yes | Error details. Present only when `status` is `failed`. |
| `error.type` | `string` | — | `CONFLICT` (duplicate ID), `NORMALIZATION_FAILED` (AI couldn't extract required fields). |
| `error.message` | `string` | — | Human-readable error description. |

#### Errors (Request-Level)

| Status | Type | Condition |
|---|---|---|
| `400` | `VALIDATION_ERROR` | Body is not a JSON array, or array is empty. |
| `400` | `BATCH_LIMIT_EXCEEDED` | Array contains more than 100 items. |
| `401` | `UNAUTHORIZED` | Missing or invalid API key. |
| `403` | `FORBIDDEN` | Retailer is `Suspended` or `Deactivated`. |

**400 — Batch limit exceeded:**
```json
{
  "type": "BATCH_LIMIT_EXCEEDED",
  "message": "Request contains 150 products. Maximum batch size is 100."
}
```

---

### `PUT /v1/catalog/products/{productId}`

Updates an existing product. The request body is a **freeform JSON object** in the retailer's native format. The product is re-normalized and re-embedded. The `isAvailable` flag is **preserved** from the previous version.

#### Auth

`X-Glowtics-Key: <api_key>`

#### Path Parameters

| Parameter | Type | Description |
|---|---|---|
| `productId` | `string` | The product ID to update. |

#### Request

- **Content-Type:** `application/json`
- **Body:** A single product object in the retailer's native format.

```json
{
  "sku": "SKU-1042",
  "title": "Hydrating Cleanser — Reformulated",
  "type": "face wash",
  "description": "Now with added ceramide complex...",
  "ingredients": "Water, Hyaluronic Acid, Ceramide NP, Ceramide AP, ..."
}
```

#### Response — `200 OK`

Returns the re-normalized product with the preserved availability flag.

```json
{
  "productId": "SKU-1042",
  "name": "Hydrating Cleanser — Reformulated",
  "category": "Cleanser",
  "targetConditions": ["dryness", "sensitivity"],
  "activeIngredients": ["hyaluronic acid", "ceramide NP", "ceramide AP"],
  "conflicts": [],
  "isAvailable": true
}
```

#### Errors

| Status | Type | Condition |
|---|---|---|
| `400` | `NORMALIZATION_FAILED` | AI could not extract required fields from the input. |
| `401` | `UNAUTHORIZED` | Missing or invalid API key. |
| `403` | `FORBIDDEN` | Retailer is `Suspended` or `Deactivated`. |
| `404` | `PRODUCT_NOT_FOUND` | No product with this ID exists in the retailer's catalog. |

**400 — Normalization failed:**
```json
{
  "type": "NORMALIZATION_FAILED",
  "message": "Could not extract required fields (name, category) from the provided product data."
}
```

---

### `DELETE /v1/catalog/products/{productId}`

Permanently removes a product and its embedding from the catalog.

#### Auth

`X-Glowtics-Key: <api_key>`

#### Path Parameters

| Parameter | Type | Description |
|---|---|---|
| `productId` | `string` | The product ID to delete. |

#### Response — `204 No Content`

No body.

#### Errors

| Status | Type | Condition |
|---|---|---|
| `401` | `UNAUTHORIZED` | Missing or invalid API key. |
| `403` | `FORBIDDEN` | Retailer is `Suspended` or `Deactivated`. |
| `404` | `PRODUCT_NOT_FOUND` | No product with this ID exists in the retailer's catalog. |

---

### `PATCH /v1/catalog/products/{productId}/availability`

Toggles a product's stock availability without re-embedding.

#### Auth

`X-Glowtics-Key: <api_key>`

#### Path Parameters

| Parameter | Type | Description |
|---|---|---|
| `productId` | `string` | The product ID to update. |

#### Request

```json
{
  "isAvailable": false
}
```

| Field | Type | Required | Description |
|---|---|---|---|
| `isAvailable` | `boolean` | ✅ | `true` to restock, `false` to mark out of stock. |

#### Response — `200 OK`

```json
{
  "productId": "SKU-1042",
  "isAvailable": false
}
```

#### Errors

| Status | Type | Condition |
|---|---|---|
| `400` | `VALIDATION_ERROR` | `isAvailable` is missing or not a boolean. |
| `401` | `UNAUTHORIZED` | Missing or invalid API key. |
| `403` | `FORBIDDEN` | Retailer is `Suspended` or `Deactivated`. |
| `404` | `PRODUCT_NOT_FOUND` | No product with this ID exists in the retailer's catalog. |

---

## 4. Shopper Endpoints

> [!NOTE]
> Shopper endpoints are **public** — no authentication required.
> Server-side validation enforces file type and size constraints.

---

### `GET /analyze`

**Page route** (not a versioned JSON API endpoint). Entry point for the shopper diagnostic experience.

Resolves the retailer by domain, validates `Active` status, and serves the diagnostic UI.

#### Query Parameters

| Parameter | Type | Required | Description |
|---|---|---|---|
| `retailer` | `string` | ✅ | The retailer's domain (case-insensitive lookup). |

#### Success — `200 OK`

Serves the diagnostic page/application.

#### Error Responses

| Condition | Behavior |
|---|---|
| `retailer` parameter is missing | Renders an error page: "Invalid request." |
| Domain not found | Renders an error page: "Retailer not found." |
| Retailer exists but is not `Active` | Renders an error page: "This service is currently unavailable for this store." |

> [!NOTE]
> No AI compute is consumed on error paths. The system short-circuits before invoking any pipeline.

---

### `POST /v1/analyze/photo`

Accepts a shopper's facial photo, runs the full AI pipeline (validation → diagnostic → search translation → RAG with data hydration), and returns the personalized routine.

#### Query Parameters

| Parameter | Type | Required | Description |
|---|---|---|---|
| `retailer` | `string` | ✅ | The retailer's domain (case-insensitive lookup). |

#### Request

- **Content-Type:** `multipart/form-data`
- **Max file size:** 10 MB
- **Accepted MIME types:** `image/jpeg`, `image/png`, `image/webp`

| Field | Type | Required | Description |
|---|---|---|---|
| `photo` | `file` | ✅ | The shopper's facial photo. |

#### Response — `200 OK`

```json
{
  "products": [
    {
      "productId": "SKU-1042"
    },
    {
      "productId": "SKU-3321"
    },
    {
      "productId": "SKU-7789"
    }
  ],
  "cartRedirectUrl": "https://skinstore.com/cart"
}
```

| Field | Type | Description |
|---|---|---|
| `products` | `array` | Recommended products from the AI pipeline. |
| `products[].productId` | `string` | The product ID from the retailer's catalog. |
| `cartRedirectUrl` | `string` | The retailer's cart redirect URL. The frontend appends selected product IDs as query parameters (e.g., `?ids=SKU-1042,SKU-3321`). |

> [!TIP]
> **Future extension:** Each product object will include a `summary` field with a consumer-friendly explanation of why the product was recommended.
> ```json
> {
>   "productId": "SKU-1042",
>   "summary": "A gentle cleanser that addresses the dryness detected on your cheeks."
> }
> ```

#### Data Hydration Behavior

The backend calls the retailer's `productEndpoint` for each recommended product before returning the response. Products that fail hydration are included in the response with a `hydrationFailed` flag:

```json
{
  "products": [
    { "productId": "SKU-1042" },
    { "productId": "SKU-3321", "hydrationFailed": true }
  ],
  "cartRedirectUrl": "https://skinstore.com/cart"
}
```

> [!NOTE]
> In the MVP response shape, `hydrationFailed` is the only additional field from hydration. As the response evolves to include live price/image data, hydration failures will be more meaningful to the frontend.

#### Errors

| Status | Type | Condition |
|---|---|---|
| `400` | `VALIDATION_ERROR` | No file attached, or `retailer` query parameter is missing. |
| `400` | `INVALID_FILE_TYPE` | File MIME type is not `image/jpeg`, `image/png`, or `image/webp`. |
| `400` | `FILE_TOO_LARGE` | File exceeds 10 MB. |
| `404` | `RETAILER_NOT_FOUND` | No retailer with this domain exists. |
| `403` | `RETAILER_NOT_ACTIVE` | Retailer exists but is not in `Active` status. |
| `422` | `ANALYSIS_FAILED` | AI pipeline could not produce a valid skin analysis (e.g., no face detected, image too blurry). |
| `500` | `INTERNAL_ERROR` | Unexpected server error during pipeline execution. |

**400 — Invalid file type:**
```json
{
  "type": "INVALID_FILE_TYPE",
  "message": "Unsupported file type. Accepted formats: JPEG, PNG, WebP."
}
```

**422 — Analysis failed:**
```json
{
  "type": "ANALYSIS_FAILED",
  "message": "Could not analyze the photo. Please upload a clear, front-facing facial photo with good lighting."
}
```

---

## Status Code Reference

| Code | Meaning | Used For |
|---|---|---|
| `200` | OK | Successful reads, updates, login. |
| `201` | Created | Successful registration. |
| `204` | No Content | Successful deletion. |
| `207` | Multi-Status | Bulk product add with per-item results. |
| `400` | Bad Request | Validation failures, malformed input, batch limit exceeded, invalid files. |
| `401` | Unauthorized | Missing/invalid/expired JWT or API key. |
| `403` | Forbidden | Retailer is `Suspended`/`Deactivated`, or retailer is not `Active` (shopper flow). |
| `404` | Not Found | Resource does not exist (product, retailer). |
| `409` | Conflict | Registration failed (duplicate email/domain). |
| `422` | Unprocessable Entity | AI pipeline could not process the input (photo analysis failure). |
| `500` | Internal Server Error | Unexpected server-side errors. |

---

## Error Code Reference

| Error Code | HTTP Status | Description |
|---|---|---|
| `VALIDATION_ERROR` | `400` | One or more request fields failed validation. Includes `errors` array with per-field details. |
| `BATCH_LIMIT_EXCEEDED` | `400` | Bulk add request exceeds the 100-product maximum. |
| `INVALID_FILE_TYPE` | `400` | Uploaded file is not an accepted image format. |
| `FILE_TOO_LARGE` | `400` | Uploaded file exceeds the 10 MB size limit. |
| `INVALID_CREDENTIALS` | `401` | Email/password combination is incorrect. Generic — does not reveal which field is wrong. |
| `UNAUTHORIZED` | `401` | JWT or API key is missing, invalid, or expired. |
| `FORBIDDEN` | `403` | Retailer's account status does not permit this operation. |
| `RETAILER_NOT_ACTIVE` | `403` | Retailer exists but is not `Active`. Shopper flow is blocked. |
| `PRODUCT_NOT_FOUND` | `404` | No product with the specified ID exists in the retailer's catalog. |
| `RETAILER_NOT_FOUND` | `404` | No retailer with the specified domain exists. |
| `REGISTRATION_FAILED` | `409` | Registration could not be completed. Generic — does not reveal duplicate email vs. domain. |
| `CONFLICT` | `409` | Product with this ID already exists (used in bulk add per-item errors). |
| `NORMALIZATION_FAILED` | `400`/`207` | AI could not extract required fields from the native product data. |
| `ANALYSIS_FAILED` | `422` | AI pipeline could not produce a skin analysis from the uploaded photo. |
| `INTERNAL_ERROR` | `500` | Unexpected server-side error. |
