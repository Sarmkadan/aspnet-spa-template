# ManifestController
The `ManifestController` class is responsible for serving the application's dynamically generated Web App Manifest and exposing the colors used by the application's theme. It uses `IManifestService` to build manifest data for the current request origin and to retrieve the configured theme and background colors.

## API
The `ManifestController` class exposes the following public members:
* `public ManifestController`: The constructor for the `ManifestController` class. It requires an `IManifestService` instance.
* `public IActionResult GetManifest`: Handles `GET /manifest.json` and returns the Web App Manifest with the `application/manifest+json` content type. Manifest property names use snake case and properties with null values are omitted. The response includes an ETag and a `Vary: Host` header, and it is publicly cached for one hour. A request whose `If-None-Match` header matches the current ETag receives a `304 Not Modified` response.
* `public IActionResult GetThemeColor`: Handles `GET /api/v1/manifest/theme-color` and returns a JSON object containing the configured `themeColor` and `backgroundColor` values.

## Usage
Here are two examples of calling the endpoints:
```http
GET /manifest.json HTTP/1.1
Host: example.com
Accept: application/manifest+json
```

```http
GET /api/v1/manifest/theme-color HTTP/1.1
Host: example.com
Accept: application/json
```

## Notes
When using the `ManifestController` class, consider the following behavior:
* Manifest icon and shortcut URLs can be generated from the current request scheme and host, allowing the service to produce origin-aware URLs.
* Caches must keep separate manifest representations for different hosts because the response sets `Vary: Host`.
* Clients can use the ETag returned by `GetManifest` in an `If-None-Match` request header to avoid downloading an unchanged manifest.
* `GetThemeColor` returns both the theme color and the background color even though the method name refers only to the theme color.
