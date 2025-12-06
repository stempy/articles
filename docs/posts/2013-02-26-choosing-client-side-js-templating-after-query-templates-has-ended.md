---
title: "Choose client side js templating after jquery templates has ended"
categories:
  - Blog
tags:
  - Post Formats
--- 

Choosing a client side JS templating system after jquery templates has been abandoned, NOT knockout, just simple templating solution. 

http://stackoverflow.com/questions/10194921/choosing-the-right-ui-templating-tool-dust-js

dust.js
handlebars
mustache

Going to settle on handlebars for meantime, as dust requires precompilation to really be useful.

ok, dust can obtain it's templates source from any content, in this case from a script tag, just use $(selector).html() to grab content, or alternatively from a file or get request etc.

```html
<!-- define notesRow for displaying each row item -->
< script type= "text/x-jquery-tmpl" id="notesRowTpl" >
    < div class= "columns allowEditRow" >
        < div class= "eight-columns eight-columns-mobile" >
            < div class= "noteType {@@noteTypeClass partyNoteTypeId=1}">{PartyNoteTypeStr}</ div>
            < div class= "noteCell" >
                {Note}< br />
                < div class= "createdBy" >
                    < small>Created by {CreatedBy} on {CreatedDate}</small >
                </ div>
            </ div>
        </ div>
        < div class= "four-columns three-columns-mobile align-right" >
            < div class= "float-right button-group compact" style= "margin-top: 0px;" >
                < a href= "edit" data-party-note-id= "{PartyNoteId}" data-cmd= "edit-row" class="ajaxLoader button icon-pencil" >Edit</a>
                < a href= "#" class= "button icon-trash with-tooltip confirm" title="Delete"></ a>
            </ div>
        </ div>
    </ div>
    < hr class= "dottedRow" />
</ script>


<!-- define template for entire notes list display -->
< script type= "text/x-jquery-tmpl" id="notesTblTpl" >
    < div class= "editorTable" >
        {#Notes}
            {>rowdisplay /}
        {/Notes}
    </ div>
</ script>
```

http://blog.teamtreehouse.com/handlebars-js-part-2-partials-and-helpers

