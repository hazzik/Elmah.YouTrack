Elmah.YouTrack
==============

This is a logger for ELMAH which is aim to submit exceptions to your YouTrack tracker.


##Basic configuration

```xml
<elmah>
    <!--...-->
    <youtrack
        url="your_youtrack_URI_address"
        project="MyProject" />
    <!--...-->
</elmah>
```

##Configuration parameters

* **project** - *string*, *required* - the project
* **password** - *string*, *optional* - the password
* **username** - *string*, *optional* - the user name 

Connection configuration


* **url** - *URI*, *required* - the URL of your YouTrack server

**OR**

* **host** - *string*, *required* - the host name of your YouTrack server
* **port** - *number*, *optional*, default is `80` - the port of your YouTrack server application
* **useSsl** - *boolean*, *optional*, default is `false` - set to `true` if you are using SSL on your server
* **path** - *string*, *optional* - the path of the YouTrack application at the server
