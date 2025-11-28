---
permalink: /contact/
title: "Contact Me"
---

<form id="contactme" name="contact-form" netlify>
<label for="emailAddr">Your Email</label> 
<input id="emailAddr" type="text" name="email" /><br />
<label for="msgArea">Message :</label>
<textarea id="msgArea" name="message" rows="5" cols="40"></textarea>
<input type="submit" name="submit" />
</form>


Subscribe to my newsletter with your email to keep up to date with content. And dont worry, it wont be sold or spammed.

<form style="padding:3px;text-align:center;" action="https://feedburner.google.com/fb/a/mailverify" method="post" target="popupwindow" onsubmit="window.open('https://feedburner.google.com/fb/a/mailverify?uri=stempy', 'popupwindow', 'scrollbars=yes,width=550,height=520');return true">
    <div style="padding:4px">
        <label for="usrEmail">Please enter your email to subscribe to my newsletter</label>
        <input id="usrEmail" type="text" style="width:160px; border:1px solid #9a9a9a;" name="email"/>
        <div style="clear:both"></div>
    </div>
    <input type="hidden" value="stempy" name="uri"/>
    <input type="hidden" name="loc" value="en_US"/>
    <input type="submit" value="Subscribe" />
</form>
