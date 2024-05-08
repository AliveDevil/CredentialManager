using DynamicData;

using Windows.Win32.Security.Credentials;

using static Windows.Win32.PInvoke;

namespace CredentialManager.Models;

internal class CredentialProvider
{
	private readonly IObservableCache<Credential, string> _cache;
	private readonly SourceList<Credential> _credentials = new();
	private readonly object _sync = new();
	private bool _loaded = false;

	public CredentialProvider()
	{
		_cache = _credentials.Connect()
			.AddKey(x => x.Target)
			.AsObservableCache();
	}

	public IObservableCache<Credential, string> GetCredentials()
	{
		bool load;
		lock (_sync)
		{
			load = !_loaded;
			if (load)
			{
				_loaded = true;
			}
		}

		if (load)
		{
			FetchCredentials();
		}

		return _cache;
	}

	private unsafe void FetchCredentials()
	{
		CREDENTIALW** credentials = null;
		try
		{
			if (!CredEnumerate(default, out var count, out credentials))
			{
				return;
			}

			var buffer = new Credential[(int)count];
			for (int i = 0; i < count; i++)
			{
				ref CREDENTIALW cred = ref *credentials[i];
				buffer[i] = new(
					cred.TargetName.ToString(),
					cred.UserName.ToString(),
					cred.Type,
					cred.Flags);
			}

			_credentials.EditDiff(buffer);
		}
		finally
		{
			CredFree(credentials);
		}
	}
}
