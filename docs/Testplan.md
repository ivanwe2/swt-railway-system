# Validation, Verification, and Testing Plan

## 1.0 General Information
**System:** Railway Ticketing Portal
**Version:** 1.0
**Scope:** Validation of Pricing Logic (White-box), Verification of User Flows (Black-box), and UI Usability.

## 2.0 Test Evaluation
**Traceability:** See `RequirementTraceabilityMatrix.csv`.
**Exit Criteria:** * 100% Pass rate on High Priority Unit Tests.
* No critical severity defects in the Defect Report.

### 2.1 Code Coverage Results
The following coverage metrics were captured using Coverlet during the final build:

| Module | Line | Branch | Method |
| :--- | :--- | :--- | :--- |
| **RailwaySystem.Console** | 33.59% | 41.11% | 82.14% |
| **Total** | **33.59%** | **41.11%** | **82.14%** |
| **Average** | 33.59% | 41.11% | 82.14% |

*Note: UI layer (Console) has lower coverage due to `Spectre.Console` interaction. Domain and Application logic layers aim for >80%.*

## 3.0 Test Description

### 3.1 Unit Testing (White-Box)
* **Target:** `RailcardPricingStrategy.cs` (The core complexity engine).
* **Technique:** Condition Coverage (CC).
* **Objective:** Verify nested Boolean logic:
    * `Age >= 60 AND Card == Over60s`
    * `Age < 16 AND Card == Family`
* **Tools:** xUnit, Coverlet.

### 3.2 Functional Testing (Black-Box)
* **Target:** `TimePricingStrategy.cs` and `PricingService.cs`.
* **Technique:** Equivalence Partitioning.
* **Data Set (Bulgarian Routes):**
    * **Rush Hour:** Sofia -> Plovdiv (07:30)
    * **Saver:** Sofia -> Varna (10:00)
    * **Late Night:** Burgas -> Sofia (22:00)

### 3.3 Integration Testing
* **Target:** `BookingService` + Repository.
* **Scenario:** Verify "Modify Reservation" workflow:
    * Create Booking (OneWay) -> Save to JSON -> Modify (Return) -> Verify Price Update.