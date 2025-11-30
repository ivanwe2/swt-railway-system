# Railway Ticketing Portal 🚂
### Software Testing Coursework - Final Submission

![Build Status](https://github.com/username/repo/actions/workflows/build.yml/badge.svg)
![Coverage](https://img.shields.io/badge/coverage-80%25-green)

## 📖 Project Overview
This project is a C# Console Application designed to simulate a railway ticketing system. It demonstrates **Clean Architecture** principles and a rigorous **Software Testing Lifecycle** (STLC).

The system supports:
* Search and Booking (One-Way & Round Trip).
* Dynamic Pricing (Rush Hour, Evening Discounts).
* Passenger Concessions (Senior & Family Railcards).
* Profile Management and Multi-language Support (EN, FR, DE, ES).

---

## 📂 Deliverables & Documentation
Below are links to the required artifacts for the scoring criteria, located in the `docs/` folder.

### 📝 Requirements Engineering (Lab 01 & 02)
* **[Requirements Specification (FR/NFR)](docs/Requirements.md)** - Criteria #1
* **[Gherkin Feature Specifications](docs/Gherkin_Feautures.md)** - Criteria #2
* **[Defect Report (Static Testing)](docs/Defect_Report.md)** - Criteria #4

### 🧪 Test Design (Lab 03 & 06)
* **[Control Flow Graph (Pricing Logic)](docs/ControlFlowDiagram_Pricing.png)** - Criteria #5
   > * **[State Transition Diagram](docs/ReservationState_Diagram.png)** - Criteria #14
   > * **[Requirement Traceability Matrix (RTM)](docs/RequirementTraceabilityMatrix.csv)** - Criteria #10
* **[Master Test Plan](docs/Testplan.md)** - Criteria #15

### ⚙️ Implementation (Lab 05)
* **[Source Code Folder](src/)** - Criteria #9
* **[Unit Test Project](test/RailwaySystem.UnitTests/)** - Criteria #6, 7, 8

---

## 🚀 How to Run
Prerequisites: .NET 8 or 9 SDK.

1.  **Navigate to the UI folder:**
    ```bash
    cd src/RailwaySystem.Console
    ```
2.  **Run the application:**
    ```bash
    dotnet run
    ```
3.  **Usage:** Use Arrow Keys to navigate menus. Select "Search Trains" to test pricing logic.

---

## 🧪 How to Test
This project uses **xUnit** and **Coverlet** to verify Statement, Decision, and Condition coverage.

### Run Unit Tests
```bash
dotnet test test/RailwaySystem.UnitTests/RailwaySystem.UnitTests.csproj