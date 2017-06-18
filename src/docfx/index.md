# DeviceOAuth2
Limited input device OAuth 2 flow for .NET

[NuGet Package](https://www.nuget.org/packages/DeviceOAuth2/)

Tested with [Google Device OAuth2 Flow](https://developers.google.com/identity/protocols/OAuth2ForDevices) and [Facebook Login for Devices Flow](https://developers.facebook.com/docs/facebook-login/for-devices).

Also tested on Windows IoT Core (see example Facebook app).

OAuth flow for scenarios with limited access to input devices or web browsers, like console apps or IoT devices.

    IDeviceOAuth2 auth = new DeviceOAuth(EndPointInfo.Google, "scope", "client_id", "client_secret");

    auth.PromptUser += (o, e) =>
    {
        Console.WriteLine("Go to this url on any computer:");
        Console.WriteLine(e.VerificationUri);
        Console.WriteLine("And enter this code:");
        Console.WriteLine(e.UserCode);
    };

    var token = await auth.Authorize(null);

    
## Quick Start Notes:
1. [NuGet Package](https://www.nuget.org/packages/DeviceOAuth2/)
2. [GitHub Project](https://github.com/dkackman/DeviceOAuth2/)