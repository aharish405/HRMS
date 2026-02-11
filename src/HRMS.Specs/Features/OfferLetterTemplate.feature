Feature: Offer Letter Template Management
  As an HR Administrator
  I want to create and manage offer letter templates with rich text
  So that I can generate professional offer letters for candidates

  Scenario: Create a new Offer Letter Template with valid details
    Given I have a template name "Standard Tech Offer"
    And I provide rich text content "<html><body><p>Welcome {{CandidateName}}</p></body></html>"
    When I submit the create template form
    Then the template should be saved successfully
    And the template name should be "Standard Tech Offer"

  Scenario: Create a template with duplicate name
    Given a template "Existing Template" already exists
    When I try to create another template with name "Existing Template"
    Then the creation should fail with an error message "Template name already exists"

  Scenario: Edit an existing template content
    Given an existing template "Template To Edit"
    When I update the template content to "Updated Content"
    Then the template should be updated successfully
    And the content should become "Updated Content"
