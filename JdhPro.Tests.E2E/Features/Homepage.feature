Feature: Homepage
  As a visitor
  I want to see the homepage
  So that I can learn about JDH Productions

Scenario: Homepage loads successfully
  Given I navigate to the homepage
  Then I should see the hero heading "Engineering Tomorrow's"
  And I should see the tagline containing "technical excellence"

Scenario: Services grid displays all services
  Given I navigate to the homepage
  Then I should see 4 service cards
  And I should see "Technical Consulting"
  And I should see "Custom Application Development"
  And I should see "AI-Assisted Workflow Design"
  And I should see "Process Engineering"

Scenario: Start Your Project CTA works
  Given I navigate to the homepage
  When I click "Start Your Project"
  Then I should be scrolled to the contact section
