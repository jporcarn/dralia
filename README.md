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

## Folder Structure

The solution is organized into the following folder structure:

```plaintext
Dralia/
├── Docplanner.Api/
│   ├── Controllers/
│   ├── Models/
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   └── Docplanner.Api.csproj
├── Docplanner.Application/
│   ├── UseCases/
│   ├── Interfaces/
│   └── Docplanner.Application.csproj
├── Docplanner.Domain/
│   ├── Models/
│   ├── Interfaces/
│   └── Docplanner.Domain.csproj
├── Docplanner.Infrastructure/
│   ├── Repositories/
│   ├── Mappings/
│   ├── SlotService/
│   └── Docplanner.Infrastructure.csproj
├── Docplanner.ApiTests.Integration/
│   ├── TestFiles/
│   ├── appsettings.json (linked)
│   ├── appsettings.Development.json (linked)
│   └── Docplanner.ApiTests.Integration.csproj
└── README.md
```

---

## Design Principles

- **Separation of Concerns**: Each layer has a distinct responsibility, ensuring maintainability and scalability.
  - The architecture is clean and follows the onion/clean architecture principles.
  - The API acts as a mediator between the frontend and the slot service, ensuring the frontend does not directly interact with the service.
- **Cost-Effective Azure Resources**: Using Azure Static Web Apps (Free Tier) and Azure App Service (Basic Plan) is a great choice for cost-effectiveness
- **CI/CD Pipeline**: The solution includes a CI/CD pipeline for automated deployment to Azure.
  - The inclusion of a CI/CD pipeline using GitHub Actions ensures automated deployments and testing.
- **Dependency Inversion**: Higher-level layers depend on abstractions, not concrete implementations.
- **Testability**: The architecture facilitates unit and integration testing by isolating business logic and external dependencies.
- **IaC**: Infrastructure as Code (IaC) is used to provision Azure resources, ensuring consistent and repeatable deployments.

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

## Error Handling

The API includes global exception handling to ensure meaningful error messages are returned to the client. Common error scenarios include:

- **Invalid Input**: Returns a 400 Bad Request with details about the invalid fields.
- **Slot Unavailability**: Returns a 409 Conflict if the selected slot is no longer available.
- **Server Errors**: Returns a 500 Internal Server Error for unexpected issues.

Logs are generated for all errors to assist with debugging.

---

## Slot Service Integration

The API consumes the slot service at `https://draliatest.azurewebsites.net/api/availability` to retrieve available slots. The service is called using an HTTP client, and the response is mapped to domain models for further processing.

During testing, the slot service is mocked to ensure tests are independent of external dependencies.

**By Design**: Assume a Default Time Zone for the Availability API. 
Since the third-party availability API does not provide time zone information, I assumeed a default time zone for the data it returns. CET (Central European Time).

---

## Infrastructure as Code (IaC)

To ensure consistent and repeatable deployments of the Dralia solution, Infrastructure as Code (IaC) is used to define and provision the required Azure resources. Terraform is the recommended IaC platform for this project due to its flexibility, multi-cloud support, and strong integration with Azure.

### Why Terraform?

- **Declarative Syntax**: Define the desired state of your infrastructure, and Terraform will handle the provisioning.
- **State Management**: Terraform maintains a state file to track the current state of your infrastructure.
- **Multi-Cloud Support**: Easily extend the solution to other cloud providers if needed.
- **Azure Integration**: Terraform provides first-class support for Azure resources.

---

## Running the Solution Locally

### Prerequisites

1. Install [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).
2. Install [Node.js](https://nodejs.org/) for the SPA.
3. Clone the [repository](https://github.com/jporcarn/dralia.git) to your local machine.

### Steps

1. **API Setup**:
   - Navigate to the `Docplanner.Api` project directory.
   - Create an `appsettings.Development.json` file with the following content:

```json
{
  "AvailabilityApi": {
    "BaseUrl": "https://draliatest.azurewebsites.net/api/availability",
    "Credentials": {
      "Username": "your-username",
      "Password": "your-password"
    }
  }
}
```

- Replace `your-username` and `your-password` with the appropriate credentials.

- Alternatively, you can set the environment variables `AVAILABILITYAPI__BASEURL`, `AVAILABILITYAPI__CREDENTIALS__USERNAME` and `AVAILABILITYAPI__CREDENTIALS__PASSWORD` in the launchSettings.json file.
- Navigate to Docplanner.Api/Properties/launchSettings.json and add the following configuration:

```json
{
  "https": {
    "commandName": "Project",
    "dotnetRunMessages": true,
    "launchBrowser": true,
    "launchUrl": "swagger",
    "applicationUrl": "https://localhost:7236;http://localhost:5058",
    "environmentVariables": {
      "ASPNETCORE_ENVIRONMENT": "Development",
      "AVAILABILITYAPI__BASEURL": "https://draliatest.azurewebsites.net/api/availability",
      "AVAILABILITYAPI__CREDENTIALS__USERNAME": "your-username",
      "AVAILABILITYAPI__CREDENTIALS__PASSWORD": "your-password"
    }
  }
}
```

2. **SPA Setup**:

   - Navigate to the SPA project directory.
   - Run `npm install` to install dependencies.
   - Run `npm start` to start the development server.

3. **Run the API**:

   - Use the command `dotnet run` in the `Docplanner.Api` project directory.

4. Access the SPA at `http://localhost:4200` and the API at `https://localhost:7236`.

---

## Deploying to Azure

### Prerequisites
1. An active Azure Pay-As-You-Go subscription.
2. Install the [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli).
3. Download the publish profile for the API from the Azure Portal and store it in the `AZURE_WEBAPP_PUBLISH_PROFILE` GitHub secret.
4. Generate a deployment token for the SPA from the Azure Portal and store it in the `AZURE_STATIC_WEB_APPS_API_TOKEN` GitHub secret.

### Required GitHub Secrets

The following secrets must be configured in your GitHub repository for CI/CD:

- `ARM_CLIENT_ID`
- `ARM_CLIENT_SECRET`
- `ARM_SUBSCRIPTION_ID`
- `ARM_TENANT_ID`
- `AVAILABILITYAPI__CREDENTIALS__PASSWORD`
- `AVAILABILITYAPI__CREDENTIALS__USERNAME`
- `AZURE_STATIC_WEB_APPS_API_TOKEN`
- `AZURE_WEBAPP_PUBLISH_PROFILE`

### Deployment Steps

1. **Infrastructure Deployment**:
   - Use the provided PowerShell script:

```powershell
$securePassword = Read-Host "Enter Password" -AsSecureString \
.\deploy-infra.ps1 -Username "techuser" -Password $securePassword
```

2. **API Deployment**:
   - Use the provided PowerShell script:

```powershell
.\deploy-api.ps1
```

3. **SPA Deployment**:
   - Use the provided PowerShell script:

```powershell
.\deploy-angular.ps1 -AzureStaticWebAppsApiToken "your-azure-static-web-apps-api-token"
```

### Azure Resources

The following cost-effective Azure resources are used:

- **Azure App Service**: Hosts the API with minimal cost for small-scale applications.
- **Azure Static Web Apps**: Hosts the SPA with free tier options for low-traffic applications.
- **Azure Storage Account**: Provides cost-effective storage for logs or other data.

---

## Notes

- Ensure that the `AZURE_WEBAPP_PUBLISH_PROFILE` secret contains the publish profile downloaded from the Azure Portal for the API.
- The `AZURE_STATIC_WEB_APPS_API_TOKEN` secret must contain the deployment token for the SPA.
- The deployment scripts assume that the required Azure resources are already provisioned.

## Next Steps

To further enhance the Dralia solution, the following steps are recommended:

### 1. Use Azure Key Vault for Storing Credentials

- **Objective**: Securely store the credentials required to call the third-party availability API.
- **Implementation**:
  - Use [Azure Key Vault](https://learn.microsoft.com/en-us/azure/key-vault/) to store sensitive information such as `AVAILABILITYAPI__CREDENTIALS__USERNAME` and `AVAILABILITYAPI__CREDENTIALS__PASSWORD`.
  - Update the application to retrieve these credentials at runtime using Azure Key Vault's integration with .NET Configuration Providers.
- **Benefits**:
  - Centralized management of secrets.
  - Enhanced security with access policies and auditing.

---

### 2. Decouple the Availability API from the Slot API Using a Message Broker

- **Objective**: Improve scalability and reliability by decoupling the availability API from the slot API.
- **Implementation**:
  - Use [Azure Service Bus](https://learn.microsoft.com/en-us/azure/service-bus-messaging/) as a message broker for simplicity.
  - Publish availability requests to a Service Bus queue or topic.
  - Create a background worker in the infrastructure layer to process messages and call the third-party availability API.
- **Benefits**:
  - Asynchronous communication.
  - Reduced coupling between services.
  - Improved fault tolerance.

---

### 3. Strategy for Handling Concurrency Conflicts

- **Objective**: Ensure data consistency when multiple users attempt to book the same slot simultaneously.
- **Implementation**:
  - Use an optimistic concurrency control strategy with [Azure Cosmos DB](https://learn.microsoft.com/en-us/azure/cosmos-db/) or SQL Server.
  - Add a version or timestamp field to the slot entity.
  - Validate the version during updates to detect conflicts.
  - Notify users of conflicts and provide options to retry or select a different slot.
- **Benefits**:
  - Prevents overwriting changes.
  - Ensures data integrity.

---

### 4. Create a NoSQL Database for Storing Activity History

- **Objective**: Store the history of activities, such as who and when an appointment was booked or canceled.
- **Implementation**:
  - Use [Azure Cosmos DB](https://learn.microsoft.com/en-us/azure/cosmos-db/) with a container for activity logs.
  - Design a schema to include fields like `UserId`, `ActionType` (e.g., "Booked", "Canceled"), `Timestamp`, and `Details`.
  - Implement a repository in the infrastructure layer to log activities.
- **Benefits**:
  - High availability and scalability.
  - Flexible schema for storing diverse activity data.

---

### 5. Push Notifications for Real-Time Updates

- **Objective**: Notify users in real-time about appointment cancellations or concurrency conflicts.
- **Implementation**:
  - Use [Azure Notification Hubs](https://learn.microsoft.com/en-us/azure/notification-hubs/) or [SignalR Service](https://learn.microsoft.com/en-us/azure/azure-signalr/) for push notifications.
  - Integrate the SPA with SignalR to receive real-time updates.
  - Trigger notifications from the API when a cancellation or conflict occurs.
- **Benefits**:
  - Improved user experience with instant updates.
  - Reduced need for manual refresh or polling.

---

By implementing these steps, the Dralia solution will become more secure, scalable, and user-friendly, while ensuring data integrity and real-time responsiveness.
