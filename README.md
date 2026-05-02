# 🌌 SAE Edge Runtime

The high-performance, event-sourced business engine for the next generation of SAE Platform. Designed for autonomy, resilience, and multi-tenant edge operations.

## 🚀 Mission
Replace the legacy local server with a modular, scalable architecture that provides **Functional Parity** while enabling **Offline-First** and **Cloud-Native** synchronization.

## 🏗️ Core Architecture
- **Event Sourcing**: 100% auditability and state reconstruction via `IEventStore`.
- **CQRS**: Clean separation of write-models (Aggregates) and read-models (Projections).
- **L3 Conflict Resolution**: Intelligent reconciliation for distributed inventory and sales.
- **Edge Fabric**: Real-time synchronization via **NATS JetStream**.

## 📦 Business Modules
Current implementation status:

| Module | Status | Description |
| :--- | :---: | :--- |
| **Catalog** | ✅ | Product management, taxes, and SKUs. |
| **Orders** | ✅ | High-fidelity multi-line sales processing. |
| **Inventory**| ✅ | Recipe-based automatic stock deduction. |
| **Caja**    | ✅ | Cashier sessions and cash flow tracking. |
| **Billing**  | ✅ | Legal fiscal document generation. |
| **Restaurant**| 🏗️ | Table management and KDS (In Progress). |
| **Kitchen**  | 🏗️ | Preparation status and workflows (In Progress). |

## 🛠️ Getting Started
### Prerequisites
- .NET 10.0 SDK
- PostgreSQL (Edge Persistence)
- NATS Server (Replication Fabric)

### Installation
```powershell
dotnet restore
dotnet build
```

### Running Tests
```powershell
dotnet test SAE_EDGE_RUNTIME\SAE.EdgeRuntime.Tests\SAE.EdgeRuntime.Tests.csproj
```

## 📜 Documentation
Documentation for the legacy server is deprecated. Refer to the `walkthrough.md` for the current implementation status and convergence roadmap.
