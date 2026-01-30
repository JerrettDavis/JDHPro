Feature: Blog Listing Page
  As a visitor
  I want to view the blog listing page
  So that I can see all available blog posts

  # NOTE: These tests are prepared for when blog pages are implemented
  # They will be skipped if blog pages don't exist yet

@future @blog
Scenario: Blog listing page loads successfully
  When I navigate to the blog page
  Then I should see the page title "Blog"
  And the page should display blog posts

@future @blog
Scenario: Blog posts are displayed from posts.json
  Given blog syndication has completed
  When I navigate to the blog page
  Then I should see blog post cards
  And each post should display a title
  And each post should display a date
  And each post should display categories

@future @blog
Scenario: Blog posts link to detail pages
  Given blog syndication has completed
  When I navigate to the blog page
  And I click on a blog post
  Then I should navigate to the post detail page

@future @blog
Scenario: Blog posts are sorted by date
  Given blog syndication has completed
  When I navigate to the blog page
  Then posts should be displayed in date order (newest first)

@future @blog
Scenario: Blog posts display metadata
  Given blog syndication has completed
  When I navigate to the blog page
  Then each post should show the publish date
  And each post should show categories
  And each post should show tags (if available)

@future @blog
Scenario: Filter blog posts by category
  Given blog syndication has completed
  When I navigate to the blog page
  And I filter by "Technical" category
  Then only posts in "Technical" category should be shown

@future @blog
Scenario: Search blog posts
  Given blog syndication has completed
  When I navigate to the blog page
  And I search for "testing"
  Then only posts containing "testing" should be shown

@future @blog
Scenario: Pagination works correctly
  Given blog syndication has completed
  And there are more than 5 posts
  When I navigate to the blog page
  Then I should see pagination controls
  And I should see up to 10 posts per page
  When I click next page
  Then I should see the next set of posts
