using System;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;

using CredentialManager.Views;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CredentialManager;

internal static class Program
{
	[STAThread]
	public static void Main(string[] args)
		=> BuildAvaloniaApp()
			.UseMicrosoftDependencyInjection(out var configuration)
			.StartWithClassicDesktopLifetime(args, configuration.WithMicrosoftDependencyInjection);

	private static AppBuilder BuildAvaloniaApp()
		=> AppBuilder.Configure<App>()
			.UsePlatformDetect()
			.WithInterFont()
			.LogToTrace();

	private static void SetupApp(this IServiceCollection services, AppBuilder appBuilder)
	{
		var dispatcher = Dispatcher.UIThread;
		var app = appBuilder.Instance!;
		services
			.AddSingleton(app)
			.AddSingleton(app.ApplicationLifetime!)
			.AddSingleton(app.PlatformSettings!)
			.AddSingleton(dispatcher);

		var serviceProvider = services.BuildServiceProvider();
		app.SetValue(App.ServiceProviderProperty, serviceProvider);
		_ = new ServiceProviderDisposer(serviceProvider, dispatcher);
		_ = app.ApplicationLifetime switch
		{
			IClassicDesktopStyleApplicationLifetime desktop => desktop.MainWindow = new AppWindow(),
			_ => throw new NotSupportedException(),
		};
	}

	private static AppBuilder UseMicrosoftDependencyInjection(this AppBuilder builder, out ConfigurationManager configuration)
	{
		configuration = new();
		ServiceCollection services = [];
		services.AddSingleton<IConfiguration>(configuration);
		App.Configure(services, configuration);

		builder.AfterSetup(services.SetupApp);
		return builder;
	}

	private static void WithMicrosoftDependencyInjection(this ConfigurationManager configuration, IClassicDesktopStyleApplicationLifetime lifetime)
	{
		configuration.AddCommandLine(lifetime.Args ?? []);
	}

	private class ServiceProviderDisposer
	{
		private readonly ServiceProvider _serviceProvider;
		private ValueTask? _shutdownTask;

		public ServiceProviderDisposer(ServiceProvider serviceProvider, Dispatcher dispatcher)
		{
			dispatcher.ShutdownFinished += OnShutdownFinished;
			dispatcher.ShutdownStarted += OnShutdownStarted;
			_serviceProvider = serviceProvider;
		}

		private void OnShutdownFinished(object? sender, EventArgs e)
		{
			if (_shutdownTask is { IsCompleted: false } disposeTask)
			{
				disposeTask.GetAwaiter().GetResult();
			}
		}

		private void OnShutdownStarted(object? sender, EventArgs e)
		{
#pragma warning disable CA2012 // This will only ever be awaited once in ShutdownFinished
			_shutdownTask = _serviceProvider.DisposeAsync();
#pragma warning restore CA2012
		}
	}
}
