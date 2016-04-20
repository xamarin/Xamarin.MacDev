//
// AnalyticsService.cs
//
// Author:
//       Greg Munn <greg.munn@xamarin.com>
//
// Copyright (c) 2014 Xamarin Inc
//

using System;
using System.Collections.Generic;

namespace Xamarin.MacDev
{
	public interface ICustomAnalytics
	{
		void Track (string trackId, Dictionary<string, string> table);
		IDisposable TrackTime (string trackId, Dictionary<string, string> table);
		void IdentifyTrait (string trait, string value);
	}

	/// <summary>
	/// Provides a way to log analytics to Xamarin Insights, or other analytics service
	/// </summary>
	public static class AnalyticsService
	{
		static ICustomAnalytics Analytics;

		public static void SetCustomAnalytics (ICustomAnalytics customAnalytics)
		{
			Analytics = customAnalytics;
		}

		public static void Track (string trackId, Dictionary<string, string> table = null)
		{
			if (Analytics != null)
				Analytics.Track (trackId, table);
		}

		public static void Track (string trackId, string key, string value)
		{
			Track (trackId, new Dictionary<string, string> () { {key, value} });
		}

		public static IDisposable TrackTime (string trackId, Dictionary<string, string> table = null)
		{
			if (Analytics != null)
				return Analytics.TrackTime (trackId, table);

			return NullTimeTracker.Default;
		}

		public static IDisposable TrackTime (string trackId, string key, string value)
		{
			return TrackTime (trackId, new Dictionary<string, string> () { {key, value} });
		}

		public static void IdentifyTrait (string trait, string value)
		{
			if (Analytics != null)
				Analytics.IdentifyTrait (trait, value);
		}

		class NullTimeTracker : IDisposable 
		{
			public static NullTimeTracker Default = new NullTimeTracker ();

			public void Dispose ()
			{
			}
		}
	}
}