Feature: Performance and Loading
  As a user
  I want the website to load quickly and efficiently
  So that I have a good user experience

Background:
  Given I am on a fast internet connection

Scenario: App loader disappears after startup
  When I navigate to the homepage
  Then the loading indicator should disappear
  And the page should be fully interactive

Scenario: Homepage loads within acceptable time
  When I navigate to the homepage
  Then the page should load within 5 seconds
  And the DOM should be ready within 3 seconds

Scenario: Services page loads within acceptable time
  When I navigate to the services page
  Then the page should load within 5 seconds
  And the DOM should be ready within 3 seconds

Scenario: Contact page loads within acceptable time
  When I navigate to the contact page
  Then the page should load within 5 seconds
  And the DOM should be ready within 3 seconds

Scenario: Blazor WASM initializes properly
  When I navigate to the homepage
  Then Blazor should be available in the browser
  And the Blazor runtime should be loaded

Scenario: Static assets load correctly
  When I navigate to the homepage
  Then all images should load successfully
  And the CSS should be loaded
  And the JavaScript should be loaded

Scenario: No console errors during page load
  When I navigate to the homepage
  Then there should be no JavaScript errors
  And there should be no console errors

Scenario: Page is responsive and mobile-friendly
  When I navigate to the homepage on mobile
  Then the page should have viewport meta tag
  And the page should be mobile-friendly

Scenario: Navigation between pages is smooth
  Given I am on the homepage
  When I navigate to the services page
  And I navigate to the contact page
  And I navigate back to the homepage
  Then all pages should load quickly
  And there should be no console errors

Scenario: First paint happens quickly
  When I navigate to the homepage
  Then the first paint should occur within 2 seconds

@performance
Scenario: Multiple page loads are consistent
  When I load the homepage 3 times
  Then all load times should be similar
  And there should be no performance degradation

@performance
Scenario: Page metrics are acceptable
  When I navigate to the homepage
  Then the time to interactive should be less than 6 seconds
  And the first contentful paint should be less than 2 seconds
