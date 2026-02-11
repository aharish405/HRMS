Feature: Offer Acceptance
  As an HR Administrator
  I want to accept an offer letter on behalf of a candidate
  So that the draft employee record is activated and payroll is set up

  Scenario: Accept Offer for Linked Draft Employee
    Given a draft employee exists with name "Alice Wonderland"
    And an offer letter exists for "Alice Wonderland" linked to the draft employee
    And the offer letter status is "Sent"
    When I accept the offer letter
    Then the employee status should be "Active"
    And the employee code should generate a permanent "EMP" code
    And a salary record should be created for the employee
    And the offer letter status should be "Accepted"

  Scenario: Validate Acceptance Logic
    Given an offer letter exists for "Bob Builder"
    And the offer letter status is "Draft"
    When I try to accept the offer letter
    Then the acceptance should fail with error "Only sent offers can be accepted"
