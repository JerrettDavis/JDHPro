Feature: Projects Page
  As a visitor
  I want to view the projects page
  So that I can see the work portfolio

@ignore
Scenario: Projects page loads successfully
  When I navigate to the projects page
  Then I should see the page title "Projects"
  And the page should display correctly

@ignore
Scenario: Projects page has proper navigation
  Given I am on the homepage
  When I click the Projects navigation link
  Then I should be on the projects page
  And the URL should contain "/projects"

@ignore
Scenario: Projects page loads within acceptable time
  When I navigate to the projects page
  Then the page should load within 5 seconds
  And there should be no console errors
