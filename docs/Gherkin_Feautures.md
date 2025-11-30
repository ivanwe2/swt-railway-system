# Gherkin Feature Specifications
## Railway Ticketing Portal

* Feature: Train Search and Selection
  As a traveler
  I want to view available train routes
  So that I can choose a convenient departure time

  Scenario: Viewing the train timetable
    Given the system contains a schedule of Bulgarian routes
    When I select "Search Trains" from the main menu
    Then I should see a list including "Sofia -> Plovdiv" and "Burgas -> Sofia"
    And the departure times should be displayed in chronological order
#
* Feature: Dynamic Ticket Pricing
  As a system
  I want to calculate ticket prices based on time, ticket type, and passenger demographics
  So that the correct fare is charged according to business rules

  Scenario: Rush hour travel charges full fare
    Given a train departs at "07:30" (Rush Hour)
    And the ticket type is "One-Way"
    When a user checks the price
    Then the price should match the "Base Price" exactly
    And no time-based discounts are applied

  Scenario: Evening travel receives a discount
    Given a train departs at "22:00" (Late Night)
    When the price is calculated
    Then a 5% discount is applied to the fare

  Scenario: Return tickets cost double the single fare
    Given the base price for "Sofia" to "Plovdiv" is 15.00
    When a user selects a "Round Trip" ticket
    Then the calculation basis starts at 30.00 before any other discounts

  Scenario: Senior citizens receive Railcard discounts
    Given a passenger is "65" years old
    And the passenger holds an "Over 60s" rail card
    When the ticket price is calculated
    Then a 34% discount is applied to the current fare

  Scenario: Family Railcard applies maximum child discount
    Given a passenger is "12" years old (Child)
    And the passenger holds a "Family Railcard"
    When the ticket price is calculated
    Then a 50% discount is applied

  Scenario: Standard Child discount without Railcard
    Given a passenger is "10" years old
    And the passenger has "No Railcard"
    When the ticket price is calculated
    Then only a 10% discount is applied
#
* Feature: User Profile Management
  As a frequent user
  I want to create and manage my personal profile
  So that I can save my details for future use

  Scenario: Creating a new user profile
    Given I am on the "Manage Profiles" menu
    When I create a profile for "Grandma" with Age "70" and Card "Over 60s"
    Then the system saves the profile to the database
    And I should see "Grandma" in the list of available profiles

  Scenario: Editing an existing profile
    Given a profile exists for "User1" with Age "25"
    When I edit the profile to change Age to "26"
    Then the updated age is persisted
    And future bookings using this profile will use Age "26"
#
* Feature: Profile Integration
  As a user booking a ticket
  I want to load my saved details
  So that I don't have to manually type my age and railcard every time

  Scenario: Loading a profile during booking
    Given I have selected a train and ticket type
    When the system asks "Load passenger details?"
    And I select the "Grandma" profile
    Then the "Age" and "Railcard" fields are auto-filled
    And the price is immediately calculated using the "Over 60s" discount
#
* Feature: Reservation Lifecycle and Cart
  As a customer
  I want to manage items in my shopping cart
  So that I can change my mind before purchasing

  Scenario: Adding a ticket to the cart
    Given I have reviewed the calculated price
    When I confirm "Add to Cart"
    Then the reservation is saved with status "InCart"
    And I can view it in the "View My Cart" menu

  Scenario: Modifying a reservation ticket type
    Given I have a "One-Way" reservation in my cart
    When I choose "Modify Ticket Type" and select "Round Trip"
    Then the reservation status updates to "Modified"
    And the final price is recalculated to reflect the double fare

  Scenario: Cancelling a reservation
    Given I have an active reservation in my cart
    When I select "Cancel Reservation"
    Then the reservation is removed from my active view
    And the status is updated to "Cancelled" in the database

  Scenario: Cart retention policy
    Given a reservation was created "6" days ago
    When I view my cart
    Then the reservation should still be visible
    But if a reservation was created "8" days ago
    Then it should be hidden or removed as expired
#
* Feature: Internationalization
  As a non-English speaker
  I want to switch the interface language
  So that I can navigate the system comfortably

  Scenario: Switching the interface to Spanish
    Given I select "Español" from the language selection screen
    When the main menu loads
    Then I should see "Buscar Trenes" instead of "Search Trains"
    And prompts should appear in Spanish
#