# Validation, Verification, and Testing Plan

## 1.0 General Information
**System:** Railway Ticketing Portal
**Scope:** Validation of Pricing Logic (White-box) and Verification of User Flows (Black-box).

## 2.0 Test Evaluation
**Traceability:** See `RequirementTraceabilityMatrix.csv`.
**Exit Criteria:** 100% Pass rate on Unit Tests; 80% Code Coverage on Domain Logic. (currently around 60% overall)

### Code Coverage Report

| Module | Line | Branch | Method |
| :--- | :--- | :--- | :--- |
| **RailwaySystem.Console** | 23.04% | 28.88% | 58.92% |
| **Total** | **23.04%** | **28.88%** | **58.92%** |
| **Average** | 23.04% | 28.88% | 58.92% |

## 3.0 Test Description

### 3.1 Unit Testing (White-Box)
* **Module:** `RailcardPricingStrategy`
* **Technique:** Condition Coverage (CC)
* **Objective:** Verify complex nested IF logic for Age/Railcard combinations.
* **Tools:** xUnit, coverlet

### 3.2 Functional Testing (Black-Box)
* **Module:** `BookingService`
* **Technique:** State Transition Testing
* **States:** Created -> InCart -> Booked -> Modified/Cancelled.
* **Technique:** Equivalence Partitioning (Age Groups: Child < 16, Adult 16-59, Senior 60+).

### 3.3 Integration Testing
* **Module:** `PricingService` Pipeline
* **Objective:** Ensure `TimePricingStrategy` and `RailcardPricingStrategy` apply cumulatively (e.g., Return Ticket x2 -> Rush Hour -> Senior Discount).