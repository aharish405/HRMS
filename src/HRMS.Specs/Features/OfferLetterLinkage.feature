Feature: Offer Letter Linkage
  As an HR Administrator
  I want to link offer letters to draft employees
  So that I can maintain a consistent employee lifecycle

  Scenario: Create Offer for Draft Employee
    Given a draft employee exists with name "John Doe" and email "john.doe@test.com"
    And a valid offer letter template "Standard Offer"
    When I create an offer letter linked to "John Doe" using template "Standard Offer"
    Then the offer letter should be created successfully
    And the candidate name should be "John Doe"
    And the offer should be linked to the employee
    And the offer status should be "Draft"

  Scenario: Validate Employee Draft Status
    Given an active employee exists with name "Jane Smith"
    When I try to create an offer letter linked to "Jane Smith"
    Then the creation should fail with error "Selected employee must be in Draft status"
