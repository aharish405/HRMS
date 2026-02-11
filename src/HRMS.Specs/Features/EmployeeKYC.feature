Feature: Employee KYC
    As an HR Administrator
    I want to record PAN and Aadhar details for employees
    So that I can comply with statutory requirements and ensure proper identification

Scenario: Add valid KYC details for a new employee
    Given I have a new employee draft with the following details
        | FirstName | LastName | Email              | Phone      | Department | Designation |
        | John      | Doe      | john.doe@work.com  | 9876543210 | IT         | Developer   |
    And I provide the following KYC details
        | PanNumber  | AadharNumber | EmergencyContact | EmergencyPhone |
        | ABCDE1234F | 123456789012 | Jane Doe         | 9876543210     |
    When I submit the create employee form
    Then the employee should be created successfully
    And the employee status should be 'Draft'
    And the saved employee should have the correct KYC details
        | PanNumber  | AadharNumber |
        | ABCDE1234F | 123456789012 |
