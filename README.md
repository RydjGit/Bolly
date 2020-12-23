# Bolly

Bolly is an credentials stuffing tool based on OpenBullet high performance because it reuses the same http client [httpclient best practices](https://www.thecodebuzz.com/using-httpclient-best-practices-and-anti-patterns/) support HTTP proxies only

# Usage

Open your cmd and copy
```
> Bolly.exe your_config.json your_combos.txt your_proxies.txt ( optional )
```

# Config Example

```json
{
   "Settings":{
      "Name":"Example",
      "UseProxies":false,
      "UseCookies":false,
      "AllowAutoRedirect":true,
      "MaxDegreeOfParallelism":1
   },
   "Blocks":[
      {
         "Block":"Request",
         "Methode":"Get",
         "Url":"https://api.ipify.org?format=json",
         "ContentType":"",
         "Content":"",
         "Headers":[],
         "LoadSource":true
      },
      {
         "Block":"Parse",
         "ParseName":"IP",
         "Source":"<SOURCE>",
         "Methode":"JSON",
         "FirstInput":"ip",
         "SecondInput":"",
         "Capture":true
      },
      {
         "Block":"Parse",
         "ParseName":"IP",
         "Source":"<SOURCE>",
         "Methode":"REGEX",
         "FirstInput":"^(?:[0-9]{1,3}\\.){3}[0-9]{1,3}$",
         "SecondInput":"0",
         "Capture":true
      },
      {
         "Block":"KeyCheck",
         "KeyCheckPatterns":[
            {
               "Status":"Invalid",
               "Source":"Bad key",
               "Condition":"Contains",
               "Key":"Bad"
            },
            {
               "Status":"Free",
               "Source":"Bad key",
               "Condition":"EqualTo",
               "Key":"Bad key"
            },
            {
               "Status":"Success",
               "Source":"5",
               "Condition":"LessThan",
               "Key":"10"
            },
            {
               "Status":"Unknown",
               "Source":"5",
               "Condition":"GreaterThan",
               "Key":"10"
            },
            {
               "Status":"Retry",
               "Source":"Bad key",
               "Condition":"RegexMatch",
               "Key":"/d(b+)d/g"
            },
         ],
         "RetryIfNotFound":true
      }
   ]
}
```
