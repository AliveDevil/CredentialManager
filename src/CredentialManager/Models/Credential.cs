using Windows.Win32.Security.Credentials;

namespace CredentialManager.Models;

internal record struct Credential(
	string Target, 
	string Username, 
	CRED_TYPE Type,
	CRED_FLAGS Flags);
