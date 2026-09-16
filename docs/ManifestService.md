# ManifestService

The `ManifestService` class provides a concrete implementation of the `IManifestService` interface for building Web App Manifests used in Progressive Web Applications (PWAs). The manifest object is constructed on first call and reused for the lifetime of the service (registered as a singleton).

## API

### Constructor
```csharp
public ManifestService(IConfiguration config, ILogger<ManifestService> logger)
```
Initializes the service with configuration and a logger.

### Properties
* `ThemeColor`: Gets the theme color from configuration or uses the default "#2563eb".
* `BackgroundColor`: Gets the background color from configuration or uses the default "#f8fafc".

### Methods
* `BuildManifest(string requestScheme, string? requestHost = null)`: Builds the Web App Manifest from application configuration.

## Usage

Here's an example of using the `ManifestService` class:
```csharp
// Example: Creating and using ManifestService
var manifestService = new ManifestService(configuration, logger);
var manifest = manifestService.BuildManifest("https", "example.com");

// The manifest can then be used to configure PWA behavior
// or serialized to JSON for the manifest.json file
```

## Implementation Details

The service uses the following default values when configuration is not provided:
* Name: "AspNet SPA Template"
* ShortName: "SPA Template"
* Description: "A production-ready ASP.NET Core SPA with vanilla JavaScript frontend"
* StartUrl: "/"
* Scope: "/"
* Display: "standalone"
* Orientation: "portrait-primary"
* Lang: "en"
* Categories: ["productivity", "utilities"]
* PreferRelatedApplications: false

Icons are built from:
* icon-192.png (192x192, image/png, purpose: any maskable)
* icon-512.png (512x512, image/png, purpose: any maskable)

Shortcuts include:
* Browse Products (/?page=products)
* Shopping Cart (/?page=cart)

The service logs debug information when building the manifest.