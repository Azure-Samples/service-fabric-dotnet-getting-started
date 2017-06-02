# Using Service Fabric with Application Insights

This branch of the GettingStartedApplication demonstrates how to use Application Insights with a service fabric application.

# Setting a global Instrumentation Key

In the `ApplicationManifest.xml`, the following parameter sets the instrumentation key used by all services:
```xml
  <Parameters>
    <Parameter Name="ApplicationInsightsKey" DefaultValue="<your instrumentation key here>" />
  </Parameters>
```

In the same file, the `ServiceManifestImport` section for each service sets this key for each service:
```xml
  <ServiceManifestImport>
    <ConfigOverrides>
      <ConfigOverride Name="Config">
        <Settings>
          <Section Name="ApplicationInsights">
            <Parameter Name="InstrumentationKey" Value="[ApplicationInsightsKey]"/>
          </Section>
        </Settings>
      </ConfigOverride>
    </ConfigOverrides>
  </ServiceManifestImport>
```

In the `PackageRoot/Config/Config.xml` file for each service, the config paramater is defined with an empty value:
```xml
  <Section Name="ApplicationInsights">
    <Parameter Name="InstrumentationKey" Value="" />
  </Section>
```

In the constructor of each service, the following code initializes Application Insights for each service using the global instrumentation key:
```cs
    using Microsoft.ApplicationInsights.ServiceFabric;
    using Microsoft.ApplicationInsights.Extensibility;

    ...

    public StatefulBackendService(StatefulServiceContext context)
        : base(context)
    {
        var telemetryConfig = TelemetryConfiguration.Active;
        FabricTelemetryInitializerExtension.SetServiceCallContext(context);
        var config = context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
        var appInsights = config.Settings.Sections["ApplicationInsights"];
        telemetryConfig.InstrumentationKey = appInsights.Parameters["InstrumentationKey"].Value;
    }
```

# Resources
We are continuing to update this repo to demonstrate how to use Service Fabric with Application Insights.

For more information, check out our [ApplicationInsights Service Fabric SDK Github page](https://github.com/Microsoft/ApplicationInsights-ServiceFabric/blob/develop/README.md).