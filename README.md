Elmah.YouTrack
==============

It is a logger for elmah which is aim to submit exceptions to your YouTrack tracker.


Basic configuration

```xml
<elmah>

 <!--...-->

   <errorLog type="Elmah.YouTrack.YouTrackErrorLog, Elmah.YouTrack" 
      host="your_youtrack_host_url"
      applicationName="MyAppName" />

<!--...-->

</elmah>
```
