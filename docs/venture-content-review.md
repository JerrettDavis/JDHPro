# Venture and content review

Routes: /products (ventures), /projects (public portfolio), /labs (research areas),
and the existing /blog (Insights), /services, /about, /contact. No old live route
is removed. The header uses ordinary accessible link labels; the mobile menu
appears earlier to accommodate the expanded navigation. The product/portfolio
cards use text instead of unapproved or fabricated product artwork.

The linked repositories TinyBDD, FlowLedger and JDHPro were checked for public
visibility. FlowLedger is labeled in development and is not represented as a
launched product. No client logos, endorsements, fabricated case studies or
unsupported metrics are included. Confirm release messaging and corporate
ownership attribution before publishing. Add approved screenshots and case
studies over time; preserve venture-specific brand assets rather than recolor
them using the parent theme.

The service directory remains driven by Markdown content; fabrication links to
the dedicated venture rather than duplicating its offering. Keep the original
contact provider and blog syndication behavior. Additional legacy page
copy/contrast review is required, particularly form validation states and any
remaining hard-coded styling. The existing CI workflow only targets main and
currently disables its E2E job; run the documented Playwright suite before merge.
