Feature: Venture and portfolio navigation
  As a visitor I want to explore the company and its projects

Scenario Outline: The new destinations are available from the homepage
  Given I navigate to the homepage
  When I click "<link>" in the navigation
  Then I should be on the "<route>" page
  And I should see the page title "<heading>"
  Examples:
    | link     | route     | heading              |
    | Products | /products | Products & ventures  |
    | Work     | /projects | Projects             |
    | Labs     | /labs     | JDH Labs             |

Scenario: The project directory links to real repositories
  Given I navigate to "/projects"
  Then I should see "TinyBDD"
  And I should see "JDH Productions website"
