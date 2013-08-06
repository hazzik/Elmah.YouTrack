Elmah.YouTrack
==============

It is a logger for elmah which is aim to submit exceptions to your YouTrack tracker.


##Basic configuration

To configure logger put

```xml
<elmah>

 <!--...-->

   <errorLog type="Elmah.YouTrack.YouTrackErrorLog, Elmah.YouTrack" 
      host="your_youtrack_host_name"
      applicationName="MyAppName" />

<!--...-->

</elmah>
```

##Configuration parameters

* **host** - *string*, *required* - the host name of your YouTrack server
* **port** - *number*, *optional*, default is `80` - the port of your YouTrack server application
* **useSsl** - *boolean*, *optional*, default is `false` - set to `true` if you are using SSL on your server
* **path** - *string*, *optional* - the path of the YouTrack application at the server
* **username** - *string*, *optional* - the user name 
* **username** - *string*, *optional* - the password
* **project** - *string*, *required* - the project
