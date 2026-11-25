Feature: Website themes
  As a visitor
  I want a consistent brand identity and accessible theme selection

Scenario Outline: Every brand identity can be selected
  Given I navigate to the homepage
  When I select the website theme "<theme>"
  Then the resolved website theme should be "<theme>"
  When I reload the website
  Then the resolved website theme should be "<theme>"
  Examples:
    | theme             |
    | professional      |
    | professional-dark |
    | vaporwave         |
    | vaporwave-light   |
    | grayscale         |
    | monochrome-light  |
    | monochrome-dark   |

Scenario: Shareable URL overrides stored preference
  Given I navigate to the homepage
  When I select the website theme "grayscale"
  And I open a theme URL for "vaporwave"
  Then the resolved website theme should be "vaporwave"

@mobile
Scenario: Theme selection is available on mobile
  Given I navigate to the homepage
  Then the website theme selector should remain visible
  When I select the website theme "professional-dark"
  Then the resolved website theme should be "professional-dark"
