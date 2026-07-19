# TMS API Versioning Policy

## 1. What counts as a Breaking Change?
Any change that would break an existing client, including:
- Removing or renaming a JSON field.
- Changing a status code (e.g., 200 OK to 201 Created).
- Adding new required fields to a POST request.

## 2. What counts as Additive (Non-Breaking)?
Changes that existing clients can safely ignore:
- Adding a new optional field to a JSON response.
- Adding an entirely new endpoint.

## 3. Sunset Window
Once a new version (V2) is released, V1 will remain active for a minimum of **6 months** to allow clients time to migrate.

## 4. Communication
- Deprecated versions will carry `Deprecation` and `Sunset` HTTP headers.
- The `Link` header will point to the successor version.

## 5. Skipping Versions
Clients are allowed to jump from V1 to V3 directly; they do not need to implement V2 if a newer version is available.