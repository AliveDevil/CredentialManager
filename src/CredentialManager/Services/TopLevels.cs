using System;
using System.Collections.Generic;

using Avalonia;
using Avalonia.Controls;

namespace CredentialManager.Services
{
	public class TopLevels
	{
		public static readonly AttachedProperty<object?> RegisterProperty
			= AvaloniaProperty.RegisterAttached<TopLevels, Visual, object?>("Register");
		private static readonly Dictionary<object, Visual> RegistrationMapper = [];

		public static TopLevel? ActiveTopLevel { get; private set; }

		static TopLevels()
		{
			RegisterProperty.Changed.AddClassHandler<Visual>(RegisterChanged);
			WindowBase.IsActiveProperty.Changed.Subscribe(WindowActiveChanged);
		}

		public static object? GetRegister(AvaloniaObject element)
		{
			return element.GetValue(RegisterProperty);
		}

		public static TopLevel? GetTopLevelForContext(object context)
		{
			return TopLevel.GetTopLevel(GetVisualForContext(context));
		}

		public static Visual? GetVisualForContext(object context)
		{
			return RegistrationMapper.TryGetValue(context, out var result) ? result : null;
		}

		public static void SetRegister(AvaloniaObject element, object value)
		{
			element.SetValue(RegisterProperty, value);
		}

		private static void RegisterChanged(Visual sender, AvaloniaPropertyChangedEventArgs e)
		{
			ArgumentNullException.ThrowIfNull(sender);

			// Unregister any old registered context
			if (e.OldValue != null)
			{
				RegistrationMapper.Remove(e.OldValue);
			}

			// Register any new context
			if (e.NewValue != null)
			{
				RegistrationMapper.Add(e.NewValue, sender);
			}
		}

		private static void WindowActiveChanged(AvaloniaPropertyChangedEventArgs<bool> e)
		{
			ActiveTopLevel = (e.GetOldAndNewValue<bool>(), e.Sender) switch
			{
				((false, true), TopLevel topLevel) => topLevel,
				((true, false), { } topLevel) when topLevel == ActiveTopLevel => null,
				_ => ActiveTopLevel,
			};
		}
	}
}
