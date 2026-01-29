Feature: Blog Syndication
  As a content manager
  I want to syndicate blog posts from GitHub
  So that technical posts are automatically imported to the website

Background:
  Given the blog syndication tool is available

Scenario: Successfully syndicate posts from GitHub
  When I run the blog syndication tool
  Then the syndication should complete successfully
  And posts.json should be generated
  And posts.json should contain valid data

Scenario: Validate generated posts structure
  Given blog syndication has completed
  When I load the posts.json file
  Then all posts should have required fields
  And all posts should have an ID
  And all posts should have a title
  And all posts should have a date
  And all posts should have content
  And all posts should have HTML content
  And all posts should have a stub

Scenario: Filter out personal posts
  Given blog syndication has completed
  When I load the posts.json file
  Then no posts should have the "Personal" category
  And no posts should have the "Draft" tag

Scenario: Validate syndicated posts have canonical URLs
  Given blog syndication has completed
  When I load the posts.json file
  Then all syndicated posts should have a canonical URL
  And all canonical URLs should be valid HTTP URLs

Scenario: Validate posts are sorted by date
  Given blog syndication has completed
  When I load the posts.json file
  Then posts should be sorted by date descending

Scenario: Validate HTML content conversion
  Given blog syndication has completed
  When I load the posts.json file
  Then all posts should have HTML content
  And HTML content should not contain raw markdown

Scenario: Validate posts.json file size
  Given blog syndication has completed
  When I check the posts.json file
  Then the file size should be reasonable
  And the file should contain at least 1 post
  And the file should be smaller than 10MB

Scenario: Validate post metadata
  Given blog syndication has completed
  When I load the posts.json file
  Then all posts should have valid metadata
  And all stubs should be URL-friendly
  And all word counts should be greater than zero

Scenario: Filter by included categories
  Given blog syndication has completed
  When I load the posts.json file
  Then only Technical posts should be included
  And posts should have at least one included category

Scenario: Validate posts have proper source attribution
  Given blog syndication has completed
  When I load the posts.json file
  Then all syndicated posts should have source set to "syndicated"
  And syndicated posts should have a canonical URL

Scenario: Handle missing or invalid posts gracefully
  Given the blog syndication tool is available
  When I run the blog syndication tool
  Then the tool should handle errors gracefully
  And should continue processing valid posts
