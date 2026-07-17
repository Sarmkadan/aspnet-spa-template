# RegisterSubscriptionRequest

Represents a request to register a push notification subscription for a client application, including subscription details and the payload to be delivered.

## API

### Properties

- **`Endpoint`** (string, required)
  The URL endpoint where push notifications should be delivered. Must be a valid HTTPS URL.

- **`P256dhKey`** (string, required)
  The Base64-encoded P256DH public key used for encrypting push notification payloads. Used in the Web Push protocol to establish a shared secret for payload encryption.

- **`AuthKey`** (string, required)
  The Base64-encoded authentication secret used to verify the integrity of push messages. Part of the Web Push protocol to prevent tampering.

- **`DeviceLabel`** (string?, optional)
  A human-readable label for the device or subscription. May be null or empty. Used for display or debugging purposes.

- **`Title`** (string, required)
  The title of the push notification to be displayed. Used in the notification payload.

- **`Body`** (string, required)
  The body text of the push notification. Used in the notification payload.

- **`Icon`** (string?, optional)
  The URL of an icon to display with the notification. May be null or empty if no icon is provided.

- **`Badge`** (string?, optional)
  The URL of a badge to display with the notification. May be null or empty if no badge is provided.

- **`ActionUrl`** (string?, optional)
  A URL to open when the user interacts with the notification. May be null or empty if no action is defined.

- **`Tag`** (string?, optional)
  A tag to group or identify related notifications. Used to replace existing notifications with the same tag.

- **`Data`** (Dictionary<string, object>?, optional)
  Additional data to include in the push notification payload. May be null if no extra data is required.

- **`UserIds`** (IReadOnlyList<int>, required)
  A read-only list of user IDs associated with this subscription. Used to target notifications to specific users.

- **`Payload`** (PushNotificationPayload, required)
  The structured payload of the push notification, including title, body, and optional data.

- **`ClientRequestId`** (string, required)
  A unique identifier for the client request. Used for tracking and deduplication.

- **`HttpMethod`** (string, required)
  The HTTP method used to deliver the push notification (e.g., "POST").

- **`RelativePath`** (string, required)
  The relative path component of the endpoint URL where the notification should be delivered.

- **`RequestBodyJson`** (string?, optional)
  The serialized JSON body of the push notification request. May be null if no custom body is provided.

- **`SyncTag`** (string, required)
  A synchronization tag used to ensure idempotency or to track the state of the subscription.

- **`Id`** (int, required)
  A unique identifier for the subscription. Used for database persistence and reference.

## Usage

### Registering a Push Subscription
