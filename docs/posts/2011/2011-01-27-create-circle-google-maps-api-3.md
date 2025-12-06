---
title: "Create circle around google maps API v3"
categories:
  - Blog
tags:
  - link
  - Post Formats
---

Using the Google Maps API V3, create a Circle object, then use bindTo() to tie it to the position of your Marker (since they are both google.maps.MVCObject instances).

```js
// Create marker 
var marker = new google.maps.Marker({
  map: map,
  position: new google.maps.LatLng(53, -2.5),
  title: 'Some location'
});


// Add circle overlay and bind to marker
var circle = new google.maps.Circle({
  map: map,
  radius: 16093,    // 10 miles in metres
  fillColor: '#AA0000'
});
circle.bindTo('center', marker, 'position');
```

You can make it look just like the Google Latitude circle by changing the fillColor, strokeColor, strokeWeight etc (full API).
See more source code and example screenshots - http://seriouscodage.blogspot.com/2010/11/visualizing-data-as-circles-in-google.html

