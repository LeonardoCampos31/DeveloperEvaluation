# Developer Evaluation Project

**`READ CAREFULLY`**

## General Instructions

* **Repository:** The final code must be versioned in a **public** Github repository, and the link must be submitted for evaluation upon completion.
* **Template:** Use this template as a starting point for development in your repository.
* **Requirements:** Read all instructions carefully and ensure all requirements are met.
* **Quality:** Documentation (including this completed and updated README), overall project organization, code quality, and commit clarity (Semantic Commits recommended) will be considered during evaluation.

## Use Case

You are a developer on the **DeveloperStore** team. Your task is to implement an API prototype for managing sales records.

We work with `DDD`, and to reference entities from other domains (like Customer, Branch, Product), we use the `External Identities` pattern, potentially with some denormalization of descriptions/names if necessary (although the Product API is part of this test).

You will implement an API (full CRUD for Sales, partial CRUD for Products) that manages sales records. The API needs to be able to provide/manage:

* Sale Number (`SaleNumber`)
* Sale Date (`SaleDate`)
* Customer ID (`CustomerId` - External Identity)
* Branch ID (`BranchId` - External Identity)
* Cancellation Status (`Cancelled`)
* Sale Items:
    * Product ID (`ProductId` - External Identity)
    * Quantity (`Quantity`)
    * Unit Price (`UnitPrice` - price at the time of sale)
    * Applied Tax Amount (`ValueMonetaryTaxApplied` - per item)
    * Total Amount per Item (`Total` - including tax)
* Total Sale Amount (`TotalAmount` - sum of item totals, including taxes)

## Business Rules (Sales)

The following business rules must be implemented when creating a sale:

1.  **Per-Item Tax Calculation (VAT):**
    * Purchases with **up to 4 identical items**: No tax (TAX Free).
    * Purchases between **5 and 9 identical items** (inclusive): Apply **VAT (10%)** on the total value of those items (`UnitPrice * Quantity`).
    * Purchases between **10 and 20 identical items** (inclusive): Apply **SPECIAL VAT (20%)** on the total value of those items (`UnitPrice * Quantity`).
2.  **Quantity Limit:**
    * It is **not possible** to sell more than **20 identical items** in a single sale (or single sale item line). The API must return an appropriate error (e.g., 400 Bad Request) in this case.

## Event Publishing (Optional)

It's not mandatory, but building the code to publish domain events when certain actions occur would be a plus. If you choose to implement this:

* **Events:** `SaleCreated`, `SaleCancelled`, `ProductCreated`.
* **Publishing:** You are **not required** to actually publish to any Message Broker (like RabbitMQ, Kafka, Rebus). It is sufficient to **log a message** in the application log indicating that the event was published (e.g., via `ILogger`).

## Project Overview and Evaluation

This project serves as a technical evaluation for senior developer candidates. The goal is to assess various skills and competencies, including (but not limited to):

* Proficiency in C# and .NET 8.
* RESTful API development.
* Project layer separation (Clean Architecture / DDD).
* Understanding and implementation of design patterns (e.g., Mediator).
* Object-relational mapping (EF Core).
* Database skills (PostgreSQL and/or MongoDB).
* Unit testing (xUnit) and mocking (NSubstitute).
* Container usage (Docker, Docker Compose).
* Versioning (Git, Git Flow).
* API error handling and response formatting.
* Asynchronous programming.
* Code quality and best practices.

## Applied Architecture

This project utilizes a Clean Architecture / Domain-Driven Design (DDD) approach with explicit layer separation:

* **Sales.Domain:** Contains entities (`Sale`, `SaleItem`, `Product`), intrinsic business rules, repository interfaces, and domain events (optional). Depends on no other layers.
* **Sales.Application:** Orchestrates use cases using the **Mediator pattern (CQRS)**. Defines Commands, Queries, and their Handlers. Contains DTOs, validation (using FluentValidation for synchronous checks), and interfaces for infrastructure services. Depends on `Sales.Domain`. **Validations requiring database access (e.g., checking product existence, price matching, duplicate sale number) are performed here, within the Handlers.**
* **Sales.Infrastructure:** Implements interfaces defined in upper layers. Contains DbContext configuration (EF Core with PostgreSQL), repository implementations, and external service implementations (like event publishing via Logging). Depends on `Sales.Application` and `Sales.Domain`.
* **Sales.Api:** The presentation layer (ASP.NET Core Web API). Receives HTTP requests, sends Commands/Queries to Mediator, formats HTTP responses. Configures Dependency Injection and the middleware pipeline. Depends on `Sales.Application`.
* **Sales.Tests:** Contains unit and integration tests for various layers, using xUnit, NSubstitute, and FluentAssertions.
* **Gateway:** Separate project for the Ocelot API Gateway.

## Tech Stack

* **Backend:** .NET 8.0, C#
* **Database:** PostgreSQL (preferred) and/or MongoDB 4.4
* **API Gateway:** Ocelot
* **Containerization:** Docker, Docker Compose v3.3
* **Testing:** xUnit

## Key Frameworks and Libraries

* **ORM:** Entity Framework Core (EF Core)
* **CQRS / Mediator:** MediatR
* **Mapping:** AutoMapper
* **Testing (Mocks):** NSubstitute
* **Testing (Fake Data):** Bogus
* **Validation:** FluentValidation (for synchronous command structure/format validation)

## API Endpoints

**Base URL (via Docker Compose):** `http://localhost:7777` (Accessing via Ocelot Gateway)

**Mandatory Endpoints:**

The following endpoints **must** be implemented with these names and paths (relative to the Base URL):

1.  **`GET /products`**
    * Description: Lists registered products.
    * Returns: `200 OK` with `ApiResponse<IEnumerable<ProductDto>>`.
2.  **`POST /products`**
    * Description: Creates a new product.
    * Request Body: JSON with `title`, `price`, `description`, `category`, `image`.
    * Returns: `201 Created` with `ApiResponse<ProductDto>` of the created product.
3.  **`GET /sales`**
    * Description: Lists created sales.
    * Returns: `200 OK` with `ApiResponse<IEnumerable<SaleDto>>`.
4.  **`POST /sales`**
    * Description: Creates a new sale, applying business rules (taxes, quantity limits).
    * Request Body: JSON matching `CreateSaleCommand` (see `general-api.txt` or code).
    * Returns: `201 Created` with `ApiResponse<SaleDto>` of the created sale.
    * Expected Errors: `400 Bad Request` for invalid data, rule violations (e.g., >20 items), product not found, price mismatch, duplicate sale number.
5.  **`DELETE /sales/{id}`**
    * Description: Cancels an existing sale (marks as cancelled).
    * Parameter: `id` (Guid) of the sale in the URL.
    * Returns: `200 OK` with `ApiResponse<object>` indicating success (e.g., `{"status": "success", "message": "Sell Cancelled"}`).
    * Expected Errors: `404 Not Found` if the sale doesn't exist.

## How to Run (Docker)

Follow these instructions to build the Docker images and run the entire application stack (API, Gateway, Database) using Docker Compose.

**Prerequisites:**

* Docker Desktop (Windows/macOS) or Docker Engine + Docker Compose (Linux) installed and running.

**Steps:**

1.  **Clone the Repository:**
    ```bash
    git clone <your-repository-url>
    cd <repository-folder>
    ```

2.  **Create External Docker Network:**
    The application and its test environment require a specific external Docker network. Create it by running the following command in your terminal (you only need to do this once):
    ```bash
    docker network create evaluation-network
    ```
    *(Your `docker-compose.yml` file is configured to use this network.)*

3.  **Review Configuration (Optional Check):**
    * Open the `docker-compose.yml` file.
    * Verify the `services` section:
        * `postgres`: Configured with user/password `postgres` and database `salesdb`. Host port `5432` is mapped to container port `5432`.
        * `sales-api`: Builds from `src/Sales.Api/Dockerfile`. Container name is `sales-api`. Host port `8090` is mapped to container port `8090`. Ensure the connection string under `environment` correctly points to the database service: `Host=postgres;...`.
        * `gateway`: Builds from `src/Gateway/Dockerfile`. Container name is `ocelot-gateway`. Host port `7777` is mapped to container port `7777`.
    * Verify that all services are connected to the external `evaluation-network`.

4.  **Build and Run Containers:**
    Navigate to the root directory of the project (where the `docker-compose.yml` file is located) in your terminal and run:
    ```bash
    docker-compose up --build -d
    ```
    * `--build`: Forces Docker Compose to build the images based on the Dockerfiles (essential if you made code changes).
    * `-d`: Runs the containers in detached mode (in the background).

5.  **Wait for Services to Start:**
    Allow a minute or two for all containers (especially the PostgreSQL database and the Sales API) to start completely. The first time you run the `sales-api`, it might take slightly longer as Entity Framework Core migrations may need to be applied to the database. You can check the status and logs:
    * Check running containers: `docker ps` (Ensure `postgres`, `sales-api`, `ocelot-gateway` are `Up`).
    * Check API logs for startup confirmation (look for "Now listening on..." and potential migration logs): `docker logs sales-api`
    * Check Database logs: `docker logs postgres`

6.  **Accessing the Application:**
    Once the containers are running, the application is accessible through the Ocelot API Gateway:
    * **API Base URL:** `http://localhost:7777`
    * **Swagger UI (API Docs):** `http://localhost:7777/swagger` (Note: This assumes Ocelot routes `/swagger` or the Gateway project itself serves it. If this specific path doesn't work, the individual API might expose Swagger on its direct port `http://localhost:8090/swagger` if needed for debugging).
    * **Example Endpoint:** `GET http://localhost:7777/products`

7.  **Running Tests:**
    See the "Running Tests (Docker)" section below for instructions on how to execute the automated tests against the running Docker environment.

8.  **Stopping Containers:**
    To stop and remove the containers defined in the `docker-compose.yml` file

## How to Use:

**Steps:**

1. Open Postman.
2. Click on "Import".
3. Select the file option.
4. Import the file (sales\_postman\_collection.json) located in the project root.

**Standard Success Response Format:**

```json
{
  "data": {}, // or []
  "status": "success",
  "message": "Operation completed successfully" // or specific message
}