# LocalSettings

This solution contains a service instance which is able to read and write settings using a yaml file in a configured location.
As this service has a very lean and clear interface, it can be consumed also by backend services who shall not learn application setting references.

# Configure the LocalSettingService

The setting service needs to be configured once in order to know which actual file to use.

        // get your service from DI or create the service instance manually
        var localPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MyApplicationOrCompany");

        var configFile = new FileInfo(Path.Combine(localPath, "ApplicationSettings.yaml");

        service.Configure(configFile, SettingWriteMode.OnChange);

        // Now you can inject or pass the service and start reading/writing values.

## Write Modes

The service supports two separate write modes:

- OnChange: Every `Set()` call will immediately cause the yaml file be written.
- Manual: The application needs to call the `WriteSettings` method on exit or whener required.
  Use this mode only if you have to write a large number of values in a limited time span - then this mode will save you write access calls.

  ## Consuming complex setting classes

  Feel free to use the extension methods `SetComplexValue` and `GetComplexValue` in order to serialize more complex setting objects.
  Be aware that the passed objects also have to be serialized into yaml.

# Binding ILocalSettingService in Microsoft DependencyInjection

In your `Startup.cs`, add this to define the binding:
`services.AddSingleton<ILocalSettingService, LocalSettingService>();`
