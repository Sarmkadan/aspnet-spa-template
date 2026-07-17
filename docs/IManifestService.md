# IManifestService
The `IManifestService` interface provides a set of properties that define the metadata for a web application, allowing it to be installed and launched as a Progressive Web App (PWA). This metadata includes information such as the application's name, description, and icons, as well as its behavior and appearance.

## API
The `IManifestService` interface includes the following properties:
* `Name`: The full name of the application.
* `ShortName`: A short name for the application, suitable for use in contexts where space is limited.
* `Description`: A brief description of the application.
* `StartUrl`: The URL that the application should start at when launched.
* `Scope`: The scope of the application, defining the set of URLs that it can handle.
* `Display`: The display mode of the application, such as "fullscreen" or "standalone".
* `Orientation`: The preferred orientation of the application, such as "portrait" or "landscape".
* `BackgroundColor`: The background color of the application.
* `ThemeColor`: The theme color of the application.
* `Lang`: The language of the application.
* `Categories`: A list of categories that the application belongs to.
* `Icons`: A list of icons that represent the application.
* `Shortcuts`: A list of shortcuts that can be used to launch the application.
* `PreferRelatedApplications`: A flag indicating whether the application prefers to be launched in the context of related applications.
* `Src`: The source of the application's manifest.
* `Sizes`: The sizes of the application's icons.
* `Type`: The type of the application's icons.
* `Purpose`: The purpose of the application.

## Usage
Here are two examples of using the `IManifestService` interface:
```csharp
// Example 1: Creating a new IManifestService instance
var manifestService = new ManifestService
{
    Name = "My Application",
    ShortName = "My App",
    Description = "A brief description of my application",
    StartUrl = "/",
    Scope = "/",
    Display = "standalone",
    Orientation = "portrait",
    BackgroundColor = "#ffffff",
    ThemeColor = "#000000",
    Lang = "en-US",
    Categories = new List<string> { "category1", "category2" },
    Icons = new List<ManifestIcon> { new ManifestIcon { Src = "icon.png", Sizes = "192x192" } },
    Shortcuts = new List<ManifestShortcut> { new ManifestShortcut { Name = "Shortcut 1", Url = "/shortcut1" } },
    PreferRelatedApplications = true,
    Src = "manifest.json",
    Sizes = "192x192",
    Type = "image/png",
    Purpose = "any"
};

// Example 2: Using an IManifestService instance to configure an application
var manifestService2 = new ManifestService
{
    Name = "My Application 2",
    ShortName = "My App 2",
    Description = "A brief description of my application 2",
    StartUrl = "/",
    Scope = "/",
    Display = "fullscreen",
    Orientation = "landscape",
    BackgroundColor = "#000000",
    ThemeColor = "#ffffff",
    Lang = "fr-FR",
    Categories = new List<string> { "category3", "category4" },
    Icons = new List<ManifestIcon> { new ManifestIcon { Src = "icon2.png", Sizes = "512x512" } },
    Shortcuts = new List<ManifestShortcut> { new ManifestShortcut { Name = "Shortcut 2", Url = "/shortcut2" } },
    PreferRelatedApplications = false,
    Src = "manifest2.json",
    Sizes = "512x512",
    Type = "image/jpeg",
    Purpose = "installation"
};
```

## Notes
When using the `IManifestService` interface, note that the `Categories`, `Icons`, and `Shortcuts` properties are lists, and therefore can be null or empty. Additionally, the `PreferRelatedApplications` property is a flag, and its value should be carefully considered based on the application's requirements. The `Src`, `Sizes`, and `Type` properties are related to the application's icons, and should be set accordingly. The `Purpose` property is used to define the purpose of the application, and should be set to one of the allowed values. The `IManifestService` interface is not thread-safe, and its instances should not be shared across multiple threads.
