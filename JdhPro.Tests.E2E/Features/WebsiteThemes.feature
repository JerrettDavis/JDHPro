Feature: Website themes
  As a visitor
  I want to choose brand identity and color mode independently

Scenario Outline: Every identity supports light and dark
  Given I navigate to the homepage
  When I select the website identity "<identity>"
  And I select the website color mode "<mode>"
  Then the resolved website theme should be "<theme>"
  And the appearance controls should show "<identity>" and "<mode>"
  When I reload the website
  Then the resolved website theme should be "<theme>"
  And the appearance controls should show "<identity>" and "<mode>"
  Examples:
    | identity     | mode  | theme             |
    | professional | light | professional      |
    | professional | dark  | professional-dark |
    | vaporwave    | light | vaporwave-light   |
    | vaporwave    | dark  | vaporwave         |
    | grayscale    | light | grayscale         |
    | grayscale    | dark  | grayscale-dark    |
    | monochrome   | light | monochrome-light  |
    | monochrome   | dark  | monochrome-dark   |

Scenario: Changing identity preserves color mode
  Given I navigate to the homepage
  When I select the website color mode "dark"
  And I select the website identity "vaporwave"
  Then the resolved website theme should be "vaporwave"
  And the appearance controls should show "vaporwave" and "dark"
  When I select the website identity "grayscale"
  Then the resolved website theme should be "grayscale-dark"
  And the appearance controls should show "grayscale" and "dark"

Scenario: Shareable appearance URL overrides stored preferences
  Given I navigate to the homepage
  When I select the website identity "grayscale"
  And I select the website color mode "dark"
  And I open an appearance URL for "vaporwave" and "light"
  Then the resolved website theme should be "vaporwave-light"
  And the appearance controls should show "vaporwave" and "light"

Scenario: Legacy theme links remain compatible
  Given I navigate to the homepage
  When I open a theme URL for "vaporwave"
  Then the resolved website theme should be "vaporwave"
  And the appearance controls should show "vaporwave" and "dark"
  When I select the website color mode "light"
  Then the resolved website theme should be "vaporwave-light"
  And the appearance controls should show "vaporwave" and "light"
  When I reload the website
  Then the resolved website theme should be "vaporwave-light"

@mobile
Scenario: Both appearance controls are available on mobile
  Given I navigate to the homepage
  Then both appearance controls should be visible
  When I select the website identity "vaporwave"
  And I select the website color mode "dark"
  Then the resolved website theme should be "vaporwave"

Scenario Outline: Appearance menus support keyboard dismissal
  Given I navigate to the homepage
  When I open the "<axis>" appearance menu with the keyboard
  Then the active "<axis>" option should have keyboard focus
  When I dismiss the "<axis>" appearance menu with Escape
  Then the resolved website theme should be "professional"
  Examples:
    | axis     |
    | mode     |
    | identity |

Scenario Outline: System mode follows the device without changing identity
  Given the device uses "dark" appearance
  And I navigate to the homepage
  When I select the website identity "<identity>"
  And I select the website color mode "system"
  Then the resolved website theme should be "<dark-theme>"
  And the appearance controls should show "<identity>" and "system"
  When the device uses "light" appearance
  Then the resolved website theme should be "<light-theme>"
  And the appearance controls should show "<identity>" and "system"
  When I reload the website
  Then the resolved website theme should be "<light-theme>"
  And the appearance controls should show "<identity>" and "system"
  Examples:
    | identity     | dark-theme        | light-theme      |
    | professional | professional-dark | professional     |
    | vaporwave    | vaporwave         | vaporwave-light  |
    | grayscale    | grayscale-dark    | grayscale        |
    | monochrome   | monochrome-dark   | monochrome-light |
