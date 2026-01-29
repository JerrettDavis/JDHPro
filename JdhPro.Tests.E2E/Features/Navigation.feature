Feature: Navigation
  As a visitor
  I want to navigate the site
  So that I can access different pages

Scenario: Header navigation links work
  Given I navigate to the homepage
  When I click "Services" in the navigation
  Then I should be on the "/services" page

Scenario: Mobile menu works
  Given I am on a mobile device
  And I navigate to the homepage
  When I click the mobile menu button
  Then I should see the mobile navigation menu
