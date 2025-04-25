# Dralia
## Doctor Slots API

Dralia is a doctor slots API designed to manage and provide availability slots for healthcare facilities. The solution is built using a clean architecture approach, inspired by the onion architecture, to ensure separation of concerns, maintainability, and scalability.

---

## Solution Architecture

The solution is structured into multiple projects, each with a specific responsibility:

### 1. **Docplanner.Api**
- **Purpose**: Acts as the entry point for the application, exposing RESTful APIs to clients.
- **Responsibilities**:
  - Handles HTTP requests and responses.
  - Maps API models (e.g., `SlotResponse`) to and from application models.
  - Configures middleware, routing, and Swagger for API documentation.
- **Key Components**:
  - `SlotController`: Manages endpoints for slot-related operations.
  - `WeeklySlotsProfile`: Maps application models to API response models.

---

### 2. **Docplanner.Application**
- **Purpose**: Implements the core application logic and orchestrates use cases.
- **Responsibilities**:
  - Contains business logic and use case handlers (e.g., `GetAvailableSlotsHandler`).
  - Coordinates between the domain layer and infrastructure layer.
  - Implements strategies and configurations for complex mappings or business rules.
- **Key Components**:
  - `UseCases`: Contains handlers for specific use cases, such as retrieving available slots.
  - `Interfaces`: Defines contracts for repositories and services used by the application layer.

---

### 3. **Docplanner.Domain**
- **Purpose**: Represents the core business models and rules.
- **Responsibilities**:
  - Defines the domain entities (e.g., `Slot`, `WeeklySlots`, `DailySlots`).
  - Encapsulates business rules and validations.
  - Remains independent of other layers to ensure reusability and testability.
- **Key Components**:
  - `Models`: Contains domain entities and value objects.
  - `Interfaces`: Defines contracts for domain services.

---

### 4. **Docplanner.Infrastructure**
- **Purpose**: Handles external dependencies, such as databases or third-party APIs.
- **Responsibilities**:
  - Implements repository interfaces to fetch and persist data.
  - Maps external data models to domain models.
  - Interacts with external APIs (e.g., availability API).
- **Key Components**:
  - `Repositories`: Implements data access logic (e.g., `AvailabilityApiRepository`).
  - `Mappings`: Contains AutoMapper profiles and resolvers for transforming external data into domain models.

---

## Design Principles

- **Separation of Concerns**: Each layer has a distinct responsibility, ensuring maintainability and scalability.
- **Dependency Inversion**: Higher-level layers depend on abstractions, not concrete implementations.
- **Testability**: The architecture facilitates unit and integration testing by isolating business logic and external dependencies.

---

## Mapping Logic

Mapping logic is distributed based on complexity:
- **Simple Mappings**: Performed in the `Docplanner.Infrastructure` layer to transform external data into domain models.
- **Complex Mappings with Business Rules**: Handled in the `Docplanner.Application` layer to ensure business logic is centralized.

---

## Testing

The solution includes comprehensive testing:
- **Unit Tests**: Validate individual components in isolation (e.g., `GetAvailableSlotsHandlerTests`).
- **Integration Tests**: Verify interactions between components and external systems (e.g., `SlotControllerTests`).

---

## Deploy to Azure

To deploy the API to Azure, use the provided PowerShell script:


`PS> $securePassword = Read-Host "Enter Password" -AsSecureString`

`PS> .\deploy-api.ps1 -Username "techuser" -Password $securePassword`
