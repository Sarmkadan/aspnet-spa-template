## ThemeServiceUnitTests

The ThemeServiceUnitTests class contains unit tests for the ThemeService class. These tests cover various scenarios, including:

* Getting a scheme when nothing is stored
* Getting a scheme after storing a dark or light scheme
* Getting a scheme with a user ID of zero or a negative number
* Getting a scheme with the maximum user ID
* Setting a scheme with a valid or invalid user ID
* Setting a scheme with the same scheme that is already stored
* Clearing a scheme and verifying the result

Example usage:

```csharp
public async Task GetSchemeAsync_WhenNothingStored_ReturnsSystem
public async Task GetSchemeAsync_AfterStoringDark_ReturnsDark
public async Task GetSchemeAsync_AfterStoringLight_ReturnsLight
public async Task GetSchemeAsync_WithUserIdZero_ThrowsArgumentOutOfRangeException
public async Task GetSchemeAsync_WithNegativeUserId_ThrowsArgumentOutOfRangeException
public async Task GetSchemeAsync_WithMaxUserId_ReturnsSystem
public async Task SetSchemeAsync_WithSystem_ClearsPreference
public async Task SetSchemeAsync_OverwritesExistingPreference
public async Task SetSchemeAsync_WithSameScheme_DoesNotWriteToCache
public async Task SetSchemeAsync_WithUserIdZero_ThrowsArgumentOutOfRangeException
public async Task SetSchemeAsync_WithNegativeUserId_ThrowsArgumentOutOfRangeException
public async Task SetSchemeAsync_AcceptsAllColourSchemes
public async Task SetSchemeAsync_WhenCacheFails_ThrowsBusinessException
public async Task ClearSchemeAsync_ReturnsSystemAfterClear
public async Task ClearSchemeAsync_WhenNoPreferenceExists_DoesNotThrow
public async Task ClearSchemeAsync_WithUserIdZero_ThrowsArgumentOutOfRangeException
public async Task ClearSchemeAsync_WithNegativeUserId_ThrowsArgumentOutOfRangeException
public async Task ClearSchemeAsync_WithMaxUserId_DoesNotThrow
public async Task ThemeChanged_EventIsRaised_WhenThemeIsSet
public async Task ThemeChanged_EventIsRaised_WhenThemeIsCleared
```
