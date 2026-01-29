Feature: Blog Post Detail Page
  As a visitor
  I want to read individual blog posts
  So that I can learn from the technical content

  # NOTE: These tests are prepared for when blog pages are implemented
  # They will be skipped if blog pages don't exist yet

@future @blog
Scenario: Blog post detail page loads successfully
  Given blog syndication has completed
  When I navigate to a blog post detail page
  Then I should see the post title
  And I should see the post content
  And I should see the post metadata

@future @blog
Scenario: Blog post displays full content
  Given blog syndication has completed
  When I navigate to a blog post detail page
  Then the post HTML content should be rendered
  And the content should be properly formatted
  And code blocks should be syntax highlighted

@future @blog
Scenario: Blog post displays metadata
  Given blog syndication has completed
  When I navigate to a blog post detail page
  Then I should see the publish date
  And I should see the author name
  And I should see the categories
  And I should see the tags

@future @blog
Scenario: Syndicated posts display canonical URL
  Given blog syndication has completed
  When I navigate to a syndicated blog post
  Then I should see a canonical URL link
  And the canonical link should point to the original post

@future @blog
Scenario: Blog post has proper meta tags for SEO
  Given blog syndication has completed
  When I navigate to a blog post detail page
  Then the page should have a title meta tag
  And the page should have a description meta tag
  And the page should have Open Graph meta tags

@future @blog
Scenario: Blog post has table of contents
  Given blog syndication has completed
  And the post has UseToc enabled
  When I navigate to the blog post detail page
  Then I should see a table of contents
  And clicking TOC links should scroll to sections

@future @blog
Scenario: Navigate between blog posts
  Given blog syndication has completed
  When I navigate to a blog post detail page
  Then I should see a "Previous Post" link (if available)
  And I should see a "Next Post" link (if available)
  When I click "Next Post"
  Then I should navigate to the next blog post

@future @blog
Scenario: Blog post URL uses slug
  Given blog syndication has completed
  When I navigate to a blog post detail page
  Then the URL should contain the post slug
  And the slug should be URL-friendly

@future @blog
Scenario: Blog post displays reading time
  Given blog syndication has completed
  When I navigate to a blog post detail page
  Then I should see the estimated reading time
  And the reading time should be calculated from word count

@future @blog
Scenario: Related posts are displayed
  Given blog syndication has completed
  When I navigate to a blog post detail page
  Then I should see related posts section
  And related posts should share categories or tags

@future @blog
Scenario: Code snippets are properly formatted
  Given blog syndication has completed
  And the post contains code snippets
  When I navigate to the blog post detail page
  Then code blocks should have syntax highlighting
  And code blocks should have copy buttons
  And code blocks should display the language
