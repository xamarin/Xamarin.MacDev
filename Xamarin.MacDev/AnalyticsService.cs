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
		/// <summary>
		/// Sends an event that contains the given trait and value
		/// </summary>
		void ReportContextProperty(string trait, string value);

		/// <summary>
		/// Sends a single event that contains the given traits and values. The number of traits must match the number of values
		/// </summary>
		void ReportContextProperty(string[] traits, string[] values);
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

		/// <summary>
		/// Sends an event that contains the given trait and value
		/// </summary>
		public static void ReportContextProperty(string trait, string value)
		{
			if (Analytics != null)
			{
				Analytics.ReportContextProperty(trait, value);
			}
		}

		/// <summary>
		/// Sends a single event that contains the given traits and values. The number of traits must match the number of values
		/// </summary>
		public static void ReportContextProperty(string[] traits, string[] values)
		{
			if (Analytics != null)
			{
				Analytics.ReportContextProperty(traits, values);
			}
		}

		[Obsolete("This method does nothing. The telemetry API is changing and for the time being we are only sending the minimum of events that need to be processed server side.")]
		public static void Track (string trackId, Dictionary<string, string> table = null)
		{
		}

		[Obsolete("This method does nothing. The telemetry API is changing and for the time being we are only sending the minimum of events that need to be processed server side.")]
		public static void Track (string trackId, string key, string value)
		{
		}

		[Obsolete("This method does nothing. The telemetry API is changing and for the time being we are only sending the minimum of events that need to be processed server side.")]
		public static IDisposable TrackTime (string trackId, Dictionary<string, string> table = null)
		{
			return NullTimeTracker.Default;
		}

		[Obsolete("This method does nothing. The telemetry API is changing and for the time being we are only sending the minimum of events that need to be processed server side.")]
		public static IDisposable TrackTime (string trackId, string key, string value)
		{
			return TrackTime (trackId, new Dictionary<string, string> () { {key, value} });
		}

		[Obsolete("This method does nothing. The telemetry API is changing and for the time being we are only sending the minimum of events that need to be processed server side.")]
		public static void IdentifyTrait (string trait, string value)
		{
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