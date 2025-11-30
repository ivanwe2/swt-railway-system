# Defect Report - Static Testing & Code Review

**Reviewer:** Me the developer
**Date:** 2025-11-30

| ID | Severity | Component | Description | Status |
| :--- | :--- | :--- | :--- | :--- |
| **DEF-01** | **Critical** | `TimePricingStrategy.cs` | **Logic Error:** The Rush Hour check was returning `train.BasePrice` directly. This inadvertently reset "Round Trip" tickets (which should be 2x price) back to the single fare price. | **FIXED** |
| **DEF-02** | **High** | `Program.cs` | **Missing Functionality:** Profile creation did not allow selecting a Railcard type. It defaulted to `None`, meaning Senior profiles failed to get discounts. | **FIXED** |
| **DEF-03** | **Medium** | `BookingService.cs` | **Missing Requirement:** The system allowed creating and cancelling bookings but lacked the ability to "Modify" them as required by the spec. | **FIXED** |
| **DEF-04** | **Low** | `Program.cs` | **UI/UX:** Train selection was hardcoded to the first item in the list, preventing users from testing different routes. | **FIXED** |