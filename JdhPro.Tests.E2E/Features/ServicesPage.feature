Feature: Services Page
  As a visitor
  I want to view detailed services
  So that I can understand what JDH Productions offers

Scenario: Services page loads
  Given I navigate to "/services"
  Then I should see the page title "Our Services"
  And I should see 4 detailed service sections

Scenario: Service content is displayed
  Given I navigate to "/services"
  Then each service should have a title
  And each service should have a description
  And each service should have benefits
  And each service should have a "Discuss This Service" button
