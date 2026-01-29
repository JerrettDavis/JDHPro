Feature: Contact Page
  As a potential client
  I want to contact JDH Productions
  So that I can discuss my project

Scenario: Contact page loads
  Given I navigate to "/contact"
  Then I should see "Let's Build Something Amazing"
  And I should see a contact form

Scenario: Contact form validation
  Given I navigate to "/contact"
  When I click "Send Message" without filling the form
  Then I should see validation errors

Scenario: Contact form with service preselection
  Given I navigate to "/contact?service=Technical Consulting"
  Then the service dropdown should have "Technical Consulting" selected
