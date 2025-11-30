# Software Requirements Specification (SRS)
## Railway Ticketing Portal

### 1. Functional Requirements (FR)

**Search & Booking**
* **FR-01 (Search):** The system shall allow users to search for trains by selecting an Origin and Destination from a predefined schedule. 
* **FR-02 (Ticket Types):** The system shall support both "One-Way" and "Round Trip" (Return) ticket types. Return tickets calculate the base fare as double the single fare before discounts.
* **FR-03 (Cart Persistence):** The system shall add tickets to a shopping cart. This cart must remain accessible and persist its state for a period of 7 days. 

**Dynamic Pricing Rules**
* **FR-04 (Rush Hour):** A full fare surcharge applies to journeys departing before 09:30 or between 16:00 and 19:30.
* **FR-05 (Saver & Evening):** A "Saver" fare applies between 09:30 and 16:00. A 5% discount is applied to journeys departing after 19:30.
* **FR-06 (Senior Concession):** Passengers aged 60+ holding an 'Over 60s' railcard receive a 34% discount.
* **FR-07 (Family/Child Concession):** Children under 16 receive a 10% discount by default. If a 'Family Railcard' is held, the discount increases to 50%. 

**Reservation Management**
* **FR-08 (Lifecycle):** Users must be able to view previous reservations, cancel existing reservations, and modify the ticket type of an active reservation.

**Profile Management**
* **FR-09 (Profiles):** The system shall allow users to create, view, and edit consumer profiles (Username, Age, Railcard). These profiles must be loadable during the booking process to auto-fill passenger details.

**Localization**
* **FR-10 (Languages):** The user interface must support dynamic switching between English, French, German, and Spanish.

### 2. Non-Functional Requirements (NFR)
* **NFR-01 (Persistence):** Data must be stored in local JSON files (`trains.json`, `users.json`, `bookings.json`) to ensure portability without a database server.
* **NFR-02 (Usability):** The command-line interface uses visual elements (tables, selection prompts) via `Spectre.Console` to minimize user input errors.
* **NFR-03 (Maintainability):** The system adheres to Clean Architecture, separating Domain logic, Data persistence, and UI presentation.