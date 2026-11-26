Feature: Homepage
  As a visitor
  I want to see the homepage
  So that I can learn about JDH Productions

Scenario: Homepage loads successfully
  Given I navigate to the homepage
  Then I should see the hero heading "Ideas engineered."
  And I should see the tagline containing "digital manufacturing"

Scenario: Capabilities grid displays four disciplines
  Given I navigate to the homepage
  Then I should see 4 service cards
  And I should see "Software engineering"
  And I should see "AI & automation"
  And I should see "Design & fabrication"
  And I should see "Technical consulting"

Scenario: Start a project CTA works
  Given I navigate to the homepage
  When I click "Start a project"
  Then I should be scrolled to the contact section
