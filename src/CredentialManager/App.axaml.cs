using System;
using System.IO;

using Avalonia;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml;

using DynamicData;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

using Splat;
using Splat.Microsoft.Extensions.DependencyInjection;

namespace CredentialManager;

public partial class App : Application
{
	public static readonly StyledProperty<IDataTemplate> AppDataTemplateProperty
		= AvaloniaProperty.Register<App, IDataTemplate>(
			name: nameof(AppDataTemplate),
			coerce: OnAppDataTemplateChanged);
	public static readonly StyledProperty<IServiceProvider> ServiceProviderProperty
		= AvaloniaProperty.Register<App, IServiceProvider>(
			name: nameof(ServiceProvider),
			coerce: OnServiceProviderChanged);

	public IDataTemplate AppDataTemplate => GetValue(AppDataTemplateProperty);

	public IServiceProvider ServiceProvider => GetValue(ServiceProviderProperty);

	public static void Configure(IServiceCollection services, ConfigurationManager configuration)
	{
		configuration.SetBasePath(AppContext.BaseDirectory);
		configuration.AddJsonFile(
			new ManifestEmbeddedFileProvider(typeof(App).Assembly),
			"Properties/appsettings.json", false, false);
		configuration.AddJsonFile(
			Path.Combine(
				Environment.GetFolderPath(
					Environment.SpecialFolder.ApplicationData,
					Environment.SpecialFolderOption.DoNotVerify),
				"CredentialManager",
				"usersettings.json"), true, true);
		configuration.AddJsonFile("appsettings.json", true, true);

		services.UseMicrosoftDependencyResolver();
		Locator.CurrentMutable.InitializeSplat();
	}

	public override void Initialize()
	{
		AvaloniaXamlLoader.Load(this);
	}

	private static IDataTemplate OnAppDataTemplateChanged(AvaloniaObject @object, IDataTemplate dataTemplate)
	{
		var host = (IDataTemplateHost)@object;
		var original = @object.GetValue(AppDataTemplateProperty);

		if (original is null && dataTemplate is not null)
		{
			host.DataTemplates.Add(dataTemplate);
		}
		else if (original is not null)
		{
			if (dataTemplate is null)
			{
				host.DataTemplates.Remove(original);
			}
			else
			{
				host.DataTemplates.ReplaceOrAdd(original, dataTemplate);
			}
		}

		return dataTemplate!;
	}

	private static IServiceProvider OnServiceProviderChanged(AvaloniaObject @object, IServiceProvider provider)
	{
		provider.UseMicrosoftDependencyResolver();
		@object.SetValue(AppDataTemplateProperty, provider.GetRequiredService<IDataTemplate>());
		return provider;
	}
}
