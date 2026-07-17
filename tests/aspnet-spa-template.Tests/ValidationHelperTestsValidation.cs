#nullable enable

using System;
using System.Collections.Generic;
using AspNetSpaTemplate.Exceptions;

namespace AspNetSpaTemplate.Tests;

/// <summary>
/// Provides validation helpers for <see cref="ValidationHelperTests"/> instances.
/// Includes methods to validate, check validity, and ensure validity of <see cref="ValidationHelperTests"/> objects.
/// </summary>
public static class ValidationHelperTestsValidation
{
	/// <summary>
	/// Validates an instance of <see cref="ValidationHelperTests"/> and returns a list of validation problems.
	/// </summary>
	/// <param name="value">The instance to validate.</param>
	/// <returns>A list of validation problems; empty if the instance is valid.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
	public static IReadOnlyList<string> Validate(this ValidationHelperTests value)
	{
		ArgumentNullException.ThrowIfNull(value);

		// ValidationHelperTests has no properties to validate
		// This provides the interface as requested by the task
		// All validation is delegated to ValidationHelper static methods

		return Array.Empty<string>();
	}

	/// <summary>
	/// Determines whether an instance of <see cref="ValidationHelperTests"/> is valid.
	/// </summary>
	/// <param name="value">The instance to check.</param>
	/// <returns>True if the instance is valid; otherwise, false.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
	public static bool IsValid(this ValidationHelperTests value)
	{
		ArgumentNullException.ThrowIfNull(value);

		try
		{
			_ = Validate(value);
			return true;
		}
		catch (ValidationException)
		{
			return false;
		}
	}

	/// <summary>
	/// Ensures that an instance of <see cref="ValidationHelperTests"/> is valid,
	/// throwing an <see cref="ArgumentException"/> if not.
	/// </summary>
	/// <param name="value">The instance to validate.</param>
	/// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
	/// <exception cref="ArgumentException">Thrown when the instance is not valid.</exception>
	public static void EnsureValid(this ValidationHelperTests value)
	{
		ArgumentNullException.ThrowIfNull(value);

		var problems = Validate(value);
		if (problems.Count > 0)
		{
			throw new ArgumentException(
			$"ValidationHelperTests instance is not valid. Problems:\n- {string.Join("\n- ", problems)}");
		}
	}
}
