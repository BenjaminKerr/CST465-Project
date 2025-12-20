
# Grover's Search Algorithm Visualizer

## Getting Started

### Prerequisites
* **.NET 8.0 SDK**: [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
* **EF Core Tools**: Install globally by running:
  ```bash
  dotnet tool install --global dotnet-ef

```

### Setup & Installation

1. **Clone the Repository**
```bash
git clone <your-repo-url>
cd CST465-Project

```


2. **Initialize the Database**
The project uses a SQLite database. Run the following command to create the local `app.db` file and build all necessary tables (Identity, Visualizations, and Comments):
```bash
dotnet ef database update

```


3. **Run the Application**
```bash
dotnet run

```


## 🔐 Admin Credentials

The application includes an automated seeding process in `Program.cs`. Upon first launch, an administrative account is created automatically using the credentials defined in `appsettings.json`.

* **Admin Email:** `admin@grovers.com`
* **Admin Password:** `Password123!`

> **Note:** Log in with these credentials to access restricted features, such as deleting visualizations from the History page.

## Project Features

### 1. Quantum Visualization Engine

* **HTML5 Canvas:** Real-time rendering of probability amplitudes.
* **Dynamic Controls:** Adjust database size (N) and observe the  speedup compared to classical search.
* **Save & Export:** Save specific simulation runs and download text-based reports.

### 2. Architecture & Patterns

* **Repository Pattern:** Decouples data access from business logic.
* **Caching Decorator:** Uses `IMemoryCache` to wrap repositories for high-performance data retrieval.
* **Dual Controllers:** - `VisualizationsController`: Handles MVC Views for the UI.
* `VisualizationsApiController`: Provides a RESTful JSON interface for frontend AJAX calls.



### 3. Security

* **ASP.NET Core Identity:** Secure user registration and login.
* **Role-Based Access Control (RBAC):** Restricts "Delete" operations to users in the "Admin" role.
* **Anti-Forgery Protection:** Validates tokens on both MVC forms and API POST requests.