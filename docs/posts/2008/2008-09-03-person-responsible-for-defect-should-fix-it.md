---
title: "Person responsible for defect should fix it"
categories:
  - Blog
tags:
  - link
  - Post Formats
---

- Person responsible for defect should be one following it up initially
- Defects appearing due to HTML change (support.aspx form changed form field values in html)
- Subversion causing issues on pc, ID' being changed inadvertently.
- defects appearing due to code moving dev to staging without config changes (add friends being particular case several times), not a true defect
- The defect itself as opposed to issue
- Specifications somewhat vague, lack of planning
- Back and forth juggling act betwwen designer, developer, lead
  
Specific Issues

- Support.aspx initially first defect (support not working) was due to form fields changed by designer, affecting calls from code-behind, while not designers issue, a notification that it had been changed before retest would have required a 5 minute change as opposed to defect cycle.
- Add Friends not working when changing domain environment, known issue due to api keys for different services.
- IP Blocking, the ipblocker works ok, just nowhere to send user, or display simple message.
  
- Changing Requirements every day, add email in support page 3/9/2008, why wasnt this specified in requirements
