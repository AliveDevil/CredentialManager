using System;

using Avalonia.Platform.Storage;

namespace CredentialManager.Services;

public static class StorageProviderExtensions
{
	public static IStorageProvider? GetStorageProvider(this object? context)
	{
		ArgumentNullException.ThrowIfNull(context);

		return TopLevels.GetTopLevelForContext(context)?.StorageProvider;
	}
}
