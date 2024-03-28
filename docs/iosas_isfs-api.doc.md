# ECC.Institute.CRM.IntegrationAPI Documentation

## Overview

The ECC.Institute.CRM.IntegrationAPI provides a comprehensive suite of endpoints for interacting with the MS Dynamics 365 environment, facilitating operations such as health checks, metadata retrieval, and data upsertion for schools, districts, and authorities. This document outlines the available API endpoints, their functionalities, and how to use them.

## API Version: 1.0

---

## Endpoints

### Health Check

- **GET /api/health-check**
  - **Description**: Checks the health of the API.
  - **Responses**:
    - `200`: Success

### Metadata Operations

- **GET /api/Metdata/GetFieldPickListValues**
  - **Parameters**:
    - `tableName` (query): Name of the table.
    - `fieldName` (query): Name of the field.
  - **Description**: Retrieves picklist values for a specified field in a table.
  - **Responses**:
    - `200`: Success

- **GET /api/Metdata/GetAllChoiceValues/{applicationName}**
  - **Parameters**:
    - `applicationName` (path): Name of the application.
    - `tableName` (query): Name of the table.
  - **Description**: Fetches all choice values for a given application and table.
  - **Responses**:
    - `200`: Success

- **GET /api/Metdata/GetMultiSelectPicklistValues/{applicationName}**
  - **Parameters**:
    - `applicationName` (path): Name of the application.
    - `tableName` (query): Name of the table.
  - **Description**: Retrieves multi-select picklist values for a specified table in an application.
  - **Responses**:
    - `200`: Success

- **GET /api/Metdata/GetPickListValues/{applicationName}/{tableName}**
  - **Parameters**:
    - `applicationName` (path): Name of the application.
    - `tableName` (path): Name of the table.
  - **Description**: Fetches picklist values for a specific table in a given application.
  - **Responses**:
    - `200`: Success

- **GET /api/Metdata/GetFieldDescritions**
  - **Parameters**:
    - `applicationName` (query): Name of the application.
    - `tableName` (query): Name of the table.
  - **Description**: Retrieves field descriptions for a given table in an application.
  - **Responses**:
    - `200`: Success

- **GET /api/Metdata/GetMetadataDefinition**
  - **Parameters**:
    - `applicationName` (query): Name of the application.
    - `tableName` (query): Name of the table.
  - **Description**: Fetches metadata definitions for a specified table in an application.
  - **Responses**:
    - `200`: Success

- **GET /api/Metdata/GetSelectedData**
  - **Parameters**:
    - `applicationName` (query): Name of the application.
    - `tableName` (query): Name of the table.
    - `selectQuery` (query): The select query to filter data.
  - **Description**: Retrieves selected data based on a query for a given table in an application.
  - **Responses**:
    - `200`: Success

- **GET /api/Metdata/GetFilteredData**
  - **Parameters**:
    - `applicationName` (query): Name of the application.
    - `tableName` (query): Name of the table.
    - `columns` (query): Columns to filter by.
    - `keyName` (query): Key name for filtering.
    - `values` (query): Values for filtering.
    - `isNumber` (query): Indicates if the key value is a number.
  - **Description**: Fetches filtered data based on specified criteria for a given table in an application.
  - **Responses**:
    - `200`: Success

### School Operations

- **POST /api/School/{applicationName}/AuthorityUpsert**
  - **Parameters**:
    - `applicationName` (path): Name of the application.
  - **Request Body**: Array of `SchoolAuthority` objects.
  - **Description**: Upserts school authority data into the system.
  - **Responses**:
    - `200`: Success

- **POST /api/School/{applicationName}/DistrictUpsert**
  - **Parameters**:
    - `applicationName` (path): Name of the application.
  - **Request Body**: Array of `SchoolDistrict` objects.
  - **Description**: Upserts school district data into the system.
  - **Responses**:
    - `200`: Success

- **POST /api/School/{applicationName}/SchoolUpsert**
  - **Parameters**:
    - `applicationName` (path): Name of the application.
  - **Request Body**: Array of `School` objects.
  - **Description**: Upserts school data into the system.
  - **Responses**:
    - `200`: Success

### Import and Verification

- **POST /import/{applicationName}/{entityName}**
  - **Parameters**:
    - `applicationName` (path): Name of the application.
    - `entityName` (path): Name of the entity to import.
    - `isVerifyOnly` (query): If set to true, verifies data without importing.
  - **Request Body**: Multi-part form data with file.
  - **Description**: Imports or verifies data for a specified entity within an application.
  - **Responses**:
    - `200`: Success

- **GET /import/{applicationName}/status**
  - **Parameters**:
    - `applicationName` (path): Name of the application.
  - **Description**: Retrieves the status of the last import operation for an application.
  - **Responses**:
    - `200`: Success

- **GET /import/{applicationName}/report**
  - **Parameters**:
    - `applicationName` (path): Name of the application.
  - **Description**: Fetches a report of the last import operation for an application.
  - **Responses**:
    - `200`: Success

- **POST /verify/{applicationName}/{entityName}**
  - **Parameters**:
    - `applicationName` (path): Name of the application.
    - `entityName` (path): Name of the entity to verify.
  - **Request Body**: Multi-part form data with file.
  - **Description**: Verifies data for a specified entity within an application.
  - **Responses**:
    - `200`: Success

## Schemas

The API utilizes a set of schemas for request and response bodies, including `Address`, `Contact`, `School`, `SchoolAuthority`, `SchoolContact`, and `SchoolDistrict`. For detailed properties and requirements of these schemas, refer to the API's full documentation.