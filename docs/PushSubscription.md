# PushSubscription

Represents a push notification subscription for a user, containing endpoint and cryptographic keys required for sending push messages via a service worker or similar client.

## API

### Properties

- **`Id`** (int)
  Unique identifier for the subscription. Read-only after creation.

- **`UserId`** (int)
  Identifier of the user associated with this subscription. Required on creation; immutable afterward.

- **`Endpoint`** (string)
  The URL endpoint where push messages should be delivered. Must be a valid HTTPS URL. Set on creation and immutable afterward.

- **`P256dhKey`** (string)
  The P-256 ECDH public key used for encrypting push message payloads. Required on creation; immutable afterward.

- **`AuthKey`** (string)
  The authentication secret used for validating push message integrity. Required on creation; immutable afterward.

- **`DeviceLabel`** (string?)
  Optional human-readable label for the device or client associated with this subscription (e.g., "My iPhone").

- **`UserAgent`** (string?)
  Optional string identifying the user agent or client software making the subscription request.

- **`IsActive`** (bool)
  Indicates whether the subscription is currently active and eligible to receive notifications. Defaults to `true` on creation. Can be toggled via `Deactivate()`.

- **`LastActiveAt`** (DateTime?)
  Timestamp of the last successful delivery or activity related to this subscription. Updated by `RecordDelivery()`. May be `null` if never activated.

- **`CreatedAt`** (DateTime)
  Timestamp when the subscription was created. Set automatically; immutable afterward.

- **`UpdatedAt`** (DateTime)
  Timestamp of the last update to the subscription. Updated automatically on changes; immutable otherwise.

- **`User`** (User?)
  Navigation property referencing the `User` entity associated with this subscription. May be `null` if the user has been deleted.

### Methods

- **`RecordDelivery()`** (void)
  Records a successful delivery event by updating `LastActiveAt` to the current UTC time and setting `IsActive` to `true`. No parameters or return value. Safe to call multiple times; idempotent.

- **`Deactivate()`** (void)
  Marks the subscription as inactive by setting `IsActive` to `false` and recording the deactivation time in `UpdatedAt`. No parameters or return value. Safe to call multiple times; idempotent.

## Usage
