# Exercise 3: The Silent Crash (Options Pattern)
Context: The TMS relies on an external payment gateway for tuition processing
(measured in Ethiopian Birr). The appsettings.json is missing the GatewayUrl key. Because the previous developer used IConfiguration["GatewayUrl"] inline during checkout, the application runs perfectly until a student tries to pay, throwing a massive server


error.[!CAUTION] Think about this before coding: If an application is missing a
critical configuration setting, is it better for it to crash entirely upon startup, or
crash later when a user tries to use the specific feature?

# Your Task: Validate Configuration at Startup
### Exercise 3: The Silent Crash (Options Pattern)

**Context:** The TMS relies on an external payment gateway for tuition processing (measured in Ethiopian Birr). The `appsettings.json` is missing the `GatewayUrl` key. Because the previous developer used `IConfiguration["GatewayUrl"]` inline during checkout, the application runs perfectly until a student tries to pay, throwing a massive server error.

> [!CAUTION]
> **Think about this before coding:** If an application is missing a critical configuration setting, is it better for it to crash entirely upon startup, or crash later when a user tries to use the specific feature?

> [!NOTE]
> **Your Task: Validate Configuration at Startup**
> 
> Build a strongly-typed options class and wire it so the app refuses to start with invalid configuration.
> 
> **Class Setup:**
> ```csharp
> // TODO 1: Create a class called PaymentOptions with two properties:
> // - GatewayUrl (string, required use [Required] attribute)
> // - MaxDepositBirr (decimal, range 100-100000 use [Range] attribute)
> 
> public class PaymentOptions 
> { 
>     [Required] 
>     public required string GatewayUrl { get; init; }
>     
>     [Range(100, 100000)]
>     public decimal MaxDepositBirr { get; init; }
> }
> ```
> 
> **Service Registration:**
> ```csharp
> // TODO 2: In Program.cs, bind PaymentOptions to the "Payments" section of appsettings.json
> // and enable startup validation. 
> 
> builder.Services.AddOptions<PaymentOptions>()
>     .BindConfiguration("Payments")
>     .ValidateDataAnnotations()
>     .ValidateOnStart();
> ```
> 
> **Testing Phase:**
> * **TODO 3:** Delete the "Payments" section from `appsettings.json` and run the app. 
> * What error do you see? Does the app start or crash immediately?

* **Run / Call / Expected / Common failure**
  * **Run:** `dotnet run` after removing or commenting out the entire "Payments" section in `appsettings.json` (or removing `GatewayUrl` only, if your validators require it).
  * **Call:** None (failure happens directly at startup).
  * **Expected:** Process does not stay running; console shows something like:
    ```text
    Microsoft.Extensions.Options.OptionsValidationException: DataAnnotation validation failed for 'PaymentOptions': 'The GatewayUrl field is required.'
    ```
  * **Common failure app starts anyway:** `.ValidateOnStart()` is missing, or options are not bound to "Payments", or the section name in JSON does not match `.BindConfiguration("Payments")`.