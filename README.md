# Multi Discord RPC
 Discord RPC that is able to utilize multiple rich presence applications.
 
 
## Writing an App
 Apps in this program are made with JSON, an example app for google chrome is:<br/><br/>

```
{
    "app_name": "Google Chrome",
    "app_id": "{APPLICATION ID}",
    "proc_name": "chrome",

    "details": "Watching Youtube",
    "state": "I don't know",
    "large_img_key": "{LARGE ICON}",
    "large_img_text": "Oh hello!",
    "small_img_key": "{SMALL ICON}",
    "small_img_text": "Cats!"
}
```

``` "proc_name" ``` -- Process Name (Without file extension)<br/>
``` "app_name" ``` -- Application name (This does not affect the RPC App, its just for identification.)<br/>
``` "app_id" ``` -- Application ID (You can create an Application [here](https://discord.com/developers/applications).)<br/>

Applications are saved in the **apps** directory. 
