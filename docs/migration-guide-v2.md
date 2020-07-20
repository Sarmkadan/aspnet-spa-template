## Migration Guide to v2.0

### Breaking Changes

* None

### New Features

* Progressive web app support with offline sync and push notifications

### Step-by-Step Migration

1. Update the `appsettings.json` file to include the new PWA settings.
2. Run the database migration script to add the new tables for PWA support.
3. Update the frontend code to use the new PWA features.

### Code Examples

#### Old API
```csharp
// Old API example
```
#### New API
```csharp
// New API example
```

### Configuration Changes

* Update the `PwaOptions` class to include the new VAPID key material.
* Configure the push notification delivery timeout and retry policy.

### Troubleshooting

* Check the application logs for any errors related to PWA support.
* Verify that the VAPID key material is correctly configured.

### Conclusion

The migration to v2.0 of the ASP.NET SPA template includes several new features and improvements. By following the step-by-step migration guide, you can easily update your application to take advantage of these new features.