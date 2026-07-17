<<<<<<< SEARCH
## License

This project is licensed under the MIT License - see [LICENSE](LICENSE) file for details.




---



**Built by [Vladyslav Zaiets](https://sarmkadan.com) - CTO & Software Architect**



[Portfolio](https://sarmkadan.com) | [GitHub](https://github.com/Sarmkadan) | [Telegram](https://t.me/sarmkadan)
=======
## DateTimeExtensions

Provides a comprehensive set of extension methods for common DateTime operations including date boundary calculations, age calculation, ISO 8601 formatting, and human-readable relative time formatting. These utilities centralize timezone handling and formatting logic to ensure consistency across the application.















**Usage Example:**





```csharp
// Calculate report date boundaries for filtering
var now = DateTime.UtcNow;
var reportStart = now.StartOfDay();           // 2025-07-19 00:00:00
var reportEnd = now.EndOfDay();             // 2025-07-19 23:59:59

// Calculate weekly report boundaries (Monday to Sunday)
var weekStart = now.StartOfWeek();           // 2025-07-14 00:00:00 (Monday)
var weekEnd = now.EndOfWeek();             // 2025-07-20 23:59:59 (Sunday)

// Calculate monthly report boundaries
var monthStart = now.StartOfMonth();        // 2025-07-01 00:00:00
var monthEnd = now.EndOfMonth();          // 2025-07-31 23:59:59

// Calculate user age from birth date
var birthDate = new DateTime(1990, 5, 15);
int age = birthDate.GetAge();              // 35 (as of 2025)

// Format dates for API responses
var utcDate = DateTime.UtcNow;
string isoDate = utcDate.ToIso8601();     // "2025-07-19T14:30:45Z"

// Display user-friendly timestamps
var eventTime = DateTime.UtcNow.AddHours(-2);
string relativeTime = eventTime.ToRelativeTime(); // "2h ago"

// Check if within business hours for scheduling
if (now.IsBusinessHours())
{
    Console.WriteLine("Processing during business hours");
}

// Check if a date is in the past or future
var pastDate = DateTime.UtcNow.AddDays(-1);
var futureDate = DateTime.UtcNow.AddDays(1);

if (pastDate.IsPast())  Console.WriteLine("This date is in the past");
if (futureDate.IsFuture()) Console.WriteLine("This date is in the future");
```



## License

This project is licensed under the MIT License - see [LICENSE](LICENSE) file for details.




---



**Built by [Vladyslav Zaiets](https://sarmkadan.com) - CTO & Software Architect**



[Portfolio](https://sarmkadan.com) | [GitHub](https://github.com/Sarmkadan) | [Telegram](https://t.me/sarmkadan)
>>>>>>> REPLACE
