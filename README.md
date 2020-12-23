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
         "FirstInput":"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}$",
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
               "Status":"Invalid",
               "Source":"Bad key",
               "Condition":"EqualTo",
               "Key":"Bad key"
            },
            {
               "Status":"Invalid",
               "Source":"5",
               "Condition":"LessThan",
               "Key":"9"
            },
            {
               "Status":"Invalid",
               "Source":"5",
               "Condition":"greaterthan",
               "Key":"9"
            },
            {
               "Status":"Invalid",
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
